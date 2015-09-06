﻿using System.Collections.Generic;
using UnityEngine;

public partial class BreakablePlatform : SpawnBucketItemBehaviour, IObjectPoolBehaviour
{
  #region nested classes
  public enum BreakMode
  {
    FallDown,
    Disappear
  }

  private class SpawnRoutine
  {
    private enum CurrentState
    {
      Idle,
      PlayerLanded,
      Falling,
    }

    public GameObject gameObject;

    private Vector2 _velocity = Vector2.zero;
    private ObjectPoolingManager _objectPoolingManager;
    private CurrentState _currentState;

    private float _fallStartTime;
    private float _respawnTime;
    private float _stableDuration;
    private float _fallGravity;

    /// <summary>
    /// Updates the specified start falling.
    /// </summary>
    /// <param name="startFalling">if set to <c>true</c> we just started falling.</param>
    public void Update(out bool startFalling)
    {
      startFalling = false;

      switch (_currentState)
      {
        case CurrentState.PlayerLanded:
          if (Time.time >= _fallStartTime)
          {
            _currentState = CurrentState.Falling;
            startFalling = true;
          }
          break;

        case CurrentState.Falling:
          Vector2 velocity = _velocity;
          velocity.y = velocity.y + _fallGravity * Time.deltaTime;
          gameObject.transform.Translate(velocity * Time.deltaTime, Space.World);
          _velocity = velocity;
          break;
      }
    }

    void attachPlayerControllerToObject_OnPlayerControllerGotGrounded()
    {
      if (_currentState == CurrentState.Idle)
      {
        _currentState = CurrentState.PlayerLanded;
        _fallStartTime = Time.time + _stableDuration;
      }
    }

    public SpawnRoutine(ObjectPoolingManager objectPoolingManager, GameObject platformPrefab, Vector3 spawnLocation, float stableDuration, float fallGravity)
    {
      _fallGravity = fallGravity;
      _stableDuration = stableDuration;
      _currentState = CurrentState.Idle;
      _objectPoolingManager = objectPoolingManager;
      _velocity = Vector2.zero;

      gameObject = _objectPoolingManager.GetObject(platformPrefab.name);
      gameObject.transform.position = spawnLocation;

      AttachPlayerControllerToObject attachPlayerControllerToObject = gameObject.GetComponent<AttachPlayerControllerToObject>();
      if (attachPlayerControllerToObject == null)
        throw new MissingComponentException("Game object " + gameObject.name + " must contain 'AttachPlayerControllerToObject' script.");

      attachPlayerControllerToObject.OnPlayerControllerGotGrounded += attachPlayerControllerToObject_OnPlayerControllerGotGrounded;
    }
  }
  #endregion

  #region fields

  #region inspector fields
  public float stableDuration = .5f;
  public float fallGravity = -3960f;
  [Tooltip("The amount of time it takes until the platfrom reappears once it fell. Set to -1 if respawning should be disabled.")]
  public float respawnTime = 2f;
  public GameObject platformPrefab;
  public BreakMode breakMode = BreakMode.FallDown;
  #endregion

  #region private fields
  private ObjectPoolingManager _objectPoolingManager;
  private List<SpawnRoutine> _spawnedObjects = new List<SpawnRoutine>();
  private Vector3 _spawnLocation;
  private float _nextSpawnTime = -1f;
  #endregion

  #endregion

  #region methods
  private void Spawn()
  {
    _spawnedObjects.Add(new SpawnRoutine(_objectPoolingManager, platformPrefab, _spawnLocation, stableDuration, fallGravity));
  }
  #endregion

  #region update
  void Update()
  {
    for (int i = _spawnedObjects.Count - 1; i >= 0; i--)
    {
      bool startFalling;
      _spawnedObjects[i].Update(out startFalling);

      if (respawnTime >= 0f && startFalling)
      {// if we just started to fall, we want to respawn

        switch (breakMode)
        {
          case BreakMode.Disappear:
            _objectPoolingManager.Deactivate(_spawnedObjects[i].gameObject);
            _spawnedObjects.RemoveAt(i);
            break;
        }

        _nextSpawnTime = Time.time + respawnTime;

        //Invoke("Spawn", respawnTime);
      }
    }

    if (_nextSpawnTime > 0f && Time.time >= _nextSpawnTime)
    {
      Spawn();
      _nextSpawnTime = -1f;
    }
  }
  #endregion

  #region start/awake
  void _objectPoolingManager_BeforeDeactivated(GameObject obj)
  {
    for (int i = _spawnedObjects.Count - 1; i >= 0; i--)
    {
      if (_spawnedObjects[i].gameObject == obj)
      {
        // object was deactivated, so we remove it from the list. That way it won't be called at the Update() method
        _spawnedObjects.RemoveAt(i);
        break;
      }
    }
  }

  void OnDisable()
  {
    for (int i = _spawnedObjects.Count - 1; i >= 0; i--)
    {
      _objectPoolingManager.Deactivate(_spawnedObjects[i].gameObject);
    }
    _spawnedObjects.Clear();
    _nextSpawnTime = -1f;
  }

  void OnEnable()
  {
    _spawnLocation = this.transform.position;

    if (_objectPoolingManager == null)
    {
      _objectPoolingManager = ObjectPoolingManager.Instance;
      
      switch (breakMode)
      {
        case BreakMode.FallDown:
          _objectPoolingManager.BeforeDeactivated += _objectPoolingManager_BeforeDeactivated;
          break;
      }
    }

    Spawn();
  }


  #endregion

  #region IObjectPoolBehaviour Members

  public List<ObjectPoolRegistrationInfo> GetObjectPoolRegistrationInfos()
  {
    return new List<ObjectPoolRegistrationInfo>()
    {
      new ObjectPoolRegistrationInfo(platformPrefab, 1)
    };
  }

  #endregion
}

