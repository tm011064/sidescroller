using UnityEngine;
using System.Collections;
using System;

public class AttachPlayerControllerToObject : MonoBehaviour, IAttachableObject
{
  #region members
  protected PlayerController _playerController;
  #endregion

  #region events
  public event Action OnPlayerControllerGotGrounded;

  void _playerController_OnGroundedPlatformChanged(object sender, PlayerController.GroundedPlatformChangedEventArgs e)
  {
    if (e.previousPlatform == this.gameObject && _playerController.transform.parent == this.gameObject.transform)
    {
      _playerController.transform.parent = null;
      var handler = this.Detached;
      if (handler != null)
        handler.Invoke(this, this.gameObject);
      Logger.Info("Removed parent (" + this.gameObject.name + " [ " + this.GetHashCode() + " ]) relationship from child (" + _playerController.name + ") [1]");
    }
    else
    {
      if (e.currentPlatform == this.gameObject && _playerController.transform.parent != this.gameObject.transform)
      {
        if (_playerController.transform.parent != null)
        {
          _playerController.transform.parent = null;
          Logger.Info("Removed parent (" + this.gameObject.name + " [ " + this.GetHashCode() + " ]) relationship from child (" + _playerController.name + ") [2]");
        }
        _playerController.transform.parent = this.gameObject.transform;
        Logger.Info("Added parent (" + this.gameObject.name + " [ " + this.GetHashCode() + " ]) relationship to child (" + _playerController.name + ")");

        var handler = OnPlayerControllerGotGrounded;
        if (handler != null)
        {
          handler.Invoke();
        }
        
        var handler2 = this.Attached;
        if (handler2 != null)
          handler2.Invoke(this, this.gameObject);
      }
    }
  }
  #endregion

  #region enable/disable
  void OnBeforeDisable()
  {
    if (_playerController != null && _playerController.transform.parent == this.gameObject.transform)
      _playerController.transform.parent = null;
  }
  void OnEnable()
  {
    _playerController = GameManager.instance.player;
    _playerController.OnGroundedPlatformChanged += _playerController_OnGroundedPlatformChanged;
  }
  void OnDisable()
  {
    _playerController.OnGroundedPlatformChanged -= _playerController_OnGroundedPlatformChanged;
  }
  #endregion

  #region IAttachableObject Members

  public event Action<IAttachableObject, GameObject> Attached;

  public event Action<IAttachableObject, GameObject> Detached;

  #endregion
}
