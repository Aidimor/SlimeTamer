using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearsPrefabEventScript : MonoBehaviour
{

    [SerializeField] private GameObject[] gears;
    public float _rotationSpped;
    [SerializeField] private GameObject[] _blockParent;
    public int _blockStation;
    public bool _Stopped;
    public float[] _yPos;
    public float timer;
    public GameObject[] _worlds;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (!_Stopped)
        //{

        //}
        GearsVoid();
        BlockVoid();
    }

    public void GearsVoid()
    {
        if (_Stopped)
        {

        }
        else
        {
            gears[0].transform.Rotate(Vector3.forward * _rotationSpped * Time.deltaTime);
            gears[1].transform.Rotate(Vector3.forward * -_rotationSpped * Time.deltaTime);
            gears[2].transform.Rotate(Vector3.forward * -_rotationSpped * Time.deltaTime / 2);
        }
  
    }

    public void BlockVoid()
    {
        if (_Stopped)
        {
            for(int i = 0; i < _blockParent.Length; i++)
            {
                _blockParent[i].GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_blockParent[i].GetComponent<RectTransform>().anchoredPosition, new Vector2(_blockParent[i].GetComponent<RectTransform>().anchoredPosition.x, _yPos[2]), 1f * Time.deltaTime);
            }
 
        }
        else
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                switch (_blockStation)
                {
                    case 0:
                        timer = 0.5f;
                        _blockStation = 1;
                        break;
                    case 1:
                        timer = 1f;
                        _blockStation = 0;
                        break;

                }
            }

            for (int i = 0; i < _blockParent.Length; i++)
            {
                _blockParent[i].GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_blockParent[i].GetComponent<RectTransform>().anchoredPosition, new Vector2(_blockParent[i].GetComponent<RectTransform>().anchoredPosition.x, _yPos[_blockStation]), 1f * Time.deltaTime);
            }

        }


    }
}
