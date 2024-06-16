using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour {

    // player movement speeds
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float lookSpeed;

    // max distance snapping for client side prediction
    [SerializeField] private float maxSyncDistance;

    // a list of locations sent to the server
    private List<object[]> sentLocation = new List<object[]>();

    // a queue of positions for no local characters
    private Queue<Vector3> newPositions = new Queue<Vector3>();

    // classes to interact with
    private CharacterController controller;
    private Camera m_Camera;
    private Vector3 moveDirection;

    // used for player and camera rotation
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private float freeLookYaw = 0.0f;
    private float freeLookPitch = 0.0f;

    // true if player is sprinting
    public bool isSprinting;

    // initalization
    private void Start()
    {
        // if we are not the local player we disable the camera
        if (!isLocalPlayer && !isServer)
            transform.GetChild(0).gameObject.SetActive(false);

        // gets camera and character controller component
        m_Camera = GetComponentInChildren<Camera>();
        controller = GetComponent<CharacterController>();
    }

    // called if this is the local player
    public override void OnStartLocalPlayer()
    {
        // hides mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // transform.GetChild(1).gameObject.SetActive(false);
        // GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    // called every frame on the client
    private void Update()
    {
        // if we are the local player
        if (isLocalPlayer)
        {

            // gets player mouse movement
            float mouseY = Input.GetAxis("Mouse Y");
            float mouseX = Input.GetAxis("Mouse X");

            // gets horizonal and vertical axis from player
            float xMov = Input.GetAxis("Horizontal");
            float zMov = Input.GetAxis("Vertical");

            // checkes if player is sprinting
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            // checks if player is jumping
            bool isJumping = Input.GetKeyDown(KeyCode.Space);

            // gets timestamp to send to server
            float timeStamp = Time.time;

            // moves player locally
            localMovePlayer(xMov, zMov, isSprinting, isJumping);
            // adds new position and time stamp to sent list to be checked later
            sentLocation.Add(new object[] { transform.position, timeStamp });
            // moves player on the server
            CmdMovePlayer(xMov, zMov, isSprinting, isJumping, timeStamp, Time.deltaTime);

            // rotates the player locally
            localRotatePlayer(mouseX, mouseY);
            // rotates the player on the server
            CmdRotatePlayer(pitch, yaw);
        }
        else if (!isLocalPlayer && !isServer)
        {
            transform.position = newPositions.Dequeue();
        }
    }

    // function that moves the player locally
    private void localMovePlayer(float x, float y, bool isSprinting, bool isJumping)
    {
        // if the character is currently on the ground
        if (controller.isGrounded)
        {
            // tranforms direction from local space to world space
            moveDirection = transform.TransformDirection(x, 0, y);
            // if player is holding shift we add more speed
            if (isSprinting)
            {
                moveDirection *= runSpeed;
                isSprinting = true;
            }
            else
            {
                moveDirection *= walkSpeed;
                isSprinting = false;
            }
            // if player is jumping we increase the y distance
            if (isJumping) moveDirection.y = jumpSpeed;
        }
        // adds gravity
        moveDirection.y -= 20 * Time.deltaTime;
        // sets characters movement
        controller.Move(moveDirection * Time.deltaTime);
    }

    // rotates the player locally
    private void localRotatePlayer(float x, float y)
    {
        // updates pitch and yaw depending on which way the mouse is moving
        float newYaw = lookSpeed * x;
        float newPitch = lookSpeed * y;

        // stops player from looking too far up or down
        pitch = Mathf.Clamp(pitch, -90, 90);

        // checks if player is using freelook
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (!(Mathf.Abs(freeLookYaw - yaw + 180) <= .1))
            {
                freeLookYaw = Mathf.Lerp(freeLookYaw, yaw + 180, 10f * Time.deltaTime);
            }
            else
            {
                // updates pitch and yaw depending on which way the mouse is moving
                freeLookYaw += newYaw;
                freeLookPitch -= newPitch;
            }
            // sets rotation of camera
            m_Camera.transform.eulerAngles = new Vector3(freeLookPitch, freeLookYaw, 0);
        }
        else
        {
            if (!(Mathf.Abs(freeLookYaw - yaw) <= .1) || !(Mathf.Abs(freeLookPitch - pitch) <= .1))
            {
                freeLookYaw = Mathf.Lerp(freeLookYaw, yaw, 10f * Time.deltaTime);
                freeLookPitch = Mathf.Lerp(freeLookPitch, pitch, 10f * Time.deltaTime);
                // sets rotation of camera
                m_Camera.transform.eulerAngles = new Vector3(freeLookPitch, freeLookYaw, 0);
            }
            else
            {
                // updates pitch and yaw depending on which way the mouse is moving
                yaw += newYaw;
                pitch -= newPitch;
                // sets rotation of player/camera
                transform.eulerAngles = new Vector3(0, yaw, 0);
                m_Camera.transform.eulerAngles = new Vector3(pitch, yaw, 0);
                freeLookYaw = yaw;
                freeLookPitch = pitch;
            }
        }
    }

    // rotates the player on server
    [Command] // called on server
    private void CmdRotatePlayer(float pitch, float yaw)
    {
        // sets rotation of player/camera
        transform.eulerAngles = new Vector3(0, yaw, 0);
        m_Camera.transform.eulerAngles = new Vector3(pitch, yaw, 0);
    }

    // moves player on the server
    [Command] // called on the server
    private void CmdMovePlayer(float x, float y, bool isSprinting, bool isJumping, float timeStep, float deltaTime)
    {
        // if the character is currently on the ground
        if (controller.isGrounded)
        {
            // tranforms direction from local space to world space
            moveDirection = transform.TransformDirection(x, 0, y);
            // if player is holding shift we add more speed
            if (isSprinting)
            {
                moveDirection *= runSpeed;
                isSprinting = true;
            }
            else
            {
                moveDirection *= walkSpeed;
                isSprinting = false;
            }
            // if player is jumping we increase the y distance
            if (isJumping) moveDirection.y = jumpSpeed;
        }
        // adds gravity
        moveDirection.y -= 20 * deltaTime;
        // sets characters movement
        controller.Move(moveDirection * deltaTime);
        // sync location with clients
        RpcSyncLocation(transform.position, timeStep);
    }

    // sync the player location with the server
    [ClientRpc] // called on client
    private void RpcSyncLocation(Vector3 position, float timeStep)
    {
        // if we are the local player
        if (isLocalPlayer)
        {
            // iterate through sent locations
            for (int i = 0; i < sentLocation.Count; i++)
            {
                // if the sent location timestamp is equal to the timestamp we have
                if ((float)sentLocation[i][1] == timeStep)
                {
                    // compares the two distances
                    if (Vector3.Distance((Vector3)sentLocation[i][0], position) > maxSyncDistance)
                    {
                        // set player position
                        transform.position = position;
                        // logs the location change
                        Logging.LogToFile("Changed player location to: " + position);
                    }
                }
            }
            // removes location and previous locations from the list
            sentLocation.RemoveAll(x => (float)x[1] <= timeStep);
        }
        else
        {
            // compares the two distances
            if (Vector3.Distance(transform.position, position) > 2)
            {
                // clears queue
                newPositions.Clear();
                // sets payer position
                transform.position = position;
            }
            else
            {
                // set player position
                newPositions.Enqueue(position);
            }

        }
    }
}
