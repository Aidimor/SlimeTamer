using System.IO;
using LoLSDK;
using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;

namespace LoL
{
    [System.Serializable]
    public class LocalizedItem
    {
        public string key;
        public string value;
        public int id;
    }

    public class GameInitScript : MonoBehaviour
    {
        public static GameInitScript Instance;

        public string _languageCode;
        private JSONNode _langNode;

        // Diccionario solo con valores para compatibilidad
        private Dictionary<string, string> _translations = new Dictionary<string, string>();

        // Diccionario con objetos completos para ID
        private Dictionary<string, LocalizedItem> _localizedItems = new Dictionary<string, LocalizedItem>();

        // Estado de preguntas
        public bool respuestaRecibida = false;
        public bool lastAnswerCorrect;
        public string lastQuestionId;
        public string lastAnswer;

        public bool languageReady = false;

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
            }

            Application.runInBackground = false;
        }

        void Start()
        {
#if UNITY_EDITOR
            ILOLSDK sdk = new LoLSDK.MockWebGL();
#elif UNITY_WEBGL
            ILOLSDK sdk = new LoLSDK.WebGL();
#elif UNITY_IOS || UNITY_ANDROID
            ILOLSDK sdk = null;
#endif

            LOLSDK.Init(sdk, "com.legends-of-learning.unity.sdk.v5.1.my-game");

            LOLSDK.Instance.StartGameReceived += OnStartGame;
            LOLSDK.Instance.LanguageDefsReceived += OnLanguageDefs;
            LOLSDK.Instance.SaveResultReceived += OnSaveResult;
            LOLSDK.Instance.QuestionsReceived += q => Debug.Log("📥 Questions: " + q);
            LOLSDK.Instance.AnswerResultReceived += OnAnswerResult;

            LOLSDK.Instance.GameStateChanged += state =>
            {
                Debug.Log("📢 GameState cambiado a: " + state);
                if (state == GameState.Paused) Time.timeScale = 0f;
                else if (state == GameState.Resumed) Time.timeScale = 1f;
            };

            LOLSDK.Instance.GameIsReady();

#if UNITY_EDITOR
            LoadMockData();
#endif

            LoadState();
        }

        public void ShowQuestion()
        {
            LOLSDK.Instance.ShowQuestion();
            respuestaRecibida = false;
            Debug.Log("❓ Pregunta mostrada al jugador.");
        }

        public void OnAnswerResult(string resultJSON)
        {
            var result = JSON.Parse(resultJSON);

            lastAnswerCorrect = result["correct"] != null && result["correct"].AsBool;
            lastQuestionId = result["questionId"] != null ? result["questionId"] : "unknown";
            lastAnswer = result["answer"] != null ? result["answer"] : "none";

            respuestaRecibida = true;
            Debug.Log($"📊 Pregunta: {lastQuestionId}, Respuesta: {lastAnswer}, Correcta: {lastAnswerCorrect}");

            ContinueGameplayAfterQuestion();
        }

        void OnStartGame(string startGameJSON)
        {
            if (string.IsNullOrEmpty(startGameJSON)) return;

            var payload = JSON.Parse(startGameJSON);

#if UNITY_EDITOR
            //_languageCode = "es"; // Fuerza español en editor
#else
            _languageCode = payload["languageCode"];
#endif

            Debug.Log("✅ StartGame recibido. Idioma: " + _languageCode);

            LoadLanguage(_languageCode);
        }

        void OnLanguageDefs(string langJSON)
        {
            if (string.IsNullOrEmpty(langJSON)) return;

            _langNode = JSON.Parse(langJSON);
            Debug.Log("✅ LanguageDefs recibido.");
        }

        void OnSaveResult(bool success)
        {
            Debug.Log(success ? "✅ Guardado exitoso en LoL" : "❌ Error al guardar en LoL");
        }

#if UNITY_EDITOR
        void LoadMockData()
        {
            string startDataFilePath = Path.Combine(Application.streamingAssetsPath, "startGame.json");
            if (File.Exists(startDataFilePath))
            {
                string startDataAsJSON = File.ReadAllText(startDataFilePath);
                OnStartGame(startDataAsJSON);
            }

            string langFilePath = Path.Combine(Application.streamingAssetsPath, "language.json");
            if (File.Exists(langFilePath))
            {
                string langDataAsJson = File.ReadAllText(langFilePath);
                var lang = JSON.Parse(langDataAsJson)?[_languageCode];
                OnLanguageDefs(lang.ToString());
            }
        }
#endif

        // 🔹 Obtener texto traducido
        public string GetText(string key)
        {
            if (_translations.ContainsKey(key))
                return _translations[key];

            return $"[{key}]";
        }

        // 🔹 Obtener ID del texto
        public int GetTextID(string key)
        {
            if (_localizedItems.ContainsKey(key))
                return _localizedItems[key].id;

            return -1;
        }

        void ContinueGameplayAfterQuestion()
        {
            Debug.Log("▶️ Continuando juego después de responder la pregunta...");
        }

        // 🔹 Cargar archivo de idioma
        public void LoadLanguage(string lang)
        {
            string path = Path.Combine(Application.dataPath, "Jsons", $"{lang}.txt");
            Debug.Log("📂 Buscando archivo de idioma en: " + path);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var langData = JSON.Parse(json);

                _translations.Clear();
                _localizedItems.Clear();

                var items = langData["items"];
                foreach (var item in items)
                {
                    string key = item.Value["key"];
                    string value = item.Value["value"];
                    int id = item.Value["id"] != null ? item.Value["id"].AsInt : -1;

                    if (!_translations.ContainsKey(key))
                        _translations.Add(key, value);

                    if (!_localizedItems.ContainsKey(key))
                        _localizedItems.Add(key, new LocalizedItem { key = key, value = value, id = id });
                }

                Debug.Log($"✅ Idioma cargado: {lang}, entradas: {_translations.Count}");
                languageReady = true;
            }
            else
            {
                Debug.LogError($"❌ No se encontró archivo de idioma: {path}");
                _translations.Clear();
                _localizedItems.Clear();
                languageReady = false;
            }
        }

        // 🔹 Guardar estado
        public void SaveGame(string dataJson)
        {
            var state = new State<string> { data = dataJson };
            LOLSDK.Instance.SaveState(state);
            Debug.Log("💾 Estado guardado: " + dataJson);
        }

        // 🔹 Cargar estado
        public void LoadState()
        {
            LOLSDK.Instance.LoadState<string>(state =>
            {
                if (state != null && !string.IsNullOrEmpty(state.data))
                {
                    Debug.Log("📂 Estado cargado: " + state.data);
                }
                else
                {
                    Debug.Log("ℹ️ No se encontró estado previo, se iniciará un nuevo juego");
                }
            });
        }
    }
}
