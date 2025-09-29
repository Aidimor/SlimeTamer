using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class MainController : MonoBehaviour
{
    public Animator _bordersAnimator;
    public Animator _cinematicBorders;
    public AudioSource _bgmAS;
    public class SaveLoadValues {
        public int _wordsUnlocked;
    }
    public SaveLoadValues _saveLoadValues;

    public int _onWorldGlobal;
    // Start is called before the first frame update

        public void LoadSceneByName(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    

}
