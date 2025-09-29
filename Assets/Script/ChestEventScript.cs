using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestEventScript : MonoBehaviour
{
    public MainGameplayScript _scriptMain;
    public Animator _chestAnimator;
    public GameObject[] _worlds;
    // Start is called before the first frame update



    public void ItemGet()
    {
        switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._chestItems[0])
        {
            case GameEvent.ChestItems.Water:
                _scriptMain._scriptFusion._elementsOptions[0]._unlocked = true;
                _scriptMain._itemGotPanel._itemObject[0].SetActive(true);
                _scriptMain._itemGotPanel._Message.text = "Water Particles Obtained";
                _scriptMain._scriptRythm._elementsInfo[1]._unlocked = true;
                break;
            case GameEvent.ChestItems.Air:
                _scriptMain._scriptFusion._elementsOptions[1]._unlocked = true;
                _scriptMain._itemGotPanel._itemObject[1].SetActive(true);
                _scriptMain._itemGotPanel._Message.text = "Air Particles Obtained";
                _scriptMain._scriptRythm._elementsInfo[2]._unlocked = true;
                break;
            case GameEvent.ChestItems.Earth:
                _scriptMain._scriptFusion._elementsOptions[2]._unlocked = true;
                _scriptMain._itemGotPanel._itemObject[2].SetActive(true);
                _scriptMain._itemGotPanel._Message.text = "Earth Particles Obtained";
                _scriptMain._scriptRythm._elementsInfo[3]._unlocked = true;
                break;
        }

    }
}
