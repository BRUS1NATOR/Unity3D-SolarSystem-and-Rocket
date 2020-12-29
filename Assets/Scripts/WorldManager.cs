using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldType { Miniature, Rocket }


public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;
    public static Vector3d worldOffset = Vector3d.zero;

    public static WorldType worldType = WorldType.Rocket;
    public GameObject rocketController;
    public Camera rocketCamera;
    public Camera miniatureCamera;
    //Cosmic world
    //Rocket world

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            DontDestroyOnLoad(this);
            instance = this;
        }
        ChangeWorld(worldType);
    }

    public static void ResetWorldOffset(GameObject centerOfWorld)
    {
        worldOffset -= (Vector3d)(centerOfWorld.transform.position);

        centerOfWorld.transform.position = Vector3.zero;
    }

    public void ChangeWorld(WorldType type)
    {
        worldType = type;
        switch (type)
        {
            case WorldType.Rocket:
                miniatureCamera.enabled = false;
                rocketCamera.enabled = true;
                break;

            case WorldType.Miniature:
                rocketCamera.enabled = false;
                miniatureCamera.enabled = true;
                break;
        }
    }

    //public void HideObject(GameObject g)
    //{
    //    Renderer[] rend = g.GetComponentsInChildren<Renderer>();
    //    foreach(var r in rend)
    //    {
    //        r.enabled = false;
    //    }
    //}
    //public void ShowObject(GameObject g)
    //{
    //    Renderer[] rend = g.GetComponentsInChildren<Renderer>();
    //    foreach (var r in rend)
    //    {
    //        r.enabled = true;
    //    }
    //}

    public static Vector3 MiniatureToCosmic(Vector3 position)
    {
        return position * Simulation.unitMiniatureM / Simulation.unitRocketM;
    }

    public static Vector3d MiniatureToCosmic(Vector3d position)
    {
        return position * Simulation.unitMiniatureM / Simulation.unitRocketM;
    }

    public static Vector3 CosmicToMiniature(Vector3 position)
    {
        return position * Simulation.unitRocketM / Simulation.unitMiniatureM;
    }

    public static Vector3d CosmicToMiniature(Vector3d position)
    {
        return position * Simulation.unitRocketM / Simulation.unitMiniatureM;
    }
}
