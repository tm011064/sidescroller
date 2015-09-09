using UnityEngine;
using System.Collections;

public class FixedBackgroundScroller : MonoBehaviour
{
  private Vector2 _savedOffset;

  private Transform _transform;
  private Vector3 _oldPos;
  Vector3 _distance;

  private float _horizontalSmoothDampVelocity;
  private float _verticalSmoothDampVelocity;

  void Awake()
  {
    _transform = Camera.main.transform;
  }

  void Start()
  {
    _oldPos = _transform.position;
  }

  void Update()
  {
    Vector2 delta = _transform.position - _oldPos;
    this.gameObject.transform.Translate(new Vector3(delta.x * .8f, delta.y * .7f), Space.World);
    _oldPos = _transform.position;
  }
}