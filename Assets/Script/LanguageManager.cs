using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON; // Manteniendo el uso de SimpleJSON para compatibilidad con GameInitScript.

namespace LoL
{
    // Asegúrate de que tu JSON siga esta estructura si usas JsonUtility:
    /*
    { "items": [
        { "key": "START_GAME", "value": "Iniciar Juego" },
        { "key": "NEXT_LEVEL", "value": "Siguiente Nivel" }
    ]}
    */

    [System.Serializable]
    public class LocalizationItem
    {
        public string key;
        public string value;
    }

    [System.Serializable]
    public class LocalizationData
    {
        // El nombre de esta variable debe coincidir con la clave principal de tu archivo JSON si usas JsonUtility
        public LocalizationItem[] items;
    }

    public class LanguageManager : MonoBehaviour
    {
        public static LanguageManager Instance;
        public string defaultLanguage = "es";

        private Dictionary<string, string> dictionary = new();
        public bool languageReady = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // No inicies la carga aquí, GameInitScript la inicia al recibir el StartGameJSON.
            }
            else Destroy(gameObject);
        }

        // -----------------------------------------------------
        // 1. Carga Externa (URL de Harness) con Fallback
        // -----------------------------------------------------

        public void LoadLanguageFromExternalURL(string url, string languageCode)
        {
            StartCoroutine(LoadExternalLanguageCoroutine(url, languageCode));
        }

        private IEnumerator LoadExternalLanguageCoroutine(string url, string lang)
        {
            dictionary.Clear();
            languageReady = false;
            string json = null;

            // Carga desde la URL remota (del Harness de LoL)
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    json = request.downloadHandler.text;
                    Debug.Log($"✅ language.json cargado desde URL remota ({url})");
                }
                else
                {
                    Debug.LogError($"❌ Error al cargar language.json desde URL remota ({url}): {request.error}. Intentando fallback local...");
                }
            }

            // Si la carga remota falla (json == null), intentar fallbacks locales
            if (string.IsNullOrEmpty(json))
            {
                // Llamamos a la coroutine de carga local para manejar StreamingAssets/Resources
                yield return StartCoroutine(LoadLocalLanguageCoroutine(lang));
            }
            else
            {
                // Si la carga remota fue exitosa (json != null), parsear y aplicar
                ApplyLanguageJSON(json, lang);

                // --- CORRECCIÓN: Usar CheckReadyState en lugar de OnAllReady ---
                if (GameInitScript.Instance != null)
                {
                    GameInitScript.Instance.CheckReadyState();
                }
                // -------------------------------------------------------------
            }
        }


        // -----------------------------------------------------
        // 2. Carga Local (StreamingAssets/Resources)
        // -----------------------------------------------------

        // Llamado por GameInitScript si no hay URL remota (Editor) o como fallback
        public void LoadLanguage(string lang)
        {
            StartCoroutine(LoadLocalLanguageCoroutine(lang));
        }

        private IEnumerator LoadLocalLanguageCoroutine(string lang)
        {
            dictionary.Clear();
            languageReady = false;
            string json = null;

            // 1️⃣ Intentar cargar desde StreamingAssets (usando UnityWebRequest para WebGL)
            string streamingPath = System.IO.Path.Combine(Application.streamingAssetsPath, "language.json");
            using (UnityWebRequest request = UnityWebRequest.Get(streamingPath))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    json = request.downloadHandler.text;
                    Debug.Log($"✅ language.json cargado desde StreamingAssets ({json.Length} bytes)");
                }
                else
                {
                    Debug.LogWarning($"⚠️ No se pudo leer StreamingAssets: {request.error} -> intentando Resources");
                }
            }

            // 2️⃣ Fallback: Resources
            if (string.IsNullOrEmpty(json))
            {
                TextAsset langFile = Resources.Load<TextAsset>("language");
                if (langFile != null)
                {
                    json = langFile.text;
                    Debug.Log($"✅ language.json cargado desde Resources ({json.Length} bytes)");
                }
                else
                {
                    Debug.LogError("❌ No se encontró language.json ni en StreamingAssets ni en Resources");
                    yield break; // Termina la coroutine sin establecer languageReady
                }
            }

            ApplyLanguageJSON(json, lang);

            // --- CORRECCIÓN: Usar CheckReadyState en lugar de OnAllReady ---
            if (GameInitScript.Instance != null)
            {
                GameInitScript.Instance.CheckReadyState();
            }
            // -------------------------------------------------------------
        }

        // -----------------------------------------------------
        // 3. Aplicación de Datos
        // -----------------------------------------------------

        private void ApplyLanguageJSON(string json, string lang)
        {
            // Usamos JsonUtility aquí, asegurando que el JSON esté estructurado con la clave "items".
            LocalizationData data = JsonUtility.FromJson<LocalizationData>(json);

            if (data != null && data.items != null)
            {
                foreach (var item in data.items)
                {
                    if (!string.IsNullOrEmpty(item.key))
                        dictionary[item.key] = item.value;
                }
            }
            else
            {
                Debug.LogError("❌ Error al parsear JSON de localización. Verifica la estructura '{\"items\": [...]}'");
            }

            languageReady = true;
            Debug.Log($"✅ Idioma '{lang}' cargado correctamente con {dictionary.Count} claves.");
        }

        public string GetText(string key)
        {
            return string.IsNullOrEmpty(key) ? "[NO_KEY]" : dictionary.TryGetValue(key, out string value) ? value : $"[{key}]";
        }
    }
}