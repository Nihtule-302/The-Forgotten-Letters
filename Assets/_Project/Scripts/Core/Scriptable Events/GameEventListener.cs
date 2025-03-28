using UnityEngine;
using UnityEngine.Events;

namespace _Project.Scripts.Core.Scriptable_Events
{
    public interface IGameEventListener<T>
    {
        void OnEvenRaised(T data);
    }


    public class GameEventListener<T> : MonoBehaviour, IGameEventListener<T>
    {
        [SerializeField] GameEvent<T> gameEvent;
        [SerializeField] UnityEvent<T> responce;

        void OnEnable() => gameEvent.RegisterListener(this);
        void OnDisable() => gameEvent.DeregisterListener(this);

        public void OnEvenRaised(T data) => responce.Invoke(data);
    }


    public class GameEventListener : GameEventListener<Unit>{}
}