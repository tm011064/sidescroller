using UnityEngine;
using System.Collections;

public class PlayerControlHandler : BaseControlHandler
{
  #region members
  private const string TRACE_TAG = "PlayerControlHandler";

  protected PlayerController _playerController;
  protected PlayerMetricSettings _playerMetricSettings;

  protected float jumpHeightMultiplier = 1f;
  protected float? _fixedJumpHeight = null;

  protected bool _hasPerformedGroundJumpThisFrame = false;
  protected bool _hadDashPressedWhileJumpOff = false;

  protected AxisState horizontalAxisOverride;
  protected AxisState verticalAxisOverride;
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

    if (_playerController.isPerformingSpinMeleeAttack)
    {
      // we need to check whether the animation has finished. If so, we set the flag which will allow the player to do another attack.
      AnimatorStateInfo animatorStateInfo = _playerController.animator.GetCurrentAnimatorStateInfo(0);
      if (animatorStateInfo.IsName("PlayerSpinMeleeAttack"))
      {// we are already running the animation
        if (animatorStateInfo.normalizedTime > 1f)
        {
          // this means a full cycle has been performed, so we can stop the animation
          _playerController.isPerformingSpinMeleeAttack = false;
        }
        else
        {
          return;
        }
      }
      else
      {
        _playerController.animator.Play(Animator.StringToHash("PlayerSpinMeleeAttack"));
        return;
      }
    }

    if (_playerController.isTakingDamage)
    {
      _playerController.animator.Play(Animator.StringToHash("PlayerDamageTaken"));
    }
    else if (_playerController.isAttachedToWall
          && _playerController.characterPhysicsManager.velocity.y < 0f
          && (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.characterWallState & CharacterWallState.OnRightWall) != 0)
    {
      if (_playerController.sprite.transform.localScale.x < 1f)
        _playerController.sprite.transform.localScale = new Vector3(_playerController.sprite.transform.localScale.x * -1, _playerController.sprite.transform.localScale.y, _playerController.sprite.transform.localScale.z);

      _playerController.animator.Play(Animator.StringToHash("PlayerWallAttached"));
    }
    else if (_playerController.isAttachedToWall
      && _playerController.characterPhysicsManager.velocity.y < 0f
      && (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.characterWallState & CharacterWallState.OnLeftWall) != 0)
    {
      if (_playerController.sprite.transform.localScale.x > -1f)
        _playerController.sprite.transform.localScale = new Vector3(_playerController.sprite.transform.localScale.x * -1, _playerController.sprite.transform.localScale.y, _playerController.sprite.transform.localScale.z);

      _playerController.animator.Play(Animator.StringToHash("PlayerWallAttached"));
    }
    else if (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.below)
    {
      float yAxis = verticalAxisOverride == null ? _gameManager.inputStateManager.GetAxisState("Vertical").value : verticalAxisOverride.value;
      float xAxis = horizontalAxisOverride == null ? _gameManager.inputStateManager.GetAxisState("Horizontal").value : horizontalAxisOverride.value;

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

        if (!_playerController.isCrouching)
        {
          // we also need to adjust the collider size...
          _characterPhysicsManager.boxCollider.offset = _playerController.boxColliderOffsetCrouched;
          _characterPhysicsManager.boxCollider.size = _playerController.boxColliderSizeCrouched;

          _characterPhysicsManager.RecalculateDistanceBetweenRays();
          _playerController.isCrouching = true;

          Logger.Info("Crouch executed, box collider size set to: " + _characterPhysicsManager.boxCollider.size + ", offset: " + _characterPhysicsManager.boxCollider.offset);
        }
      }
      else
      {
        if (_playerController.isCrouching)
        {
          if (_characterPhysicsManager.CanMoveVertically(_characterPhysicsManager.boxCollider.size.y * .5f))
          {
            // we need to check whether we can stand up

            // we also need to adjust the collider size...
            _characterPhysicsManager.boxCollider.offset = _playerController.boxColliderOffsetDefault;
            _characterPhysicsManager.boxCollider.size = _playerController.boxColliderSizeDefault;

            _characterPhysicsManager.RecalculateDistanceBetweenRays();
            _playerController.isCrouching = false;

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

        if (!_playerController.isCrouching)
        {
          if (xAxis > -threshold
            && xAxis < threshold)
          {
            _playerController.animator.Play(Animator.StringToHash("PlayerIdle"));
          }
          else
          {
            if ((xAxis > 0f && _playerController.sprite.transform.localScale.x < 1f)
              || (xAxis < 0f && _playerController.sprite.transform.localScale.x > -1f))
            {// flip local scale
              _playerController.sprite.transform.localScale = new Vector3(_playerController.sprite.transform.localScale.x * -1, _playerController.sprite.transform.localScale.y, _playerController.sprite.transform.localScale.z);
            }

            _playerController.animator.Play(Animator.StringToHash("PlayerRun"));
          }
        }
      }
    }
    else
    {
      float xAxis = horizontalAxisOverride == null ? _gameManager.inputStateManager.GetAxisState("Horizontal").value : horizontalAxisOverride.value;
      if ((xAxis > 0f && _playerController.sprite.transform.localScale.x < 1f)
        || (xAxis < 0f && _playerController.sprite.transform.localScale.x > -1f))
      {// flip local scale
        _playerController.sprite.transform.localScale = new Vector3(_playerController.sprite.transform.localScale.x * -1, _playerController.sprite.transform.localScale.y, _playerController.sprite.transform.localScale.z);
      }

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
    if (canBreakUpMovement && velocity.y > 0f
      && (_gameManager.inputStateManager.GetButtonState("Jump").buttonPressState & ButtonPressState.IsUp) != 0)
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
    if (_fixedJumpHeight.HasValue)
    {
      return Mathf.Sqrt(
            2f
            * -_playerController.jumpSettings.gravity
            * _fixedJumpHeight.Value
            );
    }
    else
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
  }

  protected float GetJumpVerticalVelocity(Vector3 velocity, bool canJump, out bool hasJumped
    , ButtonPressState allowedJumpButtonPressState = ButtonPressState.IsDown)
  {
    float value = velocity.y;
    hasJumped = false;
    _hasPerformedGroundJumpThisFrame = false;

    if (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.below)
    {
      _hadDashPressedWhileJumpOff = false; // we set this to false here as the value is only used when player jumps off, not when he is grounded
      value = 0f;
    }

    if (canJump
      && (_gameManager.inputStateManager.GetButtonState("Jump").buttonPressState & allowedJumpButtonPressState) != 0
      )
    {
      if (this.CanJump())
      {
        value = CalculateJumpHeight(velocity);
        hasJumped = true;
        _hasPerformedGroundJumpThisFrame = true;
        _hadDashPressedWhileJumpOff = (_gameManager.inputStateManager.GetButtonState("Dash").buttonPressState & ButtonPressState.IsPressed) != 0;

        _playerController.JumpedThisFrame();
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
    if ((_gameManager.inputStateManager.GetButtonState("Dash").buttonPressState & ButtonPressState.IsPressed) != 0)
    {
      if (                                                                // allow dash speed if
            _playerController.runSettings.enableRunning                   // running is enabled
            && (_characterPhysicsManager.lastMoveCalculationResult.collisionState.below                       // either the player is grounded
                || velocity.x > _playerController.runSettings.walkSpeed   // or the current horizontal velocity is higher than the walkspeed, meaning that the player jumped while running
                || velocity.x < -_playerController.runSettings.walkSpeed
                || _hadDashPressedWhileJumpOff
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

    float newVelocity = normalizedHorizontalSpeed * speed * groundedAdjustmentFactor;
    if (_playerController.jumpSettings.enableBackflipOnDirectionChange
      && _hasPerformedGroundJumpThisFrame
      && Mathf.Sign(newVelocity) != Mathf.Sign(velocity.x))
    {// Note: this only works if the jump velocity calculation is done before the horizontal calculation!
      return normalizedHorizontalSpeed * _playerController.jumpSettings.backflipOnDirectionChangeSpeed;
    }

    return Mathf.Lerp(velocity.x, newVelocity, Time.deltaTime * smoothedMovementFactor);
  }

  protected float GetDefaultHorizontalVelocity(Vector3 velocity)
  {
    AxisState horizontalAxis = horizontalAxisOverride == null ? _gameManager.inputStateManager.GetAxisState("Horizontal") : horizontalAxisOverride;
    float normalizedHorizontalSpeed = GetNormalizedHorizontalSpeed(horizontalAxis);

    return GetHorizontalVelocityWithDamping(velocity, horizontalAxis.value, normalizedHorizontalSpeed);
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
      && (_gameManager.inputStateManager.GetButtonState("Fall").buttonPressState & ButtonPressState.IsPressed) != 0
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
