using UnityEngine;

public class RunnerEnemyController : TopBounceableEnemyController
{
  public float speed = 200f;
  public float gravity = -3960f;

  protected override BaseControlHandler ApplyDamageControlHandler
  {
    get { return new DamageTakenPlayerControlHandler(); }
  }

  void Start()
  {
    // we insert in case there is already a control handler attached when spawned by another manager
    InsertControlHandler(0, new RunnerEnemyControlHandler(this, startDirection));
  }
}

