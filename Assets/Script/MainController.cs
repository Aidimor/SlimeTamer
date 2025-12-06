using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using LoL;
using LoLSDK;

// Clase simple de progreso
[System.Serializable]
public class Progress
{
    public int currentProgress;
    public int maxProgress = 8;
    public int score = 100;
}

public class MainController : MonoBehaviour
{
    public static MainController Instance;

    // --- SISTEMA CENTRALIZADO DE PROGRESO ---
    public Progress progress = new Progress();

    // --- Referencias a Scripts ---
    public PortraitController _scriptPortrait;
    public GameInitScript _scriptInit;
    public SFXscript _scriptSFX;
    public MusicController _scriptMusic;

    // --- Assets y Componentes ---
    public Animator _bordersAnimator;
    public Animator _cinematicBorders;
    public AudioSource _bgmAS;
    public AudioClip[] _allBGM;
    public Animator _currencyAnimator;

    [System.Serializable]
    public class SaveLoadValues
    {
        public bool[] _worldsUnlocked = new bool[4] { true, false, false, false };
        public bool[] _elementsUnlocked = new bool[4];
        public int _healthCoins = 1;
        public int _hintCoins = 1;
        public bool[] _slimeUnlocked = new bool[7];
        public bool _finalWorldUnlocked = false;
        public bool[] _progressSave = new bool[8];
        public int _progress = 0;
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

    [System.Serializable]
    public class NewSlimePanel
    {
        public Animator _parent;
        public Image _backgroundImage;
        public TextMeshProUGUI _slimeNameText;
    }
    public NewSlimePanel newSlimePanel;

    public int _onWorldGlobal;
    public bool _introSpecial;
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

        if (_saveLoadValues == null)
            _saveLoadValues = new SaveLoadValues();

        if (GameInitScript.Instance != null)
        {
            GameInitScript.Instance.mainController = this;
        }
    }

    public void StartGameContent()
    {
        Debug.Log("🚀 MainController: Contenido iniciado.");
        Time.timeScale = 1f;
        UpdateCurrencyUI();
    }

    public void UpdateCurrencyUI()
    {
        if (_currencyAssets == null || _currencyAssets.Length < 2 || _saveLoadValues == null)
        {
            Debug.LogError("❌ No se puede actualizar la UI.");
            return;
        }

        _currencyAssets[0]._quantityText.text = _saveLoadValues._healthCoins.ToString("f0");
        _currencyAssets[1]._quantityText.text = _saveLoadValues._hintCoins.ToString("f0");

        for(int i = 0; i < _pauseAssets._allSlimeText.Length; i++)
        {
            _pauseAssets._allSlimeText[i].gameObject.SetActive(_saveLoadValues._slimeUnlocked[i + 1]);
        }
    }

    public void LoadSceneByName(string sceneName) => SceneManager.LoadScene(sceneName);

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
        }
        else
        {
            Time.timeScale = 1f;
            _pauseAssets._pause = false;
            pauseAnimator?.SetBool("PauseIn", false);
        }
    }

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

    // -------------------------------------------------------------------------
    // GUARDADO / PROGRESO
    // -------------------------------------------------------------------------
    public void SaveProgress()
    {
        if (GameInitScript.Instance == null)
        {
            Debug.LogWarning("⚠️ GameInitScript no listo.");
            return;
        }

        // Recalcular progreso
        _saveLoadValues._progress = 0;
        for (int i = 0; i < _saveLoadValues._progressSave.Length; i++)
        {
            if (_saveLoadValues._progressSave[i])
                _saveLoadValues._progress++;
        }

        // Sincronizar con Progress
        progress.currentProgress = _saveLoadValues._progress;

        // Actualizar en LoadedFullState si existe
        if (GameInitScript.Instance.LoadedFullState != null)
            GameInitScript.Instance.LoadedFullState.currentProgress = _saveLoadValues._progress;

        GameInitScript.Instance.SaveGame();
        Debug.Log("💾 Guardado OK.");
    }

    public void SubmitProgressToLoL(int currentProgress, int score = 0)
    {
        if (LOLSDK.Instance == null)
        {
            Debug.LogWarning("⚠️ LoL SDK no inicializado.");
            return;
        }

        int maxProgress = 8;
        if (currentProgress < _lastReportedProgress)
            currentProgress = _lastReportedProgress;

        _lastReportedProgress = currentProgress;

        LOLSDK.Instance.SubmitProgress(currentProgress, maxProgress, score);
    }

    public void SetCoinsAndSave(int health, int hint)
    {
        _saveLoadValues._healthCoins = health;
        _saveLoadValues._hintCoins = hint;
        UpdateCurrencyUI();
        SaveProgress();
        Debug.Log($"Monedas forzadas: Health={health}, Hint={hint}");
    }
}
