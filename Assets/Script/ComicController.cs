using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoL;
using LoLSDK;

public class ComicController : MonoBehaviour
{
    
    public static ComicController Instance;

    public bool _comicOn;
    public Animator _comicAnimator;
    public Image _comicImage;
    public Sprite[] _allComics;
    public GameObject _continueParent;
    public TextMeshProUGUI _continuar;
    public int _onImage;
    public List<int> _imagesID = new List<int>();
    public float _waitSeconds;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator ComicStripOn()
    {
  
        if (MainController.Instance != null)
        {
            switch (MainController.Instance._onWorldGlobal)
            {
                case 0:
                    MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[1];
                    break;
                case 1:
                    MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[3];
                    break;
                case 2:
                    MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[4];
                    break;
                case 3:
                    MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[5];
                    break;
                case 4:
                    MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[6];
                    break;
            }

            MainController.Instance._scriptMusic._audioBGM.Play();
        }

        var GJ = GameInitScript.Instance;
        string key = "continue";
        string text = GameInitScript.Instance.GetText(key);
        _continuar.text = GJ.GetText("continue");
 
        _comicAnimator.SetBool("ComicOn", true);
        MainController.Instance._cinematicBorders.SetBool("FadeIn", false);
        yield return new WaitForSeconds(1);
        MainController.Instance._bordersAnimator.SetBool("BorderOut", true);
        for (int i = 0; i < _imagesID.Count; i++)
        {
            //if (MainController.Instance._onWorldGlobal == 0 && _imagesID[i] == 4)
            //{        
            //    MainController.Instance._scriptMusic._audioBGM.Stop();
            //}

            if (MainController.Instance._onWorldGlobal == 0 && _imagesID[i] == 4)
            {
                MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[2];
                MainController.Instance._scriptMusic._audioBGM.Play();
                SFXscript.Instance._fireSetVolume = 0.2f;
                SFXscript.Instance._strongWindSetVolume = 0.2f;
            }

            if (MainController.Instance._onWorldGlobal == 1 && _imagesID[i] == 13)
            {
                MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[2];
                MainController.Instance._scriptMusic._audioBGM.Play();
                SFXscript.Instance._fireSetVolume = 0.2f;
                SFXscript.Instance._strongWindSetVolume = 0.2f;
            }

            if (MainController.Instance._onWorldGlobal == 3 && _imagesID[i] == 18)
            {
                MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[6];
                MainController.Instance._scriptMusic._audioBGM.Play();
                SFXscript.Instance._fireSetVolume = 0.2f;
                SFXscript.Instance._strongWindSetVolume = 0.2f;
            }

            if (MainController.Instance._onWorldGlobal == 4 && _imagesID[i] == 24)
            {
                MainController.Instance._scriptMusic._audioBGM.clip = MainController.Instance._scriptMusic._allThemes[7];
                MainController.Instance._scriptMusic._audioBGM.Play();
                SFXscript.Instance._fireSetVolume = 0.2f;
                SFXscript.Instance._strongWindSetVolume = 0.2f;
            }
            _comicImage.sprite = _allComics[_imagesID[i]];
            _comicAnimator.SetTrigger("NextImage");
            MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._slimeJumping);
            yield return new WaitForSeconds(_waitSeconds);
        }     
        LOLSDK.Instance.SpeakText(key);
        yield return new WaitForSeconds(0.5f);
        _continueParent.gameObject.SetActive(true);
        while (!Input.GetButtonDown("Submit"))
        {
            yield return null;
        }
        SFXscript.Instance.PlaySound(SFXscript.Instance._whip);
        _continueParent.gameObject.SetActive(false);
        _imagesID.Clear();

        _comicOn = false;
    }
}
