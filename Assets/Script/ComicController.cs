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
        var GJ = GameInitScript.Instance;
        string key = "continue";
        string text = GameInitScript.Instance.GetText(key);
        var Main = MainController.Instance._scriptMusic;
        if (MainController.Instance != null)
        {
            switch (MainController.Instance._onWorldGlobal)
            {
                case 0:
                    _continuar.text = GJ.GetText("continue");
                    Main.PlayMusic(1);
                    break;
                case 1:
                    _continuar.text = GJ.GetText("continue");
                    Main.PlayMusic(3);
                    break;
                case 2:
                    _continuar.text = GJ.GetText("continue");
                    Main.PlayMusic(4);
                    break;
                case 3:
                    _continuar.text = GJ.GetText("continue");
                    Main.PlayMusic(5);
                    break;
                case 4:
                    switch (MainController.Instance._gameFinished)
                    {
                        case true:
                            _continuar.text = GJ.GetText("endgame");
                            break;
                        case false:
                            _continuar.text = GJ.GetText("continue");
                            break;
                    }
      
                    Main.PlayMusic(6);
                    break;
            }

 
        }


        //_continuar.text = GJ.GetText("continue");
 
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
                Main.PlayMusic(2);
                SFXscript.Instance._fireSetVolume = 0.2f;
                SFXscript.Instance._strongWindSetVolume = 0.2f;
            }

            if (MainController.Instance._onWorldGlobal == 1 && _imagesID[i] == 13)
            {
                Main.PlayMusic(2);
                SFXscript.Instance._fireSetVolume = 0.2f;
                SFXscript.Instance._strongWindSetVolume = 0.2f;
            }

            if (MainController.Instance._onWorldGlobal == 3 && _imagesID[i] == 18)
            {
                Main.PlayMusic(6);
                SFXscript.Instance._fireSetVolume = 0.2f;
                SFXscript.Instance._strongWindSetVolume = 0.2f;
            }

            if (MainController.Instance._onWorldGlobal == 4 && _imagesID[i] == 24)
            {
                Main.PlayMusic(7);
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
