using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


public class LinearPath : MonoBehaviour
{
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

    public GameObjectTrackingInformation(GameObject gameObject, float percentage, float directionMultiplicationFactor)
    {
      this.gameObject = gameObject;
      this.percentage = percentage;
      this.directionMultiplicationFactor = directionMultiplicationFactor;
    }
  }

  public List<Vector3> nodes = new List<Vector3>() { Vector3.zero, Vector3.zero };
  public int nodeCount;

  public Color outlineGizmoColor = Color.white;
  public bool showGizmoOutline = true;

  public EasingType easingType = EasingType.Linear;

  public GameObject objectToAttach;
  public int totalObjectsOnPath = 1;
  public LoopMode loopMode = LoopMode.Once;

  public float time = 5f;
  public MovingPlatformType movingPlatformType = MovingPlatformType.StartsWhenPlayerLands;
  public StartPosition startPosition = StartPosition.Center;
  public StartPathDirection startPathDirection = StartPathDirection.Forward;

  private bool _isMoving = false;
  private List<GameObjectTrackingInformation> _gameObjectTrackingInformation = new List<GameObjectTrackingInformation>();

  private float[] _segmentLengthPercentages;

  void OnDrawGizmos()
  {
    if (showGizmoOutline)
    {
      Gizmos.color = outlineGizmoColor;
      for (int i = 1; i < nodes.Count; i++)
      {
        Gizmos.DrawLine(this.gameObject.transform.TransformPoint(nodes[i - 1]), this.gameObject.transform.TransformPoint(nodes[i]));
      }
    }
  }

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

    for (int i = 0; i < _gameObjectTrackingInformation.Count; i++)
    {
      _gameObjectTrackingInformation[i].gameObject.transform.position = GetLengthAdjustedPoint(itemPositionPercentage);
      _gameObjectTrackingInformation[i].percentage = itemPositionPercentage;

      itemPositionPercentage += step;
    }
  }
  
  void attachableObject_Attached(IAttachableObject attachableObject, GameObject obj)
  {
    if (!_isMoving)
    {
      Logger.Info("Player landed on platform, start move...");
      _isMoving = true;

      for (int i = 0; i < totalObjectsOnPath; i++)
      {
        _gameObjectTrackingInformation[i].gameObject.GetComponent<IAttachableObject>().Attached -= attachableObject_Attached;
      }
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
    }
  }

  public Vector3 GetLengthAdjustedPoint(float t)
  {
    int index = 0;
    if (t >= 1f)
    {
      t = 1f;
      index = _segmentLengthPercentages.Length - 2;
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

    Vector3 vector = nodes[index + 1] - nodes[index];
    Vector3 direction = nodes[index] + (vector.normalized * (vector.magnitude * t));

    return transform.TransformPoint(direction);
  }

  void Update()
  {
    if (_isMoving)
    {
      float pct = Time.deltaTime / time;
      if (pct > 0f)
      {
        for (int i = 0; i < _gameObjectTrackingInformation.Count; i++)
        {
          _gameObjectTrackingInformation[i].percentage += (pct * _gameObjectTrackingInformation[i].directionMultiplicationFactor);

          if (_gameObjectTrackingInformation[i].percentage > 1f)
          {// we reached the end, so react
            switch (loopMode)
            {
              case LoopMode.Loop:

                GameObject newObject = ObjectPoolingManager.Instance.GetObject(objectToAttach.name);
                ObjectPoolingManager.Instance.Deactivate(_gameObjectTrackingInformation[i].gameObject);
                _gameObjectTrackingInformation[i].gameObject = newObject;
                _gameObjectTrackingInformation[i].percentage = Mathf.Repeat(_gameObjectTrackingInformation[i].percentage, 1f);

                break;

              case LoopMode.Once:
                _gameObjectTrackingInformation[i].percentage = 1f;
                break;

              case LoopMode.PingPong:
                _gameObjectTrackingInformation[i].percentage = 1f - Mathf.Repeat(_gameObjectTrackingInformation[i].percentage, 1f);
                _gameObjectTrackingInformation[i].directionMultiplicationFactor *= -1f;
                break;
            }
          }
          else if (_gameObjectTrackingInformation[i].percentage < 0f)
          {
            switch (loopMode)
            {
              case LoopMode.Loop:

                GameObject newObject = ObjectPoolingManager.Instance.GetObject(objectToAttach.name);
                ObjectPoolingManager.Instance.Deactivate(_gameObjectTrackingInformation[i].gameObject);
                _gameObjectTrackingInformation[i].gameObject = newObject;
                _gameObjectTrackingInformation[i].percentage = 1f - Mathf.Repeat(_gameObjectTrackingInformation[i].percentage * -1f, 1f);

                break;

              case LoopMode.Once:
                _gameObjectTrackingInformation[i].percentage = 0f;
                break;

              case LoopMode.PingPong:
                _gameObjectTrackingInformation[i].percentage = Mathf.Repeat(_gameObjectTrackingInformation[i].percentage * -1f, 1f);
                _gameObjectTrackingInformation[i].directionMultiplicationFactor *= -1f;
                break;
            }
          }

          _gameObjectTrackingInformation[i].gameObject.transform.position = GetLengthAdjustedPoint(_gameObjectTrackingInformation[i].percentage);
        }
      }
    }
  }

  // Use this for initialization
  void Start()
  {
    Logger.Assert(time > 0f, "Time must be set to a positive value greater than 0");

    // first calculate lengths
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
    _gameObjectTrackingInformation = new List<GameObjectTrackingInformation>();

    // we wanna do this in start as we know that the player has been added to the game context
    ObjectPoolingManager.Instance.RegisterPool(objectToAttach, totalObjectsOnPath, int.MaxValue);

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
        }
        else
        {
          GameManager.instance.player.OnGroundedPlatformChanged += player_OnGroundedPlatformChanged;
        }
      }
    }
    
    SetStartPositions();

    if (movingPlatformType == MovingPlatformType.MovesAlways)
      _isMoving = true;
  }
}