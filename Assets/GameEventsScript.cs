using UnityEngine;
using System.Collections.Generic;

public class GameEventsScript : MonoBehaviour
{
    [SerializeField] private MainGameplayScript _scriptMain;
    [Header("Lista de eventos")]
    public GameEvent[] _specialEvents;   // Ahora es directamente GameEvent[]

    [Header("Chances por clasificación (suman 1.0 aprox)")]
    [Range(0f, 1f)] public float chanceNormal = 0.6f;
    [Range(0f, 1f)] public float chanceFight = 0.3f;
    [Range(0f, 1f)] public float chanceQuestionary = 0.1f;

    [Header("Padre de los objetos instanciados")]
    public Transform _eventosParent;
    public GameObject _currentEventPrefab;
    public GameObject[] _enemiesGameObjects;

    public int _onEvent;

    void Start()
    {
        for(int i = 0; i < _enemiesGameObjects.Length; i++)
        {
            _enemiesGameObjects[i].gameObject.SetActive(false);
        }

    
        for (int i = 0; i < _scriptMain._totalStages._total; i++)
        {
            var randomEvent = GetRandomEvent();
            // Guardamos el índice dentro del array original
            _scriptMain._GamesList.Add(System.Array.IndexOf(_specialEvents, randomEvent));
        }

        StartLevel();
    }

    private GameEvent GetRandomEvent()
    {
        // Ruleta de probabilidades
        float roll = Random.value;
        GameEvent.EventClassification chosenType;

        if (roll <= chanceNormal)
            chosenType = GameEvent.EventClassification.Normal;
        else if (roll <= chanceNormal + chanceFight)
            chosenType = GameEvent.EventClassification.Fight;
        else
            chosenType = GameEvent.EventClassification.Questionary;

        // Filtrar solo los eventos de esa clasificación
        var filtered = new List<GameEvent>();
        foreach (var ev in _specialEvents)
        {
            if (ev._eventClassification == chosenType)
                filtered.Add(ev);
        }

        // Si no hay ninguno de ese tipo, usar todos como fallback
        if (filtered.Count == 0)
            filtered.AddRange(_specialEvents);

        // Devolver uno random de los filtrados
        return filtered[Random.Range(0, filtered.Count)];
    }

    public void StartLevel()
    {
        for (int i = 0; i < _enemiesGameObjects.Length; i++)
        {
            _enemiesGameObjects[i].gameObject.SetActive(false);
        }

        if (_specialEvents.Length > 0 && _specialEvents[_scriptMain._GamesList[_onEvent]]._eventPrefab != null)
        {
            GameObject evento = Instantiate(
                _specialEvents[_scriptMain._GamesList[_onEvent]]._eventPrefab,
                transform.position,
                transform.rotation,
                _eventosParent
            );

            evento.transform.parent = _eventosParent.transform;
            evento.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            evento.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 90);
            _currentEventPrefab = evento;

            switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._eventClassification)
            {
                case GameEvent.EventClassification.Normal:
                case GameEvent.EventClassification.Fight:
                    switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._weakto.Length)
                    {
                        case 1:
                            switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._weakto[0])
                            {
                                case GameEvent.WeakTo.Water: _scriptMain._rightElementID[0] = 1; break;
                                case GameEvent.WeakTo.Air: _scriptMain._rightElementID[0] = 2; break;
                                case GameEvent.WeakTo.Earth: _scriptMain._rightElementID[0] = 3; break;
                                case GameEvent.WeakTo.Sand: _scriptMain._rightElementID[0] = 4; break;
                                case GameEvent.WeakTo.Snow: _scriptMain._rightElementID[0] = 5; break;
                                case GameEvent.WeakTo.Mud: _scriptMain._rightElementID[0] = 6; break;
                            }
                            break;
                        case 2:
                            for (int i = 0; i < 2; i++)
                            {
                                switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._weakto[i])
                                {
                                    case GameEvent.WeakTo.Water: _scriptMain._rightElementID[i] = 1; break;
                                    case GameEvent.WeakTo.Air: _scriptMain._rightElementID[i] = 2; break;
                                    case GameEvent.WeakTo.Earth: _scriptMain._rightElementID[i] = 3; break;
                                    case GameEvent.WeakTo.Sand: _scriptMain._rightElementID[i] = 4; break;
                                    case GameEvent.WeakTo.Snow: _scriptMain._rightElementID[i] = 5; break;
                                    case GameEvent.WeakTo.Mud: _scriptMain._rightElementID[i] = 6; break;
                                }
                            }
                            break;
                    }

                    switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._eventType)
                    {
                        case GameEvent.EventType.Bridge: _scriptMain._onEventID = 1; break;
                        case GameEvent.EventType.Lagoon:
                            _scriptMain._onEventID = 2;
                            evento.GetComponent<WaterFallEvent>()._scriptMain = _scriptMain;
                            break;
                        case GameEvent.EventType.Well: _scriptMain._onEventID = 3; break;
                        case GameEvent.EventType.StrongAir: _scriptMain._onEventID = 4; break;
                        case GameEvent.EventType.FallingBridge: _scriptMain._onEventID = 5; break;
                        case GameEvent.EventType.Gears: _scriptMain._onEventID = 6; break;
                        case GameEvent.EventType.FightWasp:
                            _enemiesGameObjects[0].gameObject.SetActive(true);
                            evento.GetComponent<WaspFightScript>()._waspAnimator = _enemiesGameObjects[0].GetComponent<Animator>();
                            _scriptMain._onEventID = 7;
                            break;
                        case GameEvent.EventType.FightSnail:
                            _enemiesGameObjects[1].gameObject.SetActive(true);
                            evento.GetComponent<SnailFightScript>()._snailAnimator = _enemiesGameObjects[1].GetComponent<Animator>();
                            _scriptMain._onEventID = 8;
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
