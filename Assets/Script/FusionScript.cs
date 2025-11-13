using System.Collections.Generic;
using UnityEngine;


public class FusionScript : MonoBehaviour
{
    [SerializeField] private SlimeController _scriptSlime;
    
    public GameObject _slimeRenderer;
    public Vector2[] _slimeStartPos;
    //public bool _panelActive;
    //public GameObject _fusionParent;
  
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


    public void ChangeElement()
    {
        for(int i = 0; i < 3; i++)
        {
            _elementsOptions[i]._parent.transform.localScale = new Vector3(0.6f, 0.6f, 1);
        }
        _elementsOptions[_onElement]._parent.transform.localScale = new Vector3(0.85f,0.85f, 1);
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
}
