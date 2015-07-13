﻿using System;
using UnityEngine;

[Serializable]
public class WallJumpSettings
{
  public float wallJumpEnabledTime = .5f;
  public float wallVelocityDownThreshold = -200f;
  public float wallJumpPushOffAxisOverrideTime = .2f;
  public float wallStickGravity = -100f;
  public float wallJumpWallEvaluationDuration = .1f;
  [Tooltip("Set to -1 in order to disable levitation")]
  public float wallAttachedInitialLevitationDuration = 0.12f;
  public float maxWallDownwardSpeed = -1000f;
  public float slideToLevitationDamping = 18f;
}

[Serializable]
public class RunSettings
{
  public float walkSpeed = 600f;
  public float runSpeed = 900f;
  public float accelerationGroundDamping = 5f; // how fast do we change direction? higher means faster
  public float decelerationGroundDamping = 20f; // how fast do we change direction? higher means faster
}

[Serializable]
public class JumpSettings
{
  public float gravity = -3960f;
  public float walkJumpHeight = 380f;
  public float runJumpHeight = 400f;
  public float runJumpHeightSpeedTrigger = 800f;
  public float inAirDamping = 2.5f;
  public float allowJumpAfterGroundLostThreashold = .05f;
  public float maxDownwardSpeed = -1800f;
}


public partial class PlayerController : BaseCharacterController
{
  private const string TRACE_TAG = "PlayerController";

  public WallJumpSettings wallJumpSettings = new WallJumpSettings();
  public JumpSettings jumpSettings = new JumpSettings();
  public RunSettings runSettings = new RunSettings();

  public Vector2 boxColliderOffsetCrouched = Vector2.zero;
  public Vector2 boxColliderSizeCrouched = Vector2.zero;

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

  private RaycastHit2D _lastControllerColliderHit;
  private Vector3 _velocity;

  private WallJumpControlHandler _reusableWallJumpControlHandler;
  private WallJumpEvaluationControlHandler _reusableWallJumpEvaluationControlHandler;
  private Vector3 _lastLostGroundPos = Vector3.zero;

  private GameManager _gameManager;

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

      if (!characterPhysicsManager.isGrounded
        && (characterPhysicsManager.lastMoveCalculationResult.collisionState.characterWallState & CharacterWallState.OnWall) != 0)
      {
        // wall jumps work like this: if the player makes contact with a wall, we want to keep track how long he moves towards the
        // wall (based on input axis). If a certain threshold is reached, we are "attached to the wall" which will result in a slight levitation
        // period when reaching peak height and a reduced "slide down" gravity. When a player is on a wall, he can not detach by pressing
        // the opposite direction - the only way to detach is to jump.

        if (this.CurrentControlHandler != this._reusableWallJumpControlHandler
          && this.CurrentControlHandler != this._reusableWallJumpEvaluationControlHandler)
        {
          // new event, start evaluation
          if (characterPhysicsManager.lastMoveCalculationResult.collisionState.left)
          {
            _reusableWallJumpControlHandler.Reset(wallJumpSettings.wallJumpWallEvaluationDuration + wallJumpSettings.wallJumpEnabledTime, Direction.Left, wallJumpSettings);
            _reusableWallJumpEvaluationControlHandler.Reset(wallJumpSettings.wallJumpWallEvaluationDuration, Direction.Left, wallJumpSettings);
            this.PushControlHandler(_reusableWallJumpControlHandler, _reusableWallJumpEvaluationControlHandler);
          }
          else if (characterPhysicsManager.lastMoveCalculationResult.collisionState.right)
          {
            _reusableWallJumpControlHandler.Reset(wallJumpSettings.wallJumpWallEvaluationDuration + wallJumpSettings.wallJumpEnabledTime, Direction.Right, wallJumpSettings);
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

#if UNITY_EDITOR
  void OnDrawGizmos()
  {
    if (this.CurrentControlHandler != null)
    {
      this.CurrentControlHandler.DrawGizmos();
    }
  }
#endif

  public void Respawn()
  {
    characterPhysicsManager.velocity = Vector3.zero;
    characterPhysicsManager.transform.position = spawnLocation;

    adjustedGravity = jumpSettings.gravity;

    ResetControlHandlers(new GoodHealthPlayerControlHandler(this));
    _gameManager.powerUpManager.PowerMeter = 1;
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