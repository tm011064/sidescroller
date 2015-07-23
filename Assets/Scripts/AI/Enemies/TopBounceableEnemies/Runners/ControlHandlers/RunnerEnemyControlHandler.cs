using UnityEngine;

public class RunnerEnemyControlHandler : EnemyControlHandler<RunnerEnemyController>
{
  private float _moveDirectionFactor;

  public RunnerEnemyControlHandler(RunnerEnemyController patrollerEnemyController, Direction startDirection)
    : base(patrollerEnemyController, -1f)
  {
    if (startDirection == Direction.Left)
      _moveDirectionFactor = -1f;
    else
      _moveDirectionFactor = 1f;
  }

  protected override bool DoUpdate()
  {
    // first move in patrolling mode
    MoveHorizontally(ref _moveDirectionFactor, _enemyController.speed, _enemyController.gravity, PlatformEdgeMoveMode.FallOff);

    return true;
  }
}

