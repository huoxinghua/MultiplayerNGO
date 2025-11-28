using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Utility;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using _Project.Code.Gameplay.Market.Quota;

namespace _Project.Code.Gameplay.Interactables.Truck
{
    public class LeaveMissionCollider : MonoBehaviour, IHoldToInteract
    {
        [SerializeField] private float _timeToHold;
        private bool _isInteracting = false;
        private List<GameObject> _players = new List<GameObject>();
        private List<Timer> _holdTimers = new List<Timer>();
        public void OnHold(GameObject interactingPlayer)
        {
            _players.Add(interactingPlayer);
            _holdTimers.Add(new Timer(_timeToHold));
            _holdTimers[_players.IndexOf(interactingPlayer)].Start();
        }
        public void LateUpdate()
        {
            if (_holdTimers.Count < 1) return;
            for (int i = _holdTimers.Count - 1; i >= 0; i--)
            {
                var timer = _holdTimers[i];
                timer.TimerUpdate(Time.deltaTime);
                // Debug.Log(timer.GetElapsed());

                if (timer.IsComplete)
                {
                    Debug.Log(_holdTimers.IndexOf(timer).ToString());
                    HandleLeave(_players[i]);
                }
            }
        }
        public void OnRelease(GameObject interactingPlayer)
        {
            if(_holdTimers.Count < 1) return;
            if(_players.Count < 1) return;
            _holdTimers.RemoveAt(_players.IndexOf(interactingPlayer));
            _players.Remove(interactingPlayer);
        }
        public void HandleLeave(GameObject playerLeaving)
        {
            EventBus.Instance.Publish<SuccessfulDayEvent>(new SuccessfulDayEvent());
            OnRelease(playerLeaving);
        }
        public float GetTimeToOpen()
        {
            return _timeToHold;
        }
    }
 
    
}