using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

public class CelestiaBodiesManager : MonoBehaviour
{
    public static CelestiaBodiesManager instance;

    public Rocket rocket;
    public CelestialBody[] celestialBodies;

    private OrbitDisplay orbitDisplay;

    private void Awake()
    {
        Debug.Log(Time.fixedDeltaTime);

        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            DontDestroyOnLoad(this);
            instance = this;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode && orbitDisplay != null)
        {
            orbitDisplay.Setup(true);
        }
    }

    void OnValidate()
    {
        int bodiesLength = celestialBodies != null ? celestialBodies.Length : 0;

        if (bodiesLength != FindObjectsOfType<CelestialBody>().Length)
        {
            Debug.Log("New celestials detected.");
            celestialBodies = FindObjectsOfType<CelestialBody>();
            celestialBodies = celestialBodies.OrderBy(x => x.gameObject.transform.position.x).ToArray();

            int id = 0;
            foreach (var b in celestialBodies)
            {
                b.bodyID = id;
                id++;
            }
        }

        orbitDisplay = GetComponent<OrbitDisplay>();
    }

    //MAIN PHYSICS
    private void FixedUpdate()
    {
        for (int i = 0; i < celestialBodies.Length; i++)
        {
            celestialBodies[i].UpdateVelocity(celestialBodies);
        }

        for (int i = 0; i < celestialBodies.Length; i++)
        {
            celestialBodies[i].UpdatePosition();
            celestialBodies[i].reference.UpdatePlanetPosition();
        }

        for (int i = 0; i < celestialBodies.Length; i++)
        {
            celestialBodies[i].PredictNextPosition(celestialBodies);
        }

        rocket.UpdatePosition();
    }

    private void LateUpdate()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        foreach (var body in celestialBodies)
        {
            var UI = body.planetUI;
            UI.UpdateUI();
        }
    }
}