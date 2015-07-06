using System;
using UnityEngine;

[Serializable]
public class WallJumpSettings
{
  public float wallJumpEnabledTime = .5f;
  public float wallVelocityDownThreshold = -200f;
  public float wallJumpPushOffAxisOverrideTime = .2f;
  public float wallStickTime = .2f;
  public float wallStickGravity = -100f;
}

public partial class PlayerController : BaseCharacterController
{
  private const string TRACE_TAG = "PlayerController";

  // movement config
  public float gravity = -25f;
  public float runSpeed = 8f;
  public float groundDamping = 20f; // how fast do we change direction? higher means faster
  public float inAirDamping = 5f;
  public float jumpHeight = 3f;

  public WallJumpSettings wallJumpSettings = new WallJumpSettings();

  public float allowJumpAfterGroundLostThreashold = .1f;

  [HideInInspector]
  public float adjustedGravity;

  [HideInInspector]
  public Animator animator;

  [HideInInspector]
  public BoxCollider2D boxCollider;

  private RaycastHit2D _lastControllerColliderHit;
  private Vector3 _velocity;

  private float _currentJumpVelocity;
  private float _currentJumpVelocityMultiplier;

  private WallJumpControlHandler _reusableWallJumpControlHandler;

  [HideInInspector]
  public InputStateManager inputStateManager;

  private Vector3 _lastLostGroundPos = Vector3.zero;

  [HideInInspector]
  public Quaternion spriteRotation = Quaternion.Euler(0f, 0f, 0f);

  [HideInInspector]
  public Vector3 spawnLocation;

  [HideInInspector]
  public GameObject currentPlatform = null;

  void Awake()
  {
    // register with game context so this game object can be accessed everywhere
    GameManager.instance.player = this;
    Logger.Info("Playercontroller awoke and added to game context instance.");

    boxCollider = GetComponent<BoxCollider2D>();
    characterPhysicsManager = GetComponent<CharacterPhysicsManager>();
    animator = this.transform.GetComponent<Animator>();

    // listen to some events for illustration purposes
    characterPhysicsManager.onControllerCollidedEvent += onControllerCollider;
    characterPhysicsManager.onTriggerEnterEvent += onTriggerEnterEvent;
    characterPhysicsManager.onControllerLostGround += characterPhysicsManager_onControllerLostGround;
    characterPhysicsManager.onControllerBecameGrounded += characterPhysicsManager_onControllerBecameGrounded;

    inputStateManager = new InputStateManager("Jump", "Dash", "Fall", "SwitchPowerUp");

    _reusableWallJumpControlHandler = new WallJumpControlHandler(this, -1f, false, wallJumpSettings);

    _controlHandlers.Push(new GoodHealthPlayerControlHandler(this, float.MaxValue));

    adjustedGravity = gravity;
  }

  void characterPhysicsManager_onControllerBecameGrounded(GameObject obj)
  {
    Logger.Trace(TRACE_TAG, "Jump distance: " + Mathf.RoundToInt((this.transform.position.x - _lastLostGroundPos.x)) + " px");
  }

  void characterPhysicsManager_onControllerLostGround()
  {
    currentPlatform = null;

#if UNITY_EDITOR
    _lastLostGroundPos = this.transform.position;
#endif
  }

  #region Event Listeners

  void onControllerCollider(RaycastHit2D hit)
  {
    // bail out on plain old ground hits cause they arent very interesting
    if (hit.normal.y == 1f)
    {
      this.currentPlatform = hit.collider.gameObject;
      return;
    }
    // TODO (Roman): these methods should be optimized and put into constant field...
    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Platforms"))
    {
      //if (this.characterPhysicsManager.isGrounded && hit.collider is CircleCollider2D)
      //{
      //  // TODO (Roman):  this is all a quick hack. we should create a new playercontrolhandler which attaches the player to the circle
      //  //                and let's him move left and right along the circle path without the ground collision detection.

      //  // get angle
      //  Vector3 dir = transform.position - hit.collider.transform.position;
      //  float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

      //  float toAngle = 0f;
      //  if (angle >= 0f && angle <= 90f)
      //    toAngle = 90f - angle;
      //  else if (angle > 90f && angle <= 180f)
      //    toAngle = -(angle - 90);

      //  if (this.transform.localScale.x > 0)
      //    toAngle = -toAngle;

      //  spriteRotation = Quaternion.Euler(0f, 0f, toAngle);
      //}

      if (characterPhysicsManager.collisionState.isOnWall)
      {
        if (characterPhysicsManager.collisionState.left && !characterPhysicsManager.isGrounded)
        {
          if (_controlHandlers.Peek() != _reusableWallJumpControlHandler)
          {
            _reusableWallJumpControlHandler.Reset(Time.time + wallJumpSettings.wallJumpEnabledTime, false, wallJumpSettings);
            Logger.Info("Pushing handler: " + _reusableWallJumpControlHandler.ToString());
            _controlHandlers.Push(_reusableWallJumpControlHandler);
          }
        }
        else if (characterPhysicsManager.collisionState.right && !characterPhysicsManager.isGrounded)
        {
          if (_controlHandlers.Peek() != _reusableWallJumpControlHandler)
          {
            _reusableWallJumpControlHandler.Reset(Time.time + wallJumpSettings.wallJumpEnabledTime, true, wallJumpSettings);
            Logger.Info("Pushing handler: " + _reusableWallJumpControlHandler.ToString());
            _controlHandlers.Push(_reusableWallJumpControlHandler);
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

#if UNITY_EDITOR
  void OnDrawGizmos()
  {
    if (_controlHandlers.Count > 0)
    {
      _controlHandlers.Peek().DrawGizmos();
    }
  }
#endif

  public void PushControlHandler(PlayerControlHandler playerControlHandler)
  {
    _controlHandlers.Push(playerControlHandler);
  }

  public void Respawn()
  {
    characterPhysicsManager.velocity.x = 0f;
    characterPhysicsManager.velocity.y = 0f;

    characterPhysicsManager.transform.position = spawnLocation;

    _controlHandlers.Clear();
    _controlHandlers.Push(new GoodHealthPlayerControlHandler(this, float.MaxValue));
  }

  protected override void Update()
  {
    // TODO (Roman): input states should be updated elsewhere maybe?
    inputStateManager.Update();

    if (inputStateManager["SwitchPowerUp"].IsUp)
    {
      GameManager.instance.powerUpManager.ApplyNextInventotyPowerUpItem();
    }

    base.Update();
  }
}