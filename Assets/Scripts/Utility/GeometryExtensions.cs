using UnityEngine;

public static class GeometryExtensions
{
  public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
  {
    Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
    return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
  }
  public static bool IsVisibleFrom(this Collider2D collider, Camera camera)
  {
    Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
    return GeometryUtility.TestPlanesAABB(planes, collider.bounds);
  }

  public static bool IsPointOnEdge(this Bounds bounds, Vector3 vector)
  {
    if (vector.x == bounds.center.x - bounds.extents.x)
    {
      if (vector.y >= bounds.center.y - bounds.extents.y
        && vector.y <= bounds.center.y + bounds.extents.y)
      {
        return true;
      }
    }
    else if (vector.x == bounds.center.x + bounds.extents.x)
    {
      if (vector.y >= bounds.center.y - bounds.extents.y
        && vector.y <= bounds.center.y + bounds.extents.y)
      {
        return true;
      }
    }
    else if (vector.y == bounds.center.y + bounds.extents.y)
    {
      if (vector.x >= bounds.center.x - bounds.extents.x
        && vector.x <= bounds.center.x + bounds.extents.x)
      {
        return true;
      }
    }
    else if (vector.y == bounds.center.y - bounds.extents.y)
    {
      if (vector.x >= bounds.center.x - bounds.extents.x
        && vector.x <= bounds.center.x + bounds.extents.x)
      {
        return true;
      }
    }

    return false;
  }

  public static Vector3 ToVector3(this Vector2 vector)
  {
    return new Vector3(vector.x, vector.y);
  }
  
  public static float CrossProduct(this Vector2 v1, Vector2 v2)
  {
    return v1.x * v2.y - v1.y * v2.x;
  }
}

