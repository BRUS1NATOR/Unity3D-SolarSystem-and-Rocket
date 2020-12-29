using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitDisplay))]
public class OrbitDisplayEditor : Editor
{
    OrbitDisplay manager;

    public override void OnInspectorGUI()
    {
        manager = (OrbitDisplay)target;

        if (GUILayout.Button("Simulate"))
        {
            manager.Setup(true);
        }

        base.OnInspectorGUI();

        string time = "Time speed x" + manager.timeStep.ToString();
        GUILayout.Label(time);

        time = "Years= " + (29.8f * manager.timeStep * manager.numSteps / 940000000);
        GUILayout.Label(time);
        time = "Days = " + (29.8f * manager.timeStep * manager.numSteps / 940000000 * 365);
        GUILayout.Label(time);
        time = "Hours = " + (29.8f * manager.timeStep * manager.numSteps / 940000000 * 365 * 24);
        GUILayout.Label(time);

        foreach(var d in manager.distances)
        {
            GUILayout.Label($"{d.planetName} \t MIN: {Math.Round(d.minDistance,1)} * 10^6 km; \t MAX: {Math.Round(d.maxDistance,1)} * 10^6 km");
        }
    }
}
