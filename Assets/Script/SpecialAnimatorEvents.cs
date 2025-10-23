using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAnimatorEvents : MonoBehaviour
{
    public ParticleSystem _chestParticle;
    [SerializeField] private MainGameplayScript _scriptMain;
    [SerializeField] private MainController _scriptMainController;
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

    public void PlaySlide()
    {
        _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._stoneSlide);
    }
    public void PlayClose()
    {
        _scriptMainController._scriptSFX.PlaySound(_scriptMainController._scriptSFX._stoneClose);
    }

    public void ChargingVoid()
    {
        _scriptMain._chargingAttackEnemy.Play();
    }

    public void LaserShootVoid()
    {
        _scriptMain._chargingAttackEnemy.Stop();
        _scriptMain._AttackEnemy.Play();
    }

    public void LaserStopVoid()
    {
        _scriptMain._chargingAttackEnemy.Stop();
        _scriptMain._AttackEnemy.Stop();
    }
}
