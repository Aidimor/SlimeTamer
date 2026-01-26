using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using LoL;  // <- necesario para GameInitScript
using LoLSDK;

public class NewMainGameplay : MonoBehaviour
{
    public Transform[] _allPositions;
    public GameObject[] _groundAssets;
    public NewGameEvent[] _allStages;
    //public NewGameEvent[] _allNormalStages;
    //public NewGameEvent[] _allBossStages;
    public List<GameObject> _allGrounds = new List<GameObject>();
    public List<GameObject> _allElements = new List<GameObject>();
    public List<GameObject> _allHazards = new List<GameObject>();
    public List<GameObject> _allAtoms = new List<GameObject>();
    public List<GameObject> _allSteps = new List<GameObject>();
    public List<GameObject> _allAttacks = new List<GameObject>();
    public List<int> _allCountAttacks = new List<int>();
    public Transform _parent;

    public GameObject _elementPrefab;
    public GameObject _atomPrefab;
    public GameObject _stepsPrefab;
    public GameObject _hazardPrefab;
    public GameObject _entrancePrefab;
    public GameObject _exitPrefab;
    public GameObject _bossAttackParent;

    public int _idStage;

    public float _movementSpeed = 6f;
    public bool _movementAvailable = true;
    private bool _buttonPressed;

    public int _onPose;
    public GameObject _slimeObject;

    public Color[] _lockedColors;
    public List<GameObject> _exitEntranceObjects = new List<GameObject>();

    public SlimeController _scriptSlime;

    // 0 = abajo, 1 = arriba, 2 = derecha, 3 = izquierda
    public bool[] _movesAvailable = new bool[4];

    [System.Serializable]
    public class SlimeInfo
    {
        public int _slimeID; //0=Normal,1=Solid,2=Liquid,3=Gas
        public int[] _elementsParticles; //0=Carbon,1=Hydrogen,2=Oxygen,3=Ferreum
        public Color[] _allSlimeColors;
        public TextMeshProUGUI[] _quantityElementText;

       
        public TextMeshProUGUI _stepsText;
  
        public TextMeshProUGUI _atomsText;
    }
    public SlimeInfo _slimeInfo;
    public Sprite[] _allGroundsSprites;
    public Image[] _backgroundImage;
    public Color[] _backgroundColor;

    public Animator _slimeAnimator;
    public int _atomsObtained;
    //public List<int> _atomList = new List<int>();
    public List<int> _stepsList = new List<int>();
    public Color _slimeMainColor;
    //public Animator _transformAnimator;
    bool _transformed;



    public ParticleSystem _hitWalk;
    public ParticleSystem _waterWalk;
    public ParticleSystem _smoke;
    public ParticleSystem _deadSlimeParticle;

    public List<int> _elementsID = new List<int>();
    public List<bool> _elementsBool = new List<bool>();

    public bool _restarted;
    public GameObject _bossUI;

    [System.Serializable]
    public class TentaclesAssets
    {
        public Animator _tentacle;
        public Camera _tentacleCamera;
    }
    public TentaclesAssets[] _tentacleAssets;
    public int _onCamera;
    public GameObject _mainUI;

    public ParticleSystem _sandStorm;
    public ParticleSystem[] _windParticle;
    public ParticleSystem _snowParticle;
    public int _movementsToSandStorm;

    public int _turnsReturnToWater;
    public bool _sandStormOn;
    public int _turnToStorm;
    public bool _AtomsPanelOn;
   
    public int _onPoseJoystick;
    public Vector2[] _allJoystickPoses;
    public Animator _bossAnimator;

    [System.Serializable]
    public class BoulderSwitch
    {
        public GameObject _boulder;
     
        public bool _movingBoulder;
        public GameObject _columnObject;
        public GameObject _switchPose;

    }
    public BoulderSwitch _boulderSwitch;

    public bool itTransforms;
    public Animator _electroAnimator;

    public bool _worldNameShown;
    public bool _stageHazardOn;
    public Color _lastStageColor;

    public bool earthquakeOn;

    Coroutine earthquakeRoutine;
    public ParticleSystem _crumbleParticle;

    public TextMeshProUGUI _escapeText;
    public TextMeshProUGUI _exitText;
    public bool _gameStarts;
    public ParticleSystem _lowAirLeft;
    public ParticleSystem _lowAirRight;

    public GameObject _fusionParent;
    public TextMeshProUGUI[] _fusionMenuTexts;
    void Start()
    {   
        StartVoids();
        MainController.Instance._bordersAnimator.SetBool("BorderOut", true);
        MainController.Instance._cinematicBorders.SetBool("FadeIn", false);
        _movementAvailable = false;
        switch (MainController.Instance._onWorldGlobal)
        {
            case 2:
                _snowParticle.Play();
                break;
            case 3:
                _movementsToSandStorm = Random.Range(2, 4);
                break;
        }
        _escapeText.text = GameInitScript.Instance.GetText("escape");
        _exitText.text = GameInitScript.Instance.GetText("exitmenu");
        MainController.Instance._tutorialAssets._continueText.text = GameInitScript.Instance.GetText("continue");
    }



    public void StartVoids()
    {
        var Main = MainController.Instance;

        // ==========================
        // 🔴 VALIDACIONES BASE
        // ==========================

        if (Main == null ||
            Main._allTurnsInfo == null ||
            Main._onWorldGlobal < 0 ||
            Main._onWorldGlobal >= Main._allTurnsInfo.Length)
        {
            Debug.LogError("MainController o TurnsInfo inválido en StartVoids");
            return;
        }

        int stageIndex = Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage];

        if (stageIndex < 0 || stageIndex >= _allStages.Length)
        {
            Debug.LogError("stageIndex fuera de rango en StartVoids");
            return;
        }

        var stage = _allStages[stageIndex];

        if (stage == null)
        {
            Debug.LogError("Stage NULL en StartVoids");
            return;
        }

        // ==========================
        // 🧱 CREACIÓN DE STAGE
        // ==========================

        if (!_restarted)
        {
            StageCreationVoid();
            SetSteps();
        }

        SetElements();
        //SetAtoms();

        // ==========================
        // 🐲 BOSS
        // ==========================

        if (stage._bossAssets != null && stage._bossAssets.Length > 0 && stage._bossAssets[0] != null)
        {
            SetBossAttacks();
        }

        // ==========================
        // ⚠️ HAZARDS / ENTRADAS
        // ==========================

        SetHazards();
        SetEntranceExit();
        _exitEntranceObjects[0].GetComponent<ExitScriptObject>()._entranceText.text = GameInitScript.Instance.GetText("entrance");
        _fusionMenuTexts[0].text = GameInitScript.Instance.GetText("fusion1");
        _fusionMenuTexts[1].text = GameInitScript.Instance.GetText("fusion2");
        if (MainController.Instance._saveLoadValues._atomTutorial)
        {
            _fusionParent.gameObject.SetActive(true);
        }
        // ==========================
        // 🧪 VARIABLES DE JUEGO
        // ==========================

        _atomsObtained = 0;

        _onPose = stage._spawnPoint;

        // ==========================
        // 🟢 SLIME (SEGURO)
        // ==========================

        if (_slimeObject == null)
        {
            Debug.LogError("_slimeObject es NULL en StartVoids");
            return;
        }

        if (_onPose < 0 || _onPose >= _allPositions.Length)
        {
            Debug.LogError("SpawnPoint fuera de rango en StartVoids");
            return;
        }

        if (_allPositions[_onPose] == null)
        {
            Debug.LogError($"_allPositions[{_onPose}] es NULL");
            return;
        }

        RectTransform slimeRT = _slimeObject.GetComponent<RectTransform>();
        RectTransform spawnRT = _allPositions[_onPose].GetComponent<RectTransform>();

        if (slimeRT == null || spawnRT == null)
        {
            Debug.LogError("RectTransform NULL en slime o spawn");
            return;
        }

        slimeRT.position = spawnRT.position;
        slimeRT.localEulerAngles = new Vector3(0, 0, 180f);

        // ==========================
        // ▶️ INICIO DE JUEGO
        // ==========================

        CalculateMoves();
        StartCoroutine(StartGameNumerator());
    }


    public IEnumerator StartGameNumerator()
    {
        var Main = MainController.Instance;
        Main._tutorialAssets._continueText.gameObject.SetActive(false);
        var gi = GameInitScript.Instance;
        ResetAllHazards();
        //_slimeObject.GetComponent<RectTransform>().localScale = Vector3.zero;
        switch (_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._wind)
        {
            case NewGameEvent.Wind.Center:
                break;
            case NewGameEvent.Wind.Left:
                Main._scriptSFX._windSetVolume = 0.2f;
                _lowAirLeft.Play();
                break;
            case NewGameEvent.Wind.Right:
                _lowAirRight.Play();
                Main._scriptSFX._windSetVolume = 0.2f;
                break;
        }


        switch (Main._onWorldGlobal)
        {
            case 0:
                if (_idStage == 0)
                {
                    if (!_worldNameShown)
                    {                  
                        Main._worldAssets._worldName.text = gi.GetText("WorldName" + (Main._onWorldGlobal + 1).ToString());
                        Main._worldAssets._background.color = Main._worldAssets._worldColors[Main._onWorldGlobal];                     
                        Main._worldAssets._worldAnimator.SetTrigger("WorldNameIn");
                        _worldNameShown = true;
                        yield return new WaitForSeconds(2);
                    }
       

                    yield return new WaitForSeconds(1);
                    Main._tutorialAssets._tutorialText.text = gi.GetText("tutorial0");


                    string key = "tutorial0";                 
                    // SpeakText necesita la KEY, no el ID
                    string speakKey = key;
                    yield return new WaitForSeconds(0.2f);
                    LOLSDK.Instance.SpeakText(speakKey);

                    Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                    Main._tutorialAssets._arrowsParent.SetActive(true);
                    Main._tutorialAssets._elementsParent.SetActive(false);
                    Main._tutorialAssets._atomParent.SetActive(false);
                    yield return new WaitForSeconds(1);
                    Main._tutorialAssets._continueText.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.25f);
                    while (!Input.GetButtonDown("Submit"))
                    {
                        yield return null;
                    }
                    SFXscript.Instance.PlaySound(SFXscript.Instance._whip);
                    Main._tutorialAssets._continueText.gameObject.SetActive(false);
                    Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
                }


                break;
            case 1:
            case 2:
    
                break;
            case 3:
                if(_idStage == 0)
                {
                    if (!_worldNameShown)
                    {
                        Main._worldAssets._worldName.text = gi.GetText("WorldName" + (Main._onWorldGlobal + 1).ToString());
                        Main._worldAssets._background.color = Main._worldAssets._worldColors[Main._onWorldGlobal];
                        Main._worldAssets._worldAnimator.SetTrigger("WorldNameIn");
                        _worldNameShown = true;
                        yield return new WaitForSeconds(2);
                    }

                    yield return new WaitForSeconds(1);
                    Main._tutorialAssets._tutorialText.text = gi.GetText("tutorial5");

                    string key = "tutorial5";
                    // SpeakText necesita la KEY, no el ID
                    string speakKey = key;
                    yield return new WaitForSeconds(0.2f);
                    LOLSDK.Instance.SpeakText(speakKey);

                    Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                    Main._tutorialAssets._arrowsParent.SetActive(false);
                    Main._tutorialAssets._elementsParent.SetActive(false);
                    Main._tutorialAssets._atomParent.SetActive(false);
                    Main._tutorialAssets._stepParent.SetActive(false);
                    yield return new WaitForSeconds(1);
                    Main._tutorialAssets._continueText.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.25f);
                    while (!Input.GetButtonDown("Submit"))
                    {
                        yield return null;
                    }
                    SFXscript.Instance.PlaySound(SFXscript.Instance._whip);
                    Main._tutorialAssets._continueText.gameObject.SetActive(false);
                    Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
                }

                if (!_stageHazardOn)
                {
                    yield return new WaitForSeconds(1);
                    Main._tutorialAssets._tutorialText.text = gi.GetText(("hazard") + Main._onWorldGlobal.ToString());

                    string key = "hazard" + Main._onWorldGlobal.ToString();
                    // SpeakText necesita la KEY, no el ID
                    string speakKey = key;
                    yield return new WaitForSeconds(0.2f);
                    LOLSDK.Instance.SpeakText(speakKey);

                    Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                    Main._tutorialAssets._arrowsParent.SetActive(false);
                    Main._tutorialAssets._elementsParent.SetActive(false);
                    Main._tutorialAssets._atomParent.SetActive(false);
                    Main._tutorialAssets._stepParent.SetActive(false);
     
               
                    yield return new WaitForSeconds(2f);

                    SFXscript.Instance.PlaySound(SFXscript.Instance._whip);
                    Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
                    _stageHazardOn = true;
                }
         

                break;


        }

        Main._tutorialAssets._tutorialDeployed = true;

        var StageInfo2 = Main._allTurnsInfo[Main._onWorldGlobal];

        if (_idStage >= StageInfo2._stagesID.Count - 1)
        {
            _exitEntranceObjects[1].GetComponent<ExitScriptObject>()._exitPlatforms[0].gameObject.SetActive(false);
            _exitEntranceObjects[1].GetComponent<ExitScriptObject>()._exitPlatforms[1].gameObject.SetActive(true);
        }
        else
        {
            _exitEntranceObjects[1].GetComponent<ExitScriptObject>()._exitPlatforms[0].gameObject.SetActive(true);
            _exitEntranceObjects[1].GetComponent<ExitScriptObject>()._exitPlatforms[1].gameObject.SetActive(false);    
        }

        yield return new WaitForSeconds(1);
        _slimeObject.GetComponent<Animator>().Play("SlimeEnters");
        yield return new WaitForSeconds(0.5f);
        _turnToStorm = 5;
        _gameStarts = true;
             _movementAvailable = true;
     
 
    }


    void Update()
    {
        if (!_AtomsPanelOn)
        {

            if (!MainController.Instance._exitAssets._exitPanelOn)
            {
                PlayerMovementController();
            }
   
            _slimeMainColor = Color.Lerp(_slimeMainColor, _slimeInfo._allSlimeColors[_slimeInfo._slimeID], 2 * Time.deltaTime);
            _scriptSlime._slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor", _slimeMainColor);
            for (int i = 0; i < _slimeInfo._elementsParticles.Length; i++)
            {
                _slimeInfo._quantityElementText[i].text = _slimeInfo._elementsParticles[i].ToString();
            }
            _slimeInfo._atomsText.text = MainController.Instance._saveLoadValues._totalAtoms.ToString();
            _slimeInfo._stepsText.text = MainController.Instance._saveLoadValues._totalSteps.ToString();

            _mainUI.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_mainUI.GetComponent<RectTransform>().anchoredPosition, new Vector2(0, 0), 2 * Time.deltaTime);

            if (Input.GetButtonDown("Submit") && MainController.Instance._tutorialAssets._tutorialAnimator.GetBool("TutorialIn") == false && MainController.Instance._saveLoadValues._restartAvailable && _movementAvailable && !MainController.Instance._exitAssets._exitPanelOn)
            {
                SpecialRestartLevel();
            }


        }
   
        if (Input.GetButtonDown("Pause") && MainController.Instance._saveLoadValues._pauseAvailable && !MainController.Instance._exitAssets._exitPanelOn && _gameStarts)
        {

            _AtomsPanelOn = !_AtomsPanelOn;
            MainController.Instance._AtomAnimator.SetBool("AtomsIn", _AtomsPanelOn);

        }
        else
        {
            if (!MainController.Instance._exitAssets._exitPanelOn)
            {
                AtomPanelController();
            }
      
        }

        if (_boulderSwitch._movingBoulder){
            _boulderSwitch._boulder.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_boulderSwitch._boulder.GetComponent<RectTransform>().anchoredPosition,
               _boulderSwitch._switchPose.GetComponent<RectTransform>().anchoredPosition, 2 * Time.deltaTime);
        }


        if (MainController.Instance._saveLoadValues._totalAtoms < 0)
        {
            MainController.Instance._saveLoadValues._totalAtoms = 0;
        }
        if (MainController.Instance._exitAssets._exitPanelOn)
        {
            ExitPanelController();
      
        }

        if (Input.GetButtonDown("Cancel"))
        {
            StartCoroutine(ExitPanelNumerator());
        }
    }

    public void AtomPanelController()
    {
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            _onPoseJoystick = 1;
        }
        if (Input.GetAxisRaw("Vertical") < 0)
        {
            _onPoseJoystick = 2;
        }
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            _onPoseJoystick = 3;
        }
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            _onPoseJoystick = 4;
        }

        if(Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
        {
            _onPoseJoystick = 0;
        }

       MainController.Instance._joystickImage.GetComponent<RectTransform>().anchoredPosition = Vector2.MoveTowards(MainController.Instance._joystickImage.GetComponent<RectTransform>().anchoredPosition,
        _allJoystickPoses[_onPoseJoystick], 500);

        for(int i = 0; i < 4; i++)
        {
            MainController.Instance._elementsQuantityText[i].text = _slimeInfo._elementsParticles[i].ToString();
        }

        MainController.Instance._atomQuantityText.text = MainController.Instance._saveLoadValues._totalAtoms.ToString();

        if (Input.GetButtonDown("Submit") && !_transformed && MainController.Instance._saveLoadValues._totalAtoms > 0)
        {
            switch (_onPoseJoystick)
            {
                case 1:
                    _slimeInfo._elementsParticles[0]++;
                    if(MainController.Instance._saveLoadValues._totalAtoms > 0) { MainController.Instance._saveLoadValues._totalAtoms--; }
                    
                    if (!_transformed)
                    {
                        TransformSlimeVoid();
           
                    }
                    break;
                case 2:
                    _slimeInfo._elementsParticles[3]++;
                    if (MainController.Instance._saveLoadValues._totalAtoms > 0) { MainController.Instance._saveLoadValues._totalAtoms--; }
                    if (!_transformed)
                    {
                        TransformSlimeVoid();
              
                    }
                    break;
                 
                case 3:
                    _slimeInfo._elementsParticles[2]++;
                    if (MainController.Instance._saveLoadValues._totalAtoms > 0) { MainController.Instance._saveLoadValues._totalAtoms--; }
                    if (!_transformed)
                    {
                        TransformSlimeVoid();
                 
                    }
                    break;
            
                case 4:
                    _slimeInfo._elementsParticles[1]++;
                    if (MainController.Instance._saveLoadValues._totalAtoms > 0) { MainController.Instance._saveLoadValues._totalAtoms--; }
                    if (!_transformed)
                    {
                        TransformSlimeVoid();
          
                    }
                    break;
            }
        }
  
    }

    public IEnumerator ExitPanelNumerator()
    {
        MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._chooseElement);
        switch (MainController.Instance._exitAssets._exitPanelOn)
        {
            case false:
                MainController.Instance._exitAssets._exitPanelOn = true;
                MainController.Instance._exitAssets._exitAnimator.SetBool("ExitEnter", true);
                MainController.Instance._exitAssets._pos = 0;
                for (int i = 0; i < 3; i++)
                {
                    MainController.Instance._exitAssets._parentOptions[i].GetComponent<RectTransform>().localScale = new Vector2(1f, 1f);
                }
                MainController.Instance._exitAssets._parentOptions[MainController.Instance._exitAssets._pos].GetComponent<RectTransform>().localScale = new Vector2(1.2f, 1.2f);
                break;
            case true:
                MainController.Instance._exitAssets._exitAnimator.SetBool("ExitEnter", false);
                yield return new WaitForSeconds(1);
                MainController.Instance._exitAssets._exitPanelOn = false;
                break;
        }

    
    }

   void ExitPanelController()
    {

        if (Input.GetButtonDown("Submit"))
        {
            MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._chooseElement);
            switch (MainController.Instance._exitAssets._pos)
            {
                case 0:
                    MainController.Instance._exitAssets._exitAnimator.SetBool("ExitEnter", false);      
                    MainController.Instance._exitAssets._exitPanelOn = false;
                    break;
                case 1:
                    StartCoroutine(NormalExitNumerator());
                    break;
                case 2:
                    break;
            }
        }

        if(Input.GetAxisRaw("Vertical") < 0 && MainController.Instance._exitAssets._pos < 2 && !MainController.Instance._exitAssets._moves)
        {
            MainController.Instance._exitAssets._pos++;
            MainController.Instance._exitAssets._moves = true;
            for(int i = 0; i < 3; i++)
            {
                MainController.Instance._exitAssets._parentOptions[i].GetComponent<RectTransform>().localScale = new Vector2(1f, 1f);
            }
            MainController.Instance._exitAssets._parentOptions[MainController.Instance._exitAssets._pos].GetComponent<RectTransform>().localScale = new Vector2(1.2f, 1.2f);
            MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._next);
        }

        if (Input.GetAxisRaw("Vertical") > 0 && MainController.Instance._exitAssets._pos > 0 && !MainController.Instance._exitAssets._moves)
        {
            MainController.Instance._exitAssets._pos--;
            MainController.Instance._exitAssets._moves = true;
            for (int i = 0; i < 3; i++)
            {
                MainController.Instance._exitAssets._parentOptions[i].GetComponent<RectTransform>().localScale = new Vector2(1f, 1f);
            }
            MainController.Instance._exitAssets._parentOptions[MainController.Instance._exitAssets._pos].GetComponent<RectTransform>().localScale = new Vector2(1.2f, 1.2f);
            MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._next);
        }

        if(Input.GetAxisRaw("Vertical") == 0)
        {
            MainController.Instance._exitAssets._moves = false;
        }

    }

    IEnumerator NormalExitNumerator()
    {
        //MainController.Instance._saveLoadValues._restartAvailable = true;
        MainController.Instance._introSpecial = false;
        MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
        _AtomsPanelOn = false;
        MainController.Instance._AtomAnimator.SetBool("AtomsIn", false);
        //MainController.Instance._cinematicBorders.SetBool("FadeIn", false);
        yield return new WaitForSeconds(1);
        MainController.Instance._exitAssets._exitAnimator.SetBool("ExitEnter", false);
        MainController.Instance._exitAssets._exitPanelOn = false;
        MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[0];
        MainController.Instance._scriptMusic._audioBGM.Play();
        yield return new WaitForSeconds(1);
        MainController.Instance.LoadSceneByName("IntroScene");
    }

    // ===================== STAGE =====================

    void StageCreationVoid()
    {
        var Main = MainController.Instance;
        _allGrounds.Clear();

        int stageIndex = Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage];
        var stage = _allStages[stageIndex];

        // 🔴 PROTECCIÓN CRÍTICA
        if (stage == null)
        {
            Debug.LogError("Stage es NULL en StageCreationVoid");
            _bossUI.SetActive(false);
            return;
        }

        foreach (int place in stage._allPlaces)
        {
            GameObject ground = Instantiate(_groundAssets[0], _parent);

            RectTransform groundRT = ground.GetComponent<RectTransform>();
            RectTransform targetRT = _allPositions[place].GetComponent<RectTransform>();

            groundRT.position = targetRT.position;
            groundRT.localScale = Vector3.one;

            StageGroundScript g = ground.GetComponent<StageGroundScript>();
            g._id = place;
            g._lockedBool = false;

            Image groundImage = ground.GetComponent<Image>();
            groundImage.sprite = _allGroundsSprites[Main._onWorldGlobal];

            _backgroundImage[0].sprite = _allGroundsSprites[Main._onWorldGlobal];
            _backgroundImage[0].color = _backgroundColor[Main._onWorldGlobal];

            switch (Main._onWorldGlobal)
            {
                default:
                    _backgroundImage[1].sprite = _allGroundsSprites[Main._onWorldGlobal];
                    _backgroundImage[2].sprite = _allGroundsSprites[Main._onWorldGlobal];
                    break;

                case 4:
                    List<int> posiblesNumeros = new List<int> { 0, 3 };
                    int randomIndex = Random.Range(0, posiblesNumeros.Count);

                    _backgroundImage[1].sprite = _allGroundsSprites[0];
                    _backgroundImage[2].sprite = _allGroundsSprites[0];
                    _backgroundImage[1].color = _lastStageColor;
                    _backgroundImage[2].color = _lastStageColor;

                    groundImage.sprite = _allGroundsSprites[posiblesNumeros[randomIndex]];
                    groundImage.color = _lastStageColor;
                    break;
            }

            _allGrounds.Add(ground);
        }

        // ==========================
        // 🐲 BOSS UI (SEGURO)
        // ==========================

        Debug.Log(stage._bossAssets == null ? "bossAssets NULL" : stage._bossAssets.Length.ToString());

        if (stage._bossAssets != null && stage._bossAssets.Length > 0 && stage._bossAssets[0] != null)
        {
            _bossUI.SetActive(true);

            var boss = stage._bossAssets[0];
            RectTransform bossRT = _bossUI.GetComponent<RectTransform>();

            switch (boss._yposition)
            {
                case NewGameEvent.BossAssets._Yposition.Top:
                    bossRT.anchoredPosition = new Vector2(bossRT.anchoredPosition.x, -90f);
                    bossRT.localScale = Vector3.one;

                    switch (boss._xposition)
                    {
                        case NewGameEvent.BossAssets._Xposition.Left:
                            bossRT.localEulerAngles = new Vector3(0, 0, 45);
                            break;
                        case NewGameEvent.BossAssets._Xposition.Center:
                            bossRT.localEulerAngles = Vector3.zero;
                            break;
                        case NewGameEvent.BossAssets._Xposition.Right:
                            bossRT.localEulerAngles = new Vector3(0, 0, -45);
                            break;
                    }
                    break;

                case NewGameEvent.BossAssets._Yposition.Center:
                    bossRT.anchoredPosition = new Vector2(bossRT.anchoredPosition.x, 0);
                    bossRT.localScale = Vector3.one;

                    switch (boss._xposition)
                    {
                        case NewGameEvent.BossAssets._Xposition.Left:
                            bossRT.localEulerAngles = new Vector3(0, 0, 90);
                            break;
                        case NewGameEvent.BossAssets._Xposition.Center:
                            bossRT.localEulerAngles = Vector3.zero;
                            break;
                        case NewGameEvent.BossAssets._Xposition.Right:
                            bossRT.localEulerAngles = new Vector3(0, 0, -90);
                            break;
                    }
                    break;

                case NewGameEvent.BossAssets._Yposition.Bot:
                    bossRT.anchoredPosition = new Vector2(bossRT.anchoredPosition.x, 90f);
                    bossRT.localScale = new Vector3(1, -1, 1);

                    switch (boss._xposition)
                    {
                        case NewGameEvent.BossAssets._Xposition.Left:
                            bossRT.localEulerAngles = new Vector3(0, 0, -45);
                            break;
                        case NewGameEvent.BossAssets._Xposition.Center:
                            bossRT.localEulerAngles = Vector3.zero;
                            break;
                        case NewGameEvent.BossAssets._Xposition.Right:
                            bossRT.localEulerAngles = new Vector3(0, 0, 45);
                            break;
                    }
                    break;
            }
        }
        else
        {
            _bossUI.SetActive(false);
        }
    }


    void SetElements()
    {
        var Main = MainController.Instance;
 
        foreach (var data in _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._elements)
        {
            switch (Main._onWorldGlobal)
            {
                case 4:
                    int random3 = Random.Range(0, 100);
                    if (random3 < 90)
                    {


                        // 🆕 NO existe → instanciar
                        GameObject atom = Instantiate(_atomPrefab, _parent);

                        RectTransform rta1 = atom.GetComponent<RectTransform>();
                        RectTransform targetRT = _allPositions[data._onPlace].GetComponent<RectTransform>();

                        data._changed = true;

                        rta1.position = targetRT.position;
                        rta1.localScale = Vector3.one;

                        AtomScript atomScript = atom.GetComponent<AtomScript>();
                        atomScript._quantity += data._quantity;
                        atomScript._quantityText.text = data._quantity.ToString();
                        atomScript._onPose = data._onPlace;
                        _allAtoms.Add(atom);
                        data._emptyAtom = true;

                    }
                    else
                    {
                        GameObject element1 = Instantiate(_elementPrefab, _parent);
                        RectTransform rt1 = element1.GetComponent<RectTransform>();
                        rt1.position = _allPositions[data._onPlace].GetComponent<RectTransform>().position;
                        rt1.localScale = Vector3.one;

                        ElementOrbScript orb1 = element1.GetComponent<ElementOrbScript>();
                        orb1._onPose = data._onPlace;
                        switch (data._elementType)
                        {
                            case NewGameEvent.Elements.ElementType.C:
                                orb1.ID = 0;
                                break;
                            case NewGameEvent.Elements.ElementType.H:
                                orb1.ID = 1;
                                break;
                            case NewGameEvent.Elements.ElementType.O:
                                orb1.ID = 2;
                                break;
                            case NewGameEvent.Elements.ElementType.Fe:
                                orb1.ID = 3;
                                break;
                        }

                        orb1._onPose = data._onPlace;
                        orb1._quantity = data._quantity;
                        orb1.ElementSetVoid();
                        _allElements.Add(element1);
                        _elementsID.Add(data._onPlace);
                        _elementsBool.Add(true);
                        data._emptyAtom = false;
                    }
                    break;
                case 3:
                    int random1 = Random.Range(0, 100);
                    if (random1 < 50)
                    {


                        // 🆕 NO existe → instanciar
                        GameObject atom = Instantiate(_atomPrefab, _parent);

                        RectTransform rta1 = atom.GetComponent<RectTransform>();
                        RectTransform targetRT = _allPositions[data._onPlace].GetComponent<RectTransform>();

                        data._changed = true;

                        rta1.position = targetRT.position;
                        rta1.localScale = Vector3.one;

                        AtomScript atomScript = atom.GetComponent<AtomScript>();
                        atomScript._quantity += data._quantity;
                        atomScript._quantityText.text = data._quantity.ToString();
                        atomScript._onPose = data._onPlace;
                        _allAtoms.Add(atom);
                        data._emptyAtom = true;

                    }
                    else
                    {
                        GameObject element1 = Instantiate(_elementPrefab, _parent);
                        RectTransform rt1 = element1.GetComponent<RectTransform>();
                        rt1.position = _allPositions[data._onPlace].GetComponent<RectTransform>().position;
                        rt1.localScale = Vector3.one;

                        ElementOrbScript orb1 = element1.GetComponent<ElementOrbScript>();
                        orb1._onPose = data._onPlace;
                        switch (data._elementType)
                        {
                            case NewGameEvent.Elements.ElementType.C:
                                orb1.ID = 0;
                                break;
                            case NewGameEvent.Elements.ElementType.H:
                                orb1.ID = 1;
                                break;
                            case NewGameEvent.Elements.ElementType.O:
                                orb1.ID = 2;
                                break;
                            case NewGameEvent.Elements.ElementType.Fe:
                                orb1.ID = 3;
                                break;
                        }

                        orb1._onPose = data._onPlace;
                        orb1._quantity = data._quantity;
                        orb1.ElementSetVoid();
                        _allElements.Add(element1);
                        _elementsID.Add(data._onPlace);
                        _elementsBool.Add(true);
                        data._emptyAtom = false;
                    }
                    break;
                case 2:
                    int random2 = Random.Range(0, 100);
                    if (random2 < 20)
                    {


                        // 🆕 NO existe → instanciar
                        GameObject atom = Instantiate(_atomPrefab, _parent);

                        RectTransform rta1 = atom.GetComponent<RectTransform>();
                        RectTransform targetRT = _allPositions[data._onPlace].GetComponent<RectTransform>();

                        data._changed = true;

                        rta1.position = targetRT.position;
                        rta1.localScale = Vector3.one;

                        AtomScript atomScript = atom.GetComponent<AtomScript>();
                        atomScript._quantity += data._quantity;
                        atomScript._quantityText.text = data._quantity.ToString();
                        atomScript._onPose = data._onPlace;
                        data._emptyAtom = true;
                        _allAtoms.Add(atom);

                        //_atomList.Add(data._onPlace);

                    }
                    else
                    {
                        GameObject element1 = Instantiate(_elementPrefab, _parent);
                        RectTransform rt1 = element1.GetComponent<RectTransform>();
                        rt1.position = _allPositions[data._onPlace].GetComponent<RectTransform>().position;
                        rt1.localScale = Vector3.one;

                        ElementOrbScript orb1 = element1.GetComponent<ElementOrbScript>();
                        orb1._onPose = data._onPlace;
                        switch (data._elementType)
                        {
                            case NewGameEvent.Elements.ElementType.C:
                                orb1.ID = 0;
                                break;
                            case NewGameEvent.Elements.ElementType.H:
                                orb1.ID = 1;
                                break;
                            case NewGameEvent.Elements.ElementType.O:
                                orb1.ID = 2;
                                break;
                            case NewGameEvent.Elements.ElementType.Fe:
                                orb1.ID = 3;
                                break;
                        }
                        data._emptyAtom = false;
                        orb1._onPose = data._onPlace;
                        orb1._quantity = data._quantity;
                        orb1.ElementSetVoid();
                        _allElements.Add(element1);
                        _elementsID.Add(data._onPlace);
                        _elementsBool.Add(true);
                    }
                
                    break;
                default:
                    GameObject element = Instantiate(_elementPrefab, _parent);
                    RectTransform rt = element.GetComponent<RectTransform>();
                    rt.position = _allPositions[data._onPlace].GetComponent<RectTransform>().position;
                    rt.localScale = Vector3.one;

                    ElementOrbScript orb = element.GetComponent<ElementOrbScript>();
                    orb._onPose = data._onPlace;
                    switch (data._elementType)
                    {
                        case NewGameEvent.Elements.ElementType.C:
                            orb.ID = 0;
                            break;
                        case NewGameEvent.Elements.ElementType.H:
                            orb.ID = 1;
                            break;
                        case NewGameEvent.Elements.ElementType.O:
                            orb.ID = 2;
                            break;
                        case NewGameEvent.Elements.ElementType.Fe:
                            orb.ID = 3;
                            break;
                    }

                    orb._onPose = data._onPlace;
                    orb._quantity = data._quantity;
                    orb.ElementSetVoid();
                    _allElements.Add(element);
                    _elementsID.Add(data._onPlace);
                    _elementsBool.Add(true);
                    data._emptyAtom = false;
                    break;
            }
    
        }

   






    }

    void SetSteps()
    {
        var Main = MainController.Instance;

        // ==========================
        // 🔴 VALIDACIONES CRÍTICAS
        // ==========================

        if (Main == null ||
            Main._allTurnsInfo == null ||
            Main._onWorldGlobal < 0 ||
            Main._onWorldGlobal >= Main._allTurnsInfo.Length)
        {
            Debug.LogError("TurnsInfo inválido en SetSteps");
            return;
        }

        int stageIndex = Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage];

        if (stageIndex < 0 || stageIndex >= _allStages.Length)
        {
            Debug.LogError("stageIndex fuera de rango en SetSteps");
            return;
        }

        var stage = _allStages[stageIndex];

        if (stage == null)
        {
            Debug.LogError("Stage NULL en SetSteps");
            return;
        }

        if (stage._stepsPlace == null || stage._stepsPlace.Length == 0)
        {
            // No es error: simplemente no hay steps
            return;
        }

        // ==========================
        // 🪜 CREACIÓN DE STEPS
        // ==========================

        for (int i = 0; i < stage._stepsPlace.Length; i++)
        {
            int placeIndex = stage._stepsPlace[i];

            if (placeIndex < 0 || placeIndex >= _allPositions.Length)
            {
                Debug.LogWarning($"Place inválido en SetSteps: {placeIndex}");
                continue;
            }

            if (_allPositions[placeIndex] == null)
            {
                Debug.LogWarning($"_allPositions[{placeIndex}] es NULL");
                continue;
            }

            GameObject steps = Instantiate(_stepsPrefab, _parent);

            RectTransform rt = steps.GetComponent<RectTransform>();
            RectTransform targetRT = _allPositions[placeIndex].GetComponent<RectTransform>();

            if (rt == null || targetRT == null)
            {
                Debug.LogWarning("RectTransform NULL en steps");
                Destroy(steps);
                continue;
            }

            rt.position = targetRT.position;
            rt.localScale = Vector3.one;

            _allSteps.Add(steps);
            _stepsList.Add(placeIndex);
        }
    }


    void SetHazards()
    {
        var Main = MainController.Instance;
        foreach (var data in _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._hazards)
        {
            GameObject hazard = Instantiate(_hazardPrefab, _parent);
            RectTransform rt = hazard.GetComponent<RectTransform>();
            rt.position = _allPositions[data._onPlace].GetComponent<RectTransform>().position;
            rt.localScale = Vector3.one;

            ObstaclesScript obs = hazard.GetComponent<ObstaclesScript>();
            switch (data._hazards)
            {
                case NewGameEvent.Hazards.HazardsType.Fire:

                    obs._id = 0;
                    obs._allObstacles[0].SetActive(true);
                    break;
                case NewGameEvent.Hazards.HazardsType.Hole:
                    obs._id = 1;
                    obs._allObstacles[1].SetActive(true);
                    break;
                case NewGameEvent.Hazards.HazardsType.Switch:
                    obs._id = 2;
                    obs._allObstacles[2].SetActive(true);
                    _boulderSwitch._switchPose = hazard;
                    break;
                case NewGameEvent.Hazards.HazardsType.Column:
                    obs._id = 3;
                    obs._allObstacles[4].SetActive(true);
                    _boulderSwitch._columnObject = hazard;
                    break;
                case NewGameEvent.Hazards.HazardsType.MetalBall:
                    obs._id = 4;
                    obs._allObstacles[5].SetActive(true);

                    //_allBoulders.Add(hazard);
                    break;
                case NewGameEvent.Hazards.HazardsType.MagnetoPlace:
                    obs._id = 5;
                    obs._gravityPoint.gameObject.SetActive(true);
                    break;
                case NewGameEvent.Hazards.HazardsType.Electricity:
                    obs._id = 6;
                    obs._electricityParticle.gameObject.SetActive(true);
                    switch (data._rotation)
                    {
                        case NewGameEvent.Hazards.Rotation.Center:
                            break;
                        case NewGameEvent.Hazards.Rotation.Horizontal:
                            hazard.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 90);
                            break;
                        case NewGameEvent.Hazards.Rotation.Vertical:
                            hazard.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);
                            break;
                    }
                    break;

                case NewGameEvent.Hazards.HazardsType.CenterElectricity:
                    obs._id = 7;
                    obs._electricityCenterParticle.gameObject.SetActive(true);
                    break;
            }
            _allHazards.Add(hazard);
        }
    }

    void SetBossAttacks()
    {
        var Main = MainController.Instance;
        foreach (var data in _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]
            ._bossAssets[0]
            ._groundAttacks)
        {
            GameObject bossAttacks = Instantiate(_bossAttackParent, _parent);

            RectTransform rt = bossAttacks.GetComponent<RectTransform>();
            rt.position = _allPositions[data._groundID]
                .GetComponent<RectTransform>().position;
            rt.localScale = Vector3.one;

            _allAttacks.Add(bossAttacks);
            _allCountAttacks.Add(data._attackCount);
            bossAttacks.GetComponent<AtomScript>()._onTurnText.text =
                data._attackCount.ToString("F0");
            bossAttacks.GetComponent<AtomScript>()._tentacleRawImage[data._tentacleID].gameObject.SetActive(true);


        }
    }

    public IEnumerator BossAttackTurn()
    {
        var Main = MainController.Instance;
        var CurrentGrounds = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]
            ._bossAssets[0];
        ReduceCounter();
     
        for (int i = 0; i < _allCountAttacks.Count; i++)
        {
            switch (_allCountAttacks[i])
            {
                case -1:
                    _allCountAttacks[i] = CurrentGrounds
                        ._groundAttacks[i]._attackCount;

                    _allAttacks[i]
.GetComponent<AtomScript>()
._onTurnText.text = _allCountAttacks[i].ToString();
                    break;

                case 0:
                    _bossAnimator.SetTrigger("Attack");
                    _tentacleAssets[_onCamera]._tentacle
                        .SetTrigger("TentacleIn");

                    //ReduceCounter();
                    for (int y = 0; y < CurrentGrounds._groundAttacks.Length; y++)
                    {
                        Debug.Log(CurrentGrounds._groundAttacks[i]._groundID);
                        Debug.Log("Pose: " + _onPose);
                        if (_onPose == CurrentGrounds._groundAttacks[y]._groundID)
                        {
                            Debug.Log("SLIME MUERE");
                            StopMoveCoroutine();
                           _slimeObject.GetComponent<Animator>().Play("SlimeDies");
                            _deadSlimeParticle.Play();
                            yield return new WaitForSeconds(1f);
                          
                            switch(MainController.Instance._onWorldGlobal == 1)
                            {
                                case false:
                                    StartCoroutine(RestartLevel());
                                    break;
                                case true:
                                    NextWorld();
                                    break;                        

                            }
                        
                            yield break;
                        }
                    }
                    break;

                case 1:
                case 2:
                case 3:
                case 4:
                  
                    break;
            }

     
        }
    }

    void ReduceCounter()
    {
        for (int i = 0; i < _allCountAttacks.Count; i++)
        {
            _allCountAttacks[i]--;
            _allAttacks[i]
      .GetComponent<AtomScript>()
      ._onTurnText.text = _allCountAttacks[i].ToString();
        }
    }




    void SetEntranceExit()
    {
        var Main = MainController.Instance;
        CreateMarker(_entrancePrefab, _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._spawnPoint);
        CreateMarker(_exitPrefab, _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._exitPoint);       
     
    }

    void CreateMarker(GameObject prefab, int place)
    {
        GameObject obj = Instantiate(prefab, _parent);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.position = _allPositions[place].GetComponent<RectTransform>().position;
        rt.localScale = Vector3.one;
        _exitEntranceObjects.Add(obj);

 
    }

    // ===================== MOVEMENT =====================

    void PlayerMovementController()
    {
        RectTransform slimeRT = _slimeObject.GetComponent<RectTransform>();
        RectTransform targetRT = _allPositions[_onPose].GetComponent<RectTransform>();

        slimeRT.position = Vector3.MoveTowards(
            slimeRT.position,
            targetRT.position,
            _movementSpeed * Time.deltaTime
        );

        if (_movementAvailable) {
            // Reset input
            if (Input.GetAxisRaw("Horizontal") == 0 &&
            Input.GetAxisRaw("Vertical") == 0)
            {
                _buttonPressed = false;
            }

            if (_buttonPressed) return;

    
                if (Input.GetAxisRaw("Horizontal") > 0 &&
                    _movesAvailable[2] &&
                    _onPose != 4 &&
                    _onPose != 9 &&
                    _onPose != 14 &&
                    _onPose != 19 &&
                    _onPose != 24)
                {
                    _slimeObject.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, -90);
                    MoveTo(_onPose + 1);

                }


                else if (Input.GetAxisRaw("Horizontal") < 0 && _movesAvailable[3] &&
                    _onPose != 0 &&
                    _onPose != 5 &&
                    _onPose != 15 &&
                    _onPose != 20)
                {
                    _slimeObject.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 90);
                    MoveTo(_onPose - 1);

                }



                else if (Input.GetAxisRaw("Vertical") > 0 && _movesAvailable[0])
                {
                    _slimeObject.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
                    MoveTo(_onPose + 5);

                }



                else if (Input.GetAxisRaw("Vertical") < 0 && _movesAvailable[1])
                {
                    _slimeObject.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 180);
                    MoveTo(_onPose - 5);

                }
            }

        

    }

    void MoveTo(int newPose)
    {
        StartMoveCoroutine();
        LockCurrentPosition();

        _onPose = newPose;
        _buttonPressed = true;

        bool restart = IsLocked();
        //Debug.Log("restart = " + restart);
        // 🔑 SIEMPRE recalcular
        CalculateMoves();
        HazardDetection();
        StartCoroutine(ElementDetection());
        StartCoroutine(AtomDetection());
        StartCoroutine(StepDetection());
        ExitDetection();

        if (restart)
        {
        
        StartCoroutine(RestartLevel());
            // Aquí luego puedes resetear nivel, animar, etc.
        }
    }

    private Coroutine _moveCoroutine;

    bool IsLocked()
    {
        for (int i = 0; i < _allGrounds.Count; i++)
        {
            StageGroundScript ground = _allGrounds[i].GetComponent<StageGroundScript>();

            if (ground._id == _onPose && ground._lockedBool)
            {
                if (MainController.Instance._saveLoadValues._totalSteps > 0)
                {
                    MainController.Instance._saveLoadValues._totalSteps--;
                    ground._lockImage.color = _lockedColors[2];
                    return false; // estaba locked pero se desbloquea con pasos
                }
                else
                {
                    return true; // estaba locked y NO hay pasos
                }
            }
        }

        return false; // no se encontró locked
    }


    public void StartMoveCoroutine()
    {
        // Evita duplicados
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        _moveCoroutine = StartCoroutine(MoveNumerator());
    }

    public void StopMoveCoroutine()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        // Reset de estado por seguridad
        _slimeAnimator.SetBool("Moving", false);
        _movementAvailable = true;
    }



    public IEnumerator MoveNumerator()
    {
        var Main = MainController.Instance;

        // ==========================
        // 🔴 VALIDACIONES BASE
        // ==========================

        if (Main == null ||
            Main._allTurnsInfo == null ||
            Main._onWorldGlobal < 0 ||
            Main._onWorldGlobal >= Main._allTurnsInfo.Length)
        {
            Debug.LogError("Main o TurnsInfo inválido en MoveNumerator");
            yield break;
        }

        int stageIndex = Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage];

        if (stageIndex < 0 || stageIndex >= _allStages.Length)
        {
            Debug.LogError("stageIndex fuera de rango en MoveNumerator");
            yield break;
        }

        var stage = _allStages[stageIndex];

        if (stage == null)
        {
            Debug.LogError("Stage NULL en MoveNumerator");
            yield break;
        }

        if (_slimeAnimator == null)
        {
            Debug.LogError("_slimeAnimator es NULL");
            yield break;
        }

        // ==========================
        // ▶️ INICIO MOVIMIENTO
        // ==========================

        _movementAvailable = false;
        _slimeAnimator.SetBool("Moving", true);

        yield return new WaitForSeconds(0.5f);

        // ==========================
        // 🌍 EFECTOS POR MUNDO
        // ==========================

        switch (Main._onWorldGlobal)
        {
            case 1:
                if (_sandStorm != null)
                {
                    if (!_sandStormOn)
                    {
                        _movementsToSandStorm--;
                        if (_movementsToSandStorm <= 0)
                        {
                            Main._scriptSFX._windSetVolume = 0.75f;
                            _sandStorm.Play();
                            _movementsToSandStorm = Random.Range(5, 7);
                            _sandStormOn = true;
                        }
                    }
                    else
                    {
                        Main._scriptSFX._windSetVolume = 0f;
                        _sandStorm.Stop();
                        _sandStormOn = false;
                    }
                }
                break;

            case 3:
                _turnToStorm--;

                if (_turnToStorm <= 0)
                {
                    if (stage._wind != null)
                    {
                        switch (stage._wind)
                        {
                            case NewGameEvent.Wind.Left:
                                if (_windParticle != null && _windParticle.Length > 0 && _windParticle[0] != null)
                                    _windParticle[0].Play();
                        

                                yield return new WaitForSeconds(0.25f);

                                if (_movesAvailable != null &&
                                    _movesAvailable.Length > 2 &&
                                    _movesAvailable[2] &&
                                    _onPose != 4 && _onPose != 9 && _onPose != 14 && _onPose != 19)
                                {
                                    Main._scriptSFX._strongWindSetVolume = 0.7f;
                                    Main._scriptSFX._strongWind.volume = 0.7f;
                                    _onPose++;
                                }

                                yield return new WaitForSeconds(0.25f);
                                Main._scriptSFX._strongWindSetVolume = 0;
                                break;

                            case NewGameEvent.Wind.Right:
                                if (_windParticle != null && _windParticle.Length > 1 && _windParticle[1] != null)
                                    _windParticle[1].Play();
                         

                                yield return new WaitForSeconds(0.25f);

                                if (_movesAvailable != null &&
                                    _movesAvailable.Length > 3 &&
                                    _movesAvailable[3] &&
                                    _onPose != 0 && _onPose != 5 && _onPose != 10 && _onPose != 15 && _onPose != 20)
                                {
                                    Main._scriptSFX._strongWindSetVolume = 0.7f;
                                    Main._scriptSFX._strongWind.volume = 0.7f;
                                    _onPose--;
                                }
                          
                                yield return new WaitForSeconds(0.25f);
                                Main._scriptSFX._strongWindSetVolume = 0f;
                                break;
                        }
                    }

                    if (_windParticle != null)
                    {
                        if (_windParticle.Length > 0 && _windParticle[0] != null) _windParticle[0].Stop();
                        if (_windParticle.Length > 1 && _windParticle[1] != null) _windParticle[1].Stop();
                    }

                    CalculateMoves();
                    AtomDetection();
                    StartCoroutine(ElementDetection());
                    HazardDetection();
                    StepDetection();
                    ExitDetection();

                    _turnToStorm = 5;
                }
                break;
        }

        // ==========================
        // 🐲 TURNO DE BOSS
        // ==========================

        if (stage._bossAssets != null && stage._bossAssets.Length > 0 && stage._bossAssets[0] != null)
        {
            StartCoroutine(BossAttackTurn());
        }

        // ==========================
        // 🔥 RETORNO A AGUA / HIELO
        // ==========================

        if (_slimeInfo != null && _turnsReturnToWater < 0 && _slimeInfo._slimeID == 2)
        {
            if (stage._hazards != null)
            {
                bool onFire = false;

                for (int i = 0; i < stage._hazards.Length; i++)
                {
                    if (stage._hazards[i] != null &&
                        _onPose == stage._hazards[i]._onPlace &&
                        stage._hazards[i]._hazards == NewGameEvent.Hazards.HazardsType.Fire)
                    {
                        onFire = true;
                        break;
                    }
                }

                if (!onFire)
                    _turnsReturnToWater++;
            }

            if (_turnsReturnToWater >= 0)
            {
                itTransforms = true;
                _slimeInfo._slimeID = 4;

                if (Main._elementsCircles != null && Main._elementsCircles.Length >= 2)
                {
                    Main._elementsCircles[0]._cirlce.color = Main._elementsColor[1];
                    Main._elementsCircles[0]._elementLetters.text = "H";
                    Main._elementsCircles[0]._elementLetters.color = Main._elementsColor[1];
                    Main._elementsCircles[0]._quantity.text = "2";

                    Main._elementsCircles[1]._cirlce.color = Main._elementsColor[2];
                    Main._elementsCircles[1]._elementLetters.text = "O";
                    Main._elementsCircles[1]._elementLetters.color = Main._elementsColor[2];
                    Main._elementsCircles[1]._quantity.text = "";
                }

                Main._dataTexts[0].text = GameInitScript.Instance.GetText("snow1");
                Main._dataTexts[1].text = GameInitScript.Instance.GetText("snow2");

                MainController.Instance._atributeText.text = GameInitScript.Instance.GetText("ICEextra");
                MainController.Instance._nameText.text = GameInitScript.Instance.GetText("ICE");

                if (_waterWalk != null) _waterWalk.Play();
                if (_smoke != null) _smoke.Stop();

                StartCoroutine(TransormatioNumerator());
                _turnsReturnToWater = -1;
            }
        }

        // ==========================
        // ▶️ FIN MOVIMIENTO
        // ==========================

        _slimeAnimator.SetBool("Moving", false);
        yield return new WaitForSeconds(0.3f);
        _movementAvailable = true;
    }



    // ===================== LOGIC =====================

    void CalculateMoves()
    {
        var Main = MainController.Instance;
        var _stageId = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];
        for (int i = 0; i < 4; i++)
            _movesAvailable[i] = false;

        foreach (int place in _stageId._allPlaces)
        {
            if (place == _onPose + 5) _movesAvailable[0] = true;
            if (place == _onPose - 5) _movesAvailable[1] = true;
            if (place == _onPose + 1) _movesAvailable[2] = true;
            if (place == _onPose - 1) _movesAvailable[3] = true;
        }

        for(int i = 0; i < _stageId._hazards.Length; i++)
        {
            if (_stageId._hazards[i]._hazards == NewGameEvent.Hazards.HazardsType.Column)
            {
                if(_onPose + 5 == _stageId._hazards[i]._onPlace && !_stageId._hazards[i]._finished) _movesAvailable[0] = false;
                if (_onPose - 5 == _stageId._hazards[i]._onPlace && !_stageId._hazards[i]._finished) _movesAvailable[1] = false;
                if (_onPose + 1 == _stageId._hazards[i]._onPlace && !_stageId._hazards[i]._finished) _movesAvailable[2] = false;
                if (_onPose - 1 == _stageId._hazards[i]._onPlace && !_stageId._hazards[i]._finished) _movesAvailable[3] = false;

            }
        }
    }

    void LockCurrentPosition()
    {
        foreach (GameObject g in _allGrounds)
        {
            StageGroundScript ground = g.GetComponent<StageGroundScript>();

            if (ground._id == _onPose)
            {
                if (ground._lockedBool)
                {
                    if(MainController.Instance._saveLoadValues._totalSteps > 0)
                    {
                        ground._lockImage.color = _lockedColors[2];
                       
                    }
                 
             
                }
                else
                {                
                    ground._lockedBool = true;
                    ground._lockImage.color = _lockedColors[1];
                }
         
                return;
            }
        }

        for (int i = 0; i < _allGrounds.Count; i++)
        {
            if (_allGrounds[i].GetComponent<StageGroundScript>()._id == _onPose)
            {
                if (_allGrounds[i].GetComponent<StageGroundScript>()._lockedBool)
                {
                    MainController.Instance._saveLoadValues._totalSteps--;
                    _allGrounds[i].GetComponent<StageGroundScript>()._lockImage.color = _lockedColors[2];
         
                    break;
                }

            }
        }
    }




    public IEnumerator RestartLevel()
    {
     
        var Main = MainController.Instance;
        Main._scriptSFX.PlaySound(Main._scriptSFX._failSound);
        var _realID = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];
        if (!Main._saveLoadValues._restartTutorial)
        {
            StopMoveCoroutine();
            _movementAvailable = false;
            yield return new WaitForSeconds(1);
            Main._tutorialAssets._arrowsParent.SetActive(false);
            Main._tutorialAssets._atomParent.SetActive(false);
            Main._tutorialAssets._elementsParent.SetActive(false);
            Main._tutorialAssets._stepParent.SetActive(false);
            Main._tutorialAssets._tutorialText.text = GameInitScript.Instance.GetText("tutorial1");
            Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
            Main._tutorialAssets._arrowsParent.SetActive(false);
            Main._tutorialAssets._elementsParent.SetActive(false);
            Main._tutorialAssets._atomParent.SetActive(true);
            Main._tutorialAssets._stepParent.SetActive(false);
            yield return new WaitForSeconds(1);
            Main._tutorialAssets._continueText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.25f);
  
            while (!Input.GetButtonDown("Submit"))
            {
                yield return null;
            }
            Main._tutorialAssets._continueText.gameObject.SetActive(false);
            Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
            _movementAvailable = true;
      
            Main._saveLoadValues._restartTutorial = true;

        }
        MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._failSound);
        MainController.Instance._restartBeam.Play("RestartBeam");
        _slimeObject.GetComponent<Animator>().Play("SlimeLeavesAnimation");
        StopMoveCoroutine();
        for (int i = 0; i < _realID._elements.Length; i++)
        {
            _realID._elements[i]._changed = false;
        }

        _movementAvailable = false;
        for (int i = 0; i < _realID._hazards.Length; i++)
        {
            _realID._hazards[i]._finished = false;
        }
        Debug.Log(_realID._spawnPoint);
        for (int i = 0; i < _allGrounds.Count; i++)
        {
            _allGrounds[i].GetComponent<StageGroundScript>()._lockedBool = false;
            _allGrounds[i].GetComponent<StageGroundScript>()._lockImage.color = _lockedColors[0];
        }
        for (int i = 0; i < _allElements.Count; i++)
        {
            Destroy(_allElements[i]);
        }
        _allElements.Clear();
        for (int i = 0; i < _allHazards.Count; i++)
        {
            Destroy(_allHazards[i]);
        }
        Destroy(_exitEntranceObjects[0]);
        Destroy(_exitEntranceObjects[1]);
        _exitEntranceObjects.Clear();
        for (int i = 0; i < _allAtoms.Count; i++)
        {
            Destroy(_allAtoms[i]);
        }
        _allAtoms.Clear();
        for (int i = 0; i < _allAttacks.Count; i++)
        {
            Destroy(_allAttacks[i]);
        }
        _allAttacks.Clear();



        _allCountAttacks.Clear();
        _restarted = true;
        _allHazards.Clear();
        _elementsBool.Clear();
        _slimeInfo._elementsParticles[0] = 0;
        _slimeInfo._elementsParticles[1] = 0;
        _slimeInfo._elementsParticles[2] = 0;
        _slimeInfo._elementsParticles[3] = 0;
        _elementsID.Clear();
        MainController.Instance._saveLoadValues._totalAtoms -= _atomsObtained;
        _waterWalk.Stop();
        _smoke.Stop();
        _slimeInfo._slimeID = 0;
        _slimeAnimator.SetInteger("ID", 0);
        _transformed = false;
        MainController.Instance._tutorialAssets._tutorialDeployed = false;
        StartVoids();
    }


    public void SpecialRestartLevel()
    {
   
        var Main = MainController.Instance;
        var _realID = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];

        MainController.Instance._restartBeam.Play("RestartBeam");
        _slimeObject.GetComponent<Animator>().Play("SlimeLeavesAnimation");
        StopMoveCoroutine();
        for (int i = 0; i < _realID._elements.Length; i++)
        {
            _realID._elements[i]._changed = false;
        }

        _movementAvailable = false;
        for (int i = 0; i < _realID._hazards.Length; i++)
        {
            _realID._hazards[i]._finished = false;
        }
        Debug.Log(_realID._spawnPoint);
        for (int i = 0; i < _allGrounds.Count; i++)
        {
            _allGrounds[i].GetComponent<StageGroundScript>()._lockedBool = false;
            _allGrounds[i].GetComponent<StageGroundScript>()._lockImage.color = _lockedColors[0];
        }
        for (int i = 0; i < _allElements.Count; i++)
        {
            Destroy(_allElements[i]);
        }
        _allElements.Clear();
        for (int i = 0; i < _allHazards.Count; i++)
        {
            Destroy(_allHazards[i]);
        }
        Destroy(_exitEntranceObjects[0]);
        Destroy(_exitEntranceObjects[1]);
        _exitEntranceObjects.Clear();
        for (int i = 0; i < _allAtoms.Count; i++)
        {
            Destroy(_allAtoms[i]);
        }
        _allAtoms.Clear();
        for (int i = 0; i < _allAttacks.Count; i++)
        {
            Destroy(_allAttacks[i]);
        }
        _allAttacks.Clear();



        _allCountAttacks.Clear();
        _restarted = true;
        _allHazards.Clear();
        _elementsBool.Clear();
        _slimeInfo._elementsParticles[0] = 0;
        _slimeInfo._elementsParticles[1] = 0;
        _slimeInfo._elementsParticles[2] = 0;
        _slimeInfo._elementsParticles[3] = 0;
        _elementsID.Clear();
        MainController.Instance._saveLoadValues._totalAtoms -= _atomsObtained;
        _waterWalk.Stop();
        _smoke.Stop();
        _slimeInfo._slimeID = 0;
        _slimeAnimator.SetInteger("ID", 0);
        _transformed = false;
        MainController.Instance._tutorialAssets._tutorialDeployed = false;
        StartVoids();
    }

    public IEnumerator NexttLevel()
    {
      
        var Main = MainController.Instance;
        var _stageId = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];
        for (int i = 0; i < _stageId._elements.Length; i++)
        {
            _stageId._elements[i]._changed = false;
        }
        StopMoveCoroutine();
        _movementAvailable = false;
        MainController.Instance._tutorialAssets._tutorialDeployed = false;
        for (int i = 0; i < _stageId._hazards.Length; i++)
        {
            _stageId._hazards[i]._finished = false;
        }
     
  
        _elementsBool.Clear();
        for (int i = 0; i < _allGrounds.Count; i++)
        {
            Destroy(_allGrounds[i]);
        }
 
        _allGrounds.Clear();
        //yield return new WaitForSeconds(100);
        for (int i = 0; i < _allElements.Count; i++)
        {
            Destroy(_allElements[i]);
        }
        
        _allElements.Clear();

        for (int i = 0; i < _allHazards.Count; i++)
        {
            Destroy(_allHazards[i]);
        }
      
        _allHazards.Clear();

       
        for(int i = 0; i < _allAtoms.Count; i++)
        {
            Destroy(_allAtoms[i].gameObject);
        }
        _allAtoms.Clear();
        

        _slimeInfo._elementsParticles[0] = 0;
        _slimeInfo._elementsParticles[1] = 0;
        _slimeInfo._elementsParticles[2] = 0;
        _slimeInfo._elementsParticles[3] = 0;

        for (int i = 0; i < _exitEntranceObjects.Count; i++)
        {
            Destroy(_exitEntranceObjects[i]);
        }
        _exitEntranceObjects.Clear();
        for (int i = 0; i < _allAttacks.Count; i++)
        {
            Destroy(_allAttacks[i]);
        }
        _allAttacks.Clear();
        _allCountAttacks.Clear();
        _restarted = false;

        if(Main._onWorldGlobal == 0 && _idStage == 1)
        {
            ComicController.Instance._imagesID.Add(3);
            ComicController.Instance._imagesID.Add(4);
            ComicController.Instance._imagesID.Add(5);
            ComicController.Instance._imagesID.Add(6);
            ComicController.Instance._waitSeconds = 2;
            ComicController.Instance._comicOn = true;
            StartCoroutine(ComicController.Instance.ComicStripOn());
            while (ComicController.Instance._comicOn)
            {
                yield return null;
            }
            MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
            yield return new WaitForSeconds(0.5f);
            ComicController.Instance._continueParent.gameObject.SetActive(false);
            ComicController.Instance._comicAnimator.SetBool("ComicOn", false);
            earthquakeOn = true;
            StartEarthquake();

        }

        if (Main._onWorldGlobal == 1 && _idStage == 5)
        {
            ComicController.Instance._imagesID.Add(12);
            ComicController.Instance._imagesID.Add(13);
 
            ComicController.Instance._waitSeconds = 3;
            ComicController.Instance._comicOn = true;
            StartCoroutine(ComicController.Instance.ComicStripOn());
            while (ComicController.Instance._comicOn)
            {
                yield return null;
            }
            MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
            yield return new WaitForSeconds(0.5f);
            ComicController.Instance._continueParent.gameObject.SetActive(false);
            ComicController.Instance._comicAnimator.SetBool("ComicOn", false);
            earthquakeOn = true;
            StartEarthquake();

        }

        if (Main._onWorldGlobal == 3 && _idStage == 1)
        {
            ComicController.Instance._imagesID.Add(17);
            ComicController.Instance._imagesID.Add(18);
            ComicController.Instance._imagesID.Add(19);
            ComicController.Instance._waitSeconds = 3;
            ComicController.Instance._comicOn = true;
            StartCoroutine(ComicController.Instance.ComicStripOn());
            while (ComicController.Instance._comicOn)
            {
                yield return null;
            }
            MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
            yield return new WaitForSeconds(0.5f);
            ComicController.Instance._continueParent.gameObject.SetActive(false);
            ComicController.Instance._comicAnimator.SetBool("ComicOn", false);
            earthquakeOn = true;
            StartEarthquake();

        }


        if (Main._onWorldGlobal == 4 && _idStage == 1)
        {
            ComicController.Instance._imagesID.Add(23);
            ComicController.Instance._imagesID.Add(24);

            ComicController.Instance._waitSeconds = 3;
            ComicController.Instance._comicOn = true;
            StartCoroutine(ComicController.Instance.ComicStripOn());
            while (ComicController.Instance._comicOn)
            {
                yield return null;
            }
            MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
            yield return new WaitForSeconds(0.5f);
            ComicController.Instance._continueParent.gameObject.SetActive(false);
            ComicController.Instance._comicAnimator.SetBool("ComicOn", false);
            earthquakeOn = true;
            StartEarthquake();

        }


        // NUEVO spawn
        _idStage++;
        _onPose = _stageId._spawnPoint;

        // 🔑 POSICIÓN CORRECTA
        _slimeObject.GetComponent<RectTransform>().position =
            _allPositions[_onPose].GetComponent<RectTransform>().position;
        _elementsID.Clear();
        yield return new WaitForSeconds(0.2f);
        StartVoids();          // recrea stage
        CalculateMoves();      // recalcula movimientos
        _slimeInfo._slimeID = 0;
        _slimeAnimator.SetInteger("ID", 0);
        _waterWalk.Stop();
        _smoke.Stop();
        yield return new WaitForSeconds(0.2f);
        switch (_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._wind)
        {
            case NewGameEvent.Wind.Center:
                break;
            case NewGameEvent.Wind.Left:
                _lowAirLeft.Play();
                break;
            case NewGameEvent.Wind.Right:
                _lowAirRight.Play();
                break;
        }
        _transformed = false;
        MainController.Instance.SaveProgress();
        yield return new WaitForSeconds(0.5f);

        MainController.Instance._bordersAnimator.SetBool("BorderOut", true);
        MainController.Instance._cinematicBorders.SetBool("FadeIn", false);
    }


    public void HazardDetection()
    {
        var Main = MainController.Instance;
        var HazardInfo = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];
        for (int i = 0; i < HazardInfo._hazards.Length; i++)
        {
            if(_onPose == HazardInfo._hazards[i]._onPlace)
            {
                switch (HazardInfo._hazards[i]._hazards)
            {
                case NewGameEvent.Hazards.HazardsType.Fire:
                        switch (_slimeInfo._slimeID)
                        {
                            case 0:
                                StartCoroutine(RestartLevel());
                                break;
                            case 1:
                                StartCoroutine(RestartLevel());
                                break;
                            case 2:
                                _allHazards[i].GetComponent<ObstaclesScript>()._fireParticle.Stop();
                                _allHazards[i].GetComponent<ObstaclesScript>()._smokeParticle.Play();
                                Main._scriptSFX.PlaySound(Main._scriptSFX._melting);
                                break;
                            case 3:
                                StartCoroutine(RestartLevel());
                                break;
                            case 4:
                                _allHazards[i].GetComponent<ObstaclesScript>()._fireParticle.Stop();
                                _allHazards[i].GetComponent<ObstaclesScript>()._smokeParticle.Play();
                                Main._scriptSFX.PlaySound(Main._scriptSFX._melting);
                                itTransforms = true;
                                _slimeInfo._slimeID = 2;

                                Main._elementsCircles[0]._cirlce.color = Main._elementsColor[1];
                                Main._elementsCircles[0]._elementLetters.color = Main._elementsColor[1];
                                Main._elementsCircles[0]._elementLetters.text = "H";
                                Main._elementsCircles[0]._quantity.text = "2";

                                Main._elementsCircles[1]._cirlce.color = Main._elementsColor[2];
                                Main._elementsCircles[1]._elementLetters.color = Main._elementsColor[2];
                                Main._elementsCircles[1]._elementLetters.text = "0";
                                Main._elementsCircles[1]._quantity.text = "";

                                Main._dataTexts[0].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("water1");
                                Main._dataTexts[1].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("water2");

                                MainController.Instance._atributeText.text = GameInitScript.Instance.GetText("H20extra");
                                MainController.Instance._nameText.text = GameInitScript.Instance.GetText("H20");

                                _waterWalk.Play();
                                _smoke.Stop();
                                StartCoroutine(TransormatioNumerator());

                                break;
                        }  
                        Debug.Log("Fire");
               
                        break;
                case NewGameEvent.Hazards.HazardsType.Hole:
                        if (_slimeInfo._slimeID != 3)
                        {
                            StartCoroutine(RestartLevel());
                        }                 
                            
                            Debug.Log("Hole");
                 
                        break;
                 case NewGameEvent.Hazards.HazardsType.Switch:
                        if (_slimeInfo._slimeID == 1 || _slimeInfo._slimeID == 4)
                        {
                            _allHazards[i].GetComponent<ObstaclesScript>().LevelPressed();
                            for(int y = 0; y < _allHazards.Count; y++)
                            {
                                if(_allHazards[y].GetComponent<ObstaclesScript>()._id == 3)
                                {
                                    _allHazards[y].GetComponent<ObstaclesScript>()._allObstacles[4].GetComponent<Animator>().SetTrigger("Column");
                                    for(int z = 0; z < HazardInfo._hazards.Length; z++)
                                    {
                                        HazardInfo._hazards[z]._finished = true;
                                    }
                         
                    


                                }
                            }
                        }                       
                       
                        Debug.Log("Switch");
                        break;
                 case NewGameEvent.Hazards.HazardsType.Column:
                     break;
                    case NewGameEvent.Hazards.HazardsType.MagnetoPlace:
                        if (_slimeInfo._slimeID == 5)
                        {                
                            StartCoroutine(RockNumerator());
                        }
                        break;
                    case NewGameEvent.Hazards.HazardsType.CenterElectricity:
                        switch (_slimeInfo._slimeID)
                        {
                            case 1:
                            case 2:
                                StartCoroutine(ElectroNumerator());
                                break;
                            default:
                              
                                break;
                        }

                        break;
                    case NewGameEvent.Hazards.HazardsType.Electricity:
                        switch (_slimeInfo._slimeID)
                        {
                            case 1:
                            case 2:
                                break;
                            default:
                                StartCoroutine(RestartLevel());
                                break;
                        }
                 
                        break;

                }
            }
        }
    }

    public IEnumerator RockNumerator()
    {
        _movementAvailable = false;
        var Main = MainController.Instance;
        var HazardInfo = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];
        yield return new WaitForSeconds(1);
        for (int i = 0; i < HazardInfo._hazards.Length; i++)
        {
            if (_allHazards[i].GetComponent<ObstaclesScript>()._id == 4)
            {
                _boulderSwitch._boulder = _allHazards[i].gameObject;
                break;
            }
        }
        yield return new WaitForSeconds(0.25f);
       _boulderSwitch._columnObject.GetComponent<ObstaclesScript>()._allObstacles[4].GetComponent<Animator>().SetTrigger("Column");
        for (int z = 0; z < HazardInfo._hazards.Length; z++){
            if(_allHazards[z].GetComponent<ObstaclesScript>()._id == 3)
            {
                HazardInfo._hazards[z]._finished = true;
            }      
        }
        _boulderSwitch._movingBoulder = true;
        yield return new WaitForSeconds(1f);
        _movementAvailable = true;
    }
    public IEnumerator ElectroNumerator()
    {
        _movementAvailable = false;
        var Main = MainController.Instance;
        var HazardInfo = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];
        yield return new WaitForSeconds(1);

        _boulderSwitch._columnObject.GetComponent<ObstaclesScript>()._allObstacles[4].GetComponent<Animator>().SetTrigger("Column");
        for (int z = 0; z < HazardInfo._hazards.Length; z++)
        {
            if (_allHazards[z].GetComponent<ObstaclesScript>()._id == 3)
            {
                HazardInfo._hazards[z]._finished = true;
            }
            if (_allHazards[z].GetComponent<ObstaclesScript>()._id == 7)
            {
                HazardInfo._hazards[z]._finished = true;
                _electroAnimator.Play("ElectroCharge");
                _mainUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 2);
                _allHazards[z].GetComponent<ObstaclesScript>()._electricityCenterParticle.Play();
            }
        }
 
        yield return new WaitForSeconds(1f);
        _movementAvailable = true;
    }

    public void ResetAllHazards()
    {
        for (int i = 0; i < _allStages.Length; i++)
        {
            for(int y = 0; y < _allStages[i]._elements.Length; y++)
            {
                _allStages[i]._elements[y]._changed = false;
            }
        }
    }
    public IEnumerator ElementDetection()
    {
        var Main = MainController.Instance;
        var ElementInfo = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];
        Main._elementAnimatorAssets._center.gameObject.SetActive(false);
        for (int i = 0; i < ElementInfo._elements.Length; i++)
        {

       
                if (!ElementInfo._elements[i]._changed)
                {
                    //if (_onPose == ElementInfo._elements[i]._onPlace && _elementsBool[i])
                    if (_onPose == ElementInfo._elements[i]._onPlace)
                    {
                    if (!Main._saveLoadValues._elementTutorial)
                    {
                        StopMoveCoroutine();
                        _movementAvailable = false;
                        var gi = GameInitScript.Instance;
                        Main._tutorialAssets._tutorialText.text = gi.GetText("tutorial2");
                        yield return new WaitForSeconds(1);
                        Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                        Main._tutorialAssets._arrowsParent.SetActive(false);
                        Main._tutorialAssets._elementsParent.SetActive(true);
                        Main._tutorialAssets._atomParent.SetActive(false);
                        Main._tutorialAssets._stepParent.SetActive(false);
                        yield return new WaitForSeconds(1);
                        Main._tutorialAssets._continueText.gameObject.SetActive(true);
                        yield return new WaitForSeconds(0.25f);
                        yield return new WaitForSeconds(0.25f);
                        while (!Input.GetButtonDown("Submit"))
                        {
                            yield return null;
                        }
                        SFXscript.Instance.PlaySound(SFXscript.Instance._whip);
                        Main._tutorialAssets._continueText.gameObject.SetActive(false);
                        Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
                        Main._saveLoadValues._elementTutorial = true;
                    }

                    if (ElementInfo._elements[i]._emptyAtom)
                    {
                        if (!Main._saveLoadValues._atomTutorial)
                        {
                            StopMoveCoroutine();
                            _movementAvailable = false;
                            var gi = GameInitScript.Instance;
                            Main._tutorialAssets._tutorialText.text = gi.GetText("tutorialatom");
                            yield return new WaitForSeconds(1);
                            Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                            Main._tutorialAssets._arrowsParent.SetActive(false);
                            Main._tutorialAssets._elementsParent.SetActive(false);
                            Main._tutorialAssets._atomParent.SetActive(true);
                            Main._tutorialAssets._stepParent.SetActive(false);
                            yield return new WaitForSeconds(1);
                            Main._tutorialAssets._continueText.gameObject.SetActive(true);
                            yield return new WaitForSeconds(0.25f);
                            yield return new WaitForSeconds(0.25f);
                            while (!Input.GetButtonDown("Submit"))
                            {
                                yield return null;
                            }
                            _fusionParent.gameObject.SetActive(true);
                            SFXscript.Instance.PlaySound(SFXscript.Instance._whip);
                            Main._tutorialAssets._continueText.gameObject.SetActive(false);
                            Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
                            Main._saveLoadValues._atomTutorial = true;
                            Main._saveLoadValues._pauseAvailable = true;
                        }

                        _movementAvailable = true;
                        Main._elementAnimatorAssets._border.color = Color.white;
                        Main._elementAnimatorAssets._elementText.color = Color.white;
                        Main._elementAnimatorAssets._elementText.text = "";
                        Main._elementAnimatorAssets._elementName.text = "";
                        Main._elementAnimatorAssets._quantityText.text = ElementInfo._elements[i]._quantity.ToString();
                        _atomsObtained += ElementInfo._elements[i]._quantity;
                        Main._elementAnimatorAssets._center.gameObject.SetActive(true);
                    }
                    else
                    {
                        switch (ElementInfo._elements[i]._elementType)
                        {
                            case NewGameEvent.Elements.ElementType.C:
                                Main._elementAnimatorAssets._border.color = Main._elementsColor[0];
                                Main._elementAnimatorAssets._elementText.color = Main._elementsColor[0];
                                Main._elementAnimatorAssets._elementText.text = "C";
                                Main._elementAnimatorAssets._quantityText.text = ElementInfo._elements[i]._quantity.ToString();
                                _slimeInfo._elementsParticles[0] += ElementInfo._elements[i]._quantity;
                                Main._elementAnimatorAssets._elementName.text = GameInitScript.Instance.GetText("element1");
                                LOLSDK.Instance.SpeakText(GameInitScript.Instance.GetText("element1"));
                           
                                break;
                            case NewGameEvent.Elements.ElementType.H:
                                Main._elementAnimatorAssets._border.color = Main._elementsColor[1];
                                Main._elementAnimatorAssets._elementText.color = Main._elementsColor[1];
                                Main._elementAnimatorAssets._elementText.text = "H";
                                Main._elementAnimatorAssets._quantityText.text = ElementInfo._elements[i]._quantity.ToString();
                                _slimeInfo._elementsParticles[1] += ElementInfo._elements[i]._quantity;
                                Main._elementAnimatorAssets._elementName.text = GameInitScript.Instance.GetText("element2");
                                LOLSDK.Instance.SpeakText(GameInitScript.Instance.GetText("element2"));
                                break;
                            case NewGameEvent.Elements.ElementType.O:
                                Main._elementAnimatorAssets._border.color = Main._elementsColor[2];
                                Main._elementAnimatorAssets._elementText.color = Main._elementsColor[2];
                                Main._elementAnimatorAssets._elementText.text = "O";
                                Main._elementAnimatorAssets._quantityText.text = ElementInfo._elements[i]._quantity.ToString();
                                _slimeInfo._elementsParticles[2] += ElementInfo._elements[i]._quantity;
                                Main._elementAnimatorAssets._elementName.text = GameInitScript.Instance.GetText("element3");
                                LOLSDK.Instance.SpeakText(GameInitScript.Instance.GetText("element3"));
                                break;
                            case NewGameEvent.Elements.ElementType.Fe:
                                Main._elementAnimatorAssets._border.color = Main._elementsColor[3];
                                Main._elementAnimatorAssets._elementText.color = Main._elementsColor[3];
                                Main._elementAnimatorAssets._elementText.text = "Fe";
                                Main._elementAnimatorAssets._quantityText.text = ElementInfo._elements[i]._quantity.ToString();
                                _slimeInfo._elementsParticles[3] += ElementInfo._elements[i]._quantity;
                                Main._elementAnimatorAssets._elementName.text = GameInitScript.Instance.GetText("element4");
                                LOLSDK.Instance.SpeakText(GameInitScript.Instance.GetText("element4"));
                                break;
                        }
                 
                    }
                    SFXscript.Instance.PlaySound(SFXscript.Instance._slimeRelease);

                    ElementInfo._elements[i]._changed = true;
                    StartCoroutine(ElementNumerator());
                    if (!_transformed){
          
                        TransformSlimeVoid();
                    }
                 
             
        
                   
                    if (!itTransforms)
                    {
                    
                        _movementAvailable = true;
                    }
               





                    break;
                    }             
            }
 
   

        }    

    }
    public IEnumerator ElementNumerator()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < _allElements.Count; i++)
        {
            if (_allElements[i].GetComponent<ElementOrbScript>()._onPose == _onPose)
            {
                Destroy(_allElements[i].gameObject);
                _allElements.RemoveAt(i);
            }

        }
    }

    public IEnumerator AtomDetection()
    {
        var Main = MainController.Instance;
        for (int i = 0; i < _allAtoms.Count; i++)
        {
            if (_onPose == _allAtoms[i].GetComponent<AtomScript>()._onPose)
            {
         

                if (MainController.Instance._onWorldGlobal == 0 && Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage] == 5)
                {
                    StopMoveCoroutine();
                    var gi = GameInitScript.Instance;
                    Main._tutorialAssets._tutorialText.text = gi.GetText("tutorial4");
                    _movementAvailable = false;
                    yield return new WaitForSeconds(1);
                    Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                    Main._tutorialAssets._arrowsParent.SetActive(false);
                    Main._tutorialAssets._elementsParent.SetActive(false);
                    Main._tutorialAssets._atomParent.SetActive(true);
                    Main._tutorialAssets._stepParent.SetActive(false);
                    yield return new WaitForSeconds(1);
                    Main._tutorialAssets._continueText.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.25f);
                    MainController.Instance._saveLoadValues._totalAtoms++;
                    Debug.Log("Atomo en: " + _allAtoms[i].GetComponent<AtomScript>()._onPose.ToString());
                    Destroy(_allAtoms[i]);
                    _allAtoms.RemoveAt(i);
                    //_atomList.RemoveAt(i);
                    _atomsObtained++;
                    while (!Input.GetButtonDown("Pause"))
                    {
                        yield return null;
                    }
                    Main._tutorialAssets._continueText.gameObject.SetActive(false);
                    Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
                    _movementAvailable = true;
                    MainController.Instance._AtomAnimator.SetBool("AtomsIn", true);
                    MainController.Instance._saveLoadValues._pauseAvailable = true;
                    _AtomsPanelOn = true;


                }
                else
                {
                  
                    MainController.Instance._saveLoadValues._totalAtoms += _allAtoms[i].GetComponent<AtomScript>()._quantity;
                    Debug.Log("Atomo en: " + _allAtoms[i].GetComponent<AtomScript>()._onPose.ToString());
                    Destroy(_allAtoms[i]);
                    _allAtoms.RemoveAt(i);
                    //_atomList.RemoveAt(i);
                    _atomsObtained++;
                }
            }



        }

    }

    public IEnumerator StepDetection()
    {
        var Main = MainController.Instance;
        for (int i = 0; i < _stepsList.Count; i++)
        {
            if (_onPose == _stepsList[i])
            {
                if(MainController.Instance._onWorldGlobal == 0 && MainController.Instance._saveLoadValues._totalSteps <= 0)
                {
                    StopMoveCoroutine();
                    _movementAvailable = false;
                    var gi = GameInitScript.Instance;
                    Main._tutorialAssets._tutorialText.text = gi.GetText("tutorial3");
                    yield return new WaitForSeconds(1);
                    Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                    Main._tutorialAssets._arrowsParent.SetActive(false);
                    Main._tutorialAssets._elementsParent.SetActive(false);
                    Main._tutorialAssets._atomParent.SetActive(false);
                    Main._tutorialAssets._stepParent.SetActive(true);
                    yield return new WaitForSeconds(1);
                    Main._tutorialAssets._continueText.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.25f);
                              MainController.Instance._saveLoadValues._totalSteps++;
                Debug.Log("Steps en: " + _stepsList[i].ToString());
                Destroy(_allSteps[i]);
                _allSteps.RemoveAt(i);
                _stepsList.RemoveAt(i);    
                    while (!Input.GetButtonDown("Submit"))
                    {
                        yield return null;
                    }
                    Main._scriptSFX.PlaySound(Main._scriptSFX._stickyMudSound);
                    Main._tutorialAssets._continueText.gameObject.SetActive(false);
                    Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
                    _movementAvailable = true;
                }
                else
                {
                    MainController.Instance._saveLoadValues._totalSteps++;
                    Debug.Log("Steps en: " + _stepsList[i].ToString());
                    Destroy(_allSteps[i]);
                    _allSteps.RemoveAt(i);
                    _stepsList.RemoveAt(i);
                    Main._scriptSFX.PlaySound(Main._scriptSFX._stickyMudSound);
                }
               
            }



        }

    }


    public void TransformSlimeVoid()
    {
        var Main = MainController.Instance;
    
        if (_slimeInfo._elementsParticles[0] >= 1 && _slimeInfo._elementsParticles[3] >= 1)
        {
            itTransforms = true;
            _slimeInfo._slimeID = 1;      

            Main._elementsCircles[0]._cirlce.color = Main._elementsColor[0];
            Main._elementsCircles[0]._elementLetters.color = Main._elementsColor[0];
            Main._elementsCircles[0]._elementLetters.text = "C";
            Main._elementsCircles[0]._quantity.text = "";

            Main._elementsCircles[1]._cirlce.color = Main._elementsColor[3];
            Main._elementsCircles[1]._elementLetters.color = Main._elementsColor[3];
            Main._elementsCircles[1]._elementLetters.text = "Fe";
            Main._elementsCircles[1]._quantity.text = "";

            Main._dataTexts[0].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("iron1");
            Main._dataTexts[1].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("iron2");

            MainController.Instance._nameText.text = GameInitScript.Instance.GetText("FC");
            MainController.Instance._atributeText.text = GameInitScript.Instance.GetText("FCextra");
            StartCoroutine(TransormatioNumerator());
        }
        else if (_slimeInfo._elementsParticles[1] >= 2 && _slimeInfo._elementsParticles[2] >= 1)
        {
            switch (MainController.Instance._onWorldGlobal)
            {
                case 2:
                    itTransforms = true;
                    _slimeInfo._slimeID = 4;

                    Main._elementsCircles[0]._cirlce.color = Main._elementsColor[1];
                    Main._elementsCircles[0]._elementLetters.text = "H";
                    Main._elementsCircles[0]._elementLetters.color = Main._elementsColor[1];
                    Main._elementsCircles[0]._quantity.text = "2";

                    Main._elementsCircles[1]._cirlce.color = Main._elementsColor[2];
                    Main._elementsCircles[1]._elementLetters.text = "O";
                    Main._elementsCircles[1]._elementLetters.color = Main._elementsColor[2];
                    Main._elementsCircles[1]._quantity.text = "";

                    Main._dataTexts[0].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("snow1");
                    Main._dataTexts[1].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("snow2");

                    MainController.Instance._atributeText.text = GameInitScript.Instance.GetText("ICEextra");
                    MainController.Instance._nameText.text = GameInitScript.Instance.GetText("ICE");
               
                    _waterWalk.Play();
                    _smoke.Stop();
                    StartCoroutine(TransormatioNumerator());
                    _turnsReturnToWater = -1;
                    break;
                default:
                    itTransforms = true;
                    _slimeInfo._slimeID = 2;

                    Main._elementsCircles[0]._cirlce.color = Main._elementsColor[1];
                    Main._elementsCircles[0]._elementLetters.color = Main._elementsColor[1];
                    Main._elementsCircles[0]._elementLetters.text = "H";
                    Main._elementsCircles[0]._quantity.text = "2";

                    Main._elementsCircles[1]._cirlce.color = Main._elementsColor[2];
                    Main._elementsCircles[1]._elementLetters.color = Main._elementsColor[2];
                    Main._elementsCircles[1]._elementLetters.text = "0";
                    Main._elementsCircles[1]._quantity.text = "";

                    Main._dataTexts[0].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("water1");
                    Main._dataTexts[1].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("water2");

                    MainController.Instance._atributeText.text = GameInitScript.Instance.GetText("H20extra");
                    MainController.Instance._nameText.text = GameInitScript.Instance.GetText("H20");
            
                    _waterWalk.Play();
                    _smoke.Stop();
                    StartCoroutine(TransormatioNumerator());
                    break;
            }

        }
        else if (_slimeInfo._elementsParticles[0] >= 1 && _slimeInfo._elementsParticles[2] >= 2)
        {
            itTransforms = true;
            _slimeInfo._slimeID = 3;

            Main._elementsCircles[0]._cirlce.color = Main._elementsColor[0];
            Main._elementsCircles[0]._elementLetters.color = Main._elementsColor[0];
            Main._elementsCircles[0]._elementLetters.text = "C";
            Main._elementsCircles[0]._quantity.text = "";

            Main._elementsCircles[1]._cirlce.color = Main._elementsColor[2];
            Main._elementsCircles[1]._elementLetters.color = Main._elementsColor[2];
            Main._elementsCircles[1]._elementLetters.text = "0";
            Main._elementsCircles[1]._quantity.text = "2";

            Main._dataTexts[0].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("dioxide1");
            Main._dataTexts[1].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("dioxide2");

            MainController.Instance._atributeText.text = GameInitScript.Instance.GetText("CO2extra");
            _smoke.Play();
            _waterWalk.Stop();
            MainController.Instance._nameText.text = GameInitScript.Instance.GetText("CO2");
            StartCoroutine(TransormatioNumerator());
        }
        else if (_slimeInfo._elementsParticles[2] >= 4 && _slimeInfo._elementsParticles[3] >= 3) {
            itTransforms = true;
            _slimeInfo._slimeID = 5;

            Main._elementsCircles[1]._cirlce.color = Main._elementsColor[0];
            Main._elementsCircles[1]._elementLetters.color = Main._elementsColor[0];
            Main._elementsCircles[1]._elementLetters.text = "C";
            Main._elementsCircles[1]._quantity.text = "4";

            Main._elementsCircles[0]._cirlce.color = Main._elementsColor[3];
            Main._elementsCircles[0]._elementLetters.color = Main._elementsColor[3];
            Main._elementsCircles[0]._elementLetters.text = "Fe";
            Main._elementsCircles[0]._quantity.text = "3";

            Main._dataTexts[0].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("magnatite1");
            Main._dataTexts[1].text = MainController.Instance._nameText.text = GameInitScript.Instance.GetText("magnatite2");

            MainController.Instance._atributeText.text = GameInitScript.Instance.GetText("FE3O4extra");

            Debug.Log("Fe3O4");
            _smoke.Play();
            _waterWalk.Stop();
            MainController.Instance._nameText.text = GameInitScript.Instance.GetText("FE3O4");
            StartCoroutine(TransormatioNumerator());
        }
        else
        {       
            StartCoroutine(ElementAnimatorCourutine());
        }


    }

    public IEnumerator ElementAnimatorCourutine()
    {

        MainController.Instance._elementAnimatorAssets._animator.Play("ElementIn");
        yield return new WaitForSeconds(0.3f);
        _mainUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 4);
    }

    public IEnumerator TransormatioNumerator()
    {
   
        var Main = MainController.Instance;
        SFXscript.Instance.PlaySound(SFXscript.Instance._slimeCharge);
        MainController.Instance._AtomAnimator.SetBool("AtomsIn", false);
        _AtomsPanelOn = false;
        _transformed = true;
        StopMoveCoroutine();
        _movementAvailable = false;
        MainController.Instance._transformationAnimator.SetBool("Success", true);
   
        yield return new WaitForSeconds(0.5f);
        _mainUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 4);
        switch (_slimeInfo._slimeID)
        {
            case 0:
                break;
            case 1:
                MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._iron);
                _slimeAnimator.SetInteger("ID", 3);
                break;
            case 2:
                MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._melting);
                _slimeAnimator.SetInteger("ID", 1);
                break;
            case 3:
                MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._co2);
                _slimeAnimator.SetInteger("ID", 2);
                break;
            case 4:
                MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._frozen);
                _slimeAnimator.SetInteger("ID", 5);
                break;
            case 5:
                MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._magnetism);
                _slimeAnimator.SetInteger("ID", 4);
         
                break;
        }
        yield return new WaitForSeconds(1);
        MainController.Instance._continueText.gameObject.SetActive(true);
        while (!Input.GetButtonDown("Submit"))
        {
            yield return null;
        }
        SFXscript.Instance.PlaySound(SFXscript.Instance._whip);
        MainController.Instance._continueText.gameObject.SetActive(false);
        MainController.Instance._transformationAnimator.SetBool("Success", false);
        yield return new WaitForSeconds(0.5f);
        if (!MainController.Instance._saveLoadValues._hazardTutorial)
        {

            var gi = GameInitScript.Instance;

            Main._tutorialAssets._tutorialText.text = gi.GetText("tutorial6");  
            Main._tutorialAssets._arrowsParent.SetActive(false);
            Main._tutorialAssets._elementsParent.SetActive(false);
            Main._tutorialAssets._atomParent.SetActive(false);
            Main._tutorialAssets._stepParent.SetActive(false);
            Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
            yield return new WaitForSeconds(1);
            Main._tutorialAssets._continueText.gameObject.SetActive(true);
            while (!Input.GetButtonDown("Submit"))
            {
                yield return null;
            }
            SFXscript.Instance.PlaySound(SFXscript.Instance._whip);
            Main._tutorialAssets._continueText.gameObject.SetActive(false);
            Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
            MainController.Instance._saveLoadValues._hazardTutorial = true;
            yield return new WaitForSeconds(0.5f);
        }

        if (!MainController.Instance._saveLoadValues._hazardTutorial)
        {

            var gi = GameInitScript.Instance;
   
            Main._tutorialAssets._tutorialText.text = gi.GetText("tutorial6");
            Main._tutorialAssets._arrowsParent.SetActive(false);
            Main._tutorialAssets._elementsParent.SetActive(false);
            Main._tutorialAssets._atomParent.SetActive(false);
            Main._tutorialAssets._stepParent.SetActive(false);
            Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
            yield return new WaitForSeconds(1);
            Main._tutorialAssets._continueText.gameObject.SetActive(true);
            while (!Input.GetButtonDown("Submit"))
            {
                yield return null;
            }
            SFXscript.Instance.PlaySound(SFXscript.Instance._whip);
            Main._tutorialAssets._continueText.gameObject.SetActive(false);
            Main._tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
            MainController.Instance._saveLoadValues._hazardTutorial = true;
            yield return new WaitForSeconds(0.5f);
        }
    



        _movementAvailable = true;
    }

    public void ExitDetection()
    {
        var Main = MainController.Instance;
        var ExitID= _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._exitPoint;
        if (_onPose == ExitID)
        {

            StartCoroutine(ExitNumerator());            
        }
    }

    public IEnumerator ExitNumerator()
    {
        var Main = MainController.Instance;
        _gameStarts = false;

   
        StopMoveCoroutine();
        _movementAvailable = false;

        yield return new WaitForSeconds(0.5f);
        _slimeObject.GetComponent<Animator>().Play("SlimeLeavesAnimation");
        switch (_idStage == Main._allTurnsInfo[Main._onWorldGlobal]._stagesID.Count - 1)
        {
            case false:
                SFXscript.Instance.PlaySound(SFXscript.Instance._jump);
                SFXscript.Instance.PlaySound(SFXscript.Instance._whip);
                break;
            case true:
                SFXscript.Instance.PlaySound(SFXscript.Instance._slimeCharge);

                break;
        }
        _exitEntranceObjects[1].GetComponent<ExitScriptObject>()._exitParticle.Play();
        yield return new WaitForSeconds(0.25f);
        _mainUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 2);
        yield return new WaitForSeconds(0.25f);
        for(int i = 0; i < _allSteps.Count; i++)
        {
            Destroy(_allSteps[i].gameObject);
        }
        _allSteps.Clear();
        _elementsBool.Clear();
        MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
        MainController.Instance._cinematicBorders.SetBool("FadeIn", true);

        yield return new WaitForSeconds(1);
        switch (_idStage == Main._allTurnsInfo[Main._onWorldGlobal]._stagesID.Count - 1)
        {
            case false:
        
                StartCoroutine(NexttLevel());
                break;
            case true:
       
                NextWorld();
                break;
        }

    }

    public void NextWorld()
    {
        StartCoroutine(NextWorldNumerator());
    }

    public IEnumerator NextWorldNumerator()
    {
        SFXscript.Instance._fireSetVolume = 0f;
        SFXscript.Instance._strongWindSetVolume = 0f;
        switch (MainController.Instance._onWorldGlobal)
        {
            case 0:
                MainController.Instance._cinematicBorders.SetBool("FadeIn", true);
                MainController.Instance._onWorldGlobal = 1;
                MainController.Instance._saveLoadValues._worldsUnlocked[1] = true;
                break;
            case 1:
                MainController.Instance._onWorldGlobal = 2;
                MainController.Instance._saveLoadValues._worldsUnlocked[2] = true;
                break;
            case 2:
                MainController.Instance._onWorldGlobal = 3;
                MainController.Instance._saveLoadValues._worldsUnlocked[3] = true;
                break;
            case 3:
                MainController.Instance._onWorldGlobal = 4;
                MainController.Instance._saveLoadValues._worldsUnlocked[4] = true;
                break;
            case 4:
                if (MainController.Instance._onWorldGlobal == 4 && _idStage == MainController.Instance._allTurnsInfo[4]._stagesID.Count - 1)
                {
                    ComicController.Instance._imagesID.Add(25);
                    ComicController.Instance._imagesID.Add(26);
                    ComicController.Instance._imagesID.Add(27);
                    ComicController.Instance._waitSeconds = 2;
                    ComicController.Instance._comicOn = true;
                    StartCoroutine(ComicController.Instance.ComicStripOn());
                    while (ComicController.Instance._comicOn)
                    {
                        yield return null;
                    }
                    MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
                    yield return new WaitForSeconds(0.5f);
                    ComicController.Instance._continuar.text = GameInitScript.Instance.GetText("endgame");
                    ComicController.Instance._continueParent.gameObject.SetActive(false);
                    ComicController.Instance._comicAnimator.SetBool("ComicOn", false);
    
                    LOLSDK.Instance.CompleteGame();
                }
           
                break;
        }
        MainController.Instance._saveLoadValues._restartAvailable = true;
        MainController.Instance._introSpecial = true;
        MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
        MainController.Instance.SaveProgress();

        //MainController.Instance._cinematicBorders.SetBool("FadeIn", false);
        _AtomsPanelOn = false;
        MainController.Instance._AtomAnimator.SetBool("AtomsIn", false);
        switch (MainController.Instance._onWorldGlobal)
        {
            case 0:
         
                break;
            case 1:
                MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[0];
                break;
            case 2:
                MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[0];
                break;
            case 3:             
                break;
            case 4:
                MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[0];
                MainController.Instance._onWorldGlobal = 0;
                break;
        }
        MainController.Instance._scriptMusic._audioBGM.Play();
        yield return new WaitForSeconds(1);
        MainController.Instance.LoadSceneByName("IntroScene");

    }

    public void ShakeCamera()
    {
        _mainUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 2);
    }

    public void StartEarthquake()
    {
        _crumbleParticle.Play();
        if (earthquakeRoutine == null)
            earthquakeRoutine = StartCoroutine(EarthquakeCoroutine());
    }

    public void StopEarthquake()
    {
        if (earthquakeRoutine != null)
        {
            StopCoroutine(earthquakeRoutine);
            earthquakeRoutine = null;
        }

        // reset position
        _mainUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    IEnumerator EarthquakeCoroutine()
    {
        RectTransform rt = _mainUI.GetComponent<RectTransform>();

        while (earthquakeOn)
        {
            float x = Random.Range(-2f, 2f);
            //float y = Random.Range(-2f, 2f);

            rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);

            yield return new WaitForSeconds(0.1f); // once per second
        }

        rt.anchoredPosition = Vector2.zero;
    }




}
