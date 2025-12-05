using System.Collections.Generic;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Utility;
using UnityEngine;

namespace _Project.Code.Gameplay.Interactables
{
    public class TeleDoor : MonoBehaviour , IHoldToInteract
    {
        [SerializeField] private float _timeToHold;
        private bool _isInteracting = false;
        private List<GameObject> _players = new List<GameObject>();
        private List<Timer> _holdTimers = new List<Timer>();
        [SerializeField] private Transform _thisDoor;
        private Transform _linkedDoor;
        [SerializeField] private int indexForLink;
        public void OnHold(GameObject interactingPlayer)
        {
            _players.Add(interactingPlayer);
            _holdTimers.Add(new Timer(_timeToHold));
            _holdTimers[_players.IndexOf(interactingPlayer)].Start();
        }
        public void Awake()
        {
            EventBus.Instance.Subscribe<LinkDoorEvent>(this, SetLinkTransform);
        
        }
        public void Start()
        {
            EventBus.Instance.Publish<LinkDoorEvent>(new LinkDoorEvent { LinkIndex = indexForLink, LinkPos = _thisDoor });
        }
        public void SetLinkTransform(LinkDoorEvent linkDoorEvent)
        {
        
            if(linkDoorEvent.LinkIndex == indexForLink && linkDoorEvent.LinkPos != _thisDoor)
            {
                _linkedDoor = linkDoorEvent.LinkPos;
            }
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
                    HandleTeleport(_players[i]);
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
        public void HandleTeleport(GameObject playerTeleporting)
        {
            if(_linkedDoor == null) { return; }
            CharacterController cc = playerTeleporting.GetComponent<CharacterController>();
            cc.enabled = false;
            playerTeleporting.transform.position = _linkedDoor.position;
            cc.enabled = true;
            EventBus.Instance.Publish<TeleDoorEvent>(new TeleDoorEvent());
            EventBus.Instance.Publish(new PlayerChangeArea());
            OnRelease(playerTeleporting);
        }
        public float GetTimeToOpen()
        {
            return _timeToHold;
        }
    }
    public struct LinkDoorEvent: IEvent
    {
        public int LinkIndex;
        public Transform LinkPos;
    }

    public class PlayerChangeArea : IEvent
    {
        
    }
}
