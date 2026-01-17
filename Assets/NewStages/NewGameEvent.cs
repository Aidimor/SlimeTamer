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
            O,
            Fe
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
            Column,
            MetalBall,
            MagnetoPlace,
            Electricity,
            CenterElectricity
        }
        public HazardsType _hazards;
        [System.Serializable]
        public enum Rotation
        {
            Center,
            Vertical,
            Horizontal
        }
        public Rotation _rotation;
        public bool _finished;
    }
    public Hazards[] _hazards;

    public int[] _atomPlace;
    public int[] _stepsPlace;

    [System.Serializable]
    public class BossAssets
    {
       
        public enum _Xposition
        {
            Left,
            Center,
            Right
        }
        public  _Xposition _xposition;
        public enum _Yposition
        {
            Top,
            Center,
            Bot
        }
        public _Yposition _yposition;

        [System.Serializable]
        public class GroundAttacks
        {
            public int _groundID;
            public int _attackCount;
            public int _tentacleID;
        }
        public GroundAttacks[] _groundAttacks;
    }
    public BossAssets[] _bossAssets;

    [System.Serializable]
    public enum Wind
    {
        Center,
        Left,
        Right
    }
    public Wind _wind;
}
