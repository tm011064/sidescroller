﻿using UnityEngine;
using System.Collections;
using System;

public enum MovingPlatformType
{
  MovesAlways,
  StartsWhenPlayerLands
}

public class AttachObjectToPath : BaseMonoBehaviour
{
  public GameObject attachedObject;
  public iTween.EaseType easeType = iTween.EaseType.easeInOutSine;
  public iTween.LoopType loopType = iTween.LoopType.pingPong;
  public float time = 5f;
  public float delay = 0f;
  public float pauseTimeOnLoopComplete = 0f;
  public MovingPlatformType movingPlatformType = MovingPlatformType.StartsWhenPlayerLands;

  private GameObject _gameObject;
  private iTweenPath _iTweenPath;

  private bool _isMoving = false;

  void onPlayerGrounded(GameObject obj)
  {
    if (obj != _gameObject)
      return; // we need to check that the player landed on this platform

    if (!_isMoving && movingPlatformType == MovingPlatformType.StartsWhenPlayerLands)
    {
      Logger.Info("Player landed on platform, start move...");
      StartMove();
    }
  }

  private void OnTweenComplete()
  {
    if (pauseTimeOnLoopComplete > 0f)
      StartCoroutine(PauseTween(pauseTimeOnLoopComplete));
  }

  private IEnumerator PauseTween(float waitTime)
  {
    //Pause all itweens on target object
    iTween.Pause(_gameObject);
    yield return new WaitForSeconds(waitTime);

    iTween.Resume(_gameObject);
  }

  private void StartMove()
  {
    Vector3[] path = _iTweenPath.GetPathInWorldSpace();
    _gameObject.transform.position = path[0];

    iTween.MoveTo(_gameObject, iTween.Hash(
      "path", path
      , "time", time
      , "easetype", easeType
      , "looptype", loopType
      , "delay", delay
      , "oncomplete", "OnTweenComplete"
      , "oncompletetarget", this.gameObject
      ));

    _isMoving = true;

    Logger.Info("Start move floating platform " + this.name);
  }

  void Awake()
  {
    var pathContainer = GetComponent<iTweenPath>();
    pathContainer.SetPathName(Guid.NewGuid().ToString());
  }
  
  // Use this for initialization
  void Start()
  {
    // we wanna do this in start as we know that the player has been added to the game context
    ObjectPoolingManager.Instance.RegisterPool(attachedObject, 1, int.MaxValue);

    _gameObject = ObjectPoolingManager.Instance.GetObject(attachedObject.name);

    if (movingPlatformType == MovingPlatformType.StartsWhenPlayerLands)
    {
      GameManager.instance.player.characterPhysicsManager.onControllerBecameGrounded += onPlayerGrounded;
    }

    _iTweenPath = GetComponent<iTweenPath>();

    _gameObject.transform.position = _iTweenPath.GetFirstNodeInWorldSpace();
    _gameObject.SetActive(true);

    if (movingPlatformType == MovingPlatformType.MovesAlways)
      StartMove();
  }

}