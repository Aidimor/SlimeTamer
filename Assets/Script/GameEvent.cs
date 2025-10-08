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
        Intro
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
        BossFight1,
        BossBattles,
        other

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
