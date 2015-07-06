using UnityEngine;

public struct CameraMovementSettings
{
  public float? YPosLock;
  public float? XPosLock;

  public bool AllowLeftExtension;
  public bool AllowTopExtension;
  public bool AllowRightExtension;
  public bool AllowBottomExtension;

  public float LeftBoundary;
  public float TopBoundary;
  public float RightBoundary;
  public float BottomBoundary;

  public float OffsetX;
  public float OffsetY;

  public override string ToString()
  {
    return string.Format("Pos Locks: ({0}, {1}), Extensions (l = {2}, t = {3}, r = {4}, b = {5}, ox = {6}, oy = {7}): "
      , (XPosLock.HasValue ? XPosLock.Value.ToString() : "NULL")
      , (YPosLock.HasValue ? YPosLock.Value.ToString() : "NULL")
      , AllowLeftExtension ? "true" : "false"
      , AllowTopExtension ? "true" : "false"
      , AllowRightExtension ? "true" : "false"
      , AllowBottomExtension ? "true" : "false"
      , OffsetX 
      , OffsetY);
  }

  public CameraMovementSettings(float? xPosLock, float? yPosLock, bool allowTopExtension = false, bool allowBottomExtension = false
    , bool allowLeftExtension = false, bool allowRightExtension = false, float offsetX = 0f, float offsetY = 0f)
  {
    this.XPosLock = xPosLock;
    this.YPosLock = yPosLock;

    this.AllowTopExtension = allowTopExtension;
    this.AllowBottomExtension = allowBottomExtension;
    this.AllowLeftExtension = allowLeftExtension;
    this.AllowRightExtension = allowRightExtension;

    this.TopBoundary = yPosLock.HasValue ? yPosLock.Value + Screen.height * .5f : 0f;
    this.BottomBoundary = yPosLock.HasValue ? yPosLock.Value - Screen.height * .5f : 0f;
    this.LeftBoundary = xPosLock.HasValue ? xPosLock.Value - Screen.width * .5f : 0f;
    this.RightBoundary = xPosLock.HasValue ? xPosLock.Value + Screen.width * .5f : 0f;

    this.OffsetX = offsetX;
    this.OffsetY = offsetY;
  }
}
