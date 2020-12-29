using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Rocket))]
public class RocketEditor : Editor
{
    Rocket rocket;

    public override void OnInspectorGUI()
    {
        rocket = (Rocket)target;
        GUILayout.Label("Overall speed: " + rocket.velocity);
        GUILayout.Label("Overall mass: " + rocket.massOverall + "kg");

        int i = 1;
        foreach (RocketStage s in rocket.stages)
        {
            GUILayout.Label($"Stage №{i} is alive: {!s.dropped}, time left: {s.fuelMass / s.fuelConsamptionPerSecond / rocket.throttle}");
            i++;
        }

        base.OnInspectorGUI();
    }
}
