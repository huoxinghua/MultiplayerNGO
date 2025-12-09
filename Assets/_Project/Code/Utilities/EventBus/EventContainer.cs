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
    
    #endregion
}