using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Required for scene management
using UnityEngine.UI;
using TMPro;
using LoL;  // <- necesario para GameInitScript

public class MainController : MonoBehaviour
{
    public static MainController Instance; // Para acceso global desde GameInitScript u otros scripts

    public PortraitController _scriptPortrait;
    public GameInitScript _scriptInit;
    public Animator _bordersAnimator;
    public Animator _cinematicBorders;
    public AudioSource _bgmAS;
    public AudioClip[] _allBGM;

    [System.Serializable]
    public class SaveLoadValues
    {
        public int _wordsUnlocked;
        public bool[] _elementsUnlocked;
    }
    public SaveLoadValues _saveLoadValues;

    [System.Serializable]
    public class PauseAssets
    {
        public GameObject _parent;
        public GameObject[] _options;
        public Image _pointer;
        public int _onPos;
        public bool _pause;
    }
    public PauseAssets _pauseAssets;

    public int _onWorldGlobal;
    public bool _introSpecial;

    private void Awake()
    {
        // Singleton para fácil acceso
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ============================
    // Escenas
    // ============================
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ============================
    // PAUSA
    // ============================
    public void SetPause()
    {
        Debug.Log("pausa");
        // Si NO está en pausa -> pausar
        if (!_pauseAssets._pause)
        {
            // Pausar gameplay
            Time.timeScale = 0f;

            // Mostrar UI de pausa
            if (_pauseAssets._parent != null)
            {
                //_pauseAssets._parent.SetActive(true);

                // Asegurar que el Animator funcione sin escalado de tiempo
                Animator pauseAnimator = _pauseAssets._parent.GetComponent<Animator>();
                if (pauseAnimator != null)
                    pauseAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                pauseAnimator.SetBool("PauseIn", true);
            }

            _pauseAssets._pause = true; // marcamos que ahora está en pausa
            Debug.Log("⏸️ Juego pausado (solo la UI sigue funcionando)");
        }
        else // Si YA está en pausa -> reanudar
        {
            // Reanudar gameplay
            Time.timeScale = 1f;

            // Ocultar UI de pausa
            Animator pauseAnimator = _pauseAssets._parent.GetComponent<Animator>();
            if (_pauseAssets._parent != null)
                pauseAnimator.SetBool("PauseIn", false);
            //_pauseAssets._parent.SetActive(false);

            _pauseAssets._pause = false; // marcamos que ya no está en pausa
            Debug.Log("▶️ Juego reanudado");
        }
    }

}
