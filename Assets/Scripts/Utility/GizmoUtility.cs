using UnityEngine;

public static class GizmoUtility
{
  public static void DrawBoundingBox(Transform transform, Vector3 extents, Color color)
  {
    Vector3 v3FrontTopLeft = new Vector3( - extents.x,  + extents.y,  - extents.z);  // Front top left corner
    Vector3 v3FrontTopRight = new Vector3( + extents.x,  + extents.y,  - extents.z);  // Front top right corner
    Vector3 v3FrontBottomLeft = new Vector3( - extents.x,  - extents.y,  - extents.z);  // Front bottom left corner
    Vector3 v3FrontBottomRight = new Vector3( + extents.x,  - extents.y,  - extents.z);  // Front bottom right corner

    Gizmos.color = color;

    Gizmos.matrix = transform.localToWorldMatrix;

    Gizmos.DrawLine(v3FrontTopLeft, v3FrontTopRight);
    Gizmos.DrawLine(v3FrontTopRight, v3FrontBottomRight);
    Gizmos.DrawLine(v3FrontBottomRight, v3FrontBottomLeft);
    Gizmos.DrawLine(v3FrontBottomLeft, v3FrontTopLeft);
  }
  public static void DrawBoundingBox(Vector3 center, Vector3 extents, Color color)
  {
    Vector3 v3FrontTopLeft = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);  // Front top left corner
    Vector3 v3FrontTopRight = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);  // Front top right corner
    Vector3 v3FrontBottomLeft = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);  // Front bottom left corner
    Vector3 v3FrontBottomRight = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);  // Front bottom right corner

    Gizmos.color = color;

    Gizmos.DrawLine(v3FrontTopLeft, v3FrontTopRight);
    Gizmos.DrawLine(v3FrontTopRight, v3FrontBottomRight);
    Gizmos.DrawLine(v3FrontBottomRight, v3FrontBottomLeft);
    Gizmos.DrawLine(v3FrontBottomLeft, v3FrontTopLeft);
  }

  public static Rect BoundsToScreenRect(Bounds bounds)
  {
    // Get mesh origin and farthest extent (this works best with simple convex meshes)
    Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
    Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));

    // Create rect in screen space and return - does not account for camera perspective
    return new Rect(origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);

  }

  public static Vector3[] CreateEllipse(float a, float b, float h, float k, float theta, int resolution)
  {
    Vector3[] positions = new Vector3[resolution + 1];
    Quaternion q = Quaternion.AngleAxis(theta, Vector3.forward);
    Vector3 center = new Vector3(h, k, 0.0f);

    for (int i = 0; i <= resolution; i++)
    {
      float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
      positions[i] = new Vector3(a * Mathf.Cos(angle), b * Mathf.Sin(angle), 0.0f);
      positions[i] = q * positions[i] + center;
    }

    return positions;
  }
}

