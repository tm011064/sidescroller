using UnityEngine;
using System.Collections;

/// <summary>
/// 
/// </summary>
public class RotatingPlatformCollisionController : MonoBehaviour
{
  private const string TRACE_TAG = "RotatingPlatformCollisionController";

  public float rotationSpeed = 2000f;
  public GameObject rotatingObject;
  public float pushPlayerOffSlopeFactor = 286f;
  public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1f), new Keyframe(0, 1), new Keyframe(90, 0));

  private GameObject _gameObject;
  
  private PlayerController _playerController;
  private AttachPlayerControllerToObject _attachPlayerControllerToObject;

  private const float FUDGE_FACTOR = .0001f;
  
  private float _angle;

  void Awake()
  {
    _playerController = GameManager.instance.player;
  }

  void Start()
  {
    ObjectPoolingManager.Instance.RegisterPool(rotatingObject, 1, int.MaxValue);

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
    for (int i = 0; i < _playerController.characterPhysicsManager.lastRaycastHits.Count; i++)
    {
      if (_playerController.characterPhysicsManager.lastRaycastHits[i].collider.gameObject == this._gameObject)
      {
        float rotateToAngle = (_gameObject.transform.rotation.eulerAngles.z - lastAngle) * Mathf.Deg2Rad;

        float slopeAngle = Vector2.Angle(_playerController.characterPhysicsManager.lastRaycastHits[i].normal, Vector2.up);
        if (slopeAngle < _playerController.characterPhysicsManager.slopeLimit)
        {
          Vector3 rotated = new Vector3(
            Mathf.Cos(rotateToAngle) * (_playerController.transform.position.x - _gameObject.transform.position.x) - Mathf.Sin(rotateToAngle) * (_playerController.transform.position.y - _gameObject.transform.position.y) + _gameObject.transform.position.x
            , Mathf.Sin(rotateToAngle) * (_playerController.transform.position.x - _gameObject.transform.position.x) + Mathf.Cos(rotateToAngle) * (_playerController.transform.position.y - _gameObject.transform.position.y) + _gameObject.transform.position.y
            , _gameObject.transform.position.z);

          _playerController.transform.Translate(rotated - _playerController.transform.position, Space.World);
        }
        else
        {// push off
          if (rotateToAngle < 0)
          {
            _playerController.transform.Translate(new Vector3(rotateToAngle * -pushPlayerOffSlopeFactor, rotateToAngle * pushPlayerOffSlopeFactor, 0), Space.World);
          }
          else
          {
            _playerController.transform.Translate(new Vector3(rotateToAngle * -pushPlayerOffSlopeFactor, rotateToAngle * -pushPlayerOffSlopeFactor, 0), Space.World);
          }
        }

        isGrounded = true;
        break;
      }
    }

    if (isGrounded)
    {
      _playerController.characterPhysicsManager.slopeSpeedMultiplierOverride = slopeSpeedMultiplier;
    }
    else if (_playerController.characterPhysicsManager.slopeSpeedMultiplierOverride != null)
    {
      _playerController.characterPhysicsManager.slopeSpeedMultiplierOverride = null;
    }
  }
}
