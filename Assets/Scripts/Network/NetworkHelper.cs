using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkHelper : NetworkBehaviour
{
    [SerializeField] Functions.ConnectionType connectionType = Functions.ConnectionType.Client;                     // Here we save the connection type, even though its possible to get through IsClient etc..
    [SerializeField] Dictionary<ulong, (string, string)> PlayerList = new Dictionary<ulong, (string, string)>();    // A list of connected spectators.

    public int maxPlayers = 10;

    private void Start()
    {
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    /// <summary>Starts the game as a host</summary>
    /// <param name="gameSceneName">The name of the scene it should switch to</param>
    public void StartHost(string gameSceneName = "GameScene")
    {
        // Incase client clicked "Join Game" and was not able to connect, we will run a network shutdown to reset, before hosting.
        if (connectionType == Functions.ConnectionType.Client)
        {
            NetworkManager.Shutdown();
        }

        // Set connection type & switch scene, clients will automatically synchronize scene with the host/server.
        connectionType = Functions.ConnectionType.Host;

        SceneManager.LoadScene(gameSceneName);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>Starts the game as a client</summary>
    /// <param name="gameSceneName">The name of the scene it should switch to</param>
    public void StartClient(string gameSceneName = "GameScene")
    {
        connectionType = Functions.ConnectionType.Client;

        // For clients we check if its able to connect, before switching scenes.
        NetworkManager.Singleton.StartClient();
    }

    /// <summary>What happens when the scene has successfully loaded</summary>
    private void OnSceneLoaded(Scene arg01, LoadSceneMode arg02)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        switch (connectionType) 
        {
            case Functions.ConnectionType.Client:
            {
                
                break;
            }
            case Functions.ConnectionType.Host:
            {
                NetworkManager.Singleton.StartHost();
                break;
            }

            case Functions.ConnectionType.Server:
            {
                NetworkManager.Singleton.StartServer();
                break;
            }
        }
    }

    /// <summary> ApprovalCheck which is executed upon connecting, here its possible to refuse connection if the server is full forexample.</summary>
    void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;
        string[] parsedConnectionDataArray = System.Text.Encoding.ASCII.GetString(connectionData).Split('|');

        // Approve connection
        response.Approved = ApproveConnection(ref response);
        response.CreatePlayerObject = false;
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
        response.Pending = false;

        if(response.Approved)
            PlayerList.Add(clientId, ($"{parsedConnectionDataArray[0]}", $"Spectator {clientId}"));
    }

    public bool ApproveConnection(ref NetworkManager.ConnectionApprovalResponse response)
    {
        // Deny connection if max players have been reached
        if (NetworkManager.ConnectedClients.Count >= maxPlayers)
        {
            response.Reason = "Server is full";
            return false;
        }
        return true;
    }

    private void OnClientDisconnectCallback(ulong obj)
    {
        if (!NetworkManager.IsServer && NetworkManager.DisconnectReason != string.Empty)
        {
            Debug.Log($"Approval Declined Reason: {NetworkManager.DisconnectReason}");
        }
    }
}
