using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MovingPlatformSwitch : MonoBehaviour
{
  private DynamicPingPongPath[] _dynamicPingPongPaths = null;
   
  void OnEnable()
  {
    if (_dynamicPingPongPaths == null)
    {
      _dynamicPingPongPaths = this.gameObject.GetComponentsInChildren<DynamicPingPongPath>(true);
    }
  }

  void OnTriggerExit2D(Collider2D col)
  {
    for (int i = 0; i < _dynamicPingPongPaths.Length; i++)
    {
      _dynamicPingPongPaths[i].StopForwardMovement();
    }
  }

  void OnTriggerEnter2D(Collider2D col)
  {
    for (int i = 0; i < _dynamicPingPongPaths.Length; i++)
    {
      _dynamicPingPongPaths[i].StartForwardMovement();
    }
  }
}
