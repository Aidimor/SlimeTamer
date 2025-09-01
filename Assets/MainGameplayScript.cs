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
    public int _rightElementID;
    public Animator _BordersAnimator;
    public bool _snowBool;
    public Image _snowImage;

    // Start is called before the first frame update
    void Start()
    {
     
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
    }

    public IEnumerator StartStageNumerator()
    {
        yield return new WaitForSeconds(1);
        _BordersAnimator.SetBool("BorderOut", true);
        yield return new WaitForSeconds(1);
        _scriptFusion.ActivatePanel();
    }

}
