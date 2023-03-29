using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(ZombieHealth))]
public class ZombieHealthEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ZombieHealth myScript = (ZombieHealth)target;
        if (GUILayout.Button("Kill"))
        {
            myScript.Die();
        }
        if (GUILayout.Button("Spawn"))
        {
            myScript.Spawn();
        }
    }
}
#endif
