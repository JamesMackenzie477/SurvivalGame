using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

[RequireComponent(typeof(PlayerHealth))]
public class HUD : NetworkBehaviour {

    // stores the health, hunger and thirst text elements
    private Text health;
    private Text hunger;
    private Text thirst;

    // stores the player health script
    private PlayerHealth playerVitals;

    // Use this for initialization
    void Start()
    {
        // if we are not the local player then return
        if (!isLocalPlayer)
            return;

        // gets the text element of health, hunger and thirst
        health = GameObject.Find("Health").GetComponentInChildren<Text>();
        hunger = GameObject.Find("Hunger").GetComponentInChildren<Text>();
        thirst = GameObject.Find("Thirst").GetComponentInChildren<Text>();
        // gets the player vitals script
        playerVitals = GetComponent<PlayerHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        // if we are not the local player then return
        if (!isLocalPlayer)
            return;

        // update the corresponding gui elements
        health.text = string.Format("Health: {0}%", Math.Ceiling(playerVitals.health));
        hunger.text = string.Format("Hunger: {0}%", Math.Ceiling(playerVitals.hunger));
        thirst.text = string.Format("Thirst: {0}%", Math.Ceiling(playerVitals.thirst));
    }
}
