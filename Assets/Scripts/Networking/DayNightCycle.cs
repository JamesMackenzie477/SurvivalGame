using UnityEngine;
using UnityEngine.Networking;

public class DayNightCycle : NetworkBehaviour {

    [SerializeField] private Transform sun;
    [SerializeField] private Transform moon;
    [SerializeField] [Range(1,100)] private int timeScale;

    [SyncVar] private Vector3 sunPosition;
    [SyncVar] private Vector3 moonPosition;
    [SyncVar] private Quaternion sunRotation;
    [SyncVar] private Quaternion moonRotation;

    // Use this for initialization
    void Start () {
        timeScale = timeScale / 100;
	}

    // Update is called once per frame
    void Update() {
        if (isServer)
        { 
            sun.RotateAround(Vector3.zero, Vector3.right, timeScale * Time.fixedDeltaTime);
            moon.RotateAround(Vector3.zero, Vector3.right, timeScale * Time.fixedDeltaTime);
            sun.LookAt(Vector3.zero);
            moon.LookAt(Vector3.zero);

            sunPosition = sun.position;
            sunRotation = sun.rotation;
            moonPosition = moon.position;
            moonRotation = moon.rotation;

        }
        else if (isClient)
        {
            sun.position = sunPosition;
            sun.rotation = sunRotation;
            moon.position = moonPosition;
            moon.rotation = moonRotation;
        }
    }
}
