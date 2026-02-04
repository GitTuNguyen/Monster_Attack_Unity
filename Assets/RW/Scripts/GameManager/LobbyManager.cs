using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    private Dictionary<ulong, int> readyPlayers = new();

    [SerializeField]
    private NetworkLauncher networkLauncher = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void ClientReady(int characterIndex)
    {
        PlayerSelectionStore.Instance.SelectedCharacterIndex = characterIndex;
        SubmitReadyServerRpc(characterIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitReadyServerRpc(int characterIndex, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        readyPlayers[clientId] = characterIndex;

        Debug.Log($"Client {clientId} ready with character {characterIndex}");
    }
    public void StartGame()
    {
        if (!IsServer)
            return;

        if (!AllPlayersReady())
        {
            Debug.LogWarning("Not all players ready!");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            "Main",
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }

    private bool AllPlayersReady()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!readyPlayers.ContainsKey(clientId))
                return false;
        }
        return true;
    }
    public int GetCharacterIndex(ulong clientId)
    {
        return readyPlayers.TryGetValue(clientId, out var idx) ? idx : 0;
    }

    public void ShutdownNetwork()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening &&
            NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
