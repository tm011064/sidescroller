using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CameraTrolley))]
public class CameraTrolleyEditor : Editor
{
  GUIStyle style = new GUIStyle();
  private CameraTrolley _target;
  private int _selectedHandleIndex = -1;

  void OnEnable()
  {
    style.fontStyle = FontStyle.Bold;
    style.normal.textColor = Color.white;

    _target = (CameraTrolley)target;
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
        _target.nodes.Add(new Vector3(
          _target.nodes[_target.nodes.Count - 1].x + 10f
          , _target.nodes[_target.nodes.Count - 1].y
          , _target.nodes[_target.nodes.Count - 1].z));
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

      int hashCode = GetHashCode();

      //node handle display:
      for (int i = 0; i < _target.nodes.Count; i++)
      {
        int controlIDBeforeHandle = GUIUtility.GetControlID(hashCode, FocusType.Passive);
        bool isEventUsedBeforeHandle = (Event.current.type == EventType.used);


        _target.nodes[i] = _target.gameObject.transform.InverseTransformPoint(
          Handles.PositionHandle(_target.gameObject.transform.TransformPoint(_target.nodes[i]), Quaternion.identity));

        int controlIDAfterHandle = GUIUtility.GetControlID(hashCode, FocusType.Passive);
        bool isEventUsedByHandle = !isEventUsedBeforeHandle && (Event.current.type == EventType.used);

        if ((controlIDBeforeHandle < GUIUtility.hotControl && GUIUtility.hotControl < controlIDAfterHandle) || isEventUsedByHandle)
        {
          _selectedHandleIndex = i;
        }
      }
    }

    Event e = Event.current;
    switch (e.type)
    {
      case EventType.keyDown:
        if (Event.current.keyCode == (KeyCode.Delete))
        {
          Debug.Log(_selectedHandleIndex);
          if (_selectedHandleIndex >= 0)
          {
            e.Use();

            _target.nodes.RemoveAt(_selectedHandleIndex);
            _target.nodeCount = _target.nodes.Count;
          }
        }
        break;

    }
  }
}