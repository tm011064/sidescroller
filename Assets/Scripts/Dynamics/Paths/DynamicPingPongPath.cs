using System;
using System.Collections.Generic;
using UnityEngine;

public partial class DynamicPingPongPath : SpawnBucketItemBehaviour
{
  #region nested classes  
  [Serializable]
  public class DynamicPingPongPathMotionSettings
  {
    public EasingType easingType = EasingType.Linear;
    public float time;
  }
  #endregion

  public Color outlineGizmoColor = Color.white;
  public bool showGizmoOutline = true;

  public DynamicPingPongPathMotionSettings forwardMotionSettings = new DynamicPingPongPathMotionSettings();
  public DynamicPingPongPathMotionSettings backwardMotionSettings = new DynamicPingPongPathMotionSettings();

  public GameObject objectToAttach;

  public int nodeCount = 2;
  [HideInInspector]
  public List<Vector3> nodes = new List<Vector3>() { Vector3.zero, Vector3.zero };

  private bool _isMovingForward = false;

  protected GameObject _gameObject;
  protected float _percentage = 0f;

  private float[] _segmentLengthPercentages = null;
  protected GameManager _gameManager;

  protected virtual void OnForwardMovementCompleted()
  {

  }
  protected virtual void OnBackwardMovementCompleted()
  {

  }

  public Vector3 GetLengthAdjustedPoint(float t)
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

    Vector3 vector = nodes[index + 1] - nodes[index];
    Vector3 direction = nodes[index] + (vector.normalized * (vector.magnitude * t));

    return transform.TransformPoint(direction);
  }

  void Update()
  {
    if (_isMovingForward)
    {
      if (_percentage < 1f)
      {
        _percentage = Mathf.Min(1f, _percentage + Time.deltaTime / forwardMotionSettings.time);
        _gameObject.transform.position = GetLengthAdjustedPoint(
          _gameManager.easing.GetValue(forwardMotionSettings.easingType, _percentage, 1f)
          );

        if (_percentage == 1f)
          OnForwardMovementCompleted();
      }
    }
    else if (_percentage > 0f)
    {
      _percentage = Mathf.Max(0f, _percentage - Time.deltaTime / backwardMotionSettings.time);
      _gameObject.transform.position = GetLengthAdjustedPoint(
        _gameManager.easing.GetValue(backwardMotionSettings.easingType, _percentage, 1f)
        );

      if (_percentage == 0f)
        OnBackwardMovementCompleted();
    }
  }

  public void StartForwardMovement()
  {
    _isMovingForward = true;
  }
  public void StopForwardMovement()
  {
    _isMovingForward = false;
  }

  void OnDisable()
  {
    ObjectPoolingManager.Instance.Deactivate(_gameObject);
    _gameObject = null;
    _percentage = 0f;
    _isMovingForward = false;
  }

  protected virtual void OnEnable()
  {
    if (_gameManager == null)
      _gameManager = GameManager.instance;

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
    }

    // we wanna do this in start as we know that the player has been added to the game context
    ObjectPoolingManager.Instance.RegisterPool(objectToAttach, 1, int.MaxValue);

    _gameObject = ObjectPoolingManager.Instance.GetObject(objectToAttach.name, this.gameObject.transform.position);
    _percentage = 0f;

    _isMovingForward = false;
  }
}