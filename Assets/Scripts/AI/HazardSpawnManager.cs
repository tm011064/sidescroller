using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public partial class HazardSpawnManager : BaseMonoBehaviour
{
  private ObjectPoolingManager _objectPoolingManager;
  private IEnumerator _spawnContinuouslyCoroutine;

  public GameObject projectileToSpawn;

  public SpawnTriggerMode spawnTriggerMode = SpawnTriggerMode.OnGettingVisible;
  public float continuousSpawnInterval = 10f;
  public float visibiltyCheckInterval = .1f;
  [Tooltip("Projectiles get pooled internally, so we want to reuse projectiles that have been destroyed. This number is the minimum number of projectiles available at all time.")]
  public int minProjectilesToInstanciate = 10;

  public BallisticTrajectorySettings ballisticTrajectorySettings = new BallisticTrajectorySettings();
  
  private void Spawn()
  {
    GameObject spawnedProjectile = ObjectPoolingManager.Instance.GetObject(projectileToSpawn.name);

    spawnedProjectile.transform.position = this.transform.position;
    Logger.Trace("Spawning projectile at " + spawnedProjectile.transform.position + ", active: " + spawnedProjectile.activeSelf + ", layer: " + LayerMask.LayerToName(spawnedProjectile.layer));

    if (ballisticTrajectorySettings.isEnabled)
    {
      ProjectileController projectileController = spawnedProjectile.GetComponent<ProjectileController>();

      Logger.Assert(projectileController != null, "A projectile with ballistic trajectory must have a projectile controller script attached.");

      projectileController.PushControlHandler(new BallisticProjectileControlHandler(projectileController, ballisticTrajectorySettings));           
    }
  }
    
  void OnDisable()
  {
    StopCoroutine(_spawnContinuouslyCoroutine);
  }
  void OnEnable()
  {
    if (spawnTriggerMode == SpawnTriggerMode.OnSceneLoad)
    {
      StopCoroutine(_spawnContinuouslyCoroutine);
      StartCoroutine(_spawnContinuouslyCoroutine);
    }
    else if (spawnTriggerMode == SpawnTriggerMode.OnGettingVisible)
    {
      StartVisibilityChecks(visibiltyCheckInterval, GetComponent<Collider2D>());
    }
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

  void Awake()
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;
    _spawnContinuouslyCoroutine = SpawnContinuously();
  }

  void Start()
  {
    _objectPoolingManager.RegisterPool(projectileToSpawn, minProjectilesToInstanciate, int.MaxValue);
  }

  protected override void OnGotHidden()
  {
    if (spawnTriggerMode == SpawnTriggerMode.OnGettingVisible)
    {
      StopCoroutine(_spawnContinuouslyCoroutine);
    }
  }

  protected override void OnGotVisible()
  {
    if (spawnTriggerMode == SpawnTriggerMode.OnGettingVisible)
    {
      StopCoroutine(_spawnContinuouslyCoroutine);
      StartCoroutine(_spawnContinuouslyCoroutine);
    }
  }
}
