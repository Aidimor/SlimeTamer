using UnityEngine;

public class IndestructibleScript : MonoBehaviour
{
    private static IndestructibleScript instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); //  se mantiene entre escenas
        }
        else
        {
            Destroy(gameObject); //  destruye duplicados al volver a la escena
        }
    }
}
