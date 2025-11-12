using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using LoL;
using LoLSDK;

[System.Serializable]
public class Progress
{
    public int currentProgress;
    public int maxProgress;
    public int score;
}

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
        public bool _finalWorldUnlocked;
        public bool[] _progressSave;
        public int _progress;
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

    private int _lastReportedProgress = 0;

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
            return;
        }

        // Inicializar _saveLoadValues con valores por defecto
        if (_saveLoadValues == null)
            _saveLoadValues = new SaveLoadValues();

        var empty = new SaveLoadValues();
        empty._worldsUnlocked = new bool[4] { true, false, false, false };
        empty._elementsUnlocked = new bool[4];
        empty._slimeUnlocked = new bool[7];
        empty._healthCoins = 1;
        empty._hintCoins = 1;
        empty._finalWorldUnlocked = false;
        empty._progressSave = new bool[8];
        empty._progress = 0;

        _saveLoadValues = empty; // Evita que la UI vea ceros
    }


    public void LoadSceneByName(string sceneName) => SceneManager.LoadScene(sceneName);

    // ---------------------------
    // PAUSA
    // ---------------------------
    public void SetPause()
    {
        _scriptSFX.PlaySound(_scriptSFX._chooseElement);
        Animator pauseAnimator = _pauseAssets._parent?.GetComponent<Animator>();
        if (pauseAnimator != null)
            pauseAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (_pauseAssets._hintAvailable && _saveLoadValues._hintCoins > 0)
            _pauseAssets._optionsText[1].color = Color.white;
        else
            _pauseAssets._optionsText[1].color = Color.gray;

        if (!_pauseAssets._pause)
        {
            Time.timeScale = 0f;
            _pauseAssets._pause = true;
            pauseAnimator?.SetBool("PauseIn", true);

            for (int i = 0; i < _pauseAssets._allSlimeText.Length; i++)
                _pauseAssets._allSlimeText[i].gameObject.SetActive(false);

            for (int i = 0; i < _pauseAssets._allSlimeText.Length; i++)
            {
                if (_saveLoadValues._slimeUnlocked != null && i + 1 < _saveLoadValues._slimeUnlocked.Length)
                {
                    if (_saveLoadValues._slimeUnlocked[i + 1])
                    {
                        _pauseAssets._allSlimeText[i].text = GameInitScript.Instance.GetText("Slime" + (i + 1).ToString("f0"));
                        _pauseAssets._allSlimeText[i].gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
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
        if (_scriptInit == null)
        {
            Debug.LogWarning("⚠️ GameInitScript no asignado");
            return;
        }
        _saveLoadValues._progress = 0;
        for(int i = 0; i < _saveLoadValues._progressSave.Length; i++)
        {
            if (_saveLoadValues._progressSave[i])
            {
                _saveLoadValues._progress++;
            }
        }

        _scriptInit.SaveGame();
        Debug.Log("💾 Guardado solicitado desde MainController");
    }

    public void Update()
    {
        _currencyAssets[0]._quantityText.text = _saveLoadValues._healthCoins.ToString();
        _currencyAssets[1]._quantityText.text = _saveLoadValues._hintCoins.ToString();
        if (!GameInitScript.Instance.stateLoaded) return; // <- evita mostrar ceros antes de cargar


        //if (_currencyAssets != null && _currencyAssets.Length >= 2)
        //{
        //    _currencyAssets[0]._quantityText.text = _saveLoadValues._healthCoins.ToString();
        //    _currencyAssets[1]._quantityText.text = _saveLoadValues._hintCoins.ToString();
        //}
    }


    public void FixSaveValues()
    {
        if (_saveLoadValues._worldsUnlocked == null || _saveLoadValues._worldsUnlocked.Length == 0)
            _saveLoadValues._worldsUnlocked = new bool[4];
        if (_saveLoadValues._elementsUnlocked == null || _saveLoadValues._elementsUnlocked.Length == 0)
            _saveLoadValues._elementsUnlocked = new bool[4];
        if (_saveLoadValues._slimeUnlocked == null || _saveLoadValues._slimeUnlocked.Length == 0)
            _saveLoadValues._slimeUnlocked = new bool[7];
        if (_saveLoadValues._healthCoins <= 0) _saveLoadValues._healthCoins = 1;
        if (_saveLoadValues._hintCoins <= 0) _saveLoadValues._hintCoins = 1;
        bool anyWorldUnlocked = false;
        foreach (bool unlocked in _saveLoadValues._worldsUnlocked)
            if (unlocked) { anyWorldUnlocked = true; break; }
        if (!anyWorldUnlocked) _saveLoadValues._worldsUnlocked[0] = true;
    }


    public void SubmitProgressToLoL(int currentProgress, int score = 0)
    {
        if (LOLSDK.Instance == null)
        {
            Debug.LogWarning("⚠️ LoL SDK no inicializado — progreso no enviado.");
            return;
        }

        int maxProgress = 8;
        if (currentProgress < _lastReportedProgress)
            currentProgress = _lastReportedProgress;

        _lastReportedProgress = currentProgress;

        LOLSDK.Instance.SubmitProgress(currentProgress, maxProgress, score);
        Debug.Log($"📊 Progreso enviado a LoL: {currentProgress}/{maxProgress}, Score: {score}");
    }
}
