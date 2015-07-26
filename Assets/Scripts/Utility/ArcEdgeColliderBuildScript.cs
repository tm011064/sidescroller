using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArcEdgeColliderBuildScript : MonoBehaviour
{
  public int totalSegments = 12;

  public MeshBuilderFillType meshBuilderFillType = MeshBuilderFillType.NoFill;
  public MeshBuilderFillDistanceType meshBuilderFillDistanceType = MeshBuilderFillDistanceType.Relative;
  public float fillDistance = 0f;
  public Material fillMaterial;

  public void BuildObject()
  {
    Logger.Info("Start building edge collider.");

    Transform ta = this.transform.FindChild("A");
    Transform tb = this.transform.FindChild("B");
    Transform tc = this.transform.FindChild("C");
    if (ta == null || tb == null || tc == null)
    {
      throw new MissingReferenceException();
    }

    Vector3 a = ta.position;
    Vector3 b = tb.position;
    Vector3 c = tc.position;

    float d = 2f * (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));

    Vector3 u = new Vector3(
      ((a.x * a.x + a.y * a.y) * (b.y - c.y) + (b.x * b.x + b.y * b.y) * (c.y - a.y) + (c.x * c.x + c.y * c.y) * (a.y - b.y)) / d
      , ((a.x * a.x + a.y * a.y) * (c.x - b.x) + (b.x * b.x + b.y * b.y) * (a.x - c.x) + (c.x * c.x + c.y * c.y) * (b.x - a.x)) / d
      );

    GameObject arcObject = new GameObject();
    arcObject.name = "Arc Edge Collider";
    arcObject.layer = LayerMask.NameToLayer("Platforms");

    EdgeCollider2D edgeCollider = arcObject.AddComponent<EdgeCollider2D>();

    List<Vector2> vectors = new List<Vector2>();

    float r = Vector3.Distance(a, u);

    Debug.Log("u: " + u + ", a: " + a + ", c: " + c);

    float sinb = ((a.x - u.x) / r);
    float startAngleRad = Mathf.Asin(sinb);
    Debug.Log("Start angle: " + startAngleRad * Mathf.Rad2Deg);

    float sinb2 = ((c.x - u.x) / r);
    float endAngleRad = Mathf.Asin(sinb2);
    Debug.Log("End angle: " + endAngleRad * Mathf.Rad2Deg);

    if (startAngleRad > endAngleRad)
    {
      float temp = startAngleRad;
      startAngleRad = endAngleRad;
      endAngleRad = temp;
    }

    Vector3 startPoint = u + new Vector3((float)(r * Mathf.Cos(endAngleRad)), (float)(r * Mathf.Sin(endAngleRad)));
    float sinb3 = ((startPoint.x - u.x) / r);
    float startPointAngle = Mathf.Asin(sinb3);

    float rotAngle = startAngleRad - startPointAngle;

    startAngleRad -= rotAngle;
    endAngleRad -= rotAngle;

    float totalAngle = Mathf.Abs(startAngleRad - endAngleRad);

    float max = totalAngle;// 2.0f * Mathf.PI;
    float step = max / (totalSegments);

    Debug.Log(startAngleRad * Mathf.Rad2Deg);
    Debug.Log(endAngleRad * Mathf.Rad2Deg);
    Debug.Log(r);

    float bottomPosition = float.MaxValue;
    for (float theta = endAngleRad; theta > startAngleRad - step / 2; theta -= step)
    {
      Vector2 vector = new Vector2((float)(r * Mathf.Cos(theta)), (float)(r * Mathf.Sin(theta)));
      vectors.Add(vector);
      if (vector.y < bottomPosition)
        bottomPosition = vector.y;
    }
    for (int i = 0; i < vectors.Count; i++)
      vectors[i] = new Vector2(vectors[i].x, vectors[i].y - bottomPosition);

    arcObject.transform.position = new Vector2(u.x, u.y + bottomPosition);

    edgeCollider.points = vectors.ToArray();

    arcObject.transform.parent = this.transform;

    if (meshBuilderFillType != MeshBuilderFillType.NoFill)
    {
      CreateMesh(arcObject, edgeCollider);
    }
  }

  private void CreateMesh(GameObject arcObject, EdgeCollider2D edgeCollider)
  {
    Logger.Info("Building meshes");

    var renderer = arcObject.AddComponent<MeshRenderer>();
    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    renderer.receiveShadows = false;
    renderer.useLightProbes = false;
    renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    renderer.material = fillMaterial;
    renderer.sortingLayerName = "Platforms";

    var mf = arcObject.AddComponent<MeshFilter>();

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
        ? (fillDistance - arcObject.transform.position.x)
        : (fillDistance - arcObject.transform.position.y);

    for (int i = 1; i < edgeCollider.points.Length; i++)
    {
      //vertices.Add(new Vector3(edgeCollider.points[i - 1].x, edgeCollider.points[i - 1].y, 0)); //top-left
      //vertices.Add(new Vector3(edgeCollider.points[i].x, edgeCollider.points[i].y, 0)); //top-right
      //vertices.Add(new Vector3(edgeCollider.points[i - 1].x, fillHeight, 0)); //bottom-left
      //vertices.Add(new Vector3(edgeCollider.points[i].x, fillHeight, 0)); //bottom-right

      if (meshBuilderFillType == MeshBuilderFillType.Vertical)
      {
        if (fillTo <= 0f)
        {
          vertices.Add(new Vector3(edgeCollider.points[i - 1].x, edgeCollider.points[i - 1].y, 0)); //top-left
          vertices.Add(new Vector3(edgeCollider.points[i].x, edgeCollider.points[i].y, 0)); //top-right
          vertices.Add(new Vector3(edgeCollider.points[i - 1].x, fillTo, 0)); //bottom-left
          vertices.Add(new Vector3(edgeCollider.points[i].x, fillTo, 0)); //bottom-right
        }
        else
        {
          vertices.Add(new Vector3(edgeCollider.points[i - 1].x, fillTo, 0)); //top-left
          vertices.Add(new Vector3(edgeCollider.points[i].x, fillTo, 0)); //top-right
          vertices.Add(new Vector3(edgeCollider.points[i - 1].x, edgeCollider.points[i - 1].y, 0)); //bottom-left
          vertices.Add(new Vector3(edgeCollider.points[i].x, edgeCollider.points[i].y, 0)); //bottom-right
        }
      }
      else
      {
        if (fillTo <= 0f)
        {
          vertices.Add(new Vector3(fillTo, edgeCollider.points[i].y, 0)); //top-left
          vertices.Add(new Vector3(edgeCollider.points[i].x, edgeCollider.points[i].y, 0)); //top-right
          vertices.Add(new Vector3(fillTo, edgeCollider.points[i - 1].y, 0)); //bottom-left
          vertices.Add(new Vector3(edgeCollider.points[i - 1].x, edgeCollider.points[i - 1].y, 0)); //bottom-right
        }
        else
        {
          vertices.Add(new Vector3(edgeCollider.points[i].x, edgeCollider.points[i].y, 0)); //top-left
          vertices.Add(new Vector3(fillTo, edgeCollider.points[i].y, 0)); //top-right
          vertices.Add(new Vector3(edgeCollider.points[i - 1].x, edgeCollider.points[i - 1].y, 0)); //bottom-left
          vertices.Add(new Vector3(fillTo, edgeCollider.points[i - 1].y, 0)); //bottom-right
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
}
