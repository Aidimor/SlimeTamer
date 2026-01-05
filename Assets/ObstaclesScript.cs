using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesScript : MonoBehaviour
{
    public int _id;
    public GameObject[] _allObstacles;
    public ParticleSystem _fireParticle;
    public ParticleSystem _smokeParticle;

    // Start is called before the first frame update
    //public void SetObstacle()
    //{
    //    _allObstacles[_id].gameObject.SetActive(true);
    //}

    public void LevelPressed()
    {
        StartCoroutine(LovePressedNumerator());
    }

    public IEnumerator LovePressedNumerator()
    {
        yield return new WaitForSeconds(1);
        _allObstacles[2].SetActive(false);
        _allObstacles[3].SetActive(true);
    }
    }
