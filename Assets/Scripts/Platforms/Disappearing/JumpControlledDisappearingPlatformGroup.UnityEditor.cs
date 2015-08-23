#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class JumpControlledDisappearingPlatformGroup : MonoBehaviour
{
  private Vector3 _gizmoCenter = Vector3.zero;
  private Vector3 _gizmoExtents = new Vector3(16, 16, 0);

  public Color outlineGizmoColor = Color.yellow;
  public bool showGizmoOutline = true;

  void OnDrawGizmos()
  {
    if (showGizmoOutline)
    {
      if (platformPrefab != null)
      {
        RectangleMeshBuildScript rectangleMeshBuildScript = platformPrefab.GetComponent<RectangleMeshBuildScript>();
        if (rectangleMeshBuildScript != null)
        {
          switch (rectangleMeshBuildScript.anchor)
          {
            case TextAnchor.MiddleCenter:
              _gizmoCenter = Vector3.zero;
              _gizmoExtents = new Vector3(rectangleMeshBuildScript.width / 2, rectangleMeshBuildScript.height / 2);
              break;

            case TextAnchor.LowerLeft:
              _gizmoCenter = new Vector3(rectangleMeshBuildScript.width / 2, rectangleMeshBuildScript.height / 2);
              _gizmoExtents = new Vector3(rectangleMeshBuildScript.width / 2, rectangleMeshBuildScript.height / 2);
              break;

            default:
              throw new ArgumentException("rectangleMeshBuildScript anchor " + rectangleMeshBuildScript.anchor + " not supported.");
          }
        }
        else
        {
          BoxCollider2D boxCollider2D = platformPrefab.GetComponent<BoxCollider2D>();
          if (boxCollider2D != null)
          {
            _gizmoCenter = boxCollider2D.offset;
            _gizmoExtents = boxCollider2D.bounds.extents;
          }
        }
      }

      for (int i = 0; i < platformPositions.Count; i++)
      {
        GizmoUtility.DrawBoundingBox(this.transform.TransformPoint(platformPositions[i] + _gizmoCenter), _gizmoExtents, outlineGizmoColor);
      }
    }
  }
}
#endif