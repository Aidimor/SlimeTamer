using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;

public class WebGLDebugOverlay : MonoBehaviour
{
    public Text debugText; // Asigna un Text UI desde tu Canvas
    public string languageUrl; // URL del JSON enviado por LoL
    public string languageCode; // Código de idioma, ejemplo "en"

    private bool jsonLoaded = false;
    private int jsonLength = 0;
    private string jsonError = "";

    void Start()
    {
        if (debugText == null)
        {
            Debug.LogWarning("DebugText no asignado. Creando uno automáticamente.");

            GameObject go = new GameObject("DebugTextOverlay");
            go.transform.SetParent(this.transform);

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            GameObject textGO = new GameObject("DebugText");
            textGO.transform.SetParent(go.transform);
            debugText = textGO.AddComponent<Text>();
            debugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            debugText.fontSize = 14;
            debugText.alignment = TextAnchor.UpperLeft;
            debugText.color = Color.white;

            RectTransform rt = debugText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(10, -10);
            rt.sizeDelta = new Vector2(600, 400);
        }

        StartCoroutine(LoadLanguageJSON());
    }

    IEnumerator LoadLanguageJSON()
    {
        if (string.IsNullOrEmpty(languageUrl))
        {
            jsonError = "URL de language.json vacía";
            yield break;
        }

        Debug.Log("🔹 Intentando descargar JSON desde URL: " + languageUrl);
        using (UnityWebRequest request = UnityWebRequest.Get(languageUrl))
        {
            yield return request.SendWebRequest();

            Debug.Log("Request result: " + request.result);
            Debug.Log("Response code: " + request.responseCode);

            if (request.result != UnityWebRequest.Result.Success)
            {
                jsonError = request.error;
                Debug.LogError("❌ Error al descargar JSON: " + jsonError);
                yield break;
            }

            string json = request.downloadHandler.text;
            jsonLength = json.Length;
            jsonLoaded = true;

            var root = JSON.Parse(json);
            if (root[languageCode] == null)
            {
                jsonError = $"Idioma '{languageCode}' no encontrado en JSON";
                Debug.LogError(jsonError);
            }
            else
            {
                Debug.Log($"✅ JSON cargado correctamente. Claves: {root[languageCode].Count}");
            }
        }
    }

    void Update()
    {
        debugText.text = $"WebGL Debug Overlay\n" +
                         $"languageCode: {languageCode}\n" +
                         $"languageUrl: {languageUrl}\n" +
                         $"JSON loaded: {jsonLoaded}\n" +
                         $"JSON length: {jsonLength}\n" +
                         $"Error: {jsonError}\n";
    }
}
