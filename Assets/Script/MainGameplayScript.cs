using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 👈 Necesario para eventos de escena

public class MainGameplayScript : MonoBehaviour
{
    public MainController _scriptMain;
   
    public GameObject _mainUI;
    public int _OnStation;
    public SlimeController _scriptSlime;
    public FusionScript _scriptFusion;
    public GameEventsScript _scriptEvents;
    public RythmFusionScript _scriptRythm;
    public int _onEventID;
    public int[] _rightElementID;
   
    public bool _snowBool;
    public Image _snowImage;
    public Animator _wasp;

    public ParticleSystem _fallParticle;


    [System.Serializable]
    public class TotalStages
    {
        public GameObject _SlimeIcon;
        public int _total;     
        public List<float> _xPoses = new List<float>();
    }
    public TotalStages _totalStages;

    public List<int> _GamesList = new List<int>();

    [System.Serializable]
    public class HearthAssets
    {
        public GameObject _parent;
        public Image[] _totalHearts;
        public Color[] _heartColors;
    }
    public HearthAssets _heartAssets;
    public int _totalLifes = 4;

    [System.Serializable]
    public class ItemGotPanel
    {
        public Animator _parent;
        public GameObject[] _itemObject;
        public TextMeshProUGUI _Message;
        public string key;
    }
    public ItemGotPanel _itemGotPanel;


    [System.Serializable]
    public class AllStageAssets
    {
        public GameObject _parentStage;
        public GameObject _backStage;
        public GameObject _frontStage;
    }
    public AllStageAssets[] _allStageAssets;
  
    


    public GameObject _topOptions;
    public bool _topOptionsOn;

    public Animator _bossAnimator;

    public bool _slimeFalling;
    public ParticleSystem _fallingParticle;

    public bool _firstStage;
    public GameObject _stageParent;

    public ParticleSystem[] _flyingSlimeParticles;
    public TextMeshProUGUI _choose2ElementsText;

    private void Awake()
    {
        _scriptMain = GameObject.Find("CanvasIndestructible/Main/MainController").GetComponent<MainController>();
        //_scriptLanguage = GameObject.Find("CanvasIndestructible/Main/LanguageManager").GetComponent<LanguageManager>();
    }


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        //UpdateWorldTexts();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        for(int i = 0; i < 3; i++)
        {
            _scriptFusion._elementsOptions[i]._unlocked = _scriptMain._saveLoadValues._elementsUnlocked[i];
        }

        switch (_scriptMain._onWorldGlobal)
        {
            case 0:
                _topOptionsOn = false;
                break;
            case 1:
            case 2:
            case 3:
                _topOptionsOn = true;
                break;
        }      
        }



    // Start is called before the first frame update
    void Start()
    {
        _totalStages._xPoses.Clear();
        StartNewWorld();
        _allStageAssets[_scriptMain._onWorldGlobal]._parentStage.SetActive(true);      
    }

    public void StartNewWorld()
    {
        int n = Mathf.Max(1, _totalStages._total); // avoid divide-by-zero

        for (int i = 0; i < n; i++)
        {
            float t = (n == 1) ? 0f : i / (n - 1f); // goes 0 → 1
            float x = Mathf.Lerp(0f, 150f, t);      // goes 0 → 150
            _totalStages._xPoses.Add(x);

        }
    }



    // Update is called once per frame
    void Update()
    {
        // 🔹 Actualizaciones del gameplay SOLO si no está pausado
        if (!_scriptMain._pauseAssets._pause)
        {
            // Nieve
            Color targetSnow = _snowBool ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 0f);
            _snowImage.color = Color.Lerp(_snowImage.color, targetSnow, 2 * Time.deltaTime);

            // Slime icon
            _totalStages._SlimeIcon.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(
                _totalStages._SlimeIcon.GetComponent<RectTransform>().anchoredPosition,
                new Vector2(_totalStages._xPoses[_scriptEvents._onEvent], 4), 5 * Time.deltaTime);

            // Top options
            _topOptions.SetActive(_topOptionsOn);

            // Stage backgrounds
            for (int i = 0; i < _allStageAssets.Length; i++)
            {
                _allStageAssets[i]._backStage.GetComponent<RectTransform>().anchoredPosition = Vector2.MoveTowards(
                    _allStageAssets[i]._backStage.GetComponent<RectTransform>().anchoredPosition,
                    new Vector2(0, 0), 30 * Time.deltaTime);
            }

            // Slime cayendo
            if (_slimeFalling)
            {
                _scriptFusion._slimeRenderer.GetComponent<RectTransform>().anchoredPosition += Vector2.up * (-150 * Time.deltaTime);
            }

            // Stage parent y UI principal
            _stageParent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(
                _stageParent.GetComponent<RectTransform>().anchoredPosition,
                new Vector2(0, 0), 15 * Time.deltaTime);

            _mainUI.transform.localScale = Vector2.Lerp(_mainUI.transform.localScale, new Vector2(1, 1), 5 * Time.deltaTime);
        }

        // 🔹 Comprobar si se presionó el botón de pausa
        if (Input.GetButtonDown("Pause"))
        {
            _scriptMain.SetPause();
        }

        // 🔹 Actualizaciones de UI de pausa (si está pausado)
        if (_scriptMain._pauseAssets._pause)
        {
            float h = Input.GetAxisRaw("Horizontal");
            if (h < 0) _scriptMain._pauseAssets._onPos = 0;
            if (h > 0) _scriptMain._pauseAssets._onPos = 1;

            // Mover puntero del menú de pausa
            _scriptMain._pauseAssets._pointer.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(_scriptMain._pauseAssets._options[_scriptMain._pauseAssets._onPos].GetComponent<RectTransform>().anchoredPosition.x,
                _scriptMain._pauseAssets._options[_scriptMain._pauseAssets._onPos].GetComponent<RectTransform>().anchoredPosition.y - 30f);

            if (Input.GetButtonDown("Submit"))
            {
                switch (_scriptMain._pauseAssets._onPos)
                {
                    case 0:
                        _scriptMain.SetPause();
                        break;
                    case 1:
                        _scriptMain.SetPause();
                        break;
                }
              
            }
        }
    }


    public IEnumerator StartStageNumerator()
    {
        yield return new WaitForSeconds(1);
        _scriptMain._bordersAnimator.SetBool("BorderOut", true);
        _scriptMain._cinematicBorders.SetBool("FadeIn", false);
        yield return new WaitForSeconds(2);
        switch (_scriptEvents._specialEvents[_GamesList[_scriptEvents._onEvent]]._eventClassification)
        {
            case GameEvent.EventClassification.Normal:
                StartCoroutine(_scriptRythm.RythmNumerator());
                break;         
        }


        //StartCoroutine(_scriptFusion.ActivatePanel());
    }

    //public IEnumerator StartStageQuestionary()
    //{
    //    yield return new WaitForSeconds(1);
    //    _scriptMain._bordersAnimator.SetBool("BorderOut", true);
    //    yield return new WaitForSeconds(2);
    //    Debug.Log("Questionario");
    //    _scriptMain._bordersAnimator.SetBool("BorderOut", false);

    //    yield return new WaitForSeconds(2);
    //    _scriptSlime._WindBlocker.gameObject.SetActive(false);
    //    Destroy(_scriptEvents._currentEventPrefab);
    //    _scriptEvents._onEvent++;
    //    StartCoroutine(_scriptEvents.StartLevelNumerator());

    //    //_scriptFusion.ActivatePanel();
    //}

    public IEnumerator StartsStageChest()
    {  
        yield return new WaitForSeconds(1);
        _scriptEvents._currentEventPrefab.GetComponent<ChestEventScript>()._chestAnimator.SetTrigger("ChestOpen");
        yield return new WaitForSeconds(2);
        _scriptEvents._currentEventPrefab.GetComponent<ChestEventScript>().ItemGet();
 
       _itemGotPanel._parent.SetTrigger("ItemGot");
        StartCoroutine(ExitNumerator());

    }

    public IEnumerator IntroStageNumerator()
    {  
        for (int i = 0; i < _allStageAssets.Length; i++)
        {
            _allStageAssets[i]._backStage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -600f);
        }
        _allStageAssets[_scriptMain._onWorldGlobal]._frontStage.SetActive(false);
        _scriptMain._cinematicBorders.SetBool("FadeIn", true);  
        _scriptSlime._slimeAnimator.SetBool("Falling", true);
        _scriptEvents._currentEventPrefab.GetComponent<IntroEventScript>().StartIntroVoid();
        _fallParticle.gameObject.SetActive(true);
        _scriptMain._bordersAnimator.SetBool("BorderOut", true);
        yield return new WaitForSeconds(2);
        //_bossAnimator.SetTrigger("Flies");
        yield return new WaitForSeconds(2);
        _slimeFalling = true;
   
        StartCoroutine(ExitNumerator());

    }

    public IEnumerator ExitNumerator()
    {
        yield return new WaitForSeconds(2);
        _scriptSlime._materialColors[1] = _scriptSlime._slimeAssets[0]._mainColor;
        _scriptSlime._materialColors[2] = _scriptSlime._slimeAssets[0]._mainColor;
        _scriptSlime.fillAmount = 0;
        _scriptMain._bordersAnimator.SetBool("BorderOut", false);

        yield return new WaitForSeconds(2);
        if(_scriptEvents._currentEventPrefab.name == "Intro(Clone)")
        {
            _scriptEvents._currentEventPrefab.GetComponent<IntroEventScript>().ExitIntroVoid();
        }

       // _scriptSlime._WindBlocker.gameObject.SetActive(false);
        _allStageAssets[_scriptMain._onWorldGlobal]._frontStage.SetActive(true);
        _scriptMain._cinematicBorders.SetBool("FadeIn", false);
        _scriptSlime._slimeAnimator.SetBool("Falling", false);
        _fallParticle.gameObject.SetActive(false);
        _slimeFalling = false;
   

        for (int i = 0; i < _allStageAssets.Length; i++)
        {
            _allStageAssets[i]._backStage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }
        _scriptEvents._onEvent++;
        Destroy(_scriptEvents._currentEventPrefab);
        StartCoroutine(_scriptEvents.StartLevelNumerator());
    }

    public void LoseHeartVoid()
    {
        _totalLifes--;
        for(int i = 0; i < _heartAssets._totalHearts.Length; i++)
        {
            _heartAssets._totalHearts[i].color = _heartAssets._heartColors[1];
        }

        for (int i = 0; i < _totalLifes; i++)
        {
            _heartAssets._totalHearts[i].color = _heartAssets._heartColors[0];
        }
    }

    //public void UpdateElementText()
    //{     
    //   if (_itemGotPanel._Message != null && !string.IsNullOrEmpty(_itemGotPanel.key))
    //   {
    //    _itemGotPanel._Message.text = LanguageManager.Instance.GetText(_elementsInfo[i].key);
    //    }      
    //}
}
