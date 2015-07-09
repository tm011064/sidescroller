using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DefaultPlayerControlHandler : PlayerControlHandler
{
  public DefaultPlayerControlHandler(PlayerController playerController)
    : base(playerController)
  {

  }
  public DefaultPlayerControlHandler(PlayerController playerController, float duration)
    : base(playerController, duration)
  {

  }
  
  protected override bool DoUpdate()
  {
    CheckOneWayPlatformFallThrough();

    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    velocity.y = GetJumpVerticalVelocity(velocity);
    velocity.y = GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity);

    velocity.x = GetDefaultHorizontalVelocity(velocity);
    
    _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);
    
    return true;
  }

}

