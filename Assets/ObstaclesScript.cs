using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesScript : MonoBehaviour
{
    public int _id;
    public GameObject[] _allObstacles;
    // Start is called before the first frame update
   public void SetObstacle()
    {
        _allObstacles[_id].gameObject.SetActive(true);
    }
}
