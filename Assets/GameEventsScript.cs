using UnityEngine;

public class GameEventsScript : MonoBehaviour
{
    [SerializeField] private MainGameplayScript _scriptMain;
    [Header("Lista de eventos")]
    public GameEvent[] _specialEvents;   // Ahora es directamente GameEvent[]

    [Header("Padre de los objetos instanciados")]
    public Transform _eventosParent;
    public GameObject _currentEventPrefab;
  

    void Start()
    {
        // Ejemplo: instanciar el primer evento
        if (_specialEvents.Length > 0 && _specialEvents[0]._eventPrefab != null)
        {
            GameObject evento = Instantiate(
                _specialEvents[0]._eventPrefab,
                transform.position,
                transform.rotation,
                _eventosParent   // opcional: lo hace hijo directo
            );

            evento.transform.parent = _eventosParent.transform;
            evento.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            evento.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 90);
            _currentEventPrefab = evento;

            switch (_specialEvents[0]._weakto)
            {
                case GameEvent.WeakTo.Water:
                    _scriptMain._rightElementID = 1;
                    break;
                case GameEvent.WeakTo.Air:
                    _scriptMain._rightElementID = 2;
                    break;
                case GameEvent.WeakTo.Earth:
                    _scriptMain._rightElementID = 3;
                    break;
                case GameEvent.WeakTo.Sand:
                    _scriptMain._rightElementID = 4;
                    break;
                case GameEvent.WeakTo.Snow:
                    _scriptMain._rightElementID = 5;
                    break;
                case GameEvent.WeakTo.Mud:
                    _scriptMain._rightElementID = 6;
                    break;
            }


            switch (_specialEvents[0]._eventType)
            {
                case GameEvent.EventType.Bridge:
                    _scriptMain._onEventID = 1;
                    break;
  
            }


            Debug.Log($"Evento instanciado: {_specialEvents[0]._eventType} débil contra {_specialEvents[0]._weakto}");
        }
    }
}
