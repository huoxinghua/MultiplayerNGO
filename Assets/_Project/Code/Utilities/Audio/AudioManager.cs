using System;
using System.Collections.Generic;
using _Project.Code.Gameplay.Player.MiscPlayer;
using UnityEngine;

namespace _Project.Code.Utilities.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Audio Library")]
        [SerializeField] private List<AudioEntry> audioLibrary = new List<AudioEntry>();
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private Dictionary<SoundIDs, AudioClip> _audioClipsEnum = new Dictionary<SoundIDs, AudioClip>();

        [Header("Pooling Settings")]
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private AudioSource prefabSource; // A prefab with an AudioSource component

        private Queue<AudioSource> audioPool = new Queue<AudioSource>();
        private List<AudioSource> activeSources = new List<AudioSource>();

        [Header("Ambient Audio")]
        [SerializeField] private AudioSource ambientSource; // Looping music/environment
    

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            foreach (var entry in audioLibrary)
            {
                if (!audioClips.ContainsKey(entry.key) && entry.clip != null)
                    audioClips.Add(entry.key, entry.clip);
            }
            CreatePool();
        }

        private void CreatePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
                AddNewSource();
        }

        private void AddNewSource()
        {
            AudioSource src = Instantiate(prefabSource, transform);
            src.playOnAwake = false;
            src.spatialBlend = 1f; // fully 3D by default
            src.gameObject.SetActive(false);
            audioPool.Enqueue(src);
        }

        private AudioSource GetSource()
        {
            if (audioPool.Count == 0) AddNewSource();
            var src = audioPool.Dequeue();
            src.gameObject.SetActive(true);
            activeSources.Add(src);
            return src;
        }

        private void ReleaseSource(AudioSource src)
        {
            src.Stop();
            src.clip = null;
            src.transform.SetParent(transform);
            src.gameObject.SetActive(false);
            activeSources.Remove(src);
            audioPool.Enqueue(src);
        }

        private void Update()
        {
            // Recycle finished sounds
            for (int i = activeSources.Count - 1; i >= 0; i--)
            {
                if (!activeSources[i].isPlaying)
                    ReleaseSource(activeSources[i]);
            }
        }

        // ----------------- Public API -----------------

        /// <summary>Plays a 2D sound (UI, effects).</summary>
        public void PlayByKey3D(string key, Vector3 position, float volume = 1f)
        {
            if (audioClips.TryGetValue(key, out AudioClip clip))
                Play3D(clip, position, volume);
            else
                Debug.LogWarning($"Audio key not found: {key}");
        }

        public void PlayByKey2D(string key, float volume = 1f)
        {
            if (audioClips.TryGetValue(key, out AudioClip clip))
                Play2D(clip, volume);
            else
                Debug.LogWarning($"Audio key not found: {key}");
        }
        public void PlayByKeyAttached(string key, Transform attachedTransform, float volume = 1f)
        {
            if (audioClips.TryGetValue(key, out AudioClip clip))
                PlayAttached(clip, attachedTransform, volume);
            else
                Debug.LogWarning($"Audio key not found: {key}");
        }

        public void Play2D(AudioClip clip, float volume = 1f)
        {
            AudioSource src = GetSource();
            src.spatialBlend = 0f; // 2D
            src.volume = volume;
            src.transform.position = Vector3.zero;
            src.PlayOneShot(clip);
        }

        /// <summary>Plays a 3D sound at a world position.</summary>
        public void Play3D(AudioClip clip, Vector3 position, float volume = 1f)
        {
            AudioSource src = GetSource();
            src.spatialBlend = 1f;
            src.volume = volume;
            src.transform.position = position;
            src.PlayOneShot(clip);
        }

        /// <summary>Plays a sound attached to a specific GameObject (e.g., a player).</summary>
        public void PlayAttached(AudioClip clip, Transform target, float volume = 1f)
        {
            AudioSource src = GetSource();
            src.spatialBlend = 1f;
            src.volume = volume;
            src.transform.SetParent(target);
            src.transform.localPosition = Vector3.zero;
            src.PlayOneShot(clip);
        }

        /// <summary>Plays looping ambient or music audio.</summary>
/*    public void PlayAmbient(AudioClip clip, float volume = 1f)
    {
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.spatialBlend = 0f; // Ambient is 2D by default
        }
        ambientSource.clip = clip;
        ambientSource.volume = volume;
        ambientSource.Play();
    }*/

        public void PlayAmbient(string key, float volume = 1f)
        {
            if (audioClips.TryGetValue(key, out AudioClip clip))
            {
                if (ambientSource == null)
                {
                    ambientSource = gameObject.AddComponent<AudioSource>();
                    ambientSource.loop = true;
                    ambientSource.spatialBlend = 0f;
                }
                ambientSource.clip = clip;
                ambientSource.volume = volume;
                ambientSource.Play();
            }
            else
            {
                Debug.LogWarning($"Ambient key not found: {key}");
            }
        }

        public void StopAmbient() => ambientSource?.Stop();
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class NetworkAspectAttribute : Attribute
    {
        public enum SoundAspect
        {
            LocalOnly = 0,
            Global = 1,
            Conditional = 2,
            Local2D = 3,
            Global2D = 4,
            Default,
        }

        public SoundAspect NetworkAspect { get; }

        public NetworkAspectAttribute(SoundAspect aspect)
        {
            NetworkAspect = aspect;
        }
    }
    public enum SoundIDs
    {
        NoSound,
        
        #region  EnemySounds
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnemyBruteFootsteps,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnemyBruteAlert,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnemyBruteAttack,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnemyBruteIdleBreath,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnemyBruteHurtIdleBreath,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnemyDollGiggle,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnemyDollFootsteps,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.LocalOnly)]
        EnemyDollAlert,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnemyBeetleFootsteps,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnemyBeetleSqueak,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnemyBeetleBugNoise,
        
        #endregion
        #region  PlayerSounds
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        PlayerFootsteps,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        PlayerHurt,
        
        #endregion
        #region  ItemSounds
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        ItemFlashlightClick,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        ItemMeleeSwing,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        ItemBaseballBatHit,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        ItemSledgehammerSwing,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        ItemMacheteSwing,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        ItemTranqGunShot,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        ItemTranqGunHit,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.LocalOnly)]
        ItemTestTubeCollect,
        
        #endregion
        #region  EnviromentSounds
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnvironmentDoorSwing,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnvironmentFluorescentLightBuzz,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global2D)]
        EnvironmentAmbientMusic,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnvironmentTruckDoorSwing,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.Global)]
        EnvironmentDeliveryTruckHorn,
        
        [NetworkAspect(NetworkAspectAttribute.SoundAspect.LocalOnly)]
        EnvironmentItemCollect
        
        #endregion
    }

    public struct PlaySpatialSoundEvent : IEvent
    {
        public SoundIDs SoundID;
        public Vector3 Position;
        public float Volume;
    }

    public struct Play2DSoundEvent : IEvent
    {
        public SoundIDs SoundID;
        public float Volume;
    }
}
