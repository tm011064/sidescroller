using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DefaultTrooperControlHandler : BaseControlHandler
{
  private float _moveDirectionFactor;
  private bool _hasChangedDirection;

  private TrooperController _trooperController;

  public DefaultTrooperControlHandler(TrooperController trooperController, float overrideEndTime)
    : base(trooperController.characterPhysicsManager, overrideEndTime)
  {
    _trooperController = trooperController;
  }

  public override void OnEnemyMoveMaskCollision(Collider2D col)
  {
    _trooperController.moveDirectionFactor *= -1;
    _hasChangedDirection = true;
  }

  protected override bool DoUpdate()
  {
    Vector3 velocity = _characterPhysicsManager.velocity;

    if (_characterPhysicsManager.isGrounded)
      velocity.y = 0;

    if (!_hasChangedDirection)
    {
      velocity = _characterPhysicsManager.velocity;

      var smoothedMovementFactor = _characterPhysicsManager.isGrounded ? _trooperController.groundDamping : _trooperController.inAirDamping; // how fast do we change direction?
      velocity.x = Mathf.Lerp(velocity.x, _trooperController.moveDirectionFactor * _trooperController.runSpeed, Time.deltaTime * smoothedMovementFactor);
    }
    else
    {
      _hasChangedDirection = false;
      velocity.x = _trooperController.moveDirectionFactor * _trooperController.runSpeed;
      _trooperController.transform.localScale = new Vector3(-_trooperController.transform.localScale.x, _trooperController.transform.localScale.y, _trooperController.transform.localScale.z);
    }

    // apply gravity before moving
    velocity.y += _trooperController.gravity * Time.deltaTime;

    _characterPhysicsManager.move(velocity * Time.deltaTime);

    return true;
  }
}

