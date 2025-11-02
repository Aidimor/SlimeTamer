using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // 👈 Necesario para eventos de escena
using LoL;

public class PortraitController : MonoBehaviour
{
    [SerializeField] MainController _scriptMainController;

    public bool _falling;
    public float _fallSpeed;
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
        //public bool _unlocked;
        public TextMeshProUGUI _worldText;
        public GameObject _spaceButton;
        public GameObject _lockedParemt;
        public TextMeshProUGUI _lockedText;
        [Header("Idioma")]
        public string key; // 👈 clave que se buscará en el JSON (ej: "world1")
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

    // 👇 Se llama cuando el objeto se activa
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 🔹 Si ya estamos dentro de una escena cargada, forzamos la llamada manual
        if (SceneManager.GetActiveScene().isLoaded)
        {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        _scriptMainController._scriptMusic._audioBGM.clip = _scriptMainController._scriptMusic._allThemes[0];
        _scriptMainController._scriptMusic._audioBGM.Play();
        StartCoroutine(UpdateWorldTexts());
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 👇 Este método se ejecuta SIEMPRE que se carga una escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var mainControllerObj = GameObject.Find("CanvasIndestructible/Main/MainController");
        if (mainControllerObj != null)
        {
            _scriptMainController = mainControllerObj.GetComponent<MainController>();

            if (_scriptMainController != null)
            {
                _scriptMainController._bordersAnimator.SetBool("BorderOut", true);
                CustomStart();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró 'CanvasIndestructible/Main/MainController' en la jerarquía.");
        }


    }

    public IEnumerator UpdateWorldTexts()
    {
        // Espera a que el idioma esté listo
        yield return new WaitUntil(() =>
            LoL.GameInitScript.Instance != null &&
            LoL.GameInitScript.Instance.languageReady
        );

        var gi = GameInitScript.Instance;

        // Recorre todos los mundos
        for (int i = 0; i < _allWorlds.Length; i++)
        {
            var world = _allWorlds[i];

            // 🟦 Actualiza el texto del mundo
            if (world._worldText != null && !string.IsNullOrEmpty(world.key))
            {
                string text = gi.GetText(world.key);
                world._worldText.text = text + " " + (i + 1);
            }

            // 🔒 Actualiza el texto de "locked"
            if (world._lockedText != null)
            {
                string lockedText = gi.GetText("locked"); // clave exacta del JSON
                world._lockedText.text = lockedText;
            }
        }
    }



    public void CustomStart()
    {
      
        if (_scriptMainController._introSpecial)
        {
            for(int i = 0; i < _scriptMainController._saveLoadValues._worldsUnlocked.Length; i++)
            {
                _allWorlds[i]._worldText.gameObject.SetActive(false);
                _allWorlds[i]._lockedParemt.gameObject.SetActive(false);
                _allWorlds[i]._spaceButton.gameObject.SetActive(false);
            }

            _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._falling);
            _scriptMainController._bordersAnimator.SetBool("BorderOut", true);
            _frontMap.SetActive(false);
            _logo.gameObject.SetActive(false);
            _scriptMainController._cinematicBorders.SetBool("FadeIn", true);
            _parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50f);
            _falling = true;
            _slimeParent.SetActive(false);
        }


        // 🔹 Actualiza los textos de los mundos según el idioma cargado
        UpdateWorldTexts();
    }



    void Update()
    {
        if (!_scriptMainController._introSpecial)
        {
            if (!_changing)
            {
                _parent.GetComponent<RectTransform>().anchoredPosition =
                    Vector2.Lerp(_parent.GetComponent<RectTransform>().anchoredPosition,
                    new Vector2(_xPoses[_OnPos], _parent.GetComponent<RectTransform>().anchoredPosition.y),
                    _speed * Time.deltaTime);
            }

            if (Input.GetAxisRaw("Horizontal") == 0)
                _pressed = false;

            if (Input.GetAxisRaw("Vertical") == 0)
                _worldPressed = false;

            _worldsParent.GetComponent<RectTransform>().anchoredPosition =
                Vector2.Lerp(_worldsParent.GetComponent<RectTransform>().anchoredPosition,
                new Vector2(_worldsParent.GetComponent<RectTransform>().anchoredPosition.x, _allWorlds[_onWorldPos]._yPos),
                5 * Time.deltaTime);

            _worldBackgroundImage.color =
                Color.Lerp(_worldBackgroundImage.color, _allWorlds[_onWorldPos]._backgroundColor, 2 * Time.deltaTime);

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

            _parent.GetComponent<RectTransform>().anchoredPosition =
                Vector2.Lerp(_parent.GetComponent<RectTransform>().anchoredPosition,
                new Vector2(0, 0), 15 * Time.deltaTime);
        }
        else
        {
            if (_falling)
            {
                _worldsParent.GetComponent<RectTransform>().anchoredPosition =
                    Vector2.MoveTowards(_worldsParent.GetComponent<RectTransform>().anchoredPosition,
                    new Vector2(0, _allWorlds[_scriptMainController._onWorldGlobal]._yPos + 60), 250f * Time.deltaTime);

                if (_worldsParent.GetComponent<RectTransform>().anchoredPosition.y == _allWorlds[_scriptMainController._onWorldGlobal]._yPos + 60 && !_gameStarts)
                {
                    StartCoroutine(StartGameSpecial());
                }
            }
        }
        if (!_scriptMainController._introSpecial)
        {
            for (int i = 0; i < _scriptMainController._saveLoadValues._worldsUnlocked.Length; i++)
            {
                _allWorlds[i]._lockedParemt.gameObject.SetActive(!_scriptMainController._saveLoadValues._worldsUnlocked[i]);
                _allWorlds[i]._spaceButton.gameObject.SetActive(_scriptMainController._saveLoadValues._worldsUnlocked[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _scriptMainController._scriptInit.ClearSDKSaveManually();
        }

    }

    public IEnumerator StartGameSpecial()
    {
        _gameStarts = true;
        _fallingSlime.Play();
        _logo.gameObject.SetActive(false);
        _scriptMainController._cinematicBorders.SetBool("FadeIn", true);
        _parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50f);
        _slimeParent.SetActive(false);

        yield return new WaitForSeconds(3);
        _scriptMainController._bordersAnimator.SetBool("BorderOut", false);
        _scriptMainController._introSpecial = false;
        yield return new WaitForSeconds(1);
        _scriptMainController.LoadSceneByName("MainGame");
    }

    public IEnumerator StartGame()
    {
        _logo.gameObject.SetActive(false);
        _scriptMainController._cinematicBorders.SetBool("FadeIn", true);
        _parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50f);
        _explosionSlimeParticle.Play();
        _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._jump);
        _slimeParent.SetActive(false);
        _fallingSlime.Play();
        _gameStarts = true;
        _scriptMainController._onWorldGlobal = _onWorldPos;
        yield return new WaitForSeconds(1);
        _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._fall);
        yield return new WaitForSeconds(2);
        _scriptMainController._bordersAnimator.SetBool("BorderOut", false);
        yield return new WaitForSeconds(1);
        _scriptMainController.LoadSceneByName("MainGame");
    }
}

