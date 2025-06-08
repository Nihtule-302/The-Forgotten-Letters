using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Core.Scriptable_Events
{
    public class GameEvent<T> : ScriptableObject
    {
        private readonly List<IGameEventListener<T>> listeners = new();

        public void Raise(T data)
        {
            for (var i = listeners.Count - 1; i >= 0; i--) listeners[i].OnEventRaised(data);
        }

        public void RegisterListener(IGameEventListener<T> listener)
        {
            if(listeners.Contains(listener))
            {
                Debug.LogWarning("Listener already registered.");
                return;
            }
            listeners.Add(listener);
        }

        public void DeregisterListener(IGameEventListener<T> listener)
        {
            if (!listeners.Contains(listener))
            {
                Debug.LogWarning("Listener not registered.");
                return;
            }
            listeners.Remove(listener);
        }
    }

    [CreateAssetMenu(fileName = "GameEvent", menuName = "Events/GameEvent")]
    public class GameEvent : GameEvent<Unit>
    {
        public void Raise()
        {
            Raise(Unit.Defualt);
        }
    }

    public struct Unit
    {
        public static Unit Defualt => default;
    }
}