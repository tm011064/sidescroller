using UnityEngine;

public class TrampolineAutoBounceControlHandler : SingleUpdatePlayerControlHandler
{
  protected override void OnSingleUpdate()
  {
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    velocity.y = CalculateJumpHeight(velocity);
    velocity.y = Mathf.Max(
      GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity)
      , _playerController.jumpSettings.maxDownwardSpeed);

    velocity.x = GetDefaultHorizontalVelocity(velocity);

    _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);
  }

  public TrampolineAutoBounceControlHandler(PlayerController playerController, float jumpHeightMultiplier)
    : base(playerController, -1f)
  {
    this.jumpHeightMultiplier = jumpHeightMultiplier;
  }
}