using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraLook : MonoBehaviour
{
    // sets the zoom speed of the player zoom
    [SerializeField] [Range(1,100)] private int zoomSpeed;
    // a texture to be used for a cross hair
    [SerializeField] private Texture2D crosshair;
    // the camera component used for zoom
    private Camera m_Camera;

    // Use this for initialization
    private void Start()
    {
        // gets the camera component
        m_Camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    private void Update()
    {
        // if the player is holding right click
        if (Input.GetKey(KeyCode.Mouse1))
        {
            // sets fov to 40
            m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, 40, zoomSpeed * Time.deltaTime);
        }
        else
        {
            // sets the fov to default
            m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, 90, zoomSpeed * Time.deltaTime);
        }
    }

    // will display to the gui
    private void OnGUI()
    {
        // draws crosshair on screen
        GUI.DrawTexture(new Rect((Screen.width - crosshair.width) / 2, (Screen.height - crosshair.height) / 2, crosshair.width, crosshair.height), crosshair);
    }

}
