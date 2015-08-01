using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MultiWayCameraModifier))]
[CanEditMultipleObjects]
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
    if (GUILayout.Button("Import Settings"))
    {
      myScript.ImportSettings();
    }
  }
}


[CustomEditor(typeof(CameraModifier))]
public class CameraModifierEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    CameraModifier myScript = (CameraModifier)target;

    if (GUILayout.Button("Import Settings"))
    {
      myScript.ImportSettings();
    }
  }
}
