using System.Collections.Generic;
using UnityEngine;


public class GameEvent<T> : ScriptableObject
{
    readonly List<IGameEventListener<T>> listeners = new();

    public void Raise(T data)
    {
        for (var i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEvenRaised(data);
        }
    }

    public void RegisterListener(IGameEventListener<T> listener) => listeners.Add(listener);
    public void DeregisterListener(IGameEventListener<T> listener) => listeners.Remove(listener);
}

[CreateAssetMenu(fileName = "GameEvent", menuName = "Events/GameEvent")]
public class GameEvent : GameEvent<Unit>
{
    public void Raise() => Raise(Unit.Defualt);
}

public struct Unit
{
    public static Unit Defualt => default;
}