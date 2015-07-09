using UnityEngine;

public class PathAttachedEnemyController : TopBounceableEnemyController
{
  protected override BaseControlHandler ApplyDamageControlHandler
  {
    get { return new DamageTakenPlayerControlHandler(); }
  }

  void Start()
  {
    PushControlHandler(new PathAttachedEnemyControlHandler());
  }
}

