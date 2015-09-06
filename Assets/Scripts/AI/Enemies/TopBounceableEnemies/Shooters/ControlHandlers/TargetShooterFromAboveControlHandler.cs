using UnityEngine;

public class TargetShooterFromAboveControlHandler : EnemyControlHandler<TargetShooterFromAboveController>
{
  private ObjectPoolingManager _objectPoolingManager;
  private float _moveDirectionFactor;
  private float _playerInSightDuration = 0f;
  private BoxCollider2D _boxCollider2D;
  private float _lastShotTime;

  public TargetShooterFromAboveControlHandler(TargetShooterFromAboveController targetShooterFromAboveController, Direction startDirection)
    : base(targetShooterFromAboveController, -1f)
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;

    if (startDirection == Direction.Left)
      _moveDirectionFactor = -1f;
    else
      _moveDirectionFactor = 1f;

    _boxCollider2D = _enemyController.GetComponent<BoxCollider2D>();
  }

  protected override bool DoUpdate()
  {
    if (_playerInSightDuration == 0f // either we don't see the player
      || !_pauseAtEdgeEndTime.HasValue // or we have not reached the edge yet
      )
    {
      // first move in patrolling mode
      MoveHorizontally(ref _moveDirectionFactor, _enemyController.speed, _enemyController.gravity, PlatformEdgeMoveMode.TurnAround, _enemyController.edgeTurnAroundPause);
    }

    Vector3 raycastOrigin;
    raycastOrigin = _moveDirectionFactor > 0f
      ? new Vector3(_enemyController.gameObject.transform.position.x + _boxCollider2D.size.x / 2f, _enemyController.gameObject.transform.position.y)
      : new Vector3(_enemyController.gameObject.transform.position.x - _boxCollider2D.size.x / 2f, _enemyController.gameObject.transform.position.y);

    if (_playerInSightDuration > 0f
      && _playerInSightDuration > _enemyController.detectPlayerDuration
      && _pauseAtEdgeEndTime.HasValue)
    {
      // TODO (Roman): shoot, look at hazard spawn manager and copy/paste logic here
      if (_lastShotTime + _enemyController.shootIntervalDuration < Time.time)
      {
        GameObject spawnedProjectile = _objectPoolingManager.GetObject(_enemyController.projectileToSpawn.name);
        spawnedProjectile.transform.position = raycastOrigin;

        ProjectileController projectileController = spawnedProjectile.GetComponent<ProjectileController>();

        Logger.Assert(projectileController != null, "A projectile with ballistic trajectory must have a projectile controller script attached.");

        BallisticTrajectorySettings ballisticTrajectorySettings = new BallisticTrajectorySettings();
        ballisticTrajectorySettings.angle = 0f; // horizontal launch
        ballisticTrajectorySettings.projectileGravity = -200f;

        ballisticTrajectorySettings.endPosition = new Vector2(
          GameManager.instance.player.transform.position.x - raycastOrigin.x
          , Mathf.Min(GameManager.instance.player.transform.position.y - raycastOrigin.y, -1f)
          );

        Debug.Log("Endpos: " + ballisticTrajectorySettings.endPosition + ", " + (GameManager.instance.player.transform.position.y - raycastOrigin.y));

        projectileController.PushControlHandler(new BallisticProjectileControlHandler(projectileController, ballisticTrajectorySettings, _enemyController.maxVelocity));

        _lastShotTime = Time.time;
      }
    }

    #region now check whether we can see the player
    float startAngleRad = _moveDirectionFactor > 0f
      ? (-90f + _enemyController.scanAngleClipping / 2f) * Mathf.Deg2Rad
      : (-180f + _enemyController.scanAngleClipping / 2f) * Mathf.Deg2Rad;
    float endAngleRad = _moveDirectionFactor > 0f
      ? (0f - _enemyController.scanAngleClipping / 2f) * Mathf.Deg2Rad
      : (-90f - _enemyController.scanAngleClipping / 2f) * Mathf.Deg2Rad;
    float step = (endAngleRad - startAngleRad) / (float)(_enemyController.totalScanRays);

    bool isSeeingPlayer = false;
    for (float theta = endAngleRad; theta > startAngleRad - step / 2; theta -= step)
    {
      Vector2 vector = new Vector2((float)(_enemyController.scanRayLength * Mathf.Cos(theta)), (float)(_enemyController.scanRayLength * Mathf.Sin(theta)));

      RaycastHit2D raycastHit2D = Physics2D.Raycast(raycastOrigin, vector.normalized, vector.magnitude, _enemyController.scanRayCollisionLayers);
      if (raycastHit2D)
      {
        if (raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
          _playerInSightDuration += Time.deltaTime;
          isSeeingPlayer = true;
          DrawRay(raycastOrigin, raycastHit2D.point.ToVector3() - _enemyController.gameObject.transform.position, Color.red);
          break;
        }
        else
        {
          DrawRay(raycastOrigin, raycastHit2D.point.ToVector3() - _enemyController.gameObject.transform.position, Color.grey);
        }
      }
      else
      {
        DrawRay(raycastOrigin, vector, Color.grey);
      }
    }

    if (!isSeeingPlayer)
      _playerInSightDuration = 0f;
    else
    {
      if (_pauseAtEdgeEndTime.HasValue)
        _pauseAtEdgeEndTime += Time.deltaTime;
    }
    #endregion

    return true;
  }
}

