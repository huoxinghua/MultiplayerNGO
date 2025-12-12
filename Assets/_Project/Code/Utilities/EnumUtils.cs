using System;
using System.Reflection;
using _Project.Code.Utilities.Audio;

namespace _Project.Code.Utilities
{
    public static class EnumUtils
    {
    //     public static NetworkAspectAttribute GetNetworkAspectAttribute(this SoundIDs soundId)
    //     {
    //         Type type = typeof(SoundIDs);
    //         
    //         FieldInfo fieldInfo = type.GetField(soundId.ToString());
    //
    //         if (fieldInfo == null)
    //         {
    //             // This should only happen for NoSound if the attribute is omitted
    //             return null; 
    //         }
    //         object[] attributes = fieldInfo.GetCustomAttributes(typeof(NetworkAspectAttribute), false);
    //         
    //         if (attributes.Length > 0)
    //         {
    //             return (NetworkAspectAttribute)attributes[0];
    //         }
    //
    //         return null;
    //     }
    //     
    //     public static NetworkAspectAttribute.SoundAspect GetNetworkSoundAspect(this SoundIDs soundId)
    //     {
    //         Type type = typeof(SoundIDs);
    //     
    //         // Use the original reflection logic to get the attribute
    //         FieldInfo fieldInfo = type.GetField(soundId.ToString());
    //
    //         if (fieldInfo == null)
    //         {
    //             // If the field doesn't exist (e.g., in a complex edge case)
    //             // Return a safe default value.
    //             return NetworkAspectAttribute.SoundAspect.Default; 
    //         }
    //     
    //         object[] attributes = fieldInfo.GetCustomAttributes(typeof(NetworkAspectAttribute), false);
    //     
    //         if (attributes.Length > 0)
    //         {
    //             // 1. Cast the attribute object
    //             NetworkAspectAttribute attribute = (NetworkAspectAttribute)attributes[0];
    //         
    //             // 2. Return the NetworkAspect property (which is the SoundAspect enum)
    //             return attribute.NetworkAspect;
    //         }
    //
    //         // If the attribute is missing (like on SoundIDs.NoSound), 
    //         // return a sensible default, typically LocalOnly or Global,
    //         // depending on your game's networking philosophy for un-attributed sounds.
    //         return NetworkAspectAttribute.SoundAspect.Default;
    //     }
     }
}