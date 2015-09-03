using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(JumpControlledPlatformSwitchGroup))]
public class JumpControlledPlatformSwitchGroupEditor : Editor
{
  GUIStyle style = new GUIStyle();
  private JumpControlledPlatformSwitchGroup _target;

  void OnEnable()
  {
    style.fontStyle = FontStyle.Bold;
    style.normal.textColor = Color.white;

    _target = (JumpControlledPlatformSwitchGroup)target;
  }

  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    foreach (var item in _target.platformGroupPositions)
    {
      if (item.positions.Count < 1)
        item.positions.Add(Vector3.zero);
    }
    
    //update and redraw:
    if (GUI.changed)
    {
      EditorUtility.SetDirty(_target);
    }
  }

  void OnSceneGUI()
  {
    foreach (var item in _target.platformGroupPositions)
    {
      //allow path adjustment undo:
      Undo.RecordObject(_target, "Adjust JumpControlledPlatformSwitchGroup");
      
      //node handle display:
      for (int i = 0; i < item.positions.Count; i++)
      {
        item.positions[i] = _target.gameObject.transform.InverseTransformPoint(
          Handles.PositionHandle(_target.gameObject.transform.TransformPoint(item.positions[i]), Quaternion.identity));
      }
    }
  }
}