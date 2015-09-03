using UnityEngine;

public enum AxisType
{
  Horizontal,
  Vertical
}

public class AxisLockedChaserEnemyController : EnemyController
{
  public float speed = 800f;
  public float smoothDampFactorWhenDecelerationIsOn = 3f;
  public float smoothDampFactorWhenDecelerationIsOff = .5f;
  public float idleDistanceThreshold = 5f;
  public float decelerationDistanceMultiplicationFactor = .01f;
  public LayerMask movementBoundaryObjectLayerMask = 0;
  public AxisType axisType = AxisType.Horizontal;

  public int totalmovementBoundaryCheckRays = 3;

  //void Start()
  //{
  //  // we insert in case there is already a control handler attached when spawned by another manager
  //  InsertControlHandler(0, new FlyingFollowerEnemyControlHandler(this));
  //}
  public override void Reset(Direction startDirection)
  {
    // TODO (Roman): does that break anything - there was a reason for inserting at 0, but can't remember why :(
    ResetControlHandlers(new AxisLockedChaserEnemyControlHandler(this));
  }
}

