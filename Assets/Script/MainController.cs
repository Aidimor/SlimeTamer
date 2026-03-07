using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using LoL;
using LoLSDK;
using System.Collections.Generic;

// Clase simple de progreso
[System.Serializable]
public class Progress
{
    public int currentProgress;
    public int maxProgress = 8;
    //public int score = 100;
}

public class MainController : MonoBehaviour
{
    public static MainController Instance;

    public bool _newGameplay;

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
    public Animator _restartBeam;
    public Animator _AtomAnimator;
    public AudioSource _bgmAS;
    public AudioClip[] _allBGM;
    public Animator _currencyAnimator;

    [System.Serializable]
    public class SaveLoadValues
    {
        public int _progress;
        public bool[] _worldsUnlocked = new bool[4] { true, false, false, false };
        public bool[] _progressSave = new bool[8];
        //1 - Start the game
        //2 - Tutorial Passed
        //3 - Dessert Level
        //4 - Snow Level
        //5 - Forest Level
        //6 - Boss First Encounter
        //7 - Boss Beaten Endgame
        //8 - Corrrect Element Fusion
        public int _totalSteps;
        public int _totalAtoms;
        public bool _pauseAvailable;
    
        public bool _restartTutorial;
        public bool _elementTutorial;
        public bool _restartAvailable;
        public bool _hazardTutorial;
        public bool _atomTutorial;
        public bool _fusionTutorial;
        

        //public int _progress;
        //public int maxProgress = 8;
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
    //public bool _introSpecial;
    private int _lastReportedProgress = 0;


    public Image _joystickImage;
    public TextMeshProUGUI _atomQuantityText;
    public TextMeshProUGUI[] _elementsQuantityText;
    public Animator[] _elementsAnimator;
    public TextMeshProUGUI[] _elementsName;

    [System.Serializable]
    public class AllTurnsInfo
    {
        public List<int> _stagesID = new List<int>();
    }
    public AllTurnsInfo[] _allTurnsInfo;

    public Animator _transformationAnimator;

    public TextMeshProUGUI _nameText;
    public TextMeshProUGUI _atributeText;
    public RawImage _slimeRawImage;
    [System.Serializable]
    public class ElementsCircles {
        public Image _cirlce;
        public TextMeshProUGUI _elementLetters;
        public TextMeshProUGUI _quantity;

    }
    public ElementsCircles[] _elementsCircles;
    public TextMeshProUGUI[] _dataTexts;
    public Color[] _elementsColor;
    public TextMeshProUGUI _continueText;

    [System.Serializable]
    public class ElementAnimatorAssets
    {
        public Animator _animator;
        public TextMeshProUGUI _elementText;
        public TextMeshProUGUI _quantityText;
        public Image _border;
        public Image _center;
        public TextMeshProUGUI _elementName;

    }
    public ElementAnimatorAssets _elementAnimatorAssets;

    [System.Serializable]
    public class TutorialAssets
    {
        public Animator _tutorialAnimator;
        public TextMeshProUGUI _tutorialText;
        public GameObject _arrowsParent;
        public GameObject _elementsParent;
        public GameObject _atomParent;
        public GameObject _stepParent;
        public GameObject _exitParent;
        public GameObject _slimeParent;
        public TextMeshProUGUI _continueText;
        public bool _tutorialDeployed;

        public TextMeshProUGUI[] _slimeNameText;
    }
    public TutorialAssets _tutorialAssets;

    [System.Serializable]
    public class WorldAssets
    {
        public Animator _worldAnimator;
        public Image _background;
        public TextMeshProUGUI _worldName;
        public Color[] _worldColors;


    }
    public WorldAssets _worldAssets;

    [System.Serializable]
    public class ExitAssets
    {
        public Animator _exitAnimator;
        public GameObject[] _parentOptions;
        public TextMeshProUGUI[] _textOptions;  
        public int _pos;
        public bool _exitPanelOn;
        public bool _moves;
    }
    public ExitAssets _exitAssets;


    [System.Serializable]
    public class AtomPanelInfo
    {
        public TextMeshProUGUI[] _textInfo;
        [System.Serializable]
        public class FusionElementsAssets
        {       
            public TextMeshProUGUI _name;
            public TextMeshProUGUI _extra;
        }
        public FusionElementsAssets[] _fusionElementAssets;

        public GameObject _lockedBar;
        public bool _lockedBool;
        public Animator _circleParents;
    
    }
    public AtomPanelInfo _atomPanelInfo;

    [System.Serializable]
    public class FinalTimerAssets
    {
        public float _timer;
        public TextMeshProUGUI _timerText;
        public bool _timerOn;
    }
    public FinalTimerAssets _finalTimerAssets;
    public TextMeshProUGUI _continuarTutorial;
    public bool _gameFinished;
    public bool _onPortrait;
    public bool _firstWorld;

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
        //SetIntProgress();
        SetStagesID();


    }

    public void StartGameContent()
    {
        Debug.Log("🚀 MainController: Contenido iniciado.");
        Time.timeScale = 1f;
    
      
    }



    public void SetStagesID()
    {
        _allTurnsInfo[0]._stagesID = new List<int> { 0, 1, 2, 3, 4 };
        _allTurnsInfo[1]._stagesID = new List<int> { 5, 6, 7, 8, 9, 10 };
        _allTurnsInfo[2]._stagesID = new List<int> { 11, 12, 13, 14, 15, 16 };
        //_allTurnsInfo[3]._stagesID = new List<int> { 17, 18, 19, 20, 21, 22, 25 };
        _allTurnsInfo[3]._stagesID = new List<int> { 23, 24, 25 };


        //----- TURNO 4 RANDOM -----
        List<int> possibleStages = new List<int> { 5, 6, 7, 8, 9, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
        List<int> bossPosibleStages = new List<int> {21, 22, 23, 24, 25 };

        List<int> randomStages = new List<int>();
        List<int> bossRandomStages = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, possibleStages.Count);
            randomStages.Add(possibleStages[randomIndex]);
            possibleStages.RemoveAt(randomIndex);
        }

        for (int i = 0; i < 5; i++)
        {
            int bossRandomIndex = Random.Range(0, bossPosibleStages.Count);
            bossRandomStages.Add(bossPosibleStages[bossRandomIndex]); // ✅ lista correcta
            bossPosibleStages.RemoveAt(bossRandomIndex);
        }

        // Inicializas la lista del turno 4
        _allTurnsInfo[4]._stagesID = new List<int>();

        // Agregas todo en orden
        _allTurnsInfo[4]._stagesID.AddRange(randomStages);
        _allTurnsInfo[4]._stagesID.AddRange(bossRandomStages);

    }



    public void LoadSceneByName(string sceneName) => SceneManager.LoadScene(sceneName);

    public void SetPause()
    {
        _scriptSFX.PlaySound(_scriptSFX._chooseElement);

        Animator pauseAnimator = _pauseAssets._parent?.GetComponent<Animator>();
        if (pauseAnimator != null)
            pauseAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        //if (_pauseAssets._hintAvailable && _saveLoadValues._hintCoins > 0)
        //    _pauseAssets._optionsText[1].color = Color.white;
        //else
        //    _pauseAssets._optionsText[1].color = Color.gray;

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
        //SetIntProgress();
        //for (int i = 0; i < _saveLoadValues._progressSave.Length; i++)
        //{
        //    if (_saveLoadValues._progressSave[i])
        //        _saveLoadValues._progress++;
        //}

        // Sincronizar con Progress
        //progress.currentProgress = _saveLoadValues._progress;

        // Actualizar en LoadedFullState si existe
        //if (GameInitScript.Instance.LoadedFullState != null)
        //    GameInitScript.Instance.LoadedFullState.currentProgress = _saveLoadValues._progress;
        //SubmitProgressToLoL(_saveLoadValues._progress);

        GameInitScript.Instance.SaveGame();
        Debug.Log("💾 Guardado OK.");
    }

    //public void SubmitProgressToLoL(int currentProgress)
    //{
    //    if (LOLSDK.Instance == null)
    //    {
    //        Debug.LogWarning("⚠️ LoL SDK no inicializado.");
    //        return;
    //    }

    //    int maxProgress = 8;
    //    if (currentProgress < _lastReportedProgress)
    //        //currentProgress = _lastReportedProgress;
    //    currentProgress = progress.currentProgress;

    //    //_lastReportedProgress = currentProgress;

    //    LOLSDK.Instance.SubmitProgress(currentProgress, maxProgress);
    //}

    //public void SetCoinsAndSave(int health, int hint)
    //{


    //    SaveProgress();

    //}

    //public void SetIntProgress()
    //{
    //    progress.currentProgress = 0;
    //    progress.maxProgress = _saveLoadValues._progressSave.Length;
    //    for(int i = 0; i < _saveLoadValues._progressSave.Length; i++)
    //    {
    //        if (_saveLoadValues._progressSave[i])
    //        {
    //            progress.currentProgress++;
    //        }
    //    }
    //}
}
