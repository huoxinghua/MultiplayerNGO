using _Project.Code.Core.Patterns;
using Steamworks;
using Steamworks.NET;
using UnityEngine;

namespace _Project.Code.Network.SteamWork
{
    public class SteamBootstrap : Singleton<SteamBootstrap>
    {
        private bool ok;
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            try
            {
                ok = SteamAPI.Init();
                if (ok)
                {
                    var name = SteamFriends.GetPersonaName();
                }
                else
                {
                    Debug.LogError(
                        "[Steam] Init failed. Common reasons: wrong DLL path/architecture, missing steam_appid.txt in project root, or Steam client not running/logged in.");
                }
            }
            catch (System.DllNotFoundException e)
            {
                Debug.LogError(
                    "[Steam] steam_api64.dll not found (make sure it is in Assets/Plugins/x86_64/ and CPU is set to x86_64)\n" +
                    e);
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
                SteamAPI.RunCallbacks(); //this is very important, if without this P2P all functions not work
            }
        }

        void OnDestroy()
        {
            if (ok) SteamAPI.Shutdown();
        }
    }
}