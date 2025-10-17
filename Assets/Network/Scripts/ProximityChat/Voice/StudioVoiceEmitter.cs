using FMOD;
using FMOD.Studio;
using FMODUnity;
using ProximityChat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Project.Network.ProximityChat
{


    public class StudioVoiceEmitter : VoiceEmitter
    {
        [Header("FMOD Programmer Instrument Event Reference")]
        [SerializeField] protected EventReference _voiceEventReference;
        // Programmer instrument event
        protected EVENT_CALLBACK _voiceCallback;
        protected EventInstance _voiceEventInstance;

        private static readonly Dictionary<IntPtr, StudioVoiceEmitter> _instanceMap = new();
        /// <inheritdoc />
        public override void Init(uint sampleRate = 48000, int channelCount = 1, VoiceFormat inputFormat = VoiceFormat.PCM16Samples)
        {
            ReleaseVoiceEvent();

            base.Init(sampleRate, channelCount, inputFormat);
            FMOD.System system = RuntimeManager.CoreSystem;
            CREATESOUNDEXINFO soundInfo = new CREATESOUNDEXINFO();
            soundInfo.cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO));
            soundInfo.numchannels = channelCount;
            soundInfo.defaultfrequency = (int)sampleRate;
            soundInfo.format = SOUND_FORMAT.PCM16;
            soundInfo.length = sampleRate * VoiceConsts.SampleSize * (uint)channelCount;

            system.createSound(
                (string)null,
                MODE.OPENUSER | MODE.LOOP_NORMAL | MODE._3D,
                ref soundInfo,
                out _voiceSound
            );

            Debug.Log($"[StudioVoiceEmitter] ✅ Created _voiceSound for {gameObject.name}");

            // Wireup programmer instrument callback
            _voiceCallback = new EVENT_CALLBACK(VoiceEventCallback);
            // Create and initialize an instance of our FMOD voice event
            _voiceEventInstance = RuntimeManager.CreateInstance(_voiceEventReference);
            _voiceEventInstance.setCallback(_voiceCallback);
            _voiceEventInstance.start();
            _voiceEventInstance.setPaused(true);
            _instanceMap[_voiceEventInstance.handle] = this;
            // We're not going to be officially initialized until our event instance
            // is created, which takes a little while, so let's re-flag ourself as uninitialized
            _initialized = false;
            StartCoroutine(WaitToGetChannel());
            // Attach it to this to get spatial audio
            RuntimeManager.AttachInstanceToGameObject(_voiceEventInstance, gameObject);
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

        public override void EnqueueSamplesForPlayback(Span<short> voiceSamples)
        {
            Debug.Log($"EnqueueSamplesForPlayback,Samples={voiceSamples.Length},valid={_voiceEventInstance.isValid()}playing={_voiceEventInstance.getPlaybackState(out PLAYBACK_STATE playbackState)}");
            base.EnqueueSamplesForPlayback(voiceSamples);
        }

        private IEnumerator WaitToGetChannel()
        {
            // Wait until event is fully created (playback state == playing)
            while (true)
            {
                yield return null;
                _voiceEventInstance.getPlaybackState(out PLAYBACK_STATE playbackState);
                if (playbackState == PLAYBACK_STATE.PLAYING)
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
            }
        }

        [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        static RESULT VoiceEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            switch (type)
            {
                // Pass the sound to the programmer instrument
                case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                    {
                        var parameter = (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));
                        if (_instanceMap.TryGetValue(instancePtr, out var emitter))
                        {
                            parameter.sound = emitter._voiceSound.handle;
                        }
                        else
                        {
                            Debug.LogWarning("[FMOD] ⚠️ Missing emitter for programmer sound instance");
                        }

                        parameter.subsoundIndex = -1;
                        Marshal.StructureToPtr(parameter, parameterPtr, false);
                        break;
                    }

            }
            return RESULT.OK;
        }
        public void ReleaseVoiceEvent()
        {
            if (_voiceEventInstance.hasHandle())
            {
                _voiceEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _voiceEventInstance.release();
                _voiceEventInstance.clearHandle();
            }

            if (_voiceSound.hasHandle())
            {
                _voiceSound.release();
            }

            if (_instanceMap.ContainsKey(_voiceEventInstance.handle))
                _instanceMap.Remove(_voiceEventInstance.handle);

            _initialized = false;
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
