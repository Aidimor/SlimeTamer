using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FusionScript : MonoBehaviour
{
    public bool _panelActive;
    public GameObject _fusionParent;
    [System.Serializable]
    public class ElementsOptions
    {
        public bool _unlocked;
        public GameObject _parent;
 
    }
    public ElementsOptions[] _elementsOptions;
    public int _onElement;
    bool _used;
    public float[] _xPoses;
    public int _totalUnlocked;
    public ParticleSystem _particleOnSlime;
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
            if(Input.GetAxisRaw("Horizontal") > 0 && _onElement > 0 && !_used)
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
                _particleOnSlime.gameObject.SetActive(true);
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
}
