using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LoLSDK;
using SimpleJSON;
using System.Text;

namespace LoL
{
    // --- Estructuras de datos ---
    [System.Serializable]
    public class LocalizedItem
    {
        public string key;
        public string value;
        public int id;
    }

    [System.Serializable]
    public class GameSaveState
    {
        public bool[] _worldsUnlocked;
        public bool[] _elementsUnlocked;
        public int _healthCoins;
        public int _hintCoins;
        public bool[] _slimeUnlocked;
        public bool _finalWorldUnlocked;
        public bool[] _progressSave;
        public int _progress;
    }
    // --- Fin Estructuras de datos ---


    public class GameInitScript : MonoBehaviour
    {
        public static GameInitScript Instance;

        [Header("Language")]
        public string _languageCode = "en"; // Valor por defecto
        public bool languageReady = false;
        private Dictionary<string, string> _translations = new();
        private Dictionary<string, LocalizedItem> _localizedItems = new();

        [Header("Game Save")]
        public bool stateLoaded = false;

        // Asumiendo que MainController tiene un método para iniciar el contenido.
        // Asegúrate de que este script pueda encontrar la instancia de MainController.
        [Tooltip("Referencia al MainController de la escena.")]
        public MainController mainController;

        [Header("Answer Tracking")]
        public bool respuestaRecibida = false;
        public bool lastAnswerCorrect;
        public string lastQuestionId;
        public string lastAnswer;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Application.runInBackground = false;

            // Nota: El MainController debe inicializar sus propios valores.
        }

        void Start()
        {
            ILOLSDK sdk = null;

#if UNITY_EDITOR
            // Usamos el mock en el Editor
            sdk = new LoLSDK.MockWebGL();
#elif UNITY_WEBGL
            // Usamos el SDK real en WebGL
            sdk = new LoLSDK.WebGL();
#endif

            // 1. Init
            LOLSDK.Init(sdk, "com.legends-of-learning.slimer-tamer");

            // 2. Registrar callbacks
            // StartGameReceived es el CRÍTICO, ya que contiene el languageUrl
            LOLSDK.Instance.StartGameReceived += OnStartGame;
            LOLSDK.Instance.SaveResultReceived += OnSaveResult;
            LOLSDK.Instance.AnswerResultReceived += OnAnswerResult;
            // No necesitamos LanguageDefsReceived si usamos la URL de StartGameReceived.

            // 3. Indicar al harness que el juego está preparado.
            // ESTA LLAMADA SOLO SE HACE UNA VEZ.
            Debug.Log("⚡ Calling GameIsReady! (Global Signal)");
            LOLSDK.Instance.GameIsReady();

#if UNITY_EDITOR
            // MOCK DATA ONLY FOR EDITOR
            StartCoroutine(LoadMockData());
#endif
        }


        // -----------------------------------------------------------------
        // Lógica de Sincronización
        // -----------------------------------------------------------------

        /// <summary>
        /// Se llama cuando se completa la carga del idioma o la carga del estado.
        /// Verifica si ambos están listos para iniciar el contenido principal del juego.
        /// </summary>
        public void CheckReadyState()
        {
            if (languageReady && stateLoaded)
            {
                Debug.Log("✅ GameInitScript: Todos los sistemas listos. Iniciando contenido del juego.");

                if (mainController != null)
                {
                    // ASUMIDO: Este método inicia el juego real.
                    // Asegúrate de que MainController tenga un método StartGameContent()
                    mainController.StartGameContent();
                }
                else
                {
                    Debug.LogError("❌ MainController no está asignado en GameInitScript. ¡El juego no puede iniciar!");
                }
            }
            else
            {
                Debug.Log($"⏳ Esperando idioma y estado: languageReady={languageReady}, stateLoaded={stateLoaded}");
            }
        }

        // -----------------------------------------------------------------
        // MOCK DATA (Solo para UNITY_EDITOR)
        // -----------------------------------------------------------------
#if UNITY_EDITOR
        /// <summary>
        /// Simula la recepción de StartGame y LanguageDefs en el Editor.
        /// </summary>
        private IEnumerator LoadMockData()
        {
            yield return new WaitForSeconds(0.5f); // Dar tiempo a que los listeners se registren

            // 1. MOCK START GAME (Datos de inicio)
            string startGameJsonPath = System.IO.Path.Combine(Application.streamingAssetsPath, "startGame.json");
            string startGameJSON = "{}";

            if (System.IO.File.Exists(startGameJsonPath))
            {
                startGameJSON = System.IO.File.ReadAllText(startGameJsonPath, Encoding.UTF8);
                Debug.Log("📥 Mock StartGame cargado desde StreamingAssets/startGame.json");
            }
            else
            {
                // Si no existe el archivo mock, creamos un JSON mínimo
                var mockJson = new SimpleJSON.JSONObject();
                mockJson["languageCode"] = "en";
                mockJson["languageUrl"] = ""; // No hay URL remota en mock, forzamos carga local
                startGameJSON = mockJson.ToString();
                Debug.LogWarning("⚠️ No se encontró startGame.json. Usando datos mock mínimos.");
            }

            // Llama directamente al callback de StartGame (simulando la recepción del SDK)
            OnStartGame(startGameJSON);
        }
#endif

        // -----------------------------------------------------------------
        // SDK Callbacks & Language URL Handler
        // -----------------------------------------------------------------

        void OnStartGame(string startGameJSON)
        {
            if (string.IsNullOrEmpty(startGameJSON))
            {
                Debug.LogError("❌ StartGame JSON vacío");
                // Forzar el estado a cargado para no bloquear
                stateLoaded = true;
                languageReady = false;
                CheckReadyState();
                return;
            }

            Debug.Log("📥 StartGame JSON recibido: " + startGameJSON);

            var payload = JSON.Parse(startGameJSON);

            _languageCode = payload["languageCode"] ?? "en";
            string languageURL = payload["languageUrl"];

            Debug.Log($"🔹 languageCode: {_languageCode}, languageUrl: {languageURL}");

            if (!string.IsNullOrEmpty(languageURL))
            {
                // 🌐 Carga remota
                Debug.Log("🌐 Intentando cargar language.json desde URL del payload");
                StartCoroutine(LoadLanguageFromURL(languageURL));
            }
            else
            {
                // 📦 Carga local (para Editor o fallback)
                Debug.Log("📦 languageUrl vacío. Intentando carga local.");
                StartCoroutine(LoadLanguageCoroutine(_languageCode));
            }

            // ⚡ Cargar estado desde SDK (corre en paralelo al idioma)
            LoadGameFromSDK();
        }


        // 🟢 COROUTINE: Carga remota de idioma (URL)
        private IEnumerator LoadLanguageFromURL(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    Debug.Log("✅ language.json cargado desde URL: " + url);
                    ApplyLanguageJSON(json);
                }
                else
                {
                    Debug.LogError($"❌ No se pudo cargar language.json desde URL: {request.error}. Intentando fallback local.");

                    // 💡 FALLBACK: Si falla la carga remota, intenta la carga local
                    yield return LoadLanguageCoroutine(_languageCode);
                }
            }
        }


        // 📦 COROUTINE: Carga local de idioma (StreamingAssets y Resources)
        private IEnumerator LoadLanguageCoroutine(string lang)
        {
            string json = null;
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, "language.json");

            // Intentar cargar desde StreamingAssets (compatible con Editor y WebGL)
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    json = request.downloadHandler.text;
                    Debug.Log("✅ language.json cargado desde StreamingAssets");
                }
                else
                {
                    Debug.LogWarning("⚠️ No se pudo leer StreamingAssets: " + request.error);

                    // Intentar cargar desde Resources como último recurso
                    TextAsset langFile = Resources.Load<TextAsset>("language");
                    if (langFile != null)
                    {
                        json = langFile.text;
                        Debug.Log("✅ language.json cargado desde Resources como fallback");
                    }
                }
            }

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("❌ No se encontró language.json en ninguna parte.");
                languageReady = false;
            }
            else
            {
                ApplyLanguageJSON(json);
            }

            // Llamar a CheckReadyState incluso si falló la carga (para evitar bloqueo)
            if (!languageReady)
            {
                CheckReadyState();
            }
        }

        private void ApplyLanguageJSON(string json)
        {
            var root = JSON.Parse(json);
            // El JSON del SDK envuelve las traducciones, por lo que buscamos el código de idioma
            var langData = root[_languageCode];

            if (langData == null || langData.Count == 0)
            {
                // Fallback a inglés si el idioma solicitado no existe
                if (_languageCode != "en")
                {
                    Debug.LogWarning($"⚠️ Idioma '{_languageCode}' no encontrado. Intentando fallback a 'en'.");
                    langData = root["en"];
                    if (langData == null)
                    {
                        Debug.LogError("❌ Fallback a 'en' también falló.");
                        languageReady = false;
                        CheckReadyState();
                        return;
                    }
                    _languageCode = "en";
                }
            }

            _translations.Clear();
            _localizedItems.Clear();

            foreach (KeyValuePair<string, JSONNode> pair in langData)
            {
                string key = pair.Key;
                string value = pair.Value;
                _translations[key] = value;
                // Asume que LocalizedItem.id es -1 si no se usa
                _localizedItems[key] = new LocalizedItem { key = key, value = value, id = -1 };
            }

            languageReady = true;
            Debug.Log($"✅ Idioma '{_languageCode}' cargado con {_translations.Count} claves");

            CheckReadyState();
        }

        // -----------------------------------------------------------------
        // Game Save/Load
        // -----------------------------------------------------------------

        public void LoadGameFromSDK()
        {
            // Usar la función de carga tipada del SDK.
            LOLSDK.Instance.LoadState<GameSaveState>(state =>
            {
                GameSaveState loadedState = null;

                if (state != null && state.data != null)
                {
                    loadedState = state.data;
                    Debug.Log("📂 Estado cargado desde SDK.");
                }
                else
                {
                    Debug.Log("ℹ️ No hay estado guardado o está vacío. Creando uno nuevo.");
                    loadedState = GetEmptySave();
                }

                // Asegurar que el MainController exista antes de aplicar
                if (MainController.Instance == null || MainController.Instance._saveLoadValues == null)
                {
                    Debug.LogWarning("⚠️ MainController o _saveLoadValues no están inicializados. Inicializando...");

                    // Solo inicializamos el saveLoadValues aquí si el MainController está presente,
                    // sino, la corrutina lo intentará manejar o fallará si MainController no existe.
                    if (MainController.Instance != null && MainController.Instance._saveLoadValues == null)
                    {
                        // Se asume que MainController.SaveLoadValues es una clase/struct anidada en MainController
                        // y que se puede instanciar así, o que ya existe en MainController.
                        // Usamos un new() para evitar un NRE.
                        MainController.Instance._saveLoadValues = new MainController.SaveLoadValues();
                    }
                }

                // Iniciar la aplicación del estado
                StartCoroutine(ApplyLoadedStateWhenReady(loadedState));
            });
        }

        private IEnumerator ApplyLoadedStateWhenReady(GameSaveState state)
        {
            // Espera un frame para asegurar la inicialización del MainController si es necesario.
            yield return null;

            var mc = MainController.Instance;
            if (mc == null || mc._saveLoadValues == null)
            {
                Debug.LogError("❌ MainController o sus valores son null al aplicar el estado. No se puede continuar.");
                stateLoaded = true; // Forzamos true para no bloquear si no podemos arreglarlo
                CheckReadyState();
                yield break;
            }

            // Inicialización segura de arrays en el estado cargado
            state._worldsUnlocked ??= new bool[4] { true, false, false, false };
            state._elementsUnlocked ??= new bool[4];
            state._slimeUnlocked ??= new bool[7];
            state._progressSave ??= new bool[8];

            // Asegurar que los arrays de destino tengan el tamaño adecuado
            // NOTA: Esto asume que MainController.SaveLoadValues está en MainController y es público
            mc._saveLoadValues._worldsUnlocked ??= new bool[state._worldsUnlocked.Length];
            mc._saveLoadValues._elementsUnlocked ??= new bool[state._elementsUnlocked.Length];
            mc._saveLoadValues._slimeUnlocked ??= new bool[state._slimeUnlocked.Length];
            mc._saveLoadValues._progressSave ??= new bool[state._progressSave.Length];

            ApplyLoadedState(state);
            stateLoaded = true;

            CheckReadyState();
        }

        private void ApplyLoadedState(GameSaveState state)
        {
            var mc = MainController.Instance;
            var values = mc._saveLoadValues;

            // Copiar datos de arrays
            // NOTA: Se asume que los arrays tienen el mismo tamaño en MainController.SaveLoadValues
            state._worldsUnlocked.CopyTo(values._worldsUnlocked, 0);
            state._elementsUnlocked.CopyTo(values._elementsUnlocked, 0);
            state._slimeUnlocked.CopyTo(values._slimeUnlocked, 0);
            state._progressSave.CopyTo(values._progressSave, 0);

            // Copiar datos primitivos (asegurando un valor mínimo de 1 para monedas si son 0/null)
            values._healthCoins = state._healthCoins > 0 ? state._healthCoins : 1;
            values._hintCoins = state._hintCoins > 0 ? state._hintCoins : 1;
            values._finalWorldUnlocked = state._finalWorldUnlocked;
            values._progress = state._progress;

            Debug.Log("✅ Estado aplicado correctamente");
        }

        private GameSaveState GetEmptySave() => new()
        {
            _worldsUnlocked = new bool[4] { true, false, false, false },
            _elementsUnlocked = new bool[4],
            _slimeUnlocked = new bool[7],
            _healthCoins = 1,
            _hintCoins = 1,
            _finalWorldUnlocked = false,
            _progressSave = new bool[8],
            _progress = 0
        };

        // -----------------------------------------------------------------
        // Otros Callbacks y Funciones de Utilidad
        // -----------------------------------------------------------------

        /// <summary>
        /// NOTIFICA AL SDK QUE EL JUEGO O UNA TAREA CRÍTICA HA TERMINADO.
        /// CORRECCIÓN: Se usa CompleteGame() sin argumentos, basado en la definición del SDK proporcionada.
        /// El JSON de estado final se ignora en la llamada al SDK, pero se mantiene en la firma de la función.
        /// </summary>
        /// <param name="finalStateJson">JSON que describe el estado final.</param>
        public void GameIsComplete(string finalStateJson)
        {
            if (LOLSDK.Instance == null)
            {
                Debug.LogError("❌ LOLSDK no está inicializado. No se puede llamar a CompleteGame.");
                return;
            }
            // Llamada corregida sin argumentos
            LOLSDK.Instance.CompleteGame();
            Debug.Log("🎉 CompleteGame llamado (sin JSON). JSON de estado final ignorado: " + finalStateJson);
        }

        public void SaveGame()
        {
            if (MainController.Instance == null || MainController.Instance._saveLoadValues == null)
            {
                Debug.LogWarning("⚠️ No se puede guardar: MainController o _saveLoadValues aún no están listos.");
                return;
            }

            // Mapear los datos de MainController.SaveLoadValues a la clase GameSaveState para el SDK
            GameSaveState state = new GameSaveState
            {
                _worldsUnlocked = MainController.Instance._saveLoadValues._worldsUnlocked,
                _elementsUnlocked = MainController.Instance._saveLoadValues._elementsUnlocked,
                _slimeUnlocked = MainController.Instance._saveLoadValues._slimeUnlocked,
                _healthCoins = MainController.Instance._saveLoadValues._healthCoins,
                _hintCoins = MainController.Instance._saveLoadValues._hintCoins,
                _finalWorldUnlocked = MainController.Instance._saveLoadValues._finalWorldUnlocked,
                _progressSave = MainController.Instance._saveLoadValues._progressSave,
                _progress = MainController.Instance._saveLoadValues._progress
            };

            LOLSDK.Instance.SaveState(new State<GameSaveState> { data = state });
            Debug.Log("💾 Estado guardado en LoLSDK");
        }

        void OnSaveResult(bool success)
        {
            Debug.Log(success ? "✅ Guardado exitoso" : "❌ Error al guardar");
        }

        public void OnAnswerResult(string resultJSON)
        {
            var result = JSON.Parse(resultJSON);
            lastAnswerCorrect = result["correct"]?.AsBool ?? false;
            lastQuestionId = result["questionId"] ?? "unknown";
            lastAnswer = result["answer"] ?? "none";
            respuestaRecibida = true;
        }

        public void ShowQuestion()
        {
            LOLSDK.Instance.ShowQuestion();
            respuestaRecibida = false;
        }

        public string GetText(string key) =>
            _translations.ContainsKey(key) ? _translations[key] : $"[{key}]";

        public int GetTextID(string key) =>
            _localizedItems.ContainsKey(key) ? _localizedItems[key].id : -1;
    }
}