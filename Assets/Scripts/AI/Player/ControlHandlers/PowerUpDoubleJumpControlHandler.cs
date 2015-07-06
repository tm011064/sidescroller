using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PowerUpDoubleJumpControlHandler : PlayerControlHandler
{
  private bool _canDoubleJump;
  private bool _hasUsedDoubleJump;
  
  public PowerUpDoubleJumpControlHandler(PlayerController playerController)
    : base(playerController, float.MaxValue)
  {
    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.yellow;
  }

  protected override bool DoUpdate()
  {
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    if (_characterPhysicsManager.isGrounded)
    {
      _canDoubleJump = true;
    }

    velocity.y = GetJumpVerticalVelocity(velocity);
    
    if (_playerController.characterPhysicsManager.isGrounded)
      velocity.y = 0f;

    if (_playerController.inputStateManager["Jump"].IsDown)
    {
      if (this.CanJump())
      {
        velocity.y = Mathf.Sqrt(2f * _playerController.jumpSettings.walkJumpHeight * jumpHeightMultiplier * -_playerController.jumpSettings.gravity);
        //_animator.Play(Animator.StringToHash("Jump"));

        Logger.Info("Ground Jump executed. Velocity y: " + velocity.y);
      }
      else if (_canDoubleJump)
      {
        velocity.y = Mathf.Sqrt(2f * _playerController.jumpSettings.walkJumpHeight * jumpHeightMultiplier * -_playerController.jumpSettings.gravity);
        _canDoubleJump = false;
      }
    }

    velocity.y = GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity);

    velocity.x = GetDefaultHorizontalVelocity(velocity);
    
    _playerController.characterPhysicsManager.move(velocity * Time.deltaTime);

    return true;
  }

}

