using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DefaultPlayerControlHandler : PlayerControlHandler
{
  public DefaultPlayerControlHandler(PlayerController playerController, float overrideEndTime)
    : base(playerController, overrideEndTime)
  {

  }

  protected override bool DoUpdate()
  {
    CheckOneWayPlatformFallThrough();

    Vector3 velocity = _playerController.characterPhysicsManager.velocity;
    
    velocity.y = GetJumpVerticalVelocity(velocity);
    velocity.y = GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity);

    velocity.x = GetDefaultHorizontalVelocity(velocity);

    _playerController.characterPhysicsManager.move(velocity * Time.deltaTime);
    
    return true;
  }

}

