using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Utilities.Audio
{
    [CreateAssetMenu(fileName = "AudioListSO", menuName = "Audio/AudioListSO")]
    [Serializable]
    public class AudioListSO : ScriptableObject
    {
            [field:SerializeField] public List<AudioPlayItem> Items { get; set; }
            public AudioPlayItem GetItem(EventIDs key) => Items.Find((x) => x.Key == key);
        
    }
}
