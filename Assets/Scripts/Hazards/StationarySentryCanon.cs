using UnityEngine;
using System.Collections;
using System;

public interface IEnemyProjectile
{
  void StartMove(Vector2 startPosition, Vector2 direction, float acceleration, float targetVelocity);
}

public class StationarySentryCanon : MonoBehaviour
{
  public GameObject projectilePrefab;
  public float projectileAcceleration = .05f;
  public float projectileTargetVelocity = 600f;

  public LayerMask scanRayCollisionLayers = 0;
  public int totalScanRays = 36;
  public float scanRayStartAngle = 0f;
  public float scanRayEndAngle = 360f;
  public float scanRayLength = 1280;

  public float timeNeededToDetectPlayer = .1f;
  [Range(.1f, 6000f)]
  public float roundsPerMinute = 30f;  
  
  private float _startAngleRad;
  private float _endAngleRad;
  private float _step;
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
    _step = (_endAngleRad - _startAngleRad) / (float)totalScanRays;
    _rateOfFireInterval = 60f / roundsPerMinute;
  }

  void OnEnable()
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;
    _playerController = GameManager.instance.player;

    // TODO (Roman): this should be done by level manager...
    _objectPoolingManager.RegisterPool(projectilePrefab, 10, int.MaxValue);
  }

  void Update()
  {
    #region now check whether we can see the player
    bool isSeeingPlayer = false;
    for (float theta = _endAngleRad; theta > _startAngleRad - _step / 2; theta -= _step)
    {
      Vector2 vector = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
      RaycastHit2D raycastHit2D = Physics2D.Raycast(this.gameObject.transform.position, vector.normalized, scanRayLength, scanRayCollisionLayers);
      if (raycastHit2D)
      {
        if (raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
          _playerInSightDuration += Time.deltaTime;
          isSeeingPlayer = true;
          DrawRay(this.gameObject.transform.position, _playerController.transform.position - this.gameObject.transform.position, Color.red);
          break;
        }
        else
        {
          DrawRay(this.gameObject.transform.position, raycastHit2D.point.ToVector3() - this.gameObject.transform.position, Color.grey);
        }
      }
      else
      {
        DrawRay(this.gameObject.transform.position, vector * scanRayLength, Color.grey);
      }
    }

    if (!isSeeingPlayer)
      _playerInSightDuration = 0f;

    if (_playerInSightDuration >= timeNeededToDetectPlayer)
    {
      if (_lastRoundFiredTime + _rateOfFireInterval <= Time.time)
      {
         GameObject enemyProjectileGameObject = _objectPoolingManager.GetObject(projectilePrefab.name);
         IEnemyProjectile enemyProjectile = enemyProjectileGameObject.GetComponent<IEnemyProjectile>();
         Logger.Assert(enemyProjectile != null, "Enemy projectile must not be null");

         enemyProjectile.StartMove(this.transform.position, _playerController.transform.position - this.transform.position, projectileAcceleration, projectileTargetVelocity);

        _lastRoundFiredTime = Time.time;
      }
    }

    #endregion
  }
}
