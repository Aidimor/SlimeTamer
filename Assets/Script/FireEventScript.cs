using UnityEngine;

public class FireEventScript : MonoBehaviour
{
    public ParticleSystem[] _fireParticle;
    public GameObject[] _worlds;  


    public void FireExtinguishVoid()
    {
        for (int i = 0; i < _fireParticle.Length; i++)
        {
            _fireParticle[i].Stop();
        }        
    }
}
