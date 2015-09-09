using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnemyRocket : MonoBehaviour, IEnemyProjectile
{
  private bool _hasStarted = false;
  private float _acceleration;
  private float _targetVelocity;
  private Vector2 _direction;
  private Vector2 _velocity;

  void OnBecameInvisible()
  {
    ObjectPoolingManager.Instance.Deactivate(this.gameObject);
  }

  void OnTriggerEnter2D(Collider2D col)
  {
    // TODO (Roman): this should be somewhere else - this is just test code
    GameObject deathParticles = ObjectPoolingManager.Instance.GetObject(GameManager.instance.gameSettings.pooledObjects.defaultEnemyDeathParticlePrefab.prefab.name);
    deathParticles.transform.position = this.gameObject.transform.position;

    ObjectPoolingManager.Instance.Deactivate(this.gameObject);
    
    if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
    {
      GameManager.instance.powerUpManager.KillPlayer();
    }
  }

  void Update()
  {
    if (_hasStarted)
    {
      _velocity = _velocity + (_direction * _acceleration * Time.deltaTime);
      if (_velocity.magnitude > _targetVelocity)
        _velocity = _direction * _targetVelocity;

      this.transform.Translate(_velocity);
    }
  }

  #region IEnemyProjectile Members

  public void StartMove(Vector2 startPosition, Vector2 direction, float acceleration, float targetVelocity)
  {
    this.transform.position = startPosition;
    _hasStarted = true;
    _direction = direction.normalized;
    _acceleration = acceleration;
    _targetVelocity = targetVelocity;
    _velocity = Vector2.zero;
  }

  #endregion
}

