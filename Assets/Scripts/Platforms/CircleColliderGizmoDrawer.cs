using UnityEngine;

public class CircleColliderGizmoDrawer : BaseMonoBehaviour
{
  public Color outlineGizmoColor = Color.white;
  public Color outlineVisibilityMaskGizmoColor = Color.magenta;
  public bool showGizmoOutline = true;
  public bool showCameraGizmoOutline = true;

#if UNITY_EDITOR
  private CircleCollider2D _circleCollider = null;

  void OnDrawGizmos()
  {
    if (_circleCollider == null)
      _circleCollider = this.GetComponent<CircleCollider2D>();

    if (_circleCollider == null)
      throw new MissingComponentException();

    UnityEditor.Handles.DrawWireDisc(this.transform.position, Vector3.forward, _circleCollider.radius);
  }
#endif
}