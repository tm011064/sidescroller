using UnityEngine;
using System.Collections;


public class CameraModifier : MonoBehaviour
{
  public bool useYPosLock = false;
  public float yPosLock;

  public bool lockHorizontalCameraMovement = false;
  //public float xPosLock;

  public bool allowTopExtension;
  public bool allowBottomExtension;
  public bool allowLeftExtension;
  public bool allowRightExtension;

  public float offsetX;
  public float offsetY;

  public float zoomPercentage = 1f;
  public float zoomTime;
  public EasingType zoomEasingType;

  public float horizontalSmoothDampTime;
  public float verticalSmoothDampTime;

  public Color gizmoColor = Color.magenta;

  void OnDrawGizmos()
  {
    foreach (var bc in GetComponents<BoxCollider2D>())
    {
      GizmoUtility.DrawBoundingBox(transform.position, bc.bounds.extents, gizmoColor);
      break;
    }
  }

  void OnTriggerEnter2D(Collider2D col)
  {
    var cameraController = Camera.main.GetComponent<CameraController>();

    cameraController.SetCameraMovementSettings(new CameraMovementSettings(
      lockHorizontalCameraMovement ? (float?)this.transform.position.x : null
      , useYPosLock ? (float?)yPosLock : null
      , allowTopExtension
      , allowBottomExtension
      , allowLeftExtension
      , allowRightExtension
      , offsetX
      , offsetY
      , zoomPercentage
      , zoomTime
      , zoomEasingType
      , horizontalSmoothDampTime
      , verticalSmoothDampTime
      ));
  }
}
