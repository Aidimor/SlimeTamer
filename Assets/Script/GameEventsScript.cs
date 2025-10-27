using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LoLSDK;
using LoL;

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
    public bool _winRound;

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
    public ParticleSystem _rainParticle;


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
                _scriptMain._GamesList.Add(13);
                _scriptMain._GamesList.Add(8);
          
                StartCoroutine(StartLevelNumerator()); 
                break;
            case 1:
          
                _scriptMain._GamesList.Add(11);
                for (int i = 0; i < _scriptMain._GamesList.Count; i++)
                {
                    var randomEvent = GetRandomEvent();
                    // Guardamos el índice dentro del array original
                    _scriptMain._GamesList.Add(System.Array.IndexOf(_specialEvents, randomEvent));
                }
                StartCoroutine(StartLevelNumerator());

                break;
            case 2:
                _scriptMain._GamesList.Add(11);
                for (int i = 0; i < _scriptMain._GamesList.Count; i++)
                {
                    var randomEvent = GetRandomEvent();
                    // Guardamos el índice dentro del array original
                    _scriptMain._GamesList.Add(System.Array.IndexOf(_specialEvents, randomEvent));
                }
                StartCoroutine(StartLevelNumerator());

                break;
            case 3:
                _scriptMain._GamesList.Add(11);
                _scriptMain._GamesList.Add(2);
                _scriptMain._GamesList.Add(19);
                //_scriptMain._GamesList.Add(2);
                //_scriptMain._GamesList.Add(8);
                //_scriptMain._GamesList.Add(9);
                //_scriptMain._GamesList.Add(3);
                //_scriptMain._GamesList.Add(2);
                //_scriptMain._GamesList.Add(5);
                //_scriptMain._GamesList.Add(17);
                //_scriptMain._GamesList.Add(8);
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

        _scriptMain._scriptSlime._slimeAnimator.SetBool("WindPush", false);
        _scriptMain._scriptEvents._winRound = false;
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
            //_scriptMain._topOptionsOn = _scriptMain._firstStage;     

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
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(_scriptMain._firstStage);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);
           
                    _scriptMain._scriptFusion._slimeRenderer.GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, -200);
                    switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._eventType)
                    {
                        case GameEvent.EventType.Bridge:
                            evento.GetComponent<BridgeEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                            _scriptMain._onEventID = 1; 
                            //evento.GetComponent<BridgeEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                            break;
                        case GameEvent.EventType.Lagoon:
                            _scriptMain._onEventID = 2;
                            evento.GetComponent<WaterFallEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                            evento.GetComponent<WaterFallEvent>()._scriptMain = _scriptMain;
                            break;
                        case GameEvent.EventType.Well:
                            _scriptMain._onEventID = 3;
                            evento.GetComponent<WaterFillEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                  

                            break;
                        case GameEvent.EventType.StrongAir:
                            //_scriptMain._scriptSlime._slimeAnimator.SetBool("WindPush", true);
                            _scriptMain._onEventID = 4;
                            evento.GetComponent<StrongAirEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true); 
                            _scriptMain._scriptMain._windParticle.Play();
                            break;
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
                        case GameEvent.EventType.BossFight0:                   
                            _scriptMain._onEventID = 10;
                            evento.GetComponent<BossFightsScript>().StartBossVoid();
                            //StartCoroutine(evento.GetComponent<BossFightsScript>().StartBossNumerator());
                            //StartCoroutine(StartBossNumerator());
                            break;
                        case GameEvent.EventType.BossFight1:
                            _scriptMain._onEventID = 11;
                            _scriptMain._Cascade[0].gameObject.SetActive(true);
                            _scriptMain._Cascade[0].Play();
                            evento.GetComponent<BossFightsScript>().StartBossVoid();
                            break;
                        case GameEvent.EventType.BossFight2:
                            _scriptMain._onEventID = 11;
                            evento.GetComponent<BossFightsScript>()._events[2].GetComponent<SpecialAnimatorEvents>()._scriptMain = _scriptMain;
                            evento.GetComponent<BossFightsScript>().StartBossVoid();
                      
                            break;
                        case GameEvent.EventType.BossFight3:
                            _scriptMain._onEventID = 11;
                   
                            evento.GetComponent<BossFightsScript>().StartBossVoid();
                            break;
                        case GameEvent.EventType.BossFight4:
                            _scriptMain._onEventID = 11;
                            evento.GetComponent<BossFightsScript>().StartBossVoid();
                            break;
                         
                    }
                    StartCoroutine(_scriptMain.StartStageNumerator());

                    break;

                case GameEvent.EventClassification.Questionary:
                    evento.GetComponent<QuestionaryScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(_scriptMain._firstStage);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);
                    _scriptMain._scriptFusion._slimeRenderer.GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, -200);
        
                        yield return new WaitForSeconds(1);
                        _scriptMain._scriptSlime._slimeAnimator.SetBool("Moving", false);
                        StartCoroutine(StartStageQuestionary());
        
                    break;
                case GameEvent.EventClassification.Chest:
                    evento.GetComponent<ChestEventScript>()._scriptMain = _scriptMain;
                    evento.GetComponent<ChestEventScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(_scriptMain._firstStage);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);       
                    StartCoroutine(_scriptMain.StartsStageChest());               
                    break;
                case GameEvent.EventClassification.Intro:
                    evento.GetComponent<IntroEventScript>()._scriptMain = _scriptMain;
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(true);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);       
                    StartCoroutine(_scriptMain.IntroStageNumerator());
                    break;
                case GameEvent.EventClassification.Shop:
                    //evento.GetComponent<QuestionaryScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(true);
                    //_scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(_scriptMain._firstStage);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);
                    _scriptMain._scriptFusion._slimeRenderer.GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, -200);

                    yield return new WaitForSeconds(1);
                    _scriptMain._scriptSlime._slimeAnimator.SetBool("Moving", false);
                    StartCoroutine(_scriptMain.StartsShopNumerator());
                    break;
          
            }
        }

        yield return new WaitForSeconds(1);

        switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._eventClassification)
        {
            case GameEvent.EventClassification.Normal:
                // Referencia al AudioSource
                //AudioSource bgm = _scriptMain._scriptMain._bgmAS;
                //Debug.Log(_specialEvents[_scriptMain._GamesList[_onEvent]]._eventType);
                //// Si no está reproduciendo, entonces asigna el clip y reproduce
                //if (!bgm.isPlaying)
                //{
                //    bgm.clip = _scriptMain._scriptMain._allBGM[_scriptMain._scriptMain._onWorldGlobal];
                //    bgm.Play();
                //}
                break;


        }
    }

    public IEnumerator RestartNumerator()
    {
 
       yield return null;
    }

    public IEnumerator StartStageQuestionary()
    {
        _scriptMain._scriptMain._scriptInit.ShowQuestion();
        _winRound = true;
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
        //_scriptMain._scriptEvents._onEvent++;
        StartCoroutine(_scriptMain.ExitNumerator());
       // StartCoroutine(_scriptMain.StartStageNumerator());

        //_scriptFusion.ActivatePanel();
    }

    //public IEnumerator StartBossNumerator()
    //{

    //    yield return new WaitForSeconds(1);
    //    _scriptMain._bossAnimator.transform.gameObject.SetActive(false);

    //    yield return new WaitForSeconds(2);  

    //    _scriptMain._bossAnimator.transform.gameObject.SetActive(true);
    //    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._roar);
    //    _scriptMain._bossAnimator.SetBool("Idle", true);
    //    _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", true);
    //    _scriptMain._scriptSlime._alarmParticle.Play();
    //    yield return new WaitForSeconds(2);
    //    _scriptMain._scriptMain._cinematicBorders.SetBool("FadeIn", true);
    //    yield return new WaitForSeconds(2f);
    //    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._whip);


    //    _scriptMain._bossAnimator.SetTrigger("Attack");
    //    _scriptMain._flyingSlimeParticles[0].Play();
    //    yield return new WaitForSeconds(0.5f);
    //    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._scream);
    //    yield return new WaitForSeconds(1.5f);

    //    _scriptMain._flyingSlimeParticles[1].Play();
    //    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._ding);
    //    yield return new WaitForSeconds(2);
    //    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", false);
    //    _scriptMain._scriptMain._introSpecial = true;

    //    _scriptMain._scriptMain._saveLoadValues._worldsUnlocked[0] = false;
    //    _scriptMain._scriptMain._saveLoadValues._worldsUnlocked[3] = true;

    //    yield return new WaitForSeconds(1);
    //    _scriptMain._bossAnimator.transform.gameObject.SetActive(false);
    //    _scriptMain._scriptMain._onWorldGlobal = 3;
    //    _scriptMain._bossAnimator.gameObject.SetActive(false);
    //    //_scriptMain._firstStage = true;

    //    //_scriptMain._scriptMain._scriptInit.SaveGame();
    //    _scriptMain._scriptMain.SaveProgress();

    //    _scriptMain._scriptMain.LoadSceneByName("IntroScene");


    //}

}
