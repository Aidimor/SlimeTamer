using UnityEngine;

[CreateAssetMenu(menuName = "Events/NewEvent")]
public class GameEvent : ScriptableObject
{
    [System.Serializable]
    public enum EventClassification
    {
        Normal,
        Fight,
        Questionary
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
}
