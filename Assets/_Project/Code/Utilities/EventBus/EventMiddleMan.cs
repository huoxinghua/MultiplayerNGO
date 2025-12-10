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
        if (e.EventSoundID != SoundIDs.NoSound)
        {
            HandleEventWithSound(e);
        }
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
        //if the events SoundID attribute is global and spatial, request a client rpc to play Spatial Sound
        if (e.EventSoundID.GetNetworkSoundAspect() == NetworkAspectAttribute.SoundAspect.Global)
        {
            RequestSpatialSoundDistributionServerRpc(e.EventSoundID, e.EventPosition, e.EventVolume);
        }
        //if the events SoundID attribute is is global and 2D, request a client rpc to play 2D Sound
        else if (e.EventSoundID.GetNetworkSoundAspect() == NetworkAspectAttribute.SoundAspect.Global2D)
        {
            Request2DSoundDistributionServerRpc(e.EventSoundID, e.EventVolume);
        }
        //if the events SoundID attribute is Local and Spatial, Publish an event to play spatial sound
        //(will only execute on the client)
        else if (e.EventSoundID.GetNetworkSoundAspect() == NetworkAspectAttribute.SoundAspect.LocalOnly)
        {
            EventBus.Instance.Publish<PlaySpatialSoundEvent>(new PlaySpatialSoundEvent
            {
                SoundID = e.EventSoundID, Position = e.EventPosition, Volume = e.EventVolume
            });
        }
        //if the events SoundID attribute is Local and 2D, Publish an event to play 2D sound
        //(will only execute on the client)
        else if (e.EventSoundID.GetNetworkSoundAspect() == NetworkAspectAttribute.SoundAspect.Local2D)
        {
            EventBus.Instance.Publish<Play2DSoundEvent>(new Play2DSoundEvent
            {
                SoundID = e.EventSoundID, Volume = e.EventVolume
            });
        }
    }

    #region Spatial Sound RPCs
    //ServerRpc to properly call a client RPC for spatial Sounds
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpatialSoundDistributionServerRpc(SoundIDs soundID, Vector3 position, float volume)
    {
        DistributeSpatialSoundClientRpc(soundID, position, volume);
    }
    //ClientRpc to distribute the spatial sound event for all players
    [ClientRpc(RequireOwnership = false)]
    private void DistributeSpatialSoundClientRpc(SoundIDs soundID, Vector3 position, float volume)
    {
        EventBus.Instance.Publish(new PlaySpatialSoundEvent()
            { SoundID = soundID, Position = position, Volume = volume });
    }
    #endregion
    
    #region 2D Sound RPCs
    //ServerRpc to properly call a client RPC for 2D Sounds
    [ServerRpc(RequireOwnership = false)]
    private void Request2DSoundDistributionServerRpc(SoundIDs soundID, float volume)
    {
        Distribute2DSoundClientRpc(soundID, volume);
    }
    //ClientRpc to distribute the 2D sound event for all players
    [ClientRpc(RequireOwnership = false)]
    private void Distribute2DSoundClientRpc(SoundIDs soundID, float volume)
    {
        EventBus.Instance.Publish(new Play2DSoundEvent { SoundID = soundID, Volume = volume });
    }
    #endregion
}

