using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LoL;

public class SlimeController : MonoBehaviour
{
    [SerializeField] private MainGameplayScript _scriptMain;
    public int _slimeType; //0 = Null, 1 = Water, 2 = Air, 3 = Earth
    public Animator _slimeAnimator;
    public RawImage _slimeRawImage;
    public ParticleSystem[] _allParticles;

    [System.Serializable]
    public class SlimeAssets
    {
        public string name;
        public Color _mainColor;
        public int _particlesID;
    }
    public SlimeAssets[] _slimeAssets;

    // 🔹 Este será el material instanciado en runtime

    public Color[] _materialColors;
    public bool _borrar;

    public float fillAmount;
    public ParticleSystem _wrongParticle;
    public ParticleSystem _alarmParticle;
    public GameObject _WindBlocker;
    public GameObject _slimeMainBody;

    public int _fightChances;
    public float _slimeSpeed;


    public GameObject _slimeParent;
    void Start()
    {
        _slimeAnimator = GetComponent<Animator>();

        //// ✅ Crear una instancia segura del material
        //_mainMaterial = new Material(_mainMaterial);

        //// Asignar la instancia al renderer para este slime
        //var renderer = GetComponent<Renderer>();
        //if (renderer != null)
        //{
        //    renderer.material = _mainMaterial;
        //}

        //// Inicializar color base
        //_mainMaterial.SetColor("_BaseColor", _materialColors[0]);
    }

    void Update()
    {
        // Actualizar parámetros en la instancia

        fillAmount = Mathf.Lerp(fillAmount, _scriptMain._scriptRythm._elementsSelection.Count * 0.5f, 2 * Time.deltaTime);
        _slimeRawImage.transform.localScale = Vector2.Lerp(_slimeRawImage.transform.localScale, new Vector2(2.5f, 2.5f), 3 * Time.deltaTime);
        _slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_FillAmount", fillAmount);
        _slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetColor("_FillColorA", _materialColors[1]);
        _slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetColor("_FillColorB", _materialColors[2]);

        if (_slimeAnimator.GetBool("Moving"))
        {

            RectTransform rt = _scriptMain._slimeParent.GetComponent<RectTransform>();
            rt.anchoredPosition += Vector2.right * _slimeSpeed * Time.deltaTime;

        }
    }

    public void ChangeSlime()
    {
        // Desactivar partículas
        for (int i = 1; i < _allParticles.Length; i++)
        {
            _allParticles[i].gameObject.SetActive(false);
        }

        if (_allParticles[_slimeAssets[_slimeType]._particlesID] != null)
        {
            _allParticles[_slimeAssets[_slimeType]._particlesID].gameObject.SetActive(true);
        }

        // Cambiar animación
        _slimeAnimator.SetInteger("ID", _slimeType);
        _scriptMain._scriptMain._saveLoadValues._slimeUnlocked[_slimeType] = true;

        // Cambiar color en la instancia del material
        _slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor", _slimeAssets[_slimeType]._mainColor);
      
    }

    public void DeactivateElementsInfo()
    {
        for (int i = 0; i < 4; i++)
        {
            _scriptMain._scriptRythm._elementsInfo[i]._parent.SetActive(false);
            _scriptMain._scriptRythm._elementsInfo[i]._selector.transform.localScale = Vector2.zero;
            _scriptMain._scriptRythm._elementsInfo[i]._imageColor.color = _scriptMain._scriptRythm._halfColors[0];
        }
    }

    public IEnumerator ActionSlimeNumerator()
    {
       

        _scriptMain._eventOn = true;

        _slimeAnimator.SetBool("Scared", false);
        //yield return new WaitForSeconds(1f);

        //_scriptMain._lightChanging = false;
        //_scriptMain._shineParticle.Stop();

        Debug.Log(_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]);

        switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._weakto.Length)
        {
            case 1:
                if (_slimeType == _scriptMain._rightElementID[0])
                {
   

                    _scriptMain._slimeChanging = true;
                    _scriptMain._lightChanging = true;
                    _scriptMain._darkenerChanging = true;
                    _scriptMain._scriptRythm._elementChoosed = true;
                    _scriptMain._shineParticle.Play();
                    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._slimeCharge);

                    _scriptMain._scriptMain.newSlimePanel._backgroundImage.color = _scriptMain._scriptSlime._slimeAssets[_slimeType]._mainColor;

                    _scriptMain._scriptMain.newSlimePanel._slimeNameText.text = GameInitScript.Instance.GetText("Slime" + _slimeType.ToString("f0"));
                    //_scriptMain._scriptMain.newSlimePanel._slimeNameText.text = _scriptMain._scriptSlime._slimeAssets[_slimeType].name
                    _scriptMain._scriptMain.newSlimePanel._parent.SetBool("AnnounceIn", true);


                    
                    yield return new WaitForSeconds(1F);
                    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._slimeRelease);
                    ChangeSlime();
                    _scriptMain._snowBool = false;
                    _scriptMain._shineParticle.Stop();
                    yield return new WaitForSeconds(0.25f);
                    _scriptMain._scriptMain.newSlimePanel._parent.SetBool("AnnounceIn", false);                
                    yield return new WaitForSeconds(0.5F);
                    _scriptMain._slimeChanging = false;
                
                    yield return new WaitForSeconds(0.5F);
               
                    _scriptMain._darkenerChanging = false;
                    _scriptMain._scriptEvents._winRound = true;
                 
                    _scriptMain._lightChanging = false;
                    yield return new WaitForSeconds(1);

                    _scriptMain._successAssets._background.color = _scriptMain._successAssets._colors[0];
                    _scriptMain._successAssets._text.text = "SUCCESS";
                    _scriptMain._successAssets._parent.SetBool("Success", true);
                    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._successSound);
                    yield return new WaitForSeconds(1);
                    _scriptMain._successAssets._parent.SetBool("Success", false);
       

                    _scriptMain._scriptSlime._slimeAnimator.SetTrigger("Action");

                    switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventClassification)
                    {
                        case GameEvent.EventClassification.Normal:
                            case GameEvent.EventClassification.Fight:
                            switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventType)
                            {
                                case GameEvent.EventType.Bridge:
                                    _scriptMain._airPushParticle.Play();
                                    yield return new WaitForSeconds(0.5f);
                                    _scriptMain._windBlockPalanca.Play();
                                    yield return new WaitForSeconds(0.5f);
                                    _scriptMain._airPushParticle.Stop();
                                    _scriptMain._windBlockPalanca.Stop();
                                    _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BridgeEvent>()._activateBridge = true;
                                    yield return new WaitForSeconds(2);
                                    break;
                                case GameEvent.EventType.Lagoon:
                                    _scriptMain._snowParticle.Play();
                                    _scriptMain._scriptEvents._currentEventPrefab.GetComponent<WaterFallEvent>().ActivateFreeze();
                                    yield return new WaitForSeconds(4);
                                    break;
                                case GameEvent.EventType.Well:
                                    _scriptMain._scriptMain._scriptSFX._rainSetVolume = 1;
                                    _scriptMain._scriptEvents._rainParticle.Play();
                                    _scriptMain._scriptEvents._currentEventPrefab.GetComponent<WaterFillEvent>()._fillBool = true;
                                    yield return new WaitForSeconds(2);
                                    break;
                                case GameEvent.EventType.StrongAir:
                                    _scriptMain._windBlocker.GetComponent<ParticleSystem>().Play();
                                    //_scriptMain._scriptMain._windParticle.GetComponent<ForceField2D>().fuerza = 1250;
                                    yield return new WaitForSeconds(2);
                                    break;
                                case GameEvent.EventType.FallingBridge:
                                    _scriptMain._cutParticles[0].Play();
                                    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._cut);
                                    yield return new WaitForSeconds(0.5f);
                                    _scriptMain._cutParticles[1].Play();
                                    _scriptMain._scriptEvents._currentEventPrefab.GetComponent<SandCutEventScript>().StartCuttingVoid();
                                    yield return new WaitForSeconds(2);
                                    break;
                                case GameEvent.EventType.Gears:
                                    _scriptMain._scriptEvents._currentEventPrefab.GetComponent<GearsPrefabEventScript>()._stainsAnimator.SetTrigger("Splash");
                                    yield return new WaitForSeconds(0.5f);
                                    _scriptMain._scriptEvents._currentEventPrefab.GetComponent<GearsPrefabEventScript>()._Stopped = true;
                                    yield return new WaitForSeconds(2);
                                    break;
                                case GameEvent.EventType.Fire:
                                    _scriptMain._scriptMain._scriptSFX._rainSetVolume = 1;
                                    _scriptMain._scriptEvents._rainParticle.Play();
                                    yield return new WaitForSeconds(1);
                                    _scriptMain._scriptEvents._currentEventPrefab.GetComponent<FireEventScript>().FireExtinguishVoid();
                                    yield return new WaitForSeconds(2);
                                    break;
                                case GameEvent.EventType.BossFight0:
                                    break;
                                case GameEvent.EventType.BossFight1:
                                    Debug.Log("frozen");
                                    _scriptMain._cascadeFrozen = true;
                                    //_scriptMain._scriptEvents._currentEventPrefab.GetComponent<WaterFallEvent>().ActivateFreeze();
                                    _scriptMain._snowBool = true;

                                    _scriptMain._bossAnimator.Play("Frozen");
                                    _scriptMain._bossAnimator.SetBool("Frozen", true);
                                    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._frozen);
                                    _scriptMain._proyectileCharge[0].Stop();
                                    _scriptMain._proyectileCharge[1].Stop();
                                    _scriptMain._proyectileCharge[2].Stop();
                                    _scriptMain._scriptMain._scriptSFX._chargeAttackVolume = 0;
                                    _scriptMain._scriptMain._scriptSFX._chargeAttackPitch = 0.75f;
                                    _scriptMain._windBossParticles[0].Stop();
                                    _scriptMain._windBossParticles[1].Stop();
                                    _scriptMain._windBossParticles[2].Stop();
                                    _scriptMain._windBossParticles[3].Stop();
                                    yield return new WaitForSeconds(2);
                                    break;
                                case GameEvent.EventType.BossFight2:
                                    _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BossFightsScript>()._events[2].GetComponent<Animator>().SetBool("Cut", true);
                                    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._cut);
                                    yield return new WaitForSeconds(0.5f);
                                    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._boosDamaged);
                                    _scriptMain._scriptMain._scriptSFX._chargeAttackVolume = 0;
                                    _scriptMain._scriptMain._scriptSFX._chargeAttackPitch = 0.75f;
                                    _scriptMain._proyectileCharge[0].Stop();
                                    _scriptMain._proyectileCharge[1].Stop();
                                    _scriptMain._proyectileCharge[2].Stop();
                 
                                    //_scriptMain._bossAnimator.SetBool("Damaged", true);
                                    _scriptMain._enemyExplosion.Play();
                                    yield return new WaitForSeconds(2);
                                    StartCoroutine(_scriptMain.GameEndsNumerator());
                                    yield break;
                                    break;
                                case GameEvent.EventType.BossFight3:
                              
                                    break;
                                case GameEvent.EventType.BossFight4:
                                    _scriptMain._scriptMain._scriptSFX._rainSetVolume = 1;
                                    _scriptMain._scriptEvents._rainParticle.Play();
                                    yield return new WaitForSeconds(1);
                                    _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BossFightsScript>().FireExtinguish();
                                    //yield break;
                                    break;
                                case GameEvent.EventType.BossFight5:
                                    
                                    break;
                            }
                            break;
                        case GameEvent.EventClassification.Tutorial:
                            _scriptMain._scriptMain._scriptSFX._rainSetVolume = 1;
                            _scriptMain._scriptEvents._rainParticle.Play();
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<WaterFillEvent>()._fillBool = true;
                            yield return new WaitForSeconds(2);
                            break;
                    }

      
                    _scriptMain._darkenerChanging = false;            
      
                    StartCoroutine(_scriptMain.ExitNumerator());                
                }
                else
                {
           
                    _scriptMain._scriptEvents._winRound = false;
                    switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventClassification)
                    {
                        case GameEvent.EventClassification.Normal:
                        case GameEvent.EventClassification.Fight:
                        
                            if(_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventType != GameEvent.EventType.BossFight3 ||
                               _scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventType != GameEvent.EventType.StrongAir)
                            {
                                _slimeAnimator.SetTrigger("Wrong");
                            }
                            _wrongParticle.Play();

                            _scriptMain._successAssets._background.color = _scriptMain._successAssets._colors[1];
                            _scriptMain._successAssets._text.text = "WRONG";
                            _scriptMain._successAssets._parent.SetBool("Success", true);
                            _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._failSound);
                            yield return new WaitForSeconds(1);
                            _scriptMain._successAssets._parent.SetBool("Success", false);
                        

                            switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventType)
                            {
                                case GameEvent.EventType.BossFight0:
                                    break;
                                case GameEvent.EventType.BossFight1:
                                case GameEvent.EventType.BossFight2:
                                    _scriptMain._scriptMain._scriptSFX._chargeAttackPitch = 0.75f;
                                    _scriptMain._scriptMain._scriptSFX._chargeAttackVolume = 0;
                                    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._bossAttack);
                                    _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", true);
                                    yield return new WaitForSeconds(0.5f);
                                    _scriptMain._bossAnimator.Play("SideFinalAttack");
                                    StartCoroutine(_scriptMain.LoseLifeNumerator());
                                    break;
                                case GameEvent.EventType.BossFight3:
                                    _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", true);
                                    yield return new WaitForSeconds(0.5f);
                                 
                                    StartCoroutine(_scriptMain.LoseLifeNumerator());
                                    break;
                                case GameEvent.EventType.BossFight4:
                                    _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", true);
                                    yield return new WaitForSeconds(0.5f);
                                    for(int i = 0; i < _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BossFightsScript>()._fire.Length; i++)
                                    {
                                        _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BossFightsScript>()._fire[i].Play();
                                    }                 
                                     StartCoroutine(_scriptMain.LoseLifeNumerator());
                                    break;
                                default:
                                    yield return new WaitForSeconds(2);



                                    //_scriptMain._scriptFusion.ActivatePanel();
                                    //if (_scriptMain._scriptMain._saveLoadValues._healthCoins > 1)
                                    //{
                                    //    _scriptMain._scriptMain._saveLoadValues._healthCoins--;

                                    //}
                                    //else
                                    //{
                                    //    StartCoroutine(_scriptMain.LoseLifeNumerator());
                                    //}


                                    StartCoroutine(_scriptMain._scriptRythm.RythmNumerator());
                                    break;
                            }

                      


                



                            break;
           
                        case GameEvent.EventClassification.Questionary:
                            break;
                    }

                }
                break; 
        }

        _scriptMain._eventOn = false;

    }

    public void JumpingSlime()
    {
        _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._slimeJumping);
      
    }


}
