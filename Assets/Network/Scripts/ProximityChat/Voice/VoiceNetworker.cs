using System;
using System.Collections.Generic;
using System.Text;
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
                Debug.Log($"[VoiceNetwork] emitterRef name={_voiceEmitter?.name} id={_voiceEmitter?.GetInstanceID()} IsReady={_voiceEmitter?.IsReady} format={_voiceEmitter?.GetFormat()} owner={IsOwner}");

            }

            else

            {

                Debug.LogError("[VoiceNetworker] Missing VoiceEmitter reference");

            }


            if (IsOwner && !_playbackOwnVoice)

                _voiceEmitter.SetVolume(0f);

        }




        [ServerRpc]
        public void SendEncodedVoiceServerRpc(byte[] encodedVoiceData,ServerRpcParams rpcParams = default)
        {
            // Debug.Log($"[PVoiceNetwork] ServerRpc reveive {encodedVoiceData.Length} byte");
            ulong senderId = rpcParams.Receive.SenderClientId;
            SendEncodedVoiceClientRpc(encodedVoiceData, senderId);   
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
        private readonly Queue<short[]> _pendingSamples = new Queue<short[]>();
        [ClientRpc]
        public void SendEncodedVoiceClientRpc(byte[] encodedVoiceData,ulong senderID)
        {
            Debug.Log("SendEncodedVoiceClientRpc" + OwnerClientId + senderID);
            if (OwnerClientId != senderID)
            {
                Debug.Log("OwnerClientId != senderID");
                return;
            }
            else
            {
                Debug.Log("OwnerClientId = senderID");
            }

            //Debug.Log($"[PVoiceNetwork] ClientRpc receive {encodedVoiceData.Length} byte");
            Debug.Log($"[VoiceNetwork] emitterRef name={_voiceEmitter?.name} id={_voiceEmitter?.GetInstanceID()} IsReady={_voiceEmitter?.IsReady} format={_voiceEmitter?.GetFormat()} owner={IsOwner}");

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
            Debug.Log($"********[PVoiceNetwork] !IsOwner {!IsOwner} _playbackOwnVoice {_playbackOwnVoice}");
        
             if (!IsOwner|| _playbackOwnVoice)
            {
                Span<short> decodedVoiceSamples = _voiceDecoder.DecodeVoiceSamples(encodedVoiceData);
                Debug.Log($"[Net] decodedLen={decodedVoiceSamples.Length}");

               
                if (_voiceEmitter == null)
                {
                    Debug.Log("[VoiceNetwork] VoiceEmitter is null, caching samples...");
                    _pendingSamples.Enqueue(decodedVoiceSamples.ToArray());
                   // Debug.LogWarning("[VoiceNetwork]  samples..." + _pendingSamples.Count);
                    return;
                }
                Debug.Log("[VoiceNetwork] VoiceEmitter is ready, ？" + _voiceEmitter.IsReady );
                if (!_voiceEmitter.IsReady)
                {
                    Debug.Log("[VoiceNetwork] VoiceEmitter not ready yet, caching samples...");
                    _pendingSamples.Enqueue(decodedVoiceSamples.ToArray());
                  //  Debug.LogWarning("[VoiceNetwork]  samples..."+ _pendingSamples.Count);
                    return;
                }
                Debug.Log("[VoiceNetwork] VoiceEmitter is ready, ？" + _voiceEmitter.IsReady);
                while (_pendingSamples.Count > 0)
                {
                    _voiceEmitter.EnqueueSamplesForPlayback(_pendingSamples.Dequeue());
                }

                _voiceEmitter.EnqueueSamplesForPlayback(decodedVoiceSamples);

            }

            // if (IsOwner|| _playbackOwnVoice)
           // if (true)
          /*  {
                Span<short> decodedVoiceSamples = _voiceDecoder.DecodeVoiceSamples(encodedVoiceData);
               *//* if (_voiceEmitter is null || !_voiceEmitter.IsReady)
                    return;*//*
                Debug.Log($"[Net] decodedLen={decodedVoiceSamples.Length}");

                _voiceEmitter.EnqueueSamplesForPlayback(decodedVoiceSamples);
            }*/

            

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
