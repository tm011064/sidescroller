using UnityEngine;

public class FlyingFollowerEnemyController : EnemyController
{
  public float speed = 100f;
  public float smoothDampFactor = 2.5f;

  //void Start()
  //{
  //  // we insert in case there is already a control handler attached when spawned by another manager
  //  InsertControlHandler(0, new FlyingFollowerEnemyControlHandler(this));
  //}
  public override void Reset(Direction startDirection)
  {
    // TODO (Roman): does that break anything - there was a reason for inserting at 0, but can't remember why :(
    ResetControlHandlers(new FlyingFollowerEnemyControlHandler(this));
  }
}

