using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BadHealthPlayerControlHandler : DefaultPlayerControlHandler
{
  public BadHealthPlayerControlHandler(PlayerController playerController, float overrideEndTime)
    : base(playerController, overrideEndTime)
  {
    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.red;
  }

}

