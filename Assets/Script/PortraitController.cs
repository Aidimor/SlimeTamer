using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using LoL;
using LoLSDK;

public class PortraitController : MonoBehaviour
{


    public bool _falling;
    public GameObject _parent;
    public Image _logo;
    public int _OnPos;
    //public float[] _xPoses;
    public float _speed;
    public bool _pressed;

    //[System.Serializable]
    //public class AllWorlds
    //{
    //    public GameObject _worldParent;
    //    public float _yPos;
    //    public Color _backgroundColor;
    //    public TextMeshProUGUI _worldText;

    //    [Header("Idioma")]
    //    public string key;
    //}

    //public AllWorlds[] _allWorlds;

    [System.Serializable]
    public class NewAllWorlds
    {
        public GameObject _worldParent;
        public GameObject _icon;
        public float _yPos;
        public Color _backgroundColor;
        public TextMeshProUGUI _worldText;

        [Header("Idioma")]
        public string key;
    }
    public NewAllWorlds[] _newAllWorlds;

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
    public TextMeshProUGUI _scapeGame;

    public bool _movementAvailable;

    public GameObject _portraitParent;
    public TextMeshProUGUI _startText;

    public GameObject _newWolrdChoose;

    public void Awake()
    {
        Instance = this;
        _slimeParent.SetActive(false);
        _portraitParent.SetActive(MainController.Instance._onPortrait);
        //switch (MainController.Instance._onPortrait)
        //{
        //    case false:
        //        break;
        //    case true:
        //        break;
        //}

    }
    void OnEnable()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
        _slimeParent.SetActive(false);
        _portraitParent.SetActive(MainController.Instance._onPortrait);


        if (SceneManager.GetActiveScene().isLoaded)
   
  

        MainController.Instance._scriptMusic.PlayMusic(0);

        //StartCoroutine(UpdateWorldTexts());



        _onWorldPos = MainController.Instance._onWorldGlobal;
        ParentWorldScale();
        StartCoroutine(StartsSceneNumerator());
    }

    public IEnumerator StartsSceneNumerator()
    {
        yield return new WaitForSeconds(1);
        //MainController.Instance._currencyAssets[0]._quantityText.text = MainController.Instance._saveLoadValues._healthCoins.ToString("f0");
        //MainController.Instance._currencyAssets[1]._quantityText.text = MainController.Instance._saveLoadValues._hintCoins.ToString("f0");

        var Main = MainController.Instance._saveLoadValues;
        for (int i = 4; i < Main._worldsUnlocked.Length; i++)
        {
            if (Main._worldsUnlocked[i])
            {         
                _worldsUnlocked++;
            }
        }
        if (!MainController.Instance._onPortrait)
        {
            StartCoroutine(UpdateWorldTexts());
        }

        for (int i = 0; i < Main._worldsUnlocked.Length; i++)
        {
            if (Main._worldsUnlocked[i])
            {
                _newAllWorlds[i]._icon.GetComponent<Image>().color = Color.white;
                _newAllWorlds[i]._worldText.gameObject.SetActive(true);
                _newAllWorlds[i]._worldParent.GetComponent<Image>().color = _newAllWorlds[i]._backgroundColor;
            }
            else
            {
                _newAllWorlds[i]._icon.GetComponent<Image>().color = Color.black;
                _newAllWorlds[i]._worldText.gameObject.SetActive(false);
            }
        }

   
    }

    void ParentWorldScale()
    {
        for(int i = 0; i < _newAllWorlds.Length; i++)
        {
            _newAllWorlds[i]._worldParent.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
        }
        _newAllWorlds[_onWorldPos]._worldParent.GetComponent<RectTransform>().localScale = new Vector2(1.25f, 1.25f);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (MainController.Instance != null && MainController.Instance._onPortrait)
        {

            if (MainController.Instance != null)
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
        //Debug.Log("aqui la caga");
        yield return new WaitUntil(() =>
            GameInitScript.Instance != null &&
            GameInitScript.Instance.languageReady
        );

        var gi = GameInitScript.Instance;

        for (int i = 0; i < _newAllWorlds.Length; i++)
        {
            int displayNumber = i + 1;

            var world = _newAllWorlds[i];
            //if (world._worldText != null && !string.IsNullOrEmpty(world.key))
            //world._worldText.text = gi.GetText(world.key) + " " + displayNumber;
            world._worldText.text = gi.GetText("WorldName" + (i + 1)).ToString();

        }

        MainController.Instance._exitAssets._textOptions[0].text = GameInitScript.Instance.GetText("pause1");
        MainController.Instance._exitAssets._textOptions[1].text = GameInitScript.Instance.GetText("pause2");
        MainController.Instance._exitAssets._textOptions[2].text = GameInitScript.Instance.GetText("pause3");
        _textTutorials[0].text = GameInitScript.Instance.GetText("movewith");
        _textTutorials[1].text = GameInitScript.Instance.GetText("pressbutton");

        _startText.text = GameInitScript.Instance.GetText("continue");
        MainController.Instance._atomPanelInfo._textInfo[0].text = GameInitScript.Instance.GetText("movewith");
        MainController.Instance._atomPanelInfo._textInfo[1].text = GameInitScript.Instance.GetText("pressbutton");


        MainController.Instance._atomPanelInfo._fusionElementAssets[0]._name.text = GameInitScript.Instance.GetText("CO2");
        MainController.Instance._atomPanelInfo._fusionElementAssets[0]._extra.text = GameInitScript.Instance.GetText("CO2extra");

        MainController.Instance._atomPanelInfo._fusionElementAssets[1]._name.text = GameInitScript.Instance.GetText("H2O");
        MainController.Instance._atomPanelInfo._fusionElementAssets[1]._extra.text = GameInitScript.Instance.GetText("H2Oextra");

        MainController.Instance._atomPanelInfo._fusionElementAssets[2]._name.text = GameInitScript.Instance.GetText("FC");
        MainController.Instance._atomPanelInfo._fusionElementAssets[2]._extra.text = GameInitScript.Instance.GetText("FCextra");

        MainController.Instance._atomPanelInfo._fusionElementAssets[3]._name.text = GameInitScript.Instance.GetText("FE3O4");
        MainController.Instance._atomPanelInfo._fusionElementAssets[3]._extra.text = GameInitScript.Instance.GetText("FE3O4extra");

        MainController.Instance._continuarTutorial.text = GameInitScript.Instance.GetText("continue");

        MainController.Instance._elementsName[0].text = gi.GetText("element1");
        MainController.Instance._elementsName[1].text = gi.GetText("element2");
        MainController.Instance._elementsName[2].text = gi.GetText("element3");
        MainController.Instance._elementsName[3].text = gi.GetText("element4");

        MainController.Instance._tutorialAssets._slimeNameText[0].text = gi.GetText("CO2");
        MainController.Instance._tutorialAssets._slimeNameText[1].text = gi.GetText("H2O");
        MainController.Instance._tutorialAssets._slimeNameText[2].text = gi.GetText("FC");

        _scapeGame.text = GameInitScript.Instance.GetText("escapegame");

        yield return new WaitForSeconds(0.5f);

        MainController.Instance._bordersAnimator.SetBool("BorderOut", true);

        _slimeParent.SetActive(true);
        switch (MainController.Instance._onPortrait)
        {
            case false:
                yield return new WaitForSeconds(1);
                _movementAvailable = true;
                break;
            case true:
                yield return new WaitForSeconds(5);
            
                break;
        }
    
        //for (int i = 0; i < _allWorlds.Length; i++)
        //{
        //    var world = _allWorlds[i];

        //    if (world._lockedText != null)
        //        world._lockedText.text = gi.GetText("locked");
        //}
    }

    void Update()
    {
        if (MainController.Instance == null) return;

        switch (MainController.Instance._onPortrait)
        {
            case false:
                if (_movementAvailable)
                {

                    HandleWorldSelection();

                }
                break;
            case true:
                if (Input.GetButtonDown("Submit"))
                {
                    StartCoroutine(toMainPanel());
                }
                break;
        }





        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }

        //_slimeParent.GetComponent<RectTransform>().anchoredPosition = Vector2.MoveTowards(_slimeParent.GetComponent<RectTransform>().anchoredPosition, new Vector2(135, _newAllWorlds[_onWorldPos]._worldParent.GetComponent<RectTransform>().anchoredPosition.y - 25), 1500f * Time.deltaTime);
        _slimeParent.GetComponent<RectTransform>().anchoredPosition = Vector2.MoveTowards(_slimeParent.GetComponent<RectTransform>().anchoredPosition, new Vector2(135, _newAllWorlds[_onWorldPos]._yPos), 1500f * Time.deltaTime);



        _parent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_parent.GetComponent<RectTransform>().anchoredPosition, Vector2.zero, 20 * Time.deltaTime);
        _portraitParent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_portraitParent.GetComponent<RectTransform>().anchoredPosition, Vector2.zero, 20 * Time.deltaTime);

        if(_onWorldPos == 4)
        {
            _newWolrdChoose.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_newWolrdChoose.GetComponent<RectTransform>().anchoredPosition, new Vector2(_newWolrdChoose.GetComponent<RectTransform>().anchoredPosition.x, 175), 15 * Time.deltaTime);
        }
        else
        {
            _newWolrdChoose.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_newWolrdChoose.GetComponent<RectTransform>().anchoredPosition, new Vector2(_newWolrdChoose.GetComponent<RectTransform>().anchoredPosition.x, -10), 15 * Time.deltaTime);
        }

    }


    public IEnumerator toMainPanel()
    {
        MainController.Instance._onPortrait = false;
        SFXscript.Instance.PlaySound(SFXscript.Instance._chooseElement);
        _portraitParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(_portraitParent.GetComponent<RectTransform>().anchoredPosition.x, _portraitParent.GetComponent<RectTransform>().anchoredPosition.y + 10);
        MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
        SFXscript.Instance.PlaySound(SFXscript.Instance._chooseElement);
        yield return new WaitForSeconds(1);

        StartCoroutine(UpdateWorldTexts());
        _portraitParent.SetActive(MainController.Instance._onPortrait);
        MainController.Instance._bordersAnimator.SetBool("BorderOut", true);
        yield return new WaitForSeconds(1);
        _movementAvailable = true;

    }

    private void HandleWorldSelection()
    {
        if (!_gameStarts)
        {
            if (Input.GetAxisRaw("Vertical") > 0 && !_worldPressed && _onWorldPos > 0)
            {
                SFXscript.Instance.PlaySound(SFXscript.Instance._next);
                _onWorldPos--;
                _worldPressed = true;

                ParentWorldScale();

                string key = "WorldName" + (_onWorldPos + 1).ToString();
                Debug.Log(key);
                string text = GameInitScript.Instance.GetText(key);
                string speakKey = key;
                LOLSDK.Instance.SpeakText(speakKey);
            }

            if (Input.GetAxisRaw("Vertical") < 0 && !_worldPressed && _onWorldPos < _newAllWorlds.Length - 1)
            {
                SFXscript.Instance.PlaySound(SFXscript.Instance._next);
                _onWorldPos++;
                _worldPressed = true;

                ParentWorldScale();

                string key = "WorldName" + (_onWorldPos + 1).ToString();              
                //string text = GameInitScript.Instance.GetText(key);
                string speakKey = key;
                LOLSDK.Instance.SpeakText(speakKey);
            }

            if(Input.GetAxisRaw("Vertical") == 0)
            {
                _worldPressed = false;
            }

            if (Input.GetButton("Submit") && MainController.Instance._saveLoadValues._worldsUnlocked[_onWorldPos])
            {
                StartCoroutine(StartGame());
            }
        }

        _worldBackgroundImage.color =
            Color.Lerp(_worldBackgroundImage.color, _newAllWorlds[_onWorldPos]._backgroundColor, 2 * Time.deltaTime);
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
        //MainController.Instance._cinematicBorders.SetBool("FadeIn", false);
        _onWorldPos = MainController.Instance._onWorldGlobal;
        MainController.Instance._saveLoadValues._worldsUnlocked[_onWorldPos] = true;
        //_scriptMainController._introSpecial = false;
        _logo.gameObject.SetActive(true);
        _gameStarts = false;
        yield return new WaitForSeconds(1);
        _movementAvailable = true;
    }

    public IEnumerator StartGame()
    {
        _movementAvailable = false;
        _slimeParent.SetActive(false);
        _parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(_parent.GetComponent<RectTransform>().anchoredPosition.x, _parent.GetComponent<RectTransform>().anchoredPosition.y + 10);
        GameInitScript.Instance.SubmitProgressToSDK();
        _gameStarts = true;
        //_scriptMainController._cinematicBorders.SetBool("FadeIn", true);
        SFXscript.Instance.PlaySound(SFXscript.Instance._chooseElement);
        //_logo.gameObject.SetActive(false);
        _explosionSlimeParticle.Play();
        //_slimeParent.SetActive(false);
        _fallingSlime.Play();
        //yield return new WaitForSeconds(1);
       SFXscript.Instance.PlaySound(SFXscript.Instance._fall);
        MainController.Instance._onWorldGlobal = _onWorldPos;


        MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
        //_scriptMainController._cinematicBorders.SetBool("FadeIn", false);
        yield return new WaitForSeconds(1);
 
        switch (MainController.Instance._onWorldGlobal)
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
                ComicController.Instance._imagesID.Add(11);
                ComicController.Instance._imagesID.Add(12);
                ComicController.Instance._imagesID.Add(13);
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
        MainController.Instance._firstWorld = true;
        MainController.Instance._saveLoadValues._progressSave[0] = true;
        MainController.Instance._bordersAnimator.SetBool("BorderOut", false);
        yield return new WaitForSeconds(1);
        ComicController.Instance._continueParent.SetActive(false);
        ComicController.Instance._comicAnimator.SetBool("ComicOn", false);
        MainController.Instance.LoadSceneByName("MainGame");



    }

}
