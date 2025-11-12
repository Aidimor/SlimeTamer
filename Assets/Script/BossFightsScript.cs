using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightsScript : MonoBehaviour
{
    public MainGameplayScript _scriptMain;
    public GameObject[] _worlds;
    public GameObject[] _events;
    public ParticleSystem[] _fire;


    // Start is called before the first frame update
    void Start()
    {
        _scriptMain = GameObject.Find("MainGameplayScript").GetComponent<MainGameplayScript>();
        _worlds[_scriptMain._scriptMain._onWorldGlobal].gameObject.SetActive(true);
    }




    public void FliesVoid()
    {
        GetComponent<Animator>().SetTrigger("Flies");
    }

    public void StartBossVoid()
    {
        StartCoroutine(StartBossNumerator());
    }

    public IEnumerator StartBossNumerator()
    {

        yield return new WaitForSeconds(1);
        //_scriptMain._scriptMain._scriptMusic.PlayMusic(5);
      
        _scriptMain._bossAnimator.transform.gameObject.SetActive(false);

        yield return new WaitForSeconds(1);
        switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventType)
        {
            case GameEvent.EventType.BossFight0:
                _scriptMain._bossAnimator.transform.localEulerAngles = _scriptMain._bossPoses[0]._bossStartRot;
                _scriptMain._bossAnimator.transform.localPosition = _scriptMain._bossPoses[0]._bossStartPos;
                break;
            case GameEvent.EventType.BossFight1:
                _scriptMain._cascadeFrozen = false;
                _scriptMain._bossAnimator.transform.localEulerAngles = _scriptMain._bossPoses[0]._bossStartRot;
                _scriptMain._bossAnimator.transform.localPosition = _scriptMain._bossPoses[0]._bossStartPos;
                break;
            case GameEvent.EventType.BossFight2:
                _scriptMain._bossAnimator.transform.localEulerAngles = _scriptMain._bossPoses[0]._bossStartRot;
                _scriptMain._bossAnimator.transform.localPosition = _scriptMain._bossPoses[0]._bossStartPos;
                break;
            case GameEvent.EventType.BossFight3:
                _scriptMain._bossAnimator.transform.localEulerAngles = _scriptMain._bossPoses[1]._bossStartRot;
                _scriptMain._bossAnimator.transform.localPosition = _scriptMain._bossPoses[1]._bossStartPos;
                break;
            case GameEvent.EventType.BossFight4:
                _scriptMain._bossAnimator.transform.localEulerAngles = _scriptMain._bossPoses[0]._bossStartRot;
                _scriptMain._bossAnimator.transform.localPosition = _scriptMain._bossPoses[0]._bossStartPos;
                break;
            case GameEvent.EventType.BossFight5:
                _scriptMain._bossAnimator.transform.localEulerAngles = _scriptMain._bossPoses[0]._bossStartRot;
                _scriptMain._bossAnimator.transform.localPosition = _scriptMain._bossPoses[0]._bossStartPos;
                break;
        }
        _scriptMain._bossAnimator.transform.gameObject.SetActive(true);
        _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._roar);
        _scriptMain._bossAnimator.SetBool("Idle", true);
        _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", true);
        _scriptMain._scriptSlime._alarmParticle.Play();
        yield return new WaitForSeconds(2);
        Debug.Log(_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventType);
        switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._eventType)
        {
            case GameEvent.EventType.BossFight0:
                StartCoroutine(ExitNumerator());
                break;
            case GameEvent.EventType.BossFight1:
             
                _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", false);
                yield return new WaitForSeconds(1);
                
                StartCoroutine(_scriptMain._scriptRythm.RythmNumerator());
                break;  

            case GameEvent.EventType.BossFight2:
                _events[2].gameObject.SetActive(true);  
                _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", false);
                //_events[1].gameObject.SetActive(true);
                //_scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", false);
                //_scriptMain._bossAnimator.SetTrigger("AttackFire");
                yield return new WaitForSeconds(1);
                StartCoroutine(_scriptMain._scriptRythm.RythmNumerator());
                break;
            case GameEvent.EventType.BossFight3:
      
                _scriptMain._bossAnimator.SetBool("Ventilator", true);
                _scriptMain._scriptMain._scriptSFX._strongWindSetVolume = 0.15f;
                _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", false);
                //_events[1].gameObject.SetActive(true);
                //_scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", false);
                //_scriptMain._bossAnimator.SetTrigger("AttackFire");
                yield return new WaitForSeconds(0.5f);
                _scriptMain._frontWindParticle.Play();
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(_scriptMain._scriptRythm.RythmNumerator());
                break;
            case GameEvent.EventType.BossFight4:
                _events[0].gameObject.SetActive(true);
                _scriptMain._scriptSlime._slimeAnimator.SetBool("Scared", false);
                _scriptMain._bossAnimator.SetTrigger("AttackFire");
   
                yield return new WaitForSeconds(1.5f);
                _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._flameOn);
                _scriptMain._scriptMain._scriptSFX._fireSetVolume = 1;
                _fire[0].Play();
                yield return new WaitForSeconds(1);
                StartCoroutine(_scriptMain._scriptRythm.RythmNumerator());
                break;
            case GameEvent.EventType.BossFight5:
                //StartCoroutine(ExitNumerator());
                break;
        }

  
     


    }

    public IEnumerator ExitNumerator()
    {
        MainController.Instance._introSpecial = true;
        _scriptMain._scriptMain._cinematicBorders.SetBool("FadeIn", true);
        yield return new WaitForSeconds(2f);

        _scriptMain._scriptMain._saveLoadValues._progressSave[6] = true;


        _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._whip);
        _scriptMain._bossAnimator.SetTrigger("Attack");
  
        yield return new WaitForSeconds(0.5f);
        _scriptMain._slimeExplosion.Play();
        _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._scream);
        _scriptMain._flyingSlimeParticles[0].Play();
        yield return new WaitForSeconds(1.5f);


        _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._ding);
        _scriptMain._flyingSlimeParticles[1].Play();
        yield return new WaitForSeconds(2);
        _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", false);
        _scriptMain._scriptMain._introSpecial = true;
        yield return new WaitForSeconds(1);
        _scriptMain._bossAnimator.transform.gameObject.SetActive(false);
        _scriptMain._scriptMain._saveLoadValues._worldsUnlocked[0] = false;
        _scriptMain._scriptMain._saveLoadValues._worldsUnlocked[3] = true;
        _scriptMain._scriptMain._onWorldGlobal = 3;
        _scriptMain._bossAnimator.gameObject.SetActive(false);
        _scriptMain._scriptMain.LoadSceneByName("IntroScene");

    }

    public void FireExtinguish()
    {
        for (int i = 0; i < _fire.Length; i++)
        {
            _fire[i].Stop();
        }
    }


}
