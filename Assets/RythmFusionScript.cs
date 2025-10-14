using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoL;

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
        [Header("Idioma")]
        public string key;
    }

    public ElementsInfo[] _elementsInfo;
    public Vector2[] _positions;

    [Header("Rythm Settings")]
    [Tooltip("Beats per minute (quarter notes). Example: 120.")]
    public float _bpm = 120f;

    [Tooltip("Subdivisions per quarter note. 1 = quarter, 2 = eighths, 4 = sixteenths.")]
    public int _subdivisions = 1;

    [HideInInspector] public float _timerInterval;

    [Header("Press Settings")]
    [Tooltip("Window to press the button per step in seconds.")]
    public float _pressWindow = 0.3f;
    [HideInInspector] public bool _canPress = false;

    public int _onElement;
    public int _onElementChoosed;
    public List<int> _order = new List<int>();
    public Color[] _halfColors;
    public List<int> _elementsSelection = new List<int>();
    public bool _elementChoosed;
    public bool _buttonPressed;
    public int customStep;

    [Header("Audio")]
    public AudioSource _kickSound;
    public AudioSource _chooseSound;

    void Start()
    {
        UpdateWorldTexts();
    }

    public void UpdateWorldTexts()
    {
        for (int i = 0; i < _elementsInfo.Length; i++)
        {
            if (_elementsInfo[i]._elementText != null && !string.IsNullOrEmpty(_elementsInfo[i].key + (i + 1).ToString()))
            {
                _elementsInfo[i]._elementText.text = GameInitScript.Instance.GetText(_elementsInfo[i].key);
            }
        }
        _scriptMain._choose2ElementsText.text = GameInitScript.Instance.GetText("choose");     
        _scriptMain._spaceText.text = GameInitScript.Instance.GetText("choose2");

    }

    void Update()
    {
        if (Input.GetButtonDown("Submit") && !_buttonPressed && _canPress)
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

        for (int i = 0; i < _elementsInfo.Length; i++)
        {
            _elementsInfo[i]._elementOrb.transform.localScale =
                Vector2.Lerp(_elementsInfo[i]._elementOrb.transform.localScale, new Vector2(1, 1), 4 * Time.deltaTime);
        }

         _scriptMain._spaceParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(_elementsInfo[_onElement]._parent.GetComponent<RectTransform>().anchoredPosition.x - 100,
             _scriptMain._spaceParent.transform.localPosition.y);



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

        List<int> availableElements = new List<int>();
        for (int i = 0; i < n; i++) availableElements.Add(i);

        for (int j = 0; j < n; j++)
        {
            int rand = Random.Range(0, availableElements.Count);
            int chosenElementIndex = availableElements[rand];
            _elementsInfo[chosenElementIndex]._parent.GetComponent<RectTransform>().anchoredPosition = _positions[j];
            _order.Add(chosenElementIndex);
            availableElements.RemoveAt(rand);
        }

        for (int i = 0; i < n; i++)
        {
            _elementsInfo[i]._parent.SetActive(true);
            _elementsInfo[i]._elementOrb.SetActive(_scriptMain._scriptMain._saveLoadValues._elementsUnlocked[i]);
        }
    }

    public IEnumerator RythmNumerator()
    {
        _elementChoosed = false;
        _buttonPressed = false;

        if (_order == null || _order.Count != _elementsInfo.Length)
        {
            _order.Clear();
            for (int i = 0; i < _elementsInfo.Length; i++)
                _order.Add(i);
        }

        ShuffleElements();
        yield return new WaitForSeconds(1);

        _scriptMain._spaceParent.gameObject.SetActive(true);
        _timerInterval = (60f / _bpm) / Mathf.Max(1, _subdivisions);

        int eventStance = 0;
        bool endLoopAfterSelection = false;

        while (!endLoopAfterSelection)
        {
            for (int step = 0; step < _order.Count; step++)
            {
                int elementIndex = _order[step];
                customStep = step;
                _onElement = elementIndex;

                _elementsInfo[elementIndex]._selector.GetComponent<Animator>().SetTrigger("SelectorIn");
                _scriptSlime._slimeRawImage.transform.localScale = new Vector2(3f, 3f);
                _elementsInfo[elementIndex]._elementOrb.transform.localScale = new Vector2(1.25f, 1.25f);

                _kickSound.pitch = step == 3 ? 1f : 0.9f;
                _scriptMain._mainUI.transform.localScale = step == 3 ? new Vector2(1.05f, 1.05f) : new Vector2(1.01f, 1.01f);
                _kickSound.Play();

                _canPress = true;

                float timer = 0f;
           

                while (timer < _timerInterval)
                {
                    timer += Time.unscaledDeltaTime;
                    yield return null;
                }

                _canPress = false;

                // Cuando ya se eligieron 2 elementos, marcamos que terminaremos después del loop actual
                if (_elementsSelection.Count == 2)
                {
                    endLoopAfterSelection = true;
                }
            }

            for (int i = 0; i < 4; i++)
                _elementsInfo[i]._imageColor.color = _halfColors[0];

            _buttonPressed = false;

            switch (_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent])
            {
                case 12:
                    int _real = eventStance + 1;
                    _scriptMain._scriptEvents._currentEventPrefab
                        .GetComponent<FireEventScript>()._fireParticle[_real].Play();
                    if(_real == 3)
                    {
                        _scriptSlime._slimeAnimator.SetBool("Scared", true);
                    }else if(_real == 4)
                    {
                        if (!_scriptMain._dead)
                        {
                            StartCoroutine(_scriptMain.LoseLifeNumerator());
                            endLoopAfterSelection = true;
                        }
                        
                    }
                    break;
            }

            if (eventStance < 3) eventStance++;
        }
        _scriptMain._spaceParent.gameObject.SetActive(false);

        // Después de terminar el loop donde se eligió el segundo elemento
        if (_elementsSelection.Count == 2)
            StartCoroutine(FuseElements());

        _onElement = 0;
        yield return new WaitForSeconds(1);

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
        _elementsInfo[customStep]._releaseParticles.startColor = _halfColors[_onElement];
        _elementsInfo[customStep]._releaseParticles.Play();
        _chooseSound.Play();
        _elementsSelection.Add(_onElement);
        _scriptSlime._materialColors[_elementsSelection.Count] = _halfColors[_onElement];
    }

    public IEnumerator FuseElements()
    {
        _scriptMain._slimeChanging = true;
        _scriptMain._lightChanging = true;
        _scriptMain._darkenerChanging = true;
        _elementChoosed = true;
        int e1 = _elementsSelection[0];
        int e2 = _elementsSelection[1];

        if (e1 == e2) _scriptSlime._slimeType = e1;
        else if ((e1 == 3 && e2 == 2) || (e1 == 2 && e2 == 3)) _scriptSlime._slimeType = 4;
        else if ((e1 == 2 && e2 == 1) || (e1 == 1 && e2 == 2)) _scriptSlime._slimeType = 5;
        else if ((e1 == 3 && e2 == 1) || (e1 == 1 && e2 == 3)) _scriptSlime._slimeType = 6;

        _elementsSelection.Clear();
        _scriptSlime.DeactivateElementsInfo();
        yield return new WaitForSeconds(1f);
        _scriptMain._shineParticle.Play();     
        StartCoroutine(_scriptSlime.ActionSlimeNumerator()); 
    }
}
