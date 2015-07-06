using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TrooperHitControlHandler : PlayerControlHandler
{
  bool _isCompleted = false;

  public TrooperHitControlHandler(PlayerController playerController)
    : base(playerController, float.MaxValue)
  {

  }

  protected override bool DoUpdate()
  {
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;
    if (!_isCompleted)
    {
      _isCompleted = true;
      velocity.x = -Mathf.Sign(velocity.x) * _playerController.runSettings.walkSpeed * 4f; // TODO (Roman): hardcoded...

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

