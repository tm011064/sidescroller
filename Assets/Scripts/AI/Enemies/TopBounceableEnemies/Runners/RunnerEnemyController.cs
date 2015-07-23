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
    PushControlHandler(new RunnerEnemyControlHandler(this, startDirection));
  }
}

