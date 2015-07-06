using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ArcEdgeColliderBuildScript))]
public class ArcEdgeColliderBuilderEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    ArcEdgeColliderBuildScript myScript = (ArcEdgeColliderBuildScript)target;
    if (GUILayout.Button("Build Object"))
    {
      myScript.BuildObject();
    }
  }
}
