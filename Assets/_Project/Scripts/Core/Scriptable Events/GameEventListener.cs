using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace _Project.Scripts.Core.Scriptable_Events
{
    public interface IGameEventListener<T>
    {
        void OnEvenRaised(T data);
    }


    public class GameEventListener<T> : MonoBehaviour, IGameEventListener<T>
    {
        [SerializeField] private AssetReference assetReferenceEvent;
        [SerializeField] private UnityEvent<T> responce;

        private GameEvent<T> gameEventNew => EventLoader.Instance.GetEvent<GameEvent<T>>(assetReferenceEvent);

        private void OnEnable()
        {
            gameEventNew.RegisterListener(this);
        }

        private void OnDisable()
        {
            gameEventNew.DeregisterListener(this);
        }

        public void OnEvenRaised(T data)
        {
            responce.Invoke(data);
        }
    }


    public class GameEventListener : GameEventListener<Unit>
    {
    }
}