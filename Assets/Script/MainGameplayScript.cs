using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 👈 Necesario para eventos de escena
using LoL;  // <- necesario para GameInitScript

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
    public ParticleSystem _slimeArriveParticle;

    [System.Serializable]
    public class TotalStages
    {
        public GameObject _SlimeIcon;
        //public int _total;     
        public List<float> _xPoses = new List<float>();
    }
    public TotalStages _totalStages;

    public List<int> _GamesList = new List<int>();

    //[System.Serializable]
    //public class HearthAssets
    //{
    //    public GameObject _parent;
    //    public Image[] _totalHearts;
    //    public Color[] _heartColors;
    //}
    //public HearthAssets _heartAssets;
    //public int _totalLifes = 4;

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
    //public ParticleSystem _fallingParticle;

    public bool _firstStage;
    public GameObject _stageParent;

    public ParticleSystem[] _flyingSlimeParticles;
    public TextMeshProUGUI _choose2ElementsText;
    public GameObject _spaceParent;
    public TextMeshProUGUI _spaceText;


    [System.Serializable]
    public class DialogeAssets
    {
        public string _linea;
        public Animator _dialogePanel;
        public TextMeshProUGUI _dialogeText;        
        public Sprite[] _allSprites;
        public GameObject _nextParent;
        public GameObject _nextCircle;
        public bool _startDialoge;
        public bool _typing;
        public Vector2 _dialogeSize;
    }
    public DialogeAssets _dialogeAssets;


    [System.Serializable]
    public class ShopAssets
    {
        public Animator _parent;
        public Image[] _background;
        public Image[] _coinSprite;
        public TextMeshProUGUI[] _nameText;
        public int _onOption;
        public bool _onShop;
        public Color[] _colors;
    }
    public ShopAssets _shopAssets;

    public Image _darkener;
    public Light _slimeLight;

    public GameObject _slimeParent;
    public ParticleSystem _shineParticle;

    public bool _eventOn;
    public bool _slimeChanging;
    public bool _lightChanging;
    public bool _darkenerChanging;

    public GameObject _shadow;
    public ParticleSystem _slimeExplosion;
    public bool _dead;
    public GameObject _windBlocker;
    public ParticleSystem _airPushParticle;
    public ParticleSystem _windBlockPalanca;
    public ParticleSystem _snowParticle;
    public ParticleSystem[] _cutParticles;
    public ParticleSystem _windParticle;

    public Color[] _cascadeColor;
    public ParticleSystem[] _Cascade;
    public bool _cascadeFrozen;

    public ParticleSystem _chargingAttackEnemy;
    public ParticleSystem _AttackEnemy;

    public ParticleSystem[] _bossCutParticles;
    public ParticleSystem _frontWindParticle;

  
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
                //_topOptionsOn = false;
                break;
            case 1:
            case 2:
            case 3:
                //_topOptionsOn = true;
                break;
        }

        //_totalStages._total = _GamesList.Count;
        }



    // Start is called before the first frame update
    void Start()
    {
        _totalStages._xPoses.Clear();
        StartNewWorld();
        _allStageAssets[_scriptMain._onWorldGlobal]._parentStage.SetActive(true);

        _shopAssets._nameText[0].text = GameInitScript.Instance.GetText("coin1");
        _shopAssets._nameText[1].text = GameInitScript.Instance.GetText("coin2");

        _scriptMain._pauseAssets._optionsText[0].text = GameInitScript.Instance.GetText("pause1");
        _scriptMain._pauseAssets._optionsText[1].text = GameInitScript.Instance.GetText("pause2");
        _scriptMain._pauseAssets._optionsText[2].text = GameInitScript.Instance.GetText("pause3");
    }

    public void StartNewWorld()
    {
        int n = Mathf.Max(1, _GamesList.Count); // avoid divide-by-zero

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



        // 🔹 Comprobar si se presionó el botón de pausa
        if (Input.GetButtonDown("Pause") && !_scriptMain._gameOverAssets._onGameOver)
        {
            _scriptMain.SetPause();
        }

        // 🔹 Actualizaciones de UI de pausa (si está pausado)
        if (_scriptMain._pauseAssets._pause)
        {
            float h = Input.GetAxisRaw("Horizontal");

            // 🔸 Movimiento a la izquierda
            if (h < 0 && _scriptMain._pauseAssets._onPos > 0 && !_scriptMain._pauseAssets._moved)
            {
                _scriptMain._pauseAssets._onPos--;
                _scriptMain._pauseAssets._moved = true;
            }

            // 🔸 Movimiento a la derecha
            if (h > 0 && _scriptMain._pauseAssets._onPos < _scriptMain._pauseAssets._options.Length - 1 && !_scriptMain._pauseAssets._moved)
            {
                _scriptMain._pauseAssets._onPos++;
                _scriptMain._pauseAssets._moved = true;
            }

            // 🔸 Resetear cuando se suelta el eje
            if (h == 0)
                _scriptMain._pauseAssets._moved = false;

            // 🔸 Mover puntero del menú de pausa
            RectTransform pointerRect = _scriptMain._pauseAssets._pointer.GetComponent<RectTransform>();
            RectTransform targetRect = _scriptMain._pauseAssets._options[_scriptMain._pauseAssets._onPos].GetComponent<RectTransform>();

            pointerRect.anchoredPosition = new Vector2(
                targetRect.anchoredPosition.x,
                targetRect.anchoredPosition.y - 30f
            );

            // 🔸 Confirmar selección
            if (Input.GetButtonDown("Submit"))
            {
                switch (_scriptMain._pauseAssets._onPos)
                {
                    case 0:
                        _scriptMain.SetPause(); // Reanudar
                        break;
                    case 1:
                        if (!_scriptMain._pauseAssets._hintBought && _scriptMain._pauseAssets._hintAvailable)
                        {
                            HintVoid();
                        }
     
                        // Otra acción (reiniciar, menú, etc.)
                        break;
                    case 2:
                        _scriptMain.SetPause(); // Salir o continuar
                        break;
                }
            }
        }


        // 🔹 Actualizaciones del gameplay SOLO si no está pausado
        if (!_scriptMain._gameOverAssets._onGameOver)
        {
            PauseGameOverFrozenAssets();
        }

        // 🔹 Actualizaciones de UI de pausa (si está pausado)v
        if (_scriptMain._gameOverAssets._onGameOver || !_scriptMain._pauseAssets._pause)
        {
            float h = Input.GetAxisRaw("Vertical");
            if (h < 0) _scriptMain._gameOverAssets._onPos = 0;
            if (h > 0) _scriptMain._gameOverAssets._onPos = 1;

            // Mover puntero del menú de pausa
            _scriptMain._gameOverAssets._pointer.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(_scriptMain._gameOverAssets._options[_scriptMain._gameOverAssets._onPos].GetComponent<RectTransform>().anchoredPosition.x,
                _scriptMain._gameOverAssets._options[_scriptMain._gameOverAssets._onPos].GetComponent<RectTransform>().anchoredPosition.y - 30f);

            if (Input.GetButtonDown("Submit"))
            {
                switch (_scriptMain._gameOverAssets._onPos)
                {
                    case 0:
                        //_scriptMain.SetPause();
                        break;
                    case 1:
                        //_scriptMain.SetPause();
                        break;
                }

            }
        }

        if (_GamesList[_scriptEvents._onEvent] != 11 && _eventOn)
        {
            if (_slimeChanging)
            {

                _slimeParent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(
                   _slimeParent.GetComponent<RectTransform>().anchoredPosition, new Vector2(50, -50), 5 * Time.deltaTime);
            }
            else
            {

                _slimeParent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(
             _slimeParent.GetComponent<RectTransform>().anchoredPosition, new Vector2(-240f, -190), 5 * Time.deltaTime);
            }

            if (_lightChanging)
            {

                _slimeLight.intensity = Mathf.Lerp(_slimeLight.intensity, 150, 5 * Time.deltaTime);
            }
            else
            {

                _slimeLight.intensity = Mathf.Lerp(_slimeLight.intensity, 5, 5 * Time.deltaTime);
            }

            if (_darkenerChanging)
            {
                _darkener.color = Color.Lerp(_darkener.color, new Color(0, 0, 0, 1), 4 * Time.deltaTime);
            }
            else
            {
                _darkener.color = Color.Lerp(_darkener.color, new Color(0, 0, 0, 0), 4 * Time.deltaTime);
            }
        }

        var ps = _Cascade[0];

        if (_cascadeFrozen)
        {
            // Cambia color de las partículas ya vivas
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
            int count = ps.GetParticles(particles);

            for (int i = 0; i < count; i++)
                particles[i].startColor = _cascadeColor[0];

            ps.SetParticles(particles, count);

            // Pausa y cambia color para las nuevas
            var main = ps.main;
            main.startColor = _cascadeColor[0];

            var emission = ps.emission;
            emission.enabled = false;
            ps.Pause();
        }
        else
        {
            var main = ps.main;
            main.startColor = _cascadeColor[1];
            var emission = ps.emission;
            emission.enabled = true;
            ps.Play();
        }

        if (_shopAssets._onShop)
        {
            if (Input.GetAxisRaw("Horizontal") < 0)
            {
                _shopAssets._onOption = 0;
            }
            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                _shopAssets._onOption = 1;
            }
            if (Input.GetButtonDown("Submit"))
            {
          
                _shopAssets._onShop = false;
              
            }

            for (int i = 0; i < 2; i++)
            {
                _shopAssets._background[i].color = _shopAssets._colors[0];

            }

          

            switch (_shopAssets._onOption)
            {
                case 0:
                    _shopAssets._coinSprite[0].transform.localScale = Vector2.Lerp(_shopAssets._coinSprite[0].transform.localScale, new Vector2(1.25f, 1.25f), 5 * Time.deltaTime);
                    _shopAssets._coinSprite[1].transform.localScale = Vector2.Lerp(_shopAssets._coinSprite[1].transform.localScale, new Vector2(1f, 1f), 5 * Time.deltaTime);
                    _shopAssets._background[0].color = _shopAssets._colors[1];
                    break;
                case 1:
                    _shopAssets._coinSprite[1].transform.localScale = Vector2.Lerp(_shopAssets._coinSprite[1].transform.localScale, new Vector2(1.25f, 1.25f), 5 * Time.deltaTime);
                    _shopAssets._coinSprite[0].transform.localScale = Vector2.Lerp(_shopAssets._coinSprite[0].transform.localScale, new Vector2(1f, 1f), 5 * Time.deltaTime);
                    _shopAssets._background[1].color = _shopAssets._colors[2];
                    break;
            }
            //if(_scriptMain._pauseAssets._hintAvailable)




        }
        _scriptMain._pauseAssets._hintText.gameObject.SetActive(_scriptMain._pauseAssets._hintBought);
    }

    public void HintVoid()
    {
        _scriptMain._pauseAssets._hintBought = true;        
      
        _scriptMain._saveLoadValues._hintCoins--;
        _scriptMain._pauseAssets._hintAvailable = false;
    }
    public IEnumerator StartStageNumerator()
    {

       _scriptMain._pauseAssets._hintText.text = GameInitScript.Instance.GetText("hint" + _onEventID.ToString("f0"));

        _slimeParent.gameObject.SetActive(false);
        //LoseHeartVoid();
        if(_scriptEvents._currentEventPrefab.name == "ChestEvent(Clone)" || _scriptEvents._currentEventPrefab.name == "Intro(Clone)")
        {
            _topOptionsOn = false;
        }
        else
        {
            _topOptionsOn = true;
        }
        _scriptSlime._slimeAnimator.SetBool("Moving", false);
        _darkenerChanging = false;
 
        _shadow.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);

        if(_scriptEvents._onEvent == 1)
        {
            _slimeParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-255, _slimeParent.GetComponent<RectTransform>().anchoredPosition.y);
        
            yield return new WaitForSeconds(0.25f);
            if(_scriptMain._onWorldGlobal == 3)
            {
             
                _scriptSlime._slimeAnimator.Play("Fall2");
                yield return new WaitForSeconds(1);
                _mainUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30);
                _slimeArriveParticle.Play();
                _slimeParent.gameObject.SetActive(true);
                yield return new WaitForSeconds(1);
            }
            else
            {
                _scriptSlime._slimeAnimator.Play("Fall1");
                yield return new WaitForSeconds(1);
                _mainUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30);
                _slimeArriveParticle.Play();
                _slimeParent.gameObject.SetActive(true);
            }
         
  
      
        }
        else
        {
            _slimeParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-500, _slimeParent.GetComponent<RectTransform>().anchoredPosition.y);
            _slimeParent.gameObject.SetActive(true);
            _scriptSlime._slimeAnimator.SetBool("Moving", true);
            _scriptMain._bordersAnimator.SetBool("BorderOut", true);
            _scriptMain._cinematicBorders.SetBool("FadeIn", false);
            yield return new WaitForSeconds(1.25f);
            _scriptSlime._slimeAnimator.SetBool("Moving", false);
        }



        yield return new WaitForSeconds(1);
        if (_scriptEvents._specialEvents[_GamesList[_scriptEvents._onEvent]]._eventType == GameEvent.EventType.StrongAir || _scriptEvents._specialEvents[_GamesList[_scriptEvents._onEvent]]._eventType == GameEvent.EventType.BossFight3)
        {
            _scriptSlime._slimeAnimator.SetBool("WindPush", true);
        }
     

        switch (_scriptEvents._specialEvents[_GamesList[_scriptEvents._onEvent]]._eventClassification)
        {
            case GameEvent.EventClassification.Normal:
                _scriptRythm._OnPhase = 0;
                StartCoroutine(_scriptRythm.RythmNumerator());
                break;         
        }   
    }


    public void PauseGameOverFrozenAssets()
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

    public IEnumerator IntroStageNumerator()
    {
        _shadow.gameObject.SetActive(false);
        _scriptMain._scriptSFX._windSetVolume = 1;
        for (int i = 0; i < _allStageAssets.Length; i++)
        {
            _allStageAssets[i]._backStage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -600f);
        }
        _allStageAssets[_scriptMain._onWorldGlobal]._frontStage.SetActive(false);
        _scriptMain._cinematicBorders.SetBool("FadeIn", true);  
        _scriptSlime._slimeAnimator.SetBool("Falling", true);
        _scriptEvents._currentEventPrefab.GetComponent<IntroEventScript>().StartIntroVoid();
        //_scriptEvents._onEvent++;
        _fallParticle.gameObject.SetActive(true);
        _scriptMain._bordersAnimator.SetBool("BorderOut", true);
        yield return new WaitForSeconds(2);
        //_bossAnimator.SetTrigger("Flies");
        yield return new WaitForSeconds(2);
        _slimeFalling = true;
        _scriptEvents._winRound = true;
        _firstStage = true;
        StartCoroutine(ExitNumerator());

    }



    //public void LoseHeartVoid()
    //{
    
    //    for(int i = 0; i < _heartAssets._totalHearts.Length; i++)
    //    {
    //        _heartAssets._totalHearts[i].color = _heartAssets._heartColors[1];
    //    }

    //    for (int i = 0; i < _totalLifes; i++)
    //    {
    //        _heartAssets._totalHearts[i].color = _heartAssets._heartColors[0];
    //    }
    //}

    public IEnumerator StartsStageChest()
    {

        _dialogeAssets._nextParent.SetActive(false);
        _dialogeAssets._nextCircle.SetActive(false);
        _slimeParent.gameObject.SetActive(false);
        //LoseHeartVoid();
        _topOptionsOn = false;
        _scriptSlime._slimeAnimator.SetBool("Moving", false);
        _darkenerChanging = false;

        _shadow.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);

        if(_scriptEvents._onEvent == 1)
        {
            if (_scriptMain._onWorldGlobal == 3)
            {


                yield return new WaitForSeconds(1);
                _mainUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30);
                _slimeArriveParticle.Play();
                _slimeParent.gameObject.SetActive(true);
                _scriptSlime._slimeAnimator.Play("Fall2");
                yield return new WaitForSeconds(1);
            }
            else
            {

                yield return new WaitForSeconds(1);
                _mainUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -30);
                _slimeArriveParticle.Play();
                _slimeParent.gameObject.SetActive(true);
                _scriptSlime._slimeAnimator.Play("Fall1");
            }
        }
        else
        {
            _slimeParent.gameObject.SetActive(true);
            _slimeParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-500, _slimeParent.GetComponent<RectTransform>().anchoredPosition.y);
            _scriptSlime._slimeAnimator.SetBool("Moving", true);
            _scriptMain._bordersAnimator.SetBool("BorderOut", true);
            _scriptMain._cinematicBorders.SetBool("FadeIn", false);
            yield return new WaitForSeconds(1.25f);
            _scriptSlime._slimeAnimator.SetBool("Moving", false);
        }


        _shadow.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        _dialogeAssets._dialogePanel.SetBool("DialogeIn", true);
        //_scriptEvents._currentEventPrefab.GetComponent<ChestEventScript>()._chestAnimator.SetTrigger("ChestOpen");
      

        switch (_scriptEvents._specialEvents[_GamesList[_scriptEvents._onEvent]]._eventClassification)
        {
            case GameEvent.EventClassification.Chest:
                switch (_scriptEvents._specialEvents[_GamesList[_scriptEvents._onEvent]]._chestItems[0])
                {
                    case GameEvent.ChestItems.Water:
                        _dialogeAssets._dialogeSize = new Vector2(1, 3);
                        break;
                    case GameEvent.ChestItems.Earth:
                        _dialogeAssets._dialogeSize = new Vector2(3, 6);
                        break;
                    case GameEvent.ChestItems.Air:
                        break;
          
                    case GameEvent.ChestItems.Fire:
                        break;
                }
                break;
        }


        for (int i = (int)_dialogeAssets._dialogeSize.x; i < (int)_dialogeAssets._dialogeSize.y; i++)
        {
            string text = GameInitScript.Instance.GetText("dialoge" + i.ToString());
            int id = GameInitScript.Instance.GetTextID("dialoge" + i.ToString());
            StartCoroutine(EscribirTexto(text, _dialogeAssets._dialogeText, 0.02f));
            var PrincessImage = _scriptEvents._currentEventPrefab.GetComponent<ChestEventScript>();
            PrincessImage._princessAnim.GetComponent<Image>().sprite = PrincessImage._allPrincessSprites[id];

            while (_dialogeAssets._typing)
            {
                yield return null;
            }
            _dialogeAssets._nextParent.SetActive(true);


          

 
            if (i == (int)_dialogeAssets._dialogeSize.y - 1)
            {
                _dialogeAssets._nextCircle.SetActive(true);
            }
        
      
       
            while (!Input.GetButtonDown("Submit"))
            {
                yield return null;
            }
            _scriptMain._scriptSFX.PlaySound(_scriptMain._scriptSFX._next);
            _dialogeAssets._nextParent.SetActive(false);
            _dialogeAssets._nextCircle.SetActive(false);
        }
        //_scriptEvents._onEvent++;
        _dialogeAssets._dialogePanel.SetBool("DialogeIn", false);
        _itemGotPanel._parent.SetTrigger("ItemGot");
        _scriptEvents._currentEventPrefab.GetComponent<ChestEventScript>().ItemGet();
        yield return new WaitForSeconds(1);
        _scriptEvents._winRound = true;
        StartCoroutine(ExitNumerator());

    }

    public IEnumerator StartsShopNumerator()
    {

        _dialogeAssets._nextParent.SetActive(false);
        _dialogeAssets._nextCircle.SetActive(false);
        //_slimeParent.gameObject.SetActive(false);
        //LoseHeartVoid();
        _topOptionsOn = false;
        _scriptSlime._slimeAnimator.SetBool("Moving", false);
        _darkenerChanging = false;

       
        yield return new WaitForSeconds(2);
        _shopAssets._parent.SetBool("ShopIn", true);
        yield return new WaitForSeconds(1);
        _shopAssets._onShop = true;
        while (_shopAssets._onShop)
        {
            yield return null;
        }
        switch (_shopAssets._onOption)
        {
            case 0:
                _scriptMain._saveLoadValues._healthCoins++;
                break;
            case 1:
                _scriptMain._saveLoadValues._hintCoins++;
                break;
        }
        _shopAssets._parent.SetBool("ShopIn", false);
        yield return new WaitForSeconds(1);
 
        _scriptEvents._winRound = true;
        StartCoroutine(ExitNumerator());

    }

    public IEnumerator LoseLifeNumerator()
    {
        _scriptMain._saveLoadValues._healthCoins--;
        for(int i = 0; i < 4M; i++)
        {
            _scriptRythm._elementsInfo[i]._parent.SetActive(false);
        }
        _scriptRythm._elementsSelection.Clear();
        _dead = true;
        //LoseHeartVoid();
        _slimeExplosion.Play();
        _scriptSlime._slimeRawImage.gameObject.SetActive(false);
        _scriptRythm._OnPhase = 0;
        yield return new WaitForSeconds(1);


        switch (_scriptEvents._specialEvents[_GamesList[_scriptEvents._onEvent]]._eventType)
        {
            case GameEvent.EventType.Fire:
                _scriptEvents._currentEventPrefab.GetComponent<FireEventScript>().FireExtinguishVoid();
                break;
            case GameEvent.EventType.BossFight3:
                _frontWindParticle.Stop();
                break;
            case GameEvent.EventType.BossFight4:
                _scriptEvents._currentEventPrefab.GetComponent<BossFightsScript>().FireExtinguish();
                break;
        }


        _scriptEvents._winRound = false;




        _scriptSlime._slimeAnimator.SetBool("Scared", false);
        _scriptSlime._materialColors[1] = _scriptSlime._slimeAssets[0]._mainColor;
        _scriptSlime._materialColors[2] = _scriptSlime._slimeAssets[0]._mainColor;
        _scriptSlime.fillAmount = 0;
        _scriptMain._bordersAnimator.SetBool("BorderOut", false);

        yield return new WaitForSeconds(2);



        _bossAnimator.gameObject.SetActive(false);
      
        _allStageAssets[_scriptMain._onWorldGlobal]._frontStage.SetActive(true);
        _scriptMain._cinematicBorders.SetBool("FadeIn", false);
        _scriptSlime._slimeAnimator.SetBool("Falling", false);
        _fallParticle.gameObject.SetActive(false);
        _slimeFalling = false;
        _scriptMain._scriptSFX._windSetVolume = 0;

        for (int i = 0; i < _allStageAssets.Length; i++)
        {
            _allStageAssets[i]._backStage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }
        _scriptSlime._slimeRawImage.gameObject.SetActive(true);
        if(_scriptMain._saveLoadValues._healthCoins > 0)
        {

            StartCoroutine(_scriptEvents.StartLevelNumerator());
            _dead = false;
        }
        else
        {
            StartCoroutine(GameOverNumerator());
        }
 
    }

    public IEnumerator GameOverNumerator()
    {
        _scriptMain._windParticle.Stop();
        _scriptMain._gameOverAssets._onGameOver = true;
        _scriptMain._gameOverAssets._parent.GetComponent<Animator>().SetBool("GameOver", true);
        Debug.Log("GAME OVER");
        yield return new WaitForSeconds(1);
        while (!Input.GetButtonDown("Submit"))
        {
            yield return null;
        }
        _scriptMain._gameOverAssets._onGameOver = false;
        _scriptMain._gameOverAssets._parent.GetComponent<Animator>().SetBool("GameOver", false);
        yield return new WaitForSeconds(1);
        switch (_scriptMain._gameOverAssets._onPos)
        {
            case 0:

                _scriptMain.LoadSceneByName("IntroScene");
                break;
            case 1:
                _scriptMain._saveLoadValues._healthCoins++;
             
                StartCoroutine(_scriptEvents.StartStageQuestionary());
        
                break;
        }
    }

    public IEnumerator ExitNumerator()
    {

        if (_scriptEvents._currentEventPrefab.name != "Intro(Clone)")
        {
       
            _scriptSlime._slimeAnimator.SetBool("Moving", true);
        }
        _Cascade[0].Stop();

        yield return new WaitForSeconds(0.5f);
        _scriptMain._bordersAnimator.SetBool("BorderOut", false);
        _scriptEvents._rainParticle.Stop();
        _scriptMain._scriptSFX._rainSetVolume = 0;
        _scriptMain._windParticle.GetComponent<ForceField2D>().fuerza = 5;
        _scriptMain._windParticle.Stop();
        _snowParticle.Stop();
        _scriptSlime._slimeAnimator.SetBool("Scared", false);
        _scriptSlime._materialColors[1] = _scriptSlime._slimeAssets[0]._mainColor;
        _scriptSlime._materialColors[2] = _scriptSlime._slimeAssets[0]._mainColor;
        _scriptSlime.fillAmount = 0;
        _scriptMain._bordersAnimator.SetBool("BorderOut", false);
        _snowBool = false;
        _dialogeAssets._nextParent.SetActive(false);
        _dialogeAssets._nextCircle.SetActive(false);

        yield return new WaitForSeconds(1);
        Destroy(_scriptEvents._currentEventPrefab);
        _scriptMain._pauseAssets._hintAvailable = false;
        _scriptMain._pauseAssets._hintBought = false;
        if (_scriptEvents._currentEventPrefab != null)
        {
            if (_scriptEvents._currentEventPrefab.name == "Intro(Clone)")
            {
                _scriptEvents._currentEventPrefab.GetComponent<IntroEventScript>().ExitIntroVoid();
                //_scriptEvents._onEvent++;
            }
        }
    

        // _scriptSlime._WindBlocker.gameObject.SetActive(false);
        _allStageAssets[_scriptMain._onWorldGlobal]._frontStage.SetActive(true);
        _scriptMain._cinematicBorders.SetBool("FadeIn", false);
        _scriptSlime._slimeAnimator.SetBool("Falling", false);
        _fallParticle.gameObject.SetActive(false);
        _slimeFalling = false;
        _scriptMain._scriptSFX._windSetVolume = 0;
        _scriptSlime._slimeType = 0;
        _scriptSlime.ChangeSlime();
        _windBlocker.GetComponent<ParticleSystem>().Stop();
        for (int i = 0; i < _allStageAssets.Length; i++)
        {
            _allStageAssets[i]._backStage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }

        //if (_scriptEvents._onEvent)

        switch (_scriptEvents._winRound)
        {
            case false:
        
                Destroy(_scriptEvents._currentEventPrefab);
                StartCoroutine(LoseLifeNumerator());
                break;
            case true:
             
                _scriptEvents._onEvent++;
                Destroy(_scriptEvents._currentEventPrefab);
                StartCoroutine(_scriptEvents.StartLevelNumerator());
                break;
        }

    }

    public IEnumerator EscribirTexto(string linea, TMPro.TextMeshProUGUI textoUI, float velocidad)
    {
        _dialogeAssets._typing = true;
        textoUI.text = "";
        foreach (char letra in linea.ToCharArray())
        {
            textoUI.text += letra;
            yield return new WaitForSeconds(velocidad);
        }
        _dialogeAssets._typing = false;
    }
}
