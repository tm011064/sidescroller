using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SlideDownSlopePlayerControlHandler : PlayerControlHandler
{
  private Direction _platformDirection;

  public SlideDownSlopePlayerControlHandler(PlayerController playerController, Direction platformDirection)
    : base(playerController)
  {
    _platformDirection = platformDirection;
  }
  public SlideDownSlopePlayerControlHandler(PlayerController playerController, float duration, Direction platformDirection)
    : base(playerController, duration)
  {
    _platformDirection = platformDirection;
  }

  protected override bool DoUpdate()
  {
    CheckOneWayPlatformFallThrough();

    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    float yVelocity = Mathf.Max(
      GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity, true)
      , _playerController.jumpSettings.maxDownwardSpeed);

    MoveCalculationResult moveCalculationResult = _playerController.characterPhysicsManager.SlideDown(_platformDirection, yVelocity * Time.deltaTime);
    if (_platformDirection == Direction.Left && !moveCalculationResult.collisionState.left)
      return false;
    if (_platformDirection == Direction.Right && !moveCalculationResult.collisionState.right)
      return false;

    Logger.Trace("PlayerMetricsDebug", "Position: " + _playerController.transform.position + ", Velocity: " + velocity);

    return true;
  }

}

