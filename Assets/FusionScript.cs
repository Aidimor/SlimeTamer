using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FusionScript : MonoBehaviour
{
    [SerializeField] private SlimeController _scriptSlime;
    public bool _panelActive;
    public GameObject _fusionParent;
  
    [System.Serializable]
    public class ElementsOptions
    {
        public bool _unlocked;
        public GameObject _parent;
        public ParticleSystem _releaseParticles;
 
    }
    public ElementsOptions[] _elementsOptions;
    public int _onElement;
    bool _used;
    public float[] _xPoses;
    public int _totalUnlocked;
    public ParticleSystem[] _particleOnSlime;

    public int[] _elementChoosed;

 

    public List<int> _elementSelection = new List<int>();
    public bool _buttonsAvailable;
    public Color[] _halfColors;
 
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < _elementsOptions.Length; i++)
        {
            if (_elementsOptions[i]._unlocked)
            {
                _totalUnlocked++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_panelActive)
        {
            if (_buttonsAvailable)
            {
                if (Input.GetAxisRaw("Horizontal") > 0 && _onElement > 0 && !_used)
                {
                    _used = true;
                    _onElement--;
                    ChangeElement();
                }

                if (Input.GetAxisRaw("Horizontal") < 0 && _onElement < 2 && !_used)
                {
                    _used = true;
                    _onElement++;
                    ChangeElement();
                }

                if (Input.GetAxisRaw("Horizontal") == 0)
                {
                    _used = false;
                }

                if (Input.GetButtonDown("Submit"))
                {
                    _buttonsAvailable = false;
                    _particleOnSlime[0].gameObject.SetActive(true);
                    var particle1 = _particleOnSlime[0].main;
                    var particle2 = _particleOnSlime[1].main;
                    particle1.startColor = _elementsOptions[_onElement]._releaseParticles.main.startColor;
                    particle2.startColor = _elementsOptions[_onElement]._releaseParticles.main.startColor;
                    _elementsOptions[_onElement]._releaseParticles.Play();
                    _elementsOptions[_onElement]._parent.transform.localScale = new Vector3(1f, 1f, 1);
                    _elementChoosed[_onElement]++;          
                    _elementSelection.Add(_onElement);
                    _scriptSlime._materialColors[1] = _halfColors[_onElement];
                      //  _elementsOptions[_onElement]._parent.GetComponent<Image>().color;
                    StartCoroutine(ChooseElementNumerator());

                }
            }
          

            if(_elementSelection.Count == 1)
            {
                _scriptSlime.fillAmount = Mathf.Lerp(_scriptSlime.fillAmount, 0.5f, 1 * Time.deltaTime);
       
            }
     

      

            _fusionParent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_fusionParent.GetComponent<RectTransform>().anchoredPosition,
new Vector2(_xPoses[_totalUnlocked - 1], _fusionParent.GetComponent<RectTransform>().anchoredPosition.y), 25 * Time.deltaTime);
        }
        else
        {

            _fusionParent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_fusionParent.GetComponent<RectTransform>().anchoredPosition,
                new Vector2(-570, _fusionParent.GetComponent<RectTransform>().anchoredPosition.y), 25 * Time.deltaTime);

        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _panelActive = !_panelActive;
            for (int i = 0; i < 3; i++)
            {
                _elementsOptions[i]._parent.SetActive(false);
            }
            switch (_panelActive)
            {
                case false:
           

              
                    break;
                case true:
             

                    for (int i = 0; i < _totalUnlocked; i++)
                    {
                        _elementsOptions[i]._parent.SetActive(true);
                    }
                    break;
            }
        
        }

    }

    public void ChangeElement()
    {
        for(int i = 0; i < 3; i++)
        {
            _elementsOptions[i]._parent.transform.localScale = new Vector3(0.6f, 0.6f, 1);
        }
        _elementsOptions[_onElement]._parent.transform.localScale = new Vector3(0.85f,0.85f, 1);
    }

    public IEnumerator ChooseElementNumerator()
    {
        yield return null;
    }
}
