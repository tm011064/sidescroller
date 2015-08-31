using UnityEngine;

public class RunnerEnemyController : TopBounceableEnemyController
{
  public float speed = 200f;
  public float gravity = -3960f;

  protected override BaseControlHandler ApplyDamageControlHandler
  {
    get { return new DamageTakenPlayerControlHandler(); }
  }

  public override void Reset(Direction startDirection)
  {
    // TODO (Roman): does that break anything - there was a reason for inserting at 0, but can't remember why :(
    ResetControlHandlers(new RunnerEnemyControlHandler(this, startDirection));
  }
}

