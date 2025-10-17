using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Core.Events
{
    public class EventSubscriber : MonoBehaviour
    {
        private readonly List<Action> _unsubscribeActions = new();

        protected void Subscribe<T>(Action<T> callback) where T : IEvent
        {
            if (EventBus.Instance)
            {
                EventBus.Instance.Subscribe(this, callback);
                _unsubscribeActions.Add(() => EventBus.Instance?.Unsubscribe<T>(this));
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var unsubscribe in _unsubscribeActions)
            {
                unsubscribe();
            }
            _unsubscribeActions.Clear();
        }
    }
}
