using Unity.Netcode;
using UnityEngine;

namespace Network.Scripts.TestRPC
{


    public class TestObj : NetworkBehaviour
    {
        private Renderer render;
        private void Awake()
        {
            render = GetComponent<Renderer>();
        }
        [ServerRpc]
        public void ChangeColorServerRpc()
        {
            Debug.Log("change obj color");
            Color color = Color.red;//the client must get the color from the server
            ApplyColor(color);
            ChangeColorClientRpc(color);
        }

        [ClientRpc]
        private void ChangeColorClientRpc(Color newColor)
        {
            Debug.Log("change client obj color");
            ApplyColor(newColor);
        }
        private void ApplyColor(Color newColor)
        {
            if (render != null)
            {
                render.material.color = newColor;
            }
        }
    }
}