using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameplayScript : MonoBehaviour
{
    [SerializeField] private SlimeController _scriptSlime;
    public FusionScript _scriptFusion;
    public GameEventsScript _scriptEvents;
    public int _onEventID;
    public int _rightElementID;
    public Animator _BordersAnimator;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartStageNumerator());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator StartStageNumerator()
    {
        yield return new WaitForSeconds(1);
        _BordersAnimator.SetBool("BorderOut", true);
        yield return new WaitForSeconds(1);
        _scriptFusion.ActivatePanel();
    }

}
