using UnityEngine;
using System.Collections;

public class MouseOrbit : MonoBehaviour
{
    public Transform target;

    public bool relativeRotation=true;
    public bool clampY=true;

    public Vector3 offset = Vector3.zero;

    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    float x = 0.0f;
    float y = 0.0f;

    private bool changeDir=false;

    // Use this for initialization
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.W) && WorldManager.worldType == WorldType.Rocket)
        {
            changeDir = !changeDir;
            if (changeDir)
            {
                Camera.main.transform.position = target.transform.position;
                Camera.main.transform.LookAt(target.GetComponent<Rocket>().direction.normalized * 10);
            }
        }  
        if(WorldManager.worldType == WorldType.Miniature)
        {
            changeDir = false;
        }
    }

    void LateUpdate()
    {
        if (target && !changeDir)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            if (clampY)
            {
                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }
            Quaternion rotation = Quaternion.Euler(y, x, 0);

            if (relativeRotation)
            {
                //ИМЕННО В ТАКОЙ ПОСЛЕДОВАТЕЛЬНОСТИ
                rotation = target.rotation;
                rotation *= Quaternion.Euler(y, x, 0);
            }

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit))
            {
                distance -= hit.distance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position + (transform.rotation * offset); 
        }

        if (changeDir)
        {
            Camera.main.transform.position = target.transform.position;

            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X");
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y");
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);

            target.transform.up = transform.forward;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}