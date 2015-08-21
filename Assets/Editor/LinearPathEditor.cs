// Copyright (c) 2010 Bob Berkebile
// Please direct any bugs/comments/suggestions to http://www.pixelplacement.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LinearPath))]
public class LinearPathEditor : Editor
{
  GUIStyle style = new GUIStyle();
  /*   
  public List<Vector3> nodes = new List<Vector3>() { Vector3.zero, Vector3.zero };
  public int nodeCount;

  public Color outlineGizmoColor = Color.white;
  public bool showGizmoOutline = true;
   */
  private LinearPath _target;

  void OnEnable()
  {
    style.fontStyle = FontStyle.Bold;
    style.normal.textColor = Color.white;

    _target = (LinearPath)target;
  }

  /*
   
  public EasingType easingType = EasingType.Linear;

  public GameObject objectToAttach;
  public int totalObjectsOnPath = 1;
  public LoopMode loopMode = LoopMode.Once;

  public float time = 5f;
  public MovingPlatformType movingPlatformType = MovingPlatformType.StartsWhenPlayerLands;
  public StartPosition startPosition = StartPosition.Center;
  public StartPathDirection startPathDirection = StartPathDirection.Forward;
   */

  public override void OnInspectorGUI()
  {
    //draw the path?
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("Show Gizmo Outline");
    _target.showGizmoOutline = EditorGUILayout.Toggle(_target.showGizmoOutline);
    EditorGUILayout.EndHorizontal();

    //path name:
    //EditorGUILayout.BeginHorizontal();
    //EditorGUILayout.PrefixLabel("Path Name");
    //_target.pathName = EditorGUILayout.TextField(_target.pathName);
    //EditorGUILayout.EndHorizontal();

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