using System.IO;
using LoLSDK;
using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    }

    public class GameInitScript : MonoBehaviour
    {
        public static GameInitScript Instance;

        public string _languageCode;
        private JSONNode _langNode;

        private Dictionary<string, string> _translations = new();
        private Dictionary<string, LocalizedItem> _localizedItems = new();

        public bool respuestaRecibida = false;
        public bool lastAnswerCorrect;
        public string lastQuestionId;
        public string lastAnswer;
        public bool languageReady = false;

        public bool _usePersistentSave; // true = guardar en disco, false = guardar solo en SDK

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);

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

            // Forzamos idioma para pruebas locales
#if UNITY_EDITOR
            //_languageCode = "en"; // o "es" si prefieres
#endif

            StartCoroutine(StartNumerator());
        }

        private IEnumerator StartNumerator()
        {
            yield return new WaitUntil(() => GameInitScript.Instance != null && LOLSDK.Instance != null);

#if UNITY_EDITOR
            // Cargar data local de prueba
            LoadMockData();
#endif

            LoadState();
        }

#if UNITY_EDITOR
#if UNITY_EDITOR
        void LoadMockData()
        {
            // ✅ Si ya se definió un idioma antes, NO lo sobrescribimos
            if (string.IsNullOrEmpty(_languageCode))
                _languageCode = "es"; // Valor por defecto si no se ha establecido

            Debug.Log("🧩 Cargando datos de prueba en modo Editor... (Idioma: " + _languageCode + ")");

            // No modificar el idioma cargado, solo simular evento StartGame
            string startDataFilePath = Path.Combine(Application.streamingAssetsPath, "startGame.json");
            if (File.Exists(startDataFilePath))
            {
                string startDataAsJSON = File.ReadAllText(startDataFilePath);
                var payload = JSON.Parse(startDataAsJSON);
                if (payload["languageCode"] == null)
                    payload["languageCode"] = _languageCode; // asegura que no cambie
                OnStartGame(payload.ToString());
            }
            else
            {
                // Si no hay archivo, solo lanza evento con idioma actual
                OnStartGame("{\"languageCode\": \"" + _languageCode + "\"}");
            }

            // ✅ Si ya usas tus propios JSONs en /Jsons/, NO vuelvas a cargar desde streamingAssets
            // (Solo si no se cargó nada aún)
            if (!_translations.ContainsKey("world"))
            {
                string langFilePath = Path.Combine(Application.streamingAssetsPath, "language.json");
                if (File.Exists(langFilePath))
                {
                    var lang = JSON.Parse(File.ReadAllText(langFilePath))?[_languageCode];
                    if (lang != null)
                        OnLanguageDefs(lang.ToString());
                }
            }

            Debug.Log("✅ Datos mock cargados sin modificar el idioma actual.");
        }
#endif

#endif

        // ========================
        // GUARDADO Y CARGA DE PROGRESO
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
                _elementsUnlocked = MainController.Instance._saveLoadValues._elementsUnlocked
            };

            if (_usePersistentSave)
            {
                // Guardar en disco
                string json = JsonUtility.ToJson(state, true);
                string path = Path.Combine(Application.persistentDataPath, "gameSave.json");
                File.WriteAllText(path, json);
                Debug.Log("💾 Juego guardado en disco: " + path);
            }
            else
            {
                // Guardar solo en SDK
                LOLSDK.Instance.SaveState(new State<GameSaveState> { data = state });
                Debug.Log("💾 Estado guardado solo en SDK");
            }
        }

        public void LoadState()
        {
            if (_usePersistentSave)
            {
                // Cargar desde disco
                string path = Path.Combine(Application.persistentDataPath, "gameSave.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    GameSaveState state = JsonUtility.FromJson<GameSaveState>(json);

                    if (MainController.Instance != null)
                    {
                        MainController.Instance._saveLoadValues._worldsUnlocked = state._worldsUnlocked;
                        MainController.Instance._saveLoadValues._elementsUnlocked = state._elementsUnlocked;
                    }

                    Debug.Log("📂 Juego cargado desde disco: " + path);
                }
                else
                {
                    Debug.Log("ℹ️ No existe archivo de guardado en disco. Se iniciará juego nuevo.");
                }
            }
            else
            {
                // Cargar desde SDK
                LOLSDK.Instance.LoadState<GameSaveState>(state =>
                {
                    if (MainController.Instance == null) return;

                    if (state != null && state.data != null)
                    {
                        var data = state.data;

                        if (data._worldsUnlocked != null && data._worldsUnlocked.Length > 0)
                            MainController.Instance._saveLoadValues._worldsUnlocked = data._worldsUnlocked;

                        if (data._elementsUnlocked != null && data._elementsUnlocked.Length > 0)
                            MainController.Instance._saveLoadValues._elementsUnlocked = data._elementsUnlocked;

                        Debug.Log("📂 Progreso cargado correctamente desde SDK");
                    }
                    else
                    {
                        Debug.Log("ℹ️ No hay estado previo en SDK, manteniendo valores actuales");
                    }
                });
            }
        }

        // ========================
        // FUNCIONES DE IDIOMA
        // ========================
        public void LoadLanguage(string lang)
        {
            string path = Path.Combine(Application.dataPath, "Jsons", $"{lang}.txt");
            Debug.Log("📂 Buscando archivo de idioma en: " + path);

            if (!File.Exists(path))
            {
                Debug.LogError("❌ No se encontró archivo de idioma: " + path);
                _translations.Clear();
                _localizedItems.Clear();
                languageReady = false;
                return;
            }

            string json = File.ReadAllText(path);
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
            Debug.Log($"✅ Idioma cargado: {lang} con {_translations.Count} textos");
        }
        void OnStartGame(string startGameJSON)
        {
            if (string.IsNullOrEmpty(startGameJSON)) return;

            var payload = JSON.Parse(startGameJSON);

            // 🔸 Solo cambiar idioma si no está ya definido
            if (string.IsNullOrEmpty(_languageCode))
            {
                _languageCode = payload["languageCode"] ?? "en";
            }

            Debug.Log("✅ StartGame recibido. Idioma final: " + _languageCode);

            // Cargar el idioma actual (ya existente o nuevo)
            LoadLanguage(_languageCode);
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

        public string GetText(string key) => _translations.ContainsKey(key) ? _translations[key] : $"[{key}]";
        public int GetTextID(string key) => _localizedItems.ContainsKey(key) ? _localizedItems[key].id : -1;
    }
}
