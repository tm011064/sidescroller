#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RotatingPlatformCollisionController : MonoBehaviour
{
  private Vector3 _gizmoCenter = Vector3.zero;
  private Vector3 _gizmoExtents = new Vector3(16, 16, 0);

  public Color outlineGizmoColor = Color.yellow;
  public bool showGizmoOutline = true;

  private bool _areGizmosInitialized = false;

  void OnDrawGizmos()
  {
    if (showGizmoOutline)
    {
      if (!_areGizmosInitialized)
      {
        if (rotatingObject != null)
        {
          BoxCollider2D boxCollider2D = rotatingObject.GetComponent<BoxCollider2D>();
          if (boxCollider2D != null)
          {
            _gizmoCenter = boxCollider2D.offset;
            _gizmoExtents = boxCollider2D.size/2;
          }
        }

        _areGizmosInitialized = true;
      }

      GizmoUtility.DrawBoundingBox(this.transform.TransformPoint(_gizmoCenter), _gizmoExtents, outlineGizmoColor);
    }
  }
}
#endif