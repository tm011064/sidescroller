using UnityEngine;
using System.Collections;

public class ApplyDamageHazard : MonoBehaviour
{
  private GameManager _gameManager;

  void OnTriggerStay2D(Collider2D col)
  {
    if (col.gameObject == _gameManager.player.gameObject)
    {
      if (_gameManager.player.isInvincible)
        return;

      ObjectPoolingManager.Instance.Deactivate(this.gameObject);

      switch (_gameManager.powerUpManager.AddDamage())
      {
        case PowerUpManager.DamageResult.IsDead:
          return;

        default:

          _gameManager.player.PushControlHandler(new DamageTakenPlayerControlHandler());
          break;
      }
    }
  }

  void OnTriggerEnter2D(Collider2D col)
  {
    // we have to check for player as the hazard might have collided with a hazard destroy trigger
    if (col.gameObject == _gameManager.player.gameObject)
    {
      if (_gameManager.player.isInvincible)
        return;

      ObjectPoolingManager.Instance.Deactivate(this.gameObject);

      switch (_gameManager.powerUpManager.AddDamage())
      {
        case PowerUpManager.DamageResult.IsDead:
          return;

        default:

          _gameManager.player.PushControlHandler(new DamageTakenPlayerControlHandler());
          break;
      }
    }
  }

  void Awake()
  {
    _gameManager = GameManager.instance;
  }
}
