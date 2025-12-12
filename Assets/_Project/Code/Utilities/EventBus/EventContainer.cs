using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.Audio;
using _Project.Code.Utilities.EventBus;
using UnityEngine;
using UnityEngine.LightTransport;

namespace _Project.Code.Utilities.EventBus
{
    public class EventContainer
    {
        
    }
    #region GameplayEvents
    
    //Likely will all be for sounds. Poor planning and fore thought as these could have been used for SO much more
    
    #region How to publish a gameplay event
    /*
     Publishing a gameplay event uses a new function within the event bus. This function is called PublishGameplayEvent
     It is done this way to allow subscriptions to listen to all unique events of a type. It will still publish as a unique when 
     done this way, but it can also be read with all the others of its type separately
     
     To publish with passed in parameters/arguments
     EventBus.Instance.PublishGameplayEvent<EventChild>(new EventChild(param1, param2, etc));
     
     To publish using defaults
      EventBus.Instance.PublishGameplayEvent<EventChild>(new EventChild());
     */
     #endregion
     
    #region Untested multi use version of publish<base, child>
     /*
     There is currently an untested version that would work for any base - child events. 
     
     Publishing would be the same for parameters + defaults. but instead its
     
     EventBus.Instance.PublishForType<BaseEventType, ChildEvent>(new ChildEvent());
     
     You must pass through the base of the child for the script to work. If not, bad things. Very bad things will happen
     */
    #endregion

    #region How to Subscribe to a gameplay event
    /*
     Subscribing to a GameplayEvent is like normal, but the difference is you subscribe to GameplayEvent, not the specific Event
     This is used for listening to all events of a type. 
     
     EventBus.Instance.Subscribe<GameplayEvent>(this, FunctionForEvent);
     
     
     private void FunctionForEvent(GameplayEvent e)
     {
     
     }
     */
    #endregion
    public class GameplayEvent : IEvent
    {
      public EventIDs EventID;
      public Vector3 EventPosition;

    }

    public class BaseballBatHitEvent : GameplayEvent
    {
       
    }

    public class DollAlertEvent : GameplayEvent
    {
      
    }

    public class BruteFootstepsEvent : GameplayEvent
    {
       
    }

    public class BruteAlertEvent : GameplayEvent
    {
        
    }

    public class BruteAttackEvent : GameplayEvent
    {
        
    }

    public class BruteIdleBreathEvent : GameplayEvent
    {
        
    }

    public class BruteHurtIdleBreathEvent : GameplayEvent
    {
        
    }

    public class DollGiggleEvent : GameplayEvent
    {
        
    }

    public class DollFootstepsEvent : GameplayEvent
    {
       
    }

    public class BeetleFootstepsEvent : GameplayEvent
    {
       
    }

    public class BeetleSqueakEvent : GameplayEvent
    {
        
    }

    public class BeetleBugNoiseEvent : GameplayEvent
    {
      
    }

    public class PlayerFootstepsEvent : GameplayEvent
    {
       
    }

    public class PlayerHurtEvent : GameplayEvent
    {
        
    }

    public class PlayerLandingEvent : GameplayEvent
    {
        
    }

    public class FlashLightClickEvent : GameplayEvent
    {
        
    }

    public class MeleeSwingEvent : GameplayEvent
    {
        
    }

    public class BaseballHitEvent : GameplayEvent
    {
        
    }

    public class SledgeHammerHitEvent : GameplayEvent
    {
        
    }

    public class MacheteHitEvent : GameplayEvent
    {
        
    }

    public class TranqGunShotEvent : GameplayEvent
    {
        
    }

    public class TranqGunHitEvent : GameplayEvent
    {
        
    }

    public class TestTubeCollectEvent : GameplayEvent
    {
        
    }

    public class DoorSwingEvent : GameplayEvent
    {
       
    }

    public class FluorescentLightBuzzEvent : GameplayEvent
    {
       
    }

    public class AmbientMusicEvent : GameplayEvent
    {
       
    }

    public class TruckDoorSwingEvent : GameplayEvent
    {
       
    }

    public class DeliveryTruckHornEvent : GameplayEvent
    {
        
    }

    public class ItemCollectEvent : GameplayEvent
    {
        
    }

    public class BruteHeartBeatEvent : GameplayEvent
    {
     
    }

    #endregion
}