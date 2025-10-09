using System;
using System.Collections;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Project.Network.ProximityChat;
using Debug = UnityEngine.Debug;
using System.Security.Cryptography;

namespace Project.Network.ProximityChat
{
    /// <summary>
    /// Plays 16-bit PCM voice audio through a user-defined Programmer Instrument Event in FMOD Studio.
    /// </summary>
    public class StudioVoiceEmitter : VoiceEmitter
    {
        [Header("FMOD Programmer Instrument Event Reference")]
        [SerializeField] protected EventReference _voiceEventReference;
        // Programmer instrument event
        protected EVENT_CALLBACK _voiceCallback;
        protected EventInstance _voiceEventInstance;
        protected Rigidbody _voiceRigidbody;
        /// <inheritdoc />
        public override void Init(uint sampleRate = 48000, int channelCount = 1, VoiceFormat inputFormat = VoiceFormat.PCM16Samples)
        {
            //base.Init(sampleRate, channelCount, inputFormat);
            base.Init(sampleRate, channelCount, VoiceFormat.PCM16Samples);//xh add
            // Wireup programmer instrument callback
            _voiceCallback = new EVENT_CALLBACK(VoiceEventCallback);
            // Create and initialize an instance of our FMOD voice event
            _voiceEventInstance = RuntimeManager.CreateInstance(_voiceEventReference);
            // _voiceEventInstance.setCallback(_voiceCallback);
            RESULT  res = _voiceEventInstance.setCallback(
     _voiceCallback,
     EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND | EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND);//xh add
            Debug.Log($"!!!!!![StudioVoiceEmitter] Callback registration result = {res}&&&&&&&&&&&");
            _voiceEventInstance.start();
            _voiceEventInstance.setPaused(true);
            // We're not going to be officially initialized until our event instance
            // is created, which takes a little while, so let's re-flag ourself as uninitialized
        
            StartCoroutine(WaitToGetChannel());
            // Attach it to this to get spatial audio
            StartCoroutine(AttachVoiceEventDelayed());
        }
       

        private IEnumerator AttachVoiceEventDelayed()
        {
       
            yield return new WaitForSeconds(0.5f);

            RuntimeManager.AttachInstanceToGameObject(_voiceEventInstance, gameObject);
            var attributes = RuntimeUtils.To3DAttributes(gameObject);

            FMOD.ATTRIBUTES_3D attr;
            _voiceEventInstance.get3DAttributes(out attr);
            _voiceEventInstance.set3DAttributes(attributes);
            _voiceEventInstance.setPaused(false);

          
            // _initialized = true;
            UnityEngine.Debug.Log($"[StudioVoiceEmitter] AttachInstanceToGameObject success → pos=({attr.position.x:F2}, {attr.position.y:F2}, {attr.position.z:F2})");

        }

        /// <inheritdoc />
        public override void SetVolume(float volume)
        {
            _voiceEventInstance.setVolume(volume);
        }

        protected override void SetPaused(bool isPaused)
        {
            _voiceEventInstance.setPaused(isPaused);
        }

        private IEnumerator WaitToGetChannel()
        {
            /*  // Wait until event is fully created (playback state == playing)
              while (true)
              {
                  yield return null;
                  _voiceEventInstance.getPlaybackState(out PLAYBACK_STATE playbackState);
                  if (playbackState == PLAYBACK_STATE.PLAYING)
                      Debug.Log($"[%%%%%%VoiceEmitter] Channel active={playbackState}");
                  break;
              }

              // Get the channel and initialize
              if (FMODUtilities.TryGetChannelForEvent(_voiceEventInstance, out Channel channel))
              {
                  _channel = channel;
                  _initialized = true;
              }
              else
              {
                  UnityEngine.Debug.LogError("Failed to find channel. Unable to initialize Studio voice emitter.");
              }*/
           
            while (true)
            {
                _voiceEventInstance.getPlaybackState(out PLAYBACK_STATE playbackState);
                if (playbackState == PLAYBACK_STATE.PLAYING)
                {
                    Debug.Log($"[VoiceEmitter] Playback state = {playbackState}");
                    break;
                }
                yield return null;
            }

            
            yield return new WaitForSeconds(0.1f);

           
            Channel channel;
            bool gotChannel = false;
            for (int i = 0; i < 10; i++) 
            {
                if (FMODUtilities.TryGetChannelForEvent(_voiceEventInstance, out channel))
                {
                    _channel = channel;
                    _initialized = true;
                    Debug.Log($"[VoiceEmitter] ✅ Got FMOD channel handle = {_channel.hasHandle()}");
                    gotChannel = true;
                    break;
                }

                Debug.LogWarning($"[VoiceEmitter] Channel not ready yet... retry {i + 1}/10");
                yield return new WaitForSeconds(0.05f);
            }

            if (!gotChannel)
            {
                Debug.LogError("[VoiceEmitter] ❌ Failed to find channel after retries. FMOD event may not have initialized properly.");
            }

        }
        public static short[] LastDecodedSamples;
        [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        static RESULT VoiceEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            if (type == EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND)
            {
                var parameter = (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));

               
                if (LastDecodedSamples == null || LastDecodedSamples.Length == 0)
                {
                    Debug.LogWarning("[StudioVoiceEmitter] ⚠️ No PCM samples available for programmer sound.");
                    return RESULT.OK;
                }

               
                var exInfo = new CREATESOUNDEXINFO();
                exInfo.cbsize = Marshal.SizeOf(exInfo);
                exInfo.numchannels = 1;
                exInfo.defaultfrequency = 48000;
                exInfo.format = SOUND_FORMAT.PCM16;
                exInfo.length = (uint)(LastDecodedSamples.Length * sizeof(short));

         
                RESULT result = RuntimeManager.CoreSystem.createSound(
                    (IntPtr)0,
                    MODE.OPENUSER | MODE.CREATESTREAM | MODE.LOOP_OFF,
                    ref exInfo,
                    out Sound localSound);

                if (result != RESULT.OK)
                {
                    Debug.LogError($"[StudioVoiceEmitter] ❌ createSound failed: {result}");
                    return result;
                }

            
                IntPtr ptr1, ptr2;
                uint len1, len2;
                localSound.@lock(0, exInfo.length, out ptr1, out ptr2, out len1, out len2);
                Marshal.Copy(LastDecodedSamples, 0, ptr1, LastDecodedSamples.Length);
                localSound.unlock(ptr1, ptr2, len1, len2);

               
                parameter.sound = localSound.handle;
                parameter.subsoundIndex = -1;
                Marshal.StructureToPtr(parameter, parameterPtr, false);

                Debug.Log($"[StudioVoiceEmitter] ✅ Programmer Sound Created for this emitter ({LastDecodedSamples.Length} samples)");
            }
            else if (type == EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND)
            {
                Debug.Log("[StudioVoiceEmitter] 💀 Programmer Sound destroyed");
            }

            return RESULT.OK;
        }


        /*        [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
                static RESULT VoiceEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
                {
                    switch (type)
                    {

                        // Pass the sound to the programmer instrument
                        case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                        {

                                var parameter = (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));
                            parameter.sound = _voiceSound.handle;
                                Debug.Log("[studioVoiceEmitter]  VoiceEventCallback CREATE_PROGRAMMER_SOUND"+ parameter.sound);
                                parameter.subsoundIndex = -1;
                            Marshal.StructureToPtr(parameter, parameterPtr, false);

                            break;
                        }
                    }
                    return RESULT.OK;
                }*/
        private void LateUpdate()
        {
            /* 
             //check the sound position is along with the player or not. it confirm yes
             if (_voiceEventInstance.isValid())
             {
                 _voiceEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
                 FMOD.ATTRIBUTES_3D attr;
                 _voiceEventInstance.get3DAttributes(out attr);
                 UnityEngine.Debug.Log($"[VoiceEmitter] Updating 3D pos = ({attr.position.x:F2}, {attr.position.y:F2}, {attr.position.z:F2})");
             }*/
            if (_voiceEventInstance.isValid())
                Set3DNow();
        }
        void Set3DNow()
        {
       
            ATTRIBUTES_3D attr = RuntimeUtils.To3DAttributes(transform);
            if (_voiceRigidbody) attr.velocity = RuntimeUtils.ToFMODVector(_voiceRigidbody.linearVelocity);
            _voiceEventInstance.set3DAttributes(attr);
        }

        private void OnDestroy()
        {
            if (_initialized)
            {
                _voiceSound.release();
                _voiceEventInstance.release();
            }
        }
    }
}
