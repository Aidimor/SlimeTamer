using System.Collections.Generic;
using UnityEngine;
using System.Collections;


public class NewMainGameplay : MonoBehaviour
{
    public Transform[] _allPositions;
    public GameObject[] _groundAssets;
    public NewGameEvent[] _allStages;
    public List<GameObject> _allGrounds = new List<GameObject>();
    public List<GameObject> _allElements = new List<GameObject>();
    public List<GameObject> _allHazards = new List<GameObject>();
    public Transform _parent;

    public GameObject _elementPrefab;
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

    // 0 = abajo, 1 = arriba, 2 = derecha, 3 = izquierda
    public bool[] _movesAvailable = new bool[4];

    [System.Serializable]
    public class SlimeInfo
    {
        public int _slimeID; //0=Normal,1=Solid,2=Liquid,3=Gas
        public int[] _elementsParticles; //0=Carbon,1=Hydrogen,2=Oxygen
    }
    public SlimeInfo _slimeInfo;


    void Start()
    {
        StartVoids();
    }

    public void StartVoids()
    {
        StageCreationVoid();
        SetElements();
        SetHazards();
        SetEntranceExit();

        _onPose = _allStages[_idStage]._spawnPoint;
        CalculateMoves();
    }

    void Update()
    {
        if (_movementAvailable)
            PlayerMovementController();
    }

    // ===================== STAGE =====================

    void StageCreationVoid()
    {
        _allGrounds.Clear();

        foreach (int place in _allStages[_idStage]._allPlaces)
        {
            GameObject ground = Instantiate(_groundAssets[_idStage], _parent);

            RectTransform groundRT = ground.GetComponent<RectTransform>();
            RectTransform targetRT = _allPositions[place].GetComponent<RectTransform>();

            groundRT.position = targetRT.position;
            groundRT.localScale = Vector3.one;

            StageGroundScript g = ground.GetComponent<StageGroundScript>();
            g._id = place;
            g._lockedBool = false;

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

            obs._id = data._hazards switch
            {
                NewGameEvent.Hazards.HazardsType.Fire => 0,
                NewGameEvent.Hazards.HazardsType.Hole => 1,
                NewGameEvent.Hazards.HazardsType.Switch => 2,
                _ => 3
            };

            obs.SetObstacle();
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
    }

    // ===================== MOVEMENT =====================

    void PlayerMovementController()
    {
        RectTransform slimeRT = _slimeObject.GetComponent<RectTransform>();
        RectTransform targetRT = _allPositions[_onPose].GetComponent<RectTransform>();

        slimeRT.position = Vector3.Lerp(
            slimeRT.position,
            targetRT.position,
            _movementSpeed * Time.deltaTime
        );

        // Reset input
        if (Input.GetAxisRaw("Horizontal") == 0 &&
            Input.GetAxisRaw("Vertical") == 0)
        {
            _buttonPressed = false;
        }

        if (_buttonPressed) return;

        if (Input.GetAxisRaw("Horizontal") > 0 && _movesAvailable[2])
            MoveTo(_onPose + 1);

        else if (Input.GetAxisRaw("Horizontal") < 0 && _movesAvailable[3])
            MoveTo(_onPose - 1);

        else if (Input.GetAxisRaw("Vertical") > 0 && _movesAvailable[0])
            MoveTo(_onPose + 5);

        else if (Input.GetAxisRaw("Vertical") < 0 && _movesAvailable[1])
            MoveTo(_onPose - 5);
    }

    void MoveTo(int newPose)
    {
        LockCurrentPosition();

        _onPose = newPose;
        _buttonPressed = true;

        bool restart = IsLocked();

        // 🔑 SIEMPRE recalcular
        CalculateMoves();
        HazardDetection();
        ElementDetection();
        if (restart)
        {
        
            StartCoroutine(RestartLevel());
            // Aquí luego puedes resetear nivel, animar, etc.
        }
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

        Debug.Log("SECURE");
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

    public IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(0.5f);
        for(int i = 0; i < _allGrounds.Count; i++)
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
        _slimeInfo._elementsParticles[0] = 0;
        _slimeInfo._elementsParticles[1] = 0;
        _slimeInfo._elementsParticles[2] = 0;
        Debug.Log("RESTART");
        StartVoids();
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
                            StartCoroutine(RestartLevel());
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
                        Debug.Log("Switch");
                        break;
                 case NewGameEvent.Hazards.HazardsType.Switch2:
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
            if (_onPose == ElementInfo._elements[i]._onPlace)
            {
                switch  (ElementInfo._elements[i]._elementType)
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
            }
            
        }
        for(int i = 0; i < _allElements.Count; i++)
        {
            if(_allElements[i].GetComponent<ElementOrbScript>()._onPose == _onPose)
            {
                Destroy(_allElements[i].gameObject);
                _allElements.RemoveAt(i);
            }
           
        }
    }
}
