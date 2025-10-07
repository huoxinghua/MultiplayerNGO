using System;
using Unity.Netcode;
using UnityEngine;

namespace Project.Network.ProximityChat
{
    /// <summary>
    /// Networks voice audio -- recording, encoding and sending it over the network if owner,
    /// otherwise receiving, decoding and playing it back as 3D spatial audio.
    /// </summary>
    public class VoiceNetworker : NetworkBehaviour
    {
        [Header("Recorder")]
        [SerializeField] private VoiceRecorder _voiceRecorder;
        [Header("Emitter")]
        [SerializeField] private VoiceEmitter _voiceEmitter;
        [Header("Debug")]
        [SerializeField] private bool _playbackOwnVoice;

        // Encode/decode
        private VoiceEncoder _voiceEncoder;
        private VoiceDecoder _voiceDecoder;

        //void Awake()
        //{
        //    if (IsOwner)
        //    {
        //        _voiceRecorder.Init();
        //        _voiceEncoder = new VoiceEncoder(_voiceRecorder.RecordedSamplesQueue);

        //        if (_playbackOwnVoice)
        //        {
        //            _voiceDecoder = new VoiceDecoder();
        //            _voiceEmitter.Init(VoiceConsts.OpusSampleRate);
        //        }
        //        else
        //        {
        //            _voiceEmitter.enabled = false;
        //        }
        //    }
        //    else
        //    {

        //        _voiceDecoder = new VoiceDecoder();
        //        _voiceEmitter.Init(VoiceConsts.OpusSampleRate);
        //    }
        //}
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)

            {

                _voiceRecorder.Init();

                _voiceEncoder = new VoiceEncoder(_voiceRecorder.RecordedSamplesQueue);

                Debug.Log("[VoiceNetworker] Owner Recorder & Encoder initialized");

            }


            if (_voiceEmitter != null)

            {

                _voiceDecoder = new VoiceDecoder();

                _voiceEmitter.Init(VoiceConsts.OpusSampleRate, 1, VoiceFormat.PCM16Samples);

                Debug.Log("[VoiceNetworker] Decoder & Emitter initialized for " + (IsOwner ? "Owner" : "Client"));
                Debug.Log($"[PVoiceNetwork] Emitter Type: {_voiceEmitter.GetType()} Format: {_voiceEmitter.GetFormat()}");

            }

            else

            {

                Debug.LogError("[VoiceNetworker] Missing VoiceEmitter reference");

            }


            if (IsOwner && !_playbackOwnVoice)

                _voiceEmitter.SetVolume(0f);

        }




        [ServerRpc]
        public void SendEncodedVoiceServerRpc(byte[] encodedVoiceData)
        {
           // Debug.Log($"[PVoiceNetwork] ServerRpc reveive {encodedVoiceData.Length} byte");
            SendEncodedVoiceClientRpc(encodedVoiceData);   
        }

        //[ClientRpc]
        //public void SendEncodedVoiceClientRpc(byte[] encodedVoiceData)
        //{
        //    Debug.Log($"[PVoiceNetwork] ClientRpc reveive {encodedVoiceData.Length} byte");

        //    if ((_voiceDecoder == null) || (_voiceEmitter == null))
        //    {
        //        Debug.LogWarning("[PVoiceNetwork] Decoder or Emitter not initialized yet");
        //        return;
        //    }

        //    if (!IsOwner || _playbackOwnVoice)
        //    {
        //        Span<short> decodedVoiceSamples = _voiceDecoder.DecodeVoiceSamples(encodedVoiceData);
        //        _voiceEmitter.EnqueueSamplesForPlayback(decodedVoiceSamples);
        //    }
        //}

        [ClientRpc]
        public void SendEncodedVoiceClientRpc(byte[] encodedVoiceData)
        {
            //Debug.Log($"[PVoiceNetwork] ClientRpc receive {encodedVoiceData.Length} byte");


            if (_voiceDecoder == null)
            {
                _voiceDecoder = new VoiceDecoder();
                Debug.LogWarning("[PVoiceNetwork] Decoder was null, initialized on the fly");
            }
            if (_voiceEmitter == null)
            {
                _voiceEmitter = GetComponent<VoiceEmitter>();
                if (_voiceEmitter != null)
                {
                    _voiceEmitter.Init(VoiceConsts.OpusSampleRate, 1, VoiceFormat.PCM16Samples);
                    Debug.LogWarning("[PVoiceNetwork] Emitter was null, initialized on the fly");
                }
                else
                {
                    Debug.Log($"[PVoiceNetwork] Using existing emitter { _voiceEmitter.GetInstanceID() } with format {_voiceEmitter.GetFormat()}");
                }
            }
            if (!IsOwner || _playbackOwnVoice)
            {
                Span<short> decodedVoiceSamples = _voiceDecoder.DecodeVoiceSamples(encodedVoiceData);
                if (_voiceEmitter is null || !_voiceEmitter.IsReady) 
                    return;

                _voiceEmitter.EnqueueSamplesForPlayback(decodedVoiceSamples);
            }
        }
        /// <summary>
        /// Starts recording and sending voice data over the network.
        /// </summary>
        public void StartRecording()
        {
            if (!IsOwner) return;
            _voiceRecorder.StartRecording();
        }
        
        /// <summary>
        /// Stops recording and sending voice data over the network.
        /// </summary>
        public void StopRecording()
        {
            if (!IsOwner) return;
            _voiceRecorder.StopRecording();
        }

        /// <summary>
        /// Sets the output volume of the voice emitter.
        /// </summary>
        /// <param name="volume">Volume from 0 to 1</param>
        public void SetOutputVolume(float volume)
        {
            if (IsOwner && !_playbackOwnVoice) return;
            _voiceEmitter.SetVolume(volume);
        }

        void LateUpdate()
        {
            if (!IsOwner) return;

            if (_voiceEncoder == null || _voiceRecorder == null)
            {
                Debug.LogWarning("[PVoiceNetwork] Encoder or Recorder not ready yet");
                return;
            }

            if (IsOwner)
            {
                // Encode as much queued voice as possible 
                while (_voiceEncoder.HasVoiceLeftToEncode)
                {
                    Span<byte> encodedVoice = _voiceEncoder.GetEncodedVoice();
                    SendEncodedVoiceServerRpc(encodedVoice.ToArray());
                }
                // If we've stopped recording but there's still more left to be cleared,
                // force encode it with silence
                if (!_voiceRecorder.IsRecording && !_voiceEncoder.QueueIsEmpty)
                {
                    Span<byte> encodedVoice = _voiceEncoder.GetEncodedVoice(true);
                    SendEncodedVoiceServerRpc(encodedVoice.ToArray());
                }
            }
        }
    }
}
