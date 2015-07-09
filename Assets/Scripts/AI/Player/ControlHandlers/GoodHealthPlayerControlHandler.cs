using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GoodHealthPlayerControlHandler : DefaultPlayerControlHandler
{
  public GoodHealthPlayerControlHandler(PlayerController playerController)
    : base(playerController)
  {
    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.green;
  }

}

