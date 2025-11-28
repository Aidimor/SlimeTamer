using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameEventsScript : MonoBehaviour
{
    public static GameEventsScript Instance;
    [SerializeField] private MainGameplayScript _scriptMain;
    [SerializeField] private QuestionHandler _questionHandler;
    [Header("Lista de eventos")]
    public GameEvent[] _specialEvents;   // Ahora es directamente GameEvent[]

    [Header("Chances por clasificación (suman 1.0 aprox)")]
    [Range(0f, 1f)] public float chanceNormal = 0.6f;
    [Range(0f, 1f)] public float chanceFight = 0.3f;
    [Range(0f, 1f)] public float chanceQuestionary = 0.1f;

    [Header("Padre de los objetos instanciados")]
    public Transform[] _eventosParent;
    public GameObject _currentEventPrefab;
    //public GameObject[] _enemiesGameObjects;

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
    public ParticleSystem _cascadeParticle;
    public ParticleSystem _windParticle;

    public void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        switch (_scriptMain._scriptMain._onWorldGlobal)
        {
            case 0:
                switch (_scriptMain._scriptMain._saveLoadValues._finalWorldUnlocked)
                {
                    case false:
                        //_scriptMain._GamesList.Add(0);
                        ////_scriptMain._GamesList.Add(8);
                        //_scriptMain._GamesList.Add(15);

                        //_scriptMain._GamesList.Add(17);
                        //_scriptMain._GamesList.Add(18);
                        //_scriptMain._GamesList.Add(16);    
                        //_scriptMain._GamesList.Add(13);


                        _scriptMain._GamesList.Add(0);
                        _scriptMain._GamesList.Add(11);
                        _scriptMain._GamesList.Add(20);
                        _scriptMain._GamesList.Add(6);
                        _scriptMain._GamesList.Add(3);
                        //_scriptMain._GamesList.Add(1);
                        _scriptMain._GamesList.Add(13);
                        break;
                    case true:
                        _scriptMain._GamesList.Add(0);
                        List<int> posiblesNumeros0 = new List<int> { 4, 5, 7, 15, 17, 18 };
                        for (int i = 0; i < 10; i++)
                        {
                            int randomIndex = Random.Range(0, posiblesNumeros0.Count);
                            _scriptMain._GamesList.Add(posiblesNumeros0[randomIndex]);
                        }
                        _scriptMain._GamesList.Add(16);
                        break;
                }

                StartCoroutine(StartLevelNumerator()); 
                break;
            case 1:        
         
                _scriptMain._GamesList.Add(0);

                //_scriptMain._GamesList.Add(1);
                _scriptMain._GamesList.Add(15);
                _scriptMain._GamesList.Add(3);
                _scriptMain._GamesList.Add(4);
                _scriptMain._GamesList.Add(5);
                _scriptMain._GamesList.Add(6);
                _scriptMain._GamesList.Add(7);
                List<int> posiblesNumeros1 = new List<int> { 3, 4, 5, 6, 7, 8, 9, 10, 15, 17, 18 };
                for (int i = 0; i < 4; i++)
                {
                    int randomIndex = Random.Range(0, posiblesNumeros1.Count);
                    _scriptMain._GamesList.Add(posiblesNumeros1[randomIndex]);
                }
                _scriptMain._GamesList.Add(2);
                StartCoroutine(StartLevelNumerator());

                break;
            case 2:
                _scriptMain._GamesList.Add(0);
                _scriptMain._GamesList.Add(9);
                //_scriptMain._GamesList.Add(1);
                _scriptMain._GamesList.Add(3);
                //_scriptMain._GamesList.Add(1);
                _scriptMain._GamesList.Add(19);
                _scriptMain._GamesList.Add(4);
                _scriptMain._GamesList.Add(8);
                _scriptMain._GamesList.Add(3);
                _scriptMain._GamesList.Add(15);
                // Aleatorios de un conjunto
                List<int> posiblesNumeros3 = new List<int> { 3, 4, 7, 8, 9, 15 };
                for (int i = 0; i < 4; i++)
                {
                    int randomIndex = Random.Range(0, posiblesNumeros3.Count);
                    _scriptMain._GamesList.Add(posiblesNumeros3[randomIndex]);
                }
                _scriptMain._GamesList.Add(2);
                StartCoroutine(StartLevelNumerator());

                break;
            case 3:
                _scriptMain._GamesList.Add(0);
                //_scriptMain._GamesList.Add(6);
                ////_scriptMain._GamesList.Add(1);
                //_scriptMain._GamesList.Add(3);
                //_scriptMain._GamesList.Add(18);  
                _scriptMain._GamesList.Add(12);
                _scriptMain._GamesList.Add(7);
                _scriptMain._GamesList.Add(3);
                //_scriptMain._GamesList.Add(1);
                _scriptMain._GamesList.Add(10);
                _scriptMain._GamesList.Add(3);
                //_scriptMain._GamesList.Add(1);
                _scriptMain._GamesList.Add(9);
                _scriptMain._GamesList.Add(2);
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
        //_scriptMain._scriptFusion.UnlockElements();
 
        //for (int i = 0; i < _enemiesGameObjects.Length; i++)
        //{
        //    _enemiesGameObjects[i].gameObject.SetActive(false);
        //}

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
                    _scriptMain._scriptMain._pauseAssets._hintAvailable = true;
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
                    _scriptMain._scriptSlime._slimeParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, -207);
              
                    switch (_specialEvents[_scriptMain._GamesList[_onEvent]]._eventType)
                    {
                        case GameEvent.EventType.Bridge:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                            evento.GetComponent<BridgeEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                            _scriptMain._onEventID = 1;                          
                            break;
                        case GameEvent.EventType.Lagoon:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                            _scriptMain._onEventID = 2;
                            evento.GetComponent<WaterFallEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                            evento.GetComponent<WaterFallEvent>()._scriptMain = _scriptMain;
                            _cascadeParticle.gameObject.SetActive(true);
                            break;
                        case GameEvent.EventType.Well:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                            _scriptMain._onEventID = 3;
                            evento.GetComponent<WaterFillEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);           

                            break;
                        case GameEvent.EventType.StrongAir:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                            _scriptMain._scriptMain._scriptSFX._strongWindSetVolume = 0.05f;
                            _windParticle.Play();
                            //_scriptMain._scriptSlime._slimeAnimator.SetBool("WindPush", true);
                            _scriptMain._onEventID = 4;
                            evento.GetComponent<StrongAirEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);                 
                            _scriptMain._windParticle.Play();
                            break;
                        case GameEvent.EventType.FallingBridge:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1); _scriptMain._onEventID = 5; evento.GetComponent<SandCutEventScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true); break;
                        case GameEvent.EventType.Gears:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                            _scriptMain._onEventID = 6;
                            evento.GetComponent<GearsPrefabEventScript>()._stainsAnimator.GetComponent<SpecialAnimatorEvents>()._scriptMainController = _scriptMain._scriptMain;
                            evento.GetComponent<GearsPrefabEventScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true); 
                            break;
                        //case GameEvent.EventType.FightWasp:
                        //    _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                        //    _enemiesGameObjects[0].gameObject.SetActive(true);
                        //    evento.GetComponent<WaspFightScript>()._waspAnimator = _enemiesGameObjects[0].GetComponent<Animator>();
                        //    _scriptMain._onEventID = 7;
                        //    break;
                        //case GameEvent.EventType.FightSnail:
                        //    _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                        //    _enemiesGameObjects[1].gameObject.SetActive(true);
                        //    evento.GetComponent<SnailFightScript>()._snailAnimator = _enemiesGameObjects[1].GetComponent<Animator>();
                        //    _scriptMain._onEventID = 8;
                        //    break;
                        case GameEvent.EventType.Fire:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);

                            _scriptMain._scriptMain._scriptSFX._fireSetVolume = 1;
                            _scriptMain._onEventID = 9;
                            evento.GetComponent<FireEventScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);

                            break;
                        case GameEvent.EventType.BossFight0:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(5);
                            _scriptMain._onEventID = 10;
                            evento.GetComponent<BossFightsScript>().StartBossVoid();
              
                            break;
                        case GameEvent.EventType.BossFight1:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(5);
                            _scriptMain._onEventID = 11;
                            _scriptMain._Cascade[0].gameObject.SetActive(true);
                            _scriptMain._Cascade[0].Play();
                            for(int i = 0; i < 4; i++)
                            {
                                _scriptMain._cascadeWorlds[i].SetActive(false);
                            }
                            _scriptMain._cascadeWorlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                            evento.GetComponent<BossFightsScript>().StartBossVoid();
                            break;
                        case GameEvent.EventType.BossFight2:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(5);
                            _scriptMain._onEventID = 11;
                            evento.GetComponent<BossFightsScript>()._events[2].GetComponent<SpecialAnimatorEvents>()._scriptMain = _scriptMain;
                            evento.GetComponent<BossFightsScript>().StartBossVoid();
                      
                            break;
                        case GameEvent.EventType.BossFight3:
                            _scriptMain._scriptMain._scriptMusic.PlayMusic(5);
                            _scriptMain._onEventID = 11;
                   
                            evento.GetComponent<BossFightsScript>().StartBossVoid();
                            break;
                        case GameEvent.EventType.BossFight4:

                            _scriptMain._scriptMain._scriptMusic.PlayMusic(5);
                            _scriptMain._onEventID = 11;
                            evento.GetComponent<BossFightsScript>().StartBossVoid();
                            break;
                         
                    }
                    StartCoroutine(_scriptMain.StartStageNumerator());

                    break;
                case GameEvent.EventClassification.Questionary:

                    _scriptMain._scriptSlime._slimeParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -207);
                    _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                    evento.GetComponent<QuestionaryScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(_scriptMain._firstStage);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);
              

                    yield return new WaitForSeconds(1);
                    _scriptMain._scriptSlime._slimeAnimator.SetBool("Moving", false);

                    // Llama al QuestionHandler para mostrar la pregunta y reportar progreso
                    _questionHandler.StartStageQuestionary();
                    break;


                case GameEvent.EventClassification.Chest:           
                    _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                    evento.GetComponent<ChestEventScript>()._scriptMain = _scriptMain;
                    evento.GetComponent<ChestEventScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(_scriptMain._firstStage);
                    _scriptMain._scriptSlime._slimeParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -207);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);       
                    StartCoroutine(_scriptMain.StartsStageChest());               
                    break;
                case GameEvent.EventClassification.Intro:
                    _scriptMain._scriptSlime._slimeParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-525, -500);                  
                    _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                    evento.GetComponent<IntroEventScript>()._scriptMain = _scriptMain;
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(true);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);       
                    StartCoroutine(_scriptMain.IntroStageNumerator());
                    break;
          
                case GameEvent.EventClassification.Shop:

                    _scriptMain._scriptSlime._slimeParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -207);
                    _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                    evento.GetComponent<ShopScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    //evento.GetComponent<QuestionaryScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    _scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(true);
                    //_scriptMain._scriptFusion._slimeRenderer.gameObject.SetActive(_scriptMain._firstStage);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);


                    yield return new WaitForSeconds(1);
                    _scriptMain._scriptSlime._slimeAnimator.SetBool("Moving", false);
                    StartCoroutine(_scriptMain.StartsShopNumerator());
                    break;
                case GameEvent.EventClassification.Teleporter:
                    _scriptMain._scriptSlime._slimeParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -207);
                    _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                    evento.GetComponent<TeleporterScript>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    yield return new WaitForSeconds(1);
                    _scriptMain._scriptSlime._slimeAnimator.SetBool("Moving", false);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);
                    StartCoroutine(_scriptMain.StartsTeleporterAnimator());
       
                    break;
                case GameEvent.EventClassification.Tutorial:
                    _scriptMain._scriptSlime._slimeParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, -207);
                    _scriptMain._scriptMain._pauseAssets._hintAvailable = true;
                    _scriptMain._onTutorial = true;
                    _scriptMain._scriptMain._scriptMusic.PlayMusic(_scriptMain._scriptMain._onWorldGlobal + 1);
                    _scriptMain._rightElementID[0] = 1;
                    _scriptMain._onEventID = 3;
                    evento.GetComponent<WaterFillEvent>()._worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);
                    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", true);
                    StartCoroutine(_scriptMain.StartStageNumerator());

            
                    break;
          
            }
        }


    }

}
