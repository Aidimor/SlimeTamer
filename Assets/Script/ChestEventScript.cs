using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using LoL;

public class ChestEventScript : MonoBehaviour
{
    public MainGameplayScript _scriptMain;
    public Animator _princessAnim;
    public GameObject[] _worlds;
    public Sprite[] _allPrincessSprites;


    // Start is called before the first frame update



    public void ItemGet()
    {

        switch (_scriptMain._scriptEvents._specialEvents[_scriptMain._GamesList[_scriptMain._scriptEvents._onEvent]]._chestItems[0])
        {
            case GameEvent.ChestItems.Water:
                _scriptMain._scriptFusion._elementsOptions[0]._unlocked = true;
                _scriptMain._itemGotPanel._itemObject[0].SetActive(true);
                _scriptMain._itemGotPanel.key = "description1";
                _scriptMain._scriptMain._saveLoadValues._elementsUnlocked[1] = true;
                break;
            case GameEvent.ChestItems.Air:
                _scriptMain._scriptFusion._elementsOptions[1]._unlocked = true;
                _scriptMain._itemGotPanel._itemObject[1].SetActive(true);
                _scriptMain._itemGotPanel.key = "description2";
                _scriptMain._scriptMain._saveLoadValues._elementsUnlocked[2] = true;
                break;
            case GameEvent.ChestItems.Earth:
                _scriptMain._scriptFusion._elementsOptions[2]._unlocked = true;
                _scriptMain._itemGotPanel._itemObject[2].SetActive(true);
                _scriptMain._itemGotPanel.key = "description3";
                _scriptMain._scriptMain._saveLoadValues._elementsUnlocked[3] = true;
                break;
        }
        _scriptMain._scriptMain._scriptSFX.PlaySound(_scriptMain._scriptMain._scriptSFX._newElement);

        if (_scriptMain._itemGotPanel._Message != null && !string.IsNullOrEmpty(_scriptMain._itemGotPanel.key))
        {
            // Obtener el valor traducido de la key (ej: "announce1")
            string localizedText = GameInitScript.Instance.GetText(_scriptMain._itemGotPanel.key);

            // Asignar el texto al panel de mensaje
            _scriptMain._itemGotPanel._Message.text = localizedText;
        }

        //_scriptMain._scriptMain._saveLoadValues._elementsUnlocked[1] = true;

    }
}
