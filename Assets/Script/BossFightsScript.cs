using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFightsScript : MonoBehaviour
{
    public GameObject _bossRender;
    public GameObject[] _worlds;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetButtonDown("Submit"))
    //    {
    //        FliesVoid();
    //    }
    //}

    public void StartBoss()
    {
        _bossRender.gameObject.SetActive(true);
    }

    public void FliesVoid()
    {
        GetComponent<Animator>().SetTrigger("Flies");
    }

    //public IEnumerator FliesNumerator()
    //{
    //    _bossAnimator.
    //     yield return new WaitForSeconds(1);
    //}
}
