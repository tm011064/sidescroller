using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PowerUpJetPackControlHandler : PlayerControlHandler
{
  private PowerUpSettings _powerUpSettings;
  private bool _isFloating;

  public PowerUpJetPackControlHandler(PlayerController playerController, float duration, PowerUpSettings powerUpSettings)
    : base(playerController, duration)
  {
    _powerUpSettings = powerUpSettings;

#if !FINAL
    // TODO (Release): remove this
    _playerController.GetComponent<SpriteRenderer>().color = new Color(1f, 0.2f, .5f, 1f);
#endif
  }

  public override void Dispose()
  {
#if !FINAL
    _playerController.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
#endif
  }

  private float _lastBulletTime;

  protected override bool DoUpdate()
  {
    // 1) when the player presses the Jump key, the jetpack will fire its engine and propel the player into the direction he his pointing towards
    // 2) when the Jump button is released, the player will still have some inertia before stopping in mid air
    Vector3 velocity = _playerController.characterPhysicsManager.velocity;

    AxisState horizontalAxisState = _gameManager.inputStateManager.GetAxisState("Horizontal");
    AxisState verticalAxisState = _gameManager.inputStateManager.GetAxisState("Vertical");

    if ((_gameManager.inputStateManager.GetButtonState("Jump").buttonPressState & ButtonPressState.IsPressed) != 0)
    {
      // we want to dash towards the direction the controller points to
      velocity.x = Mathf.Lerp(velocity.x, horizontalAxisState.value * _powerUpSettings.jetpackSettings.jetpackSpeed, Time.deltaTime * _powerUpSettings.jetpackSettings.airDamping);
      velocity.y = Mathf.Lerp(velocity.y, verticalAxisState.value * _powerUpSettings.jetpackSettings.jetpackSpeed, Time.deltaTime * _powerUpSettings.jetpackSettings.airDamping);
    }
    else
    {
      Debug.Log(_characterPhysicsManager.lastMoveCalculationResult.collisionState.below);
      if (_characterPhysicsManager.lastMoveCalculationResult.collisionState.below == true)
      {
        velocity.y = 0f;
        velocity.y = Mathf.Max(
          GetGravityAdjustedVerticalVelocity(velocity, _playerController.adjustedGravity, true)
          , _playerController.jumpSettings.maxDownwardSpeed);
        velocity.x = GetDefaultHorizontalVelocity(velocity);
      }
      else
      {
        if (velocity.x != 0f)
          velocity.x = Mathf.Lerp(velocity.x, 0f, Time.deltaTime * _powerUpSettings.jetpackSettings.airDamping);
        if (velocity.y != 0f)
          velocity.y = Mathf.Lerp(velocity.y, 0f, Time.deltaTime * _powerUpSettings.jetpackSettings.airDamping);

        if (velocity.x < 10f && velocity.x > -10f)
          velocity.x = 0f;
        if (velocity.y < 10f && velocity.y > -10f)
          velocity.y = 0f;
      }
    }

    // TODO (Roman): create another control handler for projectile weapons
    if ((_gameManager.inputStateManager.GetButtonState("Dash").buttonPressState & ButtonPressState.IsPressed) != 0)
    {// fire

      float autoFireBulletsPerSecond = 10f;
      if (_lastBulletTime + (1 / autoFireBulletsPerSecond) <= Time.time)
      {
        Vector2 direction = new Vector2(horizontalAxisState.value, verticalAxisState.value).normalized;

        if (direction.x == 0f && direction.y == 0f)
        {
          direction.x = _playerController.transform.localScale.x > 0f ? 1f : -1f;
        }

        GameObject bulletObject = ObjectPoolingManager.Instance.GetObject(_gameManager.gameSettings.pooledObjects.basicBullet.prefab.name);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        bullet.StartMove(_playerController.transform.position, direction * 2000f);

        float rot_z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //bulletObject.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90f);
        bulletObject.transform.rotation = Quaternion.Euler(0f, 0f, rot_z);

        _lastBulletTime = Time.time;
      }
    }

    _playerController.characterPhysicsManager.Move(velocity * Time.deltaTime);

    return true;
  }

}

