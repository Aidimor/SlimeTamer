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
    public List<GameObject> _allGrounds = new List<GameObject>();
    public List<GameObject> _allElements = new List<GameObject>();
    public List<GameObject> _allHazards = new List<GameObject>();
    public List<GameObject> _allAtoms = new List<GameObject>();
    public Transform _parent;

    public GameObject _elementPrefab;
    public GameObject _atomPrefab;
    public GameObject _hazardPrefab;
    public GameObject _entrancePrefab;
    public GameObject _exitPrefab;

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
        public int[] _elementsParticles; //0=Carbon,1=Hydrogen,2=Oxygen
        public Color[] _allSlimeColors;
    }
    public SlimeInfo _slimeInfo;
    public Sprite[] _allGroundsSprites;
    public Image[] _backgroundImage;
    public Color[] _backgroundColor;

    public Animator _slimeAnimator;
    public int _atomsObtained;
    public List<int> _atomList = new List<int>();
    public Color _slimeMainColor;
    public Animator _transformAnimator;
    bool _transformed;

    public TextMeshProUGUI _formulaText;
    public TextMeshProUGUI _nameText;

    public ParticleSystem _hitWalk;
    public ParticleSystem _waterWalk;
    public ParticleSystem _smoke;

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
        public TextMeshProUGUI _continueText;
    }
    public TutorialAssets _tutorialAssets;

    void Start()
    {
        StartVoids();
        MainController.Instance._bordersAnimator.SetBool("BorderOut", true);
        MainController.Instance._cinematicBorders.SetBool("FadeIn", false);
        _movementAvailable = false;
    }

    public void StartVoids()
    {
        StageCreationVoid();
        SetElements();
        SetHazards();
        SetEntranceExit();

        _atomsObtained = 0;

        _onPose = _allStages[_idStage]._spawnPoint;

        // 🔑 POSICIÓN CORRECTA
        _slimeObject.GetComponent<RectTransform>().position =
            _allPositions[_onPose].GetComponent<RectTransform>().position;
       
        CalculateMoves();
        StartCoroutine(StartGameNumerator());
    }

    public IEnumerator StartGameNumerator()
    {
        _tutorialAssets._continueText.gameObject.SetActive(false);
        switch (_idStage)
        {
            case 0:
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
                _tutorialAssets._tutorialAnimator.SetBool("TutorialIn", true);
                _tutorialAssets._arrowsParent.SetActive(false);
                _tutorialAssets._elementsParent.SetActive(false);
                _tutorialAssets._atomParent.SetActive(true);
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
        }
        yield return new WaitForSeconds(1);
        _movementAvailable = true;
    }


    void Update()
    {
     
            PlayerMovementController();
        _slimeMainColor = Color.Lerp(_slimeMainColor, _slimeInfo._allSlimeColors[_slimeInfo._slimeID], 2 * Time.deltaTime);
        _scriptSlime._slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor", _slimeMainColor);
    }

    // ===================== STAGE =====================

    void StageCreationVoid()
    {
        _allGrounds.Clear();

        foreach (int place in _allStages[_idStage]._allPlaces)
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
    }

    void SetElements()
    {
        foreach (var data in _allStages[_idStage]._elements)
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

        for (int i = 0; i < _allStages[_idStage]._atomPlace.Length; i++)
        {
            GameObject atom = Instantiate(_atomPrefab, _parent);

            RectTransform rt = atom.GetComponent<RectTransform>();
            RectTransform targetRT =
                _allPositions[_allStages[_idStage]._atomPlace[i]]
                .GetComponent<RectTransform>();

            rt.position = targetRT.position;
            rt.localScale = Vector3.one;

            _allAtoms.Add(atom);
            _atomList.Add(_allStages[_idStage]._atomPlace[i]);
        }

    }

    void SetHazards()
    {
        foreach (var data in _allStages[_idStage]._hazards)
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
            //obs._id = data._hazards switch
            //{
            //    NewGameEvent.Hazards.HazardsType.Fire => 0,
            //    NewGameEvent.Hazards.HazardsType.Hole => 1,
            //    NewGameEvent.Hazards.HazardsType.Switch => 2,
            //    NewGameEvent.Hazards.HazardsType.Column => 4
            //};

            //obs.SetObstacle();
            _allHazards.Add(hazard);
        }
    }

    void SetEntranceExit()
    {
        CreateMarker(_entrancePrefab, _allStages[_idStage]._spawnPoint);
        CreateMarker(_exitPrefab, _allStages[_idStage]._exitPoint);
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
        AtomDetection();
        ExitDetection();
        if (restart)
        {
        
         RestartLevel();
            // Aquí luego puedes resetear nivel, animar, etc.
        }
    }

    private Coroutine _moveCoroutine;

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
        _movementAvailable = false;
        _slimeAnimator.SetBool("Moving", true);

        yield return new WaitForSeconds(0.5f);

        _slimeAnimator.SetBool("Moving", false);

        yield return new WaitForSeconds(0.3f);

        _movementAvailable = true;
    }


    // ===================== LOGIC =====================

    void CalculateMoves()
    {
        for (int i = 0; i < 4; i++)
            _movesAvailable[i] = false;

        foreach (int place in _allStages[_idStage]._allPlaces)
        {
            if (place == _onPose + 5) _movesAvailable[0] = true;
            if (place == _onPose - 5) _movesAvailable[1] = true;
            if (place == _onPose + 1) _movesAvailable[2] = true;
            if (place == _onPose - 1) _movesAvailable[3] = true;
        }

        for(int i = 0; i < _allStages[_idStage]._hazards.Length; i++)
        {
            if (_allStages[_idStage]._hazards[i]._hazards == NewGameEvent.Hazards.HazardsType.Column)
            {
                if(_onPose + 5 == _allStages[_idStage]._hazards[i]._onPlace && !_allStages[_idStage]._hazards[i]._finished) _movesAvailable[0] = false;
                if (_onPose - 5 == _allStages[_idStage]._hazards[i]._onPlace && !_allStages[_idStage]._hazards[i]._finished) _movesAvailable[1] = false;
                if (_onPose + 1 == _allStages[_idStage]._hazards[i]._onPlace && !_allStages[_idStage]._hazards[i]._finished) _movesAvailable[2] = false;
                if (_onPose - 1 == _allStages[_idStage]._hazards[i]._onPlace && !_allStages[_idStage]._hazards[i]._finished) _movesAvailable[3] = false;

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
                ground._lockedBool = true;
                ground._lockImage.color = _lockedColors[1];
                return;
            }
        }
    }

    bool IsLocked()
    {
        foreach (GameObject g in _allGrounds)
        {
            StageGroundScript ground = g.GetComponent<StageGroundScript>();
            if (ground._id == _onPose && ground._lockedBool)
                return true;
        }
        return false;
    }

    public void RestartLevel()
    {
        MainController.Instance._restartBeam.Play("RestartBeam");
        _movementAvailable = false;
        for (int i = 0; i < _allStages[_idStage]._hazards.Length; i++)
        {
            _allStages[_idStage]._hazards[i]._finished = false;
        }
        Debug.Log(_allStages[_idStage]._spawnPoint);
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
        _movementAvailable = true;
        StartVoids();
    }

    public IEnumerator NexttLevel()
    {
        _movementAvailable = false;
        for(int i = 0; i < _allStages[_idStage]._hazards.Length; i++)
        {
            _allStages[_idStage]._hazards[i]._finished = false;
        }
        _idStage++;
        yield return new WaitForSeconds(0.5f);
        _elementsBool.Clear();
        for (int i = 0; i < _allGrounds.Count; i++)
            Destroy(_allGrounds[i]);
        _allGrounds.Clear();

        for (int i = 0; i < _allElements.Count; i++)
            Destroy(_allElements[i]);
        _allElements.Clear();

        for (int i = 0; i < _allHazards.Count; i++)
            Destroy(_allHazards[i]);
        _allHazards.Clear();

        _slimeInfo._elementsParticles[0] = 0;
        _slimeInfo._elementsParticles[1] = 0;
        _slimeInfo._elementsParticles[2] = 0;

        for(int i = 0; i < _exitEntranceObjects.Count; i++)
        {
            Destroy(_exitEntranceObjects[i]);
        }
        _exitEntranceObjects.Clear();

        Debug.Log("NextLevel");

        // NUEVO spawn
        _onPose = _allStages[_idStage]._spawnPoint;

        // 🔑 POSICIÓN CORRECTA
        _slimeObject.GetComponent<RectTransform>().position =
            _allPositions[_onPose].GetComponent<RectTransform>().position;
        _elementsID.Clear();
        StartVoids();          // recrea stage
        CalculateMoves();      // recalcula movimientos
        _slimeInfo._slimeID = 0;
        _slimeAnimator.SetInteger("ID", 0);
        _waterWalk.Stop();
        _smoke.Stop();
        yield return new WaitForSeconds(0.2f);
     
        _transformed = false;
    }


    public void HazardDetection()
    {
        var HazardInfo = _allStages[_idStage];
        for (int i = 0; i < HazardInfo._hazards.Length; i++)
        {
            if(_onPose == HazardInfo._hazards[i]._onPlace)
            {
                switch (HazardInfo._hazards[i]._hazards)
            {
                case NewGameEvent.Hazards.HazardsType.Fire:
                      if(_slimeInfo._slimeID != 2)
                        {
                            RestartLevel();
                        }
                        else
                        {
                            _allHazards[i].GetComponent<ObstaclesScript>()._fireParticle.Stop();
                            _allHazards[i].GetComponent<ObstaclesScript>()._smokeParticle.Play();
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
                        if (_slimeInfo._slimeID == 1)
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

                            //for (int y = 0; y < _allHazards.Count; y++)
                            //{
                            //    if (_allHazards[y].)
                            //    {
                   
                            //        HazardInfo._hazards[i]._finished = true;
                            //    }
                            //}


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
        var ElementInfo = _allStages[_idStage];
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

    public void AtomDetection()
    {
   
        for (int i = 0; i < _atomList.Count; i++)
        {
            if (_onPose == _atomList[i])
            {
                MainController.Instance._saveLoadValues._totalAtoms++;
                Debug.Log("Atomo en: " + _atomList[i].ToString());
                Destroy(_allAtoms[i]);
                _allAtoms.RemoveAt(i);
                _atomList.RemoveAt(i);
                _atomsObtained++;
            }
   


        }
        //for (int i = 0; i < _allAtoms.Count; i++)
        //{
        //    if (_allAtoms[i].GetComponent<AtomScript>()._onPose == _onPose)
        //    {
        //        Destroy(_allAtoms[i].gameObject);
        //        _allAtoms.RemoveAt(i);
        //    }

        //}

    }


    public void TransformSlimeVoid()
    {
        if(_slimeInfo._elementsParticles[0] >= 2)
        {
            _slimeInfo._slimeID = 1;
            Debug.Log("CARBONO");
            _formulaText.text = "C2";
            _nameText.text = GameInitScript.Instance.GetText("C2");
    
            StartCoroutine(TransormatioNumerator());
        }
        else if (_slimeInfo._elementsParticles[1] >= 2 && _slimeInfo._elementsParticles[2] >= 1)
        {
            _slimeInfo._slimeID = 2;
            _formulaText.text = "H20";
            _nameText.text = GameInitScript.Instance.GetText("H20");
            Debug.Log("AGUA");
            _waterWalk.Play();
            _smoke.Stop();
            StartCoroutine(TransormatioNumerator());
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
        }
        yield return new WaitForSeconds(1);
        _transformAnimator.SetBool("Success", false);
        yield return new WaitForSeconds(0.5f);
        _movementAvailable = true;
    }

    public void ExitDetection()
    {
        var ExitID= _allStages[_idStage]._exitPoint;
        if (_onPose == ExitID)
        {
            StartCoroutine(ExitNumerator());            
        }
    }

    public IEnumerator ExitNumerator()
    {
        _movementAvailable = false;
        yield return new WaitForSeconds(1);
        _elementsBool.Clear();
        MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
        MainController.Instance._cinematicBorders.SetBool("FadeIn", true);
       
        yield return new WaitForSeconds(1);
        StartCoroutine(NexttLevel());
        yield return new WaitForSeconds(1);
        MainController.Instance._bordersAnimator.SetBool("BorderOut", true);
        MainController.Instance._cinematicBorders.SetBool("FadeIn", false);
    }

    public IEnumerator TransformSlimeNumerator()
    {
        yield return new WaitForSeconds(0.2f);
    }
}
