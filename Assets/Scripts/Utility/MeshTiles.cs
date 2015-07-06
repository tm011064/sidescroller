using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshTiles : MonoBehaviour
{
  public int tileWidth = 16;
  public int tileHeight = 16;
  public int numTilesX = 16;
  public int numTilesY = 16;
  public int tileGridWidth = 100;
  public int tileGridHeight = 100;
  public int defaultTileX;
  public int defaultTileY;
  public Texture2D texture;

  void OnEnable()
  {
    CreatePlane(tileWidth, tileHeight, tileGridWidth, tileGridHeight);
    UpdateGrid(new Vector2(0, 0), new Vector2(0, 0), tileWidth, tileHeight, tileGridWidth);
  }

  void Update()
  {
    //var tileColumn = Random.Range(0, numTilesX);
    //var tileRow = Random.Range(0, numTilesY);

    //var x = Random.Range(0, tileGridWidth);
    //var y = Random.Range(0, tileGridHeight);

    //UpdateGrid(new Vector2(x, y), new Vector2(tileColumn, tileRow), tileWidth, tileHeight, tileGridWidth);

  }

  //public void UpdateGrid(Vector2 gridIndex, Vector2 tileIndex, int tileWidth, int tileHeight, int gridWidth)
  //{
  //  var mesh = GetComponent<MeshFilter>().mesh;
  //  var uvs = mesh.uv;

  //  var tileSizeX = 1.0f / numTilesX;
  //  var tileSizeY = 1.0f / numTilesY;

  //  mesh.uv = uvs;

  //  uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 0] = new Vector2(tileIndex.x * tileSizeX, tileIndex.y * tileSizeY);
  //  uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 1] = new Vector2((tileIndex.x + 1) * tileSizeX, tileIndex.y * tileSizeY);
  //  uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 2] = new Vector2((tileIndex.x + 1) * tileSizeX, (tileIndex.y + 1) * tileSizeY);
  //  uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 3] = new Vector2(tileIndex.x * tileSizeX, (tileIndex.y + 1) * tileSizeY);

  //  mesh.uv = uvs;
  //}

  void CreatePlane(int tileWidth, int tileHeight, int gridWidth, int gridHeight)
  {
    var mesh = new Mesh();
    var mf = GetComponent<MeshFilter>();
    var renderer = mf.GetComponent<Renderer>();
    renderer.material.SetTexture("_MainTex", texture);
    mf.mesh = mesh;

    var tileSizeX = 1.0f / numTilesX;
    var tileSizeY = 1.0f / numTilesY;

    var vertices = new List<Vector3>();
    var triangles = new List<int>();
    var normals = new List<Vector3>();
    var uvs = new List<Vector2>();

    var index = 0;
    for (var x = 0; x < gridWidth; x++)
    {
      for (var y = 0; y < gridHeight; y++)
      {
        AddVertices(tileHeight, tileWidth, y, x, vertices);
        index = AddTriangles(index, triangles);
        AddNormals(normals);
        AddUvs(defaultTileX, tileSizeY, tileSizeX, uvs, defaultTileY);
      }
    }

    mesh.vertices = vertices.ToArray();
    mesh.normals = normals.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.uv = uvs.ToArray();
    mesh.RecalculateNormals();
  }

  public void UpdateGrid(Vector2 gridIndex, Vector2 tileIndex, int tileWidth, int tileHeight, int gridWidth)
  {
    var mesh = GetComponent<MeshFilter>().mesh;
    var uvs = mesh.uv;

    var tileSizeX = 1.0f / numTilesX;
    var tileSizeY = 1.0f / numTilesY;

    mesh.uv = uvs;

    uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 0] = new Vector2(tileIndex.x * tileSizeX, tileIndex.y * tileSizeY);
    uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 1] = new Vector2((tileIndex.x + 1) * tileSizeX, tileIndex.y * tileSizeY);
    uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 2] = new Vector2((tileIndex.x + 1) * tileSizeX, (tileIndex.y + 1) * tileSizeY);
    uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 3] = new Vector2(tileIndex.x * tileSizeX, (tileIndex.y + 1) * tileSizeY);

    mesh.uv = uvs;
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

