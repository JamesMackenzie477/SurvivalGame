using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LootSpawner : NetworkBehaviour {

    [SerializeField] private GameObject prefab;

	// Use this for initialization
	void Start () {

        if (!isServer)
            return;

        for (int i = 0; i < 5; i++)
            NetworkServer.Spawn(Instantiate(prefab));
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
