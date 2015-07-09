using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DamageTakenPlayerControlHandler : DefaultPlayerControlHandler
{
  private float _suspendPhysicsEndTime;

  public DamageTakenPlayerControlHandler()
    : this(GameManager.instance.player, GameManager.instance.gameSettings.playerDamageControlHandlerSettings.duration
    , GameManager.instance.gameSettings.playerDamageControlHandlerSettings.suspendPhysicsTime) { }
  public DamageTakenPlayerControlHandler(PlayerController playerController, float duration, float suspendPhysicsTime)
    : base(playerController, duration)
  {
    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.red;

    _suspendPhysicsEndTime = Time.time + suspendPhysicsTime;
    playerController.isInvincible = true;
    playerController.isTakingDamage = true;
  }

  public override void Dispose()
  {
    _playerController.isInvincible = false;
    _playerController.isTakingDamage = false;
  }

  protected override bool DoUpdate()
  {
    if (Time.time <= _suspendPhysicsEndTime)
    {
      return true;
    }
    else
    {
      _playerController.isTakingDamage = false;
    }

    return base.DoUpdate();
  }
}

