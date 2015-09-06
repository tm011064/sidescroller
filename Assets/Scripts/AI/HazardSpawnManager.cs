using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public partial class HazardSpawnManager : SpawnBucketItemBehaviour, IObjectPoolBehaviour
{
  private ObjectPoolingManager _objectPoolingManager;
  private float _nextSpawnTime;

  [SpawnableItemAttribute]
  public GameObject projectileToSpawn;

  [Tooltip("Set to -1 if continuous spawning should be disabled.")]
  public float continuousSpawnInterval = 10f;
  [Tooltip("Projectiles get pooled internally, so we want to reuse projectiles that have been destroyed. This number is the minimum number of projectiles available at all time.")]
  public int minProjectilesToInstanciate = 10;

  public BallisticTrajectorySettings ballisticTrajectorySettings = new BallisticTrajectorySettings();

  private void Spawn()
  {
    GameObject spawnedProjectile = _objectPoolingManager.GetObject(projectileToSpawn.name);
    spawnedProjectile.transform.position = this.transform.position;

    Logger.Trace("Spawning projectile from " + this.GetHashCode() + " (" + this.transform.position + ") at " + spawnedProjectile.transform.position + ", active: " + spawnedProjectile.activeSelf + ", layer: " + LayerMask.LayerToName(spawnedProjectile.layer));

    if (ballisticTrajectorySettings.isEnabled)
    {
      ProjectileController projectileController = spawnedProjectile.GetComponent<ProjectileController>();

      Logger.Assert(projectileController != null, "A projectile with ballistic trajectory must have a projectile controller script attached.");

      projectileController.PushControlHandler(new BallisticProjectileControlHandler(projectileController, ballisticTrajectorySettings));
    }
  }

  void OnDisable()
  {
    Logger.Trace("Disabling HazardSpawnManager " + this.GetHashCode());
  }
  void OnEnable()
  {
    Logger.Trace("Enabling HazardSpawnManager " + this.GetHashCode());

    _nextSpawnTime = Time.time + continuousSpawnInterval;
  }
       
  void FixedUpdate()
  {
    // Note: we can not use a coroutine for this because when spawning on the OnEnable method the transform.position of a pooled object would
    // still point to the last active position when reactivated.
    if (continuousSpawnInterval > 0f)
    {
      if (Time.time > _nextSpawnTime)
      {
        Spawn();
        _nextSpawnTime = Time.time + continuousSpawnInterval;
      }
    }
  }

  void Awake()
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;
  }

  #region IObjectPoolBehaviour Members

  public List<ObjectPoolRegistrationInfo> GetObjectPoolRegistrationInfos()
  {
    return new List<ObjectPoolRegistrationInfo>()
    {
      new ObjectPoolRegistrationInfo(projectileToSpawn, minProjectilesToInstanciate)
    };
  }

  #endregion
}
