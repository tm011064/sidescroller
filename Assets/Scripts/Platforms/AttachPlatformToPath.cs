using UnityEngine;
using System.Collections;
using System;

public enum MovingPlatformType
{
  MovesAlways,
  StartsWhenPlayerLands
}

public class AttachPlatformToPath : BaseMonoBehaviour
{
  public GameObject floatingAttachedPlatform;
  public iTween.EaseType easeType = iTween.EaseType.easeInOutSine;
  public iTween.LoopType loopType = iTween.LoopType.pingPong;
  public float time = 5f;
  public MovingPlatformType movingPlatformType = MovingPlatformType.StartsWhenPlayerLands;
  public float visibiltyCheckInterval = .1f;

  private GameObject _gameObject;

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

  private void StartMove()
  {
    var pathContainer = GetComponent<iTweenPath>();
    Vector3[] path = iTweenPath.GetPath(pathContainer.pathName);

    _gameObject.transform.position = path[0];

    iTween.MoveTo(_gameObject, iTween.Hash(
      "path", path
      , "time", time
      , "easetype", easeType
      , "looptype", loopType
      ));

    _isMoving = true;

    Logger.Info("Start move floating platform " + this.name);
  }  

  void Awake()
  {
    var pathContainer = GetComponent<iTweenPath>();
    pathContainer.SetPathName(Guid.NewGuid().ToString());
  }

  protected override void OnGotVisible()
  {
    switch (this.movingPlatformType)
    {
      case MovingPlatformType.MovesAlways:
        if (!_isMoving)
        {
          StartMove();
        }
        break;

      case MovingPlatformType.StartsWhenPlayerLands:
        break;
    }
  }

  // Use this for initialization
  void Start()
  {
    // we wanna do this in start as we know that the player has been added to the game context
    ObjectPoolingManager.Instance.RegisterPool(floatingAttachedPlatform, 1, int.MaxValue);

    _gameObject = ObjectPoolingManager.Instance.GetObject(floatingAttachedPlatform.name);
    
    if (movingPlatformType == MovingPlatformType.StartsWhenPlayerLands)
    {
      GameManager.instance.player.characterPhysicsManager.onControllerBecameGrounded += onPlayerGrounded;
    }

    var pathContainer = GetComponent<iTweenPath>();
    Vector3[] path = iTweenPath.GetPath(pathContainer.pathName);

    Logger.Info("Start " + pathContainer.pathName + ", " + this.movingPlatformType.ToString());

    _gameObject.transform.position = path[0];
    _gameObject.SetActive(true);

    StartVisibilityChecks(visibiltyCheckInterval, _gameObject.GetComponent<BoxCollider2D>());
  }

}
