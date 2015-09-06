#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class JumpControlledPlatformSwitchGroupWheel : SpawnBucketItemBehaviour
{
  private Vector3 _gizmoCenter = Vector3.zero;
  private Vector3 _gizmoExtents = new Vector3(16, 16, 0);

  public Color outlineGizmoColor = Color.white;
  public bool showGizmoOutline = true;

  void OnDrawGizmos()
  {
    if (showGizmoOutline && platformGroups.Count > 0)
    {
      BoxCollider2D boxCollider2D = null;
      for (int i = 0; i < platformGroups.Count; i++)
      {
        if (platformGroups[i].enabledGameObject != null)
        {
          boxCollider2D = platformGroups[i].enabledGameObject.GetComponent<BoxCollider2D>();
          break;
        }
        if (platformGroups[i].disabledGameObject != null)
        {
          boxCollider2D = platformGroups[i].disabledGameObject.GetComponent<BoxCollider2D>();
          break;
        }
      }

      if (boxCollider2D != null)
      {
        _gizmoCenter = Vector2.zero;
        _gizmoExtents = new Vector3(
          width + boxCollider2D.size.x / 2
          , height + boxCollider2D.size.y / 2
          );
      }

      GizmoUtility.DrawBoundingBox(this.transform.TransformPoint(_gizmoCenter), _gizmoExtents, outlineGizmoColor);
    }
  }
}
#endif