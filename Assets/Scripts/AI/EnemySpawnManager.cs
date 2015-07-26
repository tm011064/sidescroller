﻿using UnityEngine;
using System.Collections;
using System;

public enum RespawnMode
{
  SpawnOnce,
  SpawnContinuouslyWhenVisible,
  SpawnWhenDestroyed,
  SpawnWhenDestroyedAndGettingVisible,
}

public enum SpawnTriggerMode
{
  OnGettingVisible,
  OnSceneLoad
}

public partial class EnemySpawnManager : BaseMonoBehaviour
{
  public GameObject enemyToSpawn;

  public SpawnTriggerMode spawnTriggerMode = SpawnTriggerMode.OnGettingVisible;
  public RespawnMode respawnMode = RespawnMode.SpawnOnce;
  public float continuousSpawnInterval = 10f;
  public float visibiltyCheckInterval = .1f;
  public Direction startDirection = Direction.Right;
  public BallisticTrajectorySettings ballisticTrajectorySettings = new BallisticTrajectorySettings();

  [Range(1f / 30.0f, float.MaxValue)]
  public float respawnOnDestroyDelay = .1f;

  private GameObject _spawnedEnemy;
  private EnemyController _enemyController;

  private void Spawn()
  {
    switch (respawnMode)
    {
      case RespawnMode.SpawnContinuouslyWhenVisible: 
        GameObject spawnedEnemy = ObjectPoolingManager.Instance.GetObject(enemyToSpawn.name);

        EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();
        if (enemyController == null)
          throw new MissingComponentException("Enemies spawned by an enemy spawn manager must contain an EnemyController component.");

        enemyController.startDirection = startDirection;

        spawnedEnemy.transform.position = this.transform.position;

        Logger.Trace("SPAWNING at " + spawnedEnemy.transform.position + ", active: " + spawnedEnemy.activeSelf + ", layer: " + LayerMask.LayerToName(spawnedEnemy.layer));

        if (ballisticTrajectorySettings.isEnabled)
        {
          _enemyController.PushControlHandler(new BallisticTrajectoryControlHandler(enemyController.characterPhysicsManager
            , this.transform.position
            , this.transform.position + new Vector3(ballisticTrajectorySettings.endPosition.x, ballisticTrajectorySettings.endPosition.y, transform.position.z)
            , ballisticTrajectorySettings.projectileGravity
            , ballisticTrajectorySettings.angle));
        }
        break;

      default:
        _spawnedEnemy = ObjectPoolingManager.Instance.GetObject(enemyToSpawn.name);

        _enemyController = _spawnedEnemy.GetComponent<EnemyController>();
        if (_enemyController == null)
          throw new MissingComponentException("Enemies spawned by an enemy spawn manager must contain an EnemyController component.");

        _enemyController.startDirection = startDirection;

        _spawnedEnemy.transform.position = this.transform.position;

        Logger.Trace("SPAWNING at " + _spawnedEnemy.transform.position + ", active: " + _spawnedEnemy.activeSelf + ", layer: " + LayerMask.LayerToName(_spawnedEnemy.layer));

        if (ballisticTrajectorySettings.isEnabled)
        {
          _enemyController.PushControlHandler(new BallisticTrajectoryControlHandler(_enemyController.characterPhysicsManager
            , this.transform.position
            , this.transform.position + new Vector3(ballisticTrajectorySettings.endPosition.x, ballisticTrajectorySettings.endPosition.y, transform.position.z)
            , ballisticTrajectorySettings.projectileGravity
            , ballisticTrajectorySettings.angle));
        }

        // we need to remove the spawned enemy instance since it might be reused in the pool
        _enemyController.GotDisabled += enemyController_Disabled;
        break;
    }


  }

  void enemyController_Disabled()
  {
    _spawnedEnemy = null;
    if (_enemyController != null)
    {
      _enemyController.GotDisabled -= enemyController_Disabled;
      _enemyController = null;
    }

    if (respawnMode == RespawnMode.SpawnWhenDestroyed)
    {
      // we need to invoke here as the spawn method would set the enemy active while being currently deactivated...
      try
      {
        Invoke("Spawn", .1f);
      }
      catch (MissingReferenceException)
      {
        // we swallow that one, it happens on scene unload when an enemy disables after this object has been finalized
      }
    }
  }

  void OnDisable()
  {
    CancelInvoke();
  }

  IEnumerator SpawnContinuously()
  {
    while (true)
    {
      if (_isVisible)
        Spawn();

      yield return new WaitForSeconds(continuousSpawnInterval);
    }
  }

  void Start()
  {
    ObjectPoolingManager.Instance.RegisterPool(enemyToSpawn, 1, int.MaxValue);

    if (spawnTriggerMode == SpawnTriggerMode.OnSceneLoad)
    {
      Spawn();
    }

    if (respawnMode == RespawnMode.SpawnWhenDestroyedAndGettingVisible
      || spawnTriggerMode == SpawnTriggerMode.OnGettingVisible)
    {
      StartVisibilityChecks(visibiltyCheckInterval, GetComponent<Collider2D>());
    }

    if (respawnMode == RespawnMode.SpawnContinuouslyWhenVisible)
    {
      StartCoroutine(SpawnContinuously());
    }
  }

  protected override void OnGotVisible()
  {
    if (spawnTriggerMode == SpawnTriggerMode.OnGettingVisible)
    {
      if (_spawnedEnemy == null
          || !_spawnedEnemy.activeSelf // when player kills enemy it will be set inactive since it is pooled...
          )
      {
        Spawn();
        return; // exit here so we don't progress to later checks...
      }
    }

    if (respawnMode == RespawnMode.SpawnWhenDestroyedAndGettingVisible)
    {
      if (_spawnedEnemy == null
          || !_spawnedEnemy.activeSelf // when player kills enemy it will be set inactive since it is pooled...
          )
      {
        Spawn();
        return; // exit here so we don't progress to later checks...
      }
    }
  }
}
