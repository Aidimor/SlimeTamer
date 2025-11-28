using System.Collections;
using UnityEngine;


public class WaterFallEvent : MonoBehaviour
{
    public MainGameplayScript _scriptMain;
   
    public GameObject[] _WaterIceEventObjects;
    public bool _freezeBool;
    public GameObject[] _worlds;
    // Start is called before the first frame update

    // Update is called once per frame
    public void ActivateFreeze()
    {
        StartCoroutine(FreezeNumerator());
    }

    public IEnumerator FreezeNumerator()
    {
        _scriptMain._snowBool = true;
        yield return new WaitForSeconds(1);
        _scriptMain._scriptEvents._cascadeParticle.gameObject.SetActive(false);
        _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._frozen);
        _WaterIceEventObjects[0].SetActive(false);
        _WaterIceEventObjects[1].SetActive(true);
     
    }
}
