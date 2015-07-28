using UnityEngine;
using System.Collections;

public class PlayerControlHandler : BaseControlHandler
{
  #region members
  private const string TRACE_TAG = "PlayerControlHandler";

  protected PlayerController _playerController;
  protected PlayerMetricSettings _playerMetricSettings;

  private bool _isCrouching = false;
  protected float jumpHeightMultiplier = 1f;
  #endregion

  #region methods

  #region overrides
  public override void DrawGizmos()
  {
    if (doDrawDebugBoundingBox)
    {
      GizmoUtility.DrawBoundingBox(_playerController.transform.position + _playerController.boxCollider.offset.ToVector3()
       , _playerController.boxCollider.bounds.extents, debugBoundingBoxColor);
    }
  }

  protected override void OnAfterUpdate()
  {
    Logger.Trace(TRACE_TAG, "OnAfterUpdate -> Velocity: " + _characterPhysicsManager.velocity);

    if (_playerController.isTakingDamage)
    {
      _playerController.animator.Play(Animator.StringToHash("PlayerDamageTaken"));
    }
    else if (_playerController.isAttachedToWall
      && _playerController.characterPhysicsManager.velocity.y < 0f
      && (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.characterWallState & CharacterWallState.OnRightWall) != 0)
    {
      if (_playerController.transform.localScale.x < 1f)
        _playerController.transform.localScale = new Vector3(_playerController.transform.localScale.x * -1, _playerController.transform.localScale.y, _playerController.transform.localScale.z);

      _playerController.animator.Play(Animator.StringToHash("PlayerWallAttached"));
    }
    else if (_playerController.isAttachedToWall
      && _playerController.characterPhysicsManager.velocity.y < 0f
      && (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.characterWallState & CharacterWallState.OnLeftWall) != 0)
    {
      if (_playerController.transform.localScale.x > -1f)
        _playerController.transform.localScale = new Vector3(_playerController.transform.localScale.x * -1, _playerController.transform.localScale.y, _playerController.transform.localScale.z);

      _playerController.animator.Play(Animator.StringToHash("PlayerWallAttached"));
    }
    else if (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.below)
    {
      float yAxis = _gameManager.inputStateManager.GetAxisState("Vertical").value;
      float xAxis = _gameManager.inputStateManager.GetAxisState("Horizontal").value;

      float threshold = .1f; // TODO (Roman): hardcoded
      if (yAxis < 0f)
      {
        if (xAxis > -threshold
          && xAxis < threshold)
        {
          _playerController.animator.Play(Animator.StringToHash("PlayerCrouchIdle"));
        }
        else
        {
          _playerController.animator.Play(Animator.StringToHash("PlayerCrouchRun"));
        }

        if (!_isCrouching)
        {
          // we also need to adjust the collider size...
          _characterPhysicsManager.boxCollider.offset = _playerController.boxColliderOffsetCrouched;
          _characterPhysicsManager.boxCollider.size = _playerController.boxColliderSizeCrouched;

          _characterPhysicsManager.RecalculateDistanceBetweenRays();
          _isCrouching = true;

          Logger.Info("Crouch executed, box collider size set to: " + _characterPhysicsManager.boxCollider.size + ", offset: " + _characterPhysicsManager.boxCollider.offset);
        }
      }
      else
      {
        if (_isCrouching)
        {
          if (_characterPhysicsManager.CanMoveVertically(_characterPhysicsManager.boxCollider.size.y * .5f))
          {
            // we need to check whether we can stand up

            // we also need to adjust the collider size...
            _characterPhysicsManager.boxCollider.offset = _playerController.boxColliderOffsetDefault;
            _characterPhysicsManager.boxCollider.size = _playerController.boxColliderSizeDefault;

            _characterPhysicsManager.RecalculateDistanceBetweenRays();
            _isCrouching = false;

            Logger.Info("Crouch ended, box collider size set to: " + _characterPhysicsManager.boxCollider.size + ", offset: " + _characterPhysicsManager.boxCollider.offset);
          }
          else
          {
            if (xAxis > -threshold
              && xAxis < threshold)
            {
              _playerController.animator.Play(Animator.StringToHash("PlayerCrouchIdle"));
            }
            else
            {
              _playerController.animator.Play(Animator.StringToHash("PlayerCrouchRun"));
            }
          }
        }

        if (!_isCrouching)
        {
          if (xAxis > -threshold
            && xAxis < threshold)
          {
            _playerController.animator.Play(Animator.StringToHash("PlayerIdle"));
          }
          else
          {
            _playerController.animator.Play(Animator.StringToHash("PlayerRun"));
          }
        }
      }
    }
    else
    {
      if (_playerController.characterPhysicsManager.velocity.y >= 0f)
      {
        _playerController.animator.Play(Animator.StringToHash("PlayerJump"));
      }
      else
      {
        _playerController.animator.Play(Animator.StringToHash("PlayerFall"));
      }
    }
  }
  #endregion

  protected float GetGravityAdjustedVerticalVelocity(Vector3 velocity, float gravity, bool canBreakUpMovement)
  {
    // apply gravity before moving
    if (canBreakUpMovement && velocity.y > 0f && _gameManager.inputStateManager.GetButtonState("Jump").IsUp)
    {
      return (velocity.y + gravity * Time.deltaTime) * _playerMetricSettings.jumpReleaseUpVelocityMultiplier;
    }
    else
    {
      return velocity.y + gravity * Time.deltaTime;
    }
  }

  protected float CalculateJumpHeight(Vector2 velocity)
  {
    float absVelocity = Mathf.Abs(velocity.x);
    float jumpHeight;
    if (absVelocity >= _playerController.jumpSettings.runJumpHeightSpeedTrigger)
      jumpHeight = _playerController.jumpSettings.runJumpHeight;
    else if (absVelocity >= _playerController.jumpSettings.walkJumpHeightSpeedTrigger)
      jumpHeight = _playerController.jumpSettings.walkJumpHeight;
    else
      jumpHeight = _playerController.jumpSettings.standJumpHeight;

    return Mathf.Sqrt(
          2f
          * jumpHeightMultiplier
          * -_playerController.jumpSettings.gravity
          * jumpHeight
          );
  }

  protected float GetJumpVerticalVelocity(Vector3 velocity, bool canJump, out bool hasJumped)
  {
    float value = velocity.y;
    hasJumped = false;

    if (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.below)
      value = 0f;

    if (canJump && _gameManager.inputStateManager.GetButtonState("Jump").IsDown)
    {
      if (this.CanJump())
      {
        value = CalculateJumpHeight(velocity);
        hasJumped = true;

        Logger.Info("Ground Jump executed. Velocity y: " + value);
      }
    }

    return value;
  }
  protected float GetJumpVerticalVelocity(Vector3 velocity, bool canJump)
  {
    bool hasJumped;
    return GetJumpVerticalVelocity(velocity, canJump, out hasJumped);
  }
  protected float GetJumpVerticalVelocity(Vector3 velocity)
  {
    bool hasJumped;
    return GetJumpVerticalVelocity(velocity, true, out hasJumped);
  }

  protected float GetNormalizedHorizontalSpeed(AxisState hAxis)
  {
    float normalizedHorizontalSpeed;
    if (hAxis.value > 0f && hAxis.value >= hAxis.lastValue)
    {
      normalizedHorizontalSpeed = 1;
    }
    else if (hAxis.value < 0f && hAxis.value <= hAxis.lastValue)
    {
      normalizedHorizontalSpeed = -1;
    }
    else
    {
      normalizedHorizontalSpeed = 0;
    }
    return normalizedHorizontalSpeed;
  }

  /// <summary>
  /// Gets the horizontal velocity with damping.
  /// </summary>
  /// <param name="velocity">The velocity.</param>
  /// <param name="hAxis">The h axis.</param>
  /// <param name="normalizedHorizontalSpeed">The normalized horizontal speed.</param>
  /// <returns></returns>
  protected float GetHorizontalVelocityWithDamping(Vector3 velocity, float hAxis, float normalizedHorizontalSpeed)
  {
    float speed = _playerController.runSettings.walkSpeed;
    if (_gameManager.inputStateManager.GetButtonState("Dash").IsPressed)
    {
      if (                                                                // allow dash speed if
            _playerController.runSettings.enableRunning                   // running is enabled
            && (_characterPhysicsManager.lastMoveCalculationResult.collisionState.below                       // either the player is grounded
                || velocity.x > _playerController.runSettings.walkSpeed   // or the current horizontal velocity is higher than the walkspeed, meaning that the player jumped while running
                || velocity.x < -_playerController.runSettings.walkSpeed
            )
        )
      {
        speed = _playerController.runSettings.runSpeed;
      }
    }

    float smoothedMovementFactor;
    if (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.below)
    {
      if (normalizedHorizontalSpeed == 0f)
      {
        smoothedMovementFactor = _playerController.runSettings.decelerationGroundDamping;
      }
      else if (Mathf.Sign(normalizedHorizontalSpeed) == Mathf.Sign(velocity.x))
      {// accelerating...
        smoothedMovementFactor = _playerController.runSettings.accelerationGroundDamping;
      }
      else
        smoothedMovementFactor = _playerController.runSettings.decelerationGroundDamping;
    }
    else
    {
      smoothedMovementFactor = _playerController.jumpSettings.inAirDamping;
    }

    float groundedAdjustmentFactor = _playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.below ? Mathf.Abs(hAxis) : 1f;

    return Mathf.Lerp(velocity.x, normalizedHorizontalSpeed * speed * groundedAdjustmentFactor
      , Time.deltaTime * smoothedMovementFactor);
  }

  protected float GetDefaultHorizontalVelocity(Vector3 velocity)
  {
    AxisState axisState = _gameManager.inputStateManager.GetAxisState("Horizontal");
    float normalizedHorizontalSpeed = GetNormalizedHorizontalSpeed(axisState);

    return GetHorizontalVelocityWithDamping(velocity, axisState.value, normalizedHorizontalSpeed);
  }

  protected virtual bool CanJump()
  {
    if (!_characterPhysicsManager.CanMoveVertically(_characterPhysicsManager.boxCollider.size.y * .5f))
    {
      return false;
    }

    return (
          _playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.below
        || (Time.time - _playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.lastTimeGrounded < _playerController.jumpSettings.allowJumpAfterGroundLostThreashold)
        );
  }

  protected void CheckOneWayPlatformFallThrough()
  {
    if (_characterPhysicsManager.lastMoveCalculationResult.collisionState.below
      && _gameManager.inputStateManager.GetButtonState("Fall").IsPressed
      && _playerController.currentPlatform != null
      && _playerController.currentPlatform.layer == LayerMask.NameToLayer("OneWayPlatform"))
    {
      OneWayPlatform oneWayPlatform = _playerController.currentPlatform.GetComponent<OneWayPlatform>();

      Logger.Assert(oneWayPlatform != null, "OneWayPlatform " + _playerController.currentPlatform.name + " has no 'OneWayPlatform' script attached. This script is needed in order to allow the player to fall through.");

      if (oneWayPlatform != null)
      {
        oneWayPlatform.TriggerFall();
      }
    }
  }
  #endregion

  #region constructors
  public PlayerControlHandler(PlayerController playerController)
    : this(playerController, -1f) { }
  public PlayerControlHandler(PlayerController playerController, float duration)
    : base(playerController.characterPhysicsManager, duration)
  {
    _playerController = playerController;
    _playerMetricSettings = GameManager.instance.gameSettings.playerMetricSettings;
  }
  #endregion
}
