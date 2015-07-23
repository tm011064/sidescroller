using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WallJumpSettings
{
  [Tooltip("If false, wall jumps are disabled.")]
  public bool enableWallJumps = true;
  [Tooltip("Specifies how long wall jumps are enabled after getting attached to a wall. Set to -1 if the player should always be able to jump off a wall once attached.")]
  public float wallJumpEnabledTime = .5f;
  [Tooltip("Specifies the max downward speed at which a player can jump off a wall when sliding down. If this value is higher than the \"Max Wall Downward Speed\" it won't be used.")]
  public float wallVelocityDownThreshold = -200f;
  [Tooltip("This is the duration of the period where the player can not move the character horizontally after the wall jump has been performed. This can be used to prevent the player from climbing up a wall by moving the player towards the wall after the jump.")]
  public float wallJumpPushOffAxisOverrideTime = .2f;
  [Tooltip("This is the gravity used when the player sticks to a wall and slides down.")]
  public float wallStickGravity = -100f;
  [Tooltip("After the player collided with a wall, the wall jump evaluation time is the duration at which the player can move away from the wall without getting attached. Once attached, the player can't move away except when jumping.")]
  public float wallJumpWallEvaluationDuration = .1f;
  [Tooltip("The downward max speed when sliding down a wall.")]
  public float maxWallDownwardSpeed = -1000f;
}

[Serializable]
public class RunSettings
{
  [Tooltip("The normal walk speed")]
  public float walkSpeed = 600f;
  [Tooltip("If this is enabled, the character can run faster when the player presses the dash button.")]
  public bool enableRunning = true;
  [Tooltip("The run/dash speed.")]
  public float runSpeed = 900f;
  [Tooltip("The ground damping used when accelerating. A higher value means slower acceleration.")]
  public float accelerationGroundDamping = 5f; // how fast do we change direction? higher means faster
  [Tooltip("The ground damping used when decelerating. A higher value means higher deceleration, a lower value means slower deceleration (skidding).")]
  public float decelerationGroundDamping = 20f; // how fast do we change direction? higher means faster
}

[Serializable]
public class JumpSettings
{
  [Tooltip("Default gravity")]
  public float gravity = -3960f;
  [Tooltip("Jump height when walking")]
  public float walkJumpHeight = 380f;
  [Tooltip("Jump height when running. Player must have exceeded the \"Run Jump Height Speed Trigger\" velocity in order for this height to be used.")]
  public float runJumpHeight = 400f;
  [Tooltip("Only once the character moves at a speed higher than this value the \"Run Jump Height\" will be applied.")]
  public float runJumpHeightSpeedTrigger = 800f;
  [Tooltip("This value defines how fast the character can change direction in mid air. Higher value means faster change.")]
  public float inAirDamping = 2.5f;
  [Tooltip("In order to facilitate jumps while running, this value gives the player some leeway and allows him to jump after falling of a platform. Useful when running at full speed.")]
  public float allowJumpAfterGroundLostThreashold = .05f;
  [Tooltip("The downward max speed when falling. Normally we don't want the player to accelerate indefinitely as it will make controlling the player very difficult.")]
  public float maxDownwardSpeed = -1800f;
}


public partial class PlayerController : BaseCharacterController
{
  #region fields

  #region const
  private const string TRACE_TAG = "PlayerController";
  #endregion

  #region inspector fields
  public WallJumpSettings wallJumpSettings = new WallJumpSettings();
  public JumpSettings jumpSettings = new JumpSettings();
  public RunSettings runSettings = new RunSettings();

  public Vector2 boxColliderOffsetCrouched = Vector2.zero;
  public Vector2 boxColliderSizeCrouched = Vector2.zero;
  #endregion

  #region public fields
  [HideInInspector]
  public Vector2 boxColliderOffsetDefault = Vector2.zero;
  [HideInInspector]
  public Vector2 boxColliderSizeDefault = Vector2.zero;
  [HideInInspector]
  public float adjustedGravity;
  [HideInInspector]
  public Animator animator;
  [HideInInspector]
  public BoxCollider2D boxCollider;
  [HideInInspector]
  public Vector3 spawnLocation;
  [HideInInspector]
  public GameObject currentPlatform = null;

  [HideInInspector]
  public bool isInvincible = false;
  [HideInInspector]
  public bool isTakingDamage = false;
  [HideInInspector]
  public bool isAttachedToWall = false;
  #endregion

  #region private fields
  private RaycastHit2D _lastControllerColliderHit;
  private Vector3 _velocity;

  private WallJumpControlHandler _reusableWallJumpControlHandler;
  private WallJumpEvaluationControlHandler _reusableWallJumpEvaluationControlHandler;
  private Vector3 _lastLostGroundPos = Vector3.zero;

  private GameManager _gameManager;
  #endregion

  #endregion

  #region awake/start
  void Awake()
  {
    // register with game context so this game object can be accessed everywhere
    _gameManager = GameManager.instance;

    Logger.Info("Playercontroller awoke and added to game context instance.");

    boxCollider = GetComponent<BoxCollider2D>();
    boxColliderOffsetDefault = boxCollider.offset;
    boxColliderSizeDefault = boxCollider.size;

    characterPhysicsManager = GetComponent<CharacterPhysicsManager>();
    animator = this.transform.GetComponent<Animator>();

    // listen to some events for illustration purposes
    characterPhysicsManager.onControllerCollidedEvent += onControllerCollider;
    characterPhysicsManager.onTriggerEnterEvent += onTriggerEnterEvent;
    characterPhysicsManager.onControllerLostGround += characterPhysicsManager_onControllerLostGround;
    characterPhysicsManager.onControllerBecameGrounded += characterPhysicsManager_onControllerBecameGrounded;

    _reusableWallJumpControlHandler = new WallJumpControlHandler(this);
    _reusableWallJumpEvaluationControlHandler = new WallJumpEvaluationControlHandler(this);

    PushControlHandler(new GoodHealthPlayerControlHandler(this));
    _gameManager.powerUpManager.PowerMeter = 1;

    adjustedGravity = jumpSettings.gravity;
  }
  #endregion

  #region events
  public class GroundedPlatformChangedEventArgs : EventArgs
  {
    public GameObject previousPlatform;
    public GameObject currentPlatform;

    public GroundedPlatformChangedEventArgs(GameObject previousPlatform, GameObject currentPlatform)
    {
      this.previousPlatform = previousPlatform;
      this.currentPlatform = currentPlatform;
    }
  }

  public event EventHandler<GroundedPlatformChangedEventArgs> OnGroundedPlatformChanged;

  void characterPhysicsManager_onControllerBecameGrounded(GameObject obj)
  {
    Logger.Trace(TRACE_TAG, "Jump distance: " + Mathf.RoundToInt((this.transform.position.x - _lastLostGroundPos.x)) + " px");
  }

  void characterPhysicsManager_onControllerLostGround()
  {
    if (currentPlatform != null)
    {
      var handler = OnGroundedPlatformChanged;
      if (handler != null)
      {
        GameObject previousGameObject = currentPlatform;
        currentPlatform = null;
        OnGroundedPlatformChanged(this, new GroundedPlatformChangedEventArgs(previousGameObject, currentPlatform));
      }
      else
      {
        currentPlatform = null;
      }
    }

#if UNITY_EDITOR
    _lastLostGroundPos = this.transform.position;
#endif
  }

  void onControllerCollider(RaycastHit2D hit)
  {
    // bail out on plain old ground hits cause they arent very interesting
    if (hit.normal.y == 1f)
    {
      if (currentPlatform != hit.collider.gameObject)
      {
        var handler = OnGroundedPlatformChanged;
        if (handler != null)
        {
          GameObject previousGameObject = currentPlatform;
          currentPlatform = hit.collider.gameObject;
          OnGroundedPlatformChanged(this, new GroundedPlatformChangedEventArgs(previousGameObject, currentPlatform));
        }
        else
        {
          currentPlatform = hit.collider.gameObject;
        }
      }
      return;
    }
    // TODO (Roman): these methods should be optimized and put into constant field...
    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Platforms"))
    {
      if (wallJumpSettings.enableWallJumps
        && !characterPhysicsManager.lastMoveCalculationResult.collisionState.below
        && characterPhysicsManager.lastMoveCalculationResult.deltaMovement.y < 0f
        && (characterPhysicsManager.lastMoveCalculationResult.collisionState.characterWallState & CharacterWallState.OnWall) != 0)
      {
        // wall jumps work like this: if the player makes contact with a wall, we want to keep track how long he moves towards the
        // wall (based on input axis). If a certain threshold is reached, we are "attached to the wall" which will result in a reduced "slide down"
        // gravity. When a player is on a wall, he can not detach by pressing the opposite direction - the only way to detach is to jump.
        if (this.CurrentControlHandler != this._reusableWallJumpControlHandler
          && this.CurrentControlHandler != this._reusableWallJumpEvaluationControlHandler)
        {
          float wallJumpEnabledTime = wallJumpSettings.wallJumpEnabledTime >= 0f ? wallJumpSettings.wallJumpWallEvaluationDuration + wallJumpSettings.wallJumpEnabledTime : -1f;
          // new event, start evaluation
          if (characterPhysicsManager.lastMoveCalculationResult.collisionState.left)
          {
            _reusableWallJumpControlHandler.Reset(wallJumpEnabledTime, Direction.Left, wallJumpSettings);
            _reusableWallJumpEvaluationControlHandler.Reset(wallJumpSettings.wallJumpWallEvaluationDuration, Direction.Left, wallJumpSettings);
            this.PushControlHandler(_reusableWallJumpControlHandler, _reusableWallJumpEvaluationControlHandler);
          }
          else if (characterPhysicsManager.lastMoveCalculationResult.collisionState.right)
          {
            _reusableWallJumpControlHandler.Reset(wallJumpEnabledTime, Direction.Right, wallJumpSettings);
            _reusableWallJumpEvaluationControlHandler.Reset(wallJumpSettings.wallJumpWallEvaluationDuration, Direction.Right, wallJumpSettings);
            this.PushControlHandler(_reusableWallJumpControlHandler, _reusableWallJumpEvaluationControlHandler);
          }
        }
      }
    }
  }

  void onTriggerEnterEvent(Collider2D col)
  {
    if (col.gameObject.layer == LayerMask.NameToLayer("Enemy"))
    {
      var enemyController = col.gameObject.GetComponent<EnemyController>();
      if (enemyController != null)
      {
        enemyController.onPlayerCollide(this);
      }
    }
  }

  #endregion

  #region gizmos
#if UNITY_EDITOR
  void OnDrawGizmos()
  {
    if (this.CurrentControlHandler != null)
    {
      this.CurrentControlHandler.DrawGizmos();
    }
  }
#endif
  #endregion

  public void Respawn()
  {
    characterPhysicsManager.Reset(spawnLocation);

    adjustedGravity = jumpSettings.gravity;

    ResetControlHandlers(new GoodHealthPlayerControlHandler(this));

    _gameManager.powerUpManager.PowerMeter = 1;

    this.transform.parent = null; // just in case we were still attached
  }
  
  protected override void Update()
  {
    if (_gameManager.inputStateManager.GetButtonState("SwitchPowerUp").IsUp)
    {
      _gameManager.powerUpManager.ApplyNextInventoryPowerUpItem();
    }

    base.Update();
  }
}