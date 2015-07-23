using UnityEngine;

public class ChasingChaserEnemyControlHandler : EnemyControlHandler<ChaserEnemyController>
{
  public ChasingChaserEnemyControlHandler(ChaserEnemyController chaserEnemyController, float duration, Direction startDirection)
    : base(chaserEnemyController, duration)
  {
  }

  protected override bool DoUpdate()
  {
    // if we are chasing, try to get to the player, else, patrole and watch    
    return true;
  }
}