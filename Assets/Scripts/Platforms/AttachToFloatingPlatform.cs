using UnityEngine;
using System.Collections;

public class AttachToFloatingPlatform : MonoBehaviour
{
  private bool _isPlayerAttached;

  void OnEnable()
  {
    PlayerController playerController = GameManager.instance.player;
    if (playerController != null && playerController.characterPhysicsManager != null)
    {
      playerController.characterPhysicsManager.onControllerBecameGrounded += characterPhysicsManager_onControllerBecameGrounded;
      playerController.characterPhysicsManager.onControllerLostGround += characterPhysicsManager_onControllerLostGround;
    }
    _isPlayerAttached = false;
  }

  void OnDisable()
  {
    PlayerController playerController = GameManager.instance.player;
    if (playerController != null && playerController.characterPhysicsManager != null)
    {
      playerController.characterPhysicsManager.onControllerBecameGrounded -= characterPhysicsManager_onControllerBecameGrounded;
      playerController.characterPhysicsManager.onControllerLostGround -= characterPhysicsManager_onControllerLostGround;
    }
  }

  void characterPhysicsManager_onControllerLostGround()
  {
    if (_isPlayerAttached)
    {
      if (GameManager.instance.player.transform.parent == this.gameObject.transform)
      {
        GameManager.instance.player.transform.parent = null;
        Logger.Info("Player detached from " + this.gameObject.name);

        _isPlayerAttached = false;
      }
    }
  }

  void characterPhysicsManager_onControllerBecameGrounded(GameObject obj)
  {
    if (!_isPlayerAttached)
    {
      if (obj == this.gameObject)
      {
        GameManager.instance.player.transform.parent = this.gameObject.transform;
        Logger.Info("Game object: " + obj.name + " attached to " + this.gameObject.name);

        _isPlayerAttached = true;
      }
    }
  }
}
