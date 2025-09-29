using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeEvent : MonoBehaviour
{
    public GameObject _lever;
    public GameObject _bridge;
    public bool _activateBridge;
    public float[] _bridgeYpos;
    public float[] _leverYpos;
    public GameObject[] _worlds;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        BridgeVoidController();
        
    }

    public void BridgeVoidController()
    {
        if (_activateBridge)
        {
            _bridge.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_bridge.GetComponent<RectTransform>().anchoredPosition, 
                new Vector2(_bridgeYpos[1], _bridge.GetComponent<RectTransform>().anchoredPosition.y) , 2 * Time.deltaTime);

            _lever.GetComponent<RectTransform>().rotation = Quaternion.Lerp(_lever.GetComponent<RectTransform>().rotation,
         Quaternion.Euler(0, 0, _leverYpos[1]), 5 * Time.deltaTime);
        }
        else
        {
            _bridge.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_bridge.GetComponent<RectTransform>().anchoredPosition,
           new Vector2(_bridgeYpos[0], _bridge.GetComponent<RectTransform>().anchoredPosition.y), 2 * Time.deltaTime);

            _lever.GetComponent<RectTransform>().rotation = Quaternion.Lerp(_lever.GetComponent<RectTransform>().rotation,
       Quaternion.Euler(0, 0, _leverYpos[0]), 5 * Time.deltaTime);
        }
    }

    //public IEnumerator ActivationNumerator()
    //{

    //}
}
