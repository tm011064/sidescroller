using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ManualPingPongPath))]
public class ManualPingPongPathEditor : Editor
{
  GUIStyle style = new GUIStyle();
  private ManualPingPongPath _target;

  void OnEnable()
  {
    style.fontStyle = FontStyle.Bold;
    style.normal.textColor = Color.white;

    _target = (ManualPingPongPath)target;
  }

  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    if (_target.nodeCount < 2)
      _target.nodeCount = 2;

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
      Undo.RecordObject(_target, "Adjust DynamicPingPong Path");

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