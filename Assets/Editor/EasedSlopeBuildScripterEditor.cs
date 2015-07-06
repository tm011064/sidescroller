using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(EasedSlopeBuildScript))]
public class EasedSlopeBuildScripterEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    EasedSlopeBuildScript myScript = (EasedSlopeBuildScript)target;
    if (GUILayout.Button("Build Object"))
    {
      myScript.BuildObject();
    }
  }
}