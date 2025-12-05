using System;
using System.Collections.Generic;
using _Project.Code.Gameplay.NPC.Hostile.DollEnemy;
using _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States;
using _Project.Code.Utilities.Singletons;
using _Project.Code.Utilities.Utility;
using Unity.Netcode;
using UnityEngine;

public class DollPerception : NetworkBehaviour
{

    //Timers
    private Timer HuntingCooldown;
    private Timer PerceptionCheckCooldown;

    //Players
    private PlayerCamerasNetSingleton PlayerCameras => PlayerCamerasNetSingleton.Instance;
    private List<Transform> CameraTransforms = new List<Transform>();
    private Dictionary<ulong, Transform> _cachedCameraTransforms = new Dictionary<ulong, Transform>();
    public Transform CurrentClosestPlayer { get; private set; }
    
    [field: Header("Doll Core")]
    [field: SerializeField] public DollStateMachine DollStateMachine { get; private set; }
    [field: SerializeField] public DollSO DollSO { get; private set; }

    [SerializeField] private Transform[] _rayCastPoints;

    #region Setup

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        PlayerCameras.PlayerCamerasNetList.OnListChanged += HandlePlayerListChange;
        if (!IsServer) return;

        HuntingCooldown = new Timer(DollSO.FirstHuntCooldown);
        HuntingCooldown.Start();

        PerceptionCheckCooldown = new Timer(DollSO.PerceptionFrequency);
        PerceptionCheckCooldown.Start();
    }

    #endregion

    #region Update

    private void Update()
    {
        if (!IsServer) return;
        PerceptionCheckCooldown.TimerUpdate(Time.deltaTime);
        if (PerceptionCheckCooldown.IsComplete)
        {
            OnPerceptionTimerComplete();
        }

        HuntingCooldown.TimerUpdate(Time.deltaTime);
        if (HuntingCooldown.IsComplete)
        {
            OnHuntingCooldownComplete();
        }
    }

    #endregion

    private void OnPerceptionTimerComplete()
    {
        bool anyPlayerLooking = false;
        //loop through list of players, call CheckPlayerProximity and CheckPlayerLineOfSight on each
        //Players will only be added to the list when they are in the interior. When they leave, they will remove themselves

        //for each player looped through, if CheckPlayerProximity returns false, skip checking lineofsight

        //If any of the CheckPlayerLineOfSight returns true tell SM that its looked at and set anyPlayerLooking to true.
        //If none of the players in the check pass this (not looking) tell SM looked away. This is known through anyPlayerLooking;
        if (!RebuildCameraList())
        {
            return;
        }

        foreach (Transform CamTransform in CameraTransforms)
        {
            //if not in proximity, dont check anything else
            if (!CheckPlayerProximity(CamTransform)) continue;

            //if Line of sight, player looking, else they wont be
            if (CheckPlayerLineOfSight(CamTransform))
            {
                anyPlayerLooking = true;
                break;
            }
        }

        //Set currentClosestPlayer
        CurrentClosestPlayer = GetClosestPlayer();

        //inform state machine if looked at
        if (anyPlayerLooking)
        {
            DollStateMachine.HandleLookedAt();
        }
        else
        {
            DollStateMachine.HandleLookAway(CurrentClosestPlayer);
        }

        PerceptionCheckCooldown.Reset();
    }

    private bool CheckPlayerProximity(Transform playerTransform)
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        //Checks if doll can attempt to kill
        if (distance <= DollSO.KillRange) DollStateMachine.HandleInKillDistance(playerTransform.gameObject);

        //Checks if player is "in proximity"
        if (distance <= DollSO.ProximityRange)
        {
            return true;
        }

        return false;
    }

    private bool CheckPlayerLineOfSight(Transform playerCameraTransform)
    {
        //check camera forward with a FOV angle. If enemy is within this FOV
        //Raycast to the points to check on enemy (just checking center of mass can work,
        //but movement would be seen around bends, or any blocking object. Checking a few points on the edges of the enemy will be better.)
        //check if enemy is in player FOV. If not, return false
        if (!IsInFOV(playerCameraTransform, DollSO.DollFOV)) return false;
        
        //Check if player has a line of sight to any raycast point
        if(!CheckForObstructions(playerCameraTransform)) return false;
        
        //if hasnt returned, player sees enemy, return true
        return true;
    }

    public bool CheckForObstructions(Transform cameraTransform)
    {
        bool didHitPoint = false;
        //replace raycast points with bounds
        foreach (Transform rayPoint in _rayCastPoints)
        {
            Vector3 direction = cameraTransform.position - rayPoint.position;
            float distance = direction.magnitude;
            if (!Physics.Raycast(rayPoint.position, direction.normalized, distance, LayerMasks.Instance.ObstructionMask))
            {
                didHitPoint = true;
                break;
            }  
        }
        return didHitPoint;
    }
    public bool IsInFOV(Transform cameraTransform, float fovAngle)
    {
        Vector3 toTarget = (transform.position - cameraTransform.position).normalized;

        float threshold = Mathf.Cos(fovAngle * 0.5f * Mathf.Deg2Rad);

        return Vector3.Dot(cameraTransform.forward, toTarget) >= threshold;
    }
    
    //maybe find if player in kill distance in playerProx to avoid math twice?

    private Transform GetClosestPlayer()
    {
        float shortestDistance = float.MaxValue;
        Transform closestPlayer = null;
        foreach (Transform cameraTransform in CameraTransforms)
        {
            float distance = Vector3.Distance(transform.position, cameraTransform.position);
            if (distance < shortestDistance)
            {
                closestPlayer = cameraTransform;
                shortestDistance = distance;
            }
        }
        if(closestPlayer != null) DollStateMachine.SetHuntedPlayer(closestPlayer);
        return closestPlayer;
    }

    private void OnHuntingCooldownComplete()
    {
        //maybe pass through the closest player here? Or should I update that in playerProximity/OnPercepetionTimerComplete?
        DollStateMachine.HandleHuntingTimerDone();
    }

    private void HandlePlayerListChange(NetworkListEvent<NetworkObjectReference> networkListEvent)
    {
        if (!IsServer) return;

        // Clean up cache for removed players
        if (networkListEvent.Type == NetworkListEvent<NetworkObjectReference>.EventType.Remove ||
            networkListEvent.Type == NetworkListEvent<NetworkObjectReference>.EventType.RemoveAt)
        {
            if (networkListEvent.Value.TryGet(out NetworkObject removedObj))
            {
                _cachedCameraTransforms.Remove(removedObj.NetworkObjectId);
            }
        }

        if (PlayerCameras.PlayerCamerasNetList.Count <= 0 && DollStateMachine.GetCurrentState() != StateEnum.WanderState)
        {
            // Handles going back to wander state if no valid players remain.
            DollStateMachine.SetHuntedPlayer(null);
            DollStateMachine.HandleNoValidPlayers();
            HuntingCooldown.Reset(DollSO.SubsequentHuntCooldown);
        }
    }

    private bool RebuildCameraList()
    {
        if (!IsServer) return false;

        // 1. Clear the working list (not the cache)
        CameraTransforms.Clear();

        // 2. Check the NetworkList count immediately
        if (PlayerCameras.PlayerCamerasNetList.Count <= 0)
        {
            return false;
        }

        // 3. Iterate and use cached transforms where possible
        foreach (NetworkObjectReference camRefs in PlayerCameras.PlayerCamerasNetList)
        {
            if (camRefs.TryGet(out NetworkObject camNetObj))
            {
                ulong netId = camNetObj.NetworkObjectId;

                // Check if we already cached this camera transform
                if (_cachedCameraTransforms.TryGetValue(netId, out Transform cachedTransform) && cachedTransform != null)
                {
                    CameraTransforms.Add(cachedTransform);
                }
                else
                {
                    // Not cached yet - do the expensive GetComponentInChildren once
                    Transform cameraTransform = camNetObj.GetComponentInChildren<Camera>()?.transform;
                    if (cameraTransform != null)
                    {
                        _cachedCameraTransforms[netId] = cameraTransform;
                        CameraTransforms.Add(cameraTransform);
                    }
                }
            }
        }

        // 4. Final check and closest player logic
        if (CameraTransforms.Count <= 0)
        {
            DollStateMachine.SetHuntedPlayer(null);
            return false;
        }

        CurrentClosestPlayer = GetClosestPlayer();
        return true;
    }
}
