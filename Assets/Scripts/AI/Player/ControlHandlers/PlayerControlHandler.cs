using UnityEngine;
using System.Collections;

public class PlayerControlHandler : BaseControlHandler
{
  protected PlayerController _playerController;
  protected PlayerMetricSettings _playerMetricSettings;

  protected float jumpHeightMultiplier = 1f;
  
  public override void DrawGizmos()
  {
    if (doDrawDebugBoundingBox)
    {
      GizmoUtility.DrawBoundingBox(_playerController.transform.position + _playerController.boxCollider.offset.ToVector3()
       , _playerController.boxCollider.bounds.extents, debugBoundingBoxColor);
    }
  }

  protected float GetGravityAdjustedVerticalVelocity(Vector3 velocity, float gravity)
  {
    // apply gravity before moving
    float value = velocity.y + gravity * Time.deltaTime;

    if (value > 0f && _playerController.inputStateManager["Jump"].IsUp)
    {
      // if we are still going up we want to reduce force so we "smoothly" finish the jump in mid air
      value *= _playerMetricSettings.jumpReleaseUpVelocityMultiplier;
    }

    return value;
  }

  protected float GetJumpVerticalVelocity(Vector3 velocity)
  {
    float value = velocity.y;

    if (_playerController.characterPhysicsManager.isGrounded)
      value = 0f;

    if (_playerController.inputStateManager["Jump"].IsDown)
    {
      if (this.CanJump())
      {
        value = Mathf.Sqrt(2f * _playerController.jumpSettings.walkJumpHeight * jumpHeightMultiplier * -_playerController.jumpSettings.gravity);

        Logger.Info("Ground Jump executed. Velocity y: " + velocity.y);
      }
    }

    return value;
  }

  protected float GetNormalizedHorizontalSpeed(float hAxis)
  {
    float normalizedHorizontalSpeed;
    if (hAxis > 0f)
    {
      normalizedHorizontalSpeed = 1;
    }
    else if (hAxis < 0f)
    {
      normalizedHorizontalSpeed = -1;
    }
    else
    {
      normalizedHorizontalSpeed = 0;
    }
    return normalizedHorizontalSpeed;
  }

  protected float GetHorizontalVelocityWithDamping(Vector3 velocity, float hAxis, float normalizedHorizontalSpeed)
  {
    float speed = _playerController.inputStateManager["Dash"].IsPressed ? _playerController.runSettings.runSpeed : _playerController.runSettings.walkSpeed;

    float smoothedMovementFactor;
    if (_playerController.characterPhysicsManager.isGrounded)
    {
      if (normalizedHorizontalSpeed == 0f)
      {
        smoothedMovementFactor = _playerController.runSettings.decelerationGroundDamping;  
      }
      else if (Mathf.Sign(normalizedHorizontalSpeed) == Mathf.Sign( velocity.x))
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

    float groundedAdjustmentFactor = _playerController.characterPhysicsManager.isGrounded ? Mathf.Abs(hAxis) : 1f;

    return Mathf.Lerp(velocity.x, normalizedHorizontalSpeed * speed * groundedAdjustmentFactor
      , Time.deltaTime * smoothedMovementFactor);
  }

  protected float GetDefaultHorizontalVelocity(Vector3 velocity)
  {
    float hAxis = Input.GetAxis("Horizontal");
    float normalizedHorizontalSpeed = GetNormalizedHorizontalSpeed(hAxis);
    
    return GetHorizontalVelocityWithDamping(velocity, hAxis, normalizedHorizontalSpeed);
  }

  protected virtual bool CanJump()
  {
    if (!_characterPhysicsManager.canMoveVertically(_characterPhysicsManager.boxCollider.size.y * .5f))
      return false;

    return (
          _playerController.characterPhysicsManager.isGrounded
        || (Time.time - _playerController.characterPhysicsManager.lastTimeGrounded < _playerController.jumpSettings.allowJumpAfterGroundLostThreashold) // give the player some leeway TODO (Roman): hardcoded
        );
  }

  protected void CheckOneWayPlatformFallThrough()
  {
    if (_characterPhysicsManager.isGrounded
      && _playerController.inputStateManager["Fall"].IsDown
      && _playerController.currentPlatform != null
      && _playerController.currentPlatform.layer == LayerMask.NameToLayer("OneWayPlatform"))
    {
      OneWayPlatformSpriteRenderer oneWayPlatformSpriteRenderer = _playerController.currentPlatform.GetComponent<OneWayPlatformSpriteRenderer>();
      if (oneWayPlatformSpriteRenderer != null)
      {
        oneWayPlatformSpriteRenderer.AllowFallThrough();
      }
    }
  }

  private bool _isCrouching = false;

  protected override void SetAnimation()
  {
    if (_playerController.characterPhysicsManager.isGrounded)
    {
      float yAxis = Input.GetAxis("Vertical");

      float threshold = _playerController.runSettings.walkSpeed * .05f;
      if (yAxis < 0f)
      {
        if (_playerController.characterPhysicsManager.velocity.x > -threshold
          && _playerController.characterPhysicsManager.velocity.x < threshold)
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
          _characterPhysicsManager.recalculateDistanceBetweenRays();
          Logger.Info("Crouch executed, box collider size set to: " + _characterPhysicsManager.boxCollider.size + ", offset: " + _characterPhysicsManager.boxCollider.offset);
          _isCrouching = true;
        }
      }
      else
      {
        if (_isCrouching)
        {
          if (_characterPhysicsManager.canMoveVertically(_characterPhysicsManager.boxCollider.size.y * .5f))
          {
            // we need to check whether we can stand up

            // we also need to adjust the collider size...
            _characterPhysicsManager.boxCollider.offset = _playerController.boxColliderOffsetDefault;
            _characterPhysicsManager.boxCollider.size = _playerController.boxColliderSizeDefault;
            _characterPhysicsManager.recalculateDistanceBetweenRays();
            Logger.Info("Crouch ended, box collider size set to: " + _characterPhysicsManager.boxCollider.size + ", offset: " + _characterPhysicsManager.boxCollider.offset);
            _isCrouching = false;
          }
          else
          {
            if (_playerController.characterPhysicsManager.velocity.x > -threshold
              && _playerController.characterPhysicsManager.velocity.x < threshold)
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
          if (_playerController.characterPhysicsManager.velocity.x > -threshold
            && _playerController.characterPhysicsManager.velocity.x < threshold)
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

  public PlayerControlHandler(PlayerController playerController, float overrideEndTime)
    : base(playerController.characterPhysicsManager, overrideEndTime)
  {
    _playerController = playerController;
    _playerMetricSettings = GameManager.instance.gameSettings.playerMetricSettings;
  }
}
