using UnityEngine;
using System.Collections;
using System;

public enum VerticalCameraFollowMode
{
  FollowAlways,
  FollowWhenGrounded,
}

[Serializable]
public class VerticalLockSettings
{
  public bool enabled = false;

  public bool enableDefaultHorizontalLockPosition = true;
  public float defaultHorizontalLockPosition = 540f;
  public float translatedHorizontalLockPosition;

  public bool enableTopHorizontalLock = false;
  public float topHorizontalLockPosition = 1080f;

  public bool enableBottomHorizontalLock = false;
  public float bottomHorizontalLockPosition = 0f;

  [HideInInspector]
  public float topBoundary;
  [HideInInspector]
  public float bottomBoundary;

  public override string ToString()
  {
    return string.Format("enabled: {0}; enableTopHorizontalLock: {1}; topHorizontalLockPosition: {2}; topBoundary: {3}; enableBottomHorizontalLock: {4}; bottomHorizontalLockPosition: {5}; bottomBoundary: {6};"
      , enabled
      , enableTopHorizontalLock
      , topHorizontalLockPosition
      , topBoundary
      , enableBottomHorizontalLock
      , bottomHorizontalLockPosition
      , bottomBoundary
      );
  }
}
[Serializable]
public class HorizontalLockSettings
{
  public bool enabled = false;

  public bool enableRightHorizontalLock = true;
  public float rightHorizontalLockPosition = 1920f;

  public bool enableLeftHorizontalLock = true;
  public float leftHorizontalLockPosition = 0f;

  [HideInInspector]
  public float rightBoundary;
  [HideInInspector]
  public float leftBoundary;

  public override string ToString()
  {
    return string.Format("enabled: {0}; enableRightHorizontalLock: {1}; rightHorizontalLockPosition: {2}; rightBoundary: {3}; enableLeftHorizontalLock: {4}; leftHorizontalLockPosition: {5}; leftBoundary: {6};"
      , enabled
      , enableRightHorizontalLock
      , rightHorizontalLockPosition
      , rightBoundary
      , enableLeftHorizontalLock
      , leftHorizontalLockPosition
      , leftBoundary
      );
  }
}

[Serializable]
public class ZoomSettings
{
  public float zoomPercentage = 1f;
  public float zoomTime;
  public EasingType zoomEasingType;

  public override string ToString()
  {
    return string.Format("zoomPercentage: {0}; zoomTime: {1}; zoomEasingType: {2};"
      , zoomPercentage
      , zoomTime
      , zoomEasingType
      );
  }
}

[Serializable]
public class SmoothDampMoveSettings
{
  public float horizontalSmoothDampTime = .2f;
  public float verticalSmoothDampTime = .2f;
  public float verticalRapidDescentSmoothDampTime = .01f;
  public float verticalAboveRapidAcsentSmoothDampTime = .2f;

  public override string ToString()
  {
    return string.Format("horizontalSmoothDampTime: {0}; verticalSmoothDampTime: {1}; verticalRapidDescentSmoothDampTime: {2}; verticalAboveRapidAcsentSmoothDampTime: {3};"
      , horizontalSmoothDampTime
      , verticalSmoothDampTime
      , verticalRapidDescentSmoothDampTime
      , verticalAboveRapidAcsentSmoothDampTime
      );
  }
}

public class CameraModifier : MonoBehaviour
{
  public VerticalLockSettings verticalLockSettings;
  public HorizontalLockSettings horizontalLockSettings;
  public ZoomSettings zoomSettings;
  public SmoothDampMoveSettings smoothDampMoveSettings;

  public Vector2 offset;

  public VerticalCameraFollowMode verticalCameraFollowMode;

  public Color gizmoColor = Color.magenta;

  public GameObject parentPositionObject;

  private CameraController _cameraController;

  void OnDrawGizmos()
  {
    foreach (var bc in GetComponents<BoxCollider2D>())
    {
      GizmoUtility.DrawBoundingBox(transform.position, bc.bounds.extents, gizmoColor);
      break;
    }
  }

  void Start()
  {
    _cameraController = Camera.main.GetComponent<CameraController>();
  }

  void OnTriggerEnter2D(Collider2D col)
  {
    Vector3 transformPoint = parentPositionObject.transform.TransformPoint(Vector3.zero);
    //Debug.Log(this.transform.localPosition);
    Debug.Log(transformPoint);
    if (this.verticalLockSettings.enabled)
    {
      if (this.verticalLockSettings.enableTopHorizontalLock)
        this.verticalLockSettings.topBoundary = transformPoint.y + this.verticalLockSettings.topHorizontalLockPosition - _cameraController.targetScreenSize.y * .5f;
      if (this.verticalLockSettings.enableBottomHorizontalLock)
        this.verticalLockSettings.bottomBoundary = transformPoint.y + this.verticalLockSettings.bottomHorizontalLockPosition + _cameraController.targetScreenSize.y * .5f;
    }

    if (this.horizontalLockSettings.enabled)
    {
      if (this.horizontalLockSettings.enableLeftHorizontalLock)
        this.horizontalLockSettings.leftBoundary = transformPoint.x + this.horizontalLockSettings.leftHorizontalLockPosition + _cameraController.targetScreenSize.x * .5f;
      if (this.horizontalLockSettings.enableRightHorizontalLock)
        this.horizontalLockSettings.rightBoundary = transformPoint.x + this.horizontalLockSettings.rightHorizontalLockPosition - _cameraController.targetScreenSize.x * .5f;
    }

    this.verticalLockSettings.translatedHorizontalLockPosition = transformPoint.y + this.verticalLockSettings.defaultHorizontalLockPosition;

    CameraMovementSettings cameraMovementSettings = new CameraMovementSettings(
      verticalLockSettings
      , horizontalLockSettings
      , zoomSettings
      , smoothDampMoveSettings
      , offset
      , verticalCameraFollowMode
      );
    Debug.Log(cameraMovementSettings);
    var cameraController = Camera.main.GetComponent<CameraController>();

    cameraController.SetCameraMovementSettings(cameraMovementSettings);
  }
}
