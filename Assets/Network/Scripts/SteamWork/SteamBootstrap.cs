using Steamworks;
using TMPro;
using UnityEngine;

namespace Project.Network.SteamWork
{
    public class SteamBootstrap : MonoBehaviour
    {
        public static SteamBootstrap Instance;
        private bool ok;
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            if (SteamManager.Initialized)
            {
                SteamAPI.RunCallbacks();
                
            }

            if (ok && Input.GetKeyDown(KeyCode.F1))
            {
                SteamFriends.ActivateGameOverlay("Friends");
               
            }
        }

        void OnDestroy()
        {
            if (ok) SteamAPI.Shutdown();
        }
    }
}