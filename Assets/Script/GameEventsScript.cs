using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LoLSDK;

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
    public Transform[] _eventosParent;
    public GameObject _currentEventPrefab;
    public GameObject[] _enemiesGameObjects;

    public int _onEvent;

    public Animator _stage1Animator;
    public GameObject _bossRender;

    [System.Serializable]
    public class CenterDialogeAssets
    {
        public GameObject _parent;
        public TextMeshProUGUI _dialogeText;
        public Image _princessImage;
        public Sprite[] _princessSprites;
        public float typingSpeed = 0.05f; // Velocidad de escritura (segundos entre letras)

    }
    public CenterDialogeAssets _centerDialogeAssets;

   
    public bool _enter;

    void Start()
    {
        for(int i = 0; i < _enemiesGameObjects.Length; i++)
        {
            _enemiesGameObjects[i].gameObject.SetActive(false);
        }

        switch (_scriptMain._scriptMain._onWorldGlobal)
        {
            case 0:

                _scriptMain._GamesList.Add(11);
                _scriptMain._GamesList.Add(10);
                _scriptMain._GamesList.Add(2);
                _scriptMain._GamesList.Add(12);
                _scriptMain._GamesList.Add(8);
                _scriptMain._GamesList.Add(13);
                StartCoroutine(StartLevelNumerator()); 
                break;
            case 1:
          
                _scriptMain._GamesList.Add(11);
                for (int i = 0; i < _scriptMain._totalStages._total; i++)
                {
                    var randomEvent = GetRandomEvent();
                    // Guardamos el índice dentro del array original
                    _scriptMain._GamesList.Add(System.Array.IndexOf(_specialEvents, randomEvent));
                }
                StartCoroutine(StartLevelNumerator());

                break;
            case 2:
                _scriptMain._GamesList.Add(11);
                for (int i = 0; i < _scriptMain._totalStages._total; i++)
                {
                    var randomEvent = GetRandomEvent();
                    // Guardamos el índice dentro del array original
                    _scriptMain._GamesList.Add(System.Array.IndexOf(_specialEvents, randomEvent));
                }
                StartCoroutine(StartLevelNumerator());

                break;
            case 3:                  
                _scriptMain._GamesList.Add(2);
                _scriptMain._GamesList.Add(8);
                _scriptMain._GamesList.Add(12);
                //for (int i = 0; i < _scriptMain._totalStages._total; i++)
                //{
                //    var randomEvent = GetRandomEvent();
                //    // Guardamos el índice dentro del array original
                //    _scriptMain._GamesList.Add(System.Array.IndexOf(_specialEvents, randomEvent));
                //}
                StartCoroutine(StartLevelNumerator());

                break;
        }

  

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

    public IEnumerator StartLevelNumerator()
    {
        _scriptMain._scriptFusion.UnlockElements();
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
                _eventosParent[_scriptMain._scriptMain._onWorldGlobal]
            );

            evento.transform.parent = _eventosParent[_scriptMain._scriptMain._onWorldGlobal].transform;
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
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(true);
                    switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._eventType)
                    {
                        case GameEvent.EventType.Bridge: _scriptMain._onEventID = 1; evento.GetComponent<BridgeEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true); break;
                        case GameEvent.EventType.Lagoon:
                            _scriptMain._onEventID = 2;
                            evento.GetComponent<WaterFallEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                            evento.GetComponent<WaterFallEvent>()._scriptMain = _scriptMain;
                            break;
                        case GameEvent.EventType.Well: _scriptMain._onEventID = 3; evento.GetComponent<WaterFillEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true); break;
                        case GameEvent.EventType.StrongAir: _scriptMain._onEventID = 4; evento.GetComponent<StrongAirEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true); break;
                        case GameEvent.EventType.FallingBridge: _scriptMain._onEventID = 5; evento.GetComponent<SandCutEventScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true); break;
                        case GameEvent.EventType.Gears: _scriptMain._onEventID = 6; evento.GetComponent<GearsPrefabEventScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true); break;
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
                        case GameEvent.EventType.Fire:
                            _scriptMain._onEventID = 9;
                            evento.GetComponent<FireEventScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                            break;
                        case GameEvent.EventType.BossFight1:
                   
                            _scriptMain._onEventID = 10;
                            StartCoroutine(evento.GetComponent<BossFightsScript>().StartBossNumerator());                   
                            break;
                    }
                    if (!_scriptMain._firstStage)
                    {
                        yield return new WaitForSeconds(2);
                        _scriptMain._stageParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10f);
                        _scriptMain._fallingParticle.Play();
                        _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(true);
                        _scriptMain._firstStage = true;
                    }

                    StartCoroutine(_scriptMain.StartStageNumerator());
                    break;

                case GameEvent.EventClassification.Questionary:
                    evento.GetComponent<QuestionaryScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(_scriptMain._firstStage);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);

                    if (!_scriptMain._firstStage)
                    {
                        yield return new WaitForSeconds(2);
                        _scriptMain._stageParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10f);
                        _scriptMain._fallingParticle.Play();
                        _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(true);
                        _scriptMain._firstStage = true;
                    }
                    else
                    {
            
                        yield return new WaitForSeconds(1);
                        StartCoroutine(StartStageQuestionary());
                    }
                    //StartCoroutine(_scriptMain.StartStageQuestionary());
                    break;
                case GameEvent.EventClassification.Chest:
                    evento.GetComponent<ChestEventScript>()._scriptMain = _scriptMain;
                    evento.GetComponent<ChestEventScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(_scriptMain._firstStage);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);
                    if (!_scriptMain._firstStage){                 
                        yield return new WaitForSeconds(2);
                        _scriptMain._stageParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10f);
                        _scriptMain._fallingParticle.Play();
                        _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(true);
                        _scriptMain._firstStage = true;
                    }            
                    StartCoroutine(_scriptMain.StartsStageChest());               
                    break;
                case GameEvent.EventClassification.Intro:
                    evento.GetComponent<IntroEventScript>()._scriptMain = _scriptMain;
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(true);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);       
                    StartCoroutine(_scriptMain.IntroStageNumerator());
                    break;
          
            }
        }
    }

    public IEnumerator StartStageQuestionary()
    {
        _scriptMain._scriptMain._scriptInit.ShowQuestion();

        // Espera hasta que se reciba la respuesta
        //yield return new WaitUntil(() => _scriptMain._scriptMain._scriptInit.respuestaRecibida);
        yield return new WaitForSeconds(1);

        // Reinicia la variable para la próxima pregunta
        _scriptMain._scriptMain._scriptInit.respuestaRecibida = false;

        // Ahora puedes continuar con el juego, usando lastAnswerCorrect, lastAnswer, etc.
        if (_scriptMain._scriptMain._scriptInit.lastAnswerCorrect)
        {
            Debug.Log("✅ El jugador respondió correctamente!");
        }
        else
        {
            Debug.Log("❌ El jugador respondió incorrectamente.");
        }

        _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", false);

        yield return new WaitForSeconds(2);
        //_scriptMain._scriptSlime._WindBlocker.gameObject.SetActive(false);
        Destroy(_scriptMain._scriptEvents._currentEventPrefab);
        _scriptMain._scriptEvents._onEvent++;
        StartCoroutine(_scriptMain._scriptEvents.StartLevelNumerator());

        //_scriptFusion.ActivatePanel();
    }

}
