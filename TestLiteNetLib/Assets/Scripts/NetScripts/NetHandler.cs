using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CommunicationContract;
using LiteNetLib;

public class NetHandler : MonoBehaviour, ICommuncationContract
{
    public List<GameObject> NetObjectsToSpawn;
    private Dictionary<long, GameObject> _networkObjects;
    public Dictionary<long, NetworkObject> _netUpdateObjects;
    public Dictionary<long, GameObject> _playerList;
    private Dictionary<Type, string> _messageFunctionMapper;
    private NetPeer _serverConnection;
    private List<Tuple<IConctract, SendOptions>> _updateEvents;
    private float _sendTimer;
    private const float _sendTimerToUpdate = 50;
    private Spawner _spawner;


    // Use this for initialization
    void Start () {
	    _networkObjects = new Dictionary<long, GameObject>();
        _netUpdateObjects = new Dictionary<long, NetworkObject>();
        _updateEvents = new List<Tuple<IConctract, SendOptions>>();
        _playerList = new Dictionary<long, GameObject>();
        _spawner = GetComponent<Spawner>();
        InstansiateFunctionMapper();
    }


    // Update is called once per frame
	void Update ()
	{
	    var t1 = Time.realtimeSinceStartup;
	    fetchNewUpdateEvents();
	    var t2 = Time.realtimeSinceStartup;

	}

    private void fetchNewUpdateEvents()
    {
        _sendTimer += Time.deltaTime;
        if (_sendTimer*1000 < _sendTimerToUpdate) return;
        _sendTimer = 0;
        foreach (var netUpdateObj in _netUpdateObjects)
        {
            if (netUpdateObj.Value.HasUpdate())
            {
                _updateEvents.AddRange(netUpdateObj.Value.GetUpdateEvents());
                netUpdateObj.Value.ClearUpdateEvent();
            }
        }

        if (_updateEvents.Any())
            SendUpdateEvents();
    }

    public void SpawnEvent(SpawnData spawnData)
    {
        if(spawnData.spawnType == ObjectType.Player)
            SpawnPlayer(spawnData);
    }

    public void SpawnPlayer(SpawnData spawnData)
    {
    }

    public void OnClickToPosition(ClickToPosition clickToPositionObject)
    {
        SendMessageToObject(clickToPositionObject, clickToPositionObject.objectID,
            GetFunctionName(clickToPositionObject));
    }

    public void OnSpawnData(SpawnData spawnDataObject)
    {
        Debug.Log("Got Spawn Data!");
        SpawnEvent(spawnDataObject);
    }

    public void OnUpdatePosition(UpdatePositionData updatePositionData)
    {
        Debug.Log("Got Position Data!");
    }

    public void OnUpdatePositionInterpolate(UpdatePositionInterpolateData updatePositionInterpolateData)
    {
        SendMessageToObject(updatePositionInterpolateData, updatePositionInterpolateData.objectID,
            GetFunctionName(updatePositionInterpolateData));
    }

    public void OnDeleteObject(DeleteObjectData deleteObjectData)
    {
        GameObject.Destroy(_networkObjects[deleteObjectData.objectID]);
        RemoveNetworkObjectFromLists(deleteObjectData.objectID);
    }


    public void OnSpawnPlayer(SpawnPlayerData spawnPlayerData)
    {
        Debug.Log("Got Player Spawn Data!");
        var player = _spawner.GetPlayerObject(spawnPlayerData);

        AddNewNetObject(player, spawnPlayerData.objectID);
        _playerList.Add(spawnPlayerData.owner, player);
        SendMessageToObjectWaitOneFrame(spawnPlayerData, spawnPlayerData.objectID,
            GetFunctionName(spawnPlayerData));
    }

    public void OnSpawnFireballData(SpawnFireballData spawnFireBallData)
    {
        Debug.Log("Got Fireball Spawn Data!");
        var fireball = _spawner.GetFireballObject(spawnFireBallData);

        AddNewNetObject(fireball, spawnFireBallData.objectID);
        SendMessageToObjectWaitOneFrame(spawnFireBallData, spawnFireBallData.objectID,
            GetFunctionName(spawnFireBallData));

        if (_playerList.ContainsKey(spawnFireBallData.owner))
        {
            SendMessageToObjectWaitOneFrame(
                spawnFireBallData.Direction, _playerList[spawnFireBallData.owner].GetComponent<NetworkObject>().getObjectID(),
                "CastSkill");
        }
    }

    private void Spawn(SpawnData spawnBase)
    {

    }

    public void OnClientSpawnFireballData(ClientSpawnFireballData clientSpawnFireballData)
    {
        Debug.Log("Lol wtf, client spawn fireball????");
    }

    public void OnVariableUpdate(UpdateVariableData updateVariableData)
    {
        //Debug.Log($"Update Variable Data!: {updateVariableData.variableName} : {UpdateVariableHander.GetUpdateVariableData(updateVariableData)}" );
        SendMessageToObject(updateVariableData, updateVariableData.objectID,
            GetFunctionName(updateVariableData));
    }

    public void OnClientCastAbility(ClientCastAbilityData clientCastAbilityData)
    {
        Debug.Log("lol wtf, client got clientspawn data!?");
    }

    void SendMessageToObjectWaitOneFrame<T>(T message, long objectID, string messageFunction)
    {

        StartCoroutine(SendDelayedMessage(message, objectID, messageFunction));
    }
    void SendMessageToObjectWaitOneFrame(long objectID, string messageFunction)
    {

        StartCoroutine(SendDelayedMessage(objectID, messageFunction));
    }

    IEnumerator SendDelayedMessage<T>(T message, long objectID, string messageFunction)
    {
        yield return 0;
        SendMessageToObject(message, objectID, messageFunction);
    }
    IEnumerator SendDelayedMessage(long objectID, string messageFunction)
    {
        yield return 0;
        SendMessageToObject(objectID, messageFunction);
    }

    private void SendMessageToObject<T>(T message, long objectID, string messageFunction)
    {
        var objectToUpdate = GetGameObejectByID(objectID);
        if (objectToUpdate == null) return;

        objectToUpdate.SendMessage(messageFunction, message, SendMessageOptions.DontRequireReceiver);
    }

    private void SendMessageToObject(long objectID, string messageFunction)
    {
        var objectToUpdate = GetGameObejectByID(objectID);
        if (objectToUpdate == null) return;

        objectToUpdate.SendMessage(messageFunction, SendMessageOptions.DontRequireReceiver);
    }

    private GameObject GetGameObejectByID(long objectID)
    {
        if (_networkObjects.ContainsKey(objectID))
            return _networkObjects[objectID];
        return null;
    }


    private void InstansiateFunctionMapper()
    {
        _messageFunctionMapper = new Dictionary<Type, string>();
        _messageFunctionMapper.Add(typeof(UpdatePositionInterpolateData), "UpdatePositionInterpolateData");
        _messageFunctionMapper.Add(typeof(SpawnData), "InstansiatePlayer");
        _messageFunctionMapper.Add(typeof(ClickToPosition), "UpdateMoveToPosition");
        _messageFunctionMapper.Add(typeof(SpawnPlayerData), "PlayerSpawn");
        _messageFunctionMapper.Add(typeof(SpawnFireballData), "FireballSpawn");
        _messageFunctionMapper.Add(typeof(UpdateVariableData), "VariableUpdate");
    }

    private string GetFunctionName(object dataObject)
    {
        if (_messageFunctionMapper.ContainsKey(dataObject.GetType()))
            return _messageFunctionMapper[dataObject.GetType()];

        return string.Empty;
    }

    public void ClearUpdateEvent()
    {
        this._updateEvents.Clear();
    }

    public void SendUpdateEvents()
    {
        foreach(var message in _updateEvents)
            _serverConnection.Send(message.Item1.FetchWriter(), message.Item2);

        ClearUpdateEvent();
    }

    private void AddNewNetObject(GameObject netObject, long objectID)
    {
        _networkObjects.Add(objectID, netObject);
        AddNetUpdateObjectFromNetworkObject(objectID, netObject);
    }

    private void AddNetUpdateObjectFromNetworkObject(long ID,GameObject netObject)
    {
        var tNetObject = netObject.GetComponent<NetworkObject>();
        if (tNetObject != null)
        {
            this._netUpdateObjects.Add(ID, tNetObject);
            tNetObject.SetObjectID(ID);
        }
            
    }

    private void RemoveNetworkObjectFromLists(long ID)
    {
        if(_netUpdateObjects.ContainsKey(ID))
            this._netUpdateObjects.Remove(ID);

        if (_networkObjects.ContainsKey(ID))
            this._networkObjects.Remove(ID);
    }


    public void AssignServerConnection(NetPeer server)
    {
        this._serverConnection = server;
    }
}
