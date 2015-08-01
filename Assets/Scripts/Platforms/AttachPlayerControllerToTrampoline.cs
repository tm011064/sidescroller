using System;
using UnityEngine;

public interface IAttachableObject
{
  event Action<IAttachableObject, GameObject> Attached;
  event Action<IAttachableObject, GameObject> Detached;
}

public partial class AttachPlayerControllerToTrampoline : MonoBehaviour, IAttachableObject
{
  public bool canJump = true;

  public float platformDownwardDistance = -32f;
  public float platformDownwardDuration = .2f;
  public iTween.EaseType platformDownwardEaseType = iTween.EaseType.easeInOutQuart;

  public float platformUpwardDuration = .6f;
  public iTween.EaseType platformUpwardEaseType = iTween.EaseType.easeOutBounce;

  public float autoBounceFixedJumpHeight = 224f;  
  public float fixedJumpHeight = 448f;

  public float onTrampolineSkidDamping = 5f;

  private bool _isPlayerControllerAttached = false;
  private bool _isGoingUp = false;
  private bool _hasReachedUpMoveApex = false;
  private bool _hasBounced = false;
  private Vector3 _lastPosition;

  public GameObject trampolinePrefab;
  private GameObject _gameObject;
  protected PlayerController _playerController;

  private TrampolineBounceControlHandler _trampolineBounceControlHandler = null;
  
  #region IAttachableObject Members

  public event Action<IAttachableObject, GameObject> Attached;

  public event Action<IAttachableObject, GameObject> Detached;

  #endregion

  void Awake()
  {
    _playerController = GameManager.instance.player;
  }

  void Start()
  {
    // we wanna do this in start as we know that the player has been added to the game context
    ObjectPoolingManager.Instance.RegisterPool(trampolinePrefab, 1, int.MaxValue);

    _gameObject = ObjectPoolingManager.Instance.GetObject(trampolinePrefab.name);
    _gameObject.transform.position = this.transform.position;
    _gameObject.transform.parent = this.transform;

    _lastPosition = _gameObject.transform.position;
  }

  void OnEnable()
  {
    _playerController.OnGroundedPlatformChanged += _playerController_OnGroundedPlatformChanged;
  }

  void OnDisable()
  {
    _playerController.OnGroundedPlatformChanged -= _playerController_OnGroundedPlatformChanged;
  }

  void _playerController_OnGroundedPlatformChanged(object sender, PlayerController.GroundedPlatformChangedEventArgs e)
  {
    if ((e.currentPlatform == null && _playerController.transform.parent == this._gameObject.transform) // either player is in air
      || e.previousPlatform == this._gameObject // or the previous platform was the trampoline
      )
    {
      // lost ground
      _isPlayerControllerAttached = false;
      _playerController.transform.parent = null;

      if (_trampolineBounceControlHandler != null
        && !_trampolineBounceControlHandler.HasJumped) // we check this in case the player has slid off thr trampoline before being able to jump
      {
        _playerController.RemoveControlHandler(_trampolineBounceControlHandler);
        _trampolineBounceControlHandler = null;
      }

      var handler = this.Detached;
      if (handler != null)
        handler.Invoke(this, this.gameObject);

      Logger.Info("Removed parent (" + this.gameObject.transform + ") relationship from child (" + _playerController.name + ")");
    }
    else if (e.currentPlatform == this._gameObject)
    {
      if (_playerController.transform.parent != this._gameObject.transform)
      {
        _isGoingUp = false;
        _hasBounced = false;
        _hasReachedUpMoveApex = false;

        _isPlayerControllerAttached = true;

        _playerController.transform.parent = this._gameObject.transform;

        _trampolineBounceControlHandler = new TrampolineBounceControlHandler(_playerController, -1f, fixedJumpHeight, onTrampolineSkidDamping, canJump);
        _playerController.PushControlHandler(_trampolineBounceControlHandler);

        iTween.MoveBy(this._gameObject
          , iTween.Hash(
          "y", platformDownwardDistance
          , "time", platformDownwardDuration
          , "easetype", platformDownwardEaseType
          , "oncomplete", "OnDownMoveComplete"
          , "oncompletetarget", this.gameObject
          ));

        var handler = this.Attached;
        if (handler != null)
          handler.Invoke(this, this.gameObject);

        Logger.Info("Added parent (" + this.gameObject.transform + ") relationship to child (" + _playerController.name + ")");        
      }
    }
  }
  
  private void OnUpMoveCompleted()
  {

  }

  private void OnDownMoveComplete()
  {
    _isGoingUp = true;

    iTween.MoveBy(this._gameObject
      , iTween.Hash(
      "y", -platformDownwardDistance
      , "time", platformUpwardDuration
      , "easetype", platformUpwardEaseType
        , "oncomplete", "OnUpMoveCompleted"
        , "oncompletetarget", this.gameObject
      ));
  }

  void Update()
  {
    if (!_hasReachedUpMoveApex 
      && _isGoingUp
      && _lastPosition.y > _gameObject.transform.position.y)
    {// we assume there is a bounce out animation, so we want to find out when the up move has reached the apex before bouncing around a bit and getting settled
      _hasReachedUpMoveApex = true;
    }

    if (_isPlayerControllerAttached
      && !_hasBounced
      && _hasReachedUpMoveApex
      )
    {
      if (_trampolineBounceControlHandler != null)
      {
        _playerController.RemoveControlHandler(_trampolineBounceControlHandler);
        _trampolineBounceControlHandler = null;
      }

      _playerController.PushControlHandler(new TrampolineAutoBounceControlHandler(_playerController, autoBounceFixedJumpHeight));
      _hasBounced = true;
    }

    _lastPosition = _gameObject.transform.position;
  }
}
