using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public partial class HalfWheel : SpawnBucketItemBehaviour
{
  public GameObject floatingAttachedPlatform;
  public float radius = 256f;
  public float moveDuration = 2f;
  public float stopDuration = 2f;
  public Direction startDirection = Direction.Right;
  public EasingType easingType = EasingType.Linear;

  private GameObject _platform;
  private float _currentAngle;
  private float _currentMoveDuration;
  private Direction _currentDirection = Direction.Right;
  private float _nextStartTime;
  private Easing _easing = new Easing();

  private bool _isPlayerAttached;
  private BoxCollider2D _visibilityCollider;
  private ObjectPoolingManager _objectPoolingManager;

  void Update()
  {
    if (Time.time < _nextStartTime)
      return;
    
    _currentMoveDuration += Time.deltaTime;

    if (_currentDirection == Direction.Left)
      _currentAngle = _easing.GetValue(easingType, _currentMoveDuration, moveDuration) * -Mathf.PI;
    else
      _currentAngle = -Mathf.PI + _easing.GetValue(easingType, _currentMoveDuration, moveDuration) * Mathf.PI;
    
    if (_currentMoveDuration >= moveDuration)
    {
      _nextStartTime = Time.time + stopDuration;
      _currentMoveDuration = 0f;

      if (_currentDirection == Direction.Right)
      {
        _currentAngle = 0;
        _currentDirection = Direction.Left;
      }
      else
      {
        _currentAngle = -Mathf.PI;
        _currentDirection = Direction.Right;
      }
    }

#if USE_CIRCLE
    Quaternion q = Quaternion.AngleAxis(_currentAngle, Vector3.forward);

    Vector3 rotated = new Vector3(width * Mathf.Cos(_currentAngle), height * Mathf.Sin(_currentAngle), 0.0f);
    rotated = q * rotated + this.transform.position;
#else
    Vector3 initial = new Vector3(transform.position.x + radius, transform.position.y, transform.position.z);
    Vector3 rotated = new Vector3(
      Mathf.Cos(_currentAngle) * (initial.x - transform.position.x) - Mathf.Sin(_currentAngle) * (initial.y - transform.position.y) + transform.position.x
      , Mathf.Sin(_currentAngle) * (initial.x - transform.position.x) + Mathf.Cos(_currentAngle) * (initial.y - transform.position.y) + transform.position.y
      , transform.position.z);
#endif

    _platform.transform.position = rotated;
    if (_currentMoveDuration == 0f)
      Debug.Log("Angle: " + _currentAngle * Mathf.Rad2Deg + " (rad: " + _currentAngle + ")" + ", platform pos: " + _platform.transform.position);
  }

  void OnEnable()
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;
    // TODO (Roman): this should be done at global scene load
    _objectPoolingManager.RegisterPool(floatingAttachedPlatform, 1, int.MaxValue);

    Logger.Info("Enabling half wheel " + this.name);

    GameObject platform = _objectPoolingManager.GetObject(floatingAttachedPlatform.name);

    _currentAngle = startDirection == Direction.Right ? -Mathf.PI : 0f;

#if USE_CIRCLE
    Quaternion q = Quaternion.AngleAxis(_currentAngle, Vector3.forward);

    Vector3 rotated = new Vector3(width * Mathf.Cos(_currentAngle), height * Mathf.Sin(_currentAngle), 0.0f);
    rotated = q * rotated + this.transform.position;
#else
    Vector3 initial = new Vector3(transform.position.x + radius, transform.position.y, transform.position.z);
    Vector3 rotated = new Vector3(
      Mathf.Cos(_currentAngle) * (initial.x - transform.position.x) - Mathf.Sin(_currentAngle) * (initial.y - transform.position.y) + transform.position.x
      , Mathf.Sin(_currentAngle) * (initial.x - transform.position.x) + Mathf.Cos(_currentAngle) * (initial.y - transform.position.y) + transform.position.y
      , transform.position.z);
#endif

    platform.transform.position = rotated;

    _platform = platform;
    _currentDirection = startDirection;
    _nextStartTime = Time.time + stopDuration;
  }

  void OnDisable()
  {
    Logger.Info("Disabling half wheel " + this.name);
    _objectPoolingManager.Deactivate(_platform);
  }
}

