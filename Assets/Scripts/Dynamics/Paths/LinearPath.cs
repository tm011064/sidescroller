using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public partial class LinearPath : SpawnBucketItemBehaviour, IObjectPoolBehaviour
{
  #region nested classes
  public enum StartPosition
  {
    PathStart,
    Center
  }
  public enum LoopMode
  {
    Once,
    Loop,
    PingPong
  }
  public enum StartPathDirection
  {
    Forward,
    Backward
  }

  private class GameObjectTrackingInformation
  {
    public GameObject gameObject;
    public float percentage = 0f;
    public float directionMultiplicationFactor = 1f;
    public float nextStartTime = 0f;

    public GameObjectTrackingInformation(GameObject gameObject, float percentage, float directionMultiplicationFactor)
    {
      this.gameObject = gameObject;
      this.percentage = percentage;
      this.directionMultiplicationFactor = directionMultiplicationFactor;
    }
  }
  #endregion

  public Color outlineGizmoColor = Color.white;
  public bool showGizmoOutline = true;

  public EasingType easingType = EasingType.Linear;

  public GameObject objectToAttach;
  public int totalObjectsOnPath = 1;
  public LoopMode loopMode = LoopMode.Once;

  public bool useTime = true;
  public float time = 5f;
  public bool useSpeed = false;
  public float speedInUnitsPerSecond = 100;

  public MovingPlatformType movingPlatformType = MovingPlatformType.StartsWhenPlayerLands;
  public List<GameObject> synchronizedStartObjects = new List<GameObject>();
  public StartPathDirection startPathDirection = StartPathDirection.Forward;

  public StartPosition startPosition = StartPosition.Center;
  public float startPositionOffsetPercentage = 0f;

  public float startDelayOnEnabled = 0f;
  public float delayBetweenCycles = 0f;

  public bool lookForward;

  public int nodeCount;
  [HideInInspector]
  public List<Vector3> nodes = new List<Vector3>() { Vector3.zero, Vector3.zero };

  private bool _needsToUnSubscribeAttachedEvent = false;
  private bool _isMoving = false;
  private float _moveStartTime;
  private List<GameObjectTrackingInformation> _gameObjectTrackingInformation = new List<GameObjectTrackingInformation>();
  private IMoveable[] _synchronizedStartObjects;

  private float[] _segmentLengthPercentages = null;
  private GameManager _gameManager;

  private float _totalTime = 0f;

  private void SetStartPositions()
  {
    float step = 0f;
    float itemPositionPercentage = 0f;
    switch (startPosition)
    {
      case StartPosition.Center:
        step = 1f / ((float)totalObjectsOnPath + 1f);
        itemPositionPercentage = step;
        break;

      case StartPosition.PathStart:
        step = 1f / ((float)totalObjectsOnPath);
        itemPositionPercentage = 0f;
        break;
    }

    itemPositionPercentage = Mathf.Repeat(itemPositionPercentage + startPositionOffsetPercentage, 1f);

    for (int i = 0; i < _gameObjectTrackingInformation.Count; i++)
    {
      Vector3 segmentDirectionVector;
      _gameObjectTrackingInformation[i].gameObject.transform.position = GetLengthAdjustedPoint(itemPositionPercentage, out segmentDirectionVector);
      _gameObjectTrackingInformation[i].percentage = itemPositionPercentage;

      if (lookForward)
      {
        float rot_z = Mathf.Atan2(segmentDirectionVector.y, segmentDirectionVector.x) * Mathf.Rad2Deg;
        _gameObjectTrackingInformation[i].gameObject.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
      }

      Debug.Log(this.name + "; pct: " + itemPositionPercentage);
      itemPositionPercentage = Mathf.Repeat(itemPositionPercentage + step, 1f);
      //itemPositionPercentage += step;
    }
  }

  void attachableObject_Attached(IAttachableObject attachableObject, GameObject obj)
  {
    if (!_isMoving)
    {
      Logger.Info("Player landed on platform, start move...");
      _isMoving = true;
      _moveStartTime = Time.time + startDelayOnEnabled;

      for (int i = 0; i < totalObjectsOnPath; i++)
      {
        _gameObjectTrackingInformation[i].gameObject.GetComponent<IAttachableObject>().Attached -= attachableObject_Attached;
      }

      for (int i = 0; i < _synchronizedStartObjects.Length; i++)
      {
        _synchronizedStartObjects[i].StartMove();
      }

      _needsToUnSubscribeAttachedEvent = false;
    }
  }
  void player_OnGroundedPlatformChanged(object sender, PlayerController.GroundedPlatformChangedEventArgs e)
  {
    if (!_gameObjectTrackingInformation.Exists(c => c.gameObject == e.currentPlatform))
      return;

    if (!_isMoving && movingPlatformType == MovingPlatformType.StartsWhenPlayerLands)
    {
      Logger.Info("Player landed on platform, start move...");
      _isMoving = true;
      _moveStartTime = Time.time + startDelayOnEnabled;

      for (int i = 0; i < _synchronizedStartObjects.Length; i++)
      {
        _synchronizedStartObjects[i].StartMove();
      }
    }
  }

  public Vector3 GetLengthAdjustedPoint(float t, out Vector3 segmentDirectionVector)
  {
    int index = 0;
    if (t >= 1f)
    {
      t = 1f;
      index = _segmentLengthPercentages.Length - 1;
    }
    else
    {
      float remainingPercentage = t;
      for (int i = 0; i < _segmentLengthPercentages.Length; i++)
      {
        if (remainingPercentage > _segmentLengthPercentages[i])
        {
          remainingPercentage -= _segmentLengthPercentages[i];
        }
        else
        {
          t = remainingPercentage / _segmentLengthPercentages[i];
          index = i;
          break;
        }
      }
    }

    segmentDirectionVector = nodes[index + 1] - nodes[index];
    Vector3 direction = nodes[index] + (segmentDirectionVector.normalized * (segmentDirectionVector.magnitude * t));

    return transform.TransformPoint(direction);
  }

  void Update()
  {
    if (_isMoving && Time.time >= _moveStartTime)
    {
      float pct = Time.deltaTime / _totalTime;
      if (pct > 0f)
      {
        for (int i = 0; i < _gameObjectTrackingInformation.Count; i++)
        {
          if (Time.time < _gameObjectTrackingInformation[i].nextStartTime)
          {// this happens when there is a delay between cycles
            continue;
          }

          _gameObjectTrackingInformation[i].percentage += (pct * _gameObjectTrackingInformation[i].directionMultiplicationFactor);

          if (_gameObjectTrackingInformation[i].gameObject == null)
          {// for loop mode with delay between cycles
            _gameObjectTrackingInformation[i].gameObject = ObjectPoolingManager.Instance.GetObject(objectToAttach.name);
          }

          if (_gameObjectTrackingInformation[i].percentage > 1f)
          {// we reached the end, so react
            switch (loopMode)
            {
              case LoopMode.Loop:
                _gameObjectTrackingInformation[i].percentage = Mathf.Repeat(_gameObjectTrackingInformation[i].percentage, 1f);
                if (delayBetweenCycles > 0)
                {
                  ObjectPoolingManager.Instance.Deactivate(_gameObjectTrackingInformation[i].gameObject);
                  _gameObjectTrackingInformation[i].gameObject = null;
                  _gameObjectTrackingInformation[i].nextStartTime = Time.time + delayBetweenCycles;
                }
                else
                {
                  GameObject newObject = ObjectPoolingManager.Instance.GetObject(objectToAttach.name); // note: it is important to do this before deactivating the existing one to get a new object
                  ObjectPoolingManager.Instance.Deactivate(_gameObjectTrackingInformation[i].gameObject);

                  _gameObjectTrackingInformation[i].gameObject = newObject;
                }
                break;

              case LoopMode.Once:
                _gameObjectTrackingInformation[i].percentage = 1f;
                break;

              case LoopMode.PingPong:
                _gameObjectTrackingInformation[i].percentage = 1f - Mathf.Repeat(_gameObjectTrackingInformation[i].percentage, 1f);
                _gameObjectTrackingInformation[i].directionMultiplicationFactor *= -1f;
                _gameObjectTrackingInformation[i].nextStartTime = Time.time + delayBetweenCycles;
                break;
            }
          }
          else if (_gameObjectTrackingInformation[i].percentage < 0f)
          {
            switch (loopMode)
            {
              case LoopMode.Loop:
                _gameObjectTrackingInformation[i].percentage = 1f - Mathf.Repeat(_gameObjectTrackingInformation[i].percentage * -1f, 1f);
                if (delayBetweenCycles > 0)
                {
                  ObjectPoolingManager.Instance.Deactivate(_gameObjectTrackingInformation[i].gameObject);
                  _gameObjectTrackingInformation[i].gameObject = null;
                  _gameObjectTrackingInformation[i].nextStartTime = Time.time + delayBetweenCycles;
                }
                else
                {
                  GameObject newObject = ObjectPoolingManager.Instance.GetObject(objectToAttach.name); // note: it is important to do this before deactivating the existing one to get a new object
                  ObjectPoolingManager.Instance.Deactivate(_gameObjectTrackingInformation[i].gameObject);

                  _gameObjectTrackingInformation[i].gameObject = newObject;
                }
                break;

              case LoopMode.Once:
                _gameObjectTrackingInformation[i].percentage = 0f;
                break;

              case LoopMode.PingPong:
                _gameObjectTrackingInformation[i].percentage = Mathf.Repeat(_gameObjectTrackingInformation[i].percentage * -1f, 1f);
                _gameObjectTrackingInformation[i].directionMultiplicationFactor *= -1f;
                _gameObjectTrackingInformation[i].nextStartTime = Time.time + delayBetweenCycles;
                break;
            }
          }

          if (_gameObjectTrackingInformation[i].gameObject != null)
          {// can be null if loop mode with delay

            Vector3 segmentDirectionVector;
            _gameObjectTrackingInformation[i].gameObject.transform.position = GetLengthAdjustedPoint(
              _gameManager.easing.GetValue(easingType, _gameObjectTrackingInformation[i].percentage, 1f)
              , out segmentDirectionVector
              );

            if (lookForward)
            {
              float rot_z = Mathf.Atan2(segmentDirectionVector.y, segmentDirectionVector.x) * Mathf.Rad2Deg;
              _gameObjectTrackingInformation[i].gameObject.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
            }

          }
        }
      }
    }
  }

  void OnDisable()
  {
    if (_needsToUnSubscribeAttachedEvent)
    {
      for (int i = 0; i < totalObjectsOnPath; i++)
      {
        _gameObjectTrackingInformation[i].gameObject.GetComponent<IAttachableObject>().Attached -= attachableObject_Attached;
      }
      _needsToUnSubscribeAttachedEvent = false;
    }

    for (int i = _gameObjectTrackingInformation.Count - 1; i >= 0; i--)
    {
      ObjectPoolingManager.Instance.Deactivate(_gameObjectTrackingInformation[i].gameObject);
      _gameObjectTrackingInformation[i].gameObject = null;
    }

    _gameObjectTrackingInformation = new List<GameObjectTrackingInformation>();
    _isMoving = false;

    Logger.Info("Disabled moving linear path " + this.name);
  }

  void OnEnable()
  {
    Logger.Info("Enabled moving linear path " + this.name);

    if (_gameManager == null)
      _gameManager = GameManager.instance;

    if (_synchronizedStartObjects == null)
    {
      List<IMoveable> list = new List<IMoveable>();
      for (int i = 0; i < synchronizedStartObjects.Count; i++)
      {
        IMoveable moveable = synchronizedStartObjects[i].GetComponent<IMoveable>();
        if (moveable != null)
          list.Add(moveable);
      }
      _synchronizedStartObjects = list.ToArray();
    }

    if (_segmentLengthPercentages == null)
    {// first calculate lengths      
      float totalLength = 0f;
      float[] segmentLengths = new float[nodes.Count - 1];
      for (int i = 1; i < nodes.Count; i++)
      {
        float distance = Vector3.Distance(nodes[i - 1], nodes[i]);
        segmentLengths[i - 1] = distance;
        totalLength += distance;
      }
      float[] segmentLengthPercentages = new float[segmentLengths.Length];

      for (int i = 0; i < segmentLengths.Length; i++)
      {
        segmentLengthPercentages[i] = segmentLengths[i] / totalLength;
      }

      _segmentLengthPercentages = segmentLengthPercentages;

      #region get total time
      if (useTime)
      {
        _totalTime = time;
      }
      else if (useSpeed)
      {
        Logger.Assert(speedInUnitsPerSecond > 0f, "Speed must be greater than 0.");

        _totalTime = totalLength / speedInUnitsPerSecond;
        Debug.Log("Time: " + _totalTime + "( " + totalLength + " )");
        // TODO (Roman): divide length to get time
      }
      Logger.Assert(_totalTime > 0f, "Time must be set to a positive value greater than 0");
      #endregion
    }
    _gameObjectTrackingInformation = new List<GameObjectTrackingInformation>();


    for (int i = 0; i < totalObjectsOnPath; i++)
      _gameObjectTrackingInformation.Add(new GameObjectTrackingInformation(ObjectPoolingManager.Instance.GetObject(objectToAttach.name), 0f, startPathDirection == StartPathDirection.Forward ? 1f : -1f));

    if (movingPlatformType == MovingPlatformType.StartsWhenPlayerLands)
    {
      for (int i = 0; i < totalObjectsOnPath; i++)
      {
        IAttachableObject attachableObject = _gameObjectTrackingInformation[i].gameObject.GetComponent<IAttachableObject>();
        if (attachableObject != null)
        {
          attachableObject.Attached += attachableObject_Attached;
          _needsToUnSubscribeAttachedEvent = true;
        }
        else
        {
          GameManager.instance.player.OnGroundedPlatformChanged += player_OnGroundedPlatformChanged;
        }
      }
    }

    SetStartPositions();

    if (movingPlatformType == MovingPlatformType.MovesAlways)
    {
      _isMoving = true;
      _moveStartTime = Time.time + startDelayOnEnabled;

      Logger.Info("Start moving linear path " + this.name);
    }
  }

  #region IObjectPoolBehaviour Members

  public List<ObjectPoolRegistrationInfo> GetObjectPoolRegistrationInfos()
  {
    return new List<ObjectPoolRegistrationInfo>()
    {
      new ObjectPoolRegistrationInfo(objectToAttach, totalObjectsOnPath)
    };
  }

  #endregion
}