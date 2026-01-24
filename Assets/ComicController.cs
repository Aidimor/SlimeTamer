using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        _comicAnimator.SetBool("ComicOn", true);
        MainController.Instance._cinematicBorders.SetBool("FadeIn", false);
        yield return new WaitForSeconds(1);
        MainController.Instance._bordersAnimator.SetBool("BorderOut", true);
        for (int i = 0; i < _imagesID.Count; i++)
        {
            _comicImage.sprite = _allComics[_imagesID[i]];
            _comicAnimator.SetTrigger("NextImage");
            yield return new WaitForSeconds(_waitSeconds);
        }
        _continueParent.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        while (!Input.GetButtonDown("Submit"))
        {
            yield return null;
        }
        _imagesID.Clear();
        _comicOn = false;
    }
}
