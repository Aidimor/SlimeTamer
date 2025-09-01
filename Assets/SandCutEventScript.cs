using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandCutEventScript : MonoBehaviour
{
    public Animator _anim;
    public ParticleSystem _cutParticle;

    // Start is called before the first frame update
    public void Start()
    {
        _anim = GetComponent<Animator>();
    }

    public void CutParticleVoid()
    {
        _cutParticle.Play();
    }

    public void StartCuttingVoid()
    {
        _anim.SetTrigger("CutBridge");
    }
}
