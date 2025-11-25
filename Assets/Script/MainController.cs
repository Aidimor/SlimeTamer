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
    // ... otras referencias a scripts ...
    public SFXscript _scriptSFX;
    public MusicController _scriptMusic;

    public Animator _bordersAnimator;
    // ... otras variables de Animator y AudioSource ...
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

    // ... otras clases serializables (PauseAssets, CurrencyAssets, GameOverAssets, NewSlimePanel) ...
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

        // PASO CRÍTICO 1: Crear la instancia del objeto contenedor.
        if (_saveLoadValues == null)
            _saveLoadValues = new SaveLoadValues();

        // Importante: No debe haber NINGUNA inicialización de _hintCoins o _healthCoins aquí.
    }

    // ---------------------------
    // MÉTODO DE ARRANQUE LLAMADO DESDE GameInitScript (LUEGO DE LA CARGA)
    // ---------------------------
    public void StartGameContent()
    {
        Debug.Log("🚀 MainController: Inicio de Contenido. El idioma y el estado de guardado están listos.");

        // 1. Asegúrate de que Time.timeScale esté en 1.
        Time.timeScale = 1f;

        // 2. 💡 CORRECCIÓN CLAVE: Actualiza la UI de las monedas con los valores cargados.
        UpdateCurrencyUI();

        // 3. Carga aquí la escena inicial del juego, el menú principal o inicia el primer diálogo.
        // Por ejemplo, si tienes una escena llamada "WorldMapScene":
        //LoadSceneByName("WorldMapScene");
    }

    /// <summary>
    /// Actualiza la UI de las monedas (Health y Hint) con los valores cargados en _saveLoadValues.
    /// Se llama solo una vez en StartGameContent.
    /// </summary>
    private void UpdateCurrencyUI()
    {
        // Asumiendo que _currencyAssets[0] es Health y [1] es Hint
        if (_currencyAssets != null && _currencyAssets.Length > 1)
        {
            // Moneda 0: Health
            if (_currencyAssets[0]?._quantityText != null)
            {
                _currencyAssets[0]._quantityText.text = _saveLoadValues._healthCoins.ToString();
                Debug.Log($"💰 UI Health Coin Actualizada a: {_saveLoadValues._healthCoins}");
            }

            // Moneda 1: Hint
            if (_currencyAssets[1]?._quantityText != null)
            {
                _currencyAssets[1]._quantityText.text = _saveLoadValues._hintCoins.ToString();
                Debug.Log($"💰 UI Hint Coin Actualizada a: {_saveLoadValues._hintCoins}");
            }
        }
        else
        {
            Debug.LogError("❌ CurrencyAssets no está configurado correctamente en MainController.");
        }
    }


    public void LoadSceneByName(string sceneName) => SceneManager.LoadScene(sceneName);

    // ---------------------------
    // PAUSA (Lógica omitida por brevedad, no hay cambios requeridos aquí)
    // ---------------------------
    public void SetPause()
    {
        // ... (código SetPause)
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
                        // Ahora es seguro llamar a GetText
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
    // GAME OVER (Lógica omitida por brevedad, no hay cambios requeridos aquí)
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
    // GUARDADO (Lógica correcta)
    // ---------------------------
    public void SaveProgress()
    {
        if (_scriptInit == null)
        {
            Debug.LogWarning("⚠️ GameInitScript no asignado");
            return;
        }
        _saveLoadValues._progress = 0;
        for (int i = 0; i < _saveLoadValues._progressSave.Length; i++)
        {
            if (_saveLoadValues._progressSave[i])
            {
                _saveLoadValues._progress++;
            }
        }

        _scriptInit.SaveGame();
        Debug.Log("💾 Guardado solicitado desde MainController");
    }

    // ---------------------------
    // ACTUALIZACIÓN DEL JUEGO
    // ---------------------------
    public void Update()
    {
        // 🛑 ELIMINADAS LAS ASIGNACIONES DE TEXTO DE MONEDAS DEL UPDATE.
        // La actualización de monedas (UI) ahora se gestiona en UpdateCurrencyUI()
        // y se llama manualmente cuando las monedas cambian y en StartGameContent().

        if (_scriptInit == null || !_scriptInit.stateLoaded || !_scriptInit.languageReady)
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
        Debug.Log($"📊 Progreso enviado a LoL: {currentProgress}/{maxProgress}, Score: {score}");
    }
}