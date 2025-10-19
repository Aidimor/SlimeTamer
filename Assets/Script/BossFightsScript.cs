using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightsScript : MonoBehaviour
{
    public MainGameplayScript _scriptMain;
    public GameObject[] _worlds;
 


    // Start is called before the first frame update
    void Start()
    {
        _scriptMain = GameObject.Find("MainGameplayScript").GetComponent<MainGameplayScript>();
    }




    public void FliesVoid()
    {
        GetComponent<Animator>().SetTrigger("Flies");
    }

    //public IEnumerator StartBossNumerator()
    //{

    //    yield return new WaitForSeconds(1);
    //    _scriptMain._bossAnimator.transform.gameObject.SetActive(false);

    //    yield return new WaitForSeconds(3);
    //    _scriptMain._bossAnimator.transform.gameObject.SetActive(true);
    //    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._roar);
    //    _scriptMain._bossAnimator.SetBool("Idle", true);
    //    _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", true);
    //    _scriptMain._scriptSlime._alarmParticle.Play();
    //    yield return new WaitForSeconds(2);
    //    _scriptMain._scriptMain._cinematicBorders.SetBool("FadeIn", true);
    //    yield return new WaitForSeconds(2f);
    //    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._whip);
   

    //    _scriptMain._bossAnimator.SetTrigger("Attack");
    //    _scriptMain._flyingSlimeParticles[0].Play();
    //    yield return new WaitForSeconds(0.5f);
    //    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._scream);
    //    yield return new WaitForSeconds(1.5f);

    //    _scriptMain._flyingSlimeParticles[1].Play();
    //    _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._ding);
    //    yield return new WaitForSeconds(2);
    //    _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", false);
    //    _scriptMain._scriptMain._introSpecial = true;
    //    yield return new WaitForSeconds(1);
    //    _scriptMain._bossAnimator.transform.gameObject.SetActive(false);
    //    _scriptMain._scriptMain._onWorldGlobal = 3;
    //    _scriptMain._bossAnimator.gameObject.SetActive(false);

    //    _scriptMain._scriptMain.LoadSceneByName("IntroScene");
  

    //}


}
