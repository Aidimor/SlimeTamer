using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FusionScript : MonoBehaviour
{
    [SerializeField] private SlimeController _scriptSlime;
    
    public GameObject _slimeRenderer;
    public Vector2[] _slimeStartPos;
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

    public bool _correctAction;
    public float _slimeSpeed;
 
    // Start is called before the first frame update
    public void UnlockElements()
    {
        _totalUnlocked = 0;
        for (int i = 0; i < _elementsOptions.Length; i++)
        {
            if (_elementsOptions[i]._unlocked)
            {
                _totalUnlocked++;
            }
        }
        switch (_totalUnlocked)
        {
            case 1:
                _onElement = 0;
                break;
            case 2:
                _onElement = 1;
                break;
            case 3:
                _onElement = 2;
                break;
        }
        ChangeElement();
    }

    // Update is called once per frame
    void Update()
    {
        if (_panelActive)
        {
            if (_buttonsAvailable)
            {
                if (_totalUnlocked > 1)
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
                }


                if (Input.GetButtonDown("Submit"))
                {                
           
                    StartCoroutine(ChooseElementNumerator());

                }
            }
          


       



        if(_totalUnlocked > 0)
            {

    
            _fusionParent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_fusionParent.GetComponent<RectTransform>().anchoredPosition,
new Vector2(_xPoses[_totalUnlocked - 1], _fusionParent.GetComponent<RectTransform>().anchoredPosition.y), 25 * Time.deltaTime);
        }
        else
        {

            _fusionParent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_fusionParent.GetComponent<RectTransform>().anchoredPosition,
                new Vector2(-600, _fusionParent.GetComponent<RectTransform>().anchoredPosition.y), 25 * Time.deltaTime);

        }


        if (_elementSelection.Count == 1)
        {
            _scriptSlime.fillAmount = Mathf.Lerp(_scriptSlime.fillAmount, 0.5f, 1 * Time.deltaTime);

        }

        else if (_elementSelection.Count == 2)
        {
            _scriptSlime.fillAmount = Mathf.Lerp(_scriptSlime.fillAmount, 1f, 1 * Time.deltaTime);

        }

        else if (_elementSelection.Count == 0)
        {
            _scriptSlime.fillAmount = Mathf.Lerp(_scriptSlime.fillAmount, 0, 1 * Time.deltaTime);

        }
        }
        else
        {
            _fusionParent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_fusionParent.GetComponent<RectTransform>().anchoredPosition,
    new Vector2(-600, _fusionParent.GetComponent<RectTransform>().anchoredPosition.y), 25 * Time.deltaTime);
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
        _particleOnSlime[0].gameObject.SetActive(true);
        var particle1 = _particleOnSlime[0].main;
        var particle2 = _particleOnSlime[1].main;
        particle1.startColor = _elementsOptions[_onElement]._releaseParticles.main.startColor;
        particle2.startColor = _elementsOptions[_onElement]._releaseParticles.main.startColor;
        _elementsOptions[_onElement]._releaseParticles.Play();
        _elementsOptions[_onElement]._parent.transform.localScale = new Vector3(1f, 1f, 1);
        _elementChoosed[_onElement]++;
        _elementSelection.Add(_onElement);
        _scriptSlime._materialColors[_elementSelection.Count] = _halfColors[_onElement];
        _buttonsAvailable = false;

        for (int i = 0; i < 3; i++)
        {
            _elementsOptions[i]._parent.transform.localScale = new Vector3(0.6f, 0.6f, 1);
        }

        if(_elementSelection.Count == 2)
        {
     
            _buttonsAvailable = false;
            yield return new WaitForSeconds(0.5f);
            _panelActive = false;
            for (int i = 0; i < 3; i++)
            {
                _elementsOptions[i]._parent.gameObject.SetActive(false);
            }
            FuseElements();
        }


        yield return new WaitForSeconds(1);
        _buttonsAvailable = true;
        yield return null;
    }

    public void FuseElements()
    {
    
        if (_elementSelection.Count < 2)
        {
            Debug.Log("Need at least 2 elements to fuse!");
            //return;
        }

        int e1 = _elementSelection[0];
        int e2 = _elementSelection[1];

        // Same element → produce that element's slime
        if (e1 == e2)
        {
            Debug.Log($"Created {_elementsOptions[e1]._parent.name} Slime!");
            _scriptSlime._slimeType = e1 + 1;
            Debug.Log(e1 + 1);
            _elementSelection.Clear();
            _particleOnSlime[0].gameObject.SetActive(false);
            StartCoroutine(_scriptSlime.ActionSlimeNumerator());
            _scriptSlime.ChangeSlime();
            //return;
        }

        // Fusion combinations
        if ((e1 == 2 && e2 == 1) || (e1 == 1 && e2 == 2))
        {
            // Earth + Wind = Sand
            Debug.Log("Created Sand Slime!");
            _scriptSlime._slimeType = 4;
            _elementSelection.Clear();
            _particleOnSlime[0].gameObject.SetActive(false);
            StartCoroutine(_scriptSlime.ActionSlimeNumerator());
            _scriptSlime.ChangeSlime();
        }
        else if ((e1 == 1 && e2 == 0) || (e1 == 0 && e2 == 1))
        {
            // Wind + Water = Snow
            Debug.Log("Created Snow Slime!");
            _scriptSlime._slimeType = 5;
            _elementSelection.Clear();
            _particleOnSlime[0].gameObject.SetActive(false);
            StartCoroutine(_scriptSlime.ActionSlimeNumerator());
            _scriptSlime.ChangeSlime();
        }
        else if ((e1 == 2 && e2 == 0) || (e1 == 0 && e2 == 2))
        {
            // Earth + Water = Sticky Mud
            Debug.Log("Created Sticky Mud Slime!");
            _scriptSlime._slimeType = 6;
            _elementSelection.Clear();
            _particleOnSlime[0].gameObject.SetActive(false);
            StartCoroutine(_scriptSlime.ActionSlimeNumerator());
            _scriptSlime.ChangeSlime();
        }
     
      
   
    }

    public IEnumerator ActivatePanel()
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
                yield return new WaitForSeconds(1);
                _buttonsAvailable = true;
                break;
        }
    }



}
