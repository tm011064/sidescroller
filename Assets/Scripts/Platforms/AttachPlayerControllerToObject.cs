using UnityEngine;
using System.Collections;

public class AttachPlayerControllerToObject : MonoBehaviour
{
  protected PlayerController _playerController;

  protected virtual void OnPlayerControllerGotGrounded() { /* can be overridden */  }

  void Awake()
  {
    _playerController = GameManager.instance.player;
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
    if (_playerController.transform.parent == this.gameObject.transform)
    {
      _playerController.transform.parent = null;
      Logger.Info("Removed parent (" + this.gameObject.transform + ") relationship from child (" + _playerController.name + ")");
    }
  }

  void characterPhysicsManager_onControllerBecameGrounded(GameObject obj)
  {
    if (obj == this.gameObject && _playerController.transform.parent != this.gameObject.transform)
    {
      _playerController.transform.parent = this.gameObject.transform;
      OnPlayerControllerGotGrounded();
      Logger.Info("Added parent (" + this.gameObject.transform + ") relationship to child (" + _playerController.name + ")");
    }
  }
}
