using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesScript : MonoBehaviour
{
    public int _id;
    public GameObject[] _allObstacles;
    public ParticleSystem _fireParticle;
    public ParticleSystem _smokeParticle;
    public ParticleSystem _electricityParticle;
    public ParticleSystem _electricityCenterParticle;
    public ParticleSystem _gravityPoint;
    public ParticleSystem _dustParticle;

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
        yield return new WaitForSeconds(0.5f);
        MainController.Instance._scriptSFX.PlaySound(MainController.Instance._scriptSFX._chooseElement);
    
        _allObstacles[2].SetActive(false);
        _allObstacles[3].SetActive(true);
        
  
    }
    }
