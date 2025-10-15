using System;
using System.Collections;
using System.Collections.Generic;
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
        //  private readonly Queue<short[]> _pendingSamples = new Queue<short[]>();
        private readonly Dictionary<ulong, Queue<short[]>> _pendingSamples = new();
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

           // Debug.Log($"[VoiceNetworker] OnNetworkSpawn() called on {gameObject.name} for owner={OwnerClientId}, local={NetworkManager.Singleton.LocalClientId}");
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

                //  Debug.Log("[VoiceNetworker] Decoder & Emitter initialized for " + (IsOwner ? "Owner" : "Client"));
                //  Debug.Log($"[PVoiceNetwork] Emitter Type: {_voiceEmitter.GetType()} Format: {_voiceEmitter.GetFormat()}");
                 Debug.Log($"[VoiceNetwork] emitterRef name={_voiceEmitter?.name} id={_voiceEmitter?.GetInstanceID()} IsReady={_voiceEmitter?.IsReady} format={_voiceEmitter?.GetFormat()} owner={IsOwner}");
                
            }

            else

            {

                Debug.LogError("[VoiceNetworker] Missing VoiceEmitter reference");

            }


           /* if (IsOwner && !_playbackOwnVoice)

                _voiceEmitter.SetVolume(0f);*/

            StartCoroutine(WaitToInitEmitter());

        }
        private IEnumerator WaitToInitEmitter()
        {
            yield return null; 
            if (_voiceEmitter != null && !_voiceEmitter.IsReady)
            {
                _voiceEmitter.Init(VoiceConsts.OpusSampleRate, 1, VoiceFormat.PCM16Samples);
                Debug.Log($"[VoiceNetwork] ✅ Delayed Init for emitter ({_voiceEmitter.name})");
            }
        }

        [ServerRpc]
        public void SendEncodedVoiceServerRpc(byte[] encodedVoiceData, ServerRpcParams rpcParams = default)
        {
            // Debug.Log($"[PVoiceNetwork] ServerRpc reveive {encodedVoiceData.Length} byte");
            ulong senderId = rpcParams.Receive.SenderClientId;

            SendEncodedVoiceClientRpc(encodedVoiceData, senderId);
        }


      
        [ClientRpc]
        public void SendEncodedVoiceClientRpc(byte[] encodedVoiceData, ulong senderID)
        {
            if (senderID == NetworkManager.Singleton.LocalClientId)
                return;
            bool shouldPlay = false;

            if (IsOwner)
            {
    
                shouldPlay = _playbackOwnVoice;
            }
            else
            {
   
                shouldPlay = true;
            }
   
            if (!shouldPlay)
            {
                Debug.Log("this voice should not play，return");
                return;
            }


            Debug.Log($"[VoiceNetwork] @@@@@@@@@@￥￥￥￥￥emitterRef name={_voiceEmitter?.name} id={_voiceEmitter?.GetInstanceID()} IsReady={_voiceEmitter?.IsReady} format={_voiceEmitter?.GetFormat()} owner={IsOwner} +senderId={ senderID}");

            Debug.Log($"********[PVoiceNetwork] !IsOwner {!IsOwner} _playbackOwnVoice {_playbackOwnVoice}");
            Span<short> decodedVoiceSamples = _voiceDecoder.DecodeVoiceSamples(encodedVoiceData);
            Debug.Log($"[Net] decodedLen={decodedVoiceSamples.Length}");
      
            StudioVoiceEmitter emitterToPlay = null;


            NetworkObject senderObject = null;

            if (NetworkManager.Singleton.IsServer)
            {
              
                senderObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(senderID);
            }
            else
            {
                
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(senderID, out var client))
                {
                    senderObject = client.PlayerObject;
                }
            }
            if (senderObject != null)
            {
                emitterToPlay = senderObject.GetComponentInChildren<StudioVoiceEmitter>();
            }

       
            if (emitterToPlay == null)
            {
                Debug.LogWarning($"[VoiceNetwork] ❌ Could not find emitter for sender {senderID}, fallback to local emitter.");
                emitterToPlay = _voiceEmitter as StudioVoiceEmitter;
            }

       
            if (!emitterToPlay.IsReady)
            {
                if (!_pendingSamples.ContainsKey(senderID))
                    _pendingSamples[senderID] = new Queue<short[]>();

                _pendingSamples[senderID].Enqueue(decodedVoiceSamples.ToArray());
                Debug.LogWarning($"[VoiceNetwork] Emitter not ready yet for sender {senderID}, caching {decodedVoiceSamples.Length} samples...");

                if (!_waitingForReady && isActiveAndEnabled)
                    StartCoroutine(WaitUntilEmitterReadyAndPlay(emitterToPlay, senderID));

                return;
            }


            if (_pendingSamples.TryGetValue(senderID, out var queue))
            {
                while (queue.Count > 0)
                    emitterToPlay.EnqueueSamplesForPlayback(queue.Dequeue());
            }
            emitterToPlay.EnqueueSamplesForPlayback(decodedVoiceSamples);

            Debug.Log($"[VoiceNetwork] ✅ Played voice for sender {senderID} on emitter {emitterToPlay.name}");

            /*
                        Debug.Log("[VoiceNetwork] VoiceEmitter is ready, ？" + _voiceEmitter.IsReady);
                        if (!_voiceEmitter.IsReady)
                        {
                            Debug.Log("[VoiceNetwork] VoiceEmitter not ready yet, caching samples...");
                            _pendingSamples.Enqueue(decodedVoiceSamples.ToArray());
                            //  Debug.LogWarning("[VoiceNetwork]  samples..."+ _pendingSamples.Count);
                            if (!_waitingForReady && isActiveAndEnabled)
                            {
                                StartCoroutine(WaitUntilEmitterReadyAndPlay());
                            }
                            else if (!_waitingForReady)
                            {
                                Debug.LogWarning("[VoiceNetwork] Object inactive, skipping coroutine start.");
                            }

                        }
                        Debug.Log("[VoiceNetwork] VoiceEmitter is ready, ？" + _voiceEmitter.IsReady);
                        while (_pendingSamples.Count > 0)
                        {
                            _voiceEmitter.EnqueueSamplesForPlayback(_pendingSamples.Dequeue());
                        }

                        _voiceEmitter.EnqueueSamplesForPlayback(decodedVoiceSamples);*/



        }
        private bool _waitingForReady = false;

        private IEnumerator WaitUntilEmitterReadyAndPlay(StudioVoiceEmitter emitter, ulong senderID)
        {
            _waitingForReady = true;
            yield return new WaitUntil(() => _voiceEmitter != null && _voiceEmitter.IsReady);
            Debug.Log($"[VoiceNetwork] ✅ Emitter for sender {senderID} is now ready, flushing queued samples...");

            if (_pendingSamples.TryGetValue(senderID, out var queue))
            {
                while (queue.Count > 0)
                    emitter.EnqueueSamplesForPlayback(queue.Dequeue());
            }

            _waitingForReady = false;
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
        void Update()
        {
            if (!IsOwner) return;

            if (Input.GetKeyDown(KeyCode.V))
            {
                Debug.Log("[VoiceNetwork] 🎤 StartRecording by key press");
                StartRecording();
            }

            if (Input.GetKeyUp(KeyCode.V))
            {
                Debug.Log("[VoiceNetwork] 📴 StopRecording by key release");
                StopRecording();
            }
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
