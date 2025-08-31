using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Events/NewEvent")]
public class GameEvent : ScriptableObject
{
    [System.Serializable]
    public enum EventType
    {
       Bridge,
       Lagoon,
       Well,
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
    public WeakTo _weakto;

    public GameObject _eventPrefab;

    
}

