using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField]
    public Vector3d prevPositionCosmic;


    public Vector3d positionRelativeToRocket
    {
        get
        {
            return position + WorldManager.worldOffset;
        }
    }

    public Vector3d position;
    public Vector3d predictedPosition;

    public Vector3d velocity
    {
        get
        {
            return WorldManager.MiniatureToCosmic(reference.velocity);
        }
    }

    public CelestialBody reference;
    public double velocityS;

    public float vMax = 90;
    public Vector3d wind;

    public GameObject startPoint;
    public GameObject attached;
    public Vector3 toCenterPlanet;
    

    public void Setup(CelestialBody reference)
    {
        this.reference = reference;
        float size = reference.radius_km * 1000 / Simulation.unitRocketM * 2;
        transform.localScale = new Vector3(size, size, size);
        transform.position = (Vector3)WorldManager.MiniatureToCosmic(reference.transform.position - reference.visualOffset);
        position = (Vector3d)WorldManager.MiniatureToCosmic(reference.transform.position - reference.visualOffset);
    }

    public void UpdatePlanetPosition()
    {
        prevPositionCosmic = position;
        position = WorldManager.MiniatureToCosmic(reference.position);
        predictedPosition = WorldManager.MiniatureToCosmic(reference.predictedPosition);

        velocityS = WorldManager.MiniatureToCosmic(reference.velocity).magnitude;

        if (attached != null)
        {
            WorldManager.worldOffset -= (position - prevPositionCosmic);
        }

        transform.position =(Vector3)positionRelativeToRocket;
        transform.rotation = reference.transform.rotation;
    }

    public void AttachRocket(Vector3 position, Rocket rocket)
    {
        if (startPoint == null)
        {
            startPoint = new GameObject("StartPoint");
            startPoint.transform.SetParent(this.transform);
        }

        startPoint.transform.position = position;      
        startPoint.transform.rotation = Quaternion.FromToRotation(Vector3.up, (rocket.transform.position - transform.position).normalized);

        rocket.transform.position = position;
        rocket.transform.SetParent(startPoint.transform,true);
        rocket.transform.rotation = startPoint.transform.rotation;
        rocket.attachedTo = this;

        attached = rocket.gameObject;
        Debug.Log("Rocket Attached to planet!");
    }

    public void UnAttachRocket()
    {
        if (attached != null)
        {
            attached.transform.SetParent(null, true);
            attached = null;
        }
    }

    public Vector3d CalculateRotationVelocity(float planet_latitude)
    {
        float speedAtLatitude = reference.CalculateRotationSpeed(planet_latitude);
        
        return -Vector3d.forward * speedAtLatitude / Simulation.unitRocketM;
    }
}