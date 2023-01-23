using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    private bool isGameInProgress = false;

    public List<RTSPlayer> Players { get;  } = new List<RTSPlayer>();

    #region Server

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if(!isGameInProgress) { return; }
        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();
        
        isGameInProgress = false;
    }

    public void StartGame()
    {
        if(Players.Count < 2) { return; }

        isGameInProgress=true;

        ServerChangeScene("Scene_map_01");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);

        player.SetDisplayName($"Player {Players.Count}");

        player.SetTeamColor(new Color(UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)));

        player.SetPartyOwner(Players.Count == 1);

    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_map"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach(RTSPlayer player in Players)
            {
             GameObject baseInstance =  Instantiate(unitBasePrefab, GetStartPosition().position,Quaternion.identity);

                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }

    #endregion

    #region Client
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect();

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect();

        ClientOnConnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }

    #endregion




}