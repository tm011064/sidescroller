using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MultiWayCameraModifier))]
public class MultiWayCameraModifierEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    MultiWayCameraModifier myScript = (MultiWayCameraModifier)target;
    
    if (GUILayout.Button("Build Object"))
    {
      myScript.BuildObject();
    }
  }
}