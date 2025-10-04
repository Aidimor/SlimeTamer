using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Required for scene management
using UnityEngine.UI;
using TMPro;
using LoL;  // <- necesario para GameInitScript


public class MainController : MonoBehaviour
{
    public PortraitController _scriptPortrait;   
    public GameInitScript _scriptInit;
    public Animator _bordersAnimator;
    public Animator _cinematicBorders;
    public AudioSource _bgmAS;
    [System.Serializable]
    public class SaveLoadValues {
        public int _wordsUnlocked;
        public bool[] _elementsUnlocked;
    }
    public SaveLoadValues _saveLoadValues;



    public int _onWorldGlobal;
    public bool _introSpecial;
    // Start is called before the first frame update

        public void LoadSceneByName(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }





}
