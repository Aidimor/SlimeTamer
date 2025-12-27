using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMainGameplay : MonoBehaviour
{
    public Transform[] _allPositions;
    public GameObject[] _groundAssets;
    public NewGameEvent[] _allStages;
    public Transform _parent;
    public GameObject _elementPrefab;
    public int _idStage;

    void Start()
    {
        StageCreationVoid();
        SetElements();
    }

    public void StageCreationVoid()
    {
        for (int i = 0; i < _allStages[_idStage]._allPlaces.Count; i++)
        {
            GameObject ground = Instantiate(_groundAssets[_idStage], _parent);

            RectTransform groundRT = ground.GetComponent<RectTransform>();
            RectTransform targetRT =
                _allPositions[_allStages[_idStage]._allPlaces[i]].GetComponent<RectTransform>();

            if (groundRT != null && targetRT != null)
            {
                // Posición en mundo del target
                Vector3 worldPos = targetRT.position;

                // Convertir a local del nuevo parent
                groundRT.position = worldPos;

                groundRT.localScale = Vector3.one;
            }
        }

        Debug.Log("se crea");
    }

    public void SetElements()
    {
        for(int i = 0; i < _allStages[_idStage]._elements.Length; i++)
        {
            GameObject Element = Instantiate(_elementPrefab, transform.position, transform.rotation);
            switch (_allStages[_idStage]._elements[i]._elementType)
            {
                case NewGameEvent.Elements.ElementType.C:             
                    Element.GetComponent<ElementOrbScript>().ID = 0;           
                    break;
                case NewGameEvent.Elements.ElementType.H:
                    Element.GetComponent<ElementOrbScript>().ID = 1;
                    break;
                case NewGameEvent.Elements.ElementType.O:
                    Element.GetComponent<ElementOrbScript>().ID = 2;
                    break;
            }
            Element.GetComponent<ElementOrbScript>()._quantity = _allStages[_idStage]._elements[i]._quantity;
            Element.transform.parent = _parent.transform;
            Element.transform.localScale = Vector3.one;
            Element.GetComponent<RectTransform>().anchoredPosition = _allPositions[_allStages[_idStage]._elements[i]._onPlace].GetComponent<RectTransform>().anchoredPosition;
            Element.GetComponent<ElementOrbScript>().ElementSetVoid();
        }
    }

}
