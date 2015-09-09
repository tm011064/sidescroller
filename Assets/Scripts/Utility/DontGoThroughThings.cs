using UnityEngine;
using System.Collections;

public class DontGoThroughThings : MonoBehaviour
{
  [Tooltip("All layers that the scan rays can collide with. Should include platforms and player.")]
  public LayerMask scanRayDirectionDownCollisionLayers = 0;
  [Tooltip("All layers that the scan rays can collide with. Should include platforms and player.")]
  public LayerMask scanRayDirectionUpCollisionLayers = 0;

  public float skinWidth = 0.1f; //probably doesn't need to be changed 

  private float minimumExtent;
  private float partialExtent;
  private float sqrMinimumExtent;
  private Vector3 previousPosition;
  private Collider2D myCollider;

  private bool _skipFirstFrame;

  //initialize values 
  void OnEnable()
  {
    previousPosition = this.gameObject.transform.position;
  }

  void Awake()
  {
    myCollider = GetComponent<Collider2D>();
  }

  void Start()
  {
    minimumExtent = Mathf.Min(Mathf.Min(myCollider.bounds.extents.x, myCollider.bounds.extents.y), myCollider.bounds.extents.z);
    partialExtent = minimumExtent * (1.0f - skinWidth);
    sqrMinimumExtent = minimumExtent * minimumExtent;
  }

  void Update()
  {
    //have we moved more than our minimum extent? 
    Vector2 movementThisStep = this.gameObject.transform.position - previousPosition;
    if (movementThisStep.magnitude > sqrMinimumExtent)
    {
      //check for obstructions we might have missed 
      RaycastHit2D hitInfo = Physics2D.Raycast(previousPosition, movementThisStep.normalized, movementThisStep.magnitude
        , movementThisStep.y > 0f ? scanRayDirectionUpCollisionLayers : scanRayDirectionDownCollisionLayers
        );
      if (hitInfo)
      {
        myCollider.SendMessage("OnTriggerEnter2D", hitInfo.collider, SendMessageOptions.DontRequireReceiver);
      }
    }

    previousPosition = this.gameObject.transform.position;
  }
}