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
    public List<int> _atomList = new List<int>();
    public List<int> _stepsList = new List<int>();
    public Color _slimeMainColor;
    public Animator _transformAnimator;
    bool _transformed;

    public TextMeshProUGUI _formulaText;
    public TextMeshProUGUI _nameText;

    public ParticleSystem _hitWalk;
    public ParticleSystem _waterWalk;
    public ParticleSystem _smoke;
    public ParticleSystem _deadSlimeParticle;

    public List<int> _elementsID = new List<int>();
    public List<bool> _elementsBool = new List<bool>();
    [System.Serializable]
    public class TutorialAssets
    {
        public Animator _tutorialAnimator;
        public TextMeshProUGUI _tutorialText;
        public GameObject _arrowsParent;
        public GameObject _elementsParent;
        public GameObject _atomParent;
        public GameObject _stepParent;
    
        public TextMeshProUGUI _continueText;
        public bool _tutorialDeployed;
    }
    public TutorialAssets _tutorialAssets;
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
    }



    public void StartVoids()
    {
        if (!_restarted)
        {
            StageCreationVoid();
            SetSteps();
        }
        SetElements();
        SetAtoms();

        var Main = MainController.Instance;
        //if (_allStages[MainController.Instance._allStagesData[MainController.Instance._onWorldGlobal]._stageList[_idStage]]._bossAssets.Length > 0)
        if (_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._bossAssets.Length > 0)
            {
            SetBossAttacks();
        }

        SetHazards();
        SetEntranceExit();
 
        _atomsObtained = 0;

        _onPose = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._spawnPoint;

        // 🔑 POSICIÓN CORRECTA
        _slimeObject.GetComponent<RectTransform>().position =
            _allPositions[_onPose].GetComponent<RectTransform>().position;

        _slimeObject.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 180f);


        CalculateMoves();
        StartCoroutine(StartGameNumerator());
    }

    public IEnumerator StartGameNumerator()
    {
        var Main = MainController.Instance;
        _tutorialAssets._continueText.gameObject.SetActive(false);

        //_slimeObject.GetComponent<RectTransform>().localScale = Vector3.zero;
        if (!_tutorialAssets._tutorialDeployed)
        {
            switch (Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage])
            {
                case 0:
                    yield return new WaitForSeconds(1);
                    _tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                    _tutorialAssets._arrowsParent.SetActive(true);
                    _tutorialAssets._elementsParent.SetActive(false);
                    _tutorialAssets._atomParent.SetActive(false);
                    yield return new WaitForSeconds(1);
                    _tutorialAssets._continueText.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.25f);
                    while (!Input.GetButtonDown("Submit"))
                    {
                        yield return null;
                    }
                    _tutorialAssets._continueText.gameObject.SetActive(false);
                    _tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);

                    break;
                case 2:
                    yield return new WaitForSeconds(1);
                    _tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                    _tutorialAssets._arrowsParent.SetActive(false);
                    _tutorialAssets._elementsParent.SetActive(true);
                    _tutorialAssets._atomParent.SetActive(false);
                    yield return new WaitForSeconds(1);
                    _tutorialAssets._continueText.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.25f);
                    while (!Input.GetButtonDown("Submit"))
                    {
                        yield return null;
                    }
                    _tutorialAssets._continueText.gameObject.SetActive(false);
                    _tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);

                    break;
                case 5:
                    //yield return new WaitForSeconds(1);
                    //_tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                    //_tutorialAssets._arrowsParent.SetActive(false);
                    //_tutorialAssets._elementsParent.SetActive(false);
                    //_tutorialAssets._atomParent.SetActive(true);
                    //yield return new WaitForSeconds(1);
                    //_tutorialAssets._continueText.gameObject.SetActive(true);
                    //yield return new WaitForSeconds(0.25f);
                    //while (!Input.GetButtonDown("Submit"))
                    //{
                    //    yield return null;
                    //}
                    //_tutorialAssets._continueText.gameObject.SetActive(false);
                    //_tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);

                    break;
            }
        }
  
        _tutorialAssets._tutorialDeployed = true;

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
             _movementAvailable = true;
     
 
    }


    void Update()
    {
        if (!_AtomsPanelOn)
        {

  
            PlayerMovementController();
        _slimeMainColor = Color.Lerp(_slimeMainColor, _slimeInfo._allSlimeColors[_slimeInfo._slimeID], 2 * Time.deltaTime);
        _scriptSlime._slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor", _slimeMainColor);
        for(int i = 0; i < _slimeInfo._elementsParticles.Length; i++)
        {
            _slimeInfo._quantityElementText[i].text = _slimeInfo._elementsParticles[i].ToString();
        }
        _slimeInfo._atomsText.text = MainController.Instance._saveLoadValues._totalAtoms.ToString();
        _slimeInfo._stepsText.text = MainController.Instance._saveLoadValues._totalSteps.ToString();

        _mainUI.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_mainUI.GetComponent<RectTransform>().anchoredPosition, new Vector2(0, 0), 2 * Time.deltaTime);
        }
        if (Input.GetButtonDown("Pause") && MainController.Instance._saveLoadValues._pauseAvailable)
        {
            switch (_AtomsPanelOn)
            {
                case true:
                    break;
                case false:
                    break;
            }
            _AtomsPanelOn = !_AtomsPanelOn;
            MainController.Instance._AtomAnimator.SetBool("AtomsIn", _AtomsPanelOn);
        }
        else
        {
            AtomPanelController();
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

       MainController.Instance._joystickImage.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(MainController.Instance._joystickImage.GetComponent<RectTransform>().anchoredPosition,
        _allJoystickPoses[_onPoseJoystick], 5 * Time.deltaTime);

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
                    MainController.Instance._saveLoadValues._totalAtoms--;
                    if (!_transformed)
                    {
                        TransformSlimeVoid();
           
                    }
                    break;
                case 2:
                    _slimeInfo._elementsParticles[1]++;
                    MainController.Instance._saveLoadValues._totalAtoms--;
                    if (!_transformed)
                    {
                        TransformSlimeVoid();
              
                    }
                    break;
                 
                case 3:
                    _slimeInfo._elementsParticles[2]++;
                    MainController.Instance._saveLoadValues._totalAtoms--;
                    if (!_transformed)
                    {
                        TransformSlimeVoid();
                 
                    }
                    break;
            
                case 4:
                    _slimeInfo._elementsParticles[3]++;
                    MainController.Instance._saveLoadValues._totalAtoms--;
                    if (!_transformed)
                    {
                        TransformSlimeVoid();
          
                    }
                    break;
            }
        }
  
    }


    // ===================== STAGE =====================

    void StageCreationVoid()
    {
        var Main = MainController.Instance;
        _allGrounds.Clear();

        //foreach (int place in _allStages[MainController.Instance._allStagesData[MainController.Instance._onWorldGlobal]._stageList[_idStage]]._allPlaces)
            foreach (int place in _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._allPlaces)
            {
            GameObject ground = Instantiate(_groundAssets[0], _parent);

            RectTransform groundRT = ground.GetComponent<RectTransform>();
            RectTransform targetRT = _allPositions[place].GetComponent<RectTransform>();

            groundRT.position = targetRT.position;
            groundRT.localScale = Vector3.one;

            StageGroundScript g = ground.GetComponent<StageGroundScript>();
            g._id = place;
            g._lockedBool = false;
            ground.GetComponent<Image>().sprite = _allGroundsSprites[MainController.Instance._onWorldGlobal];
            _backgroundImage[0].sprite = _allGroundsSprites[MainController.Instance._onWorldGlobal];
            _backgroundImage[0].color = _backgroundColor[MainController.Instance._onWorldGlobal];

            _backgroundImage[1].sprite = _allGroundsSprites[MainController.Instance._onWorldGlobal];
            _backgroundImage[2].sprite = _allGroundsSprites[MainController.Instance._onWorldGlobal];
            _allGrounds.Add(ground);
        
        }

        if(_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._bossAssets.Length > 0)
        {
            _bossUI.SetActive(true);


            switch (_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._bossAssets[0]._yposition)
            {
                case NewGameEvent.BossAssets._Yposition.Top:
                    _bossUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(_bossUI.GetComponent<RectTransform>().anchoredPosition.x, -90f);
                    _bossUI.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    switch (_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._bossAssets[0]._xposition)
                    {
                        case NewGameEvent.BossAssets._Xposition.Left:
                            _bossUI.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 45);
                            break;
                        case NewGameEvent.BossAssets._Xposition.Center:
                            _bossUI.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);

                            break;
                        case NewGameEvent.BossAssets._Xposition.Right:
                            _bossUI.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, -45);
                            break;
                    }
                    break;
                case NewGameEvent.BossAssets._Yposition.Center:
                    _bossUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(_bossUI.GetComponent<RectTransform>().anchoredPosition.x, 0);
                    _bossUI.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    switch (_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._bossAssets[0]._xposition)
                    {
                        case NewGameEvent.BossAssets._Xposition.Left:
                            _bossUI.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 90);
                            break;
                        case NewGameEvent.BossAssets._Xposition.Center:
                            _bossUI.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);

                            break;
                        case NewGameEvent.BossAssets._Xposition.Right:
                            _bossUI.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, -90);
                            break;
                    }
                    break;
                case NewGameEvent.BossAssets._Yposition.Bot:
                    _bossUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(_bossUI.GetComponent<RectTransform>().anchoredPosition.x, 90f);
                    _bossUI.GetComponent<RectTransform>().localScale = new Vector3(1, -1, 1);

                    switch (_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._bossAssets[0]._xposition)
                    {
                        case NewGameEvent.BossAssets._Xposition.Left:
                            _bossUI.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, -45);
                            break;
                        case NewGameEvent.BossAssets._Xposition.Center:
                            _bossUI.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);

                            break;
                        case NewGameEvent.BossAssets._Xposition.Right:
                            _bossUI.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 45);
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
            GameObject element = Instantiate(_elementPrefab, _parent);
            RectTransform rt = element.GetComponent<RectTransform>();
            rt.position = _allPositions[data._onPlace].GetComponent<RectTransform>().position;
            rt.localScale = Vector3.one;

            ElementOrbScript orb = element.GetComponent<ElementOrbScript>();
            orb._onPose = data._onPlace;
            orb.ID = data._elementType switch
            {
                NewGameEvent.Elements.ElementType.C => 0,
                NewGameEvent.Elements.ElementType.H => 1,
                _ => 2
            };

            orb._quantity = data._quantity;
            orb.ElementSetVoid();
            _allElements.Add(element);
            _elementsID.Add(data._onPlace);
            _elementsBool.Add(true);
        }





    }

    void SetAtoms()
    {
        var Main = MainController.Instance;
        for (int i = 0; i < _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._atomPlace.Length; i++)
        {
            GameObject atom = Instantiate(_atomPrefab, _parent);

            RectTransform rt = atom.GetComponent<RectTransform>();
            RectTransform targetRT =
                _allPositions[_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._atomPlace[i]]
                .GetComponent<RectTransform>();

            rt.position = targetRT.position;
            rt.localScale = Vector3.one;

            _allAtoms.Add(atom);
            _atomList.Add(_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._atomPlace[i]);
        }
    }

    void SetSteps()
    {
        var Main = MainController.Instance;
        for (int i = 0; i < _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._stepsPlace.Length; i++)
        {
            GameObject steps = Instantiate(_stepsPrefab, _parent);

            RectTransform rt = steps.GetComponent<RectTransform>();
            RectTransform targetRT =
                _allPositions[_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._stepsPlace[i]]
                .GetComponent<RectTransform>();

            rt.position = targetRT.position;
            rt.localScale = Vector3.one;

            _allSteps.Add(steps);
            _stepsList.Add(_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._stepsPlace[i]);
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
                    break;
                case NewGameEvent.Hazards.HazardsType.Column:
                    obs._id = 3;
                    obs._allObstacles[4].SetActive(true);
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
                          
                            switch(MainController.Instance._onWorldGlobal == 0 && !MainController.Instance._saveLoadValues._finalWorldUnlocked)
                            {
                                case false:
                                    RestartLevel();
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

        // 🔑 SIEMPRE recalcular
        CalculateMoves();
        HazardDetection();
        ElementDetection();
        StartCoroutine(AtomDetection());
        StartCoroutine(StepDetection());
        ExitDetection();

        if (restart)
        {
        
         RestartLevel();
            // Aquí luego puedes resetear nivel, animar, etc.
        }
    }

    private Coroutine _moveCoroutine;

    bool IsLocked()
    {
        if (MainController.Instance._saveLoadValues._totalSteps > 0)
        {
          
            for (int i = 0; i < _allGrounds.Count; i++)
            {
                if (_allGrounds[i].GetComponent<StageGroundScript>()._id == _onPose)
                {
                    if (_allGrounds[i].GetComponent<StageGroundScript>()._lockedBool)
                    {
                        MainController.Instance._saveLoadValues._totalSteps--;
                        //_allGrounds[i].GetComponent<StageGroundScript>()._lockImage.color = _lockedColors[2];
                        break;
                    }
         
                }
            }
            return false;
        }
  


        foreach (GameObject g in _allGrounds)
        {
            StageGroundScript ground = g.GetComponent<StageGroundScript>();

            if (ground._id == _onPose && ground._lockedBool)
            {
                return true;
            }
        }

        return false;
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
        _movementAvailable = false;
        //if (_allStages[MainController.Instance._allStagesData[MainController.Instance._onWorldGlobal]._stageList[_idStage]]._bossAssets.Length > 0)
        //{
        //    StartCoroutine(BossAttackTurn());
        //}
        _slimeAnimator.SetBool("Moving", true);

        yield return new WaitForSeconds(0.5f);
        switch (MainController.Instance._onWorldGlobal)
        {
            case 1:
                _turnToStorm--;
                if(_turnToStorm <= 0)
                {
                    switch (_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._wind)
                    {
                        case NewGameEvent.Wind.Left:
                            _windParticle[0].Play();
                            yield return new WaitForSeconds(0.25f);

                            if (_movesAvailable[2] &&
                                _onPose != 4 &&
                                _onPose != 9 &&
                                _onPose != 14 &&
                                _onPose != 19)
                            {
                                _onPose++;
                            }

                            yield return new WaitForSeconds(0.25f);
                          
                            break;

                        case NewGameEvent.Wind.Right:
                            _windParticle[1].Play();
                            yield return new WaitForSeconds(0.25f);

                            if (_movesAvailable[3] &&
                                _onPose != 0 &&
                                _onPose != 5 &&
                                _onPose != 10 &&
                                _onPose != 15 &&
                                _onPose != 20)
                            {
                                _onPose--;
                            }

                            yield return new WaitForSeconds(0.25f);
                      
                            break;
                    }
             
                    _windParticle[0].Stop();
                    _windParticle[1].Stop();
                    CalculateMoves();
                    AtomDetection();
                    ElementDetection();
                    HazardDetection();
                    StepDetection();
                    ExitDetection();
                    _turnToStorm = 5;
                }
                   
                break;
            case 3:
                switch (_sandStormOn)
                {
                    case false:
                        _movementsToSandStorm--;
                        if (_movementsToSandStorm <= 0)
                        {
                            _sandStorm.Play();                           
                            _movementsToSandStorm = Random.Range(3, 5);
                            _sandStormOn = true;
                        }
                        break;
                    case true:
                        _sandStorm.Stop();
                       
                        _sandStormOn = false;
                        break;
                }

                break;
        }
        if (_allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]]._bossAssets.Length > 0)
        {
            StartCoroutine(BossAttackTurn());
        }
        if (_turnsReturnToWater < 0 && _slimeInfo._slimeID == 2)
        {
           
            _turnsReturnToWater++;
            if(_turnsReturnToWater >= 0)
            {
                _slimeInfo._slimeID = 4;
                StartCoroutine(TransormatioNumerator());
            }
           
        }
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




    public void RestartLevel()
    {
        var Main = MainController.Instance;
        var _realID = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];
        MainController.Instance._restartBeam.Play("RestartBeam");
        _slimeObject.GetComponent<Animator>().Play("SlimeLeavesAnimation");
        StopMoveCoroutine();
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
        for(int i = 0; i < _allElements.Count; i++)
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
        for(int i = 0; i < _allAtoms.Count; i++)
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
        _elementsID.Clear();
        MainController.Instance._saveLoadValues._totalAtoms -= _atomsObtained;
        _waterWalk.Stop();
        _smoke.Stop();
        _slimeInfo._slimeID = 0;
        _slimeAnimator.SetInteger("ID", 0);
        _transformed = false;
    
        StartVoids();
    }

    public IEnumerator NexttLevel()
    {
        var Main = MainController.Instance;
        var _stageId = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];
        StopMoveCoroutine();
        _movementAvailable = false;
        _tutorialAssets._tutorialDeployed = false;
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

        _slimeInfo._elementsParticles[0] = 0;
        _slimeInfo._elementsParticles[1] = 0;
        _slimeInfo._elementsParticles[2] = 0;

        for(int i = 0; i < _exitEntranceObjects.Count; i++)
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
     
        _transformed = false;

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
                                RestartLevel();
                                break;
                            case 1:
                                RestartLevel();
                                break;
                            case 2:
                                _allHazards[i].GetComponent<ObstaclesScript>()._fireParticle.Stop();
                                _allHazards[i].GetComponent<ObstaclesScript>()._smokeParticle.Play();
                                break;
                            case 3:
                                RestartLevel();
                                break;
                            case 4:
                                _allHazards[i].GetComponent<ObstaclesScript>()._fireParticle.Stop();
                                _allHazards[i].GetComponent<ObstaclesScript>()._smokeParticle.Play();
                                _slimeInfo._slimeID = 2;
                                StartCoroutine(TransormatioNumerator());
                                break;
                        }  
                        Debug.Log("Fire");
               
                        break;
                case NewGameEvent.Hazards.HazardsType.Hole:
                        if (_slimeInfo._slimeID != 3)
                        {
                           RestartLevel();
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

                }
            }
        }
    }

    public void ElementDetection()
    {
        var Main = MainController.Instance;
        var ElementInfo = _allStages[Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage]];
        for (int i = 0; i < ElementInfo._elements.Length; i++)
        {
            if (_onPose == ElementInfo._elements[i]._onPlace && _elementsBool[i])
            {
                switch (ElementInfo._elements[i]._elementType)
                {
                    case NewGameEvent.Elements.ElementType.C:
                        _slimeInfo._elementsParticles[0] += ElementInfo._elements[i]._quantity;
                        break;
                    case NewGameEvent.Elements.ElementType.H:
                        _slimeInfo._elementsParticles[1] += ElementInfo._elements[i]._quantity;
                        break;
                    case NewGameEvent.Elements.ElementType.O:
                        _slimeInfo._elementsParticles[2] += ElementInfo._elements[i]._quantity;
                        break;
                }
             
                _elementsBool[i] = false;
                if (!_transformed)
                {
                    TransformSlimeVoid();
                }

                StartCoroutine(ElementNumerator());
                break;
           


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
        for (int i = 0; i < _atomList.Count; i++)
        {
            if (_onPose == _atomList[i])
            {
         

                if (MainController.Instance._onWorldGlobal == 0 && Main._allTurnsInfo[Main._onWorldGlobal]._stagesID[_idStage] == 5)
                {
                    StopMoveCoroutine();
                    _movementAvailable = false;
                    yield return new WaitForSeconds(1);
                    _tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                    _tutorialAssets._arrowsParent.SetActive(false);
                    _tutorialAssets._elementsParent.SetActive(false);
                    _tutorialAssets._atomParent.SetActive(true);
                    _tutorialAssets._stepParent.SetActive(false);
                    yield return new WaitForSeconds(1);
                    _tutorialAssets._continueText.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.25f);
                    MainController.Instance._saveLoadValues._totalAtoms++;
                    Debug.Log("Atomo en: " + _atomList[i].ToString());
                    Destroy(_allAtoms[i]);
                    _allAtoms.RemoveAt(i);
                    _atomList.RemoveAt(i);
                    _atomsObtained++;
                    while (!Input.GetButtonDown("Pause"))
                    {
                        yield return null;
                    }
                    _tutorialAssets._continueText.gameObject.SetActive(false);
                    _tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
                    _movementAvailable = true;
                    MainController.Instance._AtomAnimator.SetBool("AtomsIn", true);
                    MainController.Instance._saveLoadValues._pauseAvailable = true;
                    _AtomsPanelOn = true;


                }
                else
                {
                    MainController.Instance._saveLoadValues._totalAtoms++;
                    Debug.Log("Atomo en: " + _atomList[i].ToString());
                    Destroy(_allAtoms[i]);
                    _allAtoms.RemoveAt(i);
                    _atomList.RemoveAt(i);
                    _atomsObtained++;
                }
            }



        }

    }

    public IEnumerator StepDetection()
    {

        for (int i = 0; i < _stepsList.Count; i++)
        {
            if (_onPose == _stepsList[i])
            {
                if(MainController.Instance._onWorldGlobal == 0 && MainController.Instance._saveLoadValues._totalSteps <= 0)
                {
                    StopMoveCoroutine();
                    _movementAvailable = false;
                    yield return new WaitForSeconds(1);
                    _tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                    _tutorialAssets._arrowsParent.SetActive(false);
                    _tutorialAssets._elementsParent.SetActive(false);
                    _tutorialAssets._atomParent.SetActive(false);
                    _tutorialAssets._stepParent.SetActive(true);
                    yield return new WaitForSeconds(1);
                    _tutorialAssets._continueText.gameObject.SetActive(true);
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
                    _tutorialAssets._continueText.gameObject.SetActive(false);
                    _tutorialAssets._tutorialAnimator.SetBool("TutorialIn", false);
                    _movementAvailable = true;
                }
                else
                {
                    MainController.Instance._saveLoadValues._totalSteps++;
                    Debug.Log("Steps en: " + _stepsList[i].ToString());
                    Destroy(_allSteps[i]);
                    _allSteps.RemoveAt(i);
                    _stepsList.RemoveAt(i);
                }
               
            }



        }

    }


    public void TransformSlimeVoid()
    {
        if (_slimeInfo._elementsParticles[0] >= 2)
        {
            _slimeInfo._slimeID = 1;
            Debug.Log("CARBONO");
            _formulaText.text = "C2";
            _nameText.text = GameInitScript.Instance.GetText("C2");

            StartCoroutine(TransormatioNumerator());
        }
        else if (_slimeInfo._elementsParticles[1] >= 2 && _slimeInfo._elementsParticles[2] >= 1)
        {
            switch (MainController.Instance._onWorldGlobal)
            {
                case 2:
                    _slimeInfo._slimeID = 4;
                    _formulaText.text = "H20";
                    _nameText.text = GameInitScript.Instance.GetText("ICE");
                    Debug.Log("HIELO");
                    _waterWalk.Play();
                    _smoke.Stop();
                    StartCoroutine(TransormatioNumerator());
                    _turnsReturnToWater = -1;
                    break;
                default:
                    _slimeInfo._slimeID = 2;
                    _formulaText.text = "H20";
                    _nameText.text = GameInitScript.Instance.GetText("H20");
                    Debug.Log("AGUA");
                    _waterWalk.Play();
                    _smoke.Stop();
                    StartCoroutine(TransormatioNumerator());
                    break;
            }

        }
        else if (_slimeInfo._elementsParticles[0] >= 1 && _slimeInfo._elementsParticles[2] >= 2)
        {
            _slimeInfo._slimeID = 3;
            _formulaText.text = "C02";
            Debug.Log("C02");
            _smoke.Play();
            _waterWalk.Stop();
            _nameText.text = GameInitScript.Instance.GetText("CO2");
            StartCoroutine(TransormatioNumerator());
        }

    }

    public IEnumerator TransormatioNumerator()
    {
        MainController.Instance._AtomAnimator.SetBool("AtomsIn", false);
        _AtomsPanelOn = false;
        _transformed = true;
        StopMoveCoroutine();
        _movementAvailable = false;
        _transformAnimator.SetBool("Success", true);
        yield return new WaitForSeconds(0.5f);
        switch (_slimeInfo._slimeID)
        {
            case 0:
                break;
            case 1:
                _slimeAnimator.SetInteger("ID", 3);
                break;
            case 2:
                _slimeAnimator.SetInteger("ID", 1);
                break;
            case 3:
                _slimeAnimator.SetInteger("ID", 2);
                break;
            case 4:
                _slimeAnimator.SetInteger("ID", 5);
                break;
        }
        yield return new WaitForSeconds(1);
        _transformAnimator.SetBool("Success", false);
        yield return new WaitForSeconds(0.5f);
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
        StopMoveCoroutine();
        _movementAvailable = false;

        yield return new WaitForSeconds(0.5f);
        _slimeObject.GetComponent<Animator>().Play("SlimeLeavesAnimation");
        _exitEntranceObjects[1].GetComponent<ExitScriptObject>()._exitParticle.Play();
        yield return new WaitForSeconds(0.5f);
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
        switch (MainController.Instance._onWorldGlobal)
        {
            case 0:
                MainController.Instance._cinematicBorders.SetBool("FadeIn", true);
                MainController.Instance._onWorldGlobal = 3;
                break;
            case 1:
                MainController.Instance._onWorldGlobal = 0;
                break;
            case 2:
                MainController.Instance._onWorldGlobal = 1;
                break;
            case 3:
                MainController.Instance._onWorldGlobal = 2;
                break;
        }
        MainController.Instance._introSpecial = true;
        MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
        yield return new WaitForSeconds(1);
        MainController.Instance.LoadSceneByName("IntroScene");

    }

 
}
