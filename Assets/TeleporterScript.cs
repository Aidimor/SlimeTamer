using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterScript : MonoBehaviour
{
    public MainGameplayScript _scriptMain;
    public GameObject[] _worlds;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TeleportVoid()
    {
        StartCoroutine(TeleportNumerator());
    }

    public IEnumerator TeleportNumerator()
    {
        _scriptMain._scriptMain._cinematicBorders.SetBool("FadeIn", true);
        yield return new WaitForSeconds(2f);
        _scriptMain._teleportParticle.Play();
        _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._jump);

        yield return new WaitForSeconds(2);
        _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", false);
        _scriptMain._scriptMain._introSpecial = true;
        yield return new WaitForSeconds(1);
        _scriptMain._bossAnimator.transform.gameObject.SetActive(false);

        switch (_scriptMain._scriptMain._onWorldGlobal)
        {
            case 0:
                break;
            case 1:
                if (!_scriptMain._scriptMain._saveLoadValues._worldsUnlocked[0])
                {
                    MainController.Instance._saveLoadValues._progress++;
                }
                _scriptMain._scriptMain._saveLoadValues._finalWorldUnlocked = true;
                _scriptMain._scriptMain._saveLoadValues._worldsUnlocked[0] = true;
                _scriptMain._scriptMain._onWorldGlobal = 0;
                break;

            case 2:
                if (!_scriptMain._scriptMain._saveLoadValues._worldsUnlocked[1])
                {
                    MainController.Instance._saveLoadValues._progress++;
                }
                _scriptMain._scriptMain._saveLoadValues._worldsUnlocked[1] = true;
                _scriptMain._scriptMain._onWorldGlobal = 1;
                break;
            case 3:
                if (!_scriptMain._scriptMain._saveLoadValues._worldsUnlocked[2])
                {
                    MainController.Instance._saveLoadValues._progress++;
                }
                _scriptMain._scriptMain._saveLoadValues._worldsUnlocked[2] = true;
                _scriptMain._scriptMain._onWorldGlobal = 2;
         
                break;
        }
        _scriptMain._scriptMain._scriptInit.SaveGame();
        _scriptMain._bossAnimator.gameObject.SetActive(false);
        _scriptMain._scriptMain.LoadSceneByName("IntroScene");
    }
}
