using UnityEngine;
using System.Collections;

public class TrampolineBounceControlHandler : DefaultPlayerControlHandler
{
  public TrampolineBounceControlHandler(PlayerController playerController, float duration, float jumpHeightMultiplier)
    : base(playerController, duration)
  {
    this.jumpHeightMultiplier = jumpHeightMultiplier;
  }
}

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

public abstract class SingleUpdatePlayerControlHandler : DefaultPlayerControlHandler
{
  private bool _hasCompleted = false;

  protected abstract void OnSingleUpdate();

  protected override bool DoUpdate()
  {
    if (!_hasCompleted)
    {
      OnSingleUpdate();

      _hasCompleted = true;
      return true;
    }
    return false;
  }

  public SingleUpdatePlayerControlHandler(PlayerController playerController, float duration)
    : base(playerController, duration)
  {
  }
}


public class Trampoline : BaseMonoBehaviour
{
  public float jumpButtonDelayTime = .3f;
  public float jumpHeightMultiplier = 2f;
  public bool autoBounce = false;

  void OnTriggerEnter2D(Collider2D col)
  {
    if (autoBounce)
    {
      GameManager.instance.player.PushControlHandler(new TrampolineAutoBounceControlHandler(GameManager.instance.player, jumpHeightMultiplier));
    }
    else
    {
      GameManager.instance.player.PushControlHandler(new TrampolineBounceControlHandler(GameManager.instance.player, jumpButtonDelayTime, jumpHeightMultiplier));
    }
  }
}
