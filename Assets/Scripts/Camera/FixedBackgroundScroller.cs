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
    Debug.Log("Translate: " + delta);
    this.gameObject.transform.Translate(delta, Space.Self);
    _oldPos = _transform.position;
  }
}