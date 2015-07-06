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
}

