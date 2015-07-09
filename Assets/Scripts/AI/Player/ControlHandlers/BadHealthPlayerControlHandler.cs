﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BadHealthPlayerControlHandler : DefaultPlayerControlHandler
{
  public BadHealthPlayerControlHandler(PlayerController playerController)
    : base(playerController)
  {
    base.doDrawDebugBoundingBox = true;
    base.debugBoundingBoxColor = Color.red;
  }

}

