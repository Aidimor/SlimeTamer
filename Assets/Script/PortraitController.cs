using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using LoL;
using LoLSDK;

public class PortraitController : MonoBehaviour
{
    [SerializeField] MainController _scriptMainController;

    public bool _falling;
    public GameObject _parent;
    public Image _logo;
    public int _OnPos;
    //public float[] _xPoses;
    public float _speed;
    public bool _pressed;

    [System.Serializable]
    public class AllWorlds
    {
        public GameObject _worldParent;
        public float _yPos;
        public Color _backgroundColor;
        public TextMeshProUGUI _worldText;

        [Header("Idioma")]
        public string key;
    }

    public AllWorlds[] _allWorlds;
    public GameObject _worldsParent;
    public Image _worldBackgroundImage;
    public int _onWorldPos;
    public bool _worldPressed;
    public bool _gameStarts;

    public bool _changing;
    public ParticleSystem _explosionSlimeParticle;
    public ParticleSystem _fallingSlime;
    public GameObject _slimeParent;
    public GameObject _frontMap;

    public TextMeshProUGUI _quitar;
    public int _quitarID;
    public static PortraitController Instance;
    public int _worldsUnlocked;

  
    public Animator _comicAnimator;
    public Image _comicStrip;
    public TextMeshProUGUI[] _textTutorials;



    public void Awake()
    {
        Instance = this;
       
    }
    void OnEnable()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SceneManager.GetActiveScene().isLoaded)
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        if (_scriptMainController != null && MainController.Instance._onWorldGlobal != 4)
        {
            _scriptMainController._scriptMusic._audioBGM.clip = _scriptMainController._scriptMusic._allThemes[0];
            _scriptMainController._scriptMusic._audioBGM.Play();
        }

        StartCoroutine(UpdateWorldTexts());
        if (MainController.Instance._introSpecial)
        {
            //_scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._falling);
            _falling = true;
            //for (int i = 0; i < _allWorlds.Length; i++)
            //{
            //    _allWorlds[i]._worldText.gameObject.SetActive(false);
            //    _allWorlds[i]._spaceButton.gameObject.SetActive(false);
            //    _allWorlds[i]._lockedParemt.gameObject.SetActive(false);
            //}
            _logo.gameObject.SetActive(false);
            _frontMap.gameObject.SetActive(false);
        }


        _onWorldPos = MainController.Instance._onWorldGlobal;
        StartCoroutine(StartsSceneNumerator());
    }

    public IEnumerator StartsSceneNumerator()
    {
        yield return new WaitForSeconds(1);
        //MainController.Instance._currencyAssets[0]._quantityText.text = MainController.Instance._saveLoadValues._healthCoins.ToString("f0");
        //MainController.Instance._currencyAssets[1]._quantityText.text = MainController.Instance._saveLoadValues._hintCoins.ToString("f0");

        var Main = MainController.Instance._saveLoadValues;
        for (int i = 0; i < Main._worldsUnlocked.Length; i++)
        {
            if (Main._worldsUnlocked[i])
            {         
                _worldsUnlocked++;
            }
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var mainControllerObj = GameObject.Find("CanvasIndestructible/Main/MainController");
        if (mainControllerObj != null)
        {
            _scriptMainController = mainControllerObj.GetComponent<MainController>();
            if (_scriptMainController != null)
            {
                
                StartCoroutine(UpdateWorldTexts());
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró 'MainController' en la jerarquía.");
        }
    }

    public IEnumerator UpdateWorldTexts()
    {
        yield return new WaitUntil(() =>
            GameInitScript.Instance != null &&
            GameInitScript.Instance.languageReady
        );

        var gi = GameInitScript.Instance;

        for (int i = 0; i < _allWorlds.Length; i++)
        {
            int displayNumber = i + 1;

            var world = _allWorlds[i];
            //if (world._worldText != null && !string.IsNullOrEmpty(world.key))
                //world._worldText.text = gi.GetText(world.key) + " " + displayNumber;
            world._worldText.text = gi.GetText("WorldName" + (i + 1)).ToString();
    
        }

        MainController.Instance._exitAssets._textOptions[0].text = GameInitScript.Instance.GetText("pause1");
        MainController.Instance._exitAssets._textOptions[1].text = GameInitScript.Instance.GetText("pause2");
        MainController.Instance._exitAssets._textOptions[2].text = GameInitScript.Instance.GetText("pause3");
        _textTutorials[0].text = GameInitScript.Instance.GetText("movewith");
        _textTutorials[1].text = GameInitScript.Instance.GetText("pressbutton");


        MainController.Instance._atomPanelInfo._textInfo[0].text = GameInitScript.Instance.GetText("movewith");
        MainController.Instance._atomPanelInfo._textInfo[1].text = GameInitScript.Instance.GetText("pressbutton");


        MainController.Instance._atomPanelInfo._fusionElementAssets[0]._name.text = GameInitScript.Instance.GetText("CO2");
        MainController.Instance._atomPanelInfo._fusionElementAssets[0]._extra.text = GameInitScript.Instance.GetText("CO2extra");

        MainController.Instance._atomPanelInfo._fusionElementAssets[1]._name.text = GameInitScript.Instance.GetText("H20");
        MainController.Instance._atomPanelInfo._fusionElementAssets[1]._extra.text = GameInitScript.Instance.GetText("H20extra");

        MainController.Instance._atomPanelInfo._fusionElementAssets[2]._name.text = GameInitScript.Instance.GetText("FC");
        MainController.Instance._atomPanelInfo._fusionElementAssets[2]._extra.text = GameInitScript.Instance.GetText("FCextra");

        MainController.Instance._atomPanelInfo._fusionElementAssets[3]._name.text = GameInitScript.Instance.GetText("FE3O4");
        MainController.Instance._atomPanelInfo._fusionElementAssets[3]._extra.text = GameInitScript.Instance.GetText("FE3O4extra");


        yield return new WaitForSeconds(0.5f);

        _scriptMainController._bordersAnimator.SetBool("BorderOut", true);
        //for (int i = 0; i < _allWorlds.Length; i++)
        //{
        //    var world = _allWorlds[i];

        //    if (world._lockedText != null)
        //        world._lockedText.text = gi.GetText("locked");
        //}
    }

    void Update()
    {
        if (_scriptMainController == null) return;

        if (!_scriptMainController._introSpecial)
        {
            HandleMovement();
            HandleWorldSelection();
            //UpdateWorldUnlocks();
        }
        else if (_falling)
        {
            _worldsParent.GetComponent<RectTransform>().anchoredPosition =
                Vector2.MoveTowards(_worldsParent.GetComponent<RectTransform>().anchoredPosition,
                new Vector2(_allWorlds[_scriptMainController._onWorldGlobal]._yPos, -12),
                250f * Time.deltaTime);

            if (_worldsParent.GetComponent<RectTransform>().anchoredPosition.x ==
                _allWorlds[_scriptMainController._onWorldGlobal]._yPos && !_gameStarts)
            {
                StartCoroutine(StartGameSpecial());
            }
        }

        //MainController.Instance._currencyAssets[0]._quantityText.text = MainController.Instance._saveLoadValues._healthCoins.ToString("f0");
        //MainController.Instance._currencyAssets[1]._quantityText.text = MainController.Instance._saveLoadValues._hintCoins.ToString("f0");
    }

    private void HandleMovement()
    {
 
        if (Input.GetAxisRaw("Horizontal") == 0) _pressed = false;
   

        var worldPos = _worldsParent.GetComponent<RectTransform>().anchoredPosition;
        _worldsParent.GetComponent<RectTransform>().anchoredPosition =
            Vector2.Lerp(worldPos, new Vector2(_allWorlds[_onWorldPos]._yPos, worldPos.y), 5 * Time.deltaTime);

        _worldBackgroundImage.color =
            Color.Lerp(_worldBackgroundImage.color, _allWorlds[_onWorldPos]._backgroundColor, 2 * Time.deltaTime);
    }

    private void HandleWorldSelection()
    {
        if (!_gameStarts)
        {
            if (Input.GetAxisRaw("Horizontal") < 0 && !_worldPressed && _onWorldPos > 0)
            {
                _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._next);
                _onWorldPos--;
                _worldPressed = true;
            }

            if (Input.GetAxisRaw("Horizontal") > 0 && !_worldPressed && _onWorldPos < _worldsUnlocked - 1)
            {
                _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._next);
                _onWorldPos++;
                _worldPressed = true;
            }

            if(Input.GetAxisRaw("Horizontal") == 0)
            {
                _worldPressed = false;
            }

            if (Input.GetButton("Submit") && _scriptMainController._saveLoadValues._worldsUnlocked[_onWorldPos])
            {
                StartCoroutine(StartGame());
            }
        }
    }

    //private void UpdateWorldUnlocks()
    //{
    //    for (int i = 0; i < _scriptMainController._saveLoadValues._worldsUnlocked.Length; i++)
    //    {
    //        _allWorlds[i]._lockedParemt.SetActive(!_scriptMainController._saveLoadValues._worldsUnlocked[i]);
    //        _allWorlds[i]._spaceButton.SetActive(_scriptMainController._saveLoadValues._worldsUnlocked[i]);
    //    }
    //}

    public IEnumerator StartGameSpecial()
    {

        _gameStarts = true;    
 
   
        _frontMap.SetActive(true);
       

        yield return new WaitForSeconds(1);
        MainController.Instance._cinematicBorders.SetBool("FadeIn", false);
        _onWorldPos = MainController.Instance._onWorldGlobal;
        MainController.Instance._saveLoadValues._worldsUnlocked[_onWorldPos] = true;
        _scriptMainController._introSpecial = false;
        _logo.gameObject.SetActive(true);
        _gameStarts = false;
    }

    public IEnumerator StartGame()
    {
        Debug.Log("empieza");
        _gameStarts = true;
        _scriptMainController._cinematicBorders.SetBool("FadeIn", true);
        _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._chooseElement);
        _logo.gameObject.SetActive(false);
        _explosionSlimeParticle.Play();
        _slimeParent.SetActive(false);
        _fallingSlime.Play();
        yield return new WaitForSeconds(1);
        _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._fall);
        _scriptMainController._onWorldGlobal = _onWorldPos;
        yield return new WaitForSeconds(2);

        _scriptMainController._bordersAnimator.SetBool("BorderOut", false);
        _scriptMainController._cinematicBorders.SetBool("FadeIn", false);
        yield return new WaitForSeconds(1);
 
        switch (_scriptMainController._onWorldGlobal)
        {
            case 0:
                ComicController.Instance._imagesID.Add(0);
                ComicController.Instance._imagesID.Add(1);
                ComicController.Instance._imagesID.Add(2);
                ComicController.Instance._waitSeconds = 4;
                break;
            case 1:
                ComicController.Instance._imagesID.Add(7);
                ComicController.Instance._imagesID.Add(8);
                ComicController.Instance._waitSeconds = 4;
                break;
            case 2:
                ComicController.Instance._imagesID.Add(9);
                ComicController.Instance._imagesID.Add(10);
                ComicController.Instance._imagesID.Add(11);
                ComicController.Instance._waitSeconds = 4;
                break;
            case 3:
                ComicController.Instance._imagesID.Add(14);
                ComicController.Instance._imagesID.Add(15);
                ComicController.Instance._imagesID.Add(16);
                ComicController.Instance._waitSeconds = 4;
                break;
            case 4:
                ComicController.Instance._imagesID.Add(20);
                ComicController.Instance._imagesID.Add(21);
                ComicController.Instance._imagesID.Add(22);
                ComicController.Instance._waitSeconds = 4;
                break;
        }

        StartCoroutine(ComicController.Instance.ComicStripOn());
        ComicController.Instance._comicOn = true;
        while (ComicController.Instance._comicOn)
        {
            yield return null;
        }
        MainController.Instance._saveLoadValues._progressSave[0] = true;
        _scriptMainController._bordersAnimator.SetBool("BorderOut", false);
        yield return new WaitForSeconds(1);
        ComicController.Instance._continueParent.SetActive(false);
        ComicController.Instance._comicAnimator.SetBool("ComicOn", false);
        _scriptMainController.LoadSceneByName("MainGame");



    }

}
