#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class Wheel : BaseMonoBehaviour
{
  public Color outlineGizmoColor = Color.white;
  public Color outlineVisibilityMaskGizmoColor = Color.magenta;
  public bool showGizmoOutline = true;
  public bool showCameraGizmoOutline = true;

  private BoxCollider2D _platformCollider;

  void OnDrawGizmos()
  {
    if (_platformCollider == null)
    {
      _platformCollider = floatingAttachedPlatform.GetComponent<BoxCollider2D>();
      if (_platformCollider == null)
        throw new MissingComponentException("When attaching a platform to a Wheel, the platform must contain a BoxCollider2D for visibility checks.");
    }

    if (showCameraGizmoOutline)
    {
      GizmoUtility.DrawBoundingBox(transform.position, new Vector3(
        (radius + platformWidth)
        , (radius + platformHeight)
        , 0f
      ), outlineVisibilityMaskGizmoColor);
    }

    if (showGizmoOutline)
    {
      GizmoUtility.DrawBoundingBox(transform.position, new Vector3(
        (radius + platformWidth / 2)
        , (radius + platformHeight / 2)
        , 0f
      ), outlineGizmoColor);
    }
  }
}
#endif