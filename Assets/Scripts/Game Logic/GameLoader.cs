using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class JetpackSettings
{
  [Tooltip("The speed at which the jetpack travels")]
  public float jetpackSpeed = 600f;
  [Tooltip("The speed at which the jetpack can change direction. Higher value means faster change.")]
  public float airDamping = 2f;
  [Tooltip("If true, the jetpack will float in mid air when not propelled; otherwise, the jetpack will fall down controlled by the 'JetpackSettings -> Float Gravity' force.")]
  public bool autoFloatWithoutThrust = false;
  [Tooltip("If 'Auto Float Without Thrust' is disabled, this gravity will be used to move the player towards the ground.")]
  public float floatGravity = -200f;
}
[Serializable]
public class FloaterSettings
{
  public float floaterGravity = -100f;
  public float startFloatingDuringFallVelocityMultiplier = .1f;
  public float floaterGravityDecreaseInterpolationFactor = .05f;
  public float floaterInAirDampingOverride = 3f;
}
[Serializable]
public class LaserAimGunSetting
{
  [Tooltip("All layers that the scan rays can collide with. Should include platforms and player.")]
  public LayerMask scanRayDirectionDownCollisionLayers = 0;
  [Tooltip("All layers that the scan rays can collide with. Should include platforms and player.")]
  public LayerMask scanRayDirectionUpCollisionLayers = 0;
  [Tooltip("The length of the scan rays emitted from the position of this gameobject.")]
  public float scanRayLength = 1280;
  public float bulletsPerSecond = 10;
  [Tooltip("If true, bps rate will be real time rather than slowed time. This means that the bps is in unscaled time even though all the movement is in slow motion.")]
  public bool allowSlowMotionRealTimeBulletsPerSecond = true;
  [Tooltip("Constant speed of the bullet.")]
  public float bulletSpeed = 2000f;

  //public float aimSlowMotionTimeScaleFactor = .125f;
  [Tooltip("The time it takes to 'reload' slow motion time functionality. After the slow motion phase is over, it takes 'Interval Between Aiming' time before it can be used again.")]
  public float intervalBetweenAiming = 1f;
  public AnimationCurve slowMotionFactorMultplierCurve = new AnimationCurve(
    new Keyframe(0f, 1f)
    , new Keyframe(.2f, .125f)
    , new Keyframe(1.2f, .125f)
    , new Keyframe(2f, 1f));

  public bool doAutoAim = true;
  [Tooltip("Angle used for searching a target to auto aim/lock from the current aim direction.")]
  public float autoAimSearchAngle = 12f;
  [Range(1, 6)]
  public int totalAutoAimSearchRaysPerSide = 2;

  [Tooltip("If greater than 0, this will force shot direction to use predefined angles. For example, if the value is 90, there will be 4 possible shot angle directions: 0, 90, 180, 270. Set to -1 if not used.")]
  public float aimHelpAngle = 30;
}

[Serializable]
public class PowerUpSettings
{
  public FloaterSettings floaterSettings;
  public JetpackSettings jetpackSettings;
  public LaserAimGunSetting laserAimGunSetting;
}
[Serializable]
public class PlayerMetricSettings
{
  public float jumpReleaseUpVelocityMultiplier = .5f;
}
[Serializable]
public class PooledObjectItem
{
  public GameObject prefab;
  public int initialSize = 1;
}
[Serializable]
public class PooledObjects
{
  public PooledObjectItem basicPowerUpPrefab;
  public PooledObjectItem basicBullet;
  public PooledObjectItem defaultEnemyDeathParticlePrefab;
  public PooledObjectItem defaultPlayerDeathParticlePrefab;
}
[Serializable]
public class LogSettings
{
  public string logFile = @"Log\DefaultLog.txt";
  public int totalArchivedFilesToKeep = 3;
  public bool echoToConsole = true;
  public bool addTimeStamp = true;
  public bool breakOnError = true;
  public bool breakOnAssert = true;

  public List<string> enabledTraceTags = new List<string>();
  public bool enableAllTraceTags = false;
  public bool addTraceTagToMessage = true;
}
[Serializable]
public class PlayerDamageControlHandlerSettings
{
  public float duration = 3f;
  public float suspendPhysicsTime = 1f;
}

[Serializable]
public class GameSettings
{
  public PowerUpSettings powerUpSettings = new PowerUpSettings();
  public PlayerMetricSettings playerMetricSettings = new PlayerMetricSettings();
  public PooledObjects pooledObjects = new PooledObjects();
  public LogSettings logSettings = new LogSettings();
  public PlayerDamageControlHandlerSettings playerDamageControlHandlerSettings = new PlayerDamageControlHandlerSettings();
}

public class GameLoader : MonoBehaviour
{
  public GameObject gameManager;          //GameManager prefab to instantiate.
  public GameSettings gameSettings;

  void Awake()
  {
    Logger.Initialize(gameSettings.logSettings);

    if (GameManager.instance == null)
      Instantiate(gameManager);

    GameManager.instance.gameSettings = gameSettings;

    ObjectPoolingManager.Instance.RegisterPool(gameSettings.pooledObjects.basicPowerUpPrefab.prefab, gameSettings.pooledObjects.basicPowerUpPrefab.initialSize, int.MaxValue);
    ObjectPoolingManager.Instance.RegisterPool(gameSettings.pooledObjects.basicBullet.prefab, gameSettings.pooledObjects.basicBullet.initialSize, int.MaxValue);
    ObjectPoolingManager.Instance.RegisterPool(gameSettings.pooledObjects.defaultEnemyDeathParticlePrefab.prefab, gameSettings.pooledObjects.defaultEnemyDeathParticlePrefab.initialSize, int.MaxValue);
    ObjectPoolingManager.Instance.RegisterPool(gameSettings.pooledObjects.defaultPlayerDeathParticlePrefab.prefab, gameSettings.pooledObjects.defaultPlayerDeathParticlePrefab.initialSize, int.MaxValue);

    //if (SoundManager.instance == null)
    //  Instantiate(soundManager);

    GameManager.instance.LoadScene(); // it is important to call that here as it instanciates the player controller which needs to have the game settings set.
  }
}