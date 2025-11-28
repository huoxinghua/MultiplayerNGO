using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.ProximityChat.Voice
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

        private readonly Dictionary<ulong, Queue<short[]>> _pendingSamples = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner)
            {
                _voiceRecorder.Init();
                _voiceEncoder = new VoiceEncoder(_voiceRecorder.RecordedSamplesQueue);
            }

            if (_voiceEmitter != null)
            {
                _voiceDecoder = new VoiceDecoder();
                _voiceEmitter.Init(VoiceConsts.OpusSampleRate, 1, VoiceFormat.PCM16Samples);
            }
            else
            {
                Debug.LogError("[VoiceNetworker] Missing VoiceEmitter reference");
            }


            /* if (IsOwner && !_playbackOwnVoice)

                 _voiceEmitter.SetVolume(0f);*/

            StartCoroutine(WaitToInitEmitter());
          
           StartRecording();
            
        }
       public override void OnNetworkDespawn()
        {
            StopAllCoroutines();
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
            ulong senderId = rpcParams.Receive.SenderClientId;
            SendEncodedVoiceClientRpc(encodedVoiceData, senderId);
        }

        [ClientRpc]
        public void SendEncodedVoiceClientRpc(byte[] encodedVoiceData, ulong senderID)
        {
            if (senderID == NetworkManager.Singleton.LocalClientId)
            {
                return;
            }

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
                return;
            }

           // Debug.Log($"[VoiceNetwork] @@@@@@@@@@￥￥￥￥￥emitterRef name={_voiceEmitter?.name} id={_voiceEmitter?.GetInstanceID()} IsReady={_voiceEmitter?.IsReady} format={_voiceEmitter?.GetFormat()} owner={IsOwner} +senderId={senderID}");

            Span<short> decodedVoiceSamples = _voiceDecoder.DecodeVoiceSamples(encodedVoiceData);
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
                emitterToPlay = _voiceEmitter as StudioVoiceEmitter;
            }

            if (!emitterToPlay.IsReady)
            {
                if (!_pendingSamples.ContainsKey(senderID))
                    _pendingSamples[senderID] = new Queue<short[]>();

                _pendingSamples[senderID].Enqueue(decodedVoiceSamples.ToArray());
               // Debug.LogWarning($"[VoiceNetwork] Emitter not ready yet for sender {senderID}, caching {decodedVoiceSamples.Length} samples...");

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
      /*  void Update()
        {
            if (!IsOwner) return;

            if (Input.GetKeyDown(KeyCode.V))
            {
              //  Debug.Log("[VoiceNetwork] 🎤 StartRecording by key press");
                StartRecording();
            }

            if (Input.GetKeyUp(KeyCode.V))
            {
               // Debug.Log("[VoiceNetwork] 📴 StopRecording by key release");
                StopRecording();
            }
        }*/
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
