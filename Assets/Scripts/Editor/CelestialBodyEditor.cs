using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(CelestialBody))]
public class CelestialBodyEditor : Editor
{
    CelestialBody planet;
    public override void OnInspectorGUI()
    {
        planet = (CelestialBody)target;
        GUILayout.Label($"Equator length: {planet.equator_length_km} kilometers");

        for (int i = 0; i <= 90; i += 10) 
        {
            GUILayout.Label($"Rotation speed at latitude({i}): {planet.CalculateRotationSpeed(i)} m/s");
        }
    
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Update Planet"))
        {
            planet.UpdatePlanet();
        }
    }
}
