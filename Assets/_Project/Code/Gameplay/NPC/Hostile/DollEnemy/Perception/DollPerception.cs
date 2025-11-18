using System;
using _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States;
using _Project.Code.Utilities.Utility;
using Unity.Netcode;
using UnityEngine;

public class DollPerception : NetworkBehaviour
{
    //[field: SerializeField] public DollSO DollSO {get; private set;
    private Timer HuntingCooldown;
    private Timer PerceptionCheckCooldown;
    [field: SerializeField] public DollStateMachine  DollStateMachine { get; private set; }

    #region Setup

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        // HuntingCooldown = new Timer(DollSO.FirstHuntDelay);
        // HuntingCooldown.Start();
        
        //PerceptionCheckCooldown = new Timer(DollSO.PerceptionFrequency);
        // PerceptionCheckCooldown.Start();
    }

    #endregion

    #region Update

    private void Update()
    {
        if (!IsServer) return;
       // HuntingCooldown.TimerUpdate(Time.deltaTime);
       // PerceptionCheckCooldown.TimerUpdate(Time.deltaTime);
       //if(PerceptionCheckCooldown.IsComplete(){ OnPerceptionTimerComplete();}
       //if(HuntingCooldown.IsComplete()){ OnHuntingCooldownComplete();}
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
        //

        //how to inform enemy when a player is removed from a list? 

        //PerceptionCheckCooldown.Reset();
    }
    
    private bool CheckPlayerProximity(Transform playerTransform)
    {
        //check player distance from Doll. If in proximity, return true; 
        
        //temp
        return false;
    }

    private bool CheckPlayerLineOfSight(Transform playerCameraTransform)
    {
        //check camera forward with a FOV angle. If enemy is within this FOV
        //Raycast to the points to check on enemy (just checking center of mass can work,
        //but movement would be seen around bends, or any blocking object. Checking a few points on the edges of the enemy will be better.)
     
        //if cast hits enemy, return true, else false.
        
        //temp 
        return false;
    }

    //maybe find if player in kill distance in playerProx to avoid math twice?
    private bool CheckPlayerInKillDistance(Transform playerTransform)
    {
        
        //if player in killDistance, inform SM
        
        return false;
    }

    private Transform GetClosestPlayer()
    {
        //return the players transform that is closest to the Doll
        return null;
    }
    
    private void OnHuntingCooldownComplete()
    {
        //maybe pass through the closest player here? Or should I update that in playerProximity/OnPercepetionTimerComplete?
        DollStateMachine.HandleHuntingTimerDone();
    }
}