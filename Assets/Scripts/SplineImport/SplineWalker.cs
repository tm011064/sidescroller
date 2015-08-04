using UnityEngine;

public class SplineWalker : MonoBehaviour
{
  public BezierSpline spline;
  public float duration;
  public bool lookForward;
  public SplineWalkerMode mode;
  public MovingPlatformType movingPlatformType = MovingPlatformType.StartsWhenPlayerLands;
  public GameObject movingObjectPrefab;
  public bool goingForward = true;

  private float _progress;
  private GameObject _gameObject;
  private bool _isMoving = false;

  void player_OnGroundedPlatformChanged(object sender, PlayerController.GroundedPlatformChangedEventArgs e)
  {
    if (e.currentPlatform != _gameObject)
      return; // we need to check that the player landed on this platform

    if (!_isMoving && movingPlatformType == MovingPlatformType.StartsWhenPlayerLands)
    {
      Logger.Info("Player landed on platform, start move...");
      _isMoving = true;
    }
  }
  void attachableObject_Attached(IAttachableObject attachableObject, GameObject obj)
  {
    if (!_isMoving)
    {
      Logger.Info("Player landed on platform, start move...");
      _isMoving = true;

      attachableObject.Attached -= attachableObject_Attached;
    }
  }
  void Start()
  {
    ObjectPoolingManager.Instance.RegisterPool(movingObjectPrefab, 1, int.MaxValue);

    _gameObject = ObjectPoolingManager.Instance.GetObject(movingObjectPrefab.name);

    if (!goingForward)
      _progress = 1f;
    else
      _progress = 0f;

    spline.CalculateLengths(10);

    if (movingPlatformType == MovingPlatformType.StartsWhenPlayerLands)
    {
      IAttachableObject attachableObject = _gameObject.GetComponent<IAttachableObject>();
      if (attachableObject != null)
      {
        attachableObject.Attached += attachableObject_Attached;
      }
      else
      {
        GameManager.instance.player.OnGroundedPlatformChanged += player_OnGroundedPlatformChanged;
      }

      _gameObject.transform.localPosition = spline.GetLengthAdjustedPoint(_progress);
    }
    else
    {
      _isMoving = true;
    }
  }

  private void Update()
  {
    if (!_isMoving)
      return;

    if (goingForward)
    {
      _progress += Time.deltaTime / duration;
      if (_progress > 1f)
      {
        if (mode == SplineWalkerMode.Once)
        {
          _progress = 1f;
        }
        else if (mode == SplineWalkerMode.Loop)
        {
          _progress -= 1f;
        }
        else
        {
          _progress = 2f - _progress;
          goingForward = false;
        }
      }
    }
    else
    {
      _progress -= Time.deltaTime / duration;
      if (_progress < 0f)
      {
        if (mode == SplineWalkerMode.Once)
        {
          _progress = 1f;
        }
        else if (mode == SplineWalkerMode.Loop)
        {
          _progress = 1f - _progress;
        }
        else
        {
          _progress = -_progress;
          goingForward = true;
        }
      }
    }

    _gameObject.transform.localPosition = spline.GetLengthAdjustedPoint(_progress);
    if (lookForward)
    {
      Vector3 direction = spline.GetLengthAdjustedDirection(_progress);

      float rot_z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
      _gameObject.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }
  }
}