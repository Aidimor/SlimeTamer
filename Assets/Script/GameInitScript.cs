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
        // 🔥 VARIABLES DESBLOQUEADAS Y HABILITADAS 🔥
        public int _progress; // Progreso actual (ej. nivel o mundo completado)
        public bool[] _worldsUnlocked;
        public bool[] _progressSave;

        public int _totalSteps;
        public int _totalAtoms;
        public bool _pauseAvailable;
        public bool _restartTutorial;
        public bool _elementTutorial;
        public bool _restartAvailable;
        public bool _hazardTutorial;
        public bool _atomTutorial;
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

        [Header("Debug / Reset")]
        public bool forceNewGame = true;

        private bool _pendingForcedReset = false;


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

        private GameSaveState CreateDefaultGameState()
        {
            return new GameSaveState
            {      
                _progress = 0,
                _totalSteps = 0,
                _totalAtoms = 0,
                _pauseAvailable = false,
                _atomTutorial = false,
                _elementTutorial = false,
                _hazardTutorial = false,
                _restartTutorial = false,      
              
                _worldsUnlocked = new bool[] { true, false, false, false, false },
      
                _progressSave = new bool[] {false, false, false, false,false, false, false, false}
            };
        }


        public IEnumerator InitializeSDK()
        {
            ILOLSDK sdk = null;
#if UNITY_EDITOR
            sdk = new MockWebGL();
#elif UNITY_WEBGL
    sdk = new WebGL();
#endif

            try
            {
                LOLSDK.Init(sdk, "com.legends-of-learning.slimer-tamer");

                LOLSDK.Instance.StartGameReceived += this.OnStartGame;
                LOLSDK.Instance.SaveResultReceived += this.OnSaveResult;
                LOLSDK.Instance.AnswerResultReceived += this.OnAnswerResult;
                LOLSDK.Instance.LanguageDefsReceived += this.HandleLanguageDefs;
                LOLSDK.Instance.GameStateChanged += new GameStateChangedHandler(this.HandleGameStateChange);
                LOLSDK.Instance.QuestionsReceived += new QuestionListReceivedHandler(this.HandleQuestions);

                // Primero: informar que estamos listos
                LOLSDK.Instance.GameIsReady();
      

                // Ahora pedir al SDK que entregue el estado guardado (firma: recibe GameSaveState directamente)
                // Esto sigue el patrón del ejemplo Cooking: LoadState<T>(Action<T>)
                // Reemplaza la llamada ambigua por una lambda con cast explícito:
                LOLSDK.Instance.LoadState<GameSaveState>(state => OnLoadStateWrapper((LoLSDK.State<GameSaveState>)state));

            }
            catch (System.Exception ex)
            {
                Debug.LogError("❌ Excepción durante InitializeSDK: " + ex.ToString());
            }

            // delay opcional
            yield return new WaitForSeconds(0.5f);

#if UNITY_EDITOR
            StartCoroutine(LoadMockData());
#endif
        }


        void HandleQuestions(MultipleChoiceQuestionList questionList)
        {
            Debug.Log("HandleQuestions");
            SharedState.QuestionList = questionList;
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

        // Handle pause / resume
        void HandleGameStateChange(GameState gameState)
        {
            // Either GameState.Paused or GameState.Resumed
            Debug.Log("HandleGameStateChange");
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
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("❌ Language JSON is null or empty");
                languageReady = false;
                CheckReadyState();
                return;
            }

            json = json.Trim();

            // Protección clave para WebGL / LoL
            if (!json.StartsWith("{"))
            {
                Debug.LogError("❌ Invalid JSON received:\n" + json);
                languageReady = false;
                CheckReadyState();
                return;
            }

            JSONNode root;
            try
            {
                root = JSON.Parse(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ JSON Parse failed:\n" + e.Message + "\n" + json);
                languageReady = false;
                CheckReadyState();
                return;
            }

            var langData = root[_languageCode];

            if (langData == null || langData.Count == 0)
            {
                if (_languageCode != "en")
                {
                    Debug.LogWarning($"⚠️ Idioma '{_languageCode}' no encontrado. Intentando fallback a 'en'.");
                    langData = root["en"];

                    if (langData == null || langData.Count == 0)
                    {
                        Debug.LogError("❌ Fallback a 'en' también falló.");
                        languageReady = false;
                        CheckReadyState();
                        return;
                    }

                    _languageCode = "en";
                }
                else
                {
                    Debug.LogError("❌ Idioma 'en' no existe en el JSON.");
                    languageReady = false;
                    CheckReadyState();
                    return;
                }
            }

            _translations.Clear();
            _localizedItems.Clear();

            foreach (KeyValuePair<string, JSONNode> pair in langData)
            {
                string key = pair.Key;
                string value = pair.Value.Value; // 👈 importante

                _translations[key] = value;
                _localizedItems[key] = new LocalizedItem
                {
                    key = key,
                    value = value,
                    id = -1
                };
            }

            languageReady = true;
            Debug.Log($"✅ Idioma '{_languageCode}' cargado con {_translations.Count} claves");

            CheckReadyState();
        }


        void HandleLanguageDefs(string json)
        {
            JSONNode langDefs = JSON.Parse(json);

            // Example of accessing language strings
            // Debug.Log(langDefs);
            // Debug.Log(langDefs["welcome"]);

            SharedState.LanguageDefs = langDefs;
        }


        // Alternativa: método que la SDK puede invocar directamente si prefieres separar lógica
        // --- Método con la firma que espera la SDK: recibe LoLSDK.State<GameSaveState>
        private void OnLoadStateWrapper(LoLSDK.State<GameSaveState> state)
        {
            try
            {
                GameSaveState loaded = null;

                if (state != null)
                {
                    // El contenedor State<T> normalmente tiene la propiedad `.data`
                    loaded = state.data;
                }

                // Reusa tu método existente que toma GameSaveState
                OnLoadGameSave(loaded);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("OnLoadStateWrapper: excepción al procesar LoadState callback: " + ex);
                // Aun así llamamos con null para que la lógica de fallback corra
                OnLoadGameSave(null);
            }
        }

        // --- Tu método ya existente que aplica el GameSaveState al LoadedFullState
        //private void OnLoadGameSave(GameSaveState loadedSave)
        //{
        //    Debug.Log($"OnLoadGameSave invoked. loadedSave == null? {loadedSave == null}");

        //    if (loadedSave != null)
        //    {
        //        LoadedFullState = new GameFullState
        //        {
        //            data = loadedSave,
        //            currentProgress = loadedSave._progress,
        //            maximumProgress = Mathf.Max(loadedSave._progress, 8)
        //        };

        //        Debug.Log("OnLoadGameSave: datos cargados desde SDK: " + JsonUtility.ToJson(loadedSave));

        //    }
        //    else
        //    {
        //        LoadedFullState = null;
        //        Debug.Log("OnLoadGameSave: no había datos (nueva partida).");
        //    }

        //    StartCoroutine(ApplyLoadedStateWhenReady());
        //}

        private void OnLoadGameSave(GameSaveState loadedSave)
        {
            if (forceNewGame)
            {
                Debug.Log("🧨 FORZANDO NUEVA PARTIDA (ignorando save del SDK)");
                loadedSave = CreateDefaultGameState();
                _pendingForcedReset = true; // 👈 se guarda después
            }
            else if (loadedSave == null)
            {
                Debug.Log("🆕 Nueva partida (no había save)");
                loadedSave = CreateDefaultGameState();
                _pendingForcedReset = true;
            }

            LoadedFullState = new GameFullState
            {
                data = loadedSave,
                currentProgress = loadedSave._progress,
                maximumProgress = Mathf.Max(loadedSave._progress, 8)
            };

            StartCoroutine(ApplyLoadedStateWhenReady());
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

            try
            {
                if (LoadedFullState != null && LoadedFullState.data != null)
                {
                    GameSaveState loadedData = LoadedFullState.data;
                    //Debug.Log($"DIAGNÓSTICO INIT: Valor cargado del SDK (Persistente): {loadedData._healthCoins}");

                    // Aplicación del estado (protegida)
                    try
                    {
                        ApplyLoadedState(loadedData, mc);
                    }
                    catch (System.Exception exApply)
                    {
                        Debug.LogError("ApplyLoadedState: excepción al aplicar estado cargado: " + exApply);
                    }

                    // Reportar progreso (si corresponde)
                    try
                    {
                        ReportProgressToTeacherApp(LoadedFullState.currentProgress, LoadedFullState.maximumProgress);
                    }
                    catch (System.Exception exReport)
                    {
                        Debug.LogWarning("ReportProgressToTeacherApp: excepción al reportar progreso: " + exReport);
                    }
                }
                else
                {
                    // Fallback: no hay guardado -> usar valores por defecto del MainController
                    Debug.Log("DIAGNÓSTICO INIT: LoadedFullState es NULL o no contiene data. Usando valores por defecto de MainController.");
                    //mc.UpdateCurrencyUI();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("ApplyLoadedStateWhenReady: excepción inesperada: " + ex);
                //// Asegurarse que la UI se actualice aunque algo haya fallado
                //mc.UpdateCurrencyUI();
            }
            mc.UpdateCurrencyUI();
            stateLoaded = true;
            CheckReadyState();
            // ⬇️ GUARDA SOLO CUANDO TODO YA ESTÁ LISTO (HARNESS SAFE)
            if (_pendingForcedReset)
            {
                Debug.Log("💾 Guardando estado NUEVO después de StartGame");
                LOLSDK.Instance.SaveState(LoadedFullState.data);
                _pendingForcedReset = false;
            }

        }


        private void ApplyLoadedState(GameSaveState state, MainController mc)
        {
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
            if (state._progressSave != null)
            {
                // Solo re-dimensiona si el array del saveValues es null o tiene un tamaño diferente
                if (saveValues._progressSave == null || saveValues._progressSave.Length != state._progressSave.Length)
                    saveValues._progressSave = new bool[state._progressSave.Length];
                System.Array.Copy(state._progressSave, saveValues._progressSave, state._progressSave.Length);
            }


            saveValues._totalAtoms = state._totalAtoms;
            saveValues._totalSteps = state._totalSteps;
            saveValues._pauseAvailable = state._pauseAvailable;
            saveValues._restartTutorial = state._restartTutorial;
            saveValues._hazardTutorial = state._hazardTutorial;
            saveValues._elementTutorial = state._elementTutorial;
            saveValues._restartAvailable = state._restartAvailable;
            saveValues._atomTutorial = state._atomTutorial;

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

            GameSaveState gameState = new GameSaveState
            {
                _progress = saveValues._progress,
                _totalAtoms = saveValues._totalAtoms,
                _pauseAvailable = saveValues._pauseAvailable,
                _atomTutorial = saveValues._atomTutorial,
                _elementTutorial = saveValues._elementTutorial,
                _hazardTutorial = saveValues._hazardTutorial,
                _restartAvailable = saveValues._restartAvailable,
                _restartTutorial = saveValues._restartTutorial,
                _totalSteps = saveValues._totalSteps,
                _worldsUnlocked = saveValues._worldsUnlocked,
                _progressSave = saveValues._progressSave
                

        };

            //for(int i = 0; i < pro)

            try
            {
                // Guarda el objeto "raw" directamente (igual que el ejemplo Cooking)
                LOLSDK.Instance.SaveState(gameState);
                //Debug.Log($"💾 Estado RAW guardado en LoLSDK. Health: {gameState._healthCoins}, Progress: {gameState._progress}");
            }
            catch (System.Exception e)
            {
                Debug.LogError("SaveGame: Exception when calling SaveState: " + e);
            }

            //// Reportar el progreso por separado (si lo necesitas)
            //ReportProgressToTeacherApp(gameState._progress, /*maxProgress*/ 8);
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
            StartCoroutine(MainGameplayScript.Instance.ExitNumerator());
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