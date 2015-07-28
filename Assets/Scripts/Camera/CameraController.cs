﻿using System.Collections;
using System.Threading;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  #region nested
  class ZoomTimer : UpdateTimer
  {
    private float _startSize;
    private float _targetSize;
    private EasingType _easingType;
    private Easing _easing;

    protected override void DoUpdate(float currentTime)
    {
      float value = _easing.GetValue(_easingType, currentTime, _duration);
      Camera.main.orthographicSize = _startSize + (_targetSize - _startSize) * value;
    }

    public ZoomTimer(float duration, float startSize, float targetSize, EasingType easingType)
      : base(duration)
    {
      _startSize = startSize;
      _targetSize = targetSize;
      _easingType = easingType;
      _easing = new Easing();
    }
  }
  #endregion

  #region inspector fields
  public Vector3 cameraOffset;
  public bool useFixedUpdate = false;
  public Vector2 targetScreenSize = new Vector2(1920, 1080);
  #endregion

  #region members
  private CharacterPhysicsManager _characterPhysicsManager;

  private float _horizontalSmoothDampVelocity;
  private float _verticalSmoothDampVelocity;

  private CameraMovementSettings _cameraMovementSettings;
  private UpdateTimer _zoomTimer;

  [HideInInspector]
  public Transform target;
  [HideInInspector]
  public new Transform transform;

  private PlayerController _playerController;
  #endregion

  void Reset()
  {
    Logger.Info("Resetting camera movement settings.");
    CameraMovementSettings cameraMovementSettings = new CameraMovementSettings(
      new VerticalLockSettings()
      , new HorizontalLockSettings()
      , new ZoomSettings()
      , new SmoothDampMoveSettings()
      , Vector2.zero
      , VerticalCameraFollowMode.FollowAlways);

    SetCameraMovementSettings(cameraMovementSettings);
  }

  void Start()
  {
    transform = gameObject.transform;
    _playerController = GameManager.instance.player;
    // we set the target of the camera to our player through code
    target = _playerController.transform;

    _characterPhysicsManager = target.GetComponent<CharacterPhysicsManager>();

    Logger.Info("Window size: " + Screen.width + " x " + Screen.height);

    Reset();
  }

  void Update()
  {
    if (_zoomTimer != null)
    {
      _zoomTimer.Update();

      if (_zoomTimer.HasEnded)
        _zoomTimer = null;
    }
  }

  void LateUpdate()
  {
    if (!useFixedUpdate)
      updateCameraPosition();
  }

  void FixedUpdate()
  {
    if (useFixedUpdate)
      updateCameraPosition();
  }

  public void SetCameraMovementSettings(CameraMovementSettings cameraMovementSettings)
  {
    _cameraMovementSettings = cameraMovementSettings;

    this.cameraOffset = new Vector3(_cameraMovementSettings.offset.x, _cameraMovementSettings.offset.y, this.cameraOffset.z);

    float targetOrthographicSize = (targetScreenSize.y * .5f) / _cameraMovementSettings.zoomSettings.zoomPercentage;
    if (!Mathf.Approximately(Camera.main.orthographicSize, targetOrthographicSize))
    {
      Logger.Info("Start zoom to target size: " + targetOrthographicSize + ", current size: " + Camera.main.orthographicSize);
      if (_cameraMovementSettings.zoomSettings.zoomTime == 0f)
      {
        Camera.main.orthographicSize = targetOrthographicSize;
      }
      else
      {
        _zoomTimer = new ZoomTimer(_cameraMovementSettings.zoomSettings.zoomTime, Camera.main.orthographicSize, targetOrthographicSize, _cameraMovementSettings.zoomSettings.zoomEasingType);
        _zoomTimer.Start();
      }
    }

    Logger.Info("Camera movement set to: " + _cameraMovementSettings.ToString());
    Logger.Info("Camera size; current: " + Camera.main.orthographicSize + ", target: " + targetOrthographicSize);
  }

  private bool _isAboveJumpHeightLocked = false;

  void updateCameraPosition()
  {
    float yPos = 0f, xPos;

    bool isFallingDown = false;
    bool doSmoothDamp = false;

    switch (_cameraMovementSettings.verticalCameraFollowMode)
    {
      case VerticalCameraFollowMode.FollowWhenGrounded:
        if (_isAboveJumpHeightLocked && _characterPhysicsManager.velocity.y < 0f)
        {
          // We set this value to true in order to make the camera follow the character upwards when catapulted above the maximum jump height. The
          // character can not exceed the maximum jump heihgt without help (trampoline, powerup...).
          _isAboveJumpHeightLocked = false; // if we reached the peak we unlock
        }

        if (_isAboveJumpHeightLocked && (_cameraMovementSettings.verticalLockSettings.enableTopVerticalLock && target.position.y > _cameraMovementSettings.verticalLockSettings.topBoundary))
        {
          // we were locked but character has exceeded the top boundary. In that case we set the y pos and smooth damp
          yPos = _cameraMovementSettings.verticalLockSettings.topBoundary + cameraOffset.y;
          doSmoothDamp = true;
        }
        else
        {
          // we want to adjust the y position on upward movement if:
          if (_isAboveJumpHeightLocked                                                                                                // either we are locked in above jump height lock
              || (
                    (!_cameraMovementSettings.verticalLockSettings.enableTopVerticalLock                                              // OR (we either have no top boundary or we are beneath the top boundary in which case we can go up)
                      || target.position.y <= _cameraMovementSettings.verticalLockSettings.topBoundary)                                   //    AND
                    && (target.position.y > transform.position.y + this.cameraOffset.y + _playerController.jumpSettings.runJumpHeight  //    (the character has exceeded the jump height which means he has been artifically catapulted upwards)
                        && _characterPhysicsManager.velocity.y > 0f                                                                  //    AND we go up
                    )
                 )
            )
          {
            yPos = target.position.y - _playerController.jumpSettings.runJumpHeight;
            _isAboveJumpHeightLocked = true; // make sure for second if condition
          }
          else
          {
            isFallingDown = (_characterPhysicsManager.velocity.y < 0f
               && (target.position.y < this.transform.position.y + this.cameraOffset.y)
              );

            if (_characterPhysicsManager.lastMoveCalculationResult.collisionState.below
                || isFallingDown)
            {
              if (_cameraMovementSettings.verticalLockSettings.enabled)
              {
                if (_cameraMovementSettings.verticalLockSettings.enableDefaultVerticalLockPosition)
                  yPos = _cameraMovementSettings.verticalLockSettings.translatedVerticalLockPosition;
                else
                  yPos = target.position.y;

                if (_cameraMovementSettings.verticalLockSettings.enableTopVerticalLock
                  && target.position.y > _cameraMovementSettings.verticalLockSettings.topBoundary)
                {
                  yPos = _cameraMovementSettings.verticalLockSettings.topBoundary + cameraOffset.y;

                  // we might have been shot up, so use smooth damp override
                  doSmoothDamp = true;
                }
                else if (_cameraMovementSettings.verticalLockSettings.enableTopVerticalLock
                  && target.position.y < _cameraMovementSettings.verticalLockSettings.bottomBoundary)
                {
                  yPos = _cameraMovementSettings.verticalLockSettings.bottomBoundary + cameraOffset.y;

                  // we might have been falling down, so use smooth damp override
                  doSmoothDamp = true;
                }
              }
              else
              {
                yPos = target.position.y;
              }
            }
            else
            {
              // character is in air, so the camera stays same
              yPos = transform.position.y + this.cameraOffset.y; // we need to add offset bceause we will deduct it later on again
            }
          }
        }
        break;

      case VerticalCameraFollowMode.FollowAlways:
      default:
        _isAboveJumpHeightLocked = false; // this is not used at this mode

        #region vertical locking
        if (_cameraMovementSettings.verticalLockSettings.enabled)
        {
          if (_cameraMovementSettings.verticalLockSettings.enableDefaultVerticalLockPosition)
            yPos = _cameraMovementSettings.verticalLockSettings.translatedVerticalLockPosition;
          else
            yPos = target.position.y;

          if (_cameraMovementSettings.verticalLockSettings.enableTopVerticalLock
            && target.position.y > _cameraMovementSettings.verticalLockSettings.topBoundary)
          {
            yPos = _cameraMovementSettings.verticalLockSettings.topBoundary + cameraOffset.y;

            // we might have been shot up, so use smooth damp override
            doSmoothDamp = true;
          }
          else if (_cameraMovementSettings.verticalLockSettings.enableTopVerticalLock
            && target.position.y < _cameraMovementSettings.verticalLockSettings.bottomBoundary)
          {
            yPos = _cameraMovementSettings.verticalLockSettings.bottomBoundary + cameraOffset.y;

            // we might have been falling down, so use smooth damp override
            doSmoothDamp = true;
          }
        }
        else
        {
          yPos = target.position.y;
        }
        #endregion
        break;
    }

    #region horizontal locking
    xPos = target.position.x;
    if (_cameraMovementSettings.horizontalLockSettings.enabled)
    {
      if (_cameraMovementSettings.horizontalLockSettings.enableRightHorizontalLock
        && target.position.x > _cameraMovementSettings.horizontalLockSettings.rightBoundary)
      {
        xPos = _cameraMovementSettings.horizontalLockSettings.rightBoundary + cameraOffset.x;
      }
      else if (_cameraMovementSettings.horizontalLockSettings.enableLeftHorizontalLock
        && target.position.x < _cameraMovementSettings.horizontalLockSettings.leftBoundary)
      {
        xPos = _cameraMovementSettings.horizontalLockSettings.leftBoundary + cameraOffset.x;
      }
      else
      {
        xPos = target.position.x;
      }
    }
    else
    {
      xPos = target.position.x;
    }
    #endregion

    Vector3 hvec = new Vector3(xPos, yPos, target.position.z);

    float verticalSmoothDampTime =
      doSmoothDamp // override
      ? _cameraMovementSettings.smoothDampMoveSettings.verticalSmoothDampTime
      : isFallingDown
        ? _cameraMovementSettings.smoothDampMoveSettings.verticalRapidDescentSmoothDampTime
        : _isAboveJumpHeightLocked
          ? _cameraMovementSettings.smoothDampMoveSettings.verticalAboveRapidAcsentSmoothDampTime
          : _cameraMovementSettings.smoothDampMoveSettings.verticalSmoothDampTime;

    if (_characterPhysicsManager.velocity.x > 0)
    {
      Vector3 targetPositon = hvec - cameraOffset;
      transform.position = new Vector3(
        Mathf.SmoothDamp(transform.position.x, targetPositon.x, ref _horizontalSmoothDampVelocity, _cameraMovementSettings.smoothDampMoveSettings.horizontalSmoothDampTime)
        , Mathf.SmoothDamp(transform.position.y, targetPositon.y, ref _verticalSmoothDampVelocity, verticalSmoothDampTime)
        , targetPositon.z);
    }
    else
    {
      var leftOffset = cameraOffset;
      leftOffset.x *= -1;

      Vector3 targetPositon = hvec - leftOffset;
      transform.position = new Vector3(
        Mathf.SmoothDamp(transform.position.x, targetPositon.x, ref _horizontalSmoothDampVelocity, _cameraMovementSettings.smoothDampMoveSettings.horizontalSmoothDampTime)
        , Mathf.SmoothDamp(transform.position.y, targetPositon.y, ref _verticalSmoothDampVelocity, verticalSmoothDampTime)
        , targetPositon.z);
    }
  }

}

