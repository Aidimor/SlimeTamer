using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaspFightScript : MonoBehaviour
{
    public Animator _waspAnimator;
    
    public GameObject _waspImageParent;
    public float _fallSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
            _waspImageParent.transform.Translate(Vector2.down * _fallSpeed * Time.deltaTime);
    }

    public IEnumerator DeadNumerator()
    {
        _fallSpeed = 1;
        _waspAnimator.SetBool("Dead", true);
        yield return new WaitForSeconds(1);
        _fallSpeed = 10;
 
    }
}
