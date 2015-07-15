using UnityEngine;

public class TrampolineBounceControlHandler : PlayerControlHandler
{
  private float _onTrampolineSkidDamping;
  private bool _canJump = false;

  private bool _hasJumped = false;
  public bool HasJumped { get { return _hasJumped; } }

  public TrampolineBounceControlHandler(PlayerController playerController, float duration, float jumpHeightMultiplier, float onTrampolineSkidDamping, bool canJump)
    : base(playerController, duration)
  {
    this.jumpHeightMultiplier = jumpHeightMultiplier;
    this._canJump = canJump;
    this._onTrampolineSkidDamping = onTrampolineSkidDamping;
  }

  protected override bool DoUpdate()
  {
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    if (!_hasJumped)
    {
      velocity.y = GetJumpVerticalVelocity(velocity, _canJump, out _hasJumped);
    }
    else
    {
      velocity.y = GetJumpVerticalVelocity(velocity, false);
    }
    velocity.y = Mathf.Max(
      GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity, false)
      , _playerController.jumpSettings.maxDownwardSpeed);

    // whilst on trampoline, player must not move horizontally
    if (!_hasJumped)
      velocity.x = Mathf.Lerp(velocity.x, 0f, _onTrampolineSkidDamping * Time.deltaTime);
    else
      velocity.x = GetDefaultHorizontalVelocity(velocity);

    _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);
    
    return (!_hasJumped) || velocity.y >= 0f; // exit if player falls down
  }
}
