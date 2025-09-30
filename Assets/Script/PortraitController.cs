using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // 👈 Necesario para eventos de escena

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
        public bool _unlocked;
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

    // Suscribirse al evento de carga de escena
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _scriptMainController = GameObject.Find("CanvasIndestructible/MainController").GetComponent<MainController>();
        // Ejecutar CustomStart cada vez que se cargue una escena
        CustomStart();
    }

    public void CustomStart()
    {
        if (_scriptMainController._introSpecial)
        {
        
            _scriptMainController._bordersAnimator.SetBool("BorderOut", true);
            _frontMap.SetActive(false);
            _logo.gameObject.SetActive(false);
            _scriptMainController._cinematicBorders.SetBool("FadeIn", true);
            _parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50f);
            _falling = true;
            _slimeParent.SetActive(false);            
            //_scriptMainController._onWorldGlobal = _onWorldPos;
        }
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
                if (Input.GetAxisRaw("Vertical") < 0 && !_worldPressed && _onWorldPos < _allWorlds.Length - 1 && _allWorlds[_onWorldPos + 1]._unlocked)
                {
                    _onWorldPos++;
                    _worldPressed = true;
                }

                if (Input.GetAxisRaw("Vertical") > 0 && !_worldPressed && _onWorldPos > 0)
                {
                    _onWorldPos--;
                    _worldPressed = true;
                }

                if (Input.GetButton("Submit"))
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
                    new Vector2(0, _allWorlds[3]._yPos + 60), 250f * Time.deltaTime);

                if (_worldsParent.GetComponent<RectTransform>().anchoredPosition.y == _allWorlds[3]._yPos + 60 && !_gameStarts)
                {
                    StartCoroutine(StartGameSpecial());
                }
            }
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
        //_scriptMainController._onWorldGlobal = _onWorldPos;
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
        _slimeParent.SetActive(false);
        _fallingSlime.Play();
        _gameStarts = true;
        _scriptMainController._onWorldGlobal = _onWorldPos;
        yield return new WaitForSeconds(3);
        _scriptMainController._bordersAnimator.SetBool("BorderOut", false);
        yield return new WaitForSeconds(1);
        _scriptMainController.LoadSceneByName("MainGame");
    }
}
