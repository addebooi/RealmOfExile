using System.Collections;
using System.Collections.Generic;
using CommunicationContract;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public GameObject Player;

    public GameObject Fireball;

    public GameObject GetPlayerObject(SpawnPlayerData playerData)
    {
        var player =  baseSpawn(Player, playerData);
        var netOBJ = player.GetComponent<NetworkObject>();
        if (netOBJ != null)
        {
            netOBJ.SetIsOwner(playerData.IsOwner);
        }

        return player;
    }


    public GameObject GetFireballObject(SpawnFireballData fireballData)
    {
        return baseSpawn(Fireball, fireballData);
    }

    private GameObject baseSpawn(GameObject prefab, SpawnData spawnData)
    {
        var go = Instantiate(prefab, spawnData.position, Quaternion.Euler(spawnData.rotation));
        go.transform.localScale = new Vector3(
            go.transform.localScale.x*spawnData.scale.x,
            go.transform.localScale.y * spawnData.scale.y,
            go.transform.localScale.z * spawnData.scale.z);

        StartCoroutine(SetIDS(go, spawnData));

        return go;
    }

    IEnumerator SetIDS(GameObject go,SpawnData spawnData)
    {
        yield return 0;
        if (go != null)
        {
            var netObject = go.GetComponent<NetworkObject>();
            if (netObject != null)
            {
                netObject.SetOwnerID(spawnData.owner);
                netObject.SetObjectID(spawnData.objectID);
            }
        }

    }
}
