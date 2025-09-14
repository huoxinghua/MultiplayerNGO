using UnityEngine;
using Steamworks;
public class SteamOverlay : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SteamFriends.ActivateGameOverlay("Friends");
            Debug.Log("Steam Tried to open Overlay (Shift+Tab works too)");
        }
    }
}
