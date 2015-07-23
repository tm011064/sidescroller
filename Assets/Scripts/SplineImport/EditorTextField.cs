using UnityEngine;
using System.Collections;

public class EditorTextField : MonoBehaviour
{
  public string text;
  
#if UNITY_EDITOR
  void OnDrawGizmos()
  {
    UnityEditor.Handles.Label(transform.position, text);
  }
#endif
}
