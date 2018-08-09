using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommunicationContract;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour, INetEventListener
{
    public string IPAddress = "localhost";
    public int Port = 9050;
    private const string _connectionKey = "ConnectionString";
    private NetManager _clientManager;
    private EventBasedNetListener _netListener;

    private NetHandler _netHandler;

    // Use this for initialization
    void Start ()
    {
        _netListener = new EventBasedNetListener();
        _clientManager = new NetManager(_netListener, _connectionKey);
        _clientManager.UpdateTime = 100;
        _netHandler = GetComponent<NetHandler>();
        var connectObj = GameObject.FindGameObjectWithTag("Connect");
        if (connectObj != null)
            IPAddress = connectObj.GetComponent<Connect>().IP;
        ConnectToGameServer();
        RegisterListener();
    }
	
	// Update is called once per frame
	void Update () {
        if(_clientManager.IsRunning)
		    _clientManager.PollEvents();

        if (Input.GetKeyDown(KeyCode.R))
	    {
            DisconnectFromServer();
	        SceneManager.LoadScene(0);
        }
            
	}

    public void ConnectToGameServer()
    {
        _clientManager.Start();
        _clientManager.Connect(IPAddress, Port);
        _netHandler.AssignServerConnection(_clientManager.GetFirstPeer());
    }

    public void DisconnectFromServer()
    {
        _clientManager.Stop();
    }

    void OnDestroy()
    {
        Debug.Log("On Destory / Disconnecting");
        DisconnectFromServer();
    }



    private void RegisterListener()
    {
        _netListener.PeerConnectedEvent += OnPeerConnected;
        _netListener.PeerDisconnectedEvent += OnPeerDisconnected;
        _netListener.NetworkReceiveEvent += OnNetworkReceive;
        _netListener.NetworkLatencyUpdateEvent += OnNetworkLatencyUpdate;
        _netListener.NetworkErrorEvent += OnNetworkError;
        _netListener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnected;
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("On Peer Connected!");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("On Peer Disconnected!");
    }

    public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
    {
        Debug.Log("On Network Error!");
    }

    public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
    {
        CommunecationContract.CreateEvent(_netHandler, peer, reader);
    }

    public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
    {
        Debug.Log("On Network Recieve Unconnected!");

    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }
}
