using UnityEngine;
using System.Collections;

public class ConveyorBelt : MonoBehaviour
{
  public float speed = 200f;

  protected PlayerController _playerController;
  private bool _isGrounded;

  void Update()
  {
    if (_isGrounded)
    {
      _playerController.characterPhysicsManager.AddHorizontalForce(speed * Time.deltaTime);
    }
  }

  #region enable/disable
  void _playerController_OnGroundedPlatformChanged(object sender, PlayerController.GroundedPlatformChangedEventArgs e)
  {
    if (e.currentPlatform == this.gameObject)
    {
      _isGrounded = true;
    }
    else
    {
      _isGrounded = false;
    }
  }

  void OnEnable()
  {
    _playerController.OnGroundedPlatformChanged += _playerController_OnGroundedPlatformChanged;
  }

  void OnDisable()
  {
    _playerController.OnGroundedPlatformChanged -= _playerController_OnGroundedPlatformChanged;
  }
  #endregion

  #region start/awake
  void Awake()
  {
    _playerController = GameManager.instance.player;
  }
  #endregion
}
