using ProximityChat;
using Unity.Netcode;
using UnityEngine;
namespace Project.Network.ProxiimityChat
{
    public class PVoiceNetwork : VoiceNetworker
    {
        [ServerRpc(Delivery = RpcDelivery.Unreliable)]
        public new void SendEncodedVoiceServerRpc(byte[] encodedVoiceData)
        {
            Debug.Log($"[PVoiceNetwork] ServerRpc got {encodedVoiceData.Length} bytes from {OwnerClientId}");
            base.SendEncodedVoiceServerRpc(encodedVoiceData); 
        }

        [ClientRpc(Delivery = RpcDelivery.Unreliable)]
        public new void SendEncodedVoiceClientRpc(byte[] encodedVoiceData)
        {
            Debug.Log($"[PVoiceNetwork] ClientRpc received {encodedVoiceData.Length} bytes");
            base.SendEncodedVoiceClientRpc(encodedVoiceData); 
        }
    }
}
