using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LoLSDK;
using SimpleJSON;

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
        public bool[] _progressSave;
        public int _progress;
    }

    public class GameInitScript : MonoBehaviour
    {
        public static GameInitScript Instance;

        [Header("Language")]
        public string _languageCode;
        private JSONNode _langNode;
        private Dictionary<string, string> _translations = new();
        private Dictionary<string, LocalizedItem> _localizedItems = new();
        public bool languageReady = false;

        [Header("Game Save")]
        public bool _usePersistentSave = false;

        public bool respuestaRecibida = false;
        public bool lastAnswerCorrect;
        public string lastQuestionId;
        public string lastAnswer;

        public bool stateLoaded = false;

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

            // Asegurar que _saveLoadValues exista
            if (MainController.Instance != null && MainController.Instance._saveLoadValues == null)
                MainController.Instance._saveLoadValues = new MainController.SaveLoadValues();
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
            LoadState();
            LoadMockData();

            // Forzar estado inicial si _saveLoadValues está vacío
            if (MainController.Instance._saveLoadValues._worldsUnlocked == null || MainController.Instance._saveLoadValues._worldsUnlocked.Length == 0)
            {
                var emptyState = GetEmptySave();
                var sl = MainController.Instance._saveLoadValues;

                sl._worldsUnlocked = emptyState._worldsUnlocked;
                sl._elementsUnlocked = emptyState._elementsUnlocked;
                sl._slimeUnlocked = emptyState._slimeUnlocked;
                sl._healthCoins = emptyState._healthCoins;
                sl._hintCoins = emptyState._hintCoins;
                sl._finalWorldUnlocked = emptyState._finalWorldUnlocked;
                sl._progressSave = emptyState._progressSave;
                sl._progress = emptyState._progress;

                stateLoaded = true;
                Debug.Log("✅ Estado inicial forzado en Editor");
            }
#else
            if (_usePersistentSave)
                LoadState();
            else
                LoadGameFromSDK();
#endif

            yield return new WaitUntil(() => languageReady && stateLoaded);

            Debug.Log("🔹 Enviando GameIsReady al SDK...");
            LOLSDK.Instance.GameIsReady();
            Debug.Log("✅ GameIsReady enviado correctamente.");
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

            string langFilePath = Path.Combine(Application.streamingAssetsPath, "language.json");
            if (File.Exists(langFilePath))
            {
                var lang = JSON.Parse(File.ReadAllText(langFilePath, Encoding.UTF8))?[_languageCode];
                if (lang != null)
                    OnLanguageDefs(lang.ToString());
            }
        }
#endif

        // ========================
        // GUARDADO
        // ========================
        public void SaveGame()
        {
            if (MainController.Instance == null || MainController.Instance._saveLoadValues == null)
            {
                Debug.LogWarning("⚠️ No se puede guardar: MainController o _saveLoadValues aún no están listos.");
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

            LOLSDK.Instance.SaveState(new State<GameSaveState> { data = state });

            MainController.Instance.SubmitProgressToLoL(MainController.Instance._saveLoadValues._progress);
            Debug.Log("💾 Estado guardado en LoLSDK");
        }

        public void LoadState()
        {
            LOLSDK.Instance.LoadState<GameSaveState>(loadedState =>
            {
                if (loadedState != null && loadedState.data != null)
                {
                    StartCoroutine(ApplyLoadedStateWhenReady(loadedState.data, false));
                    Debug.Log("📂 Estado cargado desde LoLSDK");
                }
                else
                {
                    Debug.Log("ℹ️ No hay guardado, creando uno nuevo...");
                    var emptyState = GetEmptySave();
                    StartCoroutine(ApplyLoadedStateWhenReady(emptyState, true));
                }
            });
        }

        private IEnumerator ApplyLoadedStateWhenReady(GameSaveState state, bool isNewGame)
        {
            var mc = MainController.Instance;

            // Inicializa arrays si son null
            if (mc._saveLoadValues._worldsUnlocked == null)
                mc._saveLoadValues._worldsUnlocked = new bool[state._worldsUnlocked.Length];
            if (mc._saveLoadValues._elementsUnlocked == null)
                mc._saveLoadValues._elementsUnlocked = new bool[state._elementsUnlocked.Length];
            if (mc._saveLoadValues._slimeUnlocked == null)
                mc._saveLoadValues._slimeUnlocked = new bool[state._slimeUnlocked.Length];

            // Copia los valores reales
            ApplyLoadedState(state);

            stateLoaded = true;

            yield break;
        }


        private void ApplyLoadedState(GameSaveState state)
        {
            var mc = MainController.Instance;
            if (mc == null || mc._saveLoadValues == null)
            {
                Debug.LogError("❌ MainController o _saveLoadValues sigue siendo null al aplicar estado.");
                return;
            }

            var values = mc._saveLoadValues;

            state._worldsUnlocked?.CopyTo(values._worldsUnlocked, 0);
            state._elementsUnlocked?.CopyTo(values._elementsUnlocked, 0);
            state._slimeUnlocked?.CopyTo(values._slimeUnlocked, 0);

            values._healthCoins = state._healthCoins;
            values._hintCoins = state._hintCoins;
            values._finalWorldUnlocked = state._finalWorldUnlocked;
            values._progress = state._progress;
            values._progressSave = state._progressSave;

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
            _progress = 0
        };

        // ========================
        // IDIOMA
        // ========================
        // ========================
        // IDIOMA
        // ========================
        public void LoadLanguage(string lang)
        {
            StartCoroutine(LoadLanguageCoroutine(lang));
        }

        private IEnumerator LoadLanguageCoroutine(string lang)
        {
            string json = null;
            string fileName = "language.json";
            string streamingPath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

            // 🔹 Intentar leer desde StreamingAssets (Editor/Standalone/Producción)
            using (UnityWebRequest request = UnityWebRequest.Get(streamingPath))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    json = request.downloadHandler.text;
                    Debug.Log("✅ language.json cargado desde StreamingAssets");
                }
                else
                {
                    Debug.LogWarning("⚠️ No se pudo leer StreamingAssets: " + request.error + " -> Intentando Resources");
                }
            }

            // 🔹 Fallback a Resources para Harness/WebGL
            if (string.IsNullOrEmpty(json))
            {
                TextAsset langFile = Resources.Load<TextAsset>("language"); // Assets/Resources/language.json
                if (langFile != null)
                {
                    json = langFile.text;
                    Debug.Log("✅ language.json cargado desde Resources como fallback");
                }
                else
                {
                    Debug.LogError("❌ No se encontró language.json en StreamingAssets ni en Resources");
                    yield break;
                }
            }

            var root = JSON.Parse(json);
            var langData = root[lang];

            if (langData == null || langData.Count == 0)
            {
                Debug.LogError($"❌ El archivo language.json no contiene el idioma '{lang}' o está vacío");
                yield break;
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
            Debug.Log($"✅ Idioma '{lang}' cargado correctamente con {_translations.Count} claves.");
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
        }

        void OnLanguageDefs(string langJSON)
        {
            if (!string.IsNullOrEmpty(langJSON))
                _langNode = JSON.Parse(langJSON);
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

        // ========================
        // CARGA DESDE SDK
        // ========================
        public void LoadGameFromSDK()
        {
            LOLSDK.Instance.LoadState<GameSaveState>(state =>
            {
                if (state != null && state.data != null)
                {
                    StartCoroutine(ApplyLoadedStateWhenReady(state.data, false));
                    Debug.Log("📂 Juego cargado correctamente desde SDK");
                }
                else
                {
                    Debug.Log("ℹ️ No hay guardado en SDK, creando uno nuevo...");
                    var emptyState = GetEmptySave();
                    StartCoroutine(ApplyLoadedStateWhenReady(emptyState, true));
                }
            });
        }
    }
}
