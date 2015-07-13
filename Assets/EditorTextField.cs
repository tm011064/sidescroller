using UnityEngine;
using System.Collections;

public class EditorTextField : MonoBehaviour
{
  public string text;
  
#if UNITY_EDITOR
  void OnDrawGizmos()
  {
    GUIContent content = new GUIContent(text);    
    UnityEditor.Handles.Label(transform.position, text);
  }
#endif
}
