using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkLauncher : MonoBehaviour
{
    [SerializeField] private string address = "127.0.0.1";
    [SerializeField] private ushort port = 7777;

    public void StartHost()
    {
        //if (NetworkManager.Singleton.IsListening)
        //{
        //    NetworkManager.Singleton.Shutdown();
        //}
        ConfigureTransportForHost();
        NetworkManager.Singleton.StartHost();
        GameModeManager.Mode = GameMode.Multiplayer;
    }

    public void StartClient()
    {
        ConfigureTransportForClient();
        NetworkManager.Singleton.StartClient();
        GameModeManager.Mode = GameMode.Multiplayer;
    }

    private void ConfigureTransportForHost()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("UnityTransport is missing on NetworkManager.");
            return;
        }

        transport.SetConnectionData(address, port, "0.0.0.0");
    }

    private void ConfigureTransportForClient()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("UnityTransport is missing on NetworkManager.");
            return;
        }

        transport.SetConnectionData(address, port);
    }

    public void SetAddress(string newAddress) => address = newAddress;

    public void SetPort(string newPort)
    {
        if (ushort.TryParse(newPort, out var p))
            port = p;
    }
}
