using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public partial class Pendulum : SpawnBucketItemBehaviour, IObjectPoolBehaviour
{
  public GameObject floatingAttachedPlatform;
  public float radius = 256f;
  public float moveDuration = 2f;
  public float stopDuration = 2f;
  public EasingType easingType = EasingType.Linear;

  public float startAngle;
  public float endAngle;

  private GameObject _platform;
  private float _startAngleRad;
  private float _endAngleRad;
  private float _totalAngleRad;

  private float _currentAngle;
  private float _currentMoveDuration;
  private float _directionFactor;
  private bool _isMovingTowardsEndPoint;
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

    if (_isMovingTowardsEndPoint)
      _currentAngle = _startAngleRad + _easing.GetValue(easingType, _currentMoveDuration, moveDuration) * _totalAngleRad * _directionFactor;    
    else
      _currentAngle = _endAngleRad - _easing.GetValue(easingType, _currentMoveDuration, moveDuration) * _totalAngleRad * _directionFactor;
    
    if (_currentMoveDuration >= moveDuration)
    {
      _nextStartTime = Time.time + stopDuration;
      _currentMoveDuration = 0f;

      if (_isMovingTowardsEndPoint)
      {
        _currentAngle = _endAngleRad;
      }
      else
      {
        _currentAngle = _startAngleRad;
      }
      _isMovingTowardsEndPoint = !_isMovingTowardsEndPoint;
    }

    Vector3 initial = new Vector3(transform.position.x + radius, transform.position.y, transform.position.z);
    Vector3 rotated = new Vector3(
      Mathf.Cos(_currentAngle) * (initial.x - transform.position.x) - Mathf.Sin(_currentAngle) * (initial.y - transform.position.y) + transform.position.x
      , Mathf.Sin(_currentAngle) * (initial.x - transform.position.x) + Mathf.Cos(_currentAngle) * (initial.y - transform.position.y) + transform.position.y
      , transform.position.z);

    _platform.transform.position = rotated;
  }

  void OnEnable()
  {
    _objectPoolingManager = ObjectPoolingManager.Instance;
    
    _startAngleRad = startAngle * Mathf.Deg2Rad;
    _endAngleRad = endAngle * Mathf.Deg2Rad;
    _totalAngleRad = Mathf.Abs(_endAngleRad - _startAngleRad);

    _isMovingTowardsEndPoint = true;
    _directionFactor = _startAngleRad > _endAngleRad ? -1f : 1f;

    GameObject platform = _objectPoolingManager.GetObject(floatingAttachedPlatform.name);

    _currentAngle = _startAngleRad;

    Vector3 initial = new Vector3(transform.position.x + radius, transform.position.y, transform.position.z);
    Vector3 rotated = new Vector3(
      Mathf.Cos(_currentAngle) * (initial.x - transform.position.x) - Mathf.Sin(_currentAngle) * (initial.y - transform.position.y) + transform.position.x
      , Mathf.Sin(_currentAngle) * (initial.x - transform.position.x) + Mathf.Cos(_currentAngle) * (initial.y - transform.position.y) + transform.position.y
      , transform.position.z);

    platform.transform.position = rotated;

    _platform = platform;
    _nextStartTime = Time.time + stopDuration;
  }

  void OnDisable()
  {
    _objectPoolingManager.Deactivate(_platform);
  }

  #region IObjectPoolBehaviour Members

  public List<ObjectPoolRegistrationInfo> GetObjectPoolRegistrationInfos()
  {
    return new List<ObjectPoolRegistrationInfo>()
    {
      new ObjectPoolRegistrationInfo(floatingAttachedPlatform, 1)
    };
  }

  #endregion
}

