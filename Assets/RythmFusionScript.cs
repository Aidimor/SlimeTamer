using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RythmFusionScript : MonoBehaviour
{
    [SerializeField] private MainGameplayScript _scriptMain;
    [SerializeField] private SlimeController _scriptSlime;
    [System.Serializable]
    public class ElementsInfo
    {
        public GameObject _parent;
        public GameObject _selector;
        public GameObject _elementOrb;
        public Image _imageColor;
        public ParticleSystem _releaseParticles;
        public TextMeshProUGUI _elementText;
        //public bool _unlocked;
        [Header("Idioma")]
        public string key; // 👈 clave que se buscará en el JSON (ej: "world1")
    }
    public ElementsInfo[] _elementsInfo;

    public Vector2[] _positions;

    public float _timerInterval;
    public int _onElement;
    public int _onElementChoosed;

    // store activation order as element indices (e.g. [2,0,3,1])
    public List<int> _order = new List<int>();

    //public ParticleSystem[] _particleOnSlime;

    //public int[] _elementChoosed;
    public Color[] _halfColors;
    public List<int> _elementsSelection = new List<int>();
    public bool _elementChoosed;
    public bool _buttonPressed;

    void Start()
    {
        //// initial shuffle (optional)
        //ShuffleElements();
        //// ensure positions array matches elements length; if not, capture current parents' positions
        //if (_positions == null || _positions.Length != _elementsInfo.Length)
        //{
        //    _positions = new Vector2[_elementsInfo.Length];
        //    for (int i = 0; i < _elementsInfo.Length; i++)
        //    {
        //        _positions[i] = _elementsInfo[i]._parent.GetComponent<RectTransform>().anchoredPosition;
        //    }
        //}
        UpdateWorldTexts();


    }

    public void UpdateWorldTexts()
    {
        for (int i = 0; i < _elementsInfo.Length; i++)
        {
            if (_elementsInfo[i]._elementText != null && !string.IsNullOrEmpty(_elementsInfo[i].key + (i + 1).ToString()))
            {
                _elementsInfo[i]._elementText.text = LanguageManager.Instance.GetText(_elementsInfo[i].key);
            }
        }

        _scriptMain._choose2ElementsText.text = LanguageManager.Instance.GetText("choose");
    }

    void Update()
    {


        if (Input.GetButtonDown("Submit") && !_buttonPressed)
        {
            switch (_onElement)
            {
                case 0:
                    _elementsInfo[0]._imageColor.color = _halfColors[0];
                    break;
                case 1:
                    if (_scriptMain._scriptMain._saveLoadValues._elementsUnlocked[1])
                    {
                        _elementsInfo[1]._imageColor.color = _halfColors[1];
                        ChooseElementVoid();
                    }
      
                    break;
                case 2:
                    if (_scriptMain._scriptMain._saveLoadValues._elementsUnlocked[2])
                    {
                        _elementsInfo[2]._imageColor.color = _halfColors[2];
                        ChooseElementVoid();
                    }
                    break;
                case 3:
                    if (_scriptMain._scriptMain._saveLoadValues._elementsUnlocked[3])
                    {
                        _elementsInfo[3]._imageColor.color = _halfColors[3];
                        ChooseElementVoid();
                    }
                    break;
            }

        
        }

        for(int i = 0; i < _elementsInfo.Length; i++)
        {
            _elementsInfo[i]._elementOrb.transform.localScale =
                Vector2.Lerp(_elementsInfo[i]._elementOrb.transform.localScale, new Vector2(1, 1), 4 * Time.deltaTime);
        }
    }

    public void ShuffleElements()
    {
        _order.Clear();

        int n = _elementsInfo.Length;
        if (_positions == null || _positions.Length != n)
        {
            Debug.LogWarning("Positions length mismatch — rebuilding from parents.");
            _positions = new Vector2[n];
            for (int i = 0; i < n; i++)
                _positions[i] = _elementsInfo[i]._parent.GetComponent<RectTransform>().anchoredPosition;
        }

        // pool of element indices not yet placed
        List<int> availableElements = new List<int>();
        for (int i = 0; i < n; i++) availableElements.Add(i);

        // for each position slot j (0..n-1) pick a random element and place it there.
        for (int j = 0; j < n; j++)
        {
            int rand = Random.Range(0, availableElements.Count);
            int chosenElementIndex = availableElements[rand];

            // assign position j to that element
            _elementsInfo[chosenElementIndex]._parent.GetComponent<RectTransform>().anchoredPosition = _positions[j];

            // record that at activation step j we should activate 'chosenElementIndex'
            _order.Add(chosenElementIndex);

            // remove chosen from available pool
            availableElements.RemoveAt(rand);
        }

        // make sure parents are active
        for (int i = 0; i < n; i++)
        {
            _elementsInfo[i]._parent.SetActive(true);
            _elementsInfo[i]._elementOrb.SetActive(_scriptMain._scriptMain._saveLoadValues._elementsUnlocked[i]);
         
        }
      

        // debug print
        string orderStr = "";
        for (int i = 0; i < _order.Count; i++)
            orderStr += _order[i] + (i < _order.Count - 1 ? "," : "");
        //Debug.Log("Shuffle order (element indices): " + orderStr);
    }

    public IEnumerator RythmNumerator()
    {
        _elementChoosed = false;
        _buttonPressed = false;
        // fallback: if _order not set, use sequential 0..n-1
        if (_order == null || _order.Count != _elementsInfo.Length)
        {
            _order.Clear();
            for (int i = 0; i < _elementsInfo.Length; i++)
                _order.Add(i);
        }

        ShuffleElements();
        yield return new WaitForSeconds(1);
        int eventStance = 0;
        // Repeat until element is chosen
        while (!_elementChoosed)
        {
   
            for (int step = 0; step < _order.Count; step++)
            {
                int elementIndex = _order[step];
                _onElement = elementIndex;

                _elementsInfo[elementIndex]._selector
                    .GetComponent<Animator>()
                    .SetTrigger("SelectorIn");
                _scriptSlime._slimeRawImage.transform.localScale = new Vector2(3f, 3f);
                _elementsInfo[elementIndex]._elementOrb.transform.localScale = new Vector2(1.25f, 1.25f);
                if(step == 3)
                {
                    _scriptMain._mainUI.transform.localScale = new Vector2(1.05f, 1.05f);
                }
                else
                {
                    _scriptMain._mainUI.transform.localScale = new Vector2(1.01f, 1.01f);
                }
       
                yield return new WaitForSeconds(_timerInterval);
         
                if (_elementChoosed) // break out if chosen mid-loop
                    yield break;
            }

            for (int i = 0; i < 4; i++)
            {
                _elementsInfo[i]._imageColor.color = _halfColors[0];
            }
            _buttonPressed = false;

            switch (_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent])
            {
                case 12:
                    _scriptMain._scriptEvents._currentEventPrefab.GetComponent<FireEventScript>()._fireParticle[eventStance + 1].Play();
                    break;
            }
            if (eventStance < 3)
            {
                eventStance++;
            }

        }

        // When loop ends (_elementChoosed == true), reset
        _onElement = 0;
        yield return new WaitForSeconds(1);

        // make sure parents are inactive
        for (int i = 0; i < 4; i++)
        {
            _elementsInfo[i]._parent.SetActive(false);
            _elementsInfo[i]._imageColor.color = _halfColors[0];
        }
    }


    public void ChooseElementVoid()
    {
        _buttonPressed = true;
        _scriptSlime._slimeRawImage.transform.localScale = new Vector2(4f, 4f);
        _elementsInfo[_onElement]._elementOrb.transform.localScale = new Vector2(2f, 2f);
        //_particleOnSlime[0].gameObject.SetActive(true);
        //var particle1 = _particleOnSlime[0].main;
        //var particle2 = _particleOnSlime[1].main;
        //particle1.startColor = _elementsInfo[_onElement]._releaseParticles.main.startColor;
        //particle2.startColor = _elementsInfo[_onElement]._releaseParticles.main.startColor;


        if (_onElement != 0){
            _elementsInfo[_onElement]._releaseParticles.Play();
        }
        //_elementsInfo[_onElement]._parent.transform.localScale = new Vector3(1f, 1f, 1);
        //_elementChoosed[_onElementChoosed]++;
        _elementsSelection.Add(_onElement);
        _scriptSlime._materialColors[_elementsSelection.Count] = _halfColors[_onElement];
        //_buttonsAvailable = false;

        //for (int i = 0; i < 3; i++)
        //{
        //    _elementsInfo[i]._parent.transform.localScale = new Vector3(0.6f, 0.6f, 1);
        //}

        if (_elementsSelection.Count == 2)
        {
           StartCoroutine(FuseElements());
        }
    }

    public IEnumerator FuseElements()
    {
        _elementChoosed = true;
        int e1 = _elementsSelection[0];
        int e2 = _elementsSelection[1];

        // Same element → produce that element's slime
        if (e1 == e2)
        {
            _scriptSlime._slimeType = e1;
            _elementsSelection.Clear();
            StartCoroutine(_scriptSlime.ActionSlimeNumerator());
            _scriptSlime.ChangeSlime();
        }

        // Fusion combinations
        if ((e1 == 3 && e2 == 2) || (e1 == 2 && e2 == 3))
        {
            _scriptSlime._slimeType = 4;
            _elementsSelection.Clear();
            StartCoroutine(_scriptSlime.ActionSlimeNumerator());
            _scriptSlime.ChangeSlime();
        }
        else if ((e1 == 2 && e2 == 1) || (e1 == 1 && e2 == 2))
        {
            _scriptSlime._slimeType = 5;
            _elementsSelection.Clear();
            StartCoroutine(_scriptSlime.ActionSlimeNumerator());
            _scriptSlime.ChangeSlime();
        }
        else if ((e1 == 3 && e2 == 1) || (e1 == 1 && e2 == 3))
        {
            _scriptSlime._slimeType = 6;
            _elementsSelection.Clear();
            StartCoroutine(_scriptSlime.ActionSlimeNumerator());
            _scriptSlime.ChangeSlime();
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 4; i++)
        {
            _elementsInfo[i]._parent.gameObject.SetActive(false);
            _elementsInfo[i]._selector.transform.localScale = new Vector2(0, 0);
            _elementsInfo[i]._imageColor.color = _halfColors[0];
        }
    }
}
