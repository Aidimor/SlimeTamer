using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LoLSDK;
using SimpleJSON;
using System.Text;
using TMPro;

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

    // CLASE BASE DE GUARDADO (Tus variables de juego)
    [System.Serializable]
    public class GameSaveState
    {
        public int _healthCoins;
        public int _hintCoins;

        // 🔥 VARIABLES DESBLOQUEADAS Y HABILITADAS 🔥
        public int _progress; // Progreso actual (ej. nivel o mundo completado)
        public bool[] _worldsUnlocked;
        public bool[] _elementsUnlocked;
        public bool[] _slimeUnlocked;
        public bool _finalWorldUnlocked;
        public bool[] _progressSave;
    }

    // 🔥 CLASE HELPER REQUERIDA POR LOL
    [System.Serializable]
    public class GameFullState
    {
        public int score;
        public int currentProgress;
        public int maximumProgress;
        public GameSaveState data;
    }
    // --- Fin Estructuras de datos ---


    public class GameInitScript : MonoBehaviour
    {
        public static GameInitScript Instance;

        [Header("Language")]
        public string _languageCode = "en";
        public bool languageReady = false;
        private Dictionary<string, string> _translations = new();
        private Dictionary<string, LocalizedItem> _localizedItems = new();

        [Header("Game Save")]
        private bool _stateLoaded = false;
        public bool stateLoaded { get => _stateLoaded; private set => _stateLoaded = value; }
        // Almacena el estado cargado desde el SDK para persistencia
        public GameFullState LoadedFullState { get; private set; }

        [Tooltip("Referencia al MainController de la escena.")]
        public MainController mainController;

        [Header("Answer Tracking")]
        public bool respuestaRecibida = false;
        public bool lastAnswerCorrect;
        public string lastQuestionId;
        public string lastAnswer;

        private static bool _initialized = false;
        private bool _loadAttempted = false;

        [Tooltip("Permite inyectar JSON de prueba en lugar del guardado del SDK.")]
        public string SaveJson;
        [SerializeField] private TMP_Text _json;


        // **********************************************
        // 🔥 FUNCIÓN START CON LÓGICA DE RE-APLICACIÓN
        // **********************************************
        void Start()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!_initialized)
            {
                _initialized = true;
                StartCoroutine(InitializeSDK());
            }
            // SI YA ESTÁ INICIALIZADO: Re-aplica el estado cargado al nuevo MainController de la escena.
            else if (LoadedFullState != null)
            {
                Debug.Log("ℹ️ GameInitScript: Ya inicializado. Re-aplicando estado guardado al nuevo MainController.");
                StartCoroutine(ApplyLoadedStateWhenReady());
            }
            else
            {
                // Si ya está inicializado pero no había guardado (LoadedFullState es null), 
                // significa que se usó la inicialización por defecto.
                Debug.Log("ℹ️ GameInitScript: Ya inicializado sin datos guardados. Forzando verificación de estado.");
                CheckReadyState();
            }
        }


        public IEnumerator InitializeSDK()
        {
            ILOLSDK sdk = null;

#if UNITY_EDITOR
            sdk = new MockWebGL();
#elif UNITY_WEBGL
            sdk = new WebGL();
#endif

            // 1. Initialize SDK
            LOLSDK.Init(sdk, "com.legends-of-learning.slimer-tamer");

            // 2. Registrar callbacks
            LOLSDK.Instance.StartGameReceived += OnStartGame;
            LOLSDK.Instance.SaveResultReceived += OnSaveResult;
            LOLSDK.Instance.AnswerResultReceived += OnAnswerResult;

            // 3. Decirle al harness que estamos listos
            LOLSDK.Instance.GameIsReady();

            // 4. Iniciar la carga del estado
            Debug.Log("⏳ Forzando espera de 2.0 segundos antes de LoadGameFromSDK()...");
            yield return new WaitForSeconds(2f);

            LoadGameFromSDK();

#if UNITY_EDITOR
            // 5. Mock Data (solo en Editor)
            StartCoroutine(LoadMockData());
#endif
        }

        // -----------------------------------------------------------------
        // Lógica de Sincronización
        // -----------------------------------------------------------------

        public void CheckReadyState()
        {
            if (languageReady && stateLoaded)
            {
                Debug.Log("✅ GameInitScript: Todos los sistemas listos. Iniciando contenido del juego.");

                var mc = MainController.Instance;

                if (mc != null)
                {
                    // ASUMIDO: Este método inicia el juego real.
                    mc.StartGameContent();
                }
                else
                {
                    Debug.LogError("❌ MainController.Instance es NULL. ¡El juego no puede iniciar!");
                }
            }
            else
            {
                Debug.Log($"⏳ Esperando idioma y estado: languageReady={languageReady}, stateLoaded={stateLoaded}, LoadAttempted={_loadAttempted}");
            }
        }

        // -----------------------------------------------------------------
        // MOCK DATA (Solo para UNITY_EDITOR) - Sin cambios
        // -----------------------------------------------------------------
#if UNITY_EDITOR
        private IEnumerator LoadMockData()
        {
            yield return new WaitForSeconds(0.5f);
            string startGameJsonPath = System.IO.Path.Combine(Application.streamingAssetsPath, "startGame.json");
            string startGameJSON = "{}";

            if (System.IO.File.Exists(startGameJsonPath))
            {
                startGameJSON = System.IO.File.ReadAllText(startGameJsonPath, Encoding.UTF8);
                Debug.Log("📥 Mock StartGame cargado desde StreamingAssets/startGame.json");
            }
            else
            {
                var mockJson = new SimpleJSON.JSONObject();
                mockJson["languageCode"] = "en";
                mockJson["languageUrl"] = "";
                startGameJSON = mockJson.ToString();
                Debug.LogWarning("⚠️ No se encontró startGame.json. Usando datos mock mínimos.");
            }

            OnStartGame(startGameJSON);
        }
#endif

        // -----------------------------------------------------------------
        // SDK Callbacks & Language URL Handler - Sin cambios
        // -----------------------------------------------------------------

        void OnStartGame(string startGameJSON)
        {
            if (string.IsNullOrEmpty(startGameJSON))
            {
                Debug.LogError("❌ StartGame JSON vacío");
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
                Debug.Log("🌐 Intentando cargar language.json desde URL del payload");
                StartCoroutine(LoadLanguageFromURL(languageURL));
            }
            else
            {
                Debug.Log("📦 languageUrl vacío. Intentando carga local.");
                StartCoroutine(LoadLanguageCoroutine(_languageCode));
            }
        }

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
                    yield return LoadLanguageCoroutine(_languageCode);
                }
            }
        }

        private IEnumerator LoadLanguageCoroutine(string lang)
        {
            string json = null;
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, "language.json");

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

            if (!languageReady)
            {
                CheckReadyState();
            }
        }

        private void ApplyLanguageJSON(string json)
        {
            var root = JSON.Parse(json);
            var langData = root[_languageCode];

            if (langData == null || langData.Count == 0)
            {
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
                _localizedItems[key] = new LocalizedItem { key = key, value = value, id = -1 };
            }

            languageReady = true;
            Debug.Log($"✅ Idioma '{_languageCode}' cargado con {_translations.Count} claves");

            CheckReadyState();
        }

        // -----------------------------------------------------------------
        // Game Save/Load (FLUJO DE SDK)
        // -----------------------------------------------------------------

        public void LoadGameFromSDK()
        {
            if (_loadAttempted)
            {
   
                return;
            }
            _loadAttempted = true;
            stateLoaded = false;

     

            LOLSDK.Instance.LoadState<GameFullState>(state =>
            {        
                if (_json != null)
                {
                    if (state != null && state.data != null)
                    {
                        try
                        {
                            string fullResponseJson = JsonUtility.ToJson(state.data);
                            _json.text = "SAVED FULL STATE RECEIVED (RAW):\n" + fullResponseJson;
                        }
                        catch (System.Exception e)
                        {
                            _json.text = $"❌ Error al serializar JSON de guardado:\n{e.Message}";
                            Debug.LogError($"❌ Error al serializar JSON de guardado: {e.Message}");
                        }
                    }
                    else
                    {
                        _json.text = "JSON GUARDADO RECIBIDO: NULL / VACÍO (Nueva partida)";
                    }
                }
           
                if (state != null && state.data != null && state.data.data != null)
                {
                    LoadedFullState = state.data;
                    Debug.Log($"✅ Estado de guardado encontrado y cargado. Health: {LoadedFullState.data._healthCoins}, Progress: {LoadedFullState.currentProgress}/{LoadedFullState.maximumProgress}");
                }
                else
                {
                    LoadedFullState = null;
                    Debug.Log("ℹ️ No se encontró estado guardado. Se usará la inicialización de MainController (Nueva Partida).");
                }

                StartCoroutine(ApplyLoadedStateWhenReady());
            });
        }


        // ******************************************************
        // 🔥 CORRUTINA DE APLICACIÓN DEL ESTADO CARGADO (CLAVE)
        // ******************************************************
        public IEnumerator ApplyLoadedStateWhenReady()
        {
            Debug.Log("📂 Esperando inicialización de MainController...");

            // Espera hasta que el MainController de la escena actual esté listo
            while (MainController.Instance == null || MainController.Instance._saveLoadValues == null)
            {
                yield return null;
            }

            var mc = MainController.Instance;

            // Usamos LoadedFullState (que persiste entre escenas) para aplicar los datos.
            if (LoadedFullState != null && LoadedFullState.data != null)
            {
                GameSaveState loadedData = LoadedFullState.data;
                Debug.Log($"DIAGNÓSTICO INIT: Valor cargado del SDK (Persistente): {loadedData._healthCoins}");

                // --- Aplicación del estado ---
                ApplyLoadedState(loadedData, mc);

                ReportProgressToTeacherApp(LoadedFullState.currentProgress, LoadedFullState.maximumProgress);
            }
            else
            {
                // Si LoadedFullState es null (ej. primera partida o no hay datos), usa los valores por defecto.
                Debug.Log("DIAGNÓSTICO INIT: LoadedState es NULL. Usando valores por defecto de MainController.");
                mc.UpdateCurrencyUI();
            }

            stateLoaded = true;
            CheckReadyState();
        }

        private void ApplyLoadedState(GameSaveState state, MainController mc)
        {
            // 1. Aplicar datos lógicos al MainController
            mc._saveLoadValues._healthCoins = state._healthCoins;
            mc._saveLoadValues._hintCoins = state._hintCoins;
            mc._saveLoadValues._progress = state._progress;

            // Asegurar que los arrays de destino existan o re-dimensionarlos si el guardado trae datos.
            var saveValues = mc._saveLoadValues;

            // Worlds Unlocked (y otros arrays)
            if (state._worldsUnlocked != null)
            {
                // Solo re-dimensiona si el array del saveValues es null o tiene un tamaño diferente
                if (saveValues._worldsUnlocked == null || saveValues._worldsUnlocked.Length != state._worldsUnlocked.Length)
                    saveValues._worldsUnlocked = new bool[state._worldsUnlocked.Length];
                System.Array.Copy(state._worldsUnlocked, saveValues._worldsUnlocked, state._worldsUnlocked.Length);
            }

            if (state._elementsUnlocked != null)
            {
                if (saveValues._elementsUnlocked == null || saveValues._elementsUnlocked.Length != state._elementsUnlocked.Length)
                    saveValues._elementsUnlocked = new bool[state._elementsUnlocked.Length];
                System.Array.Copy(state._elementsUnlocked, saveValues._elementsUnlocked, state._elementsUnlocked.Length);
            }

            if (state._slimeUnlocked != null)
            {
                if (saveValues._slimeUnlocked == null || saveValues._slimeUnlocked.Length != state._slimeUnlocked.Length)
                    saveValues._slimeUnlocked = new bool[state._slimeUnlocked.Length];
                System.Array.Copy(state._slimeUnlocked, saveValues._slimeUnlocked, state._slimeUnlocked.Length);
            }

            if (state._progressSave != null)
            {
                if (saveValues._progressSave == null || saveValues._progressSave.Length != state._progressSave.Length)
                    saveValues._progressSave = new bool[state._progressSave.Length];
                System.Array.Copy(state._progressSave, saveValues._progressSave, state._progressSave.Length);
            }

            saveValues._finalWorldUnlocked = state._finalWorldUnlocked;

            // 2. DEBUG DE VERIFICACIÓN FINAL
            Debug.Log($"✅ Aplicando Estado FINAL al MainController. Health: {saveValues._healthCoins}, Hint: {saveValues._hintCoins}, Progress: {saveValues._progress}");

            // 3. ACTUALIZAR UI
            mc.UpdateCurrencyUI();
        }

        // -----------------------------------------------------------------
        // Funciones de Reporte de Progreso y Guardado - Sin cambios
        // -----------------------------------------------------------------

        public void ReportProgressToTeacherApp(int currentProgress, int maxProgress)
        {
            if (maxProgress > 0)
            {
                LOLSDK.Instance.SubmitProgress(currentProgress, maxProgress);
                Debug.Log($"📊 Progreso reportado al maestro: {currentProgress}/{maxProgress}");
            }
            else
            {
                Debug.LogWarning("⚠️ No se pudo reportar progreso: maximumProgress es 0. Asegúrate de definirlo.");
            }
        }

        public void SaveGame()
        {
            if (MainController.Instance == null || MainController.Instance._saveLoadValues == null)
            {
                Debug.LogWarning("⚠️ No se puede guardar: MainController o _saveLoadValues aún no están listos.");
                return;
            }

            var saveValues = MainController.Instance._saveLoadValues;

            // 1. Obtener tus datos de guardado (GameSaveState)
            GameSaveState gameState = new GameSaveState
            {
                _healthCoins = saveValues._healthCoins,
                _hintCoins = saveValues._hintCoins,
                _progress = saveValues._progress,

                _worldsUnlocked = saveValues._worldsUnlocked,
                _elementsUnlocked = saveValues._elementsUnlocked,
                _slimeUnlocked = saveValues._slimeUnlocked,
                _finalWorldUnlocked = saveValues._finalWorldUnlocked,
                _progressSave = saveValues._progressSave
            };

            // 2. Crear la estructura GameFullState con datos de progreso
            int maxProgress = 8;
            int currentProg = saveValues._progress;

            GameFullState fullState = new GameFullState
            {
                score = gameState._healthCoins + gameState._hintCoins,
                currentProgress = currentProg,
                maximumProgress = maxProgress,
                data = gameState
            };

            // 3. Guardar usando la estructura LoLSDK.State<GameFullState>
            LOLSDK.Instance.SaveState(new LoLSDK.State<GameFullState> { data = fullState });

            // 4. Reportar el progreso al maestro
            ReportProgressToTeacherApp(fullState.currentProgress, fullState.maximumProgress);

            Debug.Log($"💾 Estado guardado en LoLSDK. Health: {gameState._healthCoins}, Progress: {fullState.currentProgress}/{fullState.maximumProgress}");
        }

        void OnSaveResult(bool success)
        {
            Debug.Log(success ? "✅ Guardado exitoso" : "❌ Error al guardar");
        }

        // -----------------------------------------------------------------
        // Otros Callbacks y Funciones de Utilidad - Sin cambios
        // -----------------------------------------------------------------

        public void GameIsComplete(string finalStateJson)
        {
            if (LOLSDK.Instance == null)
            {
                Debug.LogError("❌ LOLSDK no está inicializado. No se puede llamar a CompleteGame.");
                return;
            }

            LOLSDK.Instance.CompleteGame();
            Debug.Log("🎉 CompleteGame llamado.");
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