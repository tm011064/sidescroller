using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TrooperBounceControlHandler : PlayerControlHandler
{
  bool _isCompleted = false;

  public TrooperBounceControlHandler(PlayerController playerController)
    : base(playerController)
  {

  }

  protected override bool DoUpdate()
  {
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;
    if (!_isCompleted)
    {
      _isCompleted = true;
      velocity.y = Mathf.Sqrt(2f * _playerController.jumpSettings.walkJumpHeight * -_playerController.jumpSettings.gravity) * .8f; // TODO (Roman): hardcoded...

      Logger.Info("Trooper Jump executed. Velocity y: " + velocity.y);

      _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);
      return true;
    }
    else
    {
      return false;
    }
  }
}

