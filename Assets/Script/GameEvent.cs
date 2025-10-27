using UnityEngine;

[CreateAssetMenu(menuName = "Events/NewEvent")]
public class GameEvent : ScriptableObject
{
    [System.Serializable]
    public enum EventClassification
    {
        Normal,
        Fight,
        Questionary,
        Chest,
        Intro,
        Shop
    }
    public EventClassification _eventClassification;

    [System.Serializable]
    public enum EventType
    {
        Bridge,
        Lagoon,
        Well,
        StrongAir,
        FallingBridge,
        Gears,
        FightWasp,
        FightSnail,
        Fire,
        BossFight0,
        BossFight1,
        BossFight2,
        BossFight3,
        BossFight4,
        BossFight5

    }
    public EventType _eventType;

    [System.Serializable]
    public enum WeakTo
    {
        Water,
        Air,
        Earth,
        Sand,
        Snow,
        Mud
    }
    public WeakTo[] _weakto;

    public GameObject _eventPrefab;

    public enum ChestItems
    {
        Water,
        Air,
        Earth,
        Fire
    }
    public ChestItems[] _chestItems;
}
