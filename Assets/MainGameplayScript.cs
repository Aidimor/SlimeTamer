using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class MainGameplayScript : MonoBehaviour
{
    public SlimeController _scriptSlime;
    public FusionScript _scriptFusion;
    public GameEventsScript _scriptEvents;
    public int _onEventID;
    public int[] _rightElementID;
    public Animator _BordersAnimator;
    public bool _snowBool;
    public Image _snowImage;
    public Animator _wasp;


    [System.Serializable]
    public class TotalStages
    {
        public GameObject _SlimeIcon;
        public int _total;     
        public List<float> _xPoses = new List<float>();
    }
    public TotalStages _totalStages;

    public List<int> _GamesList = new List<int>();

    [System.Serializable]
    public class HearthAssets
    {
        public GameObject _parent;
        public Image[] _totalHearts;
        public Color[] _heartColors;
    }
    public HearthAssets _heartAssets;
    public int _totalLifes = 4;


    // Start is called before the first frame update
    void Start()
    {
        _totalStages._xPoses.Clear();

        int n = Mathf.Max(1, _totalStages._total); // avoid divide-by-zero

        for (int i = 0; i < n; i++)
        {
            float t = (n == 1) ? 0f : i / (n - 1f); // goes 0 → 1
            float x = Mathf.Lerp(0f, 150f, t);      // goes 0 → 150
            _totalStages._xPoses.Add(x);
         
        }
    }



    // Update is called once per frame
    void Update()
    {
        switch (_snowBool)
        {
            case true:
                _snowImage.color = Color.Lerp(_snowImage.color, new Color(1, 1, 1, 0.5f), 2 * Time.deltaTime);
                break;
            case false:
                _snowImage.color = Color.Lerp(_snowImage.color, new Color(1, 1, 1, 0f), 2 * Time.deltaTime);
                break;
        }

        _totalStages._SlimeIcon.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(_totalStages._SlimeIcon.GetComponent<RectTransform>().anchoredPosition,
            new Vector2(_totalStages._xPoses[_scriptEvents._onEvent], 4), 5 * Time.deltaTime);
    }

    public IEnumerator StartStageNumerator()
    {
        yield return new WaitForSeconds(1);
        _BordersAnimator.SetBool("BorderOut", true);
        yield return new WaitForSeconds(1);
        _scriptFusion.ActivatePanel();
    }

    public IEnumerator StartStageQuestionary()
    {
        yield return new WaitForSeconds(1);
        _BordersAnimator.SetBool("BorderOut", true);
        yield return new WaitForSeconds(2);
        Debug.Log("Questionario");
        _BordersAnimator.SetBool("BorderOut", false);

        yield return new WaitForSeconds(2);
        _scriptSlime._WindBlocker.gameObject.SetActive(false);
        Destroy(_scriptEvents._currentEventPrefab);
        _scriptEvents._onEvent++;
        _scriptEvents.StartLevel();

        //_scriptFusion.ActivatePanel();
    }

    public void LoseHeartVoid()
    {
        _totalLifes--;
        for(int i = 0; i < _heartAssets._totalHearts.Length; i++)
        {
            _heartAssets._totalHearts[i].color = _heartAssets._heartColors[1];
        }

        for (int i = 0; i < _totalLifes; i++)
        {
            _heartAssets._totalHearts[i].color = _heartAssets._heartColors[0];
        }
    }

}
