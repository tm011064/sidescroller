using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CameraTrolley))]
public class CameraTrolleyEditor : Editor
{
  GUIStyle style = new GUIStyle();
  private CameraTrolley _target;

  void OnEnable()
  {
    style.fontStyle = FontStyle.Bold;
    style.normal.textColor = Color.white;

    _target = (CameraTrolley)target;
  }

  public override void OnInspectorGUI()
  {
    //draw the path?
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Show Gizmo Outline");
    _target.showGizmoOutline = EditorGUILayout.Toggle(_target.showGizmoOutline);
    EditorGUILayout.EndHorizontal();

    //path color:
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Outline Gizmo Color");
    _target.outlineGizmoColor = EditorGUILayout.ColorField(_target.outlineGizmoColor);
    EditorGUILayout.EndHorizontal();
    
    EditorGUILayout.BeginHorizontal();
    _target.nodeCount = Mathf.Max(2, EditorGUILayout.IntField("Node Count", _target.nodeCount));
    EditorGUILayout.EndHorizontal();

    //add node?
    if (_target.nodeCount > _target.nodes.Count)
    {
      for (int i = 0; i < _target.nodeCount - _target.nodes.Count; i++)
      {
        _target.nodes.Add(Vector3.zero);
      }
    }

    //remove node?
    if (_target.nodeCount < _target.nodes.Count)
    {
      if (EditorUtility.DisplayDialog("Remove path node?", "Shortening the node list will permantently destory parts of your path. This operation cannot be undone.", "OK", "Cancel"))
      {
        int removeCount = _target.nodes.Count - _target.nodeCount;
        _target.nodes.RemoveRange(_target.nodes.Count - removeCount, removeCount);
      }
      else
      {
        _target.nodeCount = _target.nodes.Count;
      }
    }

    //node display:
    EditorGUI.indentLevel = 4;
    for (int i = 0; i < _target.nodes.Count; i++)
    {
      _target.nodes[i] = EditorGUILayout.Vector3Field("Node " + (i + 1), _target.nodes[i]);
    }

    //update and redraw:
    if (GUI.changed)
    {
      EditorUtility.SetDirty(_target);
    }
  }

  void OnSceneGUI()
  {
    if (_target.nodes.Count > 0)
    {
      //allow path adjustment undo:
      Undo.RecordObject(_target, "Adjust Linear Path");

      Handles.Label(_target.gameObject.transform.TransformPoint(_target.nodes[0]), "'" + _target.name + "' Begin", style);
      Handles.Label(_target.gameObject.transform.TransformPoint(_target.nodes[_target.nodes.Count - 1]), "'" + _target.name + "' End", style);

      //node handle display:
      for (int i = 0; i < _target.nodes.Count; i++)
      {
        _target.nodes[i] = _target.gameObject.transform.InverseTransformPoint(
          Handles.PositionHandle(_target.gameObject.transform.TransformPoint(_target.nodes[i]), Quaternion.identity));
      }
    }
  }
}