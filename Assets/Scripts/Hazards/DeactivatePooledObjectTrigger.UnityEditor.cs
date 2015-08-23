#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DeactivatePooledObjectTrigger : MonoBehaviour
{
  private Vector3 _gizmoCenter = Vector3.zero;
  private Vector3 _gizmoExtents = new Vector3(16, 16, 0);

  public Color outlineGizmoColor = Color.magenta;
  public bool showGizmoOutline = true;

  private BoxCollider2D _boxCollider2D;

  void OnDrawGizmos()
  {
    if (showGizmoOutline)
    {
      _boxCollider2D = this.GetComponent<BoxCollider2D>();

      if (_boxCollider2D != null)
      {
        _gizmoCenter = _boxCollider2D.offset;
        _gizmoExtents = _boxCollider2D.size / 2;
      }
      GizmoUtility.DrawBoundingBox(this.transform.TransformPoint(_gizmoCenter), _gizmoExtents, outlineGizmoColor);
    }
  }
}
#endif