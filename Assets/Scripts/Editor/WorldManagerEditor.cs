using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(WorldManager))]
public class WorldManagerEditor : Editor
{
    WorldManager mgr;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        mgr = (WorldManager)target;

        GUILayout.Label("World Offset: " + WorldManager.worldOffset);

        if(GUILayout.Button("Change World"))
        {
            if(WorldManager.worldType == WorldType.Rocket)
            {
                mgr.ChangeWorld(WorldType.Miniature);
            }
            else
            {
                mgr.ChangeWorld(WorldType.Rocket);
            }
        }
    }
}
