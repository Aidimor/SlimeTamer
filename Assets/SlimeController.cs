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

    public Material _mainMaterial;
    public Color[] _materialColors;
    public bool _borrar;



    public float fillAmount;
    public ParticleSystem _wrongParticle;


    // Start is called before the first frame update
    void Start()
    {
        _slimeAnimator = GetComponent<Animator>();
        _mainMaterial.SetColor("_BaseColor", _materialColors[0]);
        // Get the material instance

    }



  





// Update is called once per frame
void Update()
    {
        // Update the fill amount in real-time
        _mainMaterial.SetFloat("_FillAmount", fillAmount);
    
        _mainMaterial.SetColor("_FillColorA", _materialColors[1]);
        _mainMaterial.SetColor("_FillColorB", _materialColors[2]);


        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    _slimeType = 1;
        //    ChangeSlime();
        //    StartCoroutine(ActionSlimeNumerator());
        // }


        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    _slimeType = 2;
        //    ChangeSlime();
        //    StartCoroutine(ActionSlimeNumerator());
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    _slimeType = 3;
        //    ChangeSlime();
        //    StartCoroutine(ActionSlimeNumerator());
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    _slimeType = 4;
        //    ChangeSlime();
        //    StartCoroutine(ActionSlimeNumerator());
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    _slimeType = 5;
        //    ChangeSlime();
        //    StartCoroutine(ActionSlimeNumerator());
        //}

        //if (Input.GetKeyDown(KeyCode.Alpha6))
        //{
        //    _slimeType = 6;
        //    ChangeSlime();
        //    StartCoroutine(ActionSlimeNumerator());
        //}

        //if(Input.GetAxisRaw("Vertical") > 0 && !_borrar)
        // {
        //     _slimeType++;
        //     ChangeSlime();
        //     _borrar = true;
        // }

        // if (Input.GetAxisRaw("Vertical") < 0 && !_borrar)
        // {
        //     _slimeType--;
        //     ChangeSlime();
        //     _borrar = true;
        // }

        // if (Input.GetAxisRaw("Vertical") == 0)
        // {       
        //     _borrar = false;
        // }

    }

    public void ChangeSlime()
    {
        Debug.Log("repite");
        //_mainMaterial.color = _slimeAssets[_slimeType]._mainColor;
        for (int i = 1; i < _allParticles.Length; i++)
        {
         
                _allParticles[i].gameObject.SetActive(false);
        }
        if(_allParticles[_slimeAssets[_slimeType]._particlesID] != null)
        {
            _allParticles[_slimeAssets[_slimeType]._particlesID].gameObject.SetActive(true);
        }

        _slimeAnimator.SetInteger("ID", _slimeType);
        _mainMaterial.SetColor("_BaseColor", _slimeAssets[_slimeType]._mainColor);
        //_materialColors[0] = _slimeAssets[_slimeType]._mainColor;
    

    }

    public IEnumerator ActionSlimeNumerator()
    {
        yield return new WaitForSeconds(0.5f);

        if (_slimeType == _scriptMain._onEventID)
        {
            Debug.Log("Correcto");
        }
        else
        {
            _wrongParticle.Play();
            _slimeAnimator.SetTrigger("Wrong");
        }
        yield return new WaitForSeconds(2);
        _slimeType = 0;
        ChangeSlime();
    }
}
