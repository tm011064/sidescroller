using UnityEngine;

public class SplineWalker : MonoBehaviour
{
  public BezierSpline spline;
  public float duration;
  public bool lookForward;
  public SplineWalkerMode mode;
  public GameObject movingObjectPrefab;
  public bool goingForward = true;

  private float _progress;
  private GameObject _gameObject;

  void Start()
  {
    ObjectPoolingManager.Instance.RegisterPool(movingObjectPrefab, 1, int.MaxValue);

    _gameObject = ObjectPoolingManager.Instance.GetObject(movingObjectPrefab.name);

    if (!goingForward)
      _progress = 1f;
    else
      _progress = 0f;

    spline.CalculateLengths(10);
  }

  private void Update()
  {
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