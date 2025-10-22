using UnityEngine;

namespace _Project.Code.Utilities.Audio
{
    [System.Serializable]
    public class AudioEntry
    {
        public string key;      // e.g. "Explosion", "Footstep"
        public AudioClip clip;
    }
}
