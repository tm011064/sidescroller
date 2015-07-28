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
  [Tooltip("If false, all vertical lock settings will be ignored.")]
  public bool enabled = false;
  
  [Tooltip("Enables the default vertical lock position. The default position simulates a Super Mario Bros style side scrolling camera which is fixed on the y axis, not reacting to vertical player movement.")]
  public bool enableDefaultVerticalLockPosition = true;
  [Tooltip("Default is center of the screen.")]
  public float defaultVerticalLockPosition = 540f;
  
  [Tooltip("If enabled, the camera follows the player upwards until the \"Top Vertical Lock Position\" is reached.")]
  public bool enableTopVerticalLock = false;
  [Tooltip("The highest visible location relative to the \"Parent Position Object\" game object space")]
  public float topVerticalLockPosition = 1080f;

  [Tooltip("If enabled, the camera follows the player downwards until the \"Bottom Vertical Lock Position\" is reached.")]
  public bool enableBottomVerticalLock = false;
  [Tooltip("The lowest visible location relative to the \"Parent Position Object\" game object space")]
  public float bottomVerticalLockPosition = 0f;

  [HideInInspector]
  public float topBoundary;
  [HideInInspector]
  public float bottomBoundary;
  [HideInInspector]
  public float translatedVerticalLockPosition;

  public override string ToString()
  {
    return string.Format(@"enabled: {0}; enableTopVerticalLock: {1}; topVerticalLockPosition: {2}; topBoundary: {3};
enableBottomVerticalLock: {4}; bottomVerticalLockPosition: {5}; bottomBoundary: {6};
enableDefaultVerticalLockPosition: {7}; defaultVerticalLockPosition: {8}; translatedVerticalLockPosition: {9}"
      , enabled
      , enableTopVerticalLock
      , topVerticalLockPosition
      , topBoundary
      , enableBottomVerticalLock
      , bottomVerticalLockPosition
      , bottomBoundary
      , enableDefaultVerticalLockPosition
      , defaultVerticalLockPosition
      , translatedVerticalLockPosition
      );
  }

  public VerticalLockSettings Clone()
  {
    return this.MemberwiseClone() as VerticalLockSettings;
  }
}
[Serializable]
public class HorizontalLockSettings
{
  [Tooltip("If false, all horizontal lock settings will be ignored.")]
  public bool enabled = false;

  [Tooltip("If enabled, the camera follows the player moving right until the \"Right Horizontal Lock Position\" is reached.")]
  public bool enableRightHorizontalLock = true;
  [Tooltip("The rightmost visible location relative to the \"Parent Position Object\" game object space")]
  public float rightHorizontalLockPosition = 1920f;

  [Tooltip("If enabled, the camera follows the player moving left until the \"Right Horizontal Lock Position\" is reached.")]
  public bool enableLeftHorizontalLock = true;
  [Tooltip("The leftmost visible location relative to the \"Parent Position Object\" game object space")]
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

  public HorizontalLockSettings Clone()
  {
    return this.MemberwiseClone() as HorizontalLockSettings;
  }
}

[Serializable]
public class ZoomSettings
{
  [Tooltip("Default is 1, 2 means a reduction of 100%, .5 means a magnification of 100%.")]
  public float zoomPercentage = 1f;
  [Tooltip("The time it takes to zoom to the desired percentage.")]
  public float zoomTime;
  [Tooltip("The easing type when zooming in or out to the desired zoom percentage.")]
  public EasingType zoomEasingType;

  public override string ToString()
  {
    return string.Format("zoomPercentage: {0}; zoomTime: {1}; zoomEasingType: {2};"
      , zoomPercentage
      , zoomTime
      , zoomEasingType
      );
  }

  public ZoomSettings Clone()
  {
    return this.MemberwiseClone() as ZoomSettings;
  }
}

[Serializable]
public class SmoothDampMoveSettings
{
  [Tooltip("Camera smooth damping on horizontal character movement.")]
  public float horizontalSmoothDampTime = .2f;
  [Tooltip("Camera smooth damping on vertical character movement.")]
  public float verticalSmoothDampTime = .2f;
  [Tooltip("Camera smooth damping on rapid descents. For example, if the player falls down at high speeds, we want the camera to stay tight so the player doesn't move off screen.")]
  public float verticalRapidDescentSmoothDampTime = .01f;
  [Tooltip("Camera smooth damping on rapid ascents. For example, if the player travel up at high speed due to being catapulted by a trampoline, we want the camera to stay tight so the player doesn't move off screen.")]
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

  public SmoothDampMoveSettings Clone()
  {
    return this.MemberwiseClone() as SmoothDampMoveSettings;
  }
}

public class CameraModifier : MonoBehaviour
{
  public VerticalLockSettings verticalLockSettings;
  public HorizontalLockSettings horizontalLockSettings;
  public ZoomSettings zoomSettings;
  public SmoothDampMoveSettings smoothDampMoveSettings;

  [Tooltip("The (x, y) offset of the camera. This can be used when default vertical locking is disabled and you want the player to be below, above, right or left of the screen center.")]
  public Vector2 offset;

  public VerticalCameraFollowMode verticalCameraFollowMode;

  public Color gizmoColor = Color.magenta;

  [Tooltip("All lock positions are relative to this object.")]
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

    if (this.zoomSettings.zoomPercentage == 0f)
      throw new ArgumentOutOfRangeException("Zoom Percentage must not be 0.");

    if (this.verticalLockSettings.enabled)
    {
      if (this.verticalLockSettings.enableTopVerticalLock)
        this.verticalLockSettings.topBoundary = transformPoint.y + this.verticalLockSettings.topVerticalLockPosition - _cameraController.targetScreenSize.y * .5f / this.zoomSettings.zoomPercentage;
      if (this.verticalLockSettings.enableBottomVerticalLock)
        this.verticalLockSettings.bottomBoundary = transformPoint.y + this.verticalLockSettings.bottomVerticalLockPosition + _cameraController.targetScreenSize.y * .5f / this.zoomSettings.zoomPercentage;
    }

    if (this.horizontalLockSettings.enabled)
    {
      if (this.horizontalLockSettings.enableLeftHorizontalLock)
        this.horizontalLockSettings.leftBoundary = transformPoint.x + this.horizontalLockSettings.leftHorizontalLockPosition + _cameraController.targetScreenSize.x * .5f / this.zoomSettings.zoomPercentage;
      if (this.horizontalLockSettings.enableRightHorizontalLock)
        this.horizontalLockSettings.rightBoundary = transformPoint.x + this.horizontalLockSettings.rightHorizontalLockPosition - _cameraController.targetScreenSize.x * .5f / this.zoomSettings.zoomPercentage;
    }

    this.verticalLockSettings.translatedVerticalLockPosition = transformPoint.y + this.verticalLockSettings.defaultVerticalLockPosition;

    CameraMovementSettings cameraMovementSettings = new CameraMovementSettings(
      verticalLockSettings
      , horizontalLockSettings
      , zoomSettings
      , smoothDampMoveSettings
      , offset
      , verticalCameraFollowMode
      );

    var cameraController = Camera.main.GetComponent<CameraController>();

    cameraController.SetCameraMovementSettings(cameraMovementSettings);
  }
}
