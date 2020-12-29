using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public static class Simulation
{
    /// <summary>
    /// Gravitational constant
    /// </summary>
    public const double G = 0.0000000000667;

    public static Vector3 earthGravity = new Vector3(0, -9.81f, 0);
    public static Vector3d earthGravityD = new Vector3d(0, -9.81f, 0);
    public static float earthRadius = 6357;
    public static double earthMass = 5.972 * Math.Pow(10, 24);

    public const float astronomicUnit = 149597870700;

    public static float[] timeSteps = new float[] {       1, 2, 5, 10, 25,
                                                    50, 100, 1000, 5000};

    public static float dt
    {
        get
        {
            return timeStep / updatesPerSecond;
        }
    }
    public static int updatesPerSecond
    {
        get
        {
            return _updatesPerSecond;
        }
        set
        {
            if (value > 1 && value <= 100)
            {
                 Time.fixedDeltaTime = 1f / value;
                _updatesPerSecond = value;
            }
        }
    }
    private static int _updatesPerSecond;
    public static int selectedTimeStep = 0;

    public static float timeStep
    {
        get
        {
            return timeSteps[selectedTimeStep];
        }
    }

    public const int unitMiniatureKM = 100000;   //1unit = 100km
    public const int unitMiniatureM = unitMiniatureKM * 1000;   //1unit = 100.000.000m

    public const int unitRocketKM = 1;   //1unit = 1km
    public const int unitRocketM = unitRocketKM * 1000; //1unit = 1000m

    public static float DistanceMiniature(Vector3 body, Vector3 anotherBody)
    {
        return Vector3.Distance(body, anotherBody) * unitMiniatureM;
    }

    public static double DistanceMiniature(Vector3d body, Vector3d anotherBody)
    {
        return Vector3d.Distance(body, anotherBody) * unitMiniatureM;
    }

    public static float DistanceRocket(Vector3 body, Vector3 anotherBody)
    {
        return Vector3.Distance(body, anotherBody) * unitRocketM;
    }

    public static double DistanceRocket(Vector3d body, Vector3d anotherBody)
    {
        return Vector3d.Distance(body, anotherBody) * unitRocketM;
    }

    public static double DistanceToPlanet(Vector3 body, Planet planet)
    {
        return (Vector3.Distance(body, planet.transform.position) - planet.transform.localScale.x / 2f);
    }
}
