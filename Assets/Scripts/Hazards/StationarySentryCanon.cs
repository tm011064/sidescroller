using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public interface IEnemyProjectile
{
  void StartMove(Vector2 startPosition, Vector2 direction, float acceleration, float targetVelocity);
}

public class StationarySentryCanon : SpawnBucketItemBehaviour, IObjectPoolBehaviour
{
  public GameObject projectilePrefab;
  public float projectileAcceleration = .05f;
  public float projectileTargetVelocity = 600f;

  public LayerMask scanRayCollisionLayers = 0;
  public float scanRayStartAngle = 0f;
  public float scanRayEndAngle = 360f;
  public float scanRayLength = 1280;

  public float timeNeededToDetectPlayer = .1f;
  [Range(.1f, 6000f)]
  public float roundsPerMinute = 30f;

  private float _startAngleRad;
  private float _endAngleRad;
  private float _rateOfFireInterval;

  private float _playerInSightDuration;
  private float _lastRoundFiredTime;

  private ObjectPoolingManager _objectPoolingManager;
  private PlayerController _playerController;

  [System.Diagnostics.Conditional("DEBUG")]
  private void DrawRay(Vector3 start, Vector3 dir, Color color)
  {
    Debug.DrawRay(start, dir, color);
  }

  void Awake()
  {
    _startAngleRad = scanRayStartAngle * Mathf.Deg2Rad;
    _endAngleRad = scanRayEndAngle * Mathf.Deg2Rad;
    _rateOfFireInterval = 60f / roundsPerMinute;
  }

  void OnEnable()
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;
    _playerController = GameManager.instance.player;

    Logger.Info("Enabled sentry canon " + this.GetHashCode());
  }

  void Update()
  {
    #region now check whether we can see the player
    bool isSeeingPlayer = false;

    Vector3 playerVector = _playerController.transform.position - this.transform.position;
    float angle = Mathf.Atan2(playerVector.y, playerVector.x);
    if (angle < 0f)
      angle += 2 * Mathf.PI;

    if (angle >= _startAngleRad && angle <= _endAngleRad)
    {
      RaycastHit2D raycastHit2D = Physics2D.Raycast(this.gameObject.transform.position, playerVector.normalized, playerVector.magnitude, scanRayCollisionLayers);
      if (raycastHit2D && raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
      {
        isSeeingPlayer = true;
        _playerInSightDuration += Time.deltaTime;
      }
    }

    DrawRay(this.gameObject.transform.position, playerVector, isSeeingPlayer ? Color.red : Color.gray);
    
    if (!isSeeingPlayer)
    {
      _playerInSightDuration = 0f;
    }

    if (_playerInSightDuration >= timeNeededToDetectPlayer)
    {
      if (_lastRoundFiredTime + _rateOfFireInterval <= Time.time)
      {
        GameObject enemyProjectileGameObject = _objectPoolingManager.GetObject(projectilePrefab.name, this.transform.position);
        IEnemyProjectile enemyProjectile = enemyProjectileGameObject.GetComponent<IEnemyProjectile>();
        Logger.Assert(enemyProjectile != null, "Enemy projectile must not be null");

        enemyProjectile.StartMove(this.transform.position, playerVector, projectileAcceleration, projectileTargetVelocity);

        _lastRoundFiredTime = Time.time;
      }
    }
    #endregion
  }

  #region IObjectPoolBehaviour Members

  public List<ObjectPoolRegistrationInfo> GetObjectPoolRegistrationInfos()
  {
    return new List<ObjectPoolRegistrationInfo>()
    {
      new ObjectPoolRegistrationInfo(projectilePrefab, 5)
    };
  }

  #endregion
}
