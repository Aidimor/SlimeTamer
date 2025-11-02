using System.IO;
using LoLSDK;
using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System; // Para Action

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

        public bool _usePersistentSave; // true = guardar en disco, false = guardar en SDK
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

            // Cargar archivo de idioma
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

            // Crear un GameSaveState vacío
            GameSaveState emptyState = new GameSaveState
            {
                _worldsUnlocked = new bool[4],     // ajustar tamaño según tu juego
                _elementsUnlocked = new bool[4],
                _slimeUnlocked = new bool[7],
                _healthCoins = 0,
                _hintCoins = 0,
                _finalWorldUnlocked = false
            };

            // Guardar manualmente en SDK
            LOLSDK.Instance.SaveState(new State<GameSaveState> { data = emptyState });

            // Aplicar inmediatamente en memoria
            ApplyLoadedState(emptyState);

            Debug.Log("✅ Guardado del SDK borrado y reiniciado manualmente.");
        }


        // ========================
        // Guardado
        // ========================
        public void SaveGame()
        {
            if (MainController.Instance == null) return;

            GameSaveState state = new GameSaveState
            {
                _worldsUnlocked = MainController.Instance._saveLoadValues._worldsUnlocked,
                _elementsUnlocked = MainController.Instance._saveLoadValues._elementsUnlocked,
                _slimeUnlocked = MainController.Instance._saveLoadValues._slimeUnlocked,
                _healthCoins = MainController.Instance._saveLoadValues._healthCoins,
                _hintCoins = MainController.Instance._saveLoadValues._hintCoins,
                _finalWorldUnlocked = MainController.Instance._saveLoadValues._finalWorldUnlocked
            };

            if (_usePersistentSave)
            {
                string json = JsonUtility.ToJson(state, true);
                string path = Path.Combine(Application.persistentDataPath, "gameSave.json");
                File.WriteAllText(path, json, Encoding.UTF8);
                Debug.Log("💾 Juego guardado localmente: " + path);
            }
            else
            {
                LOLSDK.Instance.SaveState(new State<GameSaveState> { data = state });
                Debug.Log("💾 Juego guardado en SDK");
            }
        }


        // ========================
        // CARGA
        // ========================
        public void LoadState()
        {
            if (_usePersistentSave)
            {
                string path = Path.Combine(Application.persistentDataPath, "gameSave.json");


                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path, Encoding.UTF8);
                    GameSaveState state = JsonUtility.FromJson<GameSaveState>(json);
                    ApplyLoadedState(state);
                    Debug.Log("📂 Juego cargado desde disco: " + path);
                }
                else
                {
                    Debug.Log("ℹ️ No hay guardado local. Inicializando uno nuevo...");
                    InitializeEmptySave();
                }


                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path, Encoding.UTF8);
                    GameSaveState state = JsonUtility.FromJson<GameSaveState>(json);

                    ApplyLoadedState(state);
                    Debug.Log("📂 Juego cargado desde disco: " + path);
                }
                else
                {
                    Debug.Log("ℹ️ No existe guardado. Creando archivo nuevo...");
                    StartCoroutine(InitializeEmptySaveWithDelay(() =>
                    {
                        var emptyState = new GameSaveState
                        {
                            _worldsUnlocked = MainController.Instance._saveLoadValues._worldsUnlocked,
                            _elementsUnlocked = MainController.Instance._saveLoadValues._elementsUnlocked,
                            _slimeUnlocked = MainController.Instance._saveLoadValues._slimeUnlocked,
                            _healthCoins = MainController.Instance._saveLoadValues._healthCoins,
                            _hintCoins = MainController.Instance._saveLoadValues._hintCoins,
                            _finalWorldUnlocked = MainController.Instance._saveLoadValues._finalWorldUnlocked
                        };
                        LOLSDK.Instance.SaveState(new State<GameSaveState> { data = emptyState });
                        Debug.Log("✅ Nuevo guardado vacío creado y almacenado en SDK.");
                    }));
                }
            }
            else
            {
                // 🔹 Borrar cualquier guardado existente
                LOLSDK.Instance.LoadState<GameSaveState>(state =>
                {
                    //if (state != null && state.data != null)
                    //{
                    //    Debug.Log("🧹 Guardado existente detectado. Será eliminado.");
                    //    state.data = null;
                    //    LOLSDK.Instance.SaveState(state);
                    //}

                    // 🔹 Cargar normalmente
                    LOLSDK.Instance.LoadState<GameSaveState>(newState =>
                    {
                        if (newState != null && newState.data != null)
                        {
                            ApplyLoadedState(newState.data);
                            Debug.Log("📂 Juego cargado correctamente desde SDK");
                        }
                        else
                        {
                            Debug.Log("ℹ️ No hay guardado en SDK, creando nuevo...");
                            StartCoroutine(InitializeEmptySaveWithDelay(() =>
                            {
                                var emptyState = new GameSaveState
                                {
                                    _worldsUnlocked = MainController.Instance._saveLoadValues._worldsUnlocked,
                                    _elementsUnlocked = MainController.Instance._saveLoadValues._elementsUnlocked,
                                    _slimeUnlocked = MainController.Instance._saveLoadValues._slimeUnlocked,
                                    _healthCoins = MainController.Instance._saveLoadValues._healthCoins,
                                    _hintCoins = MainController.Instance._saveLoadValues._hintCoins,
                                    _finalWorldUnlocked = MainController.Instance._saveLoadValues._finalWorldUnlocked
                                };
                                LOLSDK.Instance.SaveState(new State<GameSaveState> { data = emptyState });
                                Debug.Log("✅ Nuevo guardado vacío creado y almacenado en SDK.");
                            }));
                        }
                    });
                });
            }
        }

        // Coroutine con callback para guardar después
        private IEnumerator InitializeEmptySaveWithDelay(Action onComplete = null)
        {
            yield return new WaitUntil(() => MainController.Instance != null);

            InitializeEmptySave();
            Debug.Log("✅ Save vacío inicializado correctamente.");

            onComplete?.Invoke();
        }

        private void InitializeEmptySave()
        {
            if (MainController.Instance == null) return;

            var values = MainController.Instance._saveLoadValues;

            values._worldsUnlocked = new bool[4] { true, false, false, false };
            values._elementsUnlocked = new bool[4];
            values._slimeUnlocked = new bool[7];
            values._healthCoins = 1;
            values._hintCoins = 1;
            values._finalWorldUnlocked = false;

            Debug.Log("✅ Guardado vacío inicializado");
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

            Debug.Log("✅ Estado aplicado correctamente");
        }

        // ========================
        // IDIOMA / LOCALIZACIÓN
        // ========================
        public void LoadLanguage(string lang)
        {
            string path = Path.Combine(Application.dataPath, "Jsons", $"{lang}.txt");
            if (!File.Exists(path))
            {
                Debug.LogError("❌ No se encontró archivo de idioma: " + path);
                return;
            }

            string json = File.ReadAllText(path, Encoding.UTF8);
            var langData = JSON.Parse(json);

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
        }

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
                _finalWorldUnlocked = MainController.Instance._saveLoadValues._finalWorldUnlocked
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

            // Llamada correcta con callback
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
                    // Crear nuevo estado vacío
                    var emptyState = new GameSaveState
                    {
                        _worldsUnlocked = new bool[4] { true, false, false, false },
                        _elementsUnlocked = new bool[4],
                        _slimeUnlocked = new bool[7],
                        _healthCoins = 1,
                        _hintCoins = 1,
                        _finalWorldUnlocked = false
                    };

                    ApplyLoadedState(emptyState);
                    LOLSDK.Instance.SaveState(JsonUtility.ToJson(emptyState));
                    Debug.Log("✅ Nuevo guardado vacío creado y almacenado en SDK.");
                }
            });
        }




    }





}
