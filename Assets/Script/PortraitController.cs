using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PortraitController : MonoBehaviour
{
    [SerializeField] MainController _scriptMainController;
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

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_changing){
            _parent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_parent.GetComponent<RectTransform>().anchoredPosition, new Vector2(_xPoses[_OnPos],
    _parent.GetComponent<RectTransform>().anchoredPosition.y), _speed * Time.deltaTime);
        }
   


        if (Input.GetAxisRaw("Horizontal") == 0)
        {
            _pressed = false;
        }

        if (Input.GetAxisRaw("Vertical") == 0)
        {

            _worldPressed = false;
        }


     
        _worldsParent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_worldsParent.GetComponent<RectTransform>().anchoredPosition,
new Vector2(_worldsParent.GetComponent<RectTransform>().anchoredPosition.x, _allWorlds[_onWorldPos]._yPos), 5 * Time.deltaTime);

        _worldBackgroundImage.color = Color.Lerp(_worldBackgroundImage.color, _allWorlds[_onWorldPos]._backgroundColor, 2 * Time.deltaTime);

        if (!_gameStarts)
        {
            if (Input.GetAxisRaw("Vertical") < 0 && !_worldPressed && _onWorldPos < _allWorlds.Length - 1)
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

        _parent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_parent.GetComponent<RectTransform>().anchoredPosition, new Vector2(0, 0), 15 * Time.deltaTime);
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

