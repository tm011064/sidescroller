using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GridLineSettings
{
  [Range(16, 2048)]
  public float width = 32f;
  [Range(16, 2048)]
  public float height = 32f;
  public bool visible = false;
}

public class EditorGrid : MonoBehaviour
{
  public List<GridLineSettings> gridLineSettings = new List<GridLineSettings>();
  public bool visible = true;

  public Vector2 size = new Vector2(2048, 2048);
  public Vector2 center = Vector2.zero;

  void OnDrawGizmos()
  {
    if (visible)
    {
      Queue<Color> colors = new Queue<Color>(new List<Color>() { Color.gray, Color.white, Color.green });

      for (int i = 0; i < gridLineSettings.Count; i++)
      {
        if (!gridLineSettings[i].visible)
          continue;

        if ((int)gridLineSettings[i].height < 16 || (int)gridLineSettings[i].width < 16)
          continue;

        if (colors.Count > 0)
          Gizmos.color = colors.Dequeue();

        int leftX = Mathf.RoundToInt(center.x - size.x * .5f);
        int rightX = Mathf.RoundToInt(center.x + size.x * .5f);
        int bottomY = Mathf.RoundToInt(center.y - size.y * .5f);
        int topY = Mathf.RoundToInt(center.y + size.y * .5f);

        for (int y = 0; y < size.y * .5f; y += (int)gridLineSettings[i].height)
        {
          Gizmos.DrawLine(new Vector3(leftX, y),
                          new Vector3(rightX, y));
          if (y != 0)
          {
            Gizmos.DrawLine(new Vector3(leftX, -y),
                            new Vector3(rightX, -y));
          }
        }
        for (int x = 0; x < size.x * .5f; x += (int)gridLineSettings[i].width)
        {
          Gizmos.DrawLine(new Vector3(x, bottomY),
                          new Vector3(x, topY));
          if (x != 0)
          {
            Gizmos.DrawLine(new Vector3(-x, bottomY),
                            new Vector3(-x, topY));
          }
        }
      }
    }
  }

}