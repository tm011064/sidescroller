using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum RespawnMode
{
  /// <summary>
  /// Spawn only one time
  /// </summary>
  SpawnOnce,
  /// <summary>
  /// Spawn continuously as long as the object is enabled
  /// </summary>
  SpawnContinuously,
  /// <summary>
  /// Spawn when the previously spawned object is destroyed
  /// </summary>
  SpawnWhenDestroyed
}

public partial class EnemySpawnManager : SpawnBucketItemBehaviour
{
  #region fields

  #region inspector
  [SpawnableItemAttribute]
  public GameObject enemyToSpawn;

  public RespawnMode respawnMode = RespawnMode.SpawnOnce;
  public bool destroySpawnedEnemiesWhenGettingDisabled = false;
  public float continuousSpawnInterval = 10f;
  public Direction startDirection = Direction.Right;
  public BallisticTrajectorySettings ballisticTrajectorySettings = new BallisticTrajectorySettings();

  [Range(1f / 30.0f, float.MaxValue)]
  public float respawnOnDestroyDelay = .1f;
  #endregion

  #region private
  private List<GameObject> _spawnedEnemies = new List<GameObject>();
  private ObjectPoolingManager _objectPoolingManager;
  private bool _isDisabling;
  private float _nextSpawnTime;
  #endregion

  #endregion

  #region methods
  private void Spawn()
  {
    GameObject spawnedEnemy = _objectPoolingManager.GetObject(enemyToSpawn.name, this.transform.position);

    EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();
    if (enemyController == null)
      throw new MissingComponentException("Enemies spawned by an enemy spawn manager must contain an EnemyController component.");

    enemyController.Reset(startDirection);

    Logger.Trace("Spawning enemy from " + this.gameObject.name + " at " + spawnedEnemy.transform.position + ", active: " + spawnedEnemy.activeSelf + ", layer: " + LayerMask.LayerToName(spawnedEnemy.layer));

    if (ballisticTrajectorySettings.isEnabled)
    {
      enemyController.PushControlHandler(new BallisticTrajectoryControlHandler(enemyController.characterPhysicsManager
        , this.transform.position
        , this.transform.position + new Vector3(ballisticTrajectorySettings.endPosition.x, ballisticTrajectorySettings.endPosition.y, transform.position.z)
        , ballisticTrajectorySettings.projectileGravity
        , ballisticTrajectorySettings.angle));
    }

    enemyController.GotDisabled += enemyController_Disabled;
    _spawnedEnemies.Add(spawnedEnemy);
  }

  void enemyController_Disabled(BaseMonoBehaviour obj)
  {
    obj.GotDisabled -= enemyController_Disabled; // unsubscribed cause this could belong to a pooled object

    if (!_isDisabling)
    {
      // while we are disabling this object we don't want to touch the spawned enemies list nor respawn

      _spawnedEnemies.Remove(obj.gameObject);

      try
      {
        if (this.isActiveAndEnabled && respawnMode == RespawnMode.SpawnWhenDestroyed)
        {
          // we need to invoke here as the spawn method would set the enemy active while being currently deactivated...
          Invoke("Spawn", respawnOnDestroyDelay);
        }
      }
      catch (MissingReferenceException)
      {
        // we swallow that one, it happens on scene unload when an enemy disables after this object has been finalized
      }
    }
  }

  void Update()
  {
    if (_nextSpawnTime >= 0f
      && Time.time > _nextSpawnTime)
    {
      Spawn();

      if (respawnMode == RespawnMode.SpawnContinuously && continuousSpawnInterval > 0f)
      {
        _nextSpawnTime = Time.time + continuousSpawnInterval;
      }
      else
      {
        _nextSpawnTime = -1f;
      }
    }
  }

  #endregion

  #region monobehaviour
  public void DeactivateSpawnedObjects()
  {
    _isDisabling = true;
    for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
    {
      _objectPoolingManager.Deactivate(_spawnedEnemies[i]);
      _spawnedEnemies.RemoveAt(i);
    }
    _isDisabling = false;
  }

  void OnDisable()
  {
    Logger.Trace("Disabling EnemySpawnManager " + this.name);
    if (destroySpawnedEnemiesWhenGettingDisabled)
    {
      _isDisabling = true;
      for (int i = _spawnedEnemies.Count - 1; i >= 0; i--)
      {
        _objectPoolingManager.Deactivate(_spawnedEnemies[i]);
        _spawnedEnemies.RemoveAt(i);
      }
      _isDisabling = false;
    }
  }
  void OnEnable()
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;

    Logger.Trace("Enabling EnemySpawnManager " + this.name);

    // TODO (Roman): all this should be done at scene load, not here
    _objectPoolingManager.RegisterPool(enemyToSpawn, 1, int.MaxValue);

    _nextSpawnTime = Time.time;
  }
  #endregion
}
