using System.Collections;
using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Utilities.Audio;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    //States handling what behaviour system to use
    public enum BruteAttentionStates
    {
        Unaware,
        Alert,
        Hurt,
        Dead,
        KnockedOut
    }

    //States for handling the behaviour
    public enum BruteBehaviourStates
    {
        Idle, //standing still - Occurs in unaware, alert, and hurt
        Wander, //For random patrol around heart - Occurs in Unaware and hurt
        Investigate, //Inspecting a heard sound, but not quite agrrod to the player - Only in Alert
        Chase, //Hunting down the player that made the last sound before going into state - Only in Alert
        None

    }

    public class BruteStateController : MonoBehaviour
    {
        [SerializeField] private BruteAttentionStates _currentBruteAttentionState;
        [SerializeField] private BruteBehaviourStates _currentBruteBehaviour;
        [SerializeField] float firstAlertDelayTime;
        [SerializeField] private GameObject _heartPrefab;
        [SerializeField] BruteHearing _bruteHearing;
        private GameObject _spawnedHeart;
        [SerializeField] private BruteMovement _bruteMovementScript;
        public GameObject PlayerToChase;
        [SerializeField] BruteAnimation _bruteAnimation;
        [SerializeField] float _minIdleNoiseTime;
        [SerializeField] float _maxIdleNoiseTime;

        void Start()
        {
            TransitionToAttentionState(BruteAttentionStates.Unaware);
            TransitionToBehaviourState(BruteBehaviourStates.Wander);
        }

        public void TransitionToAttentionState(BruteAttentionStates newState)
        {
            if (_currentBruteAttentionState == newState || _currentBruteAttentionState == BruteAttentionStates.Dead || _currentBruteBehaviour == BruteBehaviourStates.None) return;
            if (_currentBruteAttentionState == BruteAttentionStates.Hurt && (newState == BruteAttentionStates.Unaware || newState == BruteAttentionStates.Alert)) return;
            _currentBruteAttentionState = newState;
            OnEnterAttentionState(_currentBruteAttentionState);
        }
        public void OnEnterAttentionState(BruteAttentionStates state)
        {
            switch (state)
            {
                case BruteAttentionStates.Unaware:
                    //_bruteHearing.OnExitAlertState();
                    _bruteAnimation.PlayNormal();
                    StartCoroutine(IdleSound());
                    break;
                case BruteAttentionStates.Alert:
                    _bruteAnimation.PlayAlert();
                    StopCoroutine(IdleSound());
                    AudioManager.Instance.PlayByKeyAttached("BruteAlert", transform);
                    break;
                case BruteAttentionStates.Hurt:
                    //  _bruteHearing.OnExitAlertState();
                    _bruteAnimation.PlayInjured();
                    StopCoroutine(IdleSound());
                    StartCoroutine(HurtSound());
                    TransitionToBehaviourState(BruteBehaviourStates.Idle);
                    break;
                case BruteAttentionStates.Dead:
                    _bruteMovementScript.OnDeathKO();
                    StopCoroutine(HurtSound());
                    StopCoroutine(IdleSound());
                    break;
                case BruteAttentionStates.KnockedOut:
                    StopCoroutine(IdleSound());
                    StopCoroutine(HurtSound());

                    break;
            }
        }
        public BruteAttentionStates GetAttentionState()
        {
            return _currentBruteAttentionState;
        }
        public void TransitionToBehaviourState(BruteBehaviourStates newState)
        {
            if (_currentBruteBehaviour == newState || _currentBruteAttentionState == BruteAttentionStates.Dead || _currentBruteBehaviour == BruteBehaviourStates.None) return;
            if (_currentBruteAttentionState == BruteAttentionStates.Hurt && (newState == BruteBehaviourStates.Investigate || newState == BruteBehaviourStates.Chase)) return;
            _currentBruteBehaviour = newState;
            OnEnterBehaviourState(_currentBruteBehaviour);
        }
        public void OnEnterBehaviourState(BruteBehaviourStates state)
        {
            switch (state)
            {
                case BruteBehaviourStates.Idle:
                    _bruteMovementScript.OnStartIdle();
                    _bruteMovementScript.OnStopChase();
                    break;
                case BruteBehaviourStates.Wander:
                    _bruteMovementScript.OnStartWander();
                    _bruteMovementScript.OnStopChase();
                    break;
                case BruteBehaviourStates.Investigate:
                    _bruteMovementScript.OnStopChase();
                    StopCoroutine(IdleSound());
                    break;
                case BruteBehaviourStates.Chase:
                    _bruteMovementScript.OnStartChase();
                    StopCoroutine(IdleSound());
                    break;

                case BruteBehaviourStates.None:

                    break;
            }
        }
        public void OnFirstAlert(GameObject player)
        {
            StartCoroutine(FirstAlertDelay(player));
        }
        IEnumerator FirstAlertDelay(GameObject player)
        {
            yield return new WaitForSeconds(firstAlertDelayTime);
            TransitionToBehaviourState(BruteBehaviourStates.Investigate);
            _bruteMovementScript.OnInvestigate(player);
        }
        public void OnSubsequentAlert(GameObject player)
        {
            TransitionToBehaviourState(BruteBehaviourStates.Investigate);
            _bruteMovementScript.OnInvestigate(player);
        }
        public void StartChasePlayer(GameObject playerToChase)
        {
            PlayerToChase = playerToChase;
            TransitionToAttentionState(BruteAttentionStates.Alert);
            TransitionToBehaviourState(BruteBehaviourStates.Chase);
        }
        IEnumerator IdleSound()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(_minIdleNoiseTime, _maxIdleNoiseTime));
                int index = Random.Range(0, 3);
                switch (index)
                {
                    case 0:
                        AudioManager.Instance.PlayByKey3D("BruteIdleBreath1", transform.position);
                        break;
                    case 1:
                        AudioManager.Instance.PlayByKey3D("BruteIdleBreath2", transform.position);
                        break;
                    case 2:
                        AudioManager.Instance.PlayByKey3D("BruteIdleBreath3", transform.position);
                        break;

                }
            }
        }
        IEnumerator HurtSound()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(_minIdleNoiseTime, _maxIdleNoiseTime));
                int index = Random.Range(0, 2);
                switch (index)
                {
                    case 0:
                        AudioManager.Instance.PlayByKey3D("BruteHurtIdleBreath1", transform.position);
                        break;
                    case 1:
                        AudioManager.Instance.PlayByKey3D("BruteHurtIdleBreath2", transform.position);
                        break;


                }
            }
        }
        public BruteBehaviourStates GetBehaviourState()
        {
            return _currentBruteBehaviour;
        }
    }
}