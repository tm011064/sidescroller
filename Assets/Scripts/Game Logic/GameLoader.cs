using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class PowerUpSettings
{
  public float floaterGravity = -100f;
  public float startFloatingDuringFallVelocityMultiplier = .1f;
  public float floaterGravityDecreaseInterpolationFactor = .05f;
  public float floaterInAirDampingOverride = 3f;
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

    //if (SoundManager.instance == null)
    //  Instantiate(soundManager);

    GameManager.instance.LoadScene(); // it is important to call that here as it instanciates the player controller which needs to have the game settings set.
  }
}