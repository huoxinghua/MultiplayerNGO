using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;


public class MyNetworkUI : MonoBehaviour
{
    private Label statusLabel;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var hostButton = root.Q<Button>("HostButton");
        var clientButton = root.Q<Button>("ClientButton");
        var serverButton = root.Q<Button>("ServerButton");
        statusLabel = root.Q<Label>("StatusLabel");

        hostButton.clicked += () => StartHost();
        clientButton.clicked += () => StartClient();
        serverButton.clicked += () => StartServer();
    }


    private void Update()
    {
        UpdateStatusLabels();
    }

    private void StartHost() => NetworkManager.Singleton.StartHost();
    private void StartClient() => NetworkManager.Singleton.StartClient();
    private void StartServer() => NetworkManager.Singleton.StartServer();

    private void UpdateStatusLabels()
    {
        if (NetworkManager.Singleton.IsHost) statusLabel.text = "Mode: Host";
        else if (NetworkManager.Singleton.IsServer) statusLabel.text = "Mode: Server";
        else if (NetworkManager.Singleton.IsClient) statusLabel.text = "Mode: Client";
        else statusLabel.text = "Mode: None";
    }
}

