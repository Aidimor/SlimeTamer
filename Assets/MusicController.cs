using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource _audioBGM;
    public AudioClip[] _allThemes;
    // Start is called before the first frame update
    void Start()
    {
        _audioBGM = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMusic()
    {
        _audioBGM.Play();
    }
}
