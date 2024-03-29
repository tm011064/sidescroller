﻿using UnityEngine;
using System;
using System.Collections.Generic;

#region types and classes
[Flags]
public enum CharacterWallState
{
  NotOnWall = 1,
  OnLeftWall = 2,
  OnRightWall = 4,
  OnWall = OnLeftWall | OnRightWall
}

public struct CharacterCollisionState2D
{
  public bool right;
  public bool left;
  public bool above;
  public bool below;
  public bool becameGroundedThisFrame;
  public bool movingDownSlope;
  public float slopeAngle;
  public CharacterWallState characterWallState;
  public bool isFullyGrounded; // indicates whether the player is standing on an edge

  public bool wasGroundedLastFrame;
  public float lastTimeGrounded;

  public bool hasCollision()
  {
    return below || right || left || above;
  }

  public void reset()
  {
    characterWallState = CharacterWallState.NotOnWall;
    right = left = above = below = becameGroundedThisFrame = movingDownSlope = isFullyGrounded = false;
    slopeAngle = 0f;
  }

  public override string ToString()
  {
    return string.Format("[CharacterCollisionState2D] r: {0}, l: {1}, a: {2}, b: {3}, movingDownSlope: {4}, angle: {5}, wasGroundedLastFrame: {6}, becameGroundedThisFrame: {7}, onWallState: {8}, ifg: {9}",
                         right, left, above, below, movingDownSlope, slopeAngle, wasGroundedLastFrame, becameGroundedThisFrame, characterWallState, isFullyGrounded);
  }
}

public struct MoveCalculationResult
{
  public CharacterCollisionState2D collisionState;
  public Vector3 deltaMovement;
  public Vector3 originalDeltaMovement;
  public bool isGoingUpSlope;
}
#endregion

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class CharacterPhysicsManager : BasePhysicsManager
{
  #region const fields
  private const string TRACE_TAG = "CharacterPhysicsManager";
  private const string SLIDE_TRACE_TAG = "CharacterPhysicsManagerSlide";
  private const float POSITIVE_ZERO_MOVE_FUDGE_FACTOR = .0001f;
  private const float NEGATIVE_ZERO_MOVE_FUDGE_FACTOR = -.0001f;
  private const float K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR = 0.001f;
  #endregion

  #region nested classes

  private struct CharacterRaycastOrigins
  {
    public Vector3 topLeft;
    public Vector3 bottomRight;
    public Vector3 bottomLeft;
  }

  #endregion

  #region events
  public event Action<RaycastHit2D> onControllerCollidedEvent;
  public event Action<GameObject> onControllerBecameGrounded;
  public event Action onControllerLostGround;
  public event Action<Collider2D> onTriggerEnterEvent;
  public event Action<Collider2D> onTriggerStayEvent;
  public event Action<Collider2D> onTriggerExitEvent;
  #endregion

  #region fields

  #region public

  #region inspector
  /// <summary>
  /// mask with all layers that the player should interact with
  /// </summary>
  public LayerMask platformMask = 0;

  /// <summary>
  /// mask with all layers that trigger events should fire when intersected
  /// </summary>
  public LayerMask triggerMask = 0;
  /// <summary>
  /// mask with all layers that should act as one-way platforms. Note that one-way platforms should always be EdgeCollider2Ds. This is private because it does not support being
  /// updated anytime outside of the inspector for now.
  /// </summary>
  [SerializeField]
  private LayerMask oneWayPlatformMask = 0;

  /// <summary>
  /// the max slope angle that the CC2D can climb
  /// </summary>
  /// <value>The slope limit.</value>
  [Range(0, 90f)]
  public float slopeLimit = 30f;

  /// <summary>
  /// the threshold in the change in vertical movement between frames that constitutes jumping
  /// </summary>
  /// <value>The jumping threshold.</value>
  public float jumpingThreshold = 0.07f;

  /// <summary>
  /// curve for multiplying speed based on slope (negative = down slope and positive = up slope)
  /// </summary>
  //public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1.5f), new Keyframe(0, 1), new Keyframe(90, 0));
  public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 1));

  [Range(2, 20)]
  public int totalHorizontalRays = 8;
  [Range(2, 20)]
  public int totalVerticalRays = 4;

  [Tooltip("If true, each move call checks whether the character is fully grounded. Fully grounded means he is not standing over an edge.")]
  public bool performFullyGroundedChecks = true;
  [Tooltip("If true, each move call checks whether the character is next to a wall. This is useful for wall jumps.")]
  public bool performIsOnWallChecks = true;

  [Tooltip("If true, the player will be able to slide up overhanging edges and only bump his head if the collision is outside the 'Edge Slide Up Corner Width' distance.")]
  public bool enableEdgeSlideUpHelp;
  [Tooltip("The width of the corner (left and right) that will cause the player to slide up the wall of a platform on a vertical top collision.")]
  public float edgeSlideUpCornerWidth = 16f;
  #endregion

  [HideInInspector]
  [NonSerialized]
  public new Transform transform;

  [HideInInspector]
  [NonSerialized]
  public BoxCollider2D boxCollider;

  [HideInInspector]
  [NonSerialized]
  public Rigidbody2D rigidBody2D;

  [HideInInspector]
  [NonSerialized]
  public MoveCalculationResult lastMoveCalculationResult = new MoveCalculationResult();

  [HideInInspector]
  [NonSerialized]
  public Vector3 velocity;

  [HideInInspector]
  public List<RaycastHit2D> lastRaycastHits = new List<RaycastHit2D>();

  [HideInInspector]
  [NonSerialized]
  public AnimationCurve slopeSpeedMultiplierOverride = null;
  #endregion

  #region private
  private LayerMask _platformMaskWithoutOneWay = 0;
  /// <summary>
  /// this is used to calculate the downward ray that is cast to check for slopes. We use the somewhat arbitrary value 75 degrees
  /// to calculate the length of the ray that checks for slopes.
  /// </summary>
  private float _slopeLimitTangent = Mathf.Tan(50f * Mathf.Deg2Rad);

  /// <summary>
  /// holder for our raycast origin corners (TR, TL, BR, BL)
  /// </summary>
  private CharacterRaycastOrigins _raycastOrigins;

  /// <summary>
  /// stores any raycast hits that occur this frame. we have to store them in case we get a hit moving
  /// horizontally and vertically so that we can send the events after all collision state is set
  /// </summary>
  private List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>(2);

  // horizontal/vertical movement data
  private float _verticalDistanceBetweenRays;
  private float _horizontalDistanceBetweenRays;
  #endregion

  #endregion

  #region properties
  [SerializeField]
  [Range(0.001f, 10.3f)]
  private float _skinWidth = 0.02f;
  /// <summary>
  /// defines how far in from the edges of the collider rays are cast from. If cast with a 0 extent it will often result in ray hits that are
  /// not desired (for example a foot collider casting horizontally from directly on the surface can result in a hit)
  /// </summary>
  public float skinWidth
  {
    get { return _skinWidth; }
    set
    {
      _skinWidth = value;
      RecalculateDistanceBetweenRays();
    }
  }
  #endregion

  #region Monobehaviour

  void Awake()
  {
    // add our one-way platforms to our normal platform mask so that we can land on them from above
    platformMask |= oneWayPlatformMask;

    // cache some components
    transform = GetComponent<Transform>();
    boxCollider = GetComponent<BoxCollider2D>();
    rigidBody2D = GetComponent<Rigidbody2D>();

    // here, we trigger our properties that have setters with bodies
    skinWidth = _skinWidth;

    // we want to set our CC2D to ignore all collision layers except what is in our triggerMask
    for (var i = 0; i < 32; i++)
    {
      // see if our triggerMask contains this layer and if not ignore it
      if ((triggerMask.value & 1 << i) == 0)
      {
        Physics2D.IgnoreLayerCollision(gameObject.layer, i);
      }
    }

    _platformMaskWithoutOneWay = platformMask;
    _platformMaskWithoutOneWay &= ~oneWayPlatformMask;

    _topEdgeCollisionTestContainers = new TopEdgeCollisionTestContainer[totalVerticalRays];
  }

  public void OnTriggerEnter2D(Collider2D col)
  {
    if (onTriggerEnterEvent != null)
      onTriggerEnterEvent(col);
  }

  public void OnTriggerStay2D(Collider2D col)
  {
    if (onTriggerStayEvent != null)
      onTriggerStayEvent(col);
  }

  public void OnTriggerExit2D(Collider2D col)
  {
    if (onTriggerExitEvent != null)
      onTriggerExitEvent(col);
  }

  #endregion

  #region methods

  #region public

  public void AddHorizontalForce(float x)
  {
    this.velocity.Set(this.velocity.x + x, this.velocity.y, this.velocity.z);
  }
  public void AddVerticalForce(float y)
  {
    this.velocity.Set(this.velocity.x, this.velocity.y + y, this.velocity.z);
  }
  public void AddForce(float x, float y)
  {
    this.velocity.Set(this.velocity.x + x, this.velocity.y + y, this.velocity.z);
  }

  public bool CanMoveVertically(float verticalRayDistance, bool allowEdgeSlideUp)
  {
    verticalRayDistance += _skinWidth;

    if (enableEdgeSlideUpHelp && allowEdgeSlideUp)
    {
      #region going up && enableEdgeSlideUpHelp
            
      // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
      float leftOriginX = _raycastOrigins.topLeft.x;
      float rightOriginX = _raycastOrigins.bottomRight.x;
      float topOriginY = _raycastOrigins.topLeft.y;
      float leftAdjustmentBoundaryPosition = leftOriginX - _skinWidth + edgeSlideUpCornerWidth;
      float rightAdjustmentBoundaryPosition = rightOriginX + _skinWidth - edgeSlideUpCornerWidth;

      bool hasHit = false;
      bool hasHitOutsideAdjustmentBoundaries = false;
      bool hasHitWithinLeftAdjustmentBoundaries = false;
      bool hasHitWithinRightAdjustmentBoundaries = false;

      for (var i = 0; i < totalVerticalRays; i++)
      {
        _topEdgeCollisionTestContainers[i].insetLeft = leftOriginX + i * _horizontalDistanceBetweenRays;
        _topEdgeCollisionTestContainers[i].isWithinLeftAdjustmentArea = _topEdgeCollisionTestContainers[i].insetLeft <= leftAdjustmentBoundaryPosition;
        _topEdgeCollisionTestContainers[i].isWithinRightAdjustmentArea = _topEdgeCollisionTestContainers[i].insetLeft >= rightAdjustmentBoundaryPosition;

        Vector2 ray = new Vector2(_topEdgeCollisionTestContainers[i].insetLeft, topOriginY);

        _topEdgeCollisionTestContainers[i].raycastHit2D = Physics2D.Raycast(ray, Vector2.up, verticalRayDistance, _platformMaskWithoutOneWay);
        
        if (_topEdgeCollisionTestContainers[i].raycastHit2D)
        {
          hasHit = true;

          if (!hasHitOutsideAdjustmentBoundaries
            && !_topEdgeCollisionTestContainers[i].isWithinLeftAdjustmentArea
            && !_topEdgeCollisionTestContainers[i].isWithinRightAdjustmentArea)
          {
            hasHitOutsideAdjustmentBoundaries = true;
          }
          if (!hasHitWithinLeftAdjustmentBoundaries
            && _topEdgeCollisionTestContainers[i].isWithinLeftAdjustmentArea)
          {
            hasHitWithinLeftAdjustmentBoundaries = true;
          }
          if (!hasHitWithinRightAdjustmentBoundaries
            && _topEdgeCollisionTestContainers[i].isWithinRightAdjustmentArea)
          {
            hasHitWithinRightAdjustmentBoundaries = true;
          }
          
          float deltaMovementY = _topEdgeCollisionTestContainers[i].raycastHit2D.point.y - ray.y;
          if (!_topEdgeCollisionTestContainers[i].isWithinLeftAdjustmentArea
            && !_topEdgeCollisionTestContainers[i].isWithinRightAdjustmentArea
            && Mathf.Abs(deltaMovementY) < _skinWidth + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR)
          {
            return false;
          }
        }
      }
      
      if (hasHit)
      {
        if (!hasHitOutsideAdjustmentBoundaries)
        {
          RaycastHit2D leftAdjustmentRaycastHit = new RaycastHit2D();

          // first left
          Vector2 leftAdjustmentRay = new Vector2(leftAdjustmentBoundaryPosition, topOriginY);

          leftAdjustmentRaycastHit = Physics2D.Raycast(leftAdjustmentRay, Vector2.up, verticalRayDistance, _platformMaskWithoutOneWay);

          if (!leftAdjustmentRaycastHit
            && !hasHitWithinRightAdjustmentBoundaries)
          {
            return true;
          }

          // we reached this line, so do right
          RaycastHit2D rightAdjustmentRaycastHit = new RaycastHit2D();
          Vector2 rightAdjustmentRay = new Vector2(rightAdjustmentBoundaryPosition, topOriginY);
          rightAdjustmentRaycastHit = Physics2D.Raycast(rightAdjustmentRay, Vector2.up, verticalRayDistance, _platformMaskWithoutOneWay);

          if (!rightAdjustmentRaycastHit
            && !hasHitWithinLeftAdjustmentBoundaries)
          {
            return true;
          }
        }

        // TODO (Roman): additional check when moving platform moves into static player
      }
      #endregion
    }
    else
    {
      var initialRayOrigin = _raycastOrigins.topLeft;

      for (var i = 0; i < totalVerticalRays; i++)
      {
        var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

        DrawRay(ray, Vector2.up * verticalRayDistance, Color.red);

        RaycastHit2D raycastHit = Physics2D.Raycast(ray, Vector2.up, verticalRayDistance, _platformMaskWithoutOneWay);
        if (raycastHit)
        {
          // we need to check whether the hit point is on the edge of the collider. If not, the ray was sent from within the collider which can happen
          // on moving platforms        
          if (raycastHit.distance == 0f)
          {
            // if the distance is 0, we are inside a collider since the raycast origin is inside the player (due to skin width logic). This can
            // happen when a moving platform moves into a non moving player. In such a case we allow the jump to proceed.
            // in case this distance check doesn't work, we can also use "if (!raycastHit.collider.bounds.IsPointOnEdge(raycastHit.point))"
            Logger.Trace(TRACE_TAG, "Jump raycast hit [" + i + ", distance: " + raycastHit.distance + "] ignored because we are inside the collider. Ray: " + ray + ", hit point: " + raycastHit.point + ", collider bounds: " + raycastHit.collider.bounds);
          }
          else
          {
            Logger.Trace(TRACE_TAG, "Can not jump [" + i + ", distance: " + raycastHit.distance + "] because of ray: " + ray + ", hit point: " + raycastHit.point + ", collider bounds: " + raycastHit.collider.bounds);
            return false;
          }
        }
      }
    }

    return true;
  }



  public MoveCalculationResult SlideDown(Direction platformDirection, float downwardVelocity)
  {
    MoveCalculationResult moveCalculationResult = new MoveCalculationResult();
    moveCalculationResult.collisionState = new CharacterCollisionState2D();
    moveCalculationResult.collisionState.characterWallState = CharacterWallState.NotOnWall;

    // save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame
    moveCalculationResult.collisionState.wasGroundedLastFrame = lastMoveCalculationResult.collisionState.below;
    moveCalculationResult.collisionState.lastTimeGrounded = lastMoveCalculationResult.collisionState.lastTimeGrounded;
    moveCalculationResult.deltaMovement = new Vector3(0f, downwardVelocity, 0f);
    moveCalculationResult.originalDeltaMovement = moveCalculationResult.deltaMovement;

    // clear our state
    moveCalculationResult.collisionState.reset();
    _raycastHitsThisFrame.Clear();

    var desiredPosition = transform.position + moveCalculationResult.deltaMovement;
    PrimeRaycastOrigins(desiredPosition, moveCalculationResult.deltaMovement);

    var rayDistance = Mathf.Abs(boxCollider.size.x) + _skinWidth; // we use the width of the character, this is arbitrary but it works
    var rayDirection = platformDirection == Direction.Right ? Vector2.right : -Vector2.right;
    var initialRayOrigin = platformDirection == Direction.Right ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;

    RaycastHit2D raycastHit = new RaycastHit2D();

    var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y);

    DrawRay(ray, rayDirection * rayDistance, Color.white);

    Logger.Trace(SLIDE_TRACE_TAG, "SlideDown -> test ray origin: {0}, ray target position: {1}, delta: {2}, delta target position: {3}, platform direction: {4}"
      , ray
      , ray + new Vector2(rayDirection.x * rayDistance, rayDirection.y * rayDistance)
      , moveCalculationResult.deltaMovement
      , this.transform.position + moveCalculationResult.deltaMovement
      , platformDirection);

    // if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one). this will allow us to
    // walk up sloped oneWayPlatforms
    raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, _platformMaskWithoutOneWay);

    if (raycastHit)
    {
      float angle = Vector2.Angle(raycastHit.normal, Vector2.up);

      if (angle >= 90f)
      {
        moveCalculationResult.deltaMovement.x = 0f;
        moveCalculationResult.deltaMovement.y = downwardVelocity;
      }
      else
      {
        if (platformDirection == Direction.Right)
        {
          moveCalculationResult.deltaMovement.x = downwardVelocity * Mathf.Cos(angle * Mathf.Deg2Rad);
          moveCalculationResult.deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad)) * moveCalculationResult.deltaMovement.x;
        }
        else
        {
          moveCalculationResult.deltaMovement.x = -downwardVelocity * Mathf.Cos(angle * Mathf.Deg2Rad);
          moveCalculationResult.deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad)) * -moveCalculationResult.deltaMovement.x;
        }
      }

      Logger.Trace(SLIDE_TRACE_TAG, "SlideDown -> downward velocity: {0}, deltamovement: {1}, angle: {2}"
        , downwardVelocity
        , moveCalculationResult.deltaMovement
        , angle);

      // remember to remove the skinWidth from our deltaMovement
      if (platformDirection == Direction.Right)
      {
        //moveCalculationResult.deltaMovement.x -= _skinWidth;
        moveCalculationResult.deltaMovement.x -= .1f;
        moveCalculationResult.collisionState.right = true;
      }
      else
      {
        //moveCalculationResult.deltaMovement.x += _skinWidth;
        moveCalculationResult.deltaMovement.x += .1f;
        moveCalculationResult.collisionState.left = true;
      }

      Logger.Trace(SLIDE_TRACE_TAG, "SlideDown -> hit; Hit Point: {0}, Target Delta: {1}, Target Position: {2}"
        , raycastHit.point
        , moveCalculationResult.deltaMovement
        , (this.transform.position + moveCalculationResult.deltaMovement));

      _raycastHitsThisFrame.Add(raycastHit);
    }

    if (raycastHit)
    {
      transform.Translate(moveCalculationResult.deltaMovement, Space.World);
    }

    // only calculate velocity if we have a non-zero deltaTime
    if (Time.deltaTime > 0)
      velocity = moveCalculationResult.deltaMovement / Time.deltaTime;

    lastMoveCalculationResult = moveCalculationResult;
    lastRaycastHits = new List<RaycastHit2D>(_raycastHitsThisFrame);

    return moveCalculationResult;
  }

  public MoveCalculationResult CalculateMove(Vector3 deltaMovement)
  {
    // set small movements to zero
    if (deltaMovement.x <= POSITIVE_ZERO_MOVE_FUDGE_FACTOR && deltaMovement.x >= NEGATIVE_ZERO_MOVE_FUDGE_FACTOR)
      deltaMovement.x = 0f;
    if (deltaMovement.y <= POSITIVE_ZERO_MOVE_FUDGE_FACTOR && deltaMovement.y >= NEGATIVE_ZERO_MOVE_FUDGE_FACTOR)
      deltaMovement.y = 0f;

    Logger.Trace(TRACE_TAG, "Start move calculation method. Current Position: " + this.transform.position + ", Delta Movement: " + deltaMovement + ", New Position: " + (this.transform.position + deltaMovement));

    MoveCalculationResult moveCalculationResult = new MoveCalculationResult();
    moveCalculationResult.collisionState = new CharacterCollisionState2D();
    moveCalculationResult.collisionState.characterWallState = CharacterWallState.NotOnWall;

    // save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame
    moveCalculationResult.collisionState.wasGroundedLastFrame = lastMoveCalculationResult.collisionState.below;
    moveCalculationResult.collisionState.lastTimeGrounded = lastMoveCalculationResult.collisionState.lastTimeGrounded;
    moveCalculationResult.deltaMovement = deltaMovement;
    moveCalculationResult.originalDeltaMovement = deltaMovement;

    // clear our state
    moveCalculationResult.collisionState.reset();
    _raycastHitsThisFrame.Clear();

    var desiredPosition = transform.position + moveCalculationResult.deltaMovement;
    PrimeRaycastOrigins(desiredPosition, moveCalculationResult.deltaMovement);

    // first, we check for a slope below us before moving
    // only check slopes if we are going down and grounded
    if (moveCalculationResult.deltaMovement.y < 0 && moveCalculationResult.collisionState.wasGroundedLastFrame)
    {
      HandleVerticalSlope(ref moveCalculationResult);
      Logger.Trace(TRACE_TAG, "After handleVerticalSlope. Delta Movement: " + moveCalculationResult.deltaMovement + ", New Position: " + (this.transform.position + moveCalculationResult.deltaMovement));
    }
    else
    {
      Logger.Trace(TRACE_TAG, "HandleVerticalSlope method not called.");
    }

    // now we check movement in the horizontal dir
    if (moveCalculationResult.deltaMovement.x != 0f)
    {
      MoveHorizontally(ref moveCalculationResult);
      Logger.Trace(TRACE_TAG, "After moveHorizontally. Delta Movement: " + moveCalculationResult.deltaMovement + ", New Position: " + (this.transform.position + moveCalculationResult.deltaMovement));
    }
    else
    {
      moveCalculationResult.collisionState.characterWallState = GetOnWallState();
      Logger.Trace(TRACE_TAG, "MoveHorizontally method not called.");
    }

    // next, check movement in the vertical dir
    if (moveCalculationResult.deltaMovement.y != 0)
    {
      if (moveCalculationResult.isGoingUpSlope)
      {
        MoveVerticallyOnSlope(ref moveCalculationResult);
      }
      else
      {
        MoveVertically(ref moveCalculationResult);
      }

      Logger.Trace(TRACE_TAG, "After moveVertically. Delta Movement: " + moveCalculationResult.deltaMovement + ", New Position: " + (this.transform.position + moveCalculationResult.deltaMovement));
    }
    else
    {
      Logger.Trace(TRACE_TAG, "MoveVertically method not called.");
    }

    if (performFullyGroundedChecks && moveCalculationResult.deltaMovement.y <= K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR)
      moveCalculationResult.collisionState.isFullyGrounded = IsFullyGrounded(moveCalculationResult);

    return moveCalculationResult;
  }

  public void PerformMove(MoveCalculationResult moveCalculationResult)
  {
    transform.Translate(moveCalculationResult.deltaMovement, Space.World);

    // only calculate velocity if we have a non-zero deltaTime
    if (Time.deltaTime > 0)
    {
      float dx = moveCalculationResult.deltaMovement.x;
      float dy = moveCalculationResult.deltaMovement.y;

      if (moveCalculationResult.collisionState.left && moveCalculationResult.originalDeltaMovement.x < moveCalculationResult.deltaMovement.x)
        dx = 0f;
      if (moveCalculationResult.collisionState.right && moveCalculationResult.originalDeltaMovement.x > moveCalculationResult.deltaMovement.x)
        dx = 0f;

      velocity = new Vector2(dx, dy) / Time.deltaTime;
    }

    // set our becameGrounded state based on the previous and current collision state
    if (!moveCalculationResult.collisionState.wasGroundedLastFrame && moveCalculationResult.collisionState.below)
    {
      moveCalculationResult.collisionState.becameGroundedThisFrame = true;

      if (onControllerBecameGrounded != null)
      {
        HashSet<GameObject> rayhitGameObjects = new HashSet<GameObject>();
        for (var i = 0; i < _raycastHitsThisFrame.Count; i++)
        {
          if (!rayhitGameObjects.Contains(_raycastHitsThisFrame[i].collider.gameObject))
          {
            onControllerBecameGrounded(_raycastHitsThisFrame[i].collider.gameObject);
            rayhitGameObjects.Add(_raycastHitsThisFrame[i].collider.gameObject);
          }
        }
      }
    }

    if (moveCalculationResult.collisionState.wasGroundedLastFrame && !moveCalculationResult.collisionState.below)
    {
      if (onControllerLostGround != null)
      {
        onControllerLostGround();
      }
    }

    // if we are going up a slope we artificially set a y velocity so we need to zero it out here
    if (moveCalculationResult.isGoingUpSlope)
      velocity.y = 0;

    // send off the collision events if we have a listener
    if (onControllerCollidedEvent != null)
    {
      for (var i = 0; i < _raycastHitsThisFrame.Count; i++)
        onControllerCollidedEvent(_raycastHitsThisFrame[i]);
    }

    lastMoveCalculationResult = moveCalculationResult;
    lastRaycastHits = new List<RaycastHit2D>(_raycastHitsThisFrame);

    Logger.Trace(TRACE_TAG, "Collision state:" + lastMoveCalculationResult.collisionState.ToString());
  }

  /// <summary>
  /// attempts to move the character to position + deltaMovement. Any colliders in the way will cause the movement to
  /// stop when run into.
  /// </summary>
  /// <param name="deltaMovement">Delta movement.</param>
  public void Move(Vector3 deltaMovement)
  {
    PerformMove(CalculateMove(deltaMovement));
  }

  /// <summary>
  /// moves directly down until grounded
  /// </summary>
  public void WarpToGrounded()
  {
    do
    {
      Move(new Vector3(0, -1f, 0));
    } while (!lastMoveCalculationResult.collisionState.below);
  }

  /// <summary>
  /// this should be called anytime you have to modify the BoxCollider2D at runtime. It will recalculate the distance between the rays used for collision detection.
  /// It is also used in the skinWidth setter in case it is changed at runtime.
  /// </summary>
  public void RecalculateDistanceBetweenRays()
  {
    // figure out the distance between our rays in both directions
    // horizontal
    var colliderUseableHeight = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2f * _skinWidth);
    _verticalDistanceBetweenRays = colliderUseableHeight / (totalHorizontalRays - 1);

    // vertical
    var colliderUseableWidth = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2f * _skinWidth);
    _horizontalDistanceBetweenRays = colliderUseableWidth / (totalVerticalRays - 1);
  }

  public void Reset(Vector3 position)
  {
    lastMoveCalculationResult = new MoveCalculationResult();
    velocity = Vector3.zero;
    this.transform.position = position;
    _raycastHitsThisFrame.Clear();
  }

  public bool IsFloorWithinDistance(float rayDistance)
  {
    rayDistance += _skinWidth;
    for (var i = 0; i < totalVerticalRays; i++)
    {
      if (Physics2D.Raycast(
        new Vector2(_raycastOrigins.bottomLeft.x + i * _horizontalDistanceBetweenRays, _raycastOrigins.bottomLeft.y)
        , -Vector2.up
        , rayDistance
        , platformMask))
        return true;
    }

    return false;
  }

  #endregion

  #region private
  [System.Diagnostics.Conditional("DEBUG")]
  private void DrawRay(Vector3 start, Vector3 dir, Color color)
  {
    Debug.DrawRay(start, dir, color);
  }

  /// <summary>
  /// resets the raycastOrigins to the current extents of the box collider inset by the skinWidth. It is inset
  /// to avoid casting a ray from a position directly touching another collider which results in wonky normal data.
  /// </summary>
  /// <param name="futurePosition">Future position.</param>
  /// <param name="deltaMovement">Delta movement.</param>
  private void PrimeRaycastOrigins(Vector3 futurePosition, Vector3 deltaMovement)
  {
    // our raycasts need to be fired from the bounds inset by the skinWidth
    var modifiedBounds = boxCollider.bounds;
    modifiedBounds.Expand(-2f * _skinWidth);

    _raycastOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
    _raycastOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
    _raycastOrigins.bottomLeft = modifiedBounds.min;
  }

  private bool IsFullyGrounded(MoveCalculationResult moveCalculationResult)
  {
    var rayDirection = -Vector2.up;
    var rayDistance = Mathf.Abs(moveCalculationResult.deltaMovement.y) + _skinWidth + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR;

    // Note: we add delta movement to simulate the new position.
    RaycastHit2D rch1 = Physics2D.Raycast(new Vector3(_raycastOrigins.bottomLeft.x, _raycastOrigins.bottomLeft.y) + moveCalculationResult.deltaMovement, rayDirection, rayDistance, platformMask);
    RaycastHit2D rch2 = Physics2D.Raycast(new Vector3(_raycastOrigins.bottomRight.x, _raycastOrigins.bottomRight.y) + moveCalculationResult.deltaMovement, rayDirection, rayDistance, platformMask);

    return rch1 && rch2;
  }

  private CharacterWallState GetOnWallState()
  {
    CharacterWallState characterWallState = CharacterWallState.NotOnWall;
    var rayDistance = _skinWidth + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR;

    // check left
    if (
           Physics2D.Raycast(new Vector2(_raycastOrigins.bottomLeft.x, _raycastOrigins.bottomLeft.y), -Vector2.right, rayDistance, _platformMaskWithoutOneWay)
        && Physics2D.Raycast(new Vector2(_raycastOrigins.bottomLeft.x, _raycastOrigins.topLeft.y), -Vector2.right, rayDistance, _platformMaskWithoutOneWay)
      )
    {
      characterWallState &= ~CharacterWallState.NotOnWall;
      characterWallState |= CharacterWallState.OnLeftWall;
    }

    // check right
    if (
           Physics2D.Raycast(new Vector2(_raycastOrigins.bottomRight.x, _raycastOrigins.bottomRight.y), Vector2.right, rayDistance, _platformMaskWithoutOneWay)
        && Physics2D.Raycast(new Vector2(_raycastOrigins.bottomRight.x, _raycastOrigins.topLeft.y), Vector2.right, rayDistance, _platformMaskWithoutOneWay)
      )
    {
      characterWallState &= ~CharacterWallState.NotOnWall;
      characterWallState |= CharacterWallState.OnRightWall;
    }

    return characterWallState;
  }

  /// <summary>
  /// we have to use a bit of trickery in this one. The rays must be cast from a small distance inside of our
  /// collider (skinWidth) to avoid zero distance rays which will get the wrong normal. Because of this small offset
  /// we have to increase the ray distance skinWidth then remember to remove skinWidth from deltaMovement before
  /// actually moving the player
  /// </summary>
  private void MoveHorizontally(ref MoveCalculationResult moveCalculationResult)
  {
    var isGoingRight = moveCalculationResult.deltaMovement.x > 0;
    var rayDistance = Mathf.Abs(moveCalculationResult.deltaMovement.x) + _skinWidth;
    var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
    var initialRayOrigin = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;

    RaycastHit2D raycastHit;
    for (var i = 0; i < totalHorizontalRays; i++)
    {
      var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays);

      DrawRay(ray, rayDirection * rayDistance, Color.red);

      Logger.Trace(TRACE_TAG, "moveHorizontally -> test ray origin: {0}, ray target position: {1}, delta: {2}, delta target position: {3}"
        , ray
        , new Vector2(rayDirection.x * rayDistance, rayDirection.y * rayDistance)
        , moveCalculationResult.deltaMovement
        , this.transform.position + moveCalculationResult.deltaMovement);

      // if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one). this will allow us to
      // walk up sloped oneWayPlatforms
      if (i == 0 && moveCalculationResult.collisionState.wasGroundedLastFrame)
        raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);
      else
        raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, _platformMaskWithoutOneWay);

      if (raycastHit)
      {
        if (i == 0)
        {
          Logger.Trace(TRACE_TAG, "moveHorizontally -> Raycast hit on first ray, initiate slope test...");
          // the bottom ray can hit slopes but no other ray can so we have special handling for those cases
          // Note (Roman): I'm passing in the current raycast hit point as reference point for the slope raycasts
          if (HandleHorizontalSlope(ref moveCalculationResult, Vector2.Angle(raycastHit.normal, Vector2.up), raycastHit.point))
          {
            _raycastHitsThisFrame.Add(raycastHit);
            Logger.Trace(TRACE_TAG, "moveHorizontally -> We are on horizontal slope.");
            break;
          }
          else
          {
            Logger.Trace(TRACE_TAG, "moveHorizontally -> We are not on horizontal slope.");
          }
        }

        // set our new deltaMovement and recalculate the rayDistance taking it into account
        moveCalculationResult.deltaMovement.x = raycastHit.point.x - ray.x;
        rayDistance = Mathf.Abs(moveCalculationResult.deltaMovement.x);

        // remember to remove the skinWidth from our deltaMovement
        if (isGoingRight)
        {
          moveCalculationResult.deltaMovement.x -= _skinWidth;
          moveCalculationResult.collisionState.right = true;
        }
        else
        {
          moveCalculationResult.deltaMovement.x += _skinWidth;
          moveCalculationResult.collisionState.left = true;
        }

        Logger.Trace(TRACE_TAG, "moveHorizontally -> hit; Hit Point: {0}, Target Delta: {1}, Target Position: {2}"
          , raycastHit.point
          , moveCalculationResult.deltaMovement
          , (this.transform.position + moveCalculationResult.deltaMovement));

        _raycastHitsThisFrame.Add(raycastHit);

        // we add a small fudge factor for the float operations here. if our rayDistance is smaller
        // than the width + fudge bail out because we have a direct impact
        if (rayDistance < _skinWidth + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR)
        {
          if (i == 0 && Mathf.RoundToInt(Vector2.Angle(raycastHit.normal, Vector2.up)) == 90)
          {
            // if the first ray was a direct hit, we also check the last ray to find out whether the character touches
            // a wall...
            ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + (totalHorizontalRays - 1) * _verticalDistanceBetweenRays);

            Logger.Trace(TRACE_TAG, "moveHorizontally -> first ray hit wall, check whether second ray hits too: ray {0}, ray target position: {1}, delta: {2}, delta target position: {3}"
              , ray
              , new Vector2(rayDirection.x * rayDistance, rayDirection.y * rayDistance)
              , moveCalculationResult.deltaMovement
              , this.transform.position + moveCalculationResult.deltaMovement);

            if (i == 0 && moveCalculationResult.collisionState.wasGroundedLastFrame)
              raycastHit = Physics2D.Raycast(ray, rayDirection, _skinWidth + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR, platformMask);
            else
              raycastHit = Physics2D.Raycast(ray, rayDirection, _skinWidth + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR, _platformMaskWithoutOneWay);

            if (raycastHit && Mathf.RoundToInt(Vector2.Angle(raycastHit.normal, Vector2.up)) == 90)
            {
              moveCalculationResult.collisionState.characterWallState &= ~CharacterWallState.NotOnWall;
              moveCalculationResult.collisionState.characterWallState |= (isGoingRight ? CharacterWallState.OnRightWall : CharacterWallState.OnLeftWall);
            }
          }

          break;
        }
      }
    }
  }

  /// <summary>
  /// handles adjusting deltaMovement if we are going up a slope.
  /// </summary>
  /// <returns><c>true</c>, if horizontal slope was handled, <c>false</c> otherwise.</returns>
  /// <param name="deltaMovement">Delta movement.</param>
  /// <param name="angle">Angle.</param>
  private bool HandleHorizontalSlope(ref MoveCalculationResult moveCalculationResult, float angle, Vector2 horizontalRaycastHit)
  {
    // disregard 90 degree angles (walls)
    if (Mathf.RoundToInt(angle) == 90)
    {
      return false;
    }

    // if we can walk on slopes and our angle is small enough we need to move up
    if (angle < slopeLimit)
    {
      // we only need to adjust the deltaMovement if we are not jumping
      // TODO: this uses a magic number which isn't ideal!
      if (moveCalculationResult.deltaMovement.y < jumpingThreshold)
      {
        // apply the slopeModifier to slow our movement up the slope
        var slopeModifier = (slopeSpeedMultiplierOverride == null ? slopeSpeedMultiplier : slopeSpeedMultiplierOverride).Evaluate(angle);

        Vector2 rayOrigin = moveCalculationResult.deltaMovement.x > 0
          ? new Vector2(horizontalRaycastHit.x - .1f, horizontalRaycastHit.y)
          : new Vector2(horizontalRaycastHit.x + .1f, horizontalRaycastHit.y); // Note (Roman): added/subtracted .1f to make sure we stay on the same side
        Vector2 currentdelta = Vector2.zero;
        Vector2 targetDelta = new Vector2();

        // we dont set collisions on the sides for this since a slope is not technically a side collision

        // smooth y movement when we climb. we make the y movement equivalent to the actual y location that corresponds
        // to our new x location using our good friend Pythagoras
        float targetMoveX = moveCalculationResult.deltaMovement.x * slopeModifier;
        float targetMoveMultiplier = targetMoveX >= 0 ? -1f : 1f;

        targetDelta.x = targetMoveX;
        targetDelta.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * targetDelta.x);

        RaycastHit2D raycastHit;

        do
        {
          // check whether we go through a wall, if so adjust...
          Logger.Trace(TRACE_TAG, "handleHorizontalSlope -> Raycast test; Current Position: {0}, Target Delta: {1}, Target Position: {2}, Current Delta: {3}, Target Move X: {4}, angle: {5}"
            , rayOrigin
            , targetDelta
            , (rayOrigin + targetDelta)
            , currentdelta
            , targetMoveX
            , angle);

          if (moveCalculationResult.collisionState.wasGroundedLastFrame)
            raycastHit = Physics2D.Raycast(rayOrigin, targetDelta.normalized, targetDelta.magnitude, platformMask);
          else
            raycastHit = Physics2D.Raycast(rayOrigin, targetDelta.normalized, targetDelta.magnitude, _platformMaskWithoutOneWay);

          if (raycastHit)
          {//we crossed an edge when using Pythagoras calculation, so we set the actual delta movement to the ray hit location
            Vector2 raycastHitVector = (raycastHit.point - rayOrigin);

            currentdelta += raycastHitVector;
            targetMoveX = targetMoveX + Mathf.Abs(currentdelta.x) * targetMoveMultiplier;

            Logger.Trace(TRACE_TAG, "handleHorizontalSlope -> hit; Hit Point: {5}, Current Position: {0}, Target Delta: {1}, Target Position: {2}, Current Delta: {3}, Target Move X: {4}"
              , rayOrigin
              , targetDelta
              , (rayOrigin + targetDelta)
              , currentdelta
              , targetMoveX
              , raycastHit.point);

            // we have adjusted the delta, now do the same thing again...
            angle = Vector2.Angle(raycastHit.normal, Vector2.up);
            if (angle < slopeLimit)
            {
              rayOrigin = rayOrigin + currentdelta;

              targetDelta.x = targetMoveX;
              targetDelta.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * targetDelta.x);
            }
            else
            {
              Logger.Trace(TRACE_TAG, "handleHorizontalSlope -> slope limit exceeded after hit.");
              break; // exit here
            }
          }
          else
          {
            currentdelta += targetDelta;

            Logger.Trace(TRACE_TAG, "handleHorizontalSlope -> no hit; final delta movement: {0}, final new position: {1}"
              , currentdelta
              , this.transform.position + new Vector3(currentdelta.x, currentdelta.y));
          }
        }
        while (raycastHit);

        moveCalculationResult.deltaMovement.y = currentdelta.y;
        moveCalculationResult.deltaMovement.x = currentdelta.x;

        moveCalculationResult.isGoingUpSlope = true;

        moveCalculationResult.collisionState.below = true;
        moveCalculationResult.collisionState.lastTimeGrounded = Time.time;
      }
      else
      {
        Logger.Trace(TRACE_TAG, "handleHorizontalSlope -> Jump threshold exceeded: deltaMovement.y >= slopeLimit [{0} >= {1}]", moveCalculationResult.deltaMovement.y, jumpingThreshold);
      }
    }
    else // too steep. get out of here
    {
      Logger.Trace(TRACE_TAG, "handleHorizontalSlope -> slope limit exceeded.");
      moveCalculationResult.deltaMovement.x = 0;
    }

    return true;
  }

  private void MoveVerticallyOnSlope(ref MoveCalculationResult moveCalculationResult)
  {
    Logger.Trace(TRACE_TAG, "moveVerticallyOnSlope -> start vert move check");

    var rayDistance = moveCalculationResult.deltaMovement.magnitude + _skinWidth;
    Vector3 rayDirection = moveCalculationResult.deltaMovement.normalized;

    var initialRayOrigin = _raycastOrigins.topLeft;

    // if we are moving up, we should ignore the layers in oneWayPlatformMask
    var mask = moveCalculationResult.collisionState.wasGroundedLastFrame ? platformMask : _platformMaskWithoutOneWay;

    RaycastHit2D raycastHit;
    for (var i = 0; i < totalVerticalRays; i++)
    {
      var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

      DrawRay(ray, rayDirection * rayDistance, Color.red);

      raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);

      Logger.Trace(TRACE_TAG, "moveVerticallyOnSlope -> test ray origin: {0}, ray target position: {1}, delta: {2}, delta target position: {3}"
        , ray
        , new Vector2(rayDirection.x * rayDistance, rayDirection.y * rayDistance)
        , moveCalculationResult.deltaMovement
        , this.transform.position + moveCalculationResult.deltaMovement);

      if (raycastHit)
      {
        // set our new deltaMovement and recalculate the rayDistance taking it into account
        moveCalculationResult.deltaMovement = raycastHit.point - ray;

        Logger.Trace(TRACE_TAG, "moveVerticallyOnSlope -> ray hit; hit point: {0}, new delta: {1}, new delta target position: {2}"
          , raycastHit.point
          , moveCalculationResult.deltaMovement
          , this.transform.position + moveCalculationResult.deltaMovement);

        rayDistance = moveCalculationResult.deltaMovement.magnitude;
        // remember to remove the skinWidth from our deltaMovement

        if (moveCalculationResult.deltaMovement.x > 0)
        {
          moveCalculationResult.deltaMovement -= new Vector3(rayDirection.x * _skinWidth, rayDirection.y * _skinWidth, 0f);
        }
        else
        {
          moveCalculationResult.deltaMovement += new Vector3(rayDirection.x * _skinWidth, rayDirection.y * _skinWidth, 0f);
        }

        moveCalculationResult.collisionState.above = true;

        _raycastHitsThisFrame.Add(raycastHit);

        // we add a small fudge factor for the float operations here. if our rayDistance is smaller
        // than the width + fudge bail out because we have a direct impact
        if (rayDistance < _skinWidth + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR)
          break;
      }
    }
  }

  struct TopEdgeCollisionTestContainer
  {
    public RaycastHit2D raycastHit2D;
    public bool isWithinLeftAdjustmentArea;
    public bool isWithinRightAdjustmentArea;
    public float insetLeft;

    public override string ToString()
    {
      return string.Format("raycastHit2D: {0}, insetLeft: {1}, isWithinLeftAdjustmentArea: {2}, isWithinRightAdjustmentArea: {3}"
        , raycastHit2D == true ? "true" : "false"
        , insetLeft
        , isWithinLeftAdjustmentArea
        , isWithinRightAdjustmentArea);
    }
  }

  private TopEdgeCollisionTestContainer[] _topEdgeCollisionTestContainers;// = new TopEdgeCollisionTestContainer[];

  private void MoveVertically(ref MoveCalculationResult moveCalculationResult)
  {
    Logger.Trace(TRACE_TAG, "moveVertically -> start vert move check");

    var isGoingUp = moveCalculationResult.deltaMovement.y > 0;
    var rayDistance = Mathf.Abs(moveCalculationResult.deltaMovement.y) + _skinWidth;
    var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;

    // if we are moving up, we should ignore the layers in oneWayPlatformMask
    var mask = isGoingUp ? _platformMaskWithoutOneWay : platformMask;

    if (isGoingUp && enableEdgeSlideUpHelp)
    {
      #region going up && enableEdgeSlideUpHelp
      Logger.Trace(TRACE_TAG, "moveVertically -> going up and top edge collision adjustment enabled");

      // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
      float leftOriginX = _raycastOrigins.topLeft.x + moveCalculationResult.deltaMovement.x;
      float rightOriginX = _raycastOrigins.bottomRight.x + moveCalculationResult.deltaMovement.x;
      float topOriginY = _raycastOrigins.topLeft.y;
      float leftAdjustmentBoundaryPosition = leftOriginX - _skinWidth + edgeSlideUpCornerWidth;
      float rightAdjustmentBoundaryPosition = rightOriginX + _skinWidth - edgeSlideUpCornerWidth;

      bool hasHit = false;
      bool hasHitOutsideAdjustmentBoundaries = false;
      bool hasHitWithinLeftAdjustmentBoundaries = false;
      bool hasHitWithinRightAdjustmentBoundaries = false;

      for (var i = 0; i < totalVerticalRays; i++)
      {
        _topEdgeCollisionTestContainers[i].insetLeft = leftOriginX + i * _horizontalDistanceBetweenRays;
        _topEdgeCollisionTestContainers[i].isWithinLeftAdjustmentArea = _topEdgeCollisionTestContainers[i].insetLeft <= leftAdjustmentBoundaryPosition;
        _topEdgeCollisionTestContainers[i].isWithinRightAdjustmentArea = _topEdgeCollisionTestContainers[i].insetLeft >= rightAdjustmentBoundaryPosition;

        Vector2 ray = new Vector2(_topEdgeCollisionTestContainers[i].insetLeft, topOriginY);

        _topEdgeCollisionTestContainers[i].raycastHit2D = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);

        Logger.Trace(TRACE_TAG, "moveVertically -> test ray origin: {0}, ray target position: {1}, delta: {2}, delta target position: {3}"
          , ray
          , new Vector2(rayDirection.x * rayDistance, rayDirection.y * rayDistance)
          , moveCalculationResult.deltaMovement
          , this.transform.position + moveCalculationResult.deltaMovement);

        if (_topEdgeCollisionTestContainers[i].raycastHit2D)
        {
          hasHit = true;

          if (!hasHitOutsideAdjustmentBoundaries
            && !_topEdgeCollisionTestContainers[i].isWithinLeftAdjustmentArea
            && !_topEdgeCollisionTestContainers[i].isWithinRightAdjustmentArea)
          {
            hasHitOutsideAdjustmentBoundaries = true;
          }
          if (!hasHitWithinLeftAdjustmentBoundaries
            && _topEdgeCollisionTestContainers[i].isWithinLeftAdjustmentArea)
          {
            hasHitWithinLeftAdjustmentBoundaries = true;
          }
          if (!hasHitWithinRightAdjustmentBoundaries
            && _topEdgeCollisionTestContainers[i].isWithinRightAdjustmentArea)
          {
            hasHitWithinRightAdjustmentBoundaries = true;
          }

          Logger.Trace(TRACE_TAG, "moveVertically -> ray hit; hit point: {0}, new delta: {1}, new delta target position: {2}"
            , _topEdgeCollisionTestContainers[i].raycastHit2D.point
            , moveCalculationResult.deltaMovement
            , this.transform.position + moveCalculationResult.deltaMovement);

          DrawRay(ray, rayDirection * rayDistance, Color.red);

          float deltaMovementY = _topEdgeCollisionTestContainers[i].raycastHit2D.point.y - ray.y;
          if (!_topEdgeCollisionTestContainers[i].isWithinLeftAdjustmentArea
            && !_topEdgeCollisionTestContainers[i].isWithinRightAdjustmentArea
            && Mathf.Abs(deltaMovementY) < _skinWidth + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR)
          {
            // direct hit within outside the adjustment area -> no adjustments possible
            moveCalculationResult.deltaMovement.y = deltaMovementY;

            moveCalculationResult.deltaMovement.y -= _skinWidth;
            moveCalculationResult.collisionState.above = true;

            _raycastHitsThisFrame.Add(_topEdgeCollisionTestContainers[i].raycastHit2D);

            return; // we exit
          }
        }
        else
        {
          DrawRay(ray, rayDirection * rayDistance, Color.blue);
        }
      }


      if (hasHit)
      {
        Logger.Trace(TRACE_TAG, string.Format("hasHit: {0}, hasHitOutsideAdjustmentBoundaries: {1}, hasHitWithinLeftAdjustmentBoundaries: {2}, hasHitWithinRightAdjustmentBoundaries: {3}"
          , hasHit
          , hasHitOutsideAdjustmentBoundaries
          , hasHitWithinLeftAdjustmentBoundaries
          , hasHitWithinRightAdjustmentBoundaries));

        Logger.Trace(TRACE_TAG, string.Format("leftOriginX: {0}, leftAdjustmentBoundaryPosition: {1}, rightOriginX: {2}, rightAdjustmentBoundaryPosition: {3}, topOriginY: {4}"
          , leftOriginX
          , leftAdjustmentBoundaryPosition
          , rightOriginX
          , rightAdjustmentBoundaryPosition
          , topOriginY));

        if (!hasHitOutsideAdjustmentBoundaries)
        {
          RaycastHit2D leftAdjustmentRaycastHit = new RaycastHit2D();

          // first left
          Vector2 leftAdjustmentRay = new Vector2(leftAdjustmentBoundaryPosition, topOriginY);
          DrawRay(leftAdjustmentRay, rayDirection * rayDistance, Color.yellow);

          leftAdjustmentRaycastHit = Physics2D.Raycast(leftAdjustmentRay, rayDirection, rayDistance, mask);

          if (!leftAdjustmentRaycastHit
            && !hasHitWithinRightAdjustmentBoundaries)
          {
            Logger.Trace(TRACE_TAG, "moveVertically -> Vert Ray Hit. isGoingUp: {0}, deltaMovement.y: {1}", isGoingUp, moveCalculationResult.deltaMovement.y);

            Vector2 raytest = new Vector2(leftOriginX - skinWidth + edgeSlideUpCornerWidth, topOriginY + skinWidth + moveCalculationResult.deltaMovement.y);
            RaycastHit2D rch = Physics2D.Raycast(raytest, -Vector2.right, edgeSlideUpCornerWidth, mask);

            DrawRay(raytest, -Vector2.right * edgeSlideUpCornerWidth, Color.magenta);
            Logger.Assert(rch == true, "This should always hit! raytest: " + raytest + "; direction: " + (-Vector2.right) + "; magnitude: " + edgeSlideUpCornerWidth);

            transform.Translate(edgeSlideUpCornerWidth - rch.distance + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR, 0f, 0f, Space.World);
            return;
          }

          // we reached this line, so do right
          RaycastHit2D rightAdjustmentRaycastHit = new RaycastHit2D();
          Vector2 rightAdjustmentRay = new Vector2(rightAdjustmentBoundaryPosition, topOriginY);
          DrawRay(rightAdjustmentRay, rayDirection * rayDistance, Color.yellow);
          rightAdjustmentRaycastHit = Physics2D.Raycast(rightAdjustmentRay, rayDirection, rayDistance, mask);

          if (!rightAdjustmentRaycastHit
            && !hasHitWithinLeftAdjustmentBoundaries)
          {
            Logger.Trace(TRACE_TAG, "moveVertically -> Vert Ray Hit. isGoingUp: {0}, deltaMovement.y: {1}", isGoingUp, moveCalculationResult.deltaMovement.y);

            Vector2 raytest = new Vector2(rightOriginX + skinWidth - edgeSlideUpCornerWidth, topOriginY + skinWidth + moveCalculationResult.deltaMovement.y);
            RaycastHit2D rch = Physics2D.Raycast(raytest, Vector2.right, edgeSlideUpCornerWidth, mask);

            DrawRay(raytest, -Vector2.right * edgeSlideUpCornerWidth, Color.magenta);
            Logger.Assert(rch == true, "This should always hit! raytest: " + raytest + "; direction: " + (Vector2.right) + "; magnitude: " + edgeSlideUpCornerWidth);

            transform.Translate(-(edgeSlideUpCornerWidth - rch.distance + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR), 0f, 0f, Space.World);
            return;
          }
        }

        // we reached this line, do normal vert ray checks
        for (int i = 0; i < totalVerticalRays; i++)
        {
          Logger.Trace(TRACE_TAG, _topEdgeCollisionTestContainers[i]);

          if (_topEdgeCollisionTestContainers[i].raycastHit2D)
          {
            Logger.Trace(TRACE_TAG, "moveVertically -> Vert Ray Hit. isGoingUp: {0}, deltaMovement.y: {1}", isGoingUp, moveCalculationResult.deltaMovement.y);

            // set our new deltaMovement and recalculate the rayDistance taking it into account
            moveCalculationResult.deltaMovement.y = _topEdgeCollisionTestContainers[i].raycastHit2D.point.y - topOriginY;

            Logger.Trace(TRACE_TAG, "moveVertically -> ray hit; hit point: {0}, new delta: {1}, new delta target position: {2}"
              , _topEdgeCollisionTestContainers[i].raycastHit2D.point
              , moveCalculationResult.deltaMovement
              , this.transform.position + moveCalculationResult.deltaMovement);

            rayDistance = Mathf.Abs(moveCalculationResult.deltaMovement.y);

            // remember to remove the skinWidth from our deltaMovement
            moveCalculationResult.deltaMovement.y -= _skinWidth;
            moveCalculationResult.collisionState.above = true;

            _raycastHitsThisFrame.Add(_topEdgeCollisionTestContainers[i].raycastHit2D);

            // we add a small fudge factor for the float operations here. if our rayDistance is smaller
            // than the width + fudge bail out because we have a direct impact
            if (rayDistance < _skinWidth + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR)
            {
              break;
            }
          }
        }
      }
      #endregion
    }
    else
    {
      var initialRayOrigin = isGoingUp ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;
      // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
      initialRayOrigin.x += moveCalculationResult.deltaMovement.x;

      RaycastHit2D raycastHit;
      for (var i = 0; i < totalVerticalRays; i++)
      {
        var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

        DrawRay(ray, rayDirection * rayDistance, Color.red);
        raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);

        Logger.Trace(TRACE_TAG, "moveVertically -> test ray origin: {0}, ray target position: {1}, delta: {2}, delta target position: {3}"
          , ray
          , new Vector2(rayDirection.x * rayDistance, rayDirection.y * rayDistance)
          , moveCalculationResult.deltaMovement
          , this.transform.position + moveCalculationResult.deltaMovement);

        if (raycastHit)
        {
          Logger.Trace(TRACE_TAG, "moveVertically -> Vert Ray Hit. isGoingUp: {0}, deltaMovement.y: {1}", isGoingUp, moveCalculationResult.deltaMovement.y);
          // set our new deltaMovement and recalculate the rayDistance taking it into account
          moveCalculationResult.deltaMovement.y = raycastHit.point.y - ray.y;
          Logger.Trace(TRACE_TAG, "moveVertically -> ray hit; hit point: {0}, new delta: {1}, new delta target position: {2}"
            , raycastHit.point
            , moveCalculationResult.deltaMovement
            , this.transform.position + moveCalculationResult.deltaMovement);

          rayDistance = Mathf.Abs(moveCalculationResult.deltaMovement.y);

          // remember to remove the skinWidth from our deltaMovement
          if (isGoingUp)
          {
            moveCalculationResult.deltaMovement.y -= _skinWidth;
            moveCalculationResult.collisionState.above = true;
          }
          else
          {
            moveCalculationResult.deltaMovement.y += _skinWidth;
            moveCalculationResult.collisionState.below = true;
            moveCalculationResult.collisionState.lastTimeGrounded = Time.time;
          }

          _raycastHitsThisFrame.Add(raycastHit);

          // this is a hack to deal with the top of slopes. if we walk up a slope and reach the apex we can get in a situation
          // where our ray gets a hit that is less then skinWidth causing us to be ungrounded the next frame due to residual velocity.
          if (!isGoingUp && moveCalculationResult.deltaMovement.y > 0.00001f)
            moveCalculationResult.isGoingUpSlope = true;

          // we add a small fudge factor for the float operations here. if our rayDistance is smaller
          // than the width + fudge bail out because we have a direct impact
          if (rayDistance < _skinWidth + K_SKIN_WIDTH_FLOAT_FUDGE_FACTOR)
          {
            break;
          }
        }
      }
    }
  }

  /// <summary>
  /// checks the center point under the BoxCollider2D for a slope. If it finds one then the deltaMovement is adjusted so that
  /// the player stays grounded and the slopeSpeedModifier is taken into account to speed up movement.
  /// </summary>
  /// <param name="deltaMovement">Delta movement.</param>
  private void HandleVerticalSlope(ref MoveCalculationResult moveCalculationResult)
  {
    // slope check from the center of our collider
    var centerOfCollider = (_raycastOrigins.bottomLeft.x + _raycastOrigins.bottomRight.x) * 0.5f;
    var rayDirection = -Vector2.up;

    // the ray distance is based on our slopeLimit
    var slopeCheckRayDistance = _slopeLimitTangent * (_raycastOrigins.bottomRight.x - centerOfCollider);

    var slopeRay = new Vector2(centerOfCollider, _raycastOrigins.bottomLeft.y);
    DrawRay(slopeRay, rayDirection * slopeCheckRayDistance, Color.yellow);
    RaycastHit2D raycastHit = Physics2D.Raycast(slopeRay, rayDirection, slopeCheckRayDistance, platformMask);
    if (raycastHit)
    {
      // bail out if we have no slope
      var angle = Vector2.Angle(raycastHit.normal, Vector2.up);
      if (angle == 0)
      {
        Logger.Trace(TRACE_TAG, "HandleVerticalSlope -> no slope");
        return;
      }

      // we are moving down the slope if our normal and movement direction are in the same x direction
      var isMovingDownSlope = Mathf.Sign(raycastHit.normal.x) == Mathf.Sign(moveCalculationResult.deltaMovement.x);
      if (isMovingDownSlope)
      {
        // going down we want to speed up in most cases so the slopeSpeedMultiplier curve should be > 1 for negative angles
        var slopeModifier = (slopeSpeedMultiplierOverride == null ? slopeSpeedMultiplier : slopeSpeedMultiplierOverride).Evaluate(-angle);
        Logger.Trace(TRACE_TAG, "HandleVerticalSlope -> moving down, slope modifier: " + slopeModifier + ", angle: " + angle
          + ", slopeSpeedMultiplierOverride == null: " + (slopeSpeedMultiplierOverride == null));
        // we add the extra downward movement here to ensure we "stick" to the surface below
        moveCalculationResult.deltaMovement.y += raycastHit.point.y - slopeRay.y - skinWidth;
        moveCalculationResult.deltaMovement.x *= slopeModifier;
        moveCalculationResult.collisionState.movingDownSlope = true;
        moveCalculationResult.collisionState.slopeAngle = angle;
      }
    }
  }

  #endregion

  #endregion
}

public class BasePhysicsManager : MonoBehaviour
{

}
