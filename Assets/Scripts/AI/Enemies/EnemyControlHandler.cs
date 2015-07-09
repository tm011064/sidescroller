using UnityEngine;
using System.Collections;

public class EnemyControlHandler<TEnemyController>
  : BaseControlHandler
  where TEnemyController : EnemyController
{
  #region members
  protected TEnemyController _enemyController;
  #endregion

  #region constructors
  public EnemyControlHandler(TEnemyController enemyController)
    : this(enemyController, -1f) { }
  public EnemyControlHandler(TEnemyController enemyController, float duration)
    : base(enemyController.characterPhysicsManager, duration)
  {
    _enemyController = enemyController;
  }
  #endregion
}
