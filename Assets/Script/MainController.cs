using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using LoL;
using SimpleJSON;

public class MainController : MonoBehaviour
{
    public static MainController Instance;

    public PortraitController _scriptPortrait;
    public GameInitScript _scriptInit;
    public SFXscript _scriptSFX;
    public MusicController _scriptMusic;

    public Animator _bordersAnimator;
    public Animator _cinematicBorders;
    public AudioSource _bgmAS;
    public AudioClip[] _allBGM;

    [System.Serializable]
    public class SaveLoadValues
    {
        public bool[] _worldsUnlocked;
        public bool[] _elementsUnlocked;
        public int _healthCoins;
        public int _hintCoins;
        public bool[] _slimeUnlocked;
    }
    public SaveLoadValues _saveLoadValues;

    [System.Serializable]
    public class PauseAssets
    {
        public GameObject _parent;
        public GameObject[] _options;
        public TextMeshProUGUI[] _optionsText;
        public Image _pointer;
        public int _onPos;
        public bool _pause;
        public bool _moved;
        public bool _hintAvailable;
        public bool _hintBought;
        public TextMeshProUGUI _hintText;
        public TextMeshProUGUI[] _allSlimeText;
    }
    public PauseAssets _pauseAssets;

    [System.Serializable]
    public class CurrencyAssets
    {
        public GameObject _parent;
        public TextMeshProUGUI _quantityText;
        public Image _icon;
        public bool _available;
    }
    public CurrencyAssets[] _currencyAssets;

    [System.Serializable]
    public class GameOverAssets
    {
        public GameObject _parent;
        public GameObject[] _options;
        public Image _pointer;
        public int _onPos;
        public bool _onGameOver;
    }
    public GameOverAssets _gameOverAssets;

    public int _onWorldGlobal;
    public bool _introSpecial;

    [System.Serializable]
    public class NewSlimePanel
    {
        public Animator _parent;
        public Image _backgroundImage;
        public TextMeshProUGUI _slimeNameText;
    }
    public NewSlimePanel newSlimePanel;

    public Animator _currencyAnimator;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //private void Start()
    //{
    //    LoadProgressFromSDK();
    //}

    public void LoadSceneByName(string sceneName) => SceneManager.LoadScene(sceneName);

    // ---------------------------
    // PAUSA
    // ---------------------------
    public void SetPause()
    {
        Animator pauseAnimator = _pauseAssets._parent?.GetComponent<Animator>();

        // Asegurar que el animador de pausa use tiempo no escalado
        if (pauseAnimator != null)
            pauseAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (_pauseAssets._hintAvailable && _saveLoadValues._hintCoins > 0)
        {
            _pauseAssets._optionsText[1].color = Color.white;
        }
        else
        {
            _pauseAssets._optionsText[1].color = Color.gray;
        }

        if (!_pauseAssets._pause)
        {
            // Pausar el juego
            Time.timeScale = 0f;
            _pauseAssets._pause = true;
            pauseAnimator?.SetBool("PauseIn", true);


            for (int i = 0; i < _pauseAssets._allSlimeText.Length; i++)
            {
             
                    _pauseAssets._allSlimeText[i].gameObject.SetActive(false);
               
            }
            for (int i = 0; i < _pauseAssets._allSlimeText.Length; i++)
            {
                if (_saveLoadValues._slimeUnlocked[i + 1])
                {
                    _pauseAssets._allSlimeText[i].gameObject.SetActive(true);
                }
            }
        }
        else
        {
            // Reanudar el juego
            Time.timeScale = 1f;
            _pauseAssets._pause = false;
            pauseAnimator?.SetBool("PauseIn", false);
        }
    }


    // ---------------------------
    // GAME OVER
    // ---------------------------
    public void SetGameOver()
    {
        if (!_gameOverAssets._onGameOver)
        {
            Time.timeScale = 0f;
            _gameOverAssets._parent?.GetComponent<Animator>().SetBool("PauseIn", true);
            _gameOverAssets._onGameOver = true;
        }
        else
        {
            Time.timeScale = 1f;
            _gameOverAssets._parent?.GetComponent<Animator>().SetBool("PauseIn", false);
            _gameOverAssets._onGameOver = false;
        }
    }

    public bool IsPaused() => _pauseAssets._pause || _gameOverAssets._onGameOver;

    // ---------------------------
    // GUARDADO
    // ---------------------------
    public void SaveProgress()
    {
        if (_scriptInit == null) { Debug.LogWarning("⚠️ GameInitScript no asignado"); return; }
        _scriptInit.SaveGame();
        Debug.Log("💾 Guardado solicitado desde MainController");
    }

    public void Update()
    {
        _currencyAssets[0]._quantityText.text = _saveLoadValues._healthCoins.ToString();
        _currencyAssets[1]._quantityText.text = _saveLoadValues._hintCoins.ToString();
    }

    // ---------------------------
    // CARGA
    // ---------------------------
    //public void LoadProgressFromSDK()
    //{
    //    if (_scriptInit == null) { Debug.LogWarning("⚠️ GameInitScript no asignado"); return; }
    //    _scriptInit.LoadState();
    //}
}
