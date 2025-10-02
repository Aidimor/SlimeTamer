using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class LocalizationItem
{
    public string key;
    public string value;
}

[System.Serializable]
public class LocalizationData
{
    public LocalizationItem[] items; // debe llamarse "items"
}

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;
    public string defaultLanguage = "es";

    private Dictionary<string, string> dictionary = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLanguage(defaultLanguage);
        }
        else Destroy(gameObject);
    }

    public void LoadLanguage(string lang)
    {
        string path = Path.Combine(Application.dataPath, "Jsons", $"{lang}.txt"); // o .json
        Debug.Log("Buscando archivo de idioma en: " + path);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            LocalizationData data = JsonUtility.FromJson<LocalizationData>(json);

            dictionary.Clear();
            if (data != null && data.items != null)
            {
                foreach (var item in data.items)
                {
                    if (!string.IsNullOrEmpty(item.key))
                        dictionary[item.key] = item.value;
                }
            }
            Debug.Log($"Idioma cargado: {lang}, entradas: {dictionary.Count}");
        }
        else
        {
            Debug.LogError($"No se encontró archivo de idioma: {path}");
            dictionary.Clear();
        }
    }

    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "[NO_KEY]"; // fallback si no hay key

        if (dictionary.TryGetValue(key, out string value))
            return value;

        return $"[{key}]"; // fallback visible si no encuentra la clave
    }

}
