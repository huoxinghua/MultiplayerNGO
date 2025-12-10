using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.Audio;
using UnityEngine;

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
        public SoundIDs EventSoundID;
        public Vector3 EventPosition;
        
        //maybe replace with another attribute? Events probably do not need this. The sounds should know their own volume?
        public float EventVolume;

        public GameplayEvent(SoundIDs itemID = SoundIDs.NoSound, Vector3 Position = default(Vector3), float Volume = 1.0f)
        {
            EventSoundID = itemID;
            EventPosition = Position;
            EventVolume = Volume;
        }
    }

    public class BaseballBatHitEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public BaseballBatHitEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public BaseballBatHitEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class DollAlertEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public DollAlertEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public DollAlertEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class BruteFootstepsEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public BruteFootstepsEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public BruteFootstepsEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class BruteAlertEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public BruteAlertEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public BruteAlertEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class BruteAttackEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public BruteAttackEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public BruteAttackEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class BruteIdleBreathEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public BruteIdleBreathEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public BruteIdleBreathEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class BruteHurtIdleBreathEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public BruteHurtIdleBreathEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public BruteHurtIdleBreathEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class DollGiggleEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public DollGiggleEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public DollGiggleEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class DollFootstepsEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public DollFootstepsEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public DollFootstepsEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class BeetleFootstepsEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public BeetleFootstepsEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public BeetleFootstepsEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class BeetleSqueakEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public BeetleSqueakEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public BeetleSqueakEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class BeetleBugNoiseEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public BeetleBugNoiseEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public BeetleBugNoiseEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class PlayerFootstepsEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public PlayerFootstepsEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public PlayerFootstepsEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class PlayerHurtEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public PlayerHurtEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public PlayerHurtEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class PlayerLandingEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public PlayerLandingEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public PlayerLandingEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class FlashLightClickEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public FlashLightClickEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public FlashLightClickEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class MeleeSwingEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public MeleeSwingEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public MeleeSwingEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class BaseballHitEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public BaseballHitEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public BaseballHitEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class SledgeHammerHitEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public SledgeHammerHitEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public SledgeHammerHitEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class MacheteHitEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public MacheteHitEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public MacheteHitEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class TranqGunShotEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public TranqGunShotEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public TranqGunShotEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class TranqGunHitEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public TranqGunHitEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public TranqGunHitEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class TestTubeCollectEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public TestTubeCollectEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public TestTubeCollectEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class DoorSwingEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public DoorSwingEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public DoorSwingEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class FluorescentLightBuzzEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public FluorescentLightBuzzEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public FluorescentLightBuzzEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class AmbientMusicEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public AmbientMusicEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public AmbientMusicEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class TruckDoorSwingEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public TruckDoorSwingEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public TruckDoorSwingEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class DeliveryTruckHornEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public DeliveryTruckHornEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public DeliveryTruckHornEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    public class ItemCollectEvent : GameplayEvent
    {
        //no arguments base with all defaults
        public ItemCollectEvent() : base()
        {
        }

        // The default values in the base class will fill in the blanks
        public ItemCollectEvent(SoundIDs itemID = SoundIDs.NoSound,
            Vector3 position = default(Vector3),
            float volume = 1.0f)
            : base(itemID, position, volume)
        {
        }
    }

    #endregion
}