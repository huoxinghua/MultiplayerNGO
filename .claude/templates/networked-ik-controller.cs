using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Template for a networked IK controller with synchronized hand positions.
/// Replace "PlayerIKController" with your actual class name if different.
/// </summary>
[RequireComponent(typeof(Animator))]
public class NetworkedIKController : NetworkBehaviour
{
    // ============================================================
    // NETWORKED IK POSITIONS
    // ============================================================

    /// <summary>
    /// Right hand position (synced across network)
    /// </summary>
    private NetworkVariable<Vector3> netHandRPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    /// <summary>
    /// Right hand rotation (synced across network)
    /// </summary>
    private NetworkVariable<Quaternion> netHandRRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    /// <summary>
    /// Left hand position (synced across network)
    /// </summary>
    private NetworkVariable<Vector3> netHandLPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    /// <summary>
    /// Left hand rotation (synced across network)
    /// </summary>
    private NetworkVariable<Quaternion> netHandLRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // ============================================================
    // CONFIGURATION
    // ============================================================

    [Header("IK Settings")]
    [SerializeField] private bool ikActive = true;
    [SerializeField] private bool applyFingers = false;

    [Header("Performance")]
    [SerializeField] private float syncInterval = 0.033f;  // 30Hz default

    // ============================================================
    // REFERENCES
    // ============================================================

    private Animator animator;
    private Transform handL;
    private Transform handR;

    // ============================================================
    // STATE
    // ============================================================

    private float syncTimer = 0f;
    private bool isInitialized = false;

    // ============================================================
    // PROPERTIES
    // ============================================================

    public bool IkActive
    {
        get => ikActive;
        set => ikActive = value;
    }

    // ============================================================
    // LIFECYCLE
    // ============================================================

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError($"[{GetType().Name}] No Animator component found!");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"[{GetType().Name}] Network spawned. IsOwner={IsOwner}");
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    /// <summary>
    /// Set the IK target transforms for hands.
    /// Call this when equipping an item.
    /// </summary>
    public void SetIKTargets(Transform leftHand, Transform rightHand)
    {
        handL = leftHand;
        handR = rightHand;

        isInitialized = (handL != null || handR != null);

        Debug.Log($"[{GetType().Name}] IK targets set. Left={handL != null}, Right={handR != null}");
    }

    /// <summary>
    /// Clear IK targets (e.g., when dropping item).
    /// </summary>
    public void ClearIKTargets()
    {
        handL = null;
        handR = null;
        isInitialized = false;

        Debug.Log($"[{GetType().Name}] IK targets cleared");
    }

    // ============================================================
    // NETWORK SYNC
    // ============================================================

    /// <summary>
    /// Sync IK positions to network (owner only).
    /// Runs at configurable interval to reduce bandwidth.
    /// </summary>
    private void Update()
    {
        if (!IsOwner) return;
        if (!isInitialized) return;

        // Throttle updates to reduce bandwidth
        syncTimer += Time.deltaTime;
        if (syncTimer < syncInterval) return;

        syncTimer = 0f;

        // Sync right hand
        if (handR != null)
        {
            netHandRPosition.Value = handR.position;
            netHandRRotation.Value = handR.rotation;
        }

        // Sync left hand
        if (handL != null)
        {
            netHandLPosition.Value = handL.position;
            netHandLRotation.Value = handL.rotation;
        }
    }

    // ============================================================
    // IK APPLICATION
    // ============================================================

    /// <summary>
    /// Unity's IK callback. Runs every frame during animation.
    /// Owner uses local transforms, non-owners use synced network values.
    /// </summary>
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;
        if (!ikActive) return;

        // Wait for network data if not owner
        if (!IsOwner && !IsNetworkDataReady())
        {
            // Network values not synced yet, skip this frame
            return;
        }

        // Apply right hand IK
        ApplyHandIK(
            AvatarIKGoal.RightHand,
            handR,
            netHandRPosition.Value,
            netHandRRotation.Value
        );

        // Apply left hand IK
        ApplyHandIK(
            AvatarIKGoal.LeftHand,
            handL,
            netHandLPosition.Value,
            netHandLRotation.Value
        );
    }

    /// <summary>
    /// Apply IK for a specific hand.
    /// </summary>
    private void ApplyHandIK(
        AvatarIKGoal goal,
        Transform localTransform,
        Vector3 networkPosition,
        Quaternion networkRotation)
    {
        if (IsOwner && localTransform != null)
        {
            // Owner: Use local transforms for zero-latency
            animator.SetIKPositionWeight(goal, 1f);
            animator.SetIKRotationWeight(goal, 1f);
            animator.SetIKPosition(goal, localTransform.position);
            animator.SetIKRotation(goal, localTransform.rotation);

            // Optional: Apply finger rotations (owner only)
            if (applyFingers)
            {
                ApplyFingerRotations(localTransform, goal);
            }
        }
        else if (!IsOwner)
        {
            // Non-owner: Use synced network values
            animator.SetIKPositionWeight(goal, 1f);
            animator.SetIKRotationWeight(goal, 1f);
            animator.SetIKPosition(goal, networkPosition);
            animator.SetIKRotation(goal, networkRotation);

            // Note: Fingers not synced for non-owners (too expensive)
        }
        else
        {
            // No valid data, disable IK for this goal
            animator.SetIKPositionWeight(goal, 0f);
            animator.SetIKRotationWeight(goal, 0f);
        }
    }

    /// <summary>
    /// Apply finger rotations (owner only, optional).
    /// Override this with your finger rotation logic if needed.
    /// </summary>
    private void ApplyFingerRotations(Transform handTransform, AvatarIKGoal goal)
    {
        // Example: Apply index finger rotation
        // HumanBodyBones fingerBone = (goal == AvatarIKGoal.RightHand)
        //     ? HumanBodyBones.RightIndexProximal
        //     : HumanBodyBones.LeftIndexProximal;
        //
        // Transform fingerTransform = animator.GetBoneTransform(fingerBone);
        // if (fingerTransform != null)
        // {
        //     fingerTransform.rotation = someRotation;
        // }

        // TODO: Implement your finger rotation logic here
    }

    /// <summary>
    /// Check if network data is ready for use.
    /// </summary>
    private bool IsNetworkDataReady()
    {
        // Check if at least one hand has valid network data
        bool rightHandReady = netHandRPosition.Value != Vector3.zero;
        bool leftHandReady = netHandLPosition.Value != Vector3.zero;

        return rightHandReady || leftHandReady;
    }

    // ============================================================
    // DEBUG
    // ============================================================

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!isInitialized) return;

        // Draw right hand target
        if (handR != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(handR.position, 0.05f);
            Gizmos.DrawLine(handR.position, handR.position + handR.forward * 0.1f);
        }

        // Draw left hand target
        if (handL != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(handL.position, 0.05f);
            Gizmos.DrawLine(handL.position, handL.position + handL.forward * 0.1f);
        }

        // Draw network synced positions (non-owners)
        if (!IsOwner && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(netHandRPosition.Value, 0.03f);
            Gizmos.DrawWireSphere(netHandLPosition.Value, 0.03f);
        }
    }

    [ContextMenu("Toggle IK Active")]
    private void ToggleIKActive()
    {
        ikActive = !ikActive;
        Debug.Log($"IK Active: {ikActive}");
    }

    [ContextMenu("Print IK Status")]
    private void PrintIKStatus()
    {
        Debug.Log($"=== IK Status ===");
        Debug.Log($"IsOwner: {IsOwner}");
        Debug.Log($"IK Active: {ikActive}");
        Debug.Log($"Initialized: {isInitialized}");
        Debug.Log($"Hand L: {handL != null}");
        Debug.Log($"Hand R: {handR != null}");
        Debug.Log($"Network Ready: {IsNetworkDataReady()}");
        Debug.Log($"Sync Interval: {syncInterval}s ({1f / syncInterval:F0}Hz)");
    }
#endif
}
