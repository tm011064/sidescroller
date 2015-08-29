using UnityEngine;

public class FlyingFollowerEnemyController : EnemyController
{
  public float speed = 100f;
  public float smoothDampFactor = 2.5f;

  void Start()
  {
    // we insert in case there is already a control handler attached when spawned by another manager
    InsertControlHandler(0, new FlyingFollowerEnemyControlHandler(this));
  }
}

