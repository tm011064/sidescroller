using UnityEngine;
using System.Collections;
using System;

public class StompingBlock : MonoBehaviour
{
  public GameObject attachedObject;
  public iTween.EaseType toMotionEaseType = iTween.EaseType.easeInOutSine;
  public iTween.EaseType backMotionEaseType = iTween.EaseType.easeInOutSine;
  public float toMotionTime = 5f;
  public float backMotionTime = 5f;
  public float pauseTimeOnToMotionComplete = 0f;
  public float pauseTimeOnBackMotionComplete = 0f;
  public float delay = 0f;

  private bool _isMovingBack = false;
  private GameObject _gameObject;
  private iTweenPath _iTweenPath;

  private void OnTweenComplete()
  {
    StartMove(!_isMovingBack);
  }

  private void StartMove()
  {
    StartMove(false);
  }
  private void StartMove(bool isMovingBack)
  {
    _isMovingBack = isMovingBack;

    _gameObject.SetActive(true);
    if (_isMovingBack)
    {
      Vector3[] path = _iTweenPath.GetPathInWorldSpaceReversed();
      _gameObject.transform.position = path[0];

      iTween.MoveTo(_gameObject, iTween.Hash(
        "path", path
        , "time", backMotionTime
        , "easetype", backMotionEaseType
        , "looptype", iTween.LoopType.none
        , "delay", pauseTimeOnBackMotionComplete
        , "oncomplete", "OnTweenComplete"
        , "oncompletetarget", this.gameObject
        ));
    }
    else
    {
      Vector3[] path = _iTweenPath.GetPathInWorldSpace();
      _gameObject.transform.position = path[0];

      iTween.MoveTo(_gameObject, iTween.Hash(
        "path", path
        , "time", toMotionTime
        , "easetype", toMotionEaseType
        , "looptype", iTween.LoopType.none
        , "delay", pauseTimeOnToMotionComplete
        , "oncomplete", "OnTweenComplete"
        , "oncompletetarget", this.gameObject
        ));
    }
  }

  void Awake()
  {
    var pathContainer = GetComponent<iTweenPath>();
    pathContainer.SetPathName(Guid.NewGuid().ToString());
  }
  void Start()
  {
    // we wanna do this in start as we know that the player has been added to the game context
    ObjectPoolingManager.Instance.RegisterPool(attachedObject, 1, int.MaxValue);

    _gameObject = ObjectPoolingManager.Instance.GetObject(attachedObject.name);

    _iTweenPath = GetComponent<iTweenPath>();

    _gameObject.transform.position = _iTweenPath.GetFirstNodeInWorldSpace();

    if (delay > 0f)
      Invoke("StartMove", delay);
    else
      StartMove(false);
  }
}
