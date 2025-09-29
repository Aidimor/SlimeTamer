using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAnimatorEvents : MonoBehaviour
{
    public ParticleSystem _chestParticle;
    [SerializeField] private MainGameplayScript _scriptMain;
    public void ChestParticleVoid()
    {
        _chestParticle.Play();
    }

    public void DeactivateObjects()
    {
        for(int i = 0; i < _scriptMain._itemGotPanel._itemObject.Length; i++)
        {
            _scriptMain._itemGotPanel._itemObject[i].SetActive(false);
        }

    }
}
