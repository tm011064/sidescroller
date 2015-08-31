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

    public float horizontalOffsetDeltaMovementFactor = 40f;

    public VerticalCameraFollowMode verticalCameraFollowMode;

    public MultiWayCameraModificationSetting Clone()
    {
      MultiWayCameraModificationSetting multiWayCameraModificationSetting = new MultiWayCameraModificationSetting();

      multiWayCameraModificationSetting.verticalLockSettings = verticalLockSettings.Clone();
      multiWayCameraModificationSetting.horizontalLockSettings = horizontalLockSettings.Clone();
      multiWayCameraModificationSetting.zoomSettings = zoomSettings.Clone();
      multiWayCameraModificationSetting.smoothDampMoveSettings = smoothDampMoveSettings.Clone();

      multiWayCameraModificationSetting.offset = offset;
      multiWayCameraModificationSetting.verticalCameraFollowMode = verticalCameraFollowMode;
      multiWayCameraModificationSetting.horizontalOffsetDeltaMovementFactor = horizontalOffsetDeltaMovementFactor;

      return multiWayCameraModificationSetting;
    }
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

  private MultiWayCameraModificationSetting CloneAndTranslaceCameraModificationSetting(MultiWayCameraModificationSetting source, CameraController cameraController)
  {
    MultiWayCameraModificationSetting clone = source.Clone();
    Vector3 transformPoint = parentPositionObject.transform.TransformPoint(Vector3.zero);
    
    if (clone.verticalLockSettings.enabled)
    {
      if (clone.verticalLockSettings.enableTopVerticalLock)
        clone.verticalLockSettings.topBoundary = transformPoint.y + clone.verticalLockSettings.topVerticalLockPosition - cameraController.targetScreenSize.y * .5f / clone.zoomSettings.zoomPercentage;
      if (clone.verticalLockSettings.enableBottomVerticalLock)
        clone.verticalLockSettings.bottomBoundary = transformPoint.y + clone.verticalLockSettings.bottomVerticalLockPosition + cameraController.targetScreenSize.y * .5f / clone.zoomSettings.zoomPercentage;
    }

    if (clone.horizontalLockSettings.enabled)
    {
      if (clone.horizontalLockSettings.enableLeftHorizontalLock)
        clone.horizontalLockSettings.leftBoundary = transformPoint.x + clone.horizontalLockSettings.leftHorizontalLockPosition + cameraController.targetScreenSize.x * .5f / clone.zoomSettings.zoomPercentage;
      if (clone.horizontalLockSettings.enableRightHorizontalLock)
        clone.horizontalLockSettings.rightBoundary = transformPoint.x + clone.horizontalLockSettings.rightHorizontalLockPosition - cameraController.targetScreenSize.x * .5f / clone.zoomSettings.zoomPercentage;
    }

    clone.verticalLockSettings.translatedVerticalLockPosition = transformPoint.y + clone.verticalLockSettings.defaultVerticalLockPosition;

    return clone;
  }

  private void SetCameraModificationSettings(MultiWayCameraModificationSetting source)
  {
    if (source.zoomSettings.zoomPercentage == 0f)
      throw new ArgumentOutOfRangeException("Zoom Percentage must not be 0.");

    MultiWayCameraModificationSetting clone = CloneAndTranslaceCameraModificationSetting(source, _cameraController);

    CameraMovementSettings cameraMovementSettings = new CameraMovementSettings(
      clone.verticalLockSettings
      , clone.horizontalLockSettings
      , clone.zoomSettings
      , clone.smoothDampMoveSettings
      , clone.offset
      , clone.verticalCameraFollowMode
      , clone.horizontalOffsetDeltaMovementFactor
      );

    var cameraController = Camera.main.GetComponent<CameraController>();

    cameraController.SetCameraMovementSettings(cameraMovementSettings);

    _lastMultiWayCameraModificationSetting = source;
    Logger.Info("Applied " + (source == redCameraModificationSettings ? "red" : "green") + " camera setting for camera modifier " + this.name);
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
