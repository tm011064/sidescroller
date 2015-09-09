using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class StationaryCanon : SpawnBucketItemBehaviour, IObjectPoolBehaviour
{
  #region nested
  [Serializable]
  public class CanonFireDirectionVectors
  {
    public List<Vector2> vectors = new List<Vector2>();
  }
  #endregion

  public GameObject projectilePrefab;
  public float projectileAcceleration = .05f;
  public float projectileTargetVelocity = 600f;
  public Space fireDirectionSpace = Space.World;

  public List<CanonFireDirectionVectors> fireDirectionVectorGroups = new List<CanonFireDirectionVectors>();

  public float roundsPerMinute = 30f;
  public bool onlyShootWhenInvisible;

  private float _rateOfFireInterval;

  private float _playerInSightDuration;
  private float _lastRoundFiredTime;

  private ObjectPoolingManager _objectPoolingManager;
  private PlayerController _playerController;

  private int _currentfireDirectionVectorGroupIndex = 0;

  private CameraController _cameraController;

  void Awake()
  {
    _rateOfFireInterval = 60f / roundsPerMinute;
    _cameraController = Camera.main.GetComponent<CameraController>();

    Logger.Assert(fireDirectionVectorGroups.Count > 0, "Please specify at least one fire direction vector. " + this.name);
  }

  void OnEnable()
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;
    _playerController = GameManager.instance.player;

    _currentfireDirectionVectorGroupIndex = 0;
  }

  void Update()
  {
    #region now check whether we can see the player
    if (!onlyShootWhenInvisible || _cameraController.IsPointVisible(this.transform.position))
    {
      if (_lastRoundFiredTime + _rateOfFireInterval <= Time.time)
      {
        for (int i = 0; i < fireDirectionVectorGroups[_currentfireDirectionVectorGroupIndex].vectors.Count; i++)
        {
          GameObject enemyProjectileGameObject = _objectPoolingManager.GetObject(projectilePrefab.name, this.transform.position);
          IEnemyProjectile enemyProjectile = enemyProjectileGameObject.GetComponent<IEnemyProjectile>();
          Logger.Assert(enemyProjectile != null, "Enemy projectile must not be null");

          Vector2 direction;
          if (fireDirectionSpace == Space.World)
            direction = fireDirectionVectorGroups[_currentfireDirectionVectorGroupIndex].vectors[i];
          else
            direction = this.transform.TransformDirection(fireDirectionVectorGroups[_currentfireDirectionVectorGroupIndex].vectors[i]);

          enemyProjectile.StartMove(this.transform.position, direction, projectileAcceleration, projectileTargetVelocity);
        }

        _currentfireDirectionVectorGroupIndex = _currentfireDirectionVectorGroupIndex == fireDirectionVectorGroups.Count - 1 ? 0 : _currentfireDirectionVectorGroupIndex + 1;
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
