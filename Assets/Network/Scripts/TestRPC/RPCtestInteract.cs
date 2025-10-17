using UnityEngine;
namespace Project.Network.TestRPC
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

