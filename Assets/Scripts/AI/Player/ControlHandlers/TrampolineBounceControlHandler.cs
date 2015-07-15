using UnityEngine;

public class TrampolineBounceControlHandler : PlayerControlHandler
{
  private float _onTrampolineSkidDamping;

  public TrampolineBounceControlHandler(PlayerController playerController, float duration, float jumpHeightMultiplier, float onTrampolineSkidDamping)
    : base(playerController, duration)
  {
    this.jumpHeightMultiplier = jumpHeightMultiplier;
    _onTrampolineSkidDamping = onTrampolineSkidDamping;
  }

  protected override bool DoUpdate()
  {
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    velocity.y = GetJumpVerticalVelocity(velocity);
    velocity.y = Mathf.Max(
      GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity)
      , _playerController.jumpSettings.maxDownwardSpeed);

    // whilst on trampoline, player must not move horizontally
    velocity.x = Mathf.Lerp(velocity.x, 0f, _onTrampolineSkidDamping * Time.deltaTime);

    _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);

    return true;
  }
}
