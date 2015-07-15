using UnityEngine;

public class TrampolineAutoBounceControlHandler : PlayerControlHandler
{
  private bool _isFirstUpdate = true;

  protected override bool DoUpdate()
  {
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    if (_isFirstUpdate)
    {
      velocity.y = CalculateJumpHeight(velocity);
      _isFirstUpdate = false;
    }

    velocity.y = Mathf.Max(
        GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity, false)
        , _playerController.jumpSettings.maxDownwardSpeed);
    velocity.x = GetDefaultHorizontalVelocity(velocity);

    _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);

    return velocity.y >= 0f; // exit if player falls down
  }

  public TrampolineAutoBounceControlHandler(PlayerController playerController, float jumpHeightMultiplier)
    : base(playerController, -1f)
  {
    this.jumpHeightMultiplier = jumpHeightMultiplier;
  }
}