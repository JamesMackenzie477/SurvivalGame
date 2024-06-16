using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    // the port to start the server on
    [SerializeField] private int port;
    // the max players for the server
    [SerializeField] [Range(1, 250)] private int maxPlayers;
    // the gameobject of the player model to spawn
    [SerializeField] private Object playerPrefab;
    // an array of spawn locations for the player
    [SerializeField] private Transform[] spawnPoints;

    // when this script becomes active
    void Awake()
    {
        // creates configuration struct
        ConnectionConfig serverConfig = new ConnectionConfig();
        // adds a channel to send packets on
        serverConfig.AddChannel(QosType.ReliableSequenced);
        serverConfig.AddChannel(QosType.Unreliable);
        serverConfig.MinUpdateTimeout = 10;
        serverConfig.ConnectTimeout = 2000;
        serverConfig.DisconnectTimeout = 2000;
        serverConfig.PingTimeout = 500;
        // apply configuration and alters max connections
        NetworkServer.Configure(serverConfig, maxPlayers);
        // sets up handlers for clients
        // this handler is called as soon as a connection for a client is recieved
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnect);
        // this handler is called when the client calls NetworkClient.AddPlayer()
        NetworkServer.RegisterHandler(MsgType.AddPlayer, OnAddPlayer);
        // this handler is called when a player disconnects from the server
        // this should be used to clean up the connection and anything left behind from the player
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnPlayerDisconnect);

        // binds server to port
        // the client will need to connect to this port in order to join the server
        if (NetworkServer.Listen(port))
            // notifies user of the port
            Logging.LogToFile("Server started on port: " + NetworkServer.listenPort);
        else
            // notifies user if failed
            Logging.LogToFile("There was a problem starting the server");

        // loads up all networkidentity objects
        NetworkServer.SpawnObjects();
    }

    // this is called when a player connects to our server
    void OnConnect(NetworkMessage netMsg)
    {
        // notifies the console
        Logging.LogToFile("Recieved connection from: " + netMsg.conn.address);
    }

    // this is called when a player requests their player to be added
    void OnAddPlayer(NetworkMessage netMsg)
    {
        // chooses a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        // what we will spawn, at what location and at what rotation
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
        // associate the player object with the connection
        NetworkServer.AddPlayerForConnection(netMsg.conn, player, 0);
        // logs to server log
        Logging.LogToFile("Spawned player for: " + netMsg.conn.address + " at: " + spawnPoint.position);
    }

    // this is called when a player disconnects from the server
    void OnPlayerDisconnect(NetworkMessage netMsg)
    {
        // notifies the console
        Logging.LogToFile("Connection terminated with: " + netMsg.conn.address);
        // destorys all objects associated with the player
        NetworkServer.DestroyPlayersForConnection(netMsg.conn);
    }
}
