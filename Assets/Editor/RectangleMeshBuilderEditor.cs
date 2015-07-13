using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RectangleMeshBuildScript))]
public class RectangleMeshBuilderEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    RectangleMeshBuildScript myScript = (RectangleMeshBuildScript)target;
    myScript.SafeDeleteColliders();
    
    if (GUILayout.Button("Build Object"))
    {
      myScript.BuildObject();
    }
  }
}