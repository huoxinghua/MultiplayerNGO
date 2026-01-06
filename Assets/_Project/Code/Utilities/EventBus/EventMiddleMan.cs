using System;
using System.Reflection;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Audio;
using Unity.Netcode;
using UnityEngine;
using EventBus = _Project.Code.Utilities.EventBus.EventBus;

public class EventMiddleMan : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //subscribe in spawn?
        EventBus.Instance.Subscribe<GameplayEvent>(this, HandleGameplayEvent);
    }

    private void Start()
    {
        //for testing? maybe better to subscribe not attached to spawn?
        EventBus.Instance.Subscribe<GameplayEvent>(this, HandleGameplayEvent);
    }

    private void HandleGameplayEvent(GameplayEvent e)
    {
        //if event has a sound, call sound logic function
        // if (e.EventSoundID != Eventid)
        // {
        //     HandleEventWithSound(e);
        // }
        /* Proves that subscribing to GameplayEvent can be found from publishing an inherited event
        Debug.Log("Recieved Gameplay Event");

        can get the sound id. When no parameter passed it will see as NoSound. When one is passed it will print the
        correct sound ID
        Debug.Log(e.EventSoundID.ToString());

        Can get the local, global, etc of the event based on the enum
        Debug.Log(e.EventSoundID.GetNetworkSoundAspect().ToString());
        */
    }

    private void HandleEventWithSound(GameplayEvent e)
    {
       
    }

    #region Spatial Sound RPCs
    //ServerRpc to properly call a client RPC for spatial Sounds
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpatialSoundDistributionServerRpc(EventIDs eventIDs, Vector3 position, float volume)
    {
        DistributeSpatialSoundClientRpc(eventIDs, position, volume);
    }
    //ClientRpc to distribute the spatial sound event for all players
    [ClientRpc(RequireOwnership = false)]
    private void DistributeSpatialSoundClientRpc(EventIDs eventIDs, Vector3 position, float volume)
    {
        EventBus.Instance.Publish(new PlaySpatialSoundEvent()
            { Key = eventIDs, Position = position, Volume = volume });
    }
    #endregion
    
    #region 2D Sound RPCs
    //ServerRpc to properly call a client RPC for 2D Sounds
    [ServerRpc(RequireOwnership = false)]
    private void Request2DSoundDistributionServerRpc(EventIDs eventIDs, float volume)
    {
        Distribute2DSoundClientRpc(eventIDs, volume);
    }
    //ClientRpc to distribute the 2D sound event for all players
    [ClientRpc(RequireOwnership = false)]
    private void Distribute2DSoundClientRpc(EventIDs eventIDs, float volume)
    {
        EventBus.Instance.Publish(new Play2DSoundEvent { Key = eventIDs, Volume = volume });
    }
    #endregion
}

