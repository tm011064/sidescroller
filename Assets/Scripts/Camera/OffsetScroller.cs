using UnityEngine;
using System.Collections;

public class OffsetScroller : MonoBehaviour
{
  public float speedFactor = 2000f;

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

    float y = Mathf.Repeat(delta.y / speedFactor, 1);
    float x = Mathf.Repeat(delta.x / speedFactor, 1);
    
    _lastOffset = _lastOffset + new Vector2(x, y);
    _renderer.sharedMaterial.SetTextureOffset("_MainTex", new Vector2(Mathf.Repeat( _lastOffset.x, 1), Mathf.Repeat( _lastOffset.y, 1)));
    
    _oldPos = _transform.position;
  }

  void OnDisable()
  {
    _renderer.sharedMaterial.SetTextureOffset("_MainTex", _savedOffset);
  }
}