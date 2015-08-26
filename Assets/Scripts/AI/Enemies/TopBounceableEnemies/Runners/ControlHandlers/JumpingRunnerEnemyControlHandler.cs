using UnityEngine;

public class JumpingRunnerEnemyControlHandler : EnemyControlHandler<JumpingRunnerEnemyController>
{
  private float _moveDirectionFactor;
  private float _nextJumpTime;
  

  public JumpingRunnerEnemyControlHandler(JumpingRunnerEnemyController controller, Direction startDirection)
    : base(controller, -1f)
  {
    if (startDirection == Direction.Left)
      _moveDirectionFactor = -1f;
    else
      _moveDirectionFactor = 1f;

    _characterPhysicsManager.onControllerBecameGrounded += _characterPhysicsManager_onControllerBecameGrounded;
  }

  void _characterPhysicsManager_onControllerBecameGrounded(GameObject obj)
  {
    _nextJumpTime = Time.time + _enemyController.jumpInterval;
  }

  protected override bool DoUpdate()
  {

    // first move in patrolling mode
    if (Time.time >= _nextJumpTime && _characterPhysicsManager.lastMoveCalculationResult.collisionState.below)
    {
      float velocityY = Mathf.Sqrt(
            2f
            * -_enemyController.gravity
            * _enemyController.jumpHeight
            );
      MoveHorizontally(ref _moveDirectionFactor, _enemyController.speed, _enemyController.gravity, PlatformEdgeMoveMode.FallOff
        , jumpVelocityY: Mathf.Sqrt(2f * -_enemyController.gravity * _enemyController.jumpHeight))
        ;
      _nextJumpTime = Time.time + _enemyController.jumpInterval;
    }
    else
    {
      MoveHorizontally(ref _moveDirectionFactor, _enemyController.speed, _enemyController.gravity, PlatformEdgeMoveMode.FallOff);
    }
    return true;
  }
}

