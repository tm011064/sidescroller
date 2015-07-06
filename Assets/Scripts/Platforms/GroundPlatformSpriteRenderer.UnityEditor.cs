using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
public partial class GroundPlatformSpriteRenderer : BasePlatform
{
  public Color outlineGizmoColor = Color.white;
  public Color outlineVisibilityMaskGizmoColor = Color.magenta;
  public bool showGizmoOutline = true;
  public bool showCameraGizmoOutline = true;

  void OnDrawGizmos()
  {
    if (_physicsCollider == null || _visibilityCollider == null)
      Awake();
    
    if ((int)_physicsCollider.bounds.extents.x != width
      || (int)_physicsCollider.bounds.extents.y != height)
    {
      _physicsCollider.size = new Vector2(width, height);
    }
    if ((int)_visibilityCollider.bounds.extents.x != width
      || (int)_visibilityCollider.bounds.extents.y != height)
    {
      _visibilityCollider.size = new Vector2(width + Screen.width / 2, height + Screen.height / 2); // add some padding
    }

    if (showCameraGizmoOutline)
      GizmoUtility.DrawBoundingBox(transform, _visibilityCollider.bounds.extents, outlineVisibilityMaskGizmoColor);
    if (showGizmoOutline)
      GizmoUtility.DrawBoundingBox(transform, _physicsCollider.bounds.extents, outlineGizmoColor);
  }
}
#endif