using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using LoL;
using LoLSDK;

// Se recomienda usar el GameSaveState de LoL.GameInitScript si es el mismo.
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

    // --- Referencias a Scripts ---
    public PortraitController _scriptPortrait;
    public GameInitScript _scriptInit;
    public SFXscript _scriptSFX;
    public MusicController _scriptMusic;
    // --- Fin Referencias a Scripts ---

    // --- Assets y Componentes ---
    public Animator _bordersAnimator;
    public Animator _cinematicBorders;
    public AudioSource _bgmAS;
    public AudioClip[] _allBGM;
    public Animator _currencyAnimator;
    // --- Fin Assets y Componentes ---

    // La clase SaveLoadValues debe coincidir con GameSaveState en GameInitScript
    [System.Serializable]
    public class SaveLoadValues
    {
        // 🔑 VALORES INICIALES (JUEGO NUEVO).
        public bool[] _worldsUnlocked = new bool[4] { true, false, false, false };
        public bool[] _elementsUnlocked = new bool[4];
        public int _healthCoins = 1;
        public int _hintCoins = 1;
        public bool[] _slimeUnlocked = new bool[7];
        public bool _finalWorldUnlocked = false;
        public bool[] _progressSave = new bool[8];
        public int _progress = 0;
    }

    // CRÍTICO: Objeto de datos que será cargado por GameInitScript.
    public SaveLoadValues _saveLoadValues;

    // --- Otras Clases Serializables ---
    // (PauseAssets, CurrencyAssets, GameOverAssets, NewSlimePanel sin cambios)
    #region SerializableClasses
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
    #endregion

    // --- Variables de Estado ---
    public int _onWorldGlobal;
    public bool _introSpecial;
    private int _lastReportedProgress = 0;
    // --- Fin Variables de Estado ---


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

        // 🔥 CORRECCIÓN CRÍTICA: Inicializar la clase de guardado si es NULL.
        // Esto garantiza que el GameInitScript siempre tiene una estructura válida para sobreescribir.
        if (_saveLoadValues == null)
            _saveLoadValues = new SaveLoadValues();

        // Asegurar que GameInitScript tiene una referencia a este MainController
        if (GameInitScript.Instance != null)
        {
            GameInitScript.Instance.mainController = this;
        }
    }

    // -----------------------------------------------------------------------------------
    // MÉTODO DE ARRANQUE LLAMADO DESDE GameInitScript
    // -----------------------------------------------------------------------------------
    public void StartGameContent()
    {
        Debug.Log("🚀 MainController: Inicio de Contenido. El idioma y el estado de guardado están listos.");
        Time.timeScale = 1f;
        UpdateCurrencyUI();
    }

    // -----------------------------------------------------------------------------------
    // ACTUALIZACIÓN DE UI
    // -----------------------------------------------------------------------------------
    public void UpdateCurrencyUI()
    {
        if (_currencyAssets == null || _currencyAssets.Length < 2 || _saveLoadValues == null)
        {
            Debug.LogError("❌ No se puede actualizar la UI: Faltan referencias en MainController o SaveLoadValues es NULL.");
            return;
        }

        // Moneda 0: Health
        if (_currencyAssets[0]?._quantityText != null)
        {
            _currencyAssets[0]._quantityText.text = _saveLoadValues._healthCoins.ToString("f0");
            Debug.Log($"💰 UI Health Coin Actualizada a: {_saveLoadValues._healthCoins}");
        }

        // Moneda 1: Hint
        if (_currencyAssets[1]?._quantityText != null)
        {
            _currencyAssets[1]._quantityText.text = _saveLoadValues._hintCoins.ToString("f0");
            Debug.Log($"💰 UI Hint Coin Actualizada a: {_saveLoadValues._hintCoins}");
        }
    }


    public void LoadSceneByName(string sceneName) => SceneManager.LoadScene(sceneName);

    // -----------------------------------------------------------------------------------
    // PAUSA
    // -----------------------------------------------------------------------------------
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

    // -----------------------------------------------------------------------------------
    // GAME OVER
    // -----------------------------------------------------------------------------------
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

    // -----------------------------------------------------------------------------------
    // GUARDADO
    // -----------------------------------------------------------------------------------
    public void SaveProgress()
    {
        // Usar GameInitScript.Instance en lugar de _scriptInit, ya que es el Singleton persistente
        if (GameInitScript.Instance == null)
        {
            Debug.LogWarning("⚠️ GameInitScript no está listo o no existe.");
            return;
        }

        // Recalcular el progreso total antes de guardar
        _saveLoadValues._progress = 0;
        for (int i = 0; i < _saveLoadValues._progressSave.Length; i++)
        {
            if (_saveLoadValues._progressSave[i])
            {
                _saveLoadValues._progress++;
            }
        }

        GameInitScript.Instance.SaveGame();
        Debug.Log("💾 Guardado solicitado desde MainController");
    }

    public void Update()
    {
        if (GameInitScript.Instance == null || !GameInitScript.Instance.stateLoaded || !GameInitScript.Instance.languageReady)
            return;
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
    }

    public void SetCoinsAndSave(int health, int hint)
    {
        if (_saveLoadValues == null) return;
        _saveLoadValues._healthCoins = health;
        _saveLoadValues._hintCoins = hint;
        UpdateCurrencyUI(); // Muestra los nuevos valores inmediatamente
        SaveProgress(); // Llama a GameInitScript.SaveGame()
        Debug.Log($"¡FORZADO! Monedas guardadas: Health={health}, Hint={hint}");
    }
}