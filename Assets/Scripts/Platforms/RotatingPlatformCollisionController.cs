using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public partial class RotatingPlatformCollisionController : MonoBehaviour, IObjectPoolBehaviour
{
  private const string TRACE_TAG = "RotatingPlatformCollisionController";

  public float rotationSpeed = 2000f;
  public GameObject rotatingObject;
  public float pushPlayerOffSlopeFactor = 286f;
  public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1f), new Keyframe(0, 1), new Keyframe(90, 0));
  public float slopeLimit = 60f;

  private GameObject _gameObject;

  private PlayerController _playerController;
  private AttachPlayerControllerToObject _attachPlayerControllerToObject;

  private const float FUDGE_FACTOR = .0001f;

  private float _angle;
  private bool _isSlidingDown = false;
  private SlideDownSlopePlayerControlHandler _slideDownSlopePlayerControlHandler = null;

  void Awake()
  {
    _playerController = GameManager.instance.player;
  }

  void Start()
  {
    _gameObject = ObjectPoolingManager.Instance.GetObject(rotatingObject.name);
    _gameObject.transform.position = this.transform.position;
    _gameObject.SetActive(true);

    _playerController = GameManager.instance.player;
  }

  void Update()
  {
    float angleToRotate = rotationSpeed * Time.deltaTime;
    float lastAngle = _gameObject.transform.rotation.eulerAngles.z;
    _gameObject.transform.Rotate(new Vector3(0f, 0f, angleToRotate));

    // this must be called before player controller updates
    bool isGrounded = false;
    if (!_isSlidingDown)
    {
      for (int i = 0; i < _playerController.characterPhysicsManager.lastRaycastHits.Count; i++)
      {
        if (_playerController.characterPhysicsManager.lastRaycastHits[i].collider.gameObject == this._gameObject)
        {
          float rotateToAngle = (_gameObject.transform.rotation.eulerAngles.z - lastAngle) * Mathf.Deg2Rad;

          float slopeAngle = Vector2.Angle(_playerController.characterPhysicsManager.lastRaycastHits[i].normal, Vector2.up);
          if (slopeAngle < slopeLimit//_playerController.characterPhysicsManager.slopeLimit
            || (rotationSpeed < 0f && _playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.right)
            || (rotationSpeed > 0f && _playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.left)
            )
          {
            Vector3 rotated = new Vector3(
             Mathf.Cos(rotateToAngle) * (_playerController.characterPhysicsManager.lastRaycastHits[i].point.x - _gameObject.transform.position.x) - Mathf.Sin(rotateToAngle) * (_playerController.characterPhysicsManager.lastRaycastHits[i].point.y - _gameObject.transform.position.y) + _gameObject.transform.position.x
             , Mathf.Sin(rotateToAngle) * (_playerController.characterPhysicsManager.lastRaycastHits[i].point.x - _gameObject.transform.position.x) + Mathf.Cos(rotateToAngle) * (_playerController.characterPhysicsManager.lastRaycastHits[i].point.y - _gameObject.transform.position.y) + _gameObject.transform.position.y
             , _gameObject.transform.position.z);

            Vector3 delta = rotated - _playerController.characterPhysicsManager.lastRaycastHits[i].point.ToVector3();
            _playerController.transform.Translate(delta, Space.World);
          }
          else
          {// push off
            if (_gameObject.transform.position.x > _playerController.transform.position.x)
            {
              _slideDownSlopePlayerControlHandler = new SlideDownSlopePlayerControlHandler(_playerController, 1f, Direction.Right);
              _playerController.PushControlHandler(_slideDownSlopePlayerControlHandler);
            }
            else
            {
              _slideDownSlopePlayerControlHandler = new SlideDownSlopePlayerControlHandler(_playerController, 1f, Direction.Left);
              _playerController.PushControlHandler(_slideDownSlopePlayerControlHandler);
            }
            _isSlidingDown = true;
          }

          isGrounded = true;
          break;
        }
      }
    }
    else
    {
      for (int i = 0; i < _playerController.characterPhysicsManager.lastRaycastHits.Count; i++)
      {
        if (_playerController.characterPhysicsManager.lastRaycastHits[i].collider.gameObject == this._gameObject)
        {
          float rotateToAngle = (_gameObject.transform.rotation.eulerAngles.z - lastAngle) * Mathf.Deg2Rad;
          
          Vector3 rotated = new Vector3(
           Mathf.Cos(rotateToAngle) * (_playerController.characterPhysicsManager.lastRaycastHits[i].point.x - _gameObject.transform.position.x) - Mathf.Sin(rotateToAngle) * (_playerController.characterPhysicsManager.lastRaycastHits[i].point.y - _gameObject.transform.position.y) + _gameObject.transform.position.x
           , Mathf.Sin(rotateToAngle) * (_playerController.characterPhysicsManager.lastRaycastHits[i].point.x - _gameObject.transform.position.x) + Mathf.Cos(rotateToAngle) * (_playerController.characterPhysicsManager.lastRaycastHits[i].point.y - _gameObject.transform.position.y) + _gameObject.transform.position.y
           , _gameObject.transform.position.z);

          Vector3 delta = rotated - _playerController.characterPhysicsManager.lastRaycastHits[i].point.ToVector3();
          _playerController.transform.Translate(delta, Space.World);

          if (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.right)
          {
            _playerController.transform.Translate(new Vector3(-.001f, 0f, 0f));
          }
          else if (_playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.left)
          {
            _playerController.transform.Translate(new Vector3(.001f, 0f, 0f));
          }
        }
      }
    }

    if (isGrounded)
    {
      _playerController.characterPhysicsManager.slopeSpeedMultiplierOverride = slopeSpeedMultiplier;
    }
    else
    {
      if (_isSlidingDown && _playerController.CurrentControlHandler != _slideDownSlopePlayerControlHandler)
      {
        _slideDownSlopePlayerControlHandler = null;
        _isSlidingDown = false;
      }

      if (_playerController.characterPhysicsManager.slopeSpeedMultiplierOverride != null)
      {
        _playerController.characterPhysicsManager.slopeSpeedMultiplierOverride = null;
      }
    }
  }

  #region IObjectPoolBehaviour Members

  public List<ObjectPoolRegistrationInfo> GetObjectPoolRegistrationInfos()
  {
    return new List<ObjectPoolRegistrationInfo>()
    {
      new ObjectPoolRegistrationInfo(rotatingObject, 1)
    };
  }

  #endregion
}
