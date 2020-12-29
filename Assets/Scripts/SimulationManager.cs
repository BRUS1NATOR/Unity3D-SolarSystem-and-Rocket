using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Range(2, 100)]
    public int updatesPerSecond = 100;

    private void Start()
    {
        Simulation.updatesPerSecond = updatesPerSecond;
    }

    private void OnValidate()
    {
        Simulation.updatesPerSecond = updatesPerSecond;
    }
}
