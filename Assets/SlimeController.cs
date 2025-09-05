using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [SerializeField] private MainGameplayScript _scriptMain;
    public int _slimeType; //0 = Null, 1 = Water, 2 = Air, 3 = Earth
    public Animator _slimeAnimator;
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
    public GameObject _WindBlocker;
    public GameObject _slimeMainBody;

    public int _fightChances;

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
        _slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetFloat("_FillAmount", fillAmount);
        _slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetColor("_FillColorA", _materialColors[1]);
        _slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetColor("_FillColorB", _materialColors[2]);
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

        // Cambiar color en la instancia del material
        _slimeMainBody.GetComponent<SkinnedMeshRenderer>().material.SetColor("_BaseColor", _slimeAssets[_slimeType]._mainColor);
    }

    public IEnumerator ActionSlimeNumerator()
    {
        yield return new WaitForSeconds(0.5f);

        switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._weakto.Length)
        {
            case 1:
                if (_slimeType == _scriptMain._rightElementID[0])
                {
                    Debug.Log("Correcto");
                    yield return new WaitForSeconds(1);
                    switch (_scriptMain._onEventID)
                    {
                        case 1:
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BridgeEvent>()._activateBridge = true;
                            yield return new WaitForSeconds(2);
                            break;
                        case 2:
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<WaterFallEvent>().ActivateFreeze();
                            yield return new WaitForSeconds(5);
                            break;
                        case 3:
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<WaterFillEvent>()._fillBool = true;
                            yield return new WaitForSeconds(5);
                            break;
                        case 4:
                            _scriptMain._scriptSlime._WindBlocker.gameObject.SetActive(true);
                            yield return new WaitForSeconds(5);
                            break;
                        case 5:
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<SandCutEventScript>().StartCuttingVoid();
                            yield return new WaitForSeconds(5);
                            break;
                        case 6:
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<GearsPrefabEventScript>()._Stopped = true;
                            yield return new WaitForSeconds(5);
                            break;
                        case 7:
                            StartCoroutine(_scriptMain._scriptEvents._currentEventPrefab.GetComponent<WaspFightScript>().DeadNumerator());
                            Debug.Log("Wasp Destroyed");
                            yield return new WaitForSeconds(3);
                            break;
                    }
                    _scriptMain._BordersAnimator.SetBool("BorderOut", false);

                    yield return new WaitForSeconds(2);
                    _scriptMain._scriptSlime._WindBlocker.gameObject.SetActive(false);
                    Destroy(_scriptMain._scriptEvents._currentEventPrefab);
                    _scriptMain._scriptEvents._onEvent++;
                    _scriptMain._scriptEvents.StartLevel();
                }
                else
                {
                    switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventClassification)
                    {
                        case GameEvent.EventClassification.Normal:
                            _wrongParticle.Play();
                            _slimeAnimator.SetTrigger("Wrong");
                            yield return new WaitForSeconds(2);
                            _scriptMain._scriptFusion.ActivatePanel();
                            _scriptMain.LoseHeartVoid();
                            break;
                        case GameEvent.EventClassification.Fight:
                            _wrongParticle.Play();
                            _slimeAnimator.SetTrigger("Wrong");
                            yield return new WaitForSeconds(2);
                            _scriptMain._scriptFusion.ActivatePanel();
                            _scriptMain.LoseHeartVoid();
                            break;
                        case GameEvent.EventClassification.Questionary:
                            break;
                    }

                }
                break;
            case 2:
                bool _solved = false;
                for(int i = 0; i < 2; i++)
                {
                    if (_slimeType == _scriptMain._rightElementID[i]){
                        _solved = true;
                    }
                }
                if (_solved)
                {
                    Debug.Log("Correcto");
                    yield return new WaitForSeconds(1);
                    switch (_scriptMain._onEventID)
                    {
                        case 1:
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<BridgeEvent>()._activateBridge = true;
                            yield return new WaitForSeconds(2);
                            break;
                        case 2:
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<WaterFallEvent>().ActivateFreeze();
                            yield return new WaitForSeconds(5);
                            break;
                        case 3:
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<WaterFillEvent>()._fillBool = true;
                            yield return new WaitForSeconds(5);
                            break;
                        case 4:
                            _scriptMain._scriptSlime._WindBlocker.gameObject.SetActive(true);
                            yield return new WaitForSeconds(5);
                            break;
                        case 5:
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<SandCutEventScript>().StartCuttingVoid();
                            yield return new WaitForSeconds(5);
                            break;
                        case 6:
                            _scriptMain._scriptEvents._currentEventPrefab.GetComponent<GearsPrefabEventScript>()._Stopped = true;
                            yield return new WaitForSeconds(5);
                            break;
                        case 7:
                            StartCoroutine(_scriptMain._scriptEvents._currentEventPrefab.GetComponent<WaspFightScript>().DeadNumerator());
                            Debug.Log("Wasp Destroyed");
                            yield return new WaitForSeconds(3);
                            break;
                    }
                    _scriptMain._BordersAnimator.SetBool("BorderOut", false);

                    yield return new WaitForSeconds(2);
                    _scriptMain._scriptSlime._WindBlocker.gameObject.SetActive(false);
                    Destroy(_scriptMain._scriptEvents._currentEventPrefab);
                    _scriptMain._scriptEvents._onEvent++;
                    _scriptMain._scriptEvents.StartLevel();
                }
                else
                {
                    switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventClassification)
                    {
                        case GameEvent.EventClassification.Normal:
                            _wrongParticle.Play();
                            _slimeAnimator.SetTrigger("Wrong");
                            yield return new WaitForSeconds(2);
                            _scriptMain._scriptFusion.ActivatePanel();
                            break;
                        case GameEvent.EventClassification.Fight:
                            _wrongParticle.Play();
                            _slimeAnimator.SetTrigger("Wrong");
                            yield return new WaitForSeconds(2);
                            _scriptMain._scriptFusion.ActivatePanel();
                            break;
                        case GameEvent.EventClassification.Questionary:
                            break;
                    }

                }
                break;
        }
     

        _scriptMain._snowBool = false;
        _slimeType = 0;
        ChangeSlime();
    }
}
