using UnityEngine;

public class ChaserEnemyController : TopBounceableEnemyController
{
  public float speed = 200f;
  public float chaseSpeed = 600f;
  public float gravity = -3960f;

  public float scanRayLength = 256f;
  public float scanRayAngle = 120f;
  public int totalScanRays = 12;
  public LayerMask scanRayCollisionLayers = 0;

  [Tooltip("This is the duration the enemy spends at an edge before turning around and moving the other direction. Set to 0 if the player should turn around immediately.")]
  public float edgeTurnAroundPause = 0f;

  [Tooltip("This is the duration the enemy needs to have continuous sight of the player in order to trigger the detection mechanism. Use -1 if the player should be detected immediately.")]
  public float detectPlayerDuration = .5f;

  [Tooltip("This is the duration the enemy chases the player once detected. Set to -1 if the chase should go on forever.")]
  public float totalChaseDuration = 2f;

  protected override BaseControlHandler ApplyDamageControlHandler
  {
    get { return new DamageTakenPlayerControlHandler(); }
  }

  //void Start()
  //{
  //  // we insert in case there is already a control handler attached when spawned by another manager
  //  InsertControlHandler(0, new PatrollingChaserEnemyControlHandler(this, startDirection));
  //}
  public override void Reset(Direction startDirection)
  {
    // TODO (Roman): does that break anything - there was a reason for inserting at 0, but can't remember why :(
    ResetControlHandlers(new PatrollingChaserEnemyControlHandler(this, startDirection));
  }
}

