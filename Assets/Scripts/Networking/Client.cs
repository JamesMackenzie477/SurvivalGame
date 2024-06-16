using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    // prefab of the player to spawn
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject[] registeredPrefabs;

    // file descriptor for client
    private NetworkClient client;

    // when this script becomes active
    private void Awake()
    {
        // register prefabs
        RegisterPrefabs();
        // instantiates network client
        client = new NetworkClient();
        // creates configuration struct
        ConnectionConfig clientConfig = new ConnectionConfig();
        // adds a channel to send packets on
        clientConfig.AddChannel(QosType.ReliableSequenced);
        clientConfig.AddChannel(QosType.Unreliable);
        clientConfig.MinUpdateTimeout = 10;
        clientConfig.ConnectTimeout = 2000;
        clientConfig.DisconnectTimeout = 2000;
        clientConfig.PingTimeout = 500;
        // apply configuration and alters max connections
        client.Configure(clientConfig, 1);
        // sets server connection handler
        client.RegisterHandler(MsgType.Connect, OnConnect);
        //client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        //client.RegisterHandler(MsgType.Error, OnError);
        // registers player prefab with unet so it can be loaded
        ClientScene.RegisterPrefab(playerPrefab);
        ConnectToServer();
    }

    // called by other scripts to connect to server
    public void ConnectToServer()
    {
        // connects to server
        client.Connect("localhost", 47777);
        // notifies the console
        Logging.LogToFile("Connecting to server: 86.2.54.8");
    }

    // called when the player recieves a connection from the server
    private void OnConnect(NetworkMessage netMsg)
    {
        // notifies the console
        Logging.LogToFile("Successfully connected to: " + netMsg.conn.address);
        // notify server we are ready
        ClientScene.Ready(netMsg.conn);
        // request server to add player
        ClientScene.AddPlayer(0);
    }

    // this function will register an array of prefabs with unet
    private void RegisterPrefabs()
    {
        // iterate through prefab to register array
        foreach (GameObject prefab in registeredPrefabs)
        {
            // register the prefab
            ClientScene.RegisterPrefab(prefab);
        }
    }
}
