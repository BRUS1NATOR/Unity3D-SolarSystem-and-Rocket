using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimulationManager))]
public class SimulationManagerEditor : Editor
{
    SimulationManager manager;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GUILayout.Label("dt: " + Simulation.dt);

        string[] values = new string[Simulation.timeSteps.Length];
        for (int i = 0; i < values.Length; i++)
        {
            if (Simulation.timeSteps[i] == 0.01f)
            {
                values[i] = "x" + Simulation.timeSteps[i].ToString() + "(realtime)";
            }
            else
            {
                values[i] = "x" + Simulation.timeSteps[i].ToString();
            }
        }
        Simulation.selectedTimeStep = EditorGUILayout.Popup("Time Step", Simulation.selectedTimeStep, values);


        manager = (SimulationManager)target;

        string time = "Time speed x" + Simulation.timeStep.ToString();
        GUILayout.Label(time);

        time = "One year = " + 365 / Simulation.timeStep + " day(s)";
        GUILayout.Label(time);


        time = "One year = " + 365 * 24 / Simulation.timeStep + " hour(s)";
        GUILayout.Label(time);


        time = "One year = " + 365 * 24 * 60 / Simulation.timeStep + " minute(s)";
        GUILayout.Label(time);


        time = "One year = " + 365 * 24 * 60 * 60 / Simulation.timeStep + " second(s)";
        GUILayout.Label(time);
    }
}

