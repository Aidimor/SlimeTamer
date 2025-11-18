using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TestWebGLLoad : MonoBehaviour
{
    void Start() { StartCoroutine(TestLoad()); }

    IEnumerator TestLoad()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "language.json");
        using (UnityWebRequest req = UnityWebRequest.Get(path))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log("✅ File loaded! Length: " + req.downloadHandler.text.Length);
            else
                Debug.LogError("❌ Failed: " + req.error);
        }
    }

    //void Update()
    //{
    //    if (LoL.GameInitScript.Instance != null && LoL.GameInitScript.Instance.languageReady)
    //    {
    //        Debug.Log("Sample text for 'hello': " + LoL.GameInitScript.Instance.GetText("element1"));
    //    }
    //}
}
