using System.IO;
using LoLSDK;
using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System; // Para Action
using UnityEngine.Networking;

namespace LoL
{
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
        public int _progress;
    }

    public class GameInitScript : MonoBehaviour
    {
        public static GameInitScript Instance;

        public string _languageCode; // idioma actual
        private JSONNode _langNode;

        private Dictionary<string, string> _translations = new();
        private Dictionary<string, LocalizedItem> _localizedItems = new();

        public bool respuestaRecibida = false;
        public bool lastAnswerCorrect;
        public string lastQuestionId;
        public string lastAnswer;
        public bool languageReady = false;

        public bool _usePersistentSave; // true = guardar en disco, false = usar SDK
        public bool _starZero;

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
        }

        void Start()
        {
            ILOLSDK sdk = null;
#if UNITY_EDITOR
            sdk = new LoLSDK.MockWebGL();
#elif UNITY_WEBGL
            sdk = new LoLSDK.WebGL();
#endif
            LOLSDK.Init(sdk, "com.legends-of-learning.unity.sdk.v5.1.my-game");

            LOLSDK.Instance.StartGameReceived += OnStartGame;
            LOLSDK.Instance.LanguageDefsReceived += OnLanguageDefs;
            LOLSDK.Instance.SaveResultReceived += OnSaveResult;
            LOLSDK.Instance.AnswerResultReceived += OnAnswerResult;

            LOLSDK.Instance.GameStateChanged += state =>
            {
                Time.timeScale = state == GameState.Paused ? 0f : 1f;
            };

            LoadState();
            MainController.Instance.FixSaveValues();
            StartCoroutine(StartNumerator());
        }

        private IEnumerator StartNumerator()
        {
            yield return new WaitUntil(() => LOLSDK.Instance != null);

#if UNITY_EDITOR
            LoadMockData();
#endif

            if (_usePersistentSave)
                LoadState();
            else
                LoadGameFromSDK();

            Debug.Log("🔹 Enviando GameIsReady al SDK...");
            LOLSDK.Instance.GameIsReady();
            Debug.Log("✅ GameIsReady enviado correctamente. Test Harness debería responder ahora.");
        }

#if UNITY_EDITOR
        void LoadMockData()
        {
            string startDataFilePath = Path.Combine(Application.streamingAssetsPath, "startGame.json");

            if (File.Exists(startDataFilePath))
            {
                string startDataAsJSON = File.ReadAllText(startDataFilePath, Encoding.UTF8);
                var payload = JSON.Parse(startDataAsJSON);

                if (!string.IsNullOrEmpty(payload["languageCode"]))
                    _languageCode = payload["languageCode"];

                if (!string.IsNullOrEmpty(_languageCode))
                    OnStartGame(payload.ToString());
            }

            // Cargar idioma desde StreamingAssets
            string langFilePath = Path.Combine(Application.streamingAssetsPath, "language.json");
            if (File.Exists(langFilePath))
            {
                var lang = JSON.Parse(File.ReadAllText(langFilePath, Encoding.UTF8))?[_languageCode];
                if (lang != null)
                    OnLanguageDefs(lang.ToString());
            }
        }
#endif

        public void ClearSDKSaveManually()
        {
            if (MainController.Instance == null)
            {
                Debug.LogWarning("⚠️ MainController no está listo para borrar el guardado.");
                return;
            }

            Debug.Log("🧹 Borrando guardado del SDK manualmente...");

            GameSaveState emptyState = new GameSaveState
            {
                _worldsUnlocked = new bool[4],
                _elementsUnlocked = new bool[4],
                _slimeUnlocked = new bool[7],
                _healthCoins = 0,
                _hintCoins = 0,
                _finalWorldUnlocked = false,
                _progress = 0
            };

            LOLSDK.Instance.SaveState(new State<GameSaveState> { data = emptyState });
            ApplyLoadedState(emptyState);

            Debug.Log("✅ Guardado del SDK borrado y reiniciado manualmente.");
        }

        public void SaveGame()
        {
            GameSaveState state = new GameSaveState
            {
                _worldsUnlocked = MainController.Instance._saveLoadValues._worldsUnlocked,
                _elementsUnlocked = MainController.Instance._saveLoadValues._elementsUnlocked,
                _slimeUnlocked = MainController.Instance._saveLoadValues._slimeUnlocked,
                _healthCoins = MainController.Instance._saveLoadValues._healthCoins,
                _hintCoins = MainController.Instance._saveLoadValues._hintCoins,
                _finalWorldUnlocked = MainController.Instance._saveLoadValues._finalWorldUnlocked,
                _progress = MainController.Instance._saveLoadValues._progress
            };

            MainController.Instance.SubmitProgressToLoL(MainController.Instance._saveLoadValues._progress);
            LOLSDK.Instance.SaveState(new State<GameSaveState> { data = state });

            Debug.Log("💾 Estado guardado en LoLSDK");
        }

        public void LoadState()
        {
            LOLSDK.Instance.LoadState<GameSaveState>(loadedState =>
            {
                if (loadedState != null && loadedState.data != null)
                {
                    ApplyLoadedState(loadedState.data);
                    Debug.Log("📂 Estado cargado desde LoLSDK");
                }
                else
                {
                    Debug.Log("ℹ️ No hay guardado, inicializando estado por defecto");
                    var emptyState = new GameSaveState
                    {
                        _worldsUnlocked = new bool[4] { true, false, false, false },
                        _elementsUnlocked = new bool[4],
                        _slimeUnlocked = new bool[7],
                        _healthCoins = 1,
                        _hintCoins = 1,
                        _finalWorldUnlocked = false,
                        _progress = 0
                    };

                    ApplyLoadedState(emptyState);
                    LOLSDK.Instance.SaveState(new State<GameSaveState> { data = emptyState });
                    Debug.Log("✅ Guardado inicial creado en LoLSDK");
                }
            });
        }

        private void ApplyLoadedState(GameSaveState state)
        {
            if (MainController.Instance == null) return;

            var values = MainController.Instance._saveLoadValues;

            if (state._worldsUnlocked != null)
                state._worldsUnlocked.CopyTo(values._worldsUnlocked, 0);
            if (state._elementsUnlocked != null)
                state._elementsUnlocked.CopyTo(values._elementsUnlocked, 0);
            if (state._slimeUnlocked != null)
                state._slimeUnlocked.CopyTo(values._slimeUnlocked, 0);

            values._healthCoins = state._healthCoins;
            values._hintCoins = state._hintCoins;
            values._finalWorldUnlocked = state._finalWorldUnlocked;
            values._progress = state._progress;

            Debug.Log("✅ Estado aplicado correctamente");
        }

        // ========================
        // IDIOMA
        // ========================
        public void LoadLanguage(string lang)
        {
            StartCoroutine(LoadLanguageCoroutine(lang));
        }

        private IEnumerator LoadLanguageCoroutine(string lang)
        {
            string fileName = "language.json"; // 👈 Un solo archivo para todos los idiomas
            string persistentPath = Path.Combine(Application.persistentDataPath, fileName);
            string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);

            string json = null;

            // 1️⃣ Buscar en guardado persistente primero
            if (File.Exists(persistentPath))
            {
                Debug.Log($"📦 Cargando idioma desde guardado persistente: {persistentPath}");
                json = File.ReadAllText(persistentPath, Encoding.UTF8);
            }
            else
            {
#if UNITY_WEBGL
                // 2️⃣ En WebGL, cargar con UnityWebRequest
                Debug.Log("🌐 Cargando idioma desde: " + streamingPath);
                using UnityWebRequest request = UnityWebRequest.Get(streamingPath);
                yield return request.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
                if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
                {
                    Debug.LogError("❌ Error al cargar idioma: " + request.error);
                    yield break;
                }

                json = request.downloadHandler.text;
#else
        // 3️⃣ En Editor o PC: leer directamente desde StreamingAssets
        if (File.Exists(streamingPath))
        {
            Debug.Log("💾 Cargando idioma desde StreamingAssets: " + streamingPath);
            json = File.ReadAllText(streamingPath, Encoding.UTF8);
        }
        else
        {
            Debug.LogError("❌ No se encontró language.json en StreamingAssets ni PersistentDataPath");
            yield break;
        }
#endif
            }

            // 4️⃣ Aplicar idioma
            if (!string.IsNullOrEmpty(json))
            {
                var root = SimpleJSON.JSON.Parse(json);
                var langData = root?[lang]; // 👈 Ahora accedemos directamente al idioma plano

                if (langData != null)
                {
                    _translations.Clear();
                    _localizedItems.Clear();

                    foreach (var kvp in langData)
                    {
                        string key = kvp.Key;
                        string value = kvp.Value;

                        _translations[key] = value;
                        _localizedItems[key] = new LocalizedItem
                        {
                            key = key,
                            value = value,
                            id = -1 // Ya no usamos IDs
                        };
                    }

                    languageReady = true;
                    Debug.Log($"✅ Idioma '{lang}' cargado correctamente desde language.json");
                    Debug.Log($"🔤 Total de claves cargadas: {_translations.Count}");

                    // Depuración opcional
                    foreach (var k in _translations.Keys)
                        Debug.Log($" - {k}: {_translations[k]}");
                }
                else
                {
                    Debug.LogError($"❌ El archivo language.json no contiene el idioma '{lang}'");
                }
            }
        }




        private void ApplyLanguageFromJSON(string json, string lang)
        {
            var langData = SimpleJSON.JSON.Parse(json);
            _translations.Clear();
            _localizedItems.Clear();

            foreach (var item in langData["items"])
            {
                string key = item.Value["key"];
                string value = item.Value["value"];
                int id = item.Value["id"]?.AsInt ?? -1;

                _translations[key] = value;
                _localizedItems[key] = new LocalizedItem { key = key, value = value, id = id };
            }

            languageReady = true;
            Debug.Log($"✅ Idioma '{lang}' cargado correctamente.");
        }

        // ========================
        // CALLBACKS DEL SDK
        // ========================
        void OnStartGame(string startGameJSON)
        {
            if (string.IsNullOrEmpty(startGameJSON)) return;

            var payload = JSON.Parse(startGameJSON);

            if (!string.IsNullOrEmpty(payload["languageCode"]))
            {
                _languageCode = payload["languageCode"];
                Debug.Log("✅ StartGame recibido, idioma: " + _languageCode);
                LoadLanguage(_languageCode);
            }
            else
            {
                Debug.Log("ℹ️ StartGame recibido sin languageCode. Manteniendo idioma actual: " + _languageCode);
            }
        }

        void OnLanguageDefs(string langJSON)
        {
            if (!string.IsNullOrEmpty(langJSON))
            {
                _langNode = JSON.Parse(langJSON);
            }
        }

        void OnSaveResult(bool success)
        {
            Debug.Log(success ? "✅ Guardado exitoso en LoL" : "❌ Error al guardar en LoL");
        }

        public void OnAnswerResult(string resultJSON)
        {
            var result = JSON.Parse(resultJSON);
            lastAnswerCorrect = result["correct"]?.AsBool ?? false;
            lastQuestionId = result["questionId"] ?? "unknown";
            lastAnswer = result["answer"] ?? "none";
            respuestaRecibida = true;

            Debug.Log($"📊 Pregunta: {lastQuestionId}, Respuesta: {lastAnswer}, Correcta: {lastAnswerCorrect}");
        }

        public void ShowQuestion()
        {
            LOLSDK.Instance.ShowQuestion();
            respuestaRecibida = false;
            Debug.Log("❓ Pregunta mostrada al jugador.");
        }

        public string GetText(string key) =>
            _translations.ContainsKey(key) ? _translations[key] : $"[{key}]";

        public int GetTextID(string key) =>
            _localizedItems.ContainsKey(key) ? _localizedItems[key].id : -1;

        public void SaveGameToSDK()
        {
            if (LOLSDK.Instance == null)
            {
                Debug.LogWarning("⚠️ SDK no inicializado.");
                return;
            }

            if (MainController.Instance == null)
            {
                Debug.LogWarning("⚠️ MainController no inicializado.");
                return;
            }

            GameSaveState state = new GameSaveState
            {
                _worldsUnlocked = MainController.Instance._saveLoadValues._worldsUnlocked,
                _elementsUnlocked = MainController.Instance._saveLoadValues._elementsUnlocked,
                _slimeUnlocked = MainController.Instance._saveLoadValues._slimeUnlocked,
                _healthCoins = MainController.Instance._saveLoadValues._healthCoins,
                _hintCoins = MainController.Instance._saveLoadValues._hintCoins,
                _finalWorldUnlocked = MainController.Instance._saveLoadValues._finalWorldUnlocked,
                _progress = MainController.Instance._saveLoadValues._progress
            };

            string json = JsonUtility.ToJson(state);
            LOLSDK.Instance.SaveState(json);

            Debug.Log("💾 Guardado enviado al SDK: " + json);
        }

        public void LoadGameFromSDK()
        {
            if (LOLSDK.Instance == null)
            {
                Debug.LogWarning("⚠️ SDK no inicializado.");
                return;
            }

            LOLSDK.Instance.LoadState<GameSaveState>(state =>
            {
                if (state != null && state.data != null)
                {
                    ApplyLoadedState(state.data);
                    Debug.Log("📂 Juego cargado correctamente desde SDK");
                }
                else
                {
                    Debug.Log("ℹ️ No hay guardado en SDK, creando uno nuevo...");
                    var emptyState = new GameSaveState
                    {
                        _worldsUnlocked = new bool[4] { true, false, false, false },
                        _elementsUnlocked = new bool[4],
                        _slimeUnlocked = new bool[7],
                        _healthCoins = 1,
                        _hintCoins = 1,
                        _finalWorldUnlocked = false,
                        _progress = 0
                    };

                    ApplyLoadedState(emptyState);
                    LOLSDK.Instance.SaveState(JsonUtility.ToJson(emptyState));
                    Debug.Log("✅ Nuevo guardado vacío creado y almacenado en SDK.");
                }
            });
        }
    }
}
