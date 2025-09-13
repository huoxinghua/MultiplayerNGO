using Steamworks;
using TMPro;
using UnityEngine;
namespace Project.Network.SteamWork
{ 
    public class SteamBootstrap : MonoBehaviour
    {
        private bool ok;
        void Awake()
        {
            Debug.Log("[Steam] Awake: preparing initialization");
            try
            {
                ok = SteamAPI.Init();
                Debug.Log("[Steam] SteamAPI.Init() returned: " + ok);
                if (ok)
                {
                    var name = SteamFriends.GetPersonaName();
                    Debug.Log("[Steam] Current user: " + name + " | SteamID: " + SteamUser.GetSteamID());
                }
                else
                {
                    Debug.LogError("[Steam] Init failed. Common reasons: wrong DLL path/architecture, missing steam_appid.txt in project root, or Steam client not running/logged in.");
                }
            }
            catch (System.DllNotFoundException e)
            {
                Debug.LogError("[Steam] steam_api64.dll not found (make sure it is in Assets/Plugins/x86_64/ and CPU is set to x86_64)\n" + e);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Steam] Exception during initialization:\n" + e);
            }
        }

        void Update()
        {
            if (ok) SteamAPI.RunCallbacks();

            if (ok && Input.GetKeyDown(KeyCode.F1))
            {
                SteamFriends.ActivateGameOverlay("Friends");
                Debug.Log("[Steam] Tried to open Overlay (Shift+Tab should also work)");
            }
        }

        void OnDestroy()
        {
            if (ok) SteamAPI.Shutdown();
        }
    }
}