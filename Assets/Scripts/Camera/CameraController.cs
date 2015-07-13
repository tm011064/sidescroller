using System.Collections;
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
  public float maxPixelHeight = 1080f;
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
  #endregion
  
  void Reset()
  {
    Logger.Info("Resetting camera movement settings.");
    CameraMovementSettings cameraMovementSettings = new CameraMovementSettings(
      null
      , maxPixelHeight * .5f
      );

    SetCameraMovementSettings(cameraMovementSettings);
  }

  void Start()
  {
    transform = gameObject.transform;
    
    // we set the target of the camera to our player through code
    target = GameManager.instance.player.transform;

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

    float targetOrthographicSize = (maxPixelHeight * .5f) / _cameraMovementSettings.ZoomPercentage;
    if (!Mathf.Approximately(Camera.main.orthographicSize, targetOrthographicSize))
    {
      _zoomTimer = new ZoomTimer(_cameraMovementSettings.ZoomTime, Camera.main.orthographicSize, targetOrthographicSize, _cameraMovementSettings.ZoomEasingType);
      _zoomTimer.Start();
    }

    Logger.Info("Camera movement set to: " + cameraMovementSettings.ToString());
    Logger.Info("Camera size; current: " + Camera.main.orthographicSize + ", target: " + targetOrthographicSize);
  }

  void updateCameraPosition()
  {
    float yPos, xPos;

    if (_cameraMovementSettings.YPosLock.HasValue)
    {
      if (_cameraMovementSettings.AllowTopExtension && target.position.y > _cameraMovementSettings.TopBoundary)
        yPos = target.position.y + _cameraMovementSettings.OffsetY;
      else if (_cameraMovementSettings.AllowBottomExtension && target.position.y < _cameraMovementSettings.BottomBoundary)
        yPos = target.position.y + _cameraMovementSettings.OffsetY;
      else
        yPos = _cameraMovementSettings.YPosLock.Value + _cameraMovementSettings.OffsetY;
    }
    else
    {
      yPos = target.position.y + _cameraMovementSettings.OffsetY;
    }

    if (_cameraMovementSettings.XPosLock.HasValue)
    {
      float posX = _cameraMovementSettings.XPosLock.Value + _cameraMovementSettings.OffsetX;

      if (_cameraMovementSettings.AllowLeftExtension && target.position.x < posX)
        xPos = target.position.x + _cameraMovementSettings.OffsetX;
      else if (_cameraMovementSettings.AllowRightExtension && target.position.x > posX)
        xPos = target.position.x + _cameraMovementSettings.OffsetX;
      else
        xPos = _cameraMovementSettings.XPosLock.Value + _cameraMovementSettings.OffsetX;
    }
    else
    {
      xPos = target.position.x + _cameraMovementSettings.OffsetX;
    }

    Vector3 hvec = new Vector3(xPos, yPos, target.position.z);

    if (_characterPhysicsManager.velocity.x > 0)
    {      
      Vector3 targetPositon = hvec - cameraOffset;
      transform.position = new Vector3(
        Mathf.SmoothDamp(transform.position.x, targetPositon.x, ref _horizontalSmoothDampVelocity, _cameraMovementSettings.HorizontalSmoothDampTime)
        , Mathf.SmoothDamp(transform.position.y, targetPositon.y, ref _verticalSmoothDampVelocity, _cameraMovementSettings.VerticalSmoothDampTime)
        , targetPositon.z);
    }
    else
    {
      var leftOffset = cameraOffset;
      leftOffset.x *= -1;

      Vector3 targetPositon = hvec - leftOffset;
      transform.position = new Vector3(
        Mathf.SmoothDamp(transform.position.x, targetPositon.x, ref _horizontalSmoothDampVelocity, _cameraMovementSettings.HorizontalSmoothDampTime)
        , Mathf.SmoothDamp(transform.position.y, targetPositon.y, ref _verticalSmoothDampVelocity, _cameraMovementSettings.VerticalSmoothDampTime)
        , targetPositon.z);
    }
  }

}

