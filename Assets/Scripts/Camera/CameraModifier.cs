using UnityEngine;
using System.Collections;


public class CameraModifier : MonoBehaviour
{
  public bool useYPosLock = false;
  public float yPosLock;

  public bool useXPosLock = false;
  public float xPosLock;

  public bool allowTopExtension;
  public bool allowBottomExtension;
  public bool allowLeftExtension;
  public bool allowRightExtension;

  public float offsetX;
  public float offsetY;

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
    if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
    {
      var cameraController = Camera.main.GetComponent<CameraController>();
      
      cameraController.SetCameraMovementSettings(new CameraMovementSettings(
        useXPosLock ? (float?)xPosLock : null
        , useYPosLock ? (float?)yPosLock : null
        , allowTopExtension
        , allowBottomExtension
        , allowLeftExtension
        , allowRightExtension
        , offsetX
        , offsetY));
    }
  }
}
