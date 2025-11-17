using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Template for a networked singleton manager.
/// Replace "TemplateManager" with your manager name.
/// </summary>
public class TemplateManager : NetworkSingleton<TemplateManager>
{
    // ============================================================
    // NETWORKED STATE
    // ============================================================

    /// <summary>
    /// Example networked state variable.
    /// Server writes, everyone reads.
    /// </summary>
    private NetworkVariable<int> exampleState = new NetworkVariable<int>(
        0,  // Default value
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Public read-only access
    public int ExampleState => exampleState.Value;

    // ============================================================
    // LIFECYCLE
    // ============================================================

    protected override void Awake()
    {
        base.Awake();  // CRITICAL: Call base first

        // Your custom initialization here
        Debug.Log($"[{GetType().Name}] Awake");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to network state changes on ALL clients
        exampleState.OnValueChanged += OnExampleStateChanged;

        // Server-specific initialization
        if (IsServer)
        {
            InitializeServerState();
        }

        // Client-specific initialization
        if (IsClient)
        {
            InitializeClientState();
        }

        Debug.Log($"[{GetType().Name}] Network spawned. IsServer={IsServer}, IsClient={IsClient}");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unsubscribe to prevent memory leaks
        exampleState.OnValueChanged -= OnExampleStateChanged;

        Debug.Log($"[{GetType().Name}] Network despawned");
    }

    // ============================================================
    // INITIALIZATION
    // ============================================================

    private void InitializeServerState()
    {
        // Server-only initialization
        // e.g., Load save data, initialize pools, etc.
    }

    private void InitializeClientState()
    {
        // Client-only initialization
        // e.g., Initialize UI, audio, etc.
    }

    // ============================================================
    // STATE CHANGE CALLBACKS
    // ============================================================

    /// <summary>
    /// Called on ALL clients when the state changes.
    /// </summary>
    private void OnExampleStateChanged(int oldValue, int newValue)
    {
        Debug.Log($"Example state changed: {oldValue} -> {newValue}");

        // Publish event for other systems
        EventBus.EventBus.Instance?.Publish(new ExampleStateChangedEvent
        {
            NewValue = newValue
        });

        // Update UI, visuals, etc.
        UpdateVisuals(newValue);
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    /// <summary>
    /// Modify state from any client - uses ServerRpc.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ModifyStateServerRpc(int delta, ServerRpcParams rpcParams = default)
    {
        // Validate input
        if (delta == 0)
        {
            Debug.LogWarning("Delta is zero, ignoring");
            return;
        }

        // Optionally validate sender
        ulong senderId = rpcParams.Receive.SenderClientId;
        if (!IsValidSender(senderId))
        {
            Debug.LogWarning($"Invalid sender: {senderId}");
            return;
        }

        // Apply change
        exampleState.Value += delta;

        // OnValueChanged will trigger automatically on all clients
    }

    // ============================================================
    // VALIDATION
    // ============================================================

    private bool IsValidSender(ulong clientId)
    {
        // Add your validation logic
        // e.g., Check if client is connected, not banned, etc.
        return NetworkManager.ConnectedClients.ContainsKey(clientId);
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private void UpdateVisuals(int value)
    {
        // Update UI, effects, etc. on all clients
        // This runs on both server and clients
    }
}

// ============================================================
// EVENT DEFINITIONS
// ============================================================

/// <summary>
/// Event published when the example state changes.
/// </summary>
public class ExampleStateChangedEvent
{
    public int NewValue { get; set; }
}
