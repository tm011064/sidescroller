using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PowerUpSpinMeleeAttackControlHandler : DefaultPlayerControlHandler
{
  public PowerUpSpinMeleeAttackControlHandler(PlayerController playerController)
    : base(playerController)
  {
#if !FINAL
    // TODO (Release): remove this
    _playerController.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, .75f, 1f);
#endif
  }

  public override void Dispose()
  {
    _playerController.spinMeleeAttackBoxCollider.SetActive(false);
    _playerController.isPerformingSpinMeleeAttack = false;

#if !FINAL
    _playerController.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
#endif
  }

  private bool CanPerformAttack()
  {
    return !_playerController.isAttachedToWall
        && !_playerController.isCrouching
        && !_playerController.isTakingDamage;
  }

  public override void OnAfterStackPeekUpdate()
  {
    if (_playerController.isPerformingSpinMeleeAttack)
    {
      if (!CanPerformAttack())
      {
        _playerController.isPerformingSpinMeleeAttack = false;
        _playerController.spinMeleeAttackBoxCollider.SetActive(false);
      }
    }
    else
    {
      if (_playerController.spinMeleeAttackBoxCollider.activeSelf)
        _playerController.spinMeleeAttackBoxCollider.SetActive(false);      
    }
  }

  protected override bool DoUpdate()
  {
    if (
      !_playerController.isPerformingSpinMeleeAttack
      && CanPerformAttack()
      && (_gameManager.inputStateManager.GetButtonState("Attack").buttonPressState & ButtonPressState.IsDown) != 0)
    {
      if (!_playerController.spinMeleeAttackBoxCollider.activeSelf)
        _playerController.spinMeleeAttackBoxCollider.SetActive(true);

      _playerController.isPerformingSpinMeleeAttack = true;
    }

    if (!_playerController.isPerformingSpinMeleeAttack && _playerController.spinMeleeAttackBoxCollider.activeSelf)
    {
      _playerController.spinMeleeAttackBoxCollider.SetActive(false);
    }

    return base.DoUpdate();
  }

}

