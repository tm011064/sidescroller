using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnumFlagsAttribute : PropertyAttribute
{
  public EnumFlagsAttribute() { }
}
public class EnumFlagAttribute : PropertyAttribute
{
  public EnumFlagAttribute() { }
}

public static class CircularLinkedList
{
  public static LinkedListNode<T> NextOrFirst<T>(this LinkedListNode<T> current)
  {
    if (current.Next == null)
      return current.List.First;
    return current.Next;
  }

  public static LinkedListNode<T> PreviousOrLast<T>(this LinkedListNode<T> current)
  {
    if (current.Previous == null)
      return current.List.Last;
    return current.Previous;
  }
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public partial class RectangleMeshBuildScript : BasePlatform
{
  public float visibilityCheckInterval = .2f;
  public int width = 16;
  public int height = 16;
  [EnumFlag]
  public Direction colliderSides = Direction.Left | Direction.Top | Direction.Right | Direction.Bottom;

#if UNITY_EDITOR
  private EdgeCollider2D[] _edgeCollidersToDestroy;
  private BoxCollider2D[] _boxCollider2DToDestroy;

  public void SafeDeleteColliders()
  {
    if (_edgeCollidersToDestroy != null)
    {
      Debug.Log("Rectangle Mesh Builder: Deleting old edge colliders...");
      foreach (var collider in _edgeCollidersToDestroy)
        DestroyImmediate(collider);

      _edgeCollidersToDestroy = null;
    }
    if (_boxCollider2DToDestroy != null)
    {
      Debug.Log("Rectangle Mesh Builder: Deleting old box colliders...");
      foreach (var collider in _boxCollider2DToDestroy)
        DestroyImmediate(collider);

      _boxCollider2DToDestroy = null;
    }
  }

  private void SetColliders()
  {
    Debug.Log("Rectangle Mesh Builder: Destroying edge and box colliders.");

    _edgeCollidersToDestroy = this.gameObject.GetComponents<EdgeCollider2D>();
    foreach (var collider in _edgeCollidersToDestroy)
      collider.hideFlags = HideFlags.HideInInspector;
    _boxCollider2DToDestroy = this.gameObject.GetComponents<BoxCollider2D>();
    foreach (var collider in _boxCollider2DToDestroy)
      collider.hideFlags = HideFlags.HideInInspector;

    //foreach (var collider in this.gameObject.GetComponents<EdgeCollider2D>())
    //  DestroyImmediate(collider);
    //foreach (var collider in this.gameObject.GetComponents<BoxCollider2D>())
    //  DestroyImmediate(collider);
    
    if (colliderSides == (Direction.Left | Direction.Top | Direction.Right | Direction.Bottom))
    {
      Debug.Log("Rectangle Mesh Builder: Creating box collider.");

      BoxCollider2D boxCollider2D = this.gameObject.AddComponent<BoxCollider2D>();
      boxCollider2D.hideFlags = HideFlags.NotEditable;
      boxCollider2D.size = new Vector2(width, height);
      Debug.Log("Rectangle Mesh Builder: Setting box collider size to " + boxCollider2D.size);
    }
    else if (colliderSides == 0)
    {
      Debug.Log("No colliders");
    }
    else
    {
      List<Vector2> vectors;
      EdgeCollider2D collider;

      if (colliderSides == (Direction.Left | Direction.Right))
      {
        Debug.Log("Rectangle Mesh Builder: Creating Left and Right edge colliders.");
        vectors = new List<Vector2>();
        vectors.Add(new Vector2(0, 0));
        vectors.Add(new Vector2(0, height));
        collider = this.gameObject.AddComponent<EdgeCollider2D>();
        collider.hideFlags = HideFlags.NotEditable;
        collider.points = vectors.ToArray();

        vectors = new List<Vector2>();
        vectors.Add(new Vector2(width, 0));
        vectors.Add(new Vector2(width, height));
        collider = this.gameObject.AddComponent<EdgeCollider2D>();
        collider.hideFlags = HideFlags.NotEditable;
        collider.points = vectors.ToArray();
      }
      else if (colliderSides == (Direction.Top | Direction.Bottom))
      {
        Debug.Log("Rectangle Mesh Builder: Creating Top and Bottom edge colliders.");
        vectors = new List<Vector2>();
        vectors.Add(new Vector2(0, height));
        vectors.Add(new Vector2(width, height));
        collider = this.gameObject.AddComponent<EdgeCollider2D>();
        collider.hideFlags = HideFlags.NotEditable;
        collider.points = vectors.ToArray();

        vectors = new List<Vector2>();
        vectors.Add(new Vector2(0, 0));
        vectors.Add(new Vector2(width, 0));
        collider = this.gameObject.AddComponent<EdgeCollider2D>();
        collider.hideFlags = HideFlags.NotEditable;
        collider.points = vectors.ToArray();
      }
      else
      {
        Debug.Log("Rectangle Mesh Builder: Creating edge collider.");
        collider = this.gameObject.AddComponent<EdgeCollider2D>();
        collider.hideFlags = HideFlags.NotEditable;

        // go counter clockwise to find gap and then go clockwise to build vectors
        LinkedList<Direction?> linkedDirections = new LinkedList<Direction?>();
        LinkedListNode<Direction?> currentListNode = linkedDirections.AddFirst(((colliderSides & Direction.Left) != 0) ? (Direction?)Direction.Left : null);
        currentListNode = linkedDirections.AddAfter(currentListNode, ((colliderSides & Direction.Top) != 0) ? (Direction?)Direction.Top : null);
        currentListNode = linkedDirections.AddAfter(currentListNode, ((colliderSides & Direction.Right) != 0) ? (Direction?)Direction.Right : null);
        currentListNode = linkedDirections.AddAfter(currentListNode, ((colliderSides & Direction.Bottom) != 0) ? (Direction?)Direction.Bottom : null);

        while (true)
        {// search backwards for gap opening
          if (currentListNode.Value.HasValue && !currentListNode.PreviousOrLast().Value.HasValue)
            break;
          currentListNode = currentListNode.PreviousOrLast();
        }

        // add first point
        vectors = new List<Vector2>();
        switch (currentListNode.Value.Value)
        {
          case Direction.Left: vectors.Add(new Vector2(0, 0)); break;
          case Direction.Top: vectors.Add(new Vector2(0, height)); break;
          case Direction.Right: vectors.Add(new Vector2(width, height)); break;
          case Direction.Bottom: vectors.Add(new Vector2(width, 0)); break;
        }

        while (currentListNode.Value.HasValue)
        {// now go forward to fill points until gap
          switch (currentListNode.Value.Value)
          {
            case Direction.Left: vectors.Add(new Vector2(0, height)); break;
            case Direction.Top: vectors.Add(new Vector2(width, height)); break;
            case Direction.Right: vectors.Add(new Vector2(width, 0)); break;
            case Direction.Bottom: vectors.Add(new Vector2(0, 0)); break;
          }

          currentListNode = currentListNode.NextOrFirst();
        }

        collider.points = vectors.ToArray();
      }
    }
  }

  public void BuildObject()
  {
    SetColliders();

    var mesh = new Mesh();
    var meshFilter = this.gameObject.GetComponent<MeshFilter>();
    meshFilter.mesh = mesh;

    var vertices = new List<Vector3>();
    var triangles = new List<int>();
    var normals = new List<Vector3>();
    var uvs = new List<Vector2>();

    var index = 0;

    vertices.Add(new Vector3(0, height, 0)); //top-left
    vertices.Add(new Vector3(width, height, 0)); //top-right
    vertices.Add(new Vector3(0, 0, 0)); //bottom-left
    vertices.Add(new Vector3(width, 0, 0)); //bottom-right

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

    mesh.vertices = vertices.ToArray();
    mesh.normals = normals.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.uv = uvs.ToArray();
    mesh.RecalculateNormals();
  }
#endif
}
