using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TopBounceableControlHandler : DefaultPlayerControlHandler
{
  private bool _hasPerformedDefaultBounce = false;
  private float _bounceJumpMultiplier;
  
  public TopBounceableControlHandler(PlayerController playerController, float duration, float bounceJumpMultiplier)
    : base(playerController, duration)
  {
    _bounceJumpMultiplier = bounceJumpMultiplier;
  }

  protected override bool DoUpdate()
  {
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;
    if (!_hasPerformedDefaultBounce)
    {
      if ((_gameManager.inputStateManager.GetButtonState("Jump").buttonPressState & ButtonPressState.IsPressed) != 0)
      {
        velocity.y = CalculateJumpHeight(velocity);
        
        Logger.Info("Top bounce jump executed. Jump button was pressed. New velocity y: " + velocity.y);
        _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);

        return false; // exit, we are done
      }
      else
      {
        velocity.y = Mathf.Sqrt(2f * _playerController.jumpSettings.walkJumpHeight * -_playerController.jumpSettings.gravity) * _bounceJumpMultiplier;
        _hasPerformedDefaultBounce = true;

        _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);

        Logger.Info("Top bounce jump executed. Jump button was not pressed. BounceJumpMultiplier: " + _bounceJumpMultiplier + ", new velocity y: " + velocity.y);
        return true; // keep waiting, maybe user presses jump before time is up
      }
    }

    if ((_gameManager.inputStateManager.GetButtonState("Jump").buttonPressState & ButtonPressState.IsPressed) != 0)
    {
      velocity.y = CalculateJumpHeight(velocity);

      Logger.Info("Top bounce jump executed. Jump button was pressed. New velocity y: " + velocity.y);
      _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);

      return false;
    }

    return base.DoUpdate();
  }
}