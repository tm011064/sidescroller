﻿#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class Pendulum : SpawnBucketItemBehaviour
{
  private Vector3 _gizmoCenter = Vector3.zero;
  private Vector3 _gizmoExtents = new Vector3(16, 16, 0);

  public Color outlineGizmoColor = Color.white;
  public bool showGizmoOutline = true;

  void OnDrawGizmos()
  {
    if (showGizmoOutline)
    {
        if (floatingAttachedPlatform != null)
        {
          BoxCollider2D boxCollider2D = floatingAttachedPlatform.GetComponent<BoxCollider2D>();
          if (boxCollider2D != null)
          {
            _gizmoCenter = Vector2.zero;
            _gizmoExtents = new Vector3(
              radius + boxCollider2D.size.x / 2
              , radius + boxCollider2D.size.y / 2
              );
        }
      }

      GizmoUtility.DrawBoundingBox(this.transform.TransformPoint(_gizmoCenter), _gizmoExtents, outlineGizmoColor);
    }
  }
}
#endif