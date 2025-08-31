using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterFallEvent : MonoBehaviour
{
    public MainGameplayScript _scriptMain;
   
    public GameObject[] _WaterIceEventObjects;
    public bool _freezeBool;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void ActivateFreeze()
    {
        StartCoroutine(FreezeNumerator());
    }

    public IEnumerator FreezeNumerator()
    {
        _scriptMain._snowBool = true;
        yield return new WaitForSeconds(1);
        _WaterIceEventObjects[0].SetActive(false);
        _WaterIceEventObjects[1].SetActive(true);
     
    }
}
