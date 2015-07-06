using UnityEngine;
using System.Collections;

public class PowerUpItem : MonoBehaviour
{
  public PowerUpType powerUpType = PowerUpType.Basic;

  void OnTriggerEnter2D(Collider2D col)
  {
    // we know from the layer mask arrangement that this trigger can only be called by the player object
    GameManager.instance.powerUpManager.ApplyPowerUpItem(powerUpType);

    this.gameObject.SetActive(false);
  }
}
