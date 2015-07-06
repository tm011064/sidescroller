#define DEBUG_CC2D_RAYS
using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class CharacterPhysicsManager : MonoBehaviour
{
  private const string TRACE_TAG = "CharacterPhysicsManager";

  #region internal types

  private struct CharacterRaycastOrigins
  {
    public Vector3 topLeft;
    public Vector3 bottomRight;
    public Vector3 bottomLeft;
  }

  public class CharacterCollisionState2D
  {
    public bool right;
    public bool left;
    public bool above;
    public bool below;
    public bool becameGroundedThisFrame;
    public bool wasGroundedLastFrame;
    public bool movingDownSlope;
    public float slopeAngle;
    public float lastTimeGrounded;
    public bool isOnWall;


    public bool hasCollision()
    {
      return below || right || left || above;
    }


    public void reset()
    {
      right = left = above = below = becameGroundedThisFrame = movingDownSlope = isOnWall = false;
      slopeAngle = 0f;
    }


    public override string ToString()
    {
      return string.Format("[CharacterCollisionState2D] r: {0}, l: {1}, a: {2}, b: {3}, movingDownSlope: {4}, angle: {5}, wasGroundedLastFrame: {6}, becameGroundedThisFrame: {7}",
                           right, left, above, below, movingDownSlope, slopeAngle, wasGroundedLastFrame, becameGroundedThisFrame);
    }
  }

  #endregion

  #region events, properties and fields
  public event Action<RaycastHit2D> onControllerCollidedEvent;
  public event Action<GameObject> onControllerBecameGrounded;
  public event Action onControllerLostGround;
  public event Action<Collider2D> onTriggerEnterEvent;
  public event Action<Collider2D> onTriggerStayEvent;
  public event Action<Collider2D> onTriggerExitEvent;

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
      recalculateDistanceBetweenRays();
    }
  }


  /// <summary>
  /// mask with all layers that the player should interact with
  /// </summary>
  public LayerMask platformMask = 0;

  /// <summary>
  /// mask with all layers that should act as one-way platforms. Note that one-way platforms should always be EdgeCollider2Ds. This is private because it does not support being
  /// updated anytime outside of the inspector for now.
  /// </summary>
  [SerializeField]
  private LayerMask oneWayPlatformMask = 0;

  /// <summary>
  /// mask with all layers that trigger events should fire when intersected
  /// </summary>
  public LayerMask triggerMask = 0;

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
  public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1.5f), new Keyframe(0, 1), new Keyframe(90, 0));
  //public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 1));

  [Range(2, 20)]
  public int totalHorizontalRays = 8;
  [Range(2, 20)]
  public int totalVerticalRays = 4;


  /// <summary>
  /// this is used to calculate the downward ray that is cast to check for slopes. We use the somewhat arbitrary value 75 degrees
  /// to calculate the length of the ray that checks for slopes.
  /// </summary>
  //private float _slopeLimitTangent = Mathf.Tan(50f * Mathf.Deg2Rad);
  private float _slopeLimitTangent = Mathf.Tan(50f * Mathf.Deg2Rad);

  [Range(0.8f, 0.999f)]
  public float triggerHelperBoxColliderScale = 0.95f;


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
  public CharacterCollisionState2D collisionState = new CharacterCollisionState2D();
  [HideInInspector]
  [NonSerialized]
  public Vector3 velocity;
  public bool isGrounded { get { return collisionState.below; } }
  public float lastTimeGrounded { get { return collisionState.lastTimeGrounded; } }

  private const float kSkinWidthFloatFudgeFactor = 0.001f;

  #endregion


  /// <summary>
  /// holder for our raycast origin corners (TR, TL, BR, BL)
  /// </summary>
  private CharacterRaycastOrigins _raycastOrigins;

  /// <summary>
  /// stores our raycast hit during movement
  /// </summary>
  private RaycastHit2D _raycastHit;

  /// <summary>
  /// stores any raycast hits that occur this frame. we have to store them in case we get a hit moving
  /// horizontally and vertically so that we can send the events after all collision state is set
  /// </summary>
  private List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>(2);

  // horizontal/vertical movement data
  private float _verticalDistanceBetweenRays;
  private float _horizontalDistanceBetweenRays;
  // we use this flag to mark the case where we are travelling up a slope and we modified our delta.y to allow the climb to occur.
  // the reason is so that if we reach the end of the slope we can make an adjustment to stay grounded
  private bool _isGoingUpSlope = false;


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


  [System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
  private void DrawRay(Vector3 start, Vector3 dir, Color color)
  {
    Debug.DrawRay(start, dir, color);
  }


  #region Public

  /// <summary>
  /// attempts to move the character to position + deltaMovement. Any colliders in the way will cause the movement to
  /// stop when run into.
  /// </summary>
  /// <param name="deltaMovement">Delta movement.</param>
  public void move(Vector3 deltaMovement)
  {
    Logger.Trace(TRACE_TAG, "Start move method. Current Position: " + this.transform.position + ", Delta Movement: " + deltaMovement + ", New Position: " + (this.transform.position + deltaMovement));

    // save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame
    collisionState.wasGroundedLastFrame = collisionState.below;

    // clear our state
    collisionState.reset();
    _raycastHitsThisFrame.Clear();
    _isGoingUpSlope = false;

    var desiredPosition = transform.position + deltaMovement;
    primeRaycastOrigins(desiredPosition, deltaMovement);

    // first, we check for a slope below us before moving
    // only check slopes if we are going down and grounded
    if (deltaMovement.y < 0 && collisionState.wasGroundedLastFrame)
      handleVerticalSlope(ref deltaMovement);
    Logger.Trace(TRACE_TAG, "After handleVerticalSlope. Delta Movement: " + deltaMovement + ", New Position: " + (this.transform.position + deltaMovement));

    // now we check movement in the horizontal dir
    if (deltaMovement.x != 0)
      moveHorizontally(ref deltaMovement);
    Logger.Trace(TRACE_TAG, "After moveHorizontally. Delta Movement: " + deltaMovement + ", New Position: " + (this.transform.position + deltaMovement));

    // next, check movement in the vertical dir
    if (deltaMovement.y != 0)
    {
      if (_isGoingUpSlope)
      {
        moveVerticallyOnSlope(ref deltaMovement);
      }
      else
      {
        moveVertically(ref deltaMovement);
      }
    }
    Logger.Trace(TRACE_TAG, "After moveVertically. Delta Movement: " + deltaMovement + ", New Position: " + (this.transform.position + deltaMovement));

    transform.Translate(deltaMovement, Space.World);

    // only calculate velocity if we have a non-zero deltaTime
    if (Time.deltaTime > 0)
      velocity = deltaMovement / Time.deltaTime;

    // set our becameGrounded state based on the previous and current collision state
    if (!collisionState.wasGroundedLastFrame && collisionState.below)
    {
      collisionState.becameGroundedThisFrame = true;

      if (onControllerBecameGrounded != null)
      {
        for (var i = 0; i < _raycastHitsThisFrame.Count; i++)
          onControllerBecameGrounded(_raycastHitsThisFrame[i].collider.gameObject);
      }
    }

    if (collisionState.wasGroundedLastFrame && !collisionState.below)
    {
      if (onControllerLostGround != null)
      {
        onControllerLostGround();
      }
    }

    // if we are going up a slope we artificially set a y velocity so we need to zero it out here
    if (_isGoingUpSlope)
      velocity.y = 0;

    // send off the collision events if we have a listener
    if (onControllerCollidedEvent != null)
    {
      for (var i = 0; i < _raycastHitsThisFrame.Count; i++)
        onControllerCollidedEvent(_raycastHitsThisFrame[i]);
    }

    Logger.Trace(TRACE_TAG, "Collision state:" + collisionState.ToString());
  }

  /// <summary>
  /// moves directly down until grounded
  /// </summary>
  public void warpToGrounded()
  {
    do
    {
      move(new Vector3(0, -1f, 0));
    } while (!isGrounded);
  }


  /// <summary>
  /// this should be called anytime you have to modify the BoxCollider2D at runtime. It will recalculate the distance between the rays used for collision detection.
  /// It is also used in the skinWidth setter in case it is changed at runtime.
  /// </summary>
  public void recalculateDistanceBetweenRays()
  {
    // figure out the distance between our rays in both directions
    // horizontal
    var colliderUseableHeight = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2f * _skinWidth);
    _verticalDistanceBetweenRays = colliderUseableHeight / (totalHorizontalRays - 1);

    // vertical
    var colliderUseableWidth = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2f * _skinWidth);
    _horizontalDistanceBetweenRays = colliderUseableWidth / (totalVerticalRays - 1);
  }

  #endregion


  #region Private Movement Methods

  /// <summary>
  /// resets the raycastOrigins to the current extents of the box collider inset by the skinWidth. It is inset
  /// to avoid casting a ray from a position directly touching another collider which results in wonky normal data.
  /// </summary>
  /// <param name="futurePosition">Future position.</param>
  /// <param name="deltaMovement">Delta movement.</param>
  private void primeRaycastOrigins(Vector3 futurePosition, Vector3 deltaMovement)
  {
    // our raycasts need to be fired from the bounds inset by the skinWidth
    var modifiedBounds = boxCollider.bounds;
    modifiedBounds.Expand(-2f * _skinWidth);

    _raycastOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
    _raycastOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
    _raycastOrigins.bottomLeft = modifiedBounds.min;
  }



  /// <summary>
  /// we have to use a bit of trickery in this one. The rays must be cast from a small distance inside of our
  /// collider (skinWidth) to avoid zero distance rays which will get the wrong normal. Because of this small offset
  /// we have to increase the ray distance skinWidth then remember to remove skinWidth from deltaMovement before
  /// actually moving the player
  /// </summary>
  private void moveHorizontally(ref Vector3 deltaMovement)
  {
    var isGoingRight = deltaMovement.x > 0;
    var rayDistance = Mathf.Abs(deltaMovement.x) + _skinWidth;
    var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
    var initialRayOrigin = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;

    for (var i = 0; i < totalHorizontalRays; i++)
    {
      var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays);

      DrawRay(ray, rayDirection * rayDistance, Color.red);

      Logger.Trace(TRACE_TAG, "moveHorizontally -> test ray origin: {0}, ray target position: {1}, delta: {2}, delta target position: {3}"
        , ray
        , new Vector2(rayDirection.x * rayDistance, rayDirection.y * rayDistance)
        , deltaMovement
        , this.transform.position + deltaMovement);

      // if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one). this will allow us to
      // walk up sloped oneWayPlatforms
      if (i == 0 && collisionState.wasGroundedLastFrame)
        _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);
      else
        _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask & ~oneWayPlatformMask);

      if (_raycastHit)
      {
        if (i == 0)
        {
          Logger.Trace(TRACE_TAG, "moveHorizontally -> Raycast hit on first ray, initiate slope test...");
          // the bottom ray can hit slopes but no other ray can so we have special handling for those cases
          // Note (Roman): I'm passing in the current raycast hit point as reference point for the slope raycasts
          if (handleHorizontalSlope(ref deltaMovement, Vector2.Angle(_raycastHit.normal, Vector2.up), _raycastHit.point))
          {
            _raycastHitsThisFrame.Add(_raycastHit);
            Logger.Trace(TRACE_TAG, "moveHorizontally -> We are on horizontal slope.");
            break;
          }
          else
          {
            Logger.Trace(TRACE_TAG, "moveHorizontally -> We are not on horizontal slope.");
          }
        }

        // set our new deltaMovement and recalculate the rayDistance taking it into account
        deltaMovement.x = _raycastHit.point.x - ray.x;
        rayDistance = Mathf.Abs(deltaMovement.x);

        // remember to remove the skinWidth from our deltaMovement
        if (isGoingRight)
        {
          deltaMovement.x -= _skinWidth;
          collisionState.right = true;
        }
        else
        {
          deltaMovement.x += _skinWidth;
          collisionState.left = true;
        }

        Logger.Trace(TRACE_TAG, "moveHorizontally -> hit; Hit Point: {0}, Target Delta: {1}, Target Position: {2}"
          , _raycastHit.point
          , deltaMovement
          , (this.transform.position + deltaMovement));

        _raycastHitsThisFrame.Add(_raycastHit);

        // we add a small fudge factor for the float operations here. if our rayDistance is smaller
        // than the width + fudge bail out because we have a direct impact
        if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
        {
          if (i == 0 && Mathf.RoundToInt(Vector2.Angle(_raycastHit.normal, Vector2.up)) == 90)
          {
            // if the first ray was a direct hit, we also check the last ray to find out whether the character touches
            // a wall...
            ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + (totalHorizontalRays - 1) * _verticalDistanceBetweenRays);

            Logger.Trace(TRACE_TAG, "moveHorizontally -> first ray hit wall, check whether second ray hits too: ray {0}, ray target position: {1}, delta: {2}, delta target position: {3}"
              , ray
              , new Vector2(rayDirection.x * rayDistance, rayDirection.y * rayDistance)
              , deltaMovement
              , this.transform.position + deltaMovement);

            RaycastHit2D raycastHit; 
            if (i == 0 && collisionState.wasGroundedLastFrame)
              raycastHit = Physics2D.Raycast(ray, rayDirection, _skinWidth + kSkinWidthFloatFudgeFactor, platformMask);
            else
              raycastHit = Physics2D.Raycast(ray, rayDirection, _skinWidth + kSkinWidthFloatFudgeFactor, platformMask & ~oneWayPlatformMask);

            if (raycastHit && Mathf.RoundToInt(Vector2.Angle(_raycastHit.normal, Vector2.up)) == 90)
            {
              collisionState.isOnWall = true;
            }
          }

          break;
        }
      }
    }

    if (!_raycastHit)
    {
      Logger.Trace(TRACE_TAG, "moveHorizontally -> No horizontal ray hit.");
    }
  }


  /// <summary>
  /// handles adjusting deltaMovement if we are going up a slope.
  /// </summary>
  /// <returns><c>true</c>, if horizontal slope was handled, <c>false</c> otherwise.</returns>
  /// <param name="deltaMovement">Delta movement.</param>
  /// <param name="angle">Angle.</param>
  private bool handleHorizontalSlope(ref Vector3 deltaMovement, float angle, Vector2 horizontalRaycastHit)
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
      if (deltaMovement.y < jumpingThreshold)
      {
        // apply the slopeModifier to slow our movement up the slope
        var slopeModifier = slopeSpeedMultiplier.Evaluate(angle);

        Vector2 rayOrigin = deltaMovement.x > 0
          ? new Vector2(horizontalRaycastHit.x - .1f, horizontalRaycastHit.y)
          : new Vector2(horizontalRaycastHit.x + .1f, horizontalRaycastHit.y); // Note (Roman): added/subtracted .1f to make sure we stay on the same side
        Vector2 currentdelta = Vector2.zero;
        Vector2 targetDelta = new Vector2();

        // we dont set collisions on the sides for this since a slope is not technically a side collision

        // smooth y movement when we climb. we make the y movement equivalent to the actual y location that corresponds
        // to our new x location using our good friend Pythagoras
        float targetMoveX = deltaMovement.x * slopeModifier;
        float targetMoveMultiplier = targetMoveX >= 0 ? -1f : 1f;

        targetDelta.x = targetMoveX;
        targetDelta.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * targetDelta.x);

        RaycastHit2D raycastHit;

        do
        {
          bool isGoingRight = targetDelta.x > 0;

          // check whether we go through a wall, if so adjust...
          Logger.Trace(TRACE_TAG, "handleHorizontalSlope -> Raycast test; Current Position: {0}, Target Delta: {1}, Target Position: {2}, Current Delta: {3}, Target Move X: {4}, angle: {5}"
            , rayOrigin
            , targetDelta
            , (rayOrigin + targetDelta)
            , currentdelta
            , targetMoveX
            , angle);

          if (collisionState.wasGroundedLastFrame)
            raycastHit = Physics2D.Raycast(rayOrigin, targetDelta.normalized, targetDelta.magnitude, platformMask);
          else
            raycastHit = Physics2D.Raycast(rayOrigin, targetDelta.normalized, targetDelta.magnitude, platformMask & ~oneWayPlatformMask);

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

        deltaMovement.y = currentdelta.y;
        deltaMovement.x = currentdelta.x;

        _isGoingUpSlope = true;

        collisionState.below = true;
        collisionState.lastTimeGrounded = Time.time;
      }
      else
      {
        Logger.Trace(TRACE_TAG, "handleHorizontalSlope -> Jump threshold exceeded: deltaMovement.y >= slopeLimit [{0} >= {1}]", deltaMovement.y, jumpingThreshold);
      }
    }
    else // too steep. get out of here
    {
      Logger.Trace(TRACE_TAG, "handleHorizontalSlope -> slope limit exceeded.");
      deltaMovement.x = 0;
    }

    return true;
  }

  public bool canMoveVertically(float verticalRayDistance)
  {
    verticalRayDistance += _skinWidth;
    var initialRayOrigin = _raycastOrigins.topLeft;

    for (var i = 0; i < totalVerticalRays; i++)
    {
      var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

      DrawRay(ray, Vector2.up * verticalRayDistance, Color.red);
      _raycastHit = Physics2D.Raycast(ray, Vector2.up, verticalRayDistance, platformMask);

      if (_raycastHit)
      {
        return false;
      }
    }

    return true;
  }

  private void moveVerticallyOnSlope(ref Vector3 deltaMovement)
  {
    Logger.Trace(TRACE_TAG, "moveVerticallyOnSlope -> start vert move check");

    var rayDistance = deltaMovement.magnitude + _skinWidth;
    Vector3 rayDirection = deltaMovement.normalized;

    var initialRayOrigin = _raycastOrigins.topLeft;

    // if we are moving up, we should ignore the layers in oneWayPlatformMask
    var mask = platformMask;
    if (!collisionState.wasGroundedLastFrame)
      mask &= ~oneWayPlatformMask;

    for (var i = 0; i < totalVerticalRays; i++)
    {
      var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

      DrawRay(ray, rayDirection * rayDistance, Color.red);

      _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);

      Logger.Trace(TRACE_TAG, "moveVerticallyOnSlope -> test ray origin: {0}, ray target position: {1}, delta: {2}, delta target position: {3}"
        , ray
        , new Vector2(rayDirection.x * rayDistance, rayDirection.y * rayDistance)
        , deltaMovement
        , this.transform.position + deltaMovement);

      if (_raycastHit)
      {
        // set our new deltaMovement and recalculate the rayDistance taking it into account
        deltaMovement = _raycastHit.point - ray;

        Logger.Trace(TRACE_TAG, "moveVerticallyOnSlope -> ray hit; hit point: {0}, new delta: {1}, new delta target position: {2}"
          , _raycastHit.point
          , deltaMovement
          , this.transform.position + deltaMovement);

        rayDistance = deltaMovement.magnitude;
        // remember to remove the skinWidth from our deltaMovement

        if (deltaMovement.x > 0)
        {
          deltaMovement -= new Vector3(rayDirection.x * _skinWidth, rayDirection.y * _skinWidth, 0f);
        }
        else
        {
          deltaMovement += new Vector3(rayDirection.x * _skinWidth, rayDirection.y * _skinWidth, 0f);
        }

        collisionState.above = true;

        _raycastHitsThisFrame.Add(_raycastHit);

        // we add a small fudge factor for the float operations here. if our rayDistance is smaller
        // than the width + fudge bail out because we have a direct impact
        if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
          break;
      }
    }
    if (!_raycastHit)
    {
      Logger.Trace(TRACE_TAG, "moveVerticallyOnSlope -> No vertical ray hit.");
    }
  }

  private void moveVertically(ref Vector3 deltaMovement)
  {
    Logger.Trace(TRACE_TAG, "moveVertically -> start vert move check");

    var isGoingUp = deltaMovement.y > 0;
    var rayDistance = Mathf.Abs(deltaMovement.y) + _skinWidth;
    var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
    var initialRayOrigin = isGoingUp ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;

    // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
    initialRayOrigin.x += deltaMovement.x;

    // if we are moving up, we should ignore the layers in oneWayPlatformMask
    var mask = platformMask;
    if (isGoingUp && !collisionState.wasGroundedLastFrame)
      mask &= ~oneWayPlatformMask;

    for (var i = 0; i < totalVerticalRays; i++)
    {
      var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

      DrawRay(ray, rayDirection * rayDistance, Color.red);
      _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);

      Logger.Trace(TRACE_TAG, "moveVertically -> test ray origin: {0}, ray target position: {1}, delta: {2}, delta target position: {3}"
        , ray
        , new Vector2(rayDirection.x * rayDistance, rayDirection.y * rayDistance)
        , deltaMovement
        , this.transform.position + deltaMovement);

      if (_raycastHit)
      {
        Logger.Trace(TRACE_TAG, "moveVertically -> Vert Ray Hit. isGoingUp: {0}, deltaMovement.y: {1}", isGoingUp, deltaMovement.y);
        // set our new deltaMovement and recalculate the rayDistance taking it into account
        deltaMovement.y = _raycastHit.point.y - ray.y;
        Logger.Trace(TRACE_TAG, "moveVertically -> ray hit; hit point: {0}, new delta: {1}, new delta target position: {2}"
          , _raycastHit.point
          , deltaMovement
          , this.transform.position + deltaMovement);

        rayDistance = Mathf.Abs(deltaMovement.y);

        // remember to remove the skinWidth from our deltaMovement
        if (isGoingUp)
        {
          deltaMovement.y -= _skinWidth;
          collisionState.above = true;
        }
        else
        {
          deltaMovement.y += _skinWidth;
          collisionState.below = true;
          collisionState.lastTimeGrounded = Time.time;
        }

        _raycastHitsThisFrame.Add(_raycastHit);

        // this is a hack to deal with the top of slopes. if we walk up a slope and reach the apex we can get in a situation
        // where our ray gets a hit that is less then skinWidth causing us to be ungrounded the next frame due to residual velocity.
        if (!isGoingUp && deltaMovement.y > 0.00001f)
          _isGoingUpSlope = true;

        // we add a small fudge factor for the float operations here. if our rayDistance is smaller
        // than the width + fudge bail out because we have a direct impact
        if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
          break;
      }
    }
    if (!_raycastHit)
    {
      Logger.Trace(TRACE_TAG, "moveVertically -> No vertical ray hit.");
    }
  }


  /// <summary>
  /// checks the center point under the BoxCollider2D for a slope. If it finds one then the deltaMovement is adjusted so that
  /// the player stays grounded and the slopeSpeedModifier is taken into account to speed up movement.
  /// </summary>
  /// <param name="deltaMovement">Delta movement.</param>
  private void handleVerticalSlope(ref Vector3 deltaMovement)
  {
    // slope check from the center of our collider
    var centerOfCollider = (_raycastOrigins.bottomLeft.x + _raycastOrigins.bottomRight.x) * 0.5f;
    var rayDirection = -Vector2.up;

    // the ray distance is based on our slopeLimit
    var slopeCheckRayDistance = _slopeLimitTangent * (_raycastOrigins.bottomRight.x - centerOfCollider);

    var slopeRay = new Vector2(centerOfCollider, _raycastOrigins.bottomLeft.y);
    DrawRay(slopeRay, rayDirection * slopeCheckRayDistance, Color.yellow);
    _raycastHit = Physics2D.Raycast(slopeRay, rayDirection, slopeCheckRayDistance, platformMask);
    if (_raycastHit)
    {
      // bail out if we have no slope
      var angle = Vector2.Angle(_raycastHit.normal, Vector2.up);
      if (angle == 0)
      {
        return;
      }

      // we are moving down the slope if our normal and movement direction are in the same x direction
      var isMovingDownSlope = Mathf.Sign(_raycastHit.normal.x) == Mathf.Sign(deltaMovement.x);
      if (isMovingDownSlope)
      {
        // going down we want to speed up in most cases so the slopeSpeedMultiplier curve should be > 1 for negative angles
        var slopeModifier = slopeSpeedMultiplier.Evaluate(-angle);
        // we add the extra downward movement here to ensure we "stick" to the surface below
        deltaMovement.y += _raycastHit.point.y - slopeRay.y - skinWidth;
        deltaMovement.x *= slopeModifier;
        collisionState.movingDownSlope = true;
        collisionState.slopeAngle = angle;
      }
    }
  }

  #endregion


}

