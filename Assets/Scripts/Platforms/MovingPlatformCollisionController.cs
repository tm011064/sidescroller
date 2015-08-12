using UnityEngine;
using System.Collections;

/// <summary>
/// 
/// </summary>
public class MovingPlatformCollisionController : MonoBehaviour
{
  private const string TRACE_TAG = "MovingPlatformCollisionController";

  private BoxCollider2D _boxCollider;
  private BoxCollider2D _playerBoxCollider;

  private PlayerController _playerController;
  private AttachPlayerControllerToObject _attachPlayerControllerToObject;

  private const float FUDGE_FACTOR = .0001f;

  void Start()
  {
    _playerController = GameManager.instance.player;

    _boxCollider = this.GetComponent<BoxCollider2D>();

    _playerBoxCollider = _playerController.boxCollider;
  }

  void LateUpdate()
  {
    if (_boxCollider.bounds.Intersects(_playerBoxCollider.bounds))
    {
      if (_playerController.currentPlatform != this.gameObject)
      {// this check is to find out whether character is standing on this object. If so, no need to perform any additional checks

        // Note: we assume that the player was hit by a horizontal move
        Vector2 topLeft = new Vector2(this._boxCollider.bounds.center.x - this._boxCollider.bounds.extents.x, this._boxCollider.bounds.center.y + this._boxCollider.bounds.extents.y);
        Vector2 bottomRight = new Vector2(this._boxCollider.bounds.center.x + this._boxCollider.bounds.extents.x, this._boxCollider.bounds.center.y - this._boxCollider.bounds.extents.y);

        Vector2 playerTopLeft = new Vector2(this._playerBoxCollider.bounds.center.x - this._playerBoxCollider.bounds.extents.x, this._playerBoxCollider.bounds.center.y + this._playerBoxCollider.bounds.extents.y);
        Vector2 playerBottomRight = new Vector2(this._playerBoxCollider.bounds.center.x + this._playerBoxCollider.bounds.extents.x, this._playerBoxCollider.bounds.center.y - this._playerBoxCollider.bounds.extents.y);

        if (playerBottomRight.x > topLeft.x
          && playerBottomRight.x < bottomRight.x)
        {// right side of player inside

          Logger.Trace(TRACE_TAG, "LEFT Collision. Current player position: " + _playerController.transform.position + ", bounds: " + _boxCollider.bounds + ", bounds player: "
                    + _playerBoxCollider.bounds);

          _playerController.transform.position = new Vector3(
            (this._boxCollider.gameObject.transform.position).x
            - _playerController.boxCollider.size.x / 2
            - FUDGE_FACTOR
            , _playerController.transform.position.y
            , _playerController.transform.position.z
            );

          Logger.Trace(TRACE_TAG, "Adjusted player position: " + _playerController.transform.position);
        }
        else if (playerTopLeft.x < bottomRight.x
              && playerTopLeft.x > topLeft.x)
        {

          Logger.Trace(TRACE_TAG, "RIGHT Collision. Current player position: " + _playerController.transform.position + ", bounds: " + _boxCollider.bounds + ", bounds player: "
                    + _playerBoxCollider.bounds);

          _playerController.transform.position = new Vector3(
            (this._boxCollider.gameObject.transform.position + this._boxCollider.offset.ToVector3() + this._boxCollider.offset.ToVector3()).x
            + _playerController.boxCollider.size.x / 2
            + FUDGE_FACTOR
            , _playerController.transform.position.y
            , _playerController.transform.position.z
            );

          Logger.Trace(TRACE_TAG, "Adjusted player position: " + _playerController.transform.position);
        }
        else
        {
          Logger.Trace(TRACE_TAG, "NO OVERLAP Collision. Current player position: " + _playerController.transform.position + ", bounds: " + _boxCollider.bounds + ", bounds player: "
                    + _playerBoxCollider.bounds);
        }
      }
    }
  }
}
