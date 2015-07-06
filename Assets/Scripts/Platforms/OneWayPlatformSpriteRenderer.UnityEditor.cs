using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
public partial class OneWayPlatformSpriteRenderer : BasePlatform
{
  public Color outlineGizmoColor = Color.white;
  public Color outlineVisibilityMaskGizmoColor = Color.magenta;
  public bool showGizmoOutline = true;
  public bool showCameraGizmoOutline = true;

  void OnDrawGizmos()
  {
    if (_physicsCollider == null || _visibilityCollider == null)
      Awake();

    if (showCameraGizmoOutline)
      GizmoUtility.DrawBoundingBox(transform, _visibilityCollider.bounds.extents, outlineVisibilityMaskGizmoColor);
    if (showGizmoOutline)
      GizmoUtility.DrawBoundingBox(transform, new Vector3(width/2, height/2), outlineGizmoColor);
  }
}
#endif