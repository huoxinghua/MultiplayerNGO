using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.Audio;
using Unity.Netcode;
using UnityEngine;

public class EventMiddleMan : NetworkBehaviour
{
    
}
public class GameplayEvent : IEvent
{
    public SoundIDs SoundID;
    public GameplayEvent(SoundIDs itemID = SoundIDs.NoSound)
    {
        this.SoundID = itemID;
    }
}
public class BaseballBatHitEvent : GameplayEvent
{
    public BaseballBatHitEvent(SoundIDs itemID) :  base(itemID)
    {

    }
}
