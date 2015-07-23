using UnityEngine;

public class PatrollerEnemyControlHandler : EnemyControlHandler<PatrollerEnemyController>
{
  private float _moveDirectionFactor;

  public PatrollerEnemyControlHandler(PatrollerEnemyController patrollerEnemyController, Direction startDirection)
    : base(patrollerEnemyController, -1f)
  {
    if (startDirection == Direction.Left)
      _moveDirectionFactor = -1f;
    else
      _moveDirectionFactor = 1f;
  }

  protected override bool DoUpdate()
  {
    Vector3 velocity = _characterPhysicsManager.velocity;

    if (_characterPhysicsManager.lastMoveCalculationResult.collisionState.below)
      velocity.y = 0;

    // move with constant speed
    velocity.x = _moveDirectionFactor * _enemyController.speed;

    // apply gravity before moving
    velocity.y += _enemyController.gravity * Time.deltaTime;

    MoveCalculationResult moveCalculationResult = _characterPhysicsManager.CalculateMove(velocity * Time.deltaTime);
    if (
          (moveCalculationResult.collisionState.wasGroundedLastFrame && moveCalculationResult.collisionState.below && !moveCalculationResult.collisionState.isFullyGrounded) // we are on edge
          || (moveCalculationResult.collisionState.wasGroundedLastFrame && !moveCalculationResult.collisionState.below)
          || (_moveDirectionFactor < 0f && moveCalculationResult.collisionState.left)
          || (_moveDirectionFactor > 0f && moveCalculationResult.collisionState.right)
    )
    {
      // would go over edge, so change direction
      _moveDirectionFactor *= -1;

      velocity.x = _moveDirectionFactor * _enemyController.speed;
      _characterPhysicsManager.Move(velocity * Time.deltaTime);
    }
    else
    {
      _characterPhysicsManager.PerformMove(moveCalculationResult);
    }

    return true;
  }
}

