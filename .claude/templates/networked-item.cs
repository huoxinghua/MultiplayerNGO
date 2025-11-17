using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Template for a networked item that can be picked up and used.
/// Replace "TemplateItem" with your item name (e.g., BaseballBat, Flashlight).
/// </summary>
public class TemplateItem : NetworkBehaviour
{
    // ============================================================
    // CONFIGURATION
    // ============================================================

    [Header("Item Properties")]
    [SerializeField] private string itemName = "Template Item";
    [SerializeField] private float maxPickupDistance = 3f;
    [SerializeField] private float useCooldown = 1f;
    [SerializeField] private float maxDurability = 100f;

    [Header("Visuals")]
    [SerializeField] private GameObject worldVisual;
    [SerializeField] private GameObject heldVisualPrefab;
    [SerializeField] private Vector3 heldLocalPosition;
    [SerializeField] private Vector3 heldLocalRotation;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip useSound;

    // ============================================================
    // NETWORKED STATE
    // ============================================================

    /// <summary>
    /// Whether this item has been picked up.
    /// </summary>
    private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>
    /// Client ID of the owner (999999 = no owner).
    /// </summary>
    private NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(
        999999,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>
    /// Current durability.
    /// </summary>
    private NetworkVariable<float> durability = new NetworkVariable<float>(
        0f,  // Set in OnNetworkSpawn
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ============================================================
    // LOCAL STATE
    // ============================================================

    private Transform heldVisualTransform;
    private float lastUseTime;

    // ============================================================
    // PROPERTIES
    // ============================================================

    public bool IsPickedUp => isPickedUp.Value;
    public float Durability => durability.Value;
    public float DurabilityPercent => durability.Value / maxDurability;

    // ============================================================
    // LIFECYCLE
    // ============================================================

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to state changes
        isPickedUp.OnValueChanged += OnPickupStateChanged;
        durability.OnValueChanged += OnDurabilityChanged;

        // Initialize durability on server
        if (IsServer && durability.Value == 0f)
        {
            durability.Value = maxDurability;
        }

        // Apply initial state
        OnPickupStateChanged(false, isPickedUp.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unsubscribe
        isPickedUp.OnValueChanged -= OnPickupStateChanged;
        durability.OnValueChanged -= OnDurabilityChanged;
    }

    private void Update()
    {
        // Owner-only input
        if (!IsOwner) return;
        if (!isPickedUp.Value) return;

        // Use item on input
        if (Input.GetMouseButtonDown(0))  // Left click
        {
            Use();
        }
    }

    // ============================================================
    // PICKUP SYSTEM
    // ============================================================

    /// <summary>
    /// Called by player to request pickup.
    /// </summary>
    public void RequestPickup(PlayerInventory playerInventory)
    {
        if (isPickedUp.Value)
        {
            Debug.LogWarning($"{itemName} already picked up");
            return;
        }

        RequestPickupServerRpc(
            playerInventory.GetComponent<NetworkObject>(),
            NetworkObject
        );
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(
        NetworkObjectReference playerRef,
        NetworkObjectReference itemRef,
        ServerRpcParams rpcParams = default)
    {
        // === VALIDATION ===

        // 1. Validate player exists
        if (!playerRef.TryGet(out NetworkObject playerNetObj))
        {
            Debug.LogError("Invalid player reference");
            return;
        }

        PlayerInventory inventory = playerNetObj.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogError("Player has no inventory");
            return;
        }

        // 2. Validate item exists
        if (!itemRef.TryGet(out NetworkObject itemNetObj))
        {
            Debug.LogError("Invalid item reference");
            return;
        }

        // 3. Check if already picked up (atomic check)
        if (isPickedUp.Value)
        {
            Debug.LogWarning($"{itemName} already picked up");
            return;
        }

        // 4. Check inventory space
        if (!inventory.HasSpace())
        {
            Debug.LogWarning("Inventory full");
            NotifyInventoryFullClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { rpcParams.Receive.SenderClientId }
                }
            });
            return;
        }

        // 5. Validate distance
        float distance = Vector3.Distance(
            playerNetObj.transform.position,
            itemNetObj.transform.position
        );

        if (distance > maxPickupDistance)
        {
            Debug.LogWarning($"Player too far from item: {distance}m");
            return;
        }

        // === GRANT PICKUP ===

        GrantPickup(inventory, playerNetObj, rpcParams.Receive.SenderClientId);
    }

    private void GrantPickup(PlayerInventory inventory, NetworkObject playerNetObj, ulong clientId)
    {
        // 1. Mark as picked up (triggers OnPickupStateChanged on all clients)
        isPickedUp.Value = true;
        ownerClientId.Value = clientId;

        // 2. Add to inventory
        inventory.AddItemServerInternal(NetworkObject);

        // 3. Change ownership to player
        NetworkObject.ChangeOwnership(clientId);

        // 4. Parent to player's hand
        Transform handTransform = playerNetObj.GetComponentInChildren<PlayerController>()?.GetHandTransform();
        if (handTransform != null)
        {
            transform.SetParent(handTransform);
            transform.localPosition = heldLocalPosition;
            transform.localRotation = Quaternion.Euler(heldLocalRotation);
        }

        // 5. Notify all clients
        OnPickedUpClientRpc(clientId);

        Debug.Log($"{itemName} picked up by client {clientId}");
    }

    private void OnPickupStateChanged(bool wasPickedUp, bool nowPickedUp)
    {
        // Update visuals on ALL clients
        if (nowPickedUp)
        {
            // Hide world pickup visual
            if (worldVisual != null)
                worldVisual.SetActive(false);

            // If we're the owner, show held visual
            if (IsOwner && ownerClientId.Value == NetworkManager.Singleton.LocalClientId)
            {
                ShowHeldVisual();
            }
        }
        else
        {
            // Show world pickup visual
            if (worldVisual != null)
                worldVisual.SetActive(true);

            // Hide held visual
            HideHeldVisual();
        }
    }

    [ClientRpc]
    private void OnPickedUpClientRpc(ulong pickerId)
    {
        // Play pickup sound on all clients
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // If we're the picker, show feedback
        if (pickerId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"You picked up {itemName}");
        }
    }

    [ClientRpc]
    private void NotifyInventoryFullClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // Show UI message to specific player
        Debug.Log("Inventory Full!");
        // TODO: Show UI popup
    }

    // ============================================================
    // USAGE SYSTEM
    // ============================================================

    /// <summary>
    /// Use the item (called by owner).
    /// </summary>
    public void Use()
    {
        if (!IsOwner) return;
        if (!isPickedUp.Value) return;

        // Check cooldown (local prediction)
        if (Time.time - lastUseTime < useCooldown)
        {
            Debug.Log("Item on cooldown");
            return;
        }

        // Play local animation (prediction)
        PlayUseAnimation();

        // Request server validation
        UseItemServerRpc();
    }

    [ServerRpc]
    private void UseItemServerRpc()
    {
        // Validate cooldown
        if (Time.time - lastUseTime < useCooldown)
        {
            Debug.LogWarning("Item used too quickly");
            return;
        }

        // Validate durability
        if (durability.Value <= 0)
        {
            Debug.LogWarning("Item broken");
            OnItemBrokenClientRpc();
            return;
        }

        // Apply item effect
        ApplyItemEffect();

        // Update state
        durability.Value = Mathf.Max(0, durability.Value - 10f);  // Drain durability
        lastUseTime = Time.time;

        // Notify clients
        OnItemUsedClientRpc();
    }

    private void ApplyItemEffect()
    {
        // Implement your item's effect here
        // e.g., Deal damage, toggle light, etc.

        Debug.Log($"{itemName} used!");

        // Example: Raycast to hit enemies
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 5f))
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(25f);
            }
        }
    }

    [ClientRpc]
    private void OnItemUsedClientRpc()
    {
        // Non-owner clients see the effect
        if (!IsOwner)
        {
            PlayUseAnimation();
        }

        // All clients see/hear effects
        if (useSound != null)
            AudioSource.PlayClipAtPoint(useSound, transform.position);

        PlayVFX();
    }

    [ClientRpc]
    private void OnItemBrokenClientRpc()
    {
        // Item is broken
        Debug.Log($"{itemName} is broken!");

        // TODO: Show broken visual, disable usage
    }

    private void OnDurabilityChanged(float oldValue, float newValue)
    {
        // Update durability UI
        Debug.Log($"{itemName} durability: {newValue}/{maxDurability}");

        // TODO: Update UI bar
    }

    // ============================================================
    // VISUALS
    // ============================================================

    private void ShowHeldVisual()
    {
        if (heldVisualPrefab != null && heldVisualTransform == null)
        {
            GameObject heldObj = Instantiate(heldVisualPrefab, transform);
            heldVisualTransform = heldObj.transform;
            heldVisualTransform.localPosition = Vector3.zero;
            heldVisualTransform.localRotation = Quaternion.identity;
        }

        if (heldVisualTransform != null)
            heldVisualTransform.gameObject.SetActive(true);
    }

    private void HideHeldVisual()
    {
        if (heldVisualTransform != null)
        {
            heldVisualTransform.gameObject.SetActive(false);
        }
    }

    private void PlayUseAnimation()
    {
        // TODO: Play animation
        Debug.Log($"{itemName} use animation");
    }

    private void PlayVFX()
    {
        // TODO: Spawn VFX
        Debug.Log($"{itemName} VFX");
    }
}
