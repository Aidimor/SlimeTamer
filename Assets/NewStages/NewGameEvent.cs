using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
[CreateAssetMenu(menuName = "NewStages/NewStage")]
public class NewGameEvent : ScriptableObject
{
    public int _spawnPoint;
    public int _exitPoint;

    public List<int> _allPlaces = new List<int>();

    [System.Serializable]
    public class Elements
    {
        public int _onPlace;
        public enum ElementType
        {
            C,
            H,
            O
        }
        public ElementType _elementType;
        public int _quantity;
    }
    public Elements[] _elements;

    [System.Serializable]
    public class Hazards
    {
        public int _onPlace;
        public enum HazardsType
        {
            Fire,
            Hole,
            Switch,
            Switch2
        }
        public HazardsType _hazards;
    }
    public Hazards[] _hazards;


}
