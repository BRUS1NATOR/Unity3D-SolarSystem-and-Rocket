using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum CelesitalBodyType { Planet, Satellite }

public class CelestialBody : MonoBehaviour
{
    public string planetName;
    public Color orbitColor;
    public Sprite planetIcon;

    public int bodyID;
    public float radius_km;

    /// <summary>
    /// Длина экватора
    /// </summary>
    public float equator_length_km
    {
        get
        {
           return radius_km * 2 * (float)Math.PI;
        }
    }
    /// <summary>
    /// Масса относительно планеты Земли
    /// </summary>
    public double mass;

    /// <summary>
    /// Количество часов за которые произойдет один оборот
    /// </summary>
    public float sidereal_day_hrs = 24f;
    public float sideral_day_seconds
    {
        get
        {
            return sidereal_day_hrs * 60 * 60;
        }
    }
    /// <summary>
    /// Наклон планеты
    /// </summary>
    public float obliquity_toOrbit_deg = 23.44f;    //Наклон оси относительно орибты

    public Vector3d position
    {
        get
        {
            return (Vector3d)(transform.position - visualOffset);
        }
        set
        {
            transform.position = (Vector3)value + visualOffset;
        }
    }
    public Vector3d predictedPosition;

    [HideInInspector]
    public Vector3 visualOffset = Vector3.zero;

    public Vector3d velocity = Vector3d.zero;
    public Vector3d initialVelocity = Vector3d.zero;

    public bool drawLine;
    public LineRenderer lineRenderer;

    public bool needToUpdate;
    public Planet reference;

    public PlanetUI planetUI;

    private void OnEnable()
    {
        UpdatePlanet();
    }

    private void OnValidate()
    {
        gameObject.name = planetName;
        needToUpdate = true;
    }

    private void Awake()
    {
        velocity = initialVelocity / Simulation.unitMiniatureM;

        position = (Vector3d)(transform.position - visualOffset);
    }

    public void UpdatePlanet()
    {
        gameObject.name = planetName;
        orbitColor.a = 1;

        if (gameObject.GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<SphereCollider>();

            gameObject.GetComponent<MeshFilter>().sharedMesh = (Mesh)Resources.Load("UVSphere", typeof(Mesh));
        }

        float size = radius_km / Simulation.unitMiniatureKM * 2;
        transform.localScale = new Vector3(size, size, size);
        transform.eulerAngles = new Vector3(0, 0, obliquity_toOrbit_deg);

        if (planetUI == null)
        {
            GameObject g = new GameObject(planetName + "UI", typeof(PlanetUI));
            planetUI = g.GetComponent<PlanetUI>();
            planetUI.gameObject.layer = LayerMask.NameToLayer("UI");
        }

        planetUI.Setup(this);

        if (reference == null)
        {
            WorldManager manager = GameObject.FindObjectOfType<WorldManager>();
            GameObject planet = new GameObject(name + "Planet", typeof(Planet));
            planet.transform.SetParent(manager.transform);
            reference = planet.GetComponent<Planet>();

            planet.AddComponent<MeshRenderer>();
            planet.AddComponent<MeshFilter>();
            planet.AddComponent<SphereCollider>();
            planet.GetComponent<MeshFilter>().sharedMesh = (Mesh)Resources.Load("UVSphere", typeof(Mesh));
        }

        reference.Setup(this);
    }


    public void SetScale(float scaleMultiplier, float maxScale)
    {
        float size = radius_km / Simulation.unitMiniatureKM * scaleMultiplier;
        if(size > maxScale)
        {
            size = maxScale;
        }
        gameObject.transform.localScale = new Vector3(size, size, size);
    }

    public void CheckVisualOverlap()
    {
        transform.position -= visualOffset;
        visualOffset = Vector3.zero;

        Collider[] cols = Physics.OverlapSphere(transform.position, gameObject.transform.localScale.x / 2f, 1 << LayerMask.NameToLayer("MiniaturePlanets"));

        foreach (var c in cols.Where(x => x.transform != this.transform))
        {
            visualOffset.x += (transform.localScale.x / 2f + c.GetComponent<CelestialBody>().transform.localScale.x / 2f) - Vector3.Distance(c.gameObject.transform.position, this.gameObject.transform.position) + transform.localScale.x / 10f;
        }

        transform.position += visualOffset;
    }
    public Vector3d CalculateVelocity(CelestialBody[] allCelestialBodies)
    {
        Vector3d accelDirection = Vector3d.zero;

        foreach (var body in allCelestialBodies)
        {
            if (this != body)
            {
                accelDirection += CalculateVelocity(position, body.position, body.mass, WorldType.Miniature) / Simulation.unitMiniatureM;
            }
        }

        return accelDirection;
    }

    public static Vector3d CalculateVelocity(Vector3d thisPosition, Vector3d otherPosition, double otherMass, WorldType unit)
    {
        otherMass *= Simulation.earthMass;

        Vector3d direction = otherPosition - thisPosition;
        Vector3d forceDir = direction.normalized;

        double distance = 0;
        switch (unit)
        {
            case WorldType.Miniature:
                distance = Simulation.DistanceMiniature(thisPosition, otherPosition);
                break;
            case WorldType.Rocket:
                distance = Simulation.DistanceRocket(thisPosition, otherPosition);
                break;
        }

        double sqrDistance = Math.Pow(distance, 2);

        //force F = G*((m1m2)/r^2)
        //velocity v = F/mass
        Vector3d velocityCalc = Simulation.G * (otherMass / sqrDistance) * forceDir;

        return velocityCalc;
    }


    public void UpdatePosition()
    {
        position += velocity * Simulation.dt;

        transform.Rotate(0, -(360f / sideral_day_seconds) * (Simulation.dt), 0);
 
        //Проверка работоспособности
        //if (name == "Earth")
        //{
        //    if (transform.rotation.y > 0f && ok == true)
        //    {
        //        Debug.Log("Rotate");
        //        ok = false;
        //    }
        //    if(transform.rotation.y < 0f && ok == false)
        //    {
        //        Debug.Log("Rotate");
        //        ok = true;
        //    }
        //}

     //   speed_km_per_second = Vector3d.Distance(prevPosition, position) / Simulation.timeStep * Simulation.unitMiniatureKM;
    }

    public void UpdateVelocity(CelestialBody[] allCelestialBodies)
    {
        velocity += CalculateVelocity(allCelestialBodies) * Simulation.dt;
    }

    public void PredictNextPosition(CelestialBody[] allCelestialBodies)
    {
        Vector3d newVelocity = velocity + CalculateVelocity(allCelestialBodies) * Simulation.dt;
        predictedPosition = position + (newVelocity * Simulation.dt);
    }

    public float CalculateRotationSpeed(float planet_latitude)
    {
        float speedAtEquator = equator_length_km / sidereal_day_hrs;    //кмч
        float speedAtLatitude = Mathf.Cos(Mathf.Deg2Rad * planet_latitude) * speedAtEquator;
        return speedAtLatitude * 0.27777777777778f; //кмч в метры в секунду
    }
}
