using System;
using UnityEngine;

namespace Network.Scripts.PlayerController
{

    public class GroundCheck : MonoBehaviour
    {
        [SerializeField] private float distanceThreshold = .15f;
        public bool isGrounded = true;
        public event Action Grounded;
        const float OriginOffset = .001f;
        Vector3 RaycastOrigin => transform.position + Vector3.up * OriginOffset;
        float RaycastDistance => distanceThreshold + OriginOffset;

        void LateUpdate()
        {
            bool isGroundedNow = Physics.Raycast(RaycastOrigin, Vector3.down, distanceThreshold * 2);

            if (isGroundedNow && !isGrounded)
            {
                Grounded?.Invoke();
            }
            isGrounded = isGroundedNow;
            //OnDrawGizmosSelected();
        }
        void OnDrawGizmosSelected()
        {
            Debug.DrawLine(RaycastOrigin, RaycastOrigin + Vector3.down * RaycastDistance, isGrounded ? Color.white : Color.red);
        }
    }

}