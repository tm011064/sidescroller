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
  private float _originalInAirDamping;

  private bool _isFloating;

  public PowerUpFloaterControlHandler(PlayerController playerController, PowerUpSettings powerUpSettings)
    : base(playerController)
  {
    _powerUpSettings = powerUpSettings;
    _originalInAirDamping = _playerController.jumpSettings.inAirDamping;
#if !FINAL
    // TODO (Release): remove this
    _playerController.GetComponent<SpriteRenderer>().color = new Color(.5f, 1f, 1f, 1f);
#endif
  }

  public override void Dispose()
  {
#if !FINAL
    _playerController.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
#endif
    _playerController.adjustedGravity = _playerController.jumpSettings.gravity;
    _playerController.jumpSettings.inAirDamping = _originalInAirDamping;
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

        if ((_gameManager.inputStateManager.GetButtonState("Jump").buttonPressState & ButtonPressState.IsUp) != 0)
        {
          // player released jump button on way down, this means he can't float any more for this in air session
          _floatStatus &= ~FloatStatus.CanFloat;
        }
        if (((_floatStatus & FloatStatus.CanFloat) != 0)
          && (_gameManager.inputStateManager.GetButtonState("Jump").buttonPressState & ButtonPressState.IsDown) != 0)
        {
          // player is on his way down,can float and pressed the jump button, so he starts floating
          velocity.y *= _powerUpSettings.floaterSettings.startFloatingDuringFallVelocityMultiplier;
          _floatStatus |= FloatStatus.IsFloating;
        }
        if ((_gameManager.inputStateManager.GetButtonState("Jump").buttonPressState & ButtonPressState.IsPressed) != 0)
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
        _playerController.adjustedGravity = Mathf.Lerp(_playerController.adjustedGravity, _playerController.jumpSettings.gravity, Time.deltaTime * _powerUpSettings.floaterSettings.floaterGravityDecreaseInterpolationFactor); // TODO (Roman): hard coded...
      }
      else
      {
        _playerController.adjustedGravity = _powerUpSettings.floaterSettings.floaterGravity;
        _playerController.jumpSettings.inAirDamping = _powerUpSettings.floaterSettings.floaterInAirDampingOverride;
      }
    }
    else
    {
      _playerController.adjustedGravity = _playerController.jumpSettings.gravity;
    }
    _isFloating = isFloating;

    velocity.y = Mathf.Max(
      GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity, true)
      , _playerController.jumpSettings.maxDownwardSpeed);

    _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);

    return true;
  }
}

