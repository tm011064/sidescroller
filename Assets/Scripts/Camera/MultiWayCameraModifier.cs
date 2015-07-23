using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(EdgeCollider2D))]
public partial class MultiWayCameraModifier : MonoBehaviour
{
  [Serializable]
  public class MultiWayCameraModificationSetting
  {
    public VerticalLockSettings verticalLockSettings;
    public HorizontalLockSettings horizontalLockSettings;
    public ZoomSettings zoomSettings;
    public SmoothDampMoveSettings smoothDampMoveSettings;

    [Tooltip("The (x, y) offset of the camera. This can be used when default vertical locking is disabled and you want the player to be below, above, right or left of the screen center.")]
    public Vector2 offset;

    public VerticalCameraFollowMode verticalCameraFollowMode;
  }

  public MultiWayCameraModificationSetting redCameraModificationSettings = new MultiWayCameraModificationSetting();
  public MultiWayCameraModificationSetting greenCameraModificationSettings = new MultiWayCameraModificationSetting();

  public float edgeColliderLength = 256f;
  public float edgeColliderAngle = 0f;

  [Tooltip("All lock positions are relative to this object.")]
  public GameObject parentPositionObject;

  private CameraController _cameraController;
  private EdgeCollider2D _edgeCollider;

  private MultiWayCameraModificationSetting _lastMultiWayCameraModificationSetting;

  void Start()
  {
    _cameraController = Camera.main.GetComponent<CameraController>();
    _edgeCollider = this.GetComponent<EdgeCollider2D>();
  }

  private bool IsLeft(Vector2 lineFrom, Vector2 lineTo, Vector2 point)
  {
    return ((lineTo.x - lineFrom.x) * (point.y - lineFrom.y) - (lineTo.y - lineFrom.y) * (point.x - lineFrom.x)) > 0;
  }

  private void SetCameraModificationSettings(MultiWayCameraModificationSetting multiWayCameraModificationSetting)
  {
    Vector3 transformPoint = parentPositionObject.transform.TransformPoint(Vector3.zero);

    if (multiWayCameraModificationSetting.zoomSettings.zoomPercentage == 0f)
      throw new ArgumentOutOfRangeException("Zoom Percentage must not be 0.");

    if (multiWayCameraModificationSetting.verticalLockSettings.enabled)
    {
      if (multiWayCameraModificationSetting.verticalLockSettings.enableTopVerticalLock)
        multiWayCameraModificationSetting.verticalLockSettings.topBoundary = transformPoint.y + multiWayCameraModificationSetting.verticalLockSettings.topVerticalLockPosition - _cameraController.targetScreenSize.y * .5f / multiWayCameraModificationSetting.zoomSettings.zoomPercentage;
      if (multiWayCameraModificationSetting.verticalLockSettings.enableBottomVerticalLock)
        multiWayCameraModificationSetting.verticalLockSettings.bottomBoundary = transformPoint.y + multiWayCameraModificationSetting.verticalLockSettings.bottomVerticalLockPosition + _cameraController.targetScreenSize.y * .5f / multiWayCameraModificationSetting.zoomSettings.zoomPercentage;
    }

    if (multiWayCameraModificationSetting.horizontalLockSettings.enabled)
    {
      if (multiWayCameraModificationSetting.horizontalLockSettings.enableLeftHorizontalLock)
        multiWayCameraModificationSetting.horizontalLockSettings.leftBoundary = transformPoint.x + multiWayCameraModificationSetting.horizontalLockSettings.leftHorizontalLockPosition + _cameraController.targetScreenSize.x * .5f / multiWayCameraModificationSetting.zoomSettings.zoomPercentage;
      if (multiWayCameraModificationSetting.horizontalLockSettings.enableRightHorizontalLock)
        multiWayCameraModificationSetting.horizontalLockSettings.rightBoundary = transformPoint.x + multiWayCameraModificationSetting.horizontalLockSettings.rightHorizontalLockPosition - _cameraController.targetScreenSize.x * .5f / multiWayCameraModificationSetting.zoomSettings.zoomPercentage;
    }

    multiWayCameraModificationSetting.verticalLockSettings.translatedVerticalLockPosition = transformPoint.y + multiWayCameraModificationSetting.verticalLockSettings.defaultVerticalLockPosition;

    CameraMovementSettings cameraMovementSettings = new CameraMovementSettings(
      multiWayCameraModificationSetting.verticalLockSettings
      , multiWayCameraModificationSetting.horizontalLockSettings
      , multiWayCameraModificationSetting.zoomSettings
      , multiWayCameraModificationSetting.smoothDampMoveSettings
      , multiWayCameraModificationSetting.offset
      , multiWayCameraModificationSetting.verticalCameraFollowMode
      );

    var cameraController = Camera.main.GetComponent<CameraController>();

    cameraController.SetCameraMovementSettings(cameraMovementSettings);

    _lastMultiWayCameraModificationSetting = multiWayCameraModificationSetting;
    Logger.Info("Applied " + (multiWayCameraModificationSetting == redCameraModificationSettings ? "red" : "green") + " camera setting for camera modifier " + this.name);
  }

  void OnTriggerExit2D(Collider2D col)
  {
    // left is red
    if (IsLeft(_edgeCollider.points[0], _edgeCollider.points[1], transform.InverseTransformPoint(col.gameObject.transform.position)))
    {
      // we exit from to the left, meaning that we came from green and go to red
      if (_lastMultiWayCameraModificationSetting != redCameraModificationSettings)
      {
        SetCameraModificationSettings(redCameraModificationSettings);
      }
    }
    else
    {
      // we exit from to the right, meaning that we came from red and go to green
      if (_lastMultiWayCameraModificationSetting != greenCameraModificationSettings)
      {
        SetCameraModificationSettings(greenCameraModificationSettings);
      }
    }
  }

  void OnTriggerEnter2D(Collider2D col)
  {
    // left is red
    if (IsLeft(_edgeCollider.points[0], _edgeCollider.points[1], transform.InverseTransformPoint(col.gameObject.transform.position)))
    {
      // we enter from the left, meaning that we came from red and go to green
      SetCameraModificationSettings(greenCameraModificationSettings);
    }
    else
    {
      // we enter from the right, meaning that we came from gree and go to red
      SetCameraModificationSettings(redCameraModificationSettings);
    }
  }
}
