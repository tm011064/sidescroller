using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class VisibleRectangle
{
  public bool isEnabled = false;
  public bool isInsideOut = false;
  public Vector2 leftTop = Vector2.zero;
  public int width;
  public int height;
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CircleGroundPlatformMesh : BasePlatform
{
  public float visibilityCheckInterval = .2f;
  public int radius = 16;
  public int tileWidth = 16;
  public int tileHeight = 16;
  public VisibleRectangle visibleRectangle = new VisibleRectangle();

  void Start()
  {
    CreateCircle(tileWidth, tileHeight, radius * 2 / tileWidth, radius * 2 / tileHeight);
  }

  void CreateCircle(int tileWidth, int tileHeight, int gridWidth, int gridHeight)
  {
    var mesh = new Mesh();
    var mf = GetComponent<MeshFilter>();
    var renderer = mf.GetComponent<Renderer>();

    mf.mesh = mesh;

    var tileSizeX = 1.0f;
    var tileSizeY = 1.0f;

    var vertices = new List<Vector3>();
    var triangles = new List<int>();
    var normals = new List<Vector3>();
    var uvs = new List<Vector2>();

    var index = 0;

    int xStart, xEnd;
    int yStart, yEnd;

    if (visibleRectangle.isEnabled)
    {
      xStart = (int)visibleRectangle.leftTop.x;
      xEnd = xStart + visibleRectangle.width;
      yStart = (int)visibleRectangle.leftTop.y;
      yEnd = yStart + visibleRectangle.height;

      if (xStart > xEnd)
      {
        int temp = xStart;
        xStart = xEnd;
        xEnd = temp;
      }
      if (yStart > yEnd)
      {
        int temp = yStart;
        yStart = yEnd;
        yEnd = temp;
      }
    }
    else
    {
      xStart = -gridWidth / 2;
      xEnd = gridWidth / 2;
      yStart = -gridHeight / 2;
      yEnd = gridHeight / 2;
    }

    for (var x = xStart; x < xEnd; x++)
    {
      for (var y = yStart; y < yEnd; y++)
      {
        float distance = Vector2.Distance(new Vector2(x, y), Vector2.zero);

        bool doDraw = false;

        if (visibleRectangle.isEnabled)
        {
          if ((!visibleRectangle.isInsideOut && distance <= gridWidth / 2)
            || (visibleRectangle.isInsideOut && distance >= gridWidth / 2))
            doDraw = true;
        }
        else
        {
          doDraw = distance <= gridWidth / 2;
        }

        if (doDraw)
        {
          AddVertices(tileHeight, tileWidth, y, x, vertices);
          index = AddTriangles(index, triangles);
          AddNormals(normals);
          AddUvs(0, tileSizeY, tileSizeX, uvs, 0);
        }
      }
    }

    mesh.vertices = vertices.ToArray();
    mesh.normals = normals.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.uv = uvs.ToArray();
    mesh.RecalculateNormals();
  }

  private static void AddVertices(int tileHeight, int tileWidth, int y, int x, ICollection<Vector3> vertices)
  {
    vertices.Add(new Vector3((x * tileWidth), (y * tileHeight), 0));
    vertices.Add(new Vector3((x * tileWidth) + tileWidth, (y * tileHeight), 0));
    vertices.Add(new Vector3((x * tileWidth) + tileWidth, (y * tileHeight) + tileHeight, 0));
    vertices.Add(new Vector3((x * tileWidth), (y * tileHeight) + tileHeight, 0));
  }

  private static int AddTriangles(int index, ICollection<int> triangles)
  {
    triangles.Add(index + 2);
    triangles.Add(index + 1);
    triangles.Add(index);
    triangles.Add(index);
    triangles.Add(index + 3);
    triangles.Add(index + 2);
    index += 4;
    return index;
  }

  private static void AddNormals(ICollection<Vector3> normals)
  {
    normals.Add(Vector3.forward);
    normals.Add(Vector3.forward);
    normals.Add(Vector3.forward);
    normals.Add(Vector3.forward);
  }

  private static void AddUvs(int tileRow, float tileSizeY, float tileSizeX, ICollection<Vector2> uvs, int tileColumn)
  {
    uvs.Add(new Vector2(tileColumn * tileSizeX, tileRow * tileSizeY));
    uvs.Add(new Vector2((tileColumn + 1) * tileSizeX, tileRow * tileSizeY));
    uvs.Add(new Vector2((tileColumn + 1) * tileSizeX, (tileRow + 1) * tileSizeY));
    uvs.Add(new Vector2(tileColumn * tileSizeX, (tileRow + 1) * tileSizeY));
  }
}
