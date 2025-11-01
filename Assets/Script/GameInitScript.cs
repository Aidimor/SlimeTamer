using System.IO;
using LoLSDK;
using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;

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
            LoadState();
        }

#if UNITY_EDITOR
        void LoadMockData()
        {
            Debug.Log("🧩 Cargando datos mock en modo Editor... (Idioma actual: " + _languageCode + ")");

            string startDataFilePath = Path.Combine(Application.streamingAssetsPath, "startGame.json");
            Debug.Log(Application.streamingAssetsPath);

            if (File.Exists(startDataFilePath))
            {
                string startDataAsJSON = File.ReadAllText(startDataFilePath, Encoding.UTF8);
                var payload = JSON.Parse(startDataAsJSON);

                //startDataAsJSON = startDataAsJSON.Replace("´", "’");
       

                // Solo asigna _languageCode si el payload tiene valor
                if (!string.IsNullOrEmpty(payload["languageCode"]))
                    _languageCode = payload["languageCode"];

                // Llamamos a OnStartGame solo si _languageCode tiene valor
                if (!string.IsNullOrEmpty(_languageCode))
                    OnStartGame(payload.ToString());
            }
            else
            {
                if (!string.IsNullOrEmpty(_languageCode))
                    OnStartGame("{\"languageCode\": \"" + _languageCode + "\"}");
                else
                    Debug.Log("ℹ️ No hay languageCode definido en Inspector ni JSON. Se mantiene el idioma actual.");
            }

            // Cargar el archivo de idioma
            string langFilePath = Path.Combine(Application.streamingAssetsPath, "language.json");
            if (File.Exists(langFilePath) && !string.IsNullOrEmpty(_languageCode))
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
            if (MainController.Instance == null)
            {
                Debug.LogWarning("⚠️ MainController no disponible para guardar");
                return;
            }

            GameSaveState state = new GameSaveState
            {
                _worldsUnlocked = MainController.Instance._saveLoadValues._worldsUnlocked,
                _elementsUnlocked = MainController.Instance._saveLoadValues._elementsUnlocked,
                _healthCoins = MainController.Instance._saveLoadValues._healthCoins,
                _hintCoins = MainController.Instance._saveLoadValues._hintCoins,
                _slimeUnlocked = MainController.Instance._saveLoadValues._slimeUnlocked,
                _finalWorldUnlocked = MainController.Instance._saveLoadValues._finalWorldUnlocked
            };

            string json = JsonUtility.ToJson(state, true);
            string path = Path.Combine(Application.persistentDataPath, "gameSave.json");

            if (_usePersistentSave)
            {
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
                    Debug.Log("ℹ️ No existe guardado. Creando archivo nuevo...");

                    InitializeEmptySave();
                    SaveGame();
                }
            }
            else
            {
                LOLSDK.Instance.LoadState<GameSaveState>(state =>
                {
                    if (state != null && state.data != null)
                    {
                        ApplyLoadedState(state.data);
                        Debug.Log("📂 Juego cargado correctamente desde SDK");
                    }
                    else
                    {
                        Debug.Log("ℹ️ No hay guardado en SDK, creando nuevo...");
                        InitializeEmptySave();
                    }
                });
            }
        }

        private void ApplyLoadedState(GameSaveState state)
        {
            if (MainController.Instance == null) return;

            var values = MainController.Instance._saveLoadValues;

            // Actualizar _worldsUnlocked
            if (state._worldsUnlocked != null)
            {
                for (int i = 0; i < values._worldsUnlocked.Length && i < state._worldsUnlocked.Length; i++)
                    values._worldsUnlocked[i] = state._worldsUnlocked[i];
            }

            // Actualizar _elementsUnlocked
            if (state._elementsUnlocked != null)
            {
                for (int i = 0; i < values._elementsUnlocked.Length && i < state._elementsUnlocked.Length; i++)
                    values._elementsUnlocked[i] = state._elementsUnlocked[i];
            }

            // Actualizar _slimeUnlocked
            if (state._slimeUnlocked != null)
            {
                for (int i = 0; i < values._slimeUnlocked.Length && i < state._slimeUnlocked.Length; i++)
                    values._slimeUnlocked[i] = state._slimeUnlocked[i];
            }

            // Actualizar valores simples
            values._healthCoins = state._healthCoins;
            values._hintCoins = state._hintCoins;
            values._finalWorldUnlocked = state._finalWorldUnlocked;
        }

        private void InitializeEmptySave()
        {
            if (MainController.Instance == null) return;

            var values = MainController.Instance._saveLoadValues;

            values._worldsUnlocked[0] = true;
            for (int i = 1; i < values._worldsUnlocked.Length; i++)
                values._worldsUnlocked[i] = false;

            for (int i = 0; i < values._elementsUnlocked.Length; i++)
                values._elementsUnlocked[i] = false;

            for (int i = 0; i < values._slimeUnlocked.Length; i++)
                values._slimeUnlocked[i] = false;

            values._healthCoins = 1;
            values._hintCoins = 1;
            values._finalWorldUnlocked = false;
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

            // Leer archivo con codificación UTF-8
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
            Debug.Log($"✅ Idioma cargado: {lang} ({_translations.Count} textos)");
        }

        void OnStartGame(string startGameJSON)
        {
            if (string.IsNullOrEmpty(startGameJSON)) return;

            var payload = JSON.Parse(startGameJSON);

            // Solo asignar _languageCode si viene con un valor válido
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
                Debug.Log("✅ LanguageDefs recibido.");
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
    }
}
