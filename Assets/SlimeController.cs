using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    public int _slimeType; //0 = Null, 1 = Water, 2 = Air, 3 = Earth
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
        
    }

    // Update is called once per frame
    void Update()
    {
     
       if(Input.GetAxisRaw("Vertical") > 0 && !_borrar)
        {
            _slimeType++;
            ChangeSlime();
            _borrar = true;
        }

        if (Input.GetAxisRaw("Vertical") < 0 && !_borrar)
        {
            _slimeType--;
            ChangeSlime();
            _borrar = true;
        }

        if (Input.GetAxisRaw("Vertical") == 0)
        {       
            _borrar = false;
        }

    }

    public void ChangeSlime()
    {
        _mainMaterial.color = _slimeAssets[_slimeType]._mainColor;
        for(int i = 0; i < _allParticles.Length; i++)
        {
            _allParticles[i].gameObject.SetActive(false);
        }
        _allParticles[_slimeAssets[_slimeType]._particlesID].gameObject.SetActive(true);
    
    }
}
