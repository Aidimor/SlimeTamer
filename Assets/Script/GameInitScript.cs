using System.IO;
using LoLSDK;
using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;

namespace LoL
{
    public class GameInitScript : MonoBehaviour
    {
        public static GameInitScript Instance;

        public string _langCode;
        private JSONNode _langNode;
        private Dictionary<string, string> _translations = new Dictionary<string, string>();

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
            }
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
            LOLSDK.Instance.GameStateChanged += state => Debug.Log("GameState: " + state);
            LOLSDK.Instance.QuestionsReceived += q => Debug.Log("Questions: " + q);
            LOLSDK.Instance.AnswerResultReceived += OnAnswerResult;

            LOLSDK.Instance.GameIsReady();

#if UNITY_EDITOR
            LoadMockData();
#endif
        }

        public void ShowQuestion()
        {
            LOLSDK.Instance.ShowQuestion();
            respuestaRecibida = false;
        }

        public void OnAnswerResult(string resultJSON)
        {
            var result = JSON.Parse(resultJSON);

            lastAnswerCorrect = result["correct"] != null && result["correct"].AsBool;
            lastQuestionId = result["questionId"] != null ? result["questionId"] : "unknown";
            lastAnswer = result["answer"] != null ? result["answer"] : "none";

            respuestaRecibida = true;

            Debug.Log($"Pregunta: {lastQuestionId}, Respuesta: {lastAnswer}, Correcta: {lastAnswerCorrect}");

            ContinueGameplayAfterQuestion();
        }

        //void OnStartGame(string startGameJSON)
        //{
        //    if (string.IsNullOrEmpty(startGameJSON)) return;

        //    var payload = JSON.Parse(startGameJSON);
        //    _langCode = payload["languageCode"];
        //    Debug.Log("✅ StartGame recibido. Idioma: " + _langCode);

        //    if (GameInitScript.Instance != null)
        //        GameInitScript.Instance.LoadLanguage(_langCode);
        //}

        void OnStartGame(string startGameJSON)
        {
            if (string.IsNullOrEmpty(startGameJSON)) return;

            var payload = JSON.Parse(startGameJSON);

#if UNITY_EDITOR
            // En editor fuerza siempre "es"
            //_langCode = "es";
#else
    // En WebGL o en producción usa lo que diga LoL
    _langCode = payload["languageCode"];
#endif

            Debug.Log("✅ StartGame recibido. Idioma: " + _langCode);

            if (GameInitScript.Instance != null)
                GameInitScript.Instance.LoadLanguage(_langCode);
        }


        void OnLanguageDefs(string langJSON)
        {
            if (string.IsNullOrEmpty(langJSON)) return;

            _langNode = JSON.Parse(langJSON);
            Debug.Log("✅ LanguageDefs recibido.");
        }

        void OnSaveResult(bool success)
        {
            Debug.Log(success ? "✅ Guardado exitoso" : "❌ Error al guardar");
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
                var lang = JSON.Parse(langDataAsJson)?[_langCode];
                OnLanguageDefs(lang.ToString());
            }
        }
#endif

        // ✅ Obtener texto traducido desde diccionario
        public string GetText(string key)
        {
            if (_translations.ContainsKey(key))
                return _translations[key];

            return $"[{key}]";
        }

        void ContinueGameplayAfterQuestion()
        {
            Debug.Log("✅ Continuando juego después de la pregunta...");
        }

        // ✅ Cargar archivo de idioma y llenar diccionario
        public bool languageReady = false;

        public void LoadLanguage(string lang)
        {
            string path = Path.Combine(Application.dataPath, "Jsons", $"{lang}.txt");
            Debug.Log("Buscando archivo de idioma en: " + path);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var langData = JSON.Parse(json);

                _translations.Clear();
                var items = langData["items"];
                foreach (var item in items)
                {
                    string key = item.Value["key"];
                    string value = item.Value["value"];
                    if (!_translations.ContainsKey(key))
                        _translations.Add(key, value);
                }

                Debug.Log($"Idioma cargado: {lang}, entradas: {_translations.Count}");
                languageReady = true; // ✅ ahora está listo
            }
            else
            {
                Debug.LogError($"No se encontró archivo de idioma: {path}");
                _translations.Clear();
                languageReady = false;
            }
        }

    }
}
