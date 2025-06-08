using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace _Project.Scripts.Core.Scriptable_Events
{
    public interface IGameEventListener<T>
    {
        void OnEventRaised(T data);
    }


    public class GameEventListener<T> : MonoBehaviour, IGameEventListener<T>
    {
        [SerializeField] private AssetReference assetReferenceEvent;
        [SerializeField] private GameEvent<T> fallbackEvent;
        [SerializeField] private UnityEvent<T> responce;

        private GameEvent<T> currentEvent;

        private void OnEnable()
        {
            currentEvent = fallbackEvent;
            RegisterListener();

            if (EventLoader.Instance != null)
            {
                TrySwapToAddressableEvent();
            }
            else
            {
                EventLoader.OnInitialized += HandleEventLoaderReady;
            }
        }

        public void RegisterListener()
        {
            
            currentEvent?.RegisterListener(this);
        }

        public void DeregisterListener()
        {
            currentEvent?.DeregisterListener(this);
        }

        private void OnDisable()
        {
            currentEvent?.DeregisterListener(this);
            EventLoader.OnInitialized -= HandleEventLoaderReady;
        }

        public void OnEventRaised(T data)
        {
            responce.Invoke(data);
        }

        private void HandleEventLoaderReady()
        {
            TrySwapToAddressableEvent();
            EventLoader.OnInitialized -= HandleEventLoaderReady;
        }

        private void TrySwapToAddressableEvent()
        {
            try
            {
                var loadedEvent = EventLoader.Instance.GetEvent<GameEvent<T>>(assetReferenceEvent);

                if (loadedEvent != null && loadedEvent != currentEvent)
                {
                    currentEvent?.DeregisterListener(this);
                    currentEvent = loadedEvent;
                    currentEvent.RegisterListener(this);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to swap to addressable event: {e.Message}");
            }
        }
    }


    public class GameEventListener : GameEventListener<Unit>
    {
    }
}