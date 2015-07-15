// Copyright (c) 2010 Bob Berkebile
// Please direct any bugs/comments/suggestions to http://www.pixelplacement.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[AddComponentMenu("Pixelplacement/iTweenPath")]
public class iTweenPath : MonoBehaviour
{
  public string pathName = "";
  public Color pathColor = Color.cyan;
  public List<Vector3> nodes = new List<Vector3>() { Vector3.zero, Vector3.zero };
  public int nodeCount;
  public static Dictionary<string, iTweenPath> paths = new Dictionary<string, iTweenPath>(StringComparer.InvariantCultureIgnoreCase);
  public bool initialized = false;
  public string initialName = "";
  public bool pathVisible = true;

  public void SetPathName(string newPathName)
  {
    this.pathName = newPathName;
    paths[pathName] = this;
  }

  void OnEnable()
  {
    if (!paths.ContainsKey(pathName))
    {
      paths.Add(pathName, this);
    }
  }

  void OnDisable()
  {
    paths.Remove(pathName);
  }

  void OnDrawGizmosSelected()
  {
    if (pathVisible)
    {
      if (nodes.Count > 0)
      {
        iTween.DrawPath((from c in nodes
                         select this.gameObject.transform.TransformPoint(c)).ToArray(), pathColor);
      }
    }
  }

  /// <summary>
  /// Returns the visually edited path as a Vector3 array.
  /// </summary>
  /// <param name="requestedName">
  /// A <see cref="System.String"/> the requested name of a path.
  /// </param>
  /// <returns>
  /// A <see cref="Vector3[]"/>
  /// </returns>
  public static Vector3[] GetPath(string requestedName)
  {
    if (paths.ContainsKey(requestedName))
    {
      return paths[requestedName].nodes.ToArray();
    }
    else
    {
      Debug.Log("No path with that name (" + requestedName + ") exists! Are you sure you wrote it correctly?");
      return null;
    }
  }
  public Vector3 GetFirstNodeInWorldSpace()
  {
    return this.gameObject.transform.TransformPoint(nodes[0]);
  }
  public Vector3[] GetPathInWorldSpace()
  {
    Vector3[] worldNodes = new Vector3[nodes.Count];

    for (int i = 0; i < nodes.Count; i++)
      worldNodes[i] = this.gameObject.transform.TransformPoint(nodes[i]);

    return worldNodes;
  }

  /// <summary>
  /// Returns the reversed visually edited path as a Vector3 array.
  /// </summary>
  /// <param name="requestedName">
  /// A <see cref="System.String"/> the requested name of a path.
  /// </param>
  /// <returns>
  /// A <see cref="Vector3[]"/>
  /// </returns>
  public static Vector3[] GetPathReversed(string requestedName)
  {
    if (paths.ContainsKey(requestedName))
    {
      List<Vector3> revNodes = paths[requestedName].nodes.GetRange(0, paths[requestedName].nodes.Count);
      revNodes.Reverse();
      return revNodes.ToArray();
    }
    else
    {
      Debug.Log("No path with that name (" + requestedName + ") exists! Are you sure you wrote it correctly?");
      return null;
    }
  }
}