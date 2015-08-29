using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable
{
  void StartMove();
  void StopMove();
}

public class ManualPingPongPath : DynamicPingPongPath, IMoveable
{
  public float pauseTimeAfterForwardMoveCompleted = 0f;
  public float pauseTimeAfterBackwardMoveCompleted = 0f;
  public bool autoStart = true;

  protected override void OnForwardMovementCompleted()
  {
    if (pauseTimeAfterForwardMoveCompleted > 0f)
      this.Invoke("StopForwardMovement", pauseTimeAfterForwardMoveCompleted);
    else
      this.StopForwardMovement();
  }
  protected override void OnBackwardMovementCompleted()
  {
    if (pauseTimeAfterBackwardMoveCompleted > 0f)
      this.Invoke("StartForwardMovement", pauseTimeAfterBackwardMoveCompleted);
    else
      this.StartForwardMovement();
  }

  protected override void OnEnable()
  {
    base.OnEnable();

    if (autoStart)
      StartForwardMovement();
  }

  #region IMoveable Members

  public void StartMove()
  {
    this.StartForwardMovement();
  }

  public void StopMove()
  {
    this.StopForwardMovement();
  }

  #endregion
}