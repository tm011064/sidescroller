using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WallJumpControlHandler : PlayerControlHandler
{
  private const string TRACE_TAG = "WallJumpControlHandler";

  private bool _hasJumpedFromWall;
  private float _wallJumpDirectionMultiplier;
  private Direction _wallDirection;

  private WallJumpSettings _wallJumpSettings;
  private AxisState _axisOverride;

  public void Reset(float duration, Direction wallDirection, WallJumpSettings wallJumpSettings)
  {
    this._overrideEndTime = duration < 0f ? null : (float?)(Time.time + duration);
    this._hasJumpedFromWall = false;
    this._wallDirection = wallDirection;
    this._wallJumpSettings = wallJumpSettings;

    this._wallJumpDirectionMultiplier = wallDirection == Direction.Right ? -1f : 1f;
    _axisOverride = new AxisState(-this._wallJumpDirectionMultiplier);
  }

  public override void Dispose()
  {
    _playerController.adjustedGravity = _playerController.jumpSettings.gravity;
  }

  public WallJumpControlHandler(PlayerController playerController)
    : base(playerController)
  {
    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.cyan;
  }

  public override bool TryActivate(BaseControlHandler previousControlHandler)
  {
    WallJumpEvaluationControlHandler wallJumpEvaluationControlHandler = previousControlHandler as WallJumpEvaluationControlHandler;
    if (wallJumpEvaluationControlHandler == null)
    {
      Logger.Info("Wall Jump Control Handler can not be activated because previous control handler is null.");
      return false;
    }
    else if (wallJumpEvaluationControlHandler.HasDetached)
    {
      Logger.Info("Wall Jump Control Handler can not be activated because wall jump evaluation control handler has detached.");
      return false;
    }

    return base.TryActivate(previousControlHandler);
  }

  public override string ToString()
  {
    return "WallJumpControlHandler; override end time: " + _overrideEndTime + "; has jumped from wall: " + _hasJumpedFromWall;
  }

  /// <summary>
  /// Does the update.
  /// </summary>
  /// <returns></returns>
  protected override bool DoUpdate()
  {
    if (_playerController.characterPhysicsManager.isGrounded)
    {
      Logger.Info("Popped wall jump because player is grounded.");
      return false; // we only want this handler to be active while the player is in mid air
    }
    // TODO (Roman): when wall jumping and floating, we should not be able to climb up a wall using wall jumps and floats
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    if (velocity.y < _wallJumpSettings.wallVelocityDownThreshold)
    {
      Logger.Info("Popped wall jump because downward velocity threshold was surpassed: " + velocity.y + " < " + _wallJumpSettings.wallVelocityDownThreshold);
      return false; // we can exit as wall jump is not allowed any more after the player accelerated downward beyond threshold
    }

    if (!_hasJumpedFromWall
      && (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.characterWallState & CharacterWallState.NotOnWall) != 0)
    {
      Logger.Info("Popped wall jump because character is not on wall any more.");
      return false;
    }

    if (velocity.y <= 0f)
    {
      _playerController.adjustedGravity = _wallJumpSettings.wallStickGravity; // going down, use wall stick gravity     
    }
    else
    {
      _playerController.adjustedGravity = _playerController.jumpSettings.gravity; // still going up, use normal gravity
    }
    bool isWallJump = false;
    if (!_hasJumpedFromWall
        && (_gameManager.inputStateManager.GetButtonState("Jump").IsDown))
    {
      // set flag for later calcs outside this scope
      isWallJump = true;

      // we want to override x axis buttons for a certain amount of time so the sprite can push off from the wall
      this._overrideEndTime = Time.time + _wallJumpSettings.wallJumpPushOffAxisOverrideTime;

      // set jump height as usual
      velocity.y = Mathf.Sqrt(2f * _playerController.jumpSettings.walkJumpHeight * -_playerController.jumpSettings.gravity);

      // disable jump
      _hasJumpedFromWall = true;
      _axisOverride = new AxisState(_wallJumpDirectionMultiplier);

      Logger.Info("Wall Jump executed. Velocity y: " + velocity.y);
    }

    float normalizedHorizontalSpeed = GetNormalizedHorizontalSpeed(_axisOverride);

    if (isWallJump)
    {
      float speed = _playerController.runSettings.walkSpeed;
      velocity.x = normalizedHorizontalSpeed * speed;
    }
    else
    {
      velocity.x = GetHorizontalVelocityWithDamping(velocity, _axisOverride.value, normalizedHorizontalSpeed);
    }

    // we need to check whether we have to adjust the vertical velocity. If levitation is on, we want to defy the law of physics
    velocity.y = Mathf.Max(
      GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity, true)
      , _playerController.wallJumpSettings.maxWallDownwardSpeed);

    _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);

    return true;
  }
}

