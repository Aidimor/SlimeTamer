using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
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
    public bool _borrar;
    // Start is called before the first frame update
    void Start()
    {
        _slimeAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _slimeType = 1;
            ChangeSlime();
            StartCoroutine(ActionSlimeNumerator());
         }


        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _slimeType = 2;
            ChangeSlime();
            StartCoroutine(ActionSlimeNumerator());
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _slimeType = 3;
            ChangeSlime();
            StartCoroutine(ActionSlimeNumerator());
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _slimeType = 4;
            ChangeSlime();
            StartCoroutine(ActionSlimeNumerator());
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _slimeType = 5;
            ChangeSlime();
            StartCoroutine(ActionSlimeNumerator());
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            _slimeType = 6;
            ChangeSlime();
            StartCoroutine(ActionSlimeNumerator());
        }

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
        _mainMaterial.color = _slimeAssets[_slimeType]._mainColor;
        for(int i = 0; i < _allParticles.Length; i++)
        {
            _allParticles[i].gameObject.SetActive(false);
        }
        _allParticles[_slimeAssets[_slimeType]._particlesID].gameObject.SetActive(true);
        _slimeAnimator.SetInteger("ID", _slimeType);
    
    }

    public IEnumerator ActionSlimeNumerator()
    {
        yield return new WaitForSeconds(3);
        _slimeType = 0;
        //ChangeSlime();
    }
}
