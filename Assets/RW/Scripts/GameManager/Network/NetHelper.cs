using UnityEngine;

using Unity.Netcode;
public static class NetHelper
{
    public static bool IsServerOrOffline()
    {
        if (GameModeManager.Mode == GameMode.Offline)
            return true;

        if (NetworkManager.Singleton == null)
            return false;

        return NetworkManager.Singleton.IsServer;
    }
}
