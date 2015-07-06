using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PlatformSlopeBuilderScript))]
public class PlatformSlopeBuilderEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    PlatformSlopeBuilderScript myScript = (PlatformSlopeBuilderScript)target;
    if (GUILayout.Button("Build Object"))
    {
      myScript.BuildObject();
    }
  }
}
