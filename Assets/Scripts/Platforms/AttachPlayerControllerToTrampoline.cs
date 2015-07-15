using UnityEngine;

public class AttachPlayerControllerToTrampoline : BaseMonoBehaviour
{
  public bool canJump = true;

  public float platformDownwardDistance = -32f;
  public float platformDownwardDuration = .2f;
  public iTween.EaseType platformDownwardEaseType = iTween.EaseType.easeInOutQuart;

  public float platformUpwardDuration = .6f;
  public iTween.EaseType platformUpwardEaseType = iTween.EaseType.easeOutBounce;

  public float autoBounceJumpHeightMultiplier = 1f;
  public float jumpHeightMultiplier = 2f;

  public float onTrampolineSkidDamping = 5f;

  private bool _isPlayerControllerAttached = false;
  private bool _isGoingUp = false;
  private bool _hasUpMoveCompleted = false;
  private bool _hasBounced = false;
  private Vector3 _lastPosition;

  public GameObject trampolinePrefab;
  private GameObject _gameObject;
  protected PlayerController _playerController;

  private TrampolineBounceControlHandler _trampolineBounceControlHandler = null;

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
    _gameObject.SetActive(true);

    _lastPosition = _gameObject.transform.position;
  }

  void OnEnable()
  {
    _playerController.characterPhysicsManager.onControllerBecameGrounded += characterPhysicsManager_onControllerBecameGrounded;
    _playerController.characterPhysicsManager.onControllerLostGround += characterPhysicsManager_onControllerLostGround;
  }

  void OnDisable()
  {
    _playerController.characterPhysicsManager.onControllerBecameGrounded -= characterPhysicsManager_onControllerBecameGrounded;
    _playerController.characterPhysicsManager.onControllerLostGround -= characterPhysicsManager_onControllerLostGround;
  }

  void characterPhysicsManager_onControllerLostGround()
  {
    if (_playerController.transform.parent == this._gameObject.transform)
    {
      _isPlayerControllerAttached = false;
      _playerController.transform.parent = null;

      if (_trampolineBounceControlHandler != null
        && !_trampolineBounceControlHandler.HasJumped) // we check this in case the player has slid off thr trampoline before being able to jump
      {
        _playerController.RemoveControlHandler(_trampolineBounceControlHandler);
        _trampolineBounceControlHandler = null;
      }

      Logger.Info("Removed parent (" + this.gameObject.transform + ") relationship from child (" + _playerController.name + ")");
    }
  }

  void characterPhysicsManager_onControllerBecameGrounded(GameObject obj)
  {
    if (obj == this._gameObject && _playerController.transform.parent != this._gameObject.transform)
    {
      _isGoingUp = false;
      _hasBounced = false;
      _hasUpMoveCompleted = false;

      _isPlayerControllerAttached = true;

      _playerController.transform.parent = this._gameObject.transform;

      _trampolineBounceControlHandler = new TrampolineBounceControlHandler(_playerController, -1f, jumpHeightMultiplier, onTrampolineSkidDamping, canJump);
      _playerController.PushControlHandler(_trampolineBounceControlHandler);

      iTween.MoveBy(this._gameObject
        , iTween.Hash(
        "y", platformDownwardDistance
        , "time", platformDownwardDuration
        , "easetype", platformDownwardEaseType
        , "oncomplete", "OnDownMoveComplete"
        , "oncompletetarget", this.gameObject
        ));

      Logger.Info("Added parent (" + this.gameObject.transform + ") relationship to child (" + _playerController.name + ")");
    }
  }

  private void OnUpMoveCompleted()
  {
    _hasUpMoveCompleted = true;
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
    if (_isPlayerControllerAttached
      && !_hasBounced
      && (_isGoingUp || _hasUpMoveCompleted)
      && _lastPosition.y > _gameObject.transform.position.y
      )
    {
      if (_trampolineBounceControlHandler != null)
      {
        _playerController.RemoveControlHandler(_trampolineBounceControlHandler);
        _trampolineBounceControlHandler = null;
      }

      _playerController.PushControlHandler(new TrampolineAutoBounceControlHandler(_playerController, autoBounceJumpHeightMultiplier));
      _hasBounced = true;
    }

    _lastPosition = _gameObject.transform.position;
  }
}
