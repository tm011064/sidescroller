using UnityEngine;
using System.Collections;

public class PlayerControllerSprite : MonoBehaviour
{
  private PlayerController _playerController;

  void Start()
  {
    _playerController = GameManager.instance.player;
  }

  //void LateUpdate()
  //{
  //  float zRotation;

  //  // TODO (Roman): this is a quick hack to deal with local x axis scale on left right movement.
  //  if (Mathf.Abs(Mathf.Abs(this.transform.rotation.eulerAngles.z) - Mathf.Abs(_playerController.spriteRotation.eulerAngles.z)) >= 180f)
  //  {
  //    zRotation = _playerController.spriteRotation.eulerAngles.z;
  //  }
  //  else
  //  {
  //    zRotation = Mathf.Lerp(
  //      this.transform.rotation.eulerAngles.z
  //      , _playerController.spriteRotation.eulerAngles.z
  //      , Time.deltaTime * 12.5f);
  //  }

  //  this.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
  //}
}
