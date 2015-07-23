using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WallJumpEvaluationControlHandler : DefaultPlayerControlHandler
{
  private const string TRACE_TAG = "WallJumpEvaluationControlHandler";

  private Direction _wallDirection;
  private WallJumpSettings _wallJumpSettings;
  private bool _hasDetached = false;

  public void Reset(float duration, Direction wallDirection, WallJumpSettings wallJumpSettings)
  {
    this._overrideEndTime = Time.time + duration;
    this._wallDirection = wallDirection;
    this._wallJumpSettings = wallJumpSettings;

    this._hasDetached = false;
  }

  public WallJumpEvaluationControlHandler(PlayerController playerController)
    : base(playerController)
  {
    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.cyan;
  }

  public bool HasDetached { get { return _hasDetached; } }

  public override string ToString()
  {
    return "WallJumpEvaluationControlHandler; override end time: " + _overrideEndTime + "; has detached from wall: " + _hasDetached;
  }
  
  /// <summary>
  /// Does the update.
  /// </summary>
  /// <returns></returns>
  protected override bool DoUpdate()
  {
    if (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.below)
    {
      _hasDetached = true;
      Logger.Info("Popped wall jump evaluation because player is grounded.");
      return false; // we only want this handler to be active while the player is in mid air
    }

    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    if (velocity.y < _wallJumpSettings.wallVelocityDownThreshold)
    {
      _hasDetached = true;
      Logger.Info("Popped wall jump evaluation because downward velocity threshold was surpassed: " + velocity.y + " < " + _wallJumpSettings.wallVelocityDownThreshold);
      return false; // we can exit as wall jump is not allowed any more after the player accelerated downward beyond threshold
    }

    if ((_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.characterWallState & CharacterWallState.NotOnWall) != 0)
    {
      _hasDetached = true;
      Logger.Info("Popped wall jump evaluation because character is not on wall any more.");
      return false;
    }

    AxisState hAxis = _gameManager.inputStateManager.GetAxisState("Horizontal");
    if (_wallDirection == Direction.Right)
    {
      if (hAxis.value <= 0f || hAxis.value < hAxis.lastValue)
      {
        _hasDetached = true;
        Logger.Info("Popped wall jump evaluation because horizontal axis points to opposite direction. Current and Last axis value: (" + hAxis.value + ", " + hAxis.lastValue + ")");
        return false;
      }
    }
    else if (_wallDirection == Direction.Left)
    {
      if (hAxis.value >= 0f || hAxis.value > hAxis.lastValue)
      {
        _hasDetached = true;
        Logger.Info("Popped wall jump evaluation because horizontal axis points to opposite direction. Current and Last axis value: (" + hAxis.value + ", " + hAxis.lastValue + ")");
        return false;
      }
    }
    else
    {
      _hasDetached = true;
      Logger.Info("Popped wall jump evaluation because direction " + _wallDirection + " is not supported.");
      return false;
    }

    return base.DoUpdate();
  }
}

