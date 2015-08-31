using UnityEngine;

public class TargetShooterFromAboveController : TopBounceableEnemyController
{
  public float speed = 200f;
  public float gravity = -3960f;

  public float scanAngleClipping = 15f;
  public float scanRayLength = 2048f;
  public int totalScanRays = 24;
  public float shootIntervalDuration = 3f;
  public LayerMask scanRayCollisionLayers = 0;
  public GameObject projectileToSpawn;
  public int minProjectilesToInstanciate = 10;  
  public Vector2 maxVelocity = new Vector2(512f, 512f);

  [Tooltip("This is the duration the enemy spends at an edge before turning around and moving the other direction. Set to 0 if the player should turn around immediately.")]
  public float edgeTurnAroundPause = 0f;

  [Tooltip("This is the duration the enemy needs to have continuous sight of the player in order to trigger the detection mechanism. Use -1 if the player should be detected immediately.")]
  public float detectPlayerDuration = .5f;
  
  protected override BaseControlHandler ApplyDamageControlHandler
  {
    get { return new DamageTakenPlayerControlHandler(); }
  }

  //void Start()
  //{
  //  // we insert in case there is already a control handler attached when spawned by another manager
  //  InsertControlHandler(0, new TargetShooterFromAboveControlHandler(this, startDirection));
  //}
  public override void Reset(Direction startDirection)
  {
    // TODO (Roman): does that break anything - there was a reason for inserting at 0, but can't remember why :(
    ResetControlHandlers(new TargetShooterFromAboveControlHandler(this, startDirection));
  }
}

