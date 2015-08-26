using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// TODO (Roman): move this somewhere else if needed
public class LaserGunAimContainer
{
  private GameObject _laserGunAimGameObject;
  private LineRenderer _laserLineRenderer;
  private SpriteRenderer _laserDotSpriteRenderer;

  private float _currentAimAngle;

  public void Activate()
  {
    if (!this._laserGunAimGameObject.activeSelf)
      this._laserGunAimGameObject.SetActive(true);
  }
  public void Deactivate()
  {
    if (this._laserGunAimGameObject.activeSelf)
      this._laserGunAimGameObject.SetActive(false);
  }

  public void UpdateAimAngle(Vector3 aimPosition)
  {
    _laserLineRenderer.SetPosition(0, Vector3.zero);
    _laserLineRenderer.SetPosition(1, aimPosition);

    _laserDotSpriteRenderer.transform.localPosition = aimPosition;
  }

  public void Initialize(Transform laserGunAimGameObjectTransform)
  {
    Logger.Assert(laserGunAimGameObjectTransform != null, "Player controller is expected to have a LaserGunAim child object. If this is no longer needed, remove this line in code.");

    this._laserLineRenderer = laserGunAimGameObjectTransform.GetComponent<LineRenderer>();
    this._laserDotSpriteRenderer = laserGunAimGameObjectTransform.GetComponentInChildren<SpriteRenderer>();
    this._laserGunAimGameObject = laserGunAimGameObjectTransform.gameObject;
    this._laserGunAimGameObject.SetActive(false); // we only want to activate this when the player performs the attack.
  }
}

public class PowerUpGunControlHandler : DefaultPlayerControlHandler
{
  private PowerUpSettings _powerUpSettings;
  private float _slowMotionTotalTime;
  private float _lastAimStartTime;
  private float _nextAvailableAimStartTime;
  private float _lastBulletTime;
  private bool _isAiming;

  private float _autoAimAngleStep;

  public PowerUpGunControlHandler(PlayerController playerController, float duration, PowerUpSettings powerUpSettings)
    : base(playerController, duration)
  {
    _powerUpSettings = powerUpSettings;

    _slowMotionTotalTime = _powerUpSettings.laserAimGunSetting.slowMotionFactorMultplierCurve.keys[_powerUpSettings.laserAimGunSetting.slowMotionFactorMultplierCurve.keys.Length - 1].time;

    if (_powerUpSettings.laserAimGunSetting.doAutoAim)
    {
      Logger.Assert(_powerUpSettings.laserAimGunSetting.totalAutoAimSearchRaysPerSide != 0, "TotalAutoAimSearchRaysPerSide must not be 0.");
      _autoAimAngleStep = _powerUpSettings.laserAimGunSetting.autoAimSearchAngle
        * Mathf.Deg2Rad
        * .5f
        / (float)_powerUpSettings.laserAimGunSetting.totalAutoAimSearchRaysPerSide;
    }

#if !FINAL
    // TODO (Release): remove this
    _playerController.sprite.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.4f, .3f, 1f);
#endif
  }

  public override void Dispose()
  {
    // reset stuff
    this.horizontalAxisOverride = null;
    this.verticalAxisOverride = null;
    Time.timeScale = 1f;
    _isAiming = false;

#if !FINAL
    _playerController.sprite.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
#endif
  }

  [System.Diagnostics.Conditional("DEBUG")]
  private void DrawRay(Vector3 start, Vector3 dir, Color color)
  {
    Debug.DrawRay(start, dir, color);
  }

  private void EvaluateShot(Vector2 aimVector) //  AxisState horizontalAxisState, AxisState verticalAxisState)
  {
    if ((_gameManager.inputStateManager.GetButtonState("Dash").buttonPressState & ButtonPressState.IsDown) != 0)
    {// fire
      if (_lastBulletTime + (1 / _powerUpSettings.laserAimGunSetting.bulletsPerSecond)
        * (_powerUpSettings.laserAimGunSetting.allowSlowMotionRealTimeBulletsPerSecond ? Time.timeScale : 1f)
        <= Time.time) // TODO (Roman): for the 
      {
        Vector2 direction = aimVector.normalized;

        if (direction.x == 0f && direction.y == 0f)
          direction.x = _playerController.sprite.transform.localScale.x > 0f ? 1f : -1f;

        GameObject bulletObject = ObjectPoolingManager.Instance.GetObject(_gameManager.gameSettings.pooledObjects.basicBullet.prefab.name, _playerController.transform.position);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        bullet.StartMove(_playerController.transform.position, direction * _powerUpSettings.laserAimGunSetting.bulletSpeed); // TODO (Roman): hardcoded

        float rot_z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bulletObject.transform.rotation = Quaternion.Euler(0f, 0f, rot_z);

        _lastBulletTime = Time.time;
      }
    }
  }

  private void PerformAimAction()
  {
    AxisState horizontalAxisState = _gameManager.inputStateManager.GetAxisState("Horizontal");
    AxisState verticalAxisState = _gameManager.inputStateManager.GetAxisState("Vertical");

    if ((_gameManager.inputStateManager.GetButtonState("Aim").buttonPressState & ButtonPressState.IsDown) != 0)
    {
      if (Time.unscaledTime >= _nextAvailableAimStartTime)
      {
        _lastAimStartTime = Time.unscaledTime;

        if (!_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.below)
        {// we are in air, so we want to keep momentum
          this.horizontalAxisOverride = horizontalAxisState.Clone();
          this.verticalAxisOverride = verticalAxisState.Clone();
        }
        _isAiming = true;
      }
    }
    else if ((_gameManager.inputStateManager.GetButtonState("Aim").buttonPressState & ButtonPressState.IsPressed) != 0)
    {
      if (_isAiming)
      {
        if (_lastAimStartTime + _slowMotionTotalTime < Time.unscaledTime)
        {
          _nextAvailableAimStartTime = Time.unscaledTime + _powerUpSettings.laserAimGunSetting.intervalBetweenAiming;
          _isAiming = false;
        }
      }
    }
    else if ((_gameManager.inputStateManager.GetButtonState("Aim").buttonPressState & ButtonPressState.IsUp) != 0)
    {
      if (_isAiming)
      {
        _nextAvailableAimStartTime = Time.unscaledTime + _powerUpSettings.laserAimGunSetting.intervalBetweenAiming;
        _isAiming = false;
      }
    }
    else
    {
      _isAiming = false;
    }

    if (_isAiming)
    {
      Time.timeScale = _powerUpSettings.laserAimGunSetting.slowMotionFactorMultplierCurve.Evaluate(Time.unscaledTime - _lastAimStartTime);
      _playerController.laserGunAimContainer.Activate(); // make sure it is active

      Vector2 aimVector = new Vector3(horizontalAxisState.value, verticalAxisState.value);

      if (_powerUpSettings.laserAimGunSetting.doAutoAim)
      {
        // first we shoot a straight ray towards the axis position
        RaycastHit2D straightRayHit = Physics2D.Raycast(_playerController.transform.position, aimVector.normalized, _powerUpSettings.laserAimGunSetting.scanRayLength
            , verticalAxisState.value > 0f ? _powerUpSettings.laserAimGunSetting.scanRayDirectionUpCollisionLayers : _powerUpSettings.laserAimGunSetting.scanRayDirectionDownCollisionLayers);
        if (straightRayHit && straightRayHit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {// we have a direct hit on an enemy, set the aim vector
          aimVector = straightRayHit.collider.gameObject.transform.position - _playerController.transform.position;
        }
        else
        {// no direct enemy hit, now go towards the outer search angle limits and test whether we hit an enemy
          bool hasHit = false;
          float straightVectorTheta = Mathf.Atan2(aimVector.y, aimVector.x);
          for (int i = 1; i <= _powerUpSettings.laserAimGunSetting.totalAutoAimSearchRaysPerSide; i++)
          {
            // test right first
            float theta = straightVectorTheta + (float)i * _autoAimAngleStep;
            Vector2 searchVector = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));

            DrawRay(_playerController.transform.position, searchVector.normalized * _powerUpSettings.laserAimGunSetting.scanRayLength, Color.yellow);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_playerController.transform.position, searchVector.normalized, _powerUpSettings.laserAimGunSetting.scanRayLength
                , verticalAxisState.value > 0f ? _powerUpSettings.laserAimGunSetting.scanRayDirectionUpCollisionLayers : _powerUpSettings.laserAimGunSetting.scanRayDirectionDownCollisionLayers);

            if (raycastHit2D && raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
              aimVector = raycastHit2D.collider.gameObject.transform.position - _playerController.transform.position;
              hasHit = true;
              break;
            }

            // then left
            theta = straightVectorTheta - (float)i * _autoAimAngleStep;
            searchVector = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));

            DrawRay(_playerController.transform.position, searchVector.normalized * _powerUpSettings.laserAimGunSetting.scanRayLength, Color.yellow);
            raycastHit2D = Physics2D.Raycast(_playerController.transform.position, searchVector.normalized, _powerUpSettings.laserAimGunSetting.scanRayLength
                , verticalAxisState.value > 0f ? _powerUpSettings.laserAimGunSetting.scanRayDirectionUpCollisionLayers : _powerUpSettings.laserAimGunSetting.scanRayDirectionDownCollisionLayers);
            if (raycastHit2D && raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
              aimVector = raycastHit2D.collider.gameObject.transform.position - _playerController.transform.position;
              hasHit = true;
              break;
            }
          }

          if (!hasHit)
          {// no enemy hit
            if (straightRayHit)
            {// if we initially hit something other than an enemy, use the hitpoint
              aimVector = straightRayHit.point.ToVector3() - _playerController.transform.position;
            }
            else
            {// otherwise, just aim the full length
              aimVector = aimVector.normalized * _powerUpSettings.laserAimGunSetting.scanRayLength;
            }
          }
        }
      }
      else
      {
        RaycastHit2D raycastHit2D = Physics2D.Raycast(_playerController.transform.position, aimVector.normalized, _powerUpSettings.laserAimGunSetting.scanRayLength
        , verticalAxisState.value > 0f ? _powerUpSettings.laserAimGunSetting.scanRayDirectionUpCollisionLayers : _powerUpSettings.laserAimGunSetting.scanRayDirectionDownCollisionLayers);

        if (raycastHit2D)
        {
          aimVector = raycastHit2D.point.ToVector3() - _playerController.transform.position;
        }
        else
        {
          aimVector = aimVector.normalized * _powerUpSettings.laserAimGunSetting.scanRayLength;
        }
      }

      _playerController.laserGunAimContainer.UpdateAimAngle(aimVector);

      EvaluateShot(aimVector);
    }
    else
    {
      // reset stuff
      this.horizontalAxisOverride = null;
      this.verticalAxisOverride = null;
      Time.timeScale = 1f;
      _isAiming = false;

      _playerController.laserGunAimContainer.Deactivate(); // make sure it is inactive

      EvaluateShot(new Vector2(horizontalAxisState.value, verticalAxisState.value));
    }
  }

  public override void OnAfterStackPeekUpdate()
  {
    PerformAimAction();
  }

  protected override bool DoUpdate()
  {
    PerformAimAction();
    return base.DoUpdate();
  }
}