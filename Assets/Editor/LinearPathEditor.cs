using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LinearPath))]
public class LinearPathEditor : Editor
{
  GUIStyle style = new GUIStyle();
  private LinearPath _target;

  void OnEnable()
  {
    style.fontStyle = FontStyle.Bold;
    style.normal.textColor = Color.white;

    _target = (LinearPath)target;
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
    EditorGUILayout.PrefixLabel("Easing Type");
    _target.easingType = (EasingType)EditorGUILayout.EnumPopup(_target.easingType);
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Game Object To Attach");
    _target.objectToAttach = (GameObject)EditorGUILayout.ObjectField(_target.objectToAttach, typeof(GameObject));
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    _target.totalObjectsOnPath = Mathf.Max(1, EditorGUILayout.IntField("Total Objects On Path", _target.totalObjectsOnPath));
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    _target.time = Mathf.Max(.01f, EditorGUILayout.FloatField("Time", _target.time));
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    _target.startDelayOnEnabled = Mathf.Max(0f, EditorGUILayout.FloatField("Start Delay On Enabled", _target.startDelayOnEnabled));
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.BeginHorizontal();
    _target.delayBetweenCycles = Mathf.Max(0f, EditorGUILayout.FloatField("Delay Between Cycles", _target.delayBetweenCycles));
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Loop Mode");
    _target.loopMode = (LinearPath.LoopMode)EditorGUILayout.EnumPopup(_target.loopMode);
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Moving Platform Type");
    _target.movingPlatformType = (MovingPlatformType)EditorGUILayout.EnumPopup(_target.movingPlatformType);
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Start Position");
    _target.startPosition = (LinearPath.StartPosition)EditorGUILayout.EnumPopup(_target.startPosition);
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Start Path Direction");
    _target.startPathDirection = (LinearPath.StartPathDirection)EditorGUILayout.EnumPopup(_target.startPathDirection);
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