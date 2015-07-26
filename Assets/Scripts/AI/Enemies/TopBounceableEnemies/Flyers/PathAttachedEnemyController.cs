using UnityEngine;

public class PathAttachedEnemyController : TopBounceableEnemyController
{
  protected override BaseControlHandler ApplyDamageControlHandler
  {
    get { return new DamageTakenPlayerControlHandler(); }
  }

  void Start()
  {
    // we insert in case there is already a control handler attached when spawned by another manager
    InsertControlHandler(0, new PathAttachedEnemyControlHandler());
  }
}

