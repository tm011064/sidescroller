using UnityEngine;
using System.Collections;

public class OffsetScroller : MonoBehaviour
{
  private Vector2 _savedOffset;

  private Renderer _renderer;
  private Transform _transform;
  private Vector3 _oldPos;
  private Vector2 _lastOffset;
  Vector3 _distance;

  private float _horizontalSmoothDampVelocity;
  private float _verticalSmoothDampVelocity;

  void Awake()
  {
    _renderer = this.GetComponent<Renderer>();
    _transform = Camera.main.transform;
  }

  void Start()
  {
    _distance = this.transform.position - _transform.position;

    _savedOffset = _renderer.sharedMaterial.GetTextureOffset("_MainTex");
    _oldPos = _transform.position;
    _lastOffset = _savedOffset;
  }

  void LateUpdate()
  {
    Vector2 delta = _transform.position - _oldPos;

    float y = Mathf.Repeat(delta.y / 5000f, 1);
    float x = Mathf.Repeat(delta.x / 5000f, 1);

    //_lastOffset = new Vector2(
    //  Mathf.SmoothDamp(_lastOffset.x, _lastOffset.x - x, ref _horizontalSmoothDampVelocity, 2f)
    //  , Mathf.SmoothDamp(_lastOffset.y, _lastOffset.y - y, ref _verticalSmoothDampVelocity, 2f)
    //  );

    /*
     
      Vector3 targetPositon = hvec - cameraOffset;
      transform.position = new Vector3(
        Mathf.SmoothDamp(transform.position.x, targetPositon.x, ref _horizontalSmoothDampVelocity, _cameraMovementSettings.smoothDampMoveSettings.horizontalSmoothDampTime)
        , Mathf.SmoothDamp(transform.position.y, targetPositon.y, ref _verticalSmoothDampVelocity, verticalSmoothDampTime)
        , targetPositon.z);
     */

    _lastOffset = _lastOffset + new Vector2(x, y);



    _renderer.sharedMaterial.SetTextureOffset("_MainTex", new Vector2(Mathf.Repeat( _lastOffset.x, 1), Mathf.Repeat( _lastOffset.y, 1)));



    //Vector2 offset = _lastOffset + new Vector2(x, y);

    //_renderer.sharedMaterial.SetTextureOffset("_MainTex", offset);

    _oldPos = _transform.position;
    //_lastOffset = offset;
  }

  void OnDisable()
  {
    _renderer.sharedMaterial.SetTextureOffset("_MainTex", _savedOffset);
  }
}