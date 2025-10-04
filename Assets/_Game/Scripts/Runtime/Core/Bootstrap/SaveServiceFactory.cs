using UnityEngine;

namespace Game.Runtime.Core.Services
{
    public static class SaveServiceFactory
    {
        public static ISaveService CreateSaveService(GameObject parent, bool forceSteam = false)
        {
#if UNITY_STANDALONE && !UNITY_EDITOR
            if (IsSteamAvailable() || forceSteam)
            {
                var steamService = parent.AddComponent<SteamSaveService>();
                Debug.Log("[SaveServiceFactory] ✅ Created SteamSaveService");
                return steamService;
            }
#endif
            
            var localService = parent.AddComponent<LocalSaveService>();
            Debug.Log("[SaveServiceFactory] ✅ Created LocalSaveService (fallback)");
            return localService;
        }
        
        private static bool IsSteamAvailable()
        {
            try
            {
                return System.Type.GetType("Steamworks.SteamAPI, Assembly-CSharp") != null;
            }
            catch
            {
                return false;
            }
        }
    }
}