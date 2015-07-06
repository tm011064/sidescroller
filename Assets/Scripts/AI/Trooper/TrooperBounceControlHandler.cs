using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TrooperBounceControlHandler : PlayerControlHandler
{
  bool _isCompleted = false;

  public TrooperBounceControlHandler(PlayerController playerController)
    : base(playerController, float.MaxValue)
  {

  }

  protected override bool DoUpdate()
  {
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;
    if (!_isCompleted)
    {
      _isCompleted = true;
      velocity.y = Mathf.Sqrt(2f * _playerController.jumpHeight * -_playerController.gravity) * .8f; // TODO (Roman): hardcoded...

      Logger.Info("Trooper Jump executed. Velocity y: " + velocity.y);

      _playerController.characterPhysicsManager.move(velocity * Time.deltaTime);
      return true;
    }
    else
    {
      return false;
    }
  }
}

