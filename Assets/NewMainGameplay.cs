using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMainGameplay : MonoBehaviour
{
    public Transform[] _allPositions;
    public GameObject[] _groundAssets;
    public NewGameEvent[] _allStages;
    public List<GameObject> _allGrounds = new List<GameObject>();
    public Transform _parent;
    public GameObject _elementPrefab;
    public GameObject _hazardPrefab;
    public int _idStage;

    public GameObject _entrancePrefab;   
    public GameObject _exitPrefab;

    public float _movementSpeed;
    public bool _movementAvailable;
    public bool _buttonPressed;
    public int _onPose;
    public GameObject _slimeObject;

    public Color[] _lockedColors;

    public bool[] _movesAvailable;
 
    void Start()
    {
        StageCreationVoid();
        SetElements();
        SetHazards();
        SetEntranceExit();
        _onPose = _allStages[_idStage]._spawnPoint;

    }

    private void Update()
    {
        if(_movementAvailable)
        PlayerMovementController();
      
    }

    public void StageCreationVoid()
    {
        for (int i = 0; i < _allStages[_idStage]._allPlaces.Count; i++)
        {
            GameObject ground = Instantiate(_groundAssets[_idStage], _parent);

            RectTransform groundRT = ground.GetComponent<RectTransform>();
            RectTransform targetRT =
                _allPositions[_allStages[_idStage]._allPlaces[i]].GetComponent<RectTransform>();

            if (groundRT != null && targetRT != null)
            {
                // Posición en mundo del target
                Vector3 worldPos = targetRT.position;

                // Convertir a local del nuevo parent
                groundRT.position = worldPos;

                groundRT.localScale = Vector3.one;
            }
            ground.GetComponent<StageGroundScript>()._id = _allStages[_idStage]._allPlaces[i];
            _allGrounds.Add(ground);
        }
       
    }

    public void SetElements()
    {
        for(int i = 0; i < _allStages[_idStage]._elements.Length; i++)
        {
            GameObject Element = Instantiate(_elementPrefab, transform.position, transform.rotation);
            switch (_allStages[_idStage]._elements[i]._elementType)
            {
                case NewGameEvent.Elements.ElementType.C:             
                    Element.GetComponent<ElementOrbScript>().ID = 0;           
                    break;
                case NewGameEvent.Elements.ElementType.H:
                    Element.GetComponent<ElementOrbScript>().ID = 1;
                    break;
                case NewGameEvent.Elements.ElementType.O:
                    Element.GetComponent<ElementOrbScript>().ID = 2;
                    break;
            }
            Element.GetComponent<ElementOrbScript>()._quantity = _allStages[_idStage]._elements[i]._quantity;
            Element.transform.parent = _parent.transform;
            Element.transform.localScale = Vector3.one;
            Element.GetComponent<RectTransform>().anchoredPosition = _allPositions[_allStages[_idStage]._elements[i]._onPlace].GetComponent<RectTransform>().anchoredPosition;
            Element.GetComponent<ElementOrbScript>().ElementSetVoid();
        }
    }

    public void SetHazards()
    {
        for (int i = 0; i < _allStages[_idStage]._hazards.Length; i++)
        {
            GameObject hazard = Instantiate(_hazardPrefab, _parent);

            RectTransform hazardRT = hazard.GetComponent<RectTransform>();
            RectTransform targetRT =
                _allPositions[_allStages[_idStage]._hazards[i]._onPlace]
                .GetComponent<RectTransform>();

            switch (_allStages[_idStage]._hazards[i]._hazards)
            {
                case NewGameEvent.Hazards.HazardsType.Fire:
                    hazard.GetComponent<ObstaclesScript>()._id = 0;
                    break;
                case NewGameEvent.Hazards.HazardsType.Hole:
                    hazard.GetComponent<ObstaclesScript>()._id = 1;
                    break;
                case NewGameEvent.Hazards.HazardsType.Switch:
                    hazard.GetComponent<ObstaclesScript>()._id = 2;
                    break;
                case NewGameEvent.Hazards.HazardsType.Switch2:
                    hazard.GetComponent<ObstaclesScript>()._id = 3;
                    break;
            }

            if (hazardRT != null && targetRT != null)
            {
                // Copiar posición en mundo
                hazardRT.position = targetRT.position;
                hazardRT.localScale = Vector3.one;
            }

            hazard.GetComponent<ObstaclesScript>().SetObstacle();
        }
    }

    public void SetEntranceExit()
    {
        GameObject entrance = Instantiate(_entrancePrefab, _parent);
        RectTransform entranceRT = entrance.GetComponent<RectTransform>();
        RectTransform targetEntranceRT =
           _allPositions[_allStages[_idStage]._spawnPoint]
           .GetComponent<RectTransform>();

        if (entranceRT != null && targetEntranceRT != null)
        {
            // Copiar posición en mundo
            entranceRT.position = targetEntranceRT.position;
            entranceRT.localScale = Vector3.one;
        }


        GameObject exit = Instantiate(_exitPrefab, _parent);
        RectTransform exitRT = exit.GetComponent<RectTransform>();
        RectTransform targetExitRT =
   _allPositions[_allStages[_idStage]._exitPoint]
   .GetComponent<RectTransform>();

        if (entranceRT != null && targetExitRT != null)
        {
            // Copiar posición en mundo
            exitRT.position = targetExitRT.position;
            exitRT.localScale = Vector3.one;
        }
    }

    public void PlayerMovementController()
    {
        RectTransform slimeRT = _slimeObject.GetComponent<RectTransform>();
        RectTransform targetRT = _allPositions[_onPose].GetComponent<RectTransform>();

        slimeRT.position = Vector3.Lerp(
            slimeRT.position,
            targetRT.position,
            _movementSpeed * Time.deltaTime
        );

        if (Input.GetAxisRaw("Horizontal") > 0 && !_buttonPressed)
        {
            LockPositions();
            _onPose++;
            _buttonPressed = true;
            RestartCondition();
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && !_buttonPressed)
        {
            LockPositions();
            _onPose--;
            _buttonPressed = true;
            RestartCondition();
        }

        else if (Input.GetAxisRaw("Vertical") < 0 && !_buttonPressed)
        {
      
            if (_onPose - 5 >= 0)
            {
                LockPositions();
                _onPose -= 5;
                _buttonPressed = true;
                RestartCondition();
            }
            _buttonPressed = true;
        }

        else if (Input.GetAxisRaw("Vertical") > 0 && !_buttonPressed)
        {
            if(_onPose + 5 <= _allPositions.Length - 1)
            {
                LockPositions();
                _onPose += 5;
                _buttonPressed = true;
                RestartCondition();
            }
   
        }

        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
        {          
            _buttonPressed = false;
        }
    }
    public void LockPositions()
    {
        bool available = false;

        for (int i = 0; i < _allStages[_idStage]._allPlaces.Count; i++)
        {
            if (_onPose == _allStages[_idStage]._allPlaces[i])
            {
                available = true;
                break;
            }
        }

        if (available)
        {
            for(int i = 0; i < _allGrounds.Count; i++)
            {
                if(_onPose == _allGrounds[i].GetComponent<StageGroundScript>()._id)
                {            
                    _allGrounds[i].GetComponent<StageGroundScript>()._lockedBool = true;
                    _allGrounds[i].GetComponent<StageGroundScript>()._lockImage.color = _lockedColors[1];
                    return;
                }
            }       
        }
    }

    public void RestartCondition()
    {
        for(int i = 0; i < _allGrounds.Count; i++)
        {
            if (_onPose == _allGrounds[i].GetComponent<StageGroundScript>()._id && _allGrounds[i].GetComponent<StageGroundScript>()._lockedBool)
            {
                Debug.Log("RESTART");
                return;
            }

        }
    
    }

}
