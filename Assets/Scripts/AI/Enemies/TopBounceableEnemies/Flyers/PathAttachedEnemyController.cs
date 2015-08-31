using UnityEngine;

public class PathAttachedEnemyController : TopBounceableEnemyController
{
  protected override BaseControlHandler ApplyDamageControlHandler
  {
    get { return new DamageTakenPlayerControlHandler(); }
  }

  //void Start()
  //{
  //  // we insert in case there is already a control handler attached when spawned by another manager
  //  InsertControlHandler(0, new PathAttachedEnemyControlHandler());
  //}
  public override void Reset(Direction startDirection)
  {
    // TODO (Roman): does that break anything - there was a reason for inserting at 0, but can't remember why :(
    ResetControlHandlers(new PathAttachedEnemyControlHandler());
  }
}

