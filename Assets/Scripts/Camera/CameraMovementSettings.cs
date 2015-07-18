using UnityEngine;

public class CameraMovementSettings
{
  public VerticalLockSettings verticalLockSettings;
  public HorizontalLockSettings horizontalLockSettings;
  public ZoomSettings zoomSettings;
  public SmoothDampMoveSettings smoothDampMoveSettings;

  public Vector2 offset;

  public VerticalCameraFollowMode verticalCameraFollowMode;

  public override string ToString()
  {
    return string.Format("verticalLockSettings {{ {0} }}\nhorizontalLockSettings {{ {1} }}\nzoomSettings {{ {2} }}\nsmoothDampMoveSettings {{ {3} }}\noffset: {4}; verticalCameraFollowMode: {5}"
      , verticalLockSettings
      , horizontalLockSettings
      , zoomSettings
      , smoothDampMoveSettings
      , offset
      , verticalCameraFollowMode
      );
  }

  public CameraMovementSettings(VerticalLockSettings verticalLockSettings, HorizontalLockSettings horizontalLockSettings
      , ZoomSettings zoomSettings, SmoothDampMoveSettings smoothDampMoveSettings
      , Vector2 offset
      , VerticalCameraFollowMode verticalCameraFollowMode)
  {
    this.horizontalLockSettings = horizontalLockSettings;
    this.verticalLockSettings = verticalLockSettings;
    this.offset = offset;
    this.zoomSettings = zoomSettings;
    this.smoothDampMoveSettings = smoothDampMoveSettings;
    this.verticalCameraFollowMode = verticalCameraFollowMode;
  }
}
