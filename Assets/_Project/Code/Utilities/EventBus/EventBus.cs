using System;
using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Player.MiscPlayer;
using UnityEngine;

namespace _Project.Code.Utilities.EventBus
{
    public class EventBus : Singleton<EventBus>

    {
        private readonly Dictionary<Type, List<EventSubscription>> _subscriptions = new();
        private readonly List<EventSubscription> _pendingRemovals = new();
        private bool _isPublishing;

        private class EventSubscription
        {
            public WeakReference TargetReference { get; set; }
            public Delegate Callback { get; set; }
            public bool MarkedForRemoval { get; set; }
        }

        public void Subscribe<T>(object target, Action<T> callback) where T : IEvent
        {
            var eventType = typeof(T);

            if (!_subscriptions.TryGetValue(eventType, out var subscriptionList))
            {
                subscriptionList = new List<EventSubscription>();
                _subscriptions[eventType] = subscriptionList;
            }

            subscriptionList.Add(new EventSubscription
            {
                TargetReference = new WeakReference(target),
                Callback = callback
            });
        }

        public void Unsubscribe<T>(object target) where T : IEvent
        {
            var eventType = typeof(T);

            if (!_subscriptions.TryGetValue(eventType, out var subscriptionList))
                return;

            foreach (var subscription in subscriptionList)
            {
                if (subscription.TargetReference.Target == target)
                {
                    if (_isPublishing)
                    {
                        subscription.MarkedForRemoval = true;
                        _pendingRemovals.Add(subscription);
                    }
                    else
                    {
                        subscriptionList.Remove(subscription);
                        break;
                    }
                }
            }
        }
        public void Publish<T>(T eventData) where T : IEvent
        {
            var eventType = typeof(T);
            if (!_subscriptions.TryGetValue(eventType, out var subscriptionList))
                return;

            _isPublishing = true;

            for (int i = subscriptionList.Count - 1; i >= 0; i--)
            {
                var subscription = subscriptionList[i];

                if (subscription.MarkedForRemoval)
                    continue;

                var target = subscription.TargetReference.Target;

                if (target == null)
                {
                    subscription.MarkedForRemoval = true;
                    _pendingRemovals.Add(subscription);
                    continue;
                }

                try
                {
                    ((Action<T>)subscription.Callback).Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error publishing event {typeof(T).Name}: {e.Message}");
                }
            }

            _isPublishing = false;
            CleanupPendingRemovals();
        }

        private void CleanupPendingRemovals()
        {
            if (_pendingRemovals.Count == 0)
                return;

            foreach (var subscription in _pendingRemovals)
            {
                foreach (var kvp in _subscriptions)
                {
                    kvp.Value.Remove(subscription);
                }
            }

            _pendingRemovals.Clear();
        }

        public void Clear()
        {
            _subscriptions.Clear();
            _pendingRemovals.Clear();
            _isPublishing = false;
        }

        protected override bool PersistBetweenScenes => true;

        private void OnDisable()
        {
            Clear();
        }

        private void OnDestroy()
        {
            Clear();    
        }
    }
}
