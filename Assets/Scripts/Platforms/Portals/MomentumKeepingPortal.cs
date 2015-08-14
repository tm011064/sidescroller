using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(EdgeCollider2D))]
public class MomentumKeepingPortal : BaseMonoBehaviour
{
  [Tooltip("The other end of the portal where the player gets ejected.")]
  public GameObject connectedPortal;
  [Tooltip("When going through portal A and exiting at portal B with emission angle 180 deg, the exit velocity will be the incoming velocity rotated by the emission angle.")]
  public float emissionAngle;
  [Tooltip("This can be used to multiply the exit .")]
  public Vector2 emissionVelocityMultiplier = new Vector2(1, 1);
  public Vector3 spawnOffset = Vector3.zero;

  private MomentumKeepingPortal _connectedMomentumKeepingPortal;

  void Awake()
  {
    _connectedMomentumKeepingPortal = connectedPortal.GetComponent<MomentumKeepingPortal>();
    Logger.Assert(_connectedMomentumKeepingPortal != null, "Connected Portal must contain MomentumKeepingPortal script.");
  }

  void OnTriggerEnter2D(Collider2D col)
  {
    PlayerController playerController = GameManager.instance.player;

    Vector3 velocity = playerController.characterPhysicsManager.velocity;

    float angle = Mathf.Atan2(velocity.y, velocity.x);
    float exitAngle = _connectedMomentumKeepingPortal.emissionAngle * Mathf.Deg2Rad + angle;

    Vector3 exitVelocity = new Vector3(Mathf.Cos(exitAngle), Mathf.Sin(exitAngle), 0f);

    exitVelocity.x *= velocity.magnitude * _connectedMomentumKeepingPortal.emissionVelocityMultiplier.x;
    exitVelocity.y *= velocity.magnitude * _connectedMomentumKeepingPortal.emissionVelocityMultiplier.y;

    Debug.Log(this.name + ": Incoming velocity: " + velocity + ", angle: " + angle * Mathf.Rad2Deg + ", exit angle: " + exitAngle * Mathf.Rad2Deg + ", exit velocity: " + exitVelocity);

    playerController.transform.position = connectedPortal.transform.position + _connectedMomentumKeepingPortal.spawnOffset;
    playerController.characterPhysicsManager.lastMoveCalculationResult.collisionState.below = false;
    playerController.characterPhysicsManager.velocity = exitVelocity;
  }

}
