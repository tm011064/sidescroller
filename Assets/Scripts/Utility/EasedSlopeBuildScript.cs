using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EasedSlopeBuildScript : MonoBehaviour
{
  public int totalSteps = 32;
  public Vector2 fromPosition = Vector2.zero;
  public Vector2 toPosition = Vector2.zero;
  public EasingType easingType = EasingType.EaseInOutSine;

  public bool fillToYPos = true;
  public float yPosToFill = 0f;

  private EdgeCollider2D _edgeCollider;

  public void BuildObject()
  {
    Logger.Info("Start building edge collider.");

    _edgeCollider = this.GetComponent<EdgeCollider2D>();
    if (_edgeCollider == null)
    {
      Logger.Info("Adding edge collider component.");
      _edgeCollider = this.gameObject.AddComponent<EdgeCollider2D>();
    }
    List<Vector2> vectors = new List<Vector2>();

    Easing easing = new Easing();

    float width = toPosition.x - fromPosition.x;
    float height = toPosition.y - fromPosition.y;

    for (float i = 0; i < totalSteps; i++)
    {
      float yPosMultiplier = 1f;
      switch (easingType)
      {
        case EasingType.Linear: yPosMultiplier = easing.Linear(i, totalSteps); break;
        case EasingType.EaseInQuad: yPosMultiplier = easing.EaseInQuad(i, totalSteps); break;
        case EasingType.EaseOutQuad: yPosMultiplier = easing.EaseOutQuad(i, totalSteps); break;
        case EasingType.EaseInOutQuad: yPosMultiplier = easing.EaseInOutQuad(i, totalSteps); break;
        case EasingType.EaseInCubic: yPosMultiplier = easing.EaseInCubic(i, totalSteps); break;
        case EasingType.EaseOutCubic: yPosMultiplier = easing.EaseOutCubic(i, totalSteps); break;
        case EasingType.EaseInOutCubic: yPosMultiplier = easing.EaseInOutCubic(i, totalSteps); break;
        case EasingType.EaseInQuart: yPosMultiplier = easing.EaseInQuart(i, totalSteps); break;
        case EasingType.EaseOutQuart: yPosMultiplier = easing.EaseOutQuart(i, totalSteps); break;
        case EasingType.EaseInOutQuart: yPosMultiplier = easing.EaseInOutQuart(i, totalSteps); break;
        case EasingType.EaseInQuint: yPosMultiplier = easing.EaseInQuint(i, totalSteps); break;
        case EasingType.EaseOutQuint: yPosMultiplier = easing.EaseOutQuint(i, totalSteps); break;
        case EasingType.EaseInOutQuint: yPosMultiplier = easing.EaseInOutQuint(i, totalSteps); break;
        case EasingType.EaseInSine: yPosMultiplier = easing.EaseInSine(i, totalSteps); break;
        case EasingType.EaseOutSine: yPosMultiplier = easing.EaseOutSine(i, totalSteps); break;
        case EasingType.EaseInOutSine: yPosMultiplier = easing.EaseInOutSine(i, totalSteps); break;
        case EasingType.EaseInExpo: yPosMultiplier = easing.EaseInExpo(i, totalSteps); break;
        case EasingType.EaseOutExpo: yPosMultiplier = easing.EaseOutExpo(i, totalSteps); break;
        case EasingType.EaseInOutExpo: yPosMultiplier = easing.EaseInOutExpo(i, totalSteps); break;
        case EasingType.EaseInCirc: yPosMultiplier = easing.EaseInCirc(i, totalSteps); break;
        case EasingType.EaseOutCirc: yPosMultiplier = easing.EaseOutCirc(i, totalSteps); break;
        case EasingType.EaseInOutCirc: yPosMultiplier = easing.EaseInOutCirc(i, totalSteps); break;
        case EasingType.EaseInBack: yPosMultiplier = easing.EaseInBack(i, totalSteps); break;
        case EasingType.EaseOutBack: yPosMultiplier = easing.EaseOutBack(i, totalSteps); break;
        case EasingType.EaseInOutBack: yPosMultiplier = easing.EaseInOutBack(i, totalSteps); break;
        case EasingType.EaseInBounce: yPosMultiplier = easing.EaseInBounce(i, totalSteps); break;
        case EasingType.EaseOutBounce: yPosMultiplier = easing.EaseOutBounce(i, totalSteps); break;
        case EasingType.EaseInOutBounce: yPosMultiplier = easing.EaseInOutBounce(i, totalSteps); break;
      }

      Vector2 vector = new Vector2(width * i / (float)totalSteps, height * yPosMultiplier);
      vectors.Add(vector);
    }
    vectors.Add(toPosition);


    LineRenderer lineRenderer = this.GetComponent<LineRenderer>();
    if (lineRenderer != null)
    {
      lineRenderer.SetVertexCount(vectors.Count);
      for (int i = 0; i < vectors.Count; i++)
      {
        lineRenderer.SetPosition(i, new Vector3(vectors[i].x, vectors[i].y));
      }
    }

    _edgeCollider.points = vectors.ToArray();

    if (fillToYPos)
    {
      CreateMesh();
    }
  }

  private void CreateMesh()
  {
    var mf = GetComponentInChildren<MeshFilter>();
    if (mf != null)
    {
      Logger.Info("Building meshes");
      var mesh = new Mesh();

      mf.mesh = mesh;

      var vertices = new List<Vector3>();
      var triangles = new List<int>();
      var normals = new List<Vector3>();
      var uvs = new List<Vector2>();

      var index = 0;

      float fillHeight = yPosToFill - this.transform.position.y;
      for (int i = 1; i < _edgeCollider.points.Length; i++)
      {
        vertices.Add(new Vector3(_edgeCollider.points[i - 1].x, _edgeCollider.points[i - 1].y, 0)); //top-left
        vertices.Add(new Vector3(_edgeCollider.points[i].x, _edgeCollider.points[i].y, 0)); //top-right
        vertices.Add(new Vector3(_edgeCollider.points[i - 1].x, fillHeight, 0)); //bottom-left
        vertices.Add(new Vector3(_edgeCollider.points[i].x, fillHeight, 0)); //bottom-right

        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
        triangles.Add(index + 3);
        triangles.Add(index + 2);
        triangles.Add(index + 1);

        index += 4;

        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);

        uvs.Add(new Vector2(0, 1)); //top-left
        uvs.Add(new Vector2(1, 1)); //top-right
        uvs.Add(new Vector2(0, 0)); //bottom-left
        uvs.Add(new Vector2(1, 0)); //bottom-right
      }

      mesh.vertices = vertices.ToArray();
      mesh.normals = normals.ToArray();
      mesh.triangles = triangles.ToArray();
      mesh.uv = uvs.ToArray();
      mesh.RecalculateNormals();
    }
    else
    {
      Logger.Info("No mesh filter found meshes");
    }
  }
}
