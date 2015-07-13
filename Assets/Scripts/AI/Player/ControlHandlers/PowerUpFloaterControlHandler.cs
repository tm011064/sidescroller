using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PowerUpFloaterControlHandler : PlayerControlHandler
{
  enum FloatStatus
  {
    IsInAir = 1,
    CanFloat = 2,
    IsFloating = 4
  }

  private PowerUpSettings _powerUpSettings;
  private FloatStatus _floatStatus;

  private bool _isFloating;

  public PowerUpFloaterControlHandler(PlayerController playerController, PowerUpSettings powerUpSettings)
    : base(playerController)
  {
    _powerUpSettings = powerUpSettings;

    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.magenta;
  }

  public override void Dispose()
  {
    _playerController.adjustedGravity = _playerController.jumpSettings.gravity;
  }

  protected override bool DoUpdate()
  {
    CheckOneWayPlatformFallThrough();

    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    velocity.y = GetJumpVerticalVelocity(velocity);
    velocity.x = GetDefaultHorizontalVelocity(velocity);

    if (velocity.y == 0)
    {
      _floatStatus = FloatStatus.CanFloat;
    }
    else
    {
      _floatStatus |= FloatStatus.IsInAir;

      if (velocity.y < 0)
      { // player is moving down, so check floating logic

        if (_gameManager.inputStateManager.GetButtonState("Jump").IsUp)
        {
          // player released jump button on way down, this means he can't float any more for this in air session
          _floatStatus &= ~FloatStatus.CanFloat;
        }
        if (((_floatStatus & FloatStatus.CanFloat) != 0) && _gameManager.inputStateManager.GetButtonState("Jump").IsDown)
        {
          // player is on his way down,can float and pressed the jump button, so he starts floating
          velocity.y *= _powerUpSettings.startFloatingDuringFallVelocityMultiplier;
          _floatStatus |= FloatStatus.IsFloating;
        }
        if (_gameManager.inputStateManager.GetButtonState("Jump").IsPressed)
        {
          // player is on his way down and has the jump button pressed, so set the floating field
          _floatStatus |= FloatStatus.IsFloating;
        }
      }
      else
      {
        _floatStatus |= FloatStatus.CanFloat;
        _floatStatus &= ~FloatStatus.IsFloating;
      }
    }

    bool isFloating = velocity.y < 0 && (_floatStatus == (FloatStatus.IsInAir | FloatStatus.CanFloat | FloatStatus.IsFloating));
    if (isFloating)
    {
      if (_isFloating)
      {
        _playerController.adjustedGravity = Mathf.Lerp(_playerController.adjustedGravity, _playerController.jumpSettings.gravity, Time.deltaTime * .05f); // TODO (Roman): hard coded...
      }
      else
      {
        _playerController.adjustedGravity = _powerUpSettings.floaterGravity;
      }
    }
    else
    {
      _playerController.adjustedGravity = _playerController.jumpSettings.gravity;
    }
    _isFloating = isFloating;

    velocity.y = Mathf.Max(
      GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity)
      , _playerController.jumpSettings.maxDownwardSpeed);

    _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);

    return true;
  }
}

