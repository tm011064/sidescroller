﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WallJumpControlHandler : PlayerControlHandler
{
  private const string TRACE_TAG = "WallJumpControlHandler";

  private bool _hasJumpedFromWall;
  private float _wallJumpDirectionMultiplier;
  private bool _isWallRight = false;

  private float _oppositeDirectionPressedTime = -1f;

  private WallJumpSettings _wallJumpSettings;

  public void Reset(float duration, bool isWallRight, WallJumpSettings wallJumpSettings)
  {
    this._overrideEndTime = Time.time + duration;
    this._hasJumpedFromWall = false;
    this._oppositeDirectionPressedTime = -1f;
    this._isWallRight = isWallRight;
    this._wallJumpDirectionMultiplier = isWallRight ? -1f : 1f;
    this._wallJumpSettings = wallJumpSettings;
  }

  public override void Dispose()
  {
    _playerController.adjustedGravity = _playerController.jumpSettings.gravity;
  }

  public WallJumpControlHandler(PlayerController playerController, float duration, bool isWallRight, WallJumpSettings wallJumpSettings)
    : base(playerController, duration)
  {
    this._isWallRight = isWallRight;
    this._wallJumpDirectionMultiplier = isWallRight ? -1f : 1f;
    this._wallJumpSettings = wallJumpSettings;

    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.cyan;
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

    if ((_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.characterWallState & CharacterWallState.NotOnWall) != 0)
    {
      Logger.Info("Popped wall jump because character is not on wall any more.");
      return false;
    }

    if (velocity.y < 0f)
      _playerController.adjustedGravity = _wallJumpSettings.wallStickGravity; // going down, use wall stick gravity
    else
      _playerController.adjustedGravity = _playerController.jumpSettings.gravity; // still going up, use normal gravity

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

      Logger.Info("Wall Jump executed. Velocity y: " + velocity.y);
    }

    AxisState hAxis;
    if (_hasJumpedFromWall)
      hAxis = new AxisState(_wallJumpDirectionMultiplier);
    else
    {
      hAxis = _gameManager.inputStateManager.GetAxisState("Horizontal").Clone();
      // TODO (Roman): we want to stick to wall
      if ((_isWallRight && hAxis.value < 0f)
        || (!_isWallRight && hAxis.value > 0f))
      {// player presses opposite direction...
        if (_oppositeDirectionPressedTime < 0)
        {
          _oppositeDirectionPressedTime = Time.time;
        }
        else if (Time.time - _wallJumpSettings.wallStickTime > _oppositeDirectionPressedTime)
        {
          // leave wall...
          Logger.Info("Popped wall jump because player pressed opposite direction for more than " + _wallJumpSettings.wallStickTime + " seconds.");
          return false;
        }

        // we set to zero because we want to stick until the threshold is surpassed. Then we pop the handler...
        hAxis.value = 0f;
      }
      else
      {
        _oppositeDirectionPressedTime = -1;
      }
    }

    float normalizedHorizontalSpeed = GetNormalizedHorizontalSpeed(hAxis);

    if (isWallJump)
    {
      float speed = _playerController.runSettings.walkSpeed;
      velocity.x = normalizedHorizontalSpeed * speed;
    }
    else
    {
      velocity.x = GetHorizontalVelocityWithDamping(velocity, hAxis.value, normalizedHorizontalSpeed);
    }

    velocity.y = GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity);

    _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);

    return true;
  }
}

