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
        public Image _blocker;
        [Header("Idioma")]
        public string key;
    }

    public ElementsInfo[] _elementsInfo;

    public GameObject _background;
    public bool _backgroundOn;
    public Vector2[] _backgroundPoses;
    public Vector2[] _positions;

    [Header("Rythm Settings")]
    public float _bpm;
    public int _subdivisions = 1;
    [HideInInspector] public float _timerInterval;

    [Header("Press Settings")]
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
    public Animator _movingSelector;

    public int _OnPhase;
    public TextMeshProUGUI _pressSpaceText;

    void Start()
    {
        UpdateWorldTexts();
        switch (_scriptMain._scriptMain._onWorldGlobal)
        {
            case 0:
                switch (_scriptMain._scriptMain._saveLoadValues._finalWorldUnlocked)
                {
                    case false:
                        _bpm = 100;
                        break;
                    case true:
                        _bpm = 160;
                        break;
                }
         
                break;
            case 1:
                _bpm = 150;
                break;
            case 2:
                _bpm = 135;
                break;
            case 3:
                _bpm = 120;
                break;
        }

        for (int i = 0; i < 4; i++)
        {
            _positions[i] = _elementsInfo[i]._parent.GetComponent<RectTransform>().anchoredPosition;
        }
    }

    public void UpdateWorldTexts()
    {
        for (int i = 0; i < _elementsInfo.Length; i++)
        {
            if (_elementsInfo[i]._elementText != null && !string.IsNullOrEmpty(_elementsInfo[i].key))
            {
                _elementsInfo[i]._elementText.text = GameInitScript.Instance.GetText(_elementsInfo[i].key);
            }
        }

        _scriptMain._choose2ElementsText.text = GameInitScript.Instance.GetText("choose");
        _pressSpaceText.text = GameInitScript.Instance.GetText("press");

    }

    void Update()
    {
        if (Input.GetButtonDown("Submit") && !_buttonPressed && _canPress && !_scriptMain._scriptMain._pauseAssets._pause)
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


        switch (_backgroundOn)
        {
            case true:
                _background.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_background.GetComponent<RectTransform>().anchoredPosition, _backgroundPoses[0], 15 * Time.deltaTime);
                break;
            case false:
                _background.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_background.GetComponent<RectTransform>().anchoredPosition, _backgroundPoses[1], 15 * Time.deltaTime);
                break;
        }

        //_scriptMain._spaceParent.gameObject.SetActive(true);
        _timerInterval = (60f / _bpm) / Mathf.Max(1, _subdivisions);

    }

    public void ShuffleElements()
    {
        _order.Clear();
        int n = _elementsInfo.Length;

        if (_positions == null || _positions.Length != n)
        {
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

    // =====================================================
    // CORRUTINA PRINCIPAL CON SOPORTE DE PAUSA
    // =====================================================
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

        _backgroundOn = true;
        yield return new WaitForSeconds(1);
        ShuffleElements();

        int eventStance = 0;
        bool endLoopAfterSelection = false;
    

        while (!endLoopAfterSelection)
        {


            // Esperar si el juego está en pausa
            while (MainController.Instance != null && MainController.Instance._pauseAssets._pause)
                yield return null;

            for (int step = 0; step < _order.Count; step++)
            {

                if (step == 0)
                {
                    for (int i = 0; i < 4; i++)
                        _elementsInfo[i]._blocker.gameObject.SetActive(false);
                }
                // Esperar si el juego está en pausa (por seguridad)
                while (MainController.Instance != null && MainController.Instance._pauseAssets._pause)
                    yield return null;

                int elementIndex = _order[step];
                customStep = step;
                _onElement = elementIndex;
  
                _elementsInfo[elementIndex]._selector.GetComponent<Animator>().SetTrigger("SelectorIn");
                _scriptSlime._slimeRawImage.transform.localScale = new Vector2(3f, 3f);
                _elementsInfo[elementIndex]._elementOrb.transform.localScale = new Vector2(1.25f, 1.25f);
                _movingSelector.GetComponent<RectTransform>().anchoredPosition =  new Vector2(_elementsInfo[_onElement]._parent.GetComponent<RectTransform>().anchoredPosition.x, _movingSelector.GetComponent<RectTransform>().anchoredPosition.y);
                _movingSelector.GetComponent<Image>().color = _halfColors[_onElement];
                _kickSound.pitch = step == 3 ? 1f : 0.9f;
                _scriptMain._mainUI.transform.localScale = step == 3 ?
                    new Vector2(1.05f, 1.05f) : new Vector2(1.01f, 1.01f);
                _kickSound.Play();

                _canPress = true;
                float timer = 0f;

                while (timer < _timerInterval)
                {
                    // Si el juego se pausa, congelar el tiempo del ritmo
                    if (MainController.Instance != null && MainController.Instance._pauseAssets._pause)
                    {
                        yield return null;
                        continue;
                    }

                    timer += Time.deltaTime;

                    if (timer >= _timerInterval * 0.5f && _canPress)
                        _canPress = false;

                    yield return null;
                }

                _canPress = false;

                if (_elementsSelection.Count == 2)
                    endLoopAfterSelection = true;
            }

            for (int i = 0; i < 4; i++)
                _elementsInfo[i]._imageColor.color = _halfColors[4];

            _buttonPressed = false;

            switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventType)
            {
                case GameEvent.EventType.Fire:
                    break;
                case GameEvent.EventType.BossFight1:
                case GameEvent.EventType.BossFight2:
                    if (!_scriptMain._scriptEvents._winRound)
                    {




                        switch (_OnPhase)
                        {
                            case 0:
                                _scriptMain._scriptMain._scriptSFX._chargeAttackPitch = 0.5f;
                                _scriptMain._scriptMain._scriptSFX._chargeAttackVolume = 1;
                              
                         
                                _scriptMain._bossAnimator.SetBool("SideShoot", true);
                                break;
                            case 2:
                                _scriptMain._scriptMain._scriptSFX._chargeAttackPitch = 0.75f;
                                _scriptMain._bossAnimator.SetTrigger("Action");
                                break;
                            case 4:
                                _scriptMain._scriptMain._scriptSFX._chargeAttackPitch = 1f;
                                _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", true);
                                _scriptMain._bossAnimator.SetTrigger("Action");
                                break;
                            case 6:
                                _scriptMain._scriptMain._scriptSFX._chargeAttackPitch = 0.75f;
                                _scriptMain._scriptMain._scriptSFX._chargeAttackVolume = 0;
                                _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._bossAttack);
                                _scriptMain._bossAnimator.SetTrigger("Action");
                                yield return new WaitForSeconds(0.25f);
                                if (!_scriptMain._dead)
                                {

                                    StartCoroutine(_scriptMain.LoseLifeNumerator());
                                    endLoopAfterSelection = true;
                                }
                                break;

                        }

                    }
                    break;
                case GameEvent.EventType.BossFight3:
                    if (!_scriptMain._scriptEvents._winRound)
                    {
                        switch (_OnPhase)
                        {
                            case 0:
                                _scriptSlime._slimeAnimator.SetBool("WindPush", true);                   
                                break;
          
                            case 2:
                                _scriptSlime._slimeAnimator.SetTrigger("Action");                 
                                break;
                            case 4:

                                _scriptSlime._slimeAnimator.SetTrigger("Action");
                                break;
                            case 5:
                                _scriptSlime._slimeAnimator.SetTrigger("Action");
                                break;
                            case 6:
                                _scriptSlime._slimeAnimator.SetTrigger("Action");
                                yield return new WaitForSeconds(0.25f);
                                if (!_scriptMain._dead)
                                {
                                    StartCoroutine(_scriptMain.LoseLifeNumerator());
                                    endLoopAfterSelection = true;
                                }
                                break;

                        }

                    }
                    break;
                case GameEvent.EventType.BossFight4:
                    if (!_scriptMain._scriptEvents._winRound)
                    {

                  

                        switch (_OnPhase)
                        {
                            case 0:
                                _scriptMain._scriptMain._scriptSFX._fireSetVolume = 1;
                                _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BossFightsScript>()._fire[0].Play();
                                _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._flameOn);
                                break;
                            case 1:
                                _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BossFightsScript>()._fire[1].Play();
                                _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._flameOn);
                                break;
                            case 3:
                
                                _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BossFightsScript>()._fire[2].Play();
                                _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._flameOn);
                                break;
                            case 5:
                                _scriptSlime._slimeAnimator.SetBool("Scared", true);
                                _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BossFightsScript>()._fire[3].Play();
                                _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._flameOn);

                                break;
                            case 7:
                                _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BossFightsScript>()._fire[4].Play();
                                _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._flameOn);
                                if (!_scriptMain._dead)
                                {
                                    StartCoroutine(_scriptMain.LoseLifeNumerator());
                                    endLoopAfterSelection = true;
                                }
                                break;
                        }
                    }
            
                    break;

            }
            _OnPhase++;

            if (eventStance < 3) eventStance++;
        }

        //_scriptMain._spaceParent.gameObject.SetActive(false);

        if (_elementsSelection.Count == 2)
           FuseElements();

        _onElement = 0;
        _backgroundOn = false;
        yield return new WaitForSeconds(1);

        for (int i = 0; i < 4; i++)
        {
            _elementsInfo[i]._parent.SetActive(false);
            _elementsInfo[i]._imageColor.color = _halfColors[4];     
        }
      
    }

    // =====================================================

    public void ChooseElementVoid()
    {
        _buttonPressed = true;
        _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._chooseElement);
        _movingSelector.Play("MovingSelectorOn");
        _scriptSlime._slimeRawImage.transform.localScale = new Vector2(4f, 4f);
        _elementsInfo[_onElement]._elementOrb.transform.localScale = new Vector2(2f, 2f);
        _elementsInfo[customStep]._releaseParticles.startColor = _halfColors[_onElement];
        _elementsInfo[customStep]._releaseParticles.Play();
        _chooseSound.Play();
        _elementsSelection.Add(_onElement);
        _scriptSlime._materialColors[_elementsSelection.Count] = _halfColors[_onElement];

        for (int i = 0; i < 4; i++)
        {
            _elementsInfo[i]._blocker.gameObject.SetActive(true);
        }
        if (_elementsSelection.Count > 1)
            _scriptMain._scriptEvents._winRound = true;
    }

    public void FuseElements()
    { 
        int e1 = _elementsSelection[0];
        int e2 = _elementsSelection[1];

        if (e1 == e2) _scriptSlime._slimeType = e1;
        else if ((e1 == 3 && e2 == 2) || (e1 == 2 && e2 == 3)) _scriptSlime._slimeType = 4;
        else if ((e1 == 2 && e2 == 1) || (e1 == 1 && e2 == 2)) _scriptSlime._slimeType = 5;
        else if ((e1 == 3 && e2 == 1) || (e1 == 1 && e2 == 3)) _scriptSlime._slimeType = 6;

        _elementsSelection.Clear();
        _scriptSlime.DeactivateElementsInfo(); 
        StartCoroutine(_scriptSlime.ActionSlimeNumerator());
    }
}
