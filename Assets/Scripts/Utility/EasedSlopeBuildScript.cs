using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MeshBuilderFillType
{
  NoFill,
  Horizontal,
  Vertical
}
public enum MeshBuilderFillDistanceType
{
  Absolute,
  Relative
}

public class EasedSlopeBuildScript : MonoBehaviour
{
  public int totalSteps = 32;
  public Vector2 fromPosition = Vector2.zero;
  public Vector2 toPosition = Vector2.zero;
  public EasingType easingType = EasingType.EaseInOutSine;

  public MeshBuilderFillType meshBuilderFillType = MeshBuilderFillType.NoFill;
  public MeshBuilderFillDistanceType meshBuilderFillDistanceType = MeshBuilderFillDistanceType.Relative;
  public float fillDistance = 0f;

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
      yPosMultiplier = easing.GetValue(easingType, i, totalSteps);
      Vector2 vector = new Vector2(width * i / (float)totalSteps, height * yPosMultiplier);
      vectors.Add(vector);
    }
    vectors.Add(toPosition);

    _edgeCollider.points = vectors.ToArray();

    if (meshBuilderFillType != MeshBuilderFillType.NoFill)
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

      float fillTo = meshBuilderFillDistanceType == MeshBuilderFillDistanceType.Relative 
        ? fillDistance
        : meshBuilderFillType == MeshBuilderFillType.Horizontal
          ? (fillDistance - this.transform.position.x)
          : (fillDistance - this.transform.position.y);

      for (int i = 1; i < _edgeCollider.points.Length; i++)
      {
        if (meshBuilderFillType == MeshBuilderFillType.Vertical)
        {
          if (fillTo <= 0f)
          {
            vertices.Add(new Vector3(_edgeCollider.points[i - 1].x, _edgeCollider.points[i - 1].y, 0)); //top-left
            vertices.Add(new Vector3(_edgeCollider.points[i].x, _edgeCollider.points[i].y, 0)); //top-right
            vertices.Add(new Vector3(_edgeCollider.points[i - 1].x, fillTo, 0)); //bottom-left
            vertices.Add(new Vector3(_edgeCollider.points[i].x, fillTo, 0)); //bottom-right
          }
          else
          {
            vertices.Add(new Vector3(_edgeCollider.points[i - 1].x, fillTo, 0)); //top-left
            vertices.Add(new Vector3(_edgeCollider.points[i].x, fillTo, 0)); //top-right
            vertices.Add(new Vector3(_edgeCollider.points[i - 1].x, _edgeCollider.points[i - 1].y, 0)); //bottom-left
            vertices.Add(new Vector3(_edgeCollider.points[i].x, _edgeCollider.points[i].y, 0)); //bottom-right
          }
        }
        else
        {
          if (fillTo <= 0f)
          {
            vertices.Add(new Vector3(fillTo, _edgeCollider.points[i].y, 0)); //top-left
            vertices.Add(new Vector3(_edgeCollider.points[i].x, _edgeCollider.points[i].y, 0)); //top-right
            vertices.Add(new Vector3(fillTo, _edgeCollider.points[i - 1].y, 0)); //bottom-left
            vertices.Add(new Vector3(_edgeCollider.points[i - 1].x, _edgeCollider.points[i - 1].y, 0)); //bottom-right
          }
          else
          {
            vertices.Add(new Vector3(_edgeCollider.points[i].x, _edgeCollider.points[i].y, 0)); //top-left
            vertices.Add(new Vector3(fillTo, _edgeCollider.points[i].y, 0)); //top-right
            vertices.Add(new Vector3(_edgeCollider.points[i - 1].x, _edgeCollider.points[i - 1].y, 0)); //bottom-left
            vertices.Add(new Vector3(fillTo, _edgeCollider.points[i - 1].y, 0)); //bottom-right
          }
        }

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
