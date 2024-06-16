using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FallDamage : NetworkBehaviour {

    [SerializeField] private float minSurviveTime;
    [SerializeField] private float damagePerSecond;

    private PlayerHealth healthScript;
    private CharacterController controller;

    private float airTime = 0;

    // Use this for initialization
    void Start()
    {
        // gets health script and character controller
        healthScript = GetComponent<PlayerHealth>();
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // if we are only the server
        if (!isServer)
            return;

        // if the player is not grounded increase time
        if (!controller.isGrounded)
        {
            airTime += Time.fixedDeltaTime;
        }
        else
        {
            // if the air time is greate than the minimal survive time
            if (airTime > minSurviveTime)
            {
                // decrease health
                healthScript.health -= airTime * damagePerSecond;
            }
            // reset air time
            airTime = 0;
        }
    }
}
