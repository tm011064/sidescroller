#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraTrolley : MonoBehaviour
{
  public Color outlineGizmoColor = Color.white;
  public bool showGizmoOutline = true;

  public Color viewPortOutlineGizmoColor = Color.cyan;
  public bool showViewPortOutlineGizmoColor = true;

  private CameraController _cameraController;
  private Vector3 _verticalCameraBorder;

  void OnDrawGizmos()
  {
    if (_cameraController == null)
    {
      _cameraController = FindObjectOfType<CameraController>();
      _verticalCameraBorder = new Vector3(0f, _cameraController.targetScreenSize.y * .5f, 0f);
    }

    if (showGizmoOutline)
    {
      for (int i = 1; i < nodes.Count; i++)
      {
        Gizmos.color = outlineGizmoColor;
        Gizmos.DrawLine(this.gameObject.transform.TransformPoint(nodes[i - 1]), this.gameObject.transform.TransformPoint(nodes[i]));

        Gizmos.color = viewPortOutlineGizmoColor;
        Gizmos.DrawLine(this.gameObject.transform.TransformPoint(nodes[i - 1]) + _verticalCameraBorder, this.gameObject.transform.TransformPoint(nodes[i]) + _verticalCameraBorder);
        Gizmos.DrawLine(this.gameObject.transform.TransformPoint(nodes[i - 1]) - _verticalCameraBorder, this.gameObject.transform.TransformPoint(nodes[i]) - _verticalCameraBorder);
        
      }
    }
  }
}
#endif