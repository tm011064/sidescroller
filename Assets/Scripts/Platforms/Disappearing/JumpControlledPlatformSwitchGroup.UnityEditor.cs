#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class JumpControlledPlatformSwitchGroup : SpawnBucketItemBehaviour
{
  void OnDrawGizmos()
  {
    for (int i = 0; i < platformGroupPositions.Count; i++)
    {
      if (platformGroupPositions[i].showGizmoOutline)
      {
        Vector3 gizmoCenter = Vector3.zero;
        Vector3 gizmoExtents = new Vector3(16, 16, 0);

        if (platformGroupPositions[i].enabledGameObject != null)
        {
          for (int j = 0; j < platformGroupPositions[i].positions.Count; j++)
          {
            RectangleMeshBuildScript rectangleMeshBuildScript = platformGroupPositions[i].enabledGameObject.GetComponent<RectangleMeshBuildScript>();
            if (rectangleMeshBuildScript != null)
            {
              switch (rectangleMeshBuildScript.anchor)
              {
                case TextAnchor.MiddleCenter:
                  gizmoCenter = Vector3.zero;
                  gizmoExtents = new Vector3(rectangleMeshBuildScript.width / 2, rectangleMeshBuildScript.height / 2);
                  break;

                case TextAnchor.LowerLeft:
                  gizmoCenter = new Vector3(rectangleMeshBuildScript.width / 2, rectangleMeshBuildScript.height / 2);
                  gizmoExtents = new Vector3(rectangleMeshBuildScript.width / 2, rectangleMeshBuildScript.height / 2);
                  break;

                default:
                  throw new ArgumentException("rectangleMeshBuildScript anchor " + rectangleMeshBuildScript.anchor + " not supported.");
              }
            }
            else
            {
              BoxCollider2D boxCollider2D = platformGroupPositions[i].enabledGameObject.GetComponent<BoxCollider2D>();
              if (boxCollider2D != null)
              {
                gizmoCenter = boxCollider2D.offset;
                gizmoExtents = boxCollider2D.bounds.extents;
              }
            }

            GizmoUtility.DrawBoundingBox(this.transform.TransformPoint(platformGroupPositions[i].positions[j] + gizmoCenter), gizmoExtents, platformGroupPositions[i].outlineGizmoColor);
          }
        }
      }
    }
  }
}
#endif