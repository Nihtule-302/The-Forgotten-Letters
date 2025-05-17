using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Core.Scriptable_Events
{
    public interface IGameEventListener<T>
    {
        void OnEvenRaised(T data);
    }


    public class GameEventListener<T> : MonoBehaviour, IGameEventListener<T>
    {
        [SerializeField] GameEvent<T> gameEvent;
        [SerializeField] AssetReference assetReferenceEvent;
        [SerializeField] UnityEvent<T> responce;

        GameEvent<T> gameEventNew =>  EventLoader.Instance.GetEvent<GameEvent<T>>(assetReferenceEvent);

        void OnEnable() => gameEventNew.RegisterListener(this);

        void OnDisable() => gameEventNew.DeregisterListener(this);

        public void OnEvenRaised(T data) => responce.Invoke(data);
    }


    public class GameEventListener : GameEventListener<Unit>{}
}