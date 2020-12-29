using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    Planet planet;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        planet = (Planet)target;

        GUILayout.Label("Velocity: " + planet.velocity);
    }
}
