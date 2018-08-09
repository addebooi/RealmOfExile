using System.Collections;
using System.Collections.Generic;
using CommunicationContract;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float Speed;
    public float Damage;
    public Vector3 Direction;
    private NetworkObject _netObject;

    void Start()
    {
        _netObject = GetComponent<NetworkObject>();
    }

    void Update()
    {
        
        transform.position += this.Direction * Speed * Time.deltaTime;
        
    }

    void FireballSpawn(SpawnFireballData spawnFireballData)
    {
        this.Direction = spawnFireballData.Direction;
        this.Speed = spawnFireballData.speed;
        this.Damage = spawnFireballData.damage;
        transform.LookAt(transform.position + Direction);
    }
}
