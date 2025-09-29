using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireEventScript : MonoBehaviour
{
    public ParticleSystem _fireParticle;
    public GameObject[] _worlds;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FireExtinguishVoid()
    {
        _fireParticle.Stop();
    }
}
