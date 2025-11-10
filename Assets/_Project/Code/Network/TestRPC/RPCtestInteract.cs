using UnityEngine;

namespace _Project.Code.Network.TestRPC
{
    public class RPCtestInteract : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            TestObj obj = other.GetComponent<TestObj>();
            if (obj != null)
            {

                obj.ChangeColorServerRpc();
            }
        }
    }
}

