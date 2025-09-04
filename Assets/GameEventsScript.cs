using UnityEngine;

public class GameEventsScript : MonoBehaviour
{
    [SerializeField] private MainGameplayScript _scriptMain;
    [Header("Lista de eventos")]
    public GameEvent[] _specialEvents;   // Ahora es directamente GameEvent[]

    [Header("Padre de los objetos instanciados")]
    public Transform _eventosParent;
    public GameObject _currentEventPrefab;
    public int _onEvent;
  

    void Start()
    {
        for (int i = 0; i < _scriptMain._totalStages._total; i++)
        {
   
            _scriptMain._GamesList.Add(Random.Range(0, _specialEvents.Length));
        }
        StartLevel();
    }

    public void StartLevel()
    {
        if (_specialEvents.Length > 0 && _specialEvents[_scriptMain._GamesList[_onEvent]]._eventPrefab != null)
        {
            GameObject evento = Instantiate(
                _specialEvents[_scriptMain._GamesList[_onEvent]]._eventPrefab,
                transform.position,
                transform.rotation,
                _eventosParent   // opcional: lo hace hijo directo
            );

            evento.transform.parent = _eventosParent.transform;
            evento.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            evento.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 90);
            _currentEventPrefab = evento;
            // Ejemplo: instanciar el primer evento
            switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._eventClassification)
        {
            case GameEvent.EventClassification.Normal:
            case GameEvent.EventClassification.Fight:
                    switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._weakto.Length)
                    {
                        case 1:
                            switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._weakto[0])
                            {
                                case GameEvent.WeakTo.Water:
                                    _scriptMain._rightElementID[0] = 1;
                                    break;
                                case GameEvent.WeakTo.Air:
                                    _scriptMain._rightElementID[0] = 2;
                                    break;
                                case GameEvent.WeakTo.Earth:
                                    _scriptMain._rightElementID[0] = 3;
                                    break;
                                case GameEvent.WeakTo.Sand:
                                    _scriptMain._rightElementID[0] = 4;
                                    break;
                                case GameEvent.WeakTo.Snow:
                                    _scriptMain._rightElementID[0] = 5;
                                    break;
                                case GameEvent.WeakTo.Mud:
                                    _scriptMain._rightElementID[0] = 6;
                                    break;
                            }
                            break;
                        case 2:
                            for (int i = 0; i < 2; i++)
                            {
                                switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._weakto[i])
                                {
                                    case GameEvent.WeakTo.Water:
                                        _scriptMain._rightElementID[i] = 1;
                                        break;
                                    case GameEvent.WeakTo.Air:
                                        _scriptMain._rightElementID[i] = 2;
                                        break;
                                    case GameEvent.WeakTo.Earth:
                                        _scriptMain._rightElementID[i] = 3;
                                        break;
                                    case GameEvent.WeakTo.Sand:
                                        _scriptMain._rightElementID[i] = 4;
                                        break;
                                    case GameEvent.WeakTo.Snow:
                                        _scriptMain._rightElementID[i] = 5;
                                        break;
                                    case GameEvent.WeakTo.Mud:
                                        _scriptMain._rightElementID[i] = 6;
                                        break;
                                }
                            }
                            break;
                    }



                    switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._eventType)
                    {
                        case GameEvent.EventType.Bridge:
                            _scriptMain._onEventID = 1;
                            break;
                        case GameEvent.EventType.Lagoon:
                            _scriptMain._onEventID = 2;
                            evento.GetComponent<WaterFallEvent>()._scriptMain = _scriptMain;
                            //evento.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                            break;
                        case GameEvent.EventType.Well:
                            _scriptMain._onEventID = 3;
                            break;
                        case GameEvent.EventType.StrongAir:
                            _scriptMain._onEventID = 4;
                            break;
                        case GameEvent.EventType.FallingBridge:
                            _scriptMain._onEventID = 5;
                            break;
                        case GameEvent.EventType.Gears:
                            _scriptMain._onEventID = 6;
                            break;
                        case GameEvent.EventType.FightWasp:
                            evento.GetComponent<WaspFightScript>()._waspAnimator = _scriptMain._wasp;
                            _scriptMain._onEventID = 7;
                            break;

                    }
                    StartCoroutine(_scriptMain.StartStageNumerator());
                    break;
            case GameEvent.EventClassification.Questionary:
                    StartCoroutine(_scriptMain.StartStageQuestionary());
                    break;
        }
  
           
      

           
        
        }
    }
}
