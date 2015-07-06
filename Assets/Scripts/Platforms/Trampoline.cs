using UnityEngine;
using System.Collections;

public class TrampolineBounceControlHandler : DefaultPlayerControlHandler
{
  public TrampolineBounceControlHandler(PlayerController playerController, float overrideEndTime, float jumpHeightMultiplier)
    : base(playerController, overrideEndTime)
  {
    this.jumpHeightMultiplier = jumpHeightMultiplier;
  }
}


public class Trampoline : MonoBehaviour
{
  public float jumpButtonDelayTime = .3f;
  public float jumpHeightMultiplier = 2f;

  void OnTriggerEnter2D(Collider2D col)
  {
    if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
    {
      PlayerController playerController = col.gameObject.GetComponent<PlayerController>();
      playerController.PushControlHandler(new TrampolineBounceControlHandler(playerController, Time.time + jumpButtonDelayTime, jumpHeightMultiplier));
    }
  }
}
