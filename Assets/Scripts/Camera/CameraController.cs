using UnityEngine;

public class CameraController : MonoBehaviour
{
  public Transform target;
  public float smoothDampTime = 0.2f;

  [HideInInspector]
  public new Transform transform;

  public Vector3 cameraOffset;
  public bool useFixedUpdate = false;
  public float maxPixelHeight = 1080f;
  
  private CharacterPhysicsManager _characterPhysicsManager;
  private Vector3 _smoothDampVelocity;

  private CameraMovementSettings _cameraMovementSettings;

#if UNITY_EDITOR
  public float? YPosLock
  {
    get { return _cameraMovementSettings.YPosLock; }
  }
#endif

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

    // TODO (Roman): this should come from game manager
    // TODO (Roman): don't hardcode tags
    GameObject checkpoint = GameObject.FindGameObjectWithTag("Checkpoint 1");
    
    PlayerController playerController = Instantiate(GameManager.instance.player, checkpoint.transform.position, Quaternion.identity) as PlayerController;

    playerController.spawnLocation = checkpoint.transform.position;

    // we set the target of the camera to our player through code
    target = playerController.transform;
    
    _characterPhysicsManager = target.GetComponent<CharacterPhysicsManager>();

    Logger.Info("Window size: " + Screen.width + " x " + Screen.height);
    Logger.Info("Camera size: " + Camera.main.orthographicSize);

    Reset();
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
    Logger.Info("Camera movement set to: " + cameraMovementSettings.ToString());
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
      if (_cameraMovementSettings.AllowLeftExtension && target.position.x < _cameraMovementSettings.LeftBoundary)
        xPos = target.position.x + _cameraMovementSettings.OffsetX;
      else if (_cameraMovementSettings.AllowRightExtension && target.position.x > _cameraMovementSettings.RightBoundary)
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
      transform.position = Vector3.SmoothDamp(transform.position, hvec - cameraOffset, ref _smoothDampVelocity, smoothDampTime);
    }
    else
    {
      var leftOffset = cameraOffset;
      leftOffset.x *= -1;
      transform.position = Vector3.SmoothDamp(transform.position, hvec - leftOffset, ref _smoothDampVelocity, smoothDampTime);
    }
  }

}

