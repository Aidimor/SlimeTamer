using System.Collections;
using UnityEngine;


public class WaspFightScript : MonoBehaviour
{
    public Animator _waspAnimator;
    
    public GameObject _waspImageParent;
    public float _fallSpeed;




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
