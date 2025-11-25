using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using LoL;

public class PortraitController : MonoBehaviour
{
    [SerializeField] MainController _scriptMainController;

    public bool _falling;
    public GameObject _parent;
    public Image _logo;
    public int _OnPos;
    public float[] _xPoses;
    public float _speed;
    public bool _pressed;

    [System.Serializable]
    public class AllWorlds
    {
        public GameObject _worldParent;
        public float _yPos;
        public Color _backgroundColor;
        public TextMeshProUGUI _worldText;
        public GameObject _spaceButton;
        public GameObject _lockedParemt;
        public TextMeshProUGUI _lockedText;
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

    public void Awake()
    {
        Instance = this;
        //if (Instance != null)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

       
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SceneManager.GetActiveScene().isLoaded)
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        if (_scriptMainController != null)
        {
            _scriptMainController._scriptMusic._audioBGM.clip = _scriptMainController._scriptMusic._allThemes[0];
            _scriptMainController._scriptMusic._audioBGM.Play();
        }

        StartCoroutine(UpdateWorldTexts());
        if (MainController.Instance._introSpecial)
        {
            //_scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._falling);
            _falling = true;
            for (int i = 0; i < _allWorlds.Length; i++)
            {
                _allWorlds[i]._worldText.gameObject.SetActive(false);
                _allWorlds[i]._spaceButton.gameObject.SetActive(false);
                _allWorlds[i]._lockedParemt.gameObject.SetActive(false);
            }
            _logo.gameObject.SetActive(false);
            _frontMap.gameObject.SetActive(false);
        }
        StartCoroutine(StartsSceneNumerator());
    }

    public IEnumerator StartsSceneNumerator()
    {
        yield return new WaitForSeconds(2);
        MainController.Instance._currencyAssets[0]._quantityText.text = MainController.Instance._saveLoadValues._healthCoins.ToString("f0");
        MainController.Instance._currencyAssets[1]._quantityText.text = MainController.Instance._saveLoadValues._hintCoins.ToString("f0");
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
                _scriptMainController._bordersAnimator.SetBool("BorderOut", true);
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
            int displayNumber = _allWorlds.Length - i;

            var world = _allWorlds[i];
            if (world._worldText != null && !string.IsNullOrEmpty(world.key))
                world._worldText.text = gi.GetText(world.key) + " " + displayNumber;
        }



        for (int i = 0; i < _allWorlds.Length; i++)
        {
            var world = _allWorlds[i];

            //if (world._worldText != null && !string.IsNullOrEmpty(world.key))
            //    world._worldText.text = gi.GetText(world.key) + " " + (i + 1);

            if (world._lockedText != null)
                world._lockedText.text = gi.GetText("locked");
        }
    }

    void Update()
    {
        if (_scriptMainController == null) return;

        if (!_scriptMainController._introSpecial)
        {
            HandleMovement();
            HandleWorldSelection();
            UpdateWorldUnlocks();
        }
        else if (_falling)
        {
            _worldsParent.GetComponent<RectTransform>().anchoredPosition =
                Vector2.MoveTowards(_worldsParent.GetComponent<RectTransform>().anchoredPosition,
                new Vector2(0, _allWorlds[_scriptMainController._onWorldGlobal]._yPos),
                250f * Time.deltaTime);

            if (_worldsParent.GetComponent<RectTransform>().anchoredPosition.y ==
                _allWorlds[_scriptMainController._onWorldGlobal]._yPos && !_gameStarts)
            {
                StartCoroutine(StartGameSpecial());
            }
        }

        MainController.Instance._currencyAssets[0]._quantityText.text = MainController.Instance._saveLoadValues._healthCoins.ToString("f0");
        MainController.Instance._currencyAssets[1]._quantityText.text = MainController.Instance._saveLoadValues._hintCoins.ToString("f0");
    }

    private void HandleMovement()
    {
        if (!_changing)
        {
            var parentPos = _parent.GetComponent<RectTransform>().anchoredPosition;
            _parent.GetComponent<RectTransform>().anchoredPosition =
                Vector2.Lerp(parentPos, new Vector2(_xPoses[_OnPos], parentPos.y), _speed * Time.deltaTime);
        }

        if (Input.GetAxisRaw("Horizontal") == 0) _pressed = false;
        if (Input.GetAxisRaw("Vertical") == 0) _worldPressed = false;

        var worldPos = _worldsParent.GetComponent<RectTransform>().anchoredPosition;
        _worldsParent.GetComponent<RectTransform>().anchoredPosition =
            Vector2.Lerp(worldPos, new Vector2(worldPos.x, _allWorlds[_onWorldPos]._yPos), 5 * Time.deltaTime);

        _worldBackgroundImage.color =
            Color.Lerp(_worldBackgroundImage.color, _allWorlds[_onWorldPos]._backgroundColor, 2 * Time.deltaTime);
    }

    private void HandleWorldSelection()
    {
        if (!_gameStarts)
        {
            if (Input.GetAxisRaw("Vertical") < 0 && !_worldPressed && _onWorldPos < _allWorlds.Length - 1)
            {
                _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._next);
                _onWorldPos++;
                _worldPressed = true;
            }

            if (Input.GetAxisRaw("Vertical") > 0 && !_worldPressed && _onWorldPos > 0)
            {
                _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._next);
                _onWorldPos--;
                _worldPressed = true;
            }

            if (Input.GetButton("Submit") && _scriptMainController._saveLoadValues._worldsUnlocked[_onWorldPos])
            {
                StartCoroutine(StartGame());
            }
        }
    }

    private void UpdateWorldUnlocks()
    {
        for (int i = 0; i < _scriptMainController._saveLoadValues._worldsUnlocked.Length; i++)
        {
            _allWorlds[i]._lockedParemt.SetActive(!_scriptMainController._saveLoadValues._worldsUnlocked[i]);
            _allWorlds[i]._spaceButton.SetActive(_scriptMainController._saveLoadValues._worldsUnlocked[i]);
        }
    }

    public IEnumerator StartGameSpecial()
    {

        _gameStarts = true;
        _fallingSlime.Play();
        _logo.gameObject.SetActive(false);
        //_parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50f);
        _slimeParent.SetActive(false);
        yield return new WaitForSeconds(3);
        _scriptMainController._bordersAnimator.SetBool("BorderOut", false);

        yield return new WaitForSeconds(1);
        _scriptMainController._introSpecial = false;
        _scriptMainController.LoadSceneByName("MainGame");
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
        yield return new WaitForSeconds(2);
        _scriptMainController.LoadSceneByName("MainGame");
    }

    public void botonBorrar()
    {
        MainController.Instance._saveLoadValues._hintCoins++;
        MainController.Instance._currencyAssets[1]._quantityText.text = MainController.Instance._saveLoadValues._hintCoins.ToString("f0");
        MainController.Instance._saveLoadValues._healthCoins++;
        MainController.Instance._currencyAssets[0]._quantityText.text = MainController.Instance._saveLoadValues._hintCoins.ToString("f0");
        GameInitScript.Instance.SaveGame();
    }
}
