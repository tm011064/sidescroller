using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum EasingType
{
  Linear,
  EaseInQuad,
  EaseOutQuad,
  EaseInOutQuad,
  EaseInCubic,
  EaseOutCubic,
  EaseInOutCubic,
  EaseInQuart,
  EaseOutQuart,
  EaseInOutQuart,
  EaseInQuint,
  EaseOutQuint,
  EaseInOutQuint,
  EaseInSine,
  EaseOutSine,
  EaseInOutSine,
  EaseInExpo,
  EaseOutExpo,
  EaseInOutExpo,
  EaseInCirc,
  EaseOutCirc,
  EaseInOutCirc,
  EaseInBack,
  EaseOutBack,
  EaseInOutBack,
  EaseInBounce,
  EaseOutBounce,
  EaseInOutBounce,
}

public class Easing
{
  // http://easings.net/

  private const float PI_HALF = (float)((float)Math.PI / 2d);

  public float Linear(float currentTime, float duration)
  {
    return currentTime / duration;
  }
  public float EaseInQuad(float currentTime, float duration)
  {
    return 1f * (currentTime /= duration) * currentTime;
  }
  public float EaseOutQuad(float currentTime, float duration)
  {
    return -1f * (currentTime /= duration) * (currentTime - 2);
  }
  public float EaseInOutQuad(float currentTime, float duration)
  {
    if ((currentTime /= duration / 2) < 1) return 1f / 2 * currentTime * currentTime;
    return -1f / 2 * ((--currentTime) * (currentTime - 2) - 1);
  }
  public float EaseInCubic(float currentTime, float duration)
  {
    return 1f * (currentTime /= duration) * currentTime * currentTime;
  }
  public float EaseOutCubic(float currentTime, float duration)
  {
    return 1f * ((currentTime = currentTime / duration - 1) * currentTime * currentTime + 1);
  }
  public float EaseInOutCubic(float currentTime, float duration)
  {
    if ((currentTime /= duration / 2) < 1) return 1f / 2 * currentTime * currentTime * currentTime;
    return 1f / 2 * ((currentTime -= 2) * currentTime * currentTime + 2);
  }
  public float EaseInQuart(float currentTime, float duration)
  {
    return 1f * (currentTime /= duration) * currentTime * currentTime * currentTime;
  }
  public float EaseOutQuart(float currentTime, float duration)
  {
    return -1f * ((currentTime = currentTime / duration - 1) * currentTime * currentTime * currentTime - 1);
  }
  public float EaseInOutQuart(float currentTime, float duration)
  {
    if ((currentTime /= duration / 2) < 1) return 1f / 2 * currentTime * currentTime * currentTime * currentTime;
    return -1f / 2 * ((currentTime -= 2) * currentTime * currentTime * currentTime - 2);
  }
  public float EaseInQuint(float currentTime, float duration)
  {
    return 1f * (currentTime /= duration) * currentTime * currentTime * currentTime * currentTime;
  }
  public float EaseOutQuint(float currentTime, float duration)
  {
    return 1f * ((currentTime = currentTime / duration - 1) * currentTime * currentTime * currentTime * currentTime + 1);
  }
  public float EaseInOutQuint(float currentTime, float duration)
  {
    if ((currentTime /= duration / 2) < 1) return 1f / 2 * currentTime * currentTime * currentTime * currentTime * currentTime;
    return 1f / 2 * ((currentTime -= 2) * currentTime * currentTime * currentTime * currentTime + 2);
  }
  public float EaseInSine(float currentTime, float duration)
  {
    return -1f * (float)Math.Cos(currentTime / duration * (PI_HALF)) + 1f;
  }
  public float EaseOutSine(float currentTime, float duration)
  {
    return 1f * (float)Math.Sin(currentTime / duration * (PI_HALF));
  }
  public float EaseInOutSine(float currentTime, float duration)
  {
    return -1f / 2 * ((float)Math.Cos((float)Math.PI * currentTime / duration) - 1);
  }
  public float EaseInExpo(float currentTime, float duration)
  {
    return (currentTime == 0) ? 0f : 1f * (float)Math.Pow(2, 10 * (currentTime / duration - 1));
  }
  public float EaseOutExpo(float currentTime, float duration)
  {
    return (currentTime == duration) ? 1f : 1f * (-(float)Math.Pow(2, -10 * currentTime / duration) + 1);
  }
  public float EaseInOutExpo(float currentTime, float duration)
  {
    if (currentTime == 0) return 0f;
    if (currentTime == duration) return 1f;
    if ((currentTime /= duration / 2) < 1) return 1f / 2 * (float)Math.Pow(2, 10 * (currentTime - 1));
    return 1f / 2 * (-(float)Math.Pow(2, -10 * --currentTime) + 2);
  }
  public float EaseInCirc(float currentTime, float duration)
  {
    return -1f * ((float)Math.Sqrt(1 - (currentTime /= duration) * currentTime) - 1);
  }
  public float EaseOutCirc(float currentTime, float duration)
  {
    return 1f * (float)Math.Sqrt(1 - (currentTime = currentTime / duration - 1) * currentTime);
  }
  public float EaseInOutCirc(float currentTime, float duration)
  {
    if ((currentTime /= duration / 2) < 1) return -1f / 2 * ((float)Math.Sqrt(1 - currentTime * currentTime) - 1);
    return 1f / 2 * ((float)Math.Sqrt(1 - (currentTime -= 2) * currentTime) + 1);
  }
  public float EaseInBack(float currentTime, float duration)
  {
    float s = 1.70158f;
    return 1f * (currentTime /= duration) * currentTime * ((s + 1) * currentTime - s);
  }
  public float EaseOutBack(float currentTime, float duration)
  {
    float s = 1.70158f;
    return 1f * ((currentTime = currentTime / duration - 1) * currentTime * ((s + 1) * currentTime + s) + 1);
  }
  public float EaseInOutBack(float currentTime, float duration)
  {
    float s = 1.70158f;
    if ((currentTime /= duration / 2) < 1) return 1f / 2 * (currentTime * currentTime * (((s *= (1.525f)) + 1) * currentTime - s));
    return 1f / 2 * ((currentTime -= 2) * currentTime * (((s *= (1.525f)) + 1) * currentTime + s) + 2);
  }
  public float EaseInBounce(float currentTime, float duration)
  {
    return 1f - EaseOutBounce(duration - currentTime, duration);
  }
  public float EaseOutBounce(float currentTime, float duration)
  {
    if ((currentTime /= duration) < (1 / 2.75))
    {
      return 1f * (7.5625f * currentTime * currentTime);
    }
    else if (currentTime < (2 / 2.75))
    {
      return 1f * (7.5625f * (currentTime -= (1.5f / 2.75f)) * currentTime + .75f);
    }
    else if (currentTime < (2.5 / 2.75))
    {
      return 1f * (7.5625f * (currentTime -= (2.25f / 2.75f)) * currentTime + .9375f);
    }
    else
    {
      return 1f * (7.5625f * (currentTime -= (2.625f / 2.75f)) * currentTime + .984375f);
    }
  }

  public float EaseInOutBounce(float currentTime, float duration)
  {
    if (currentTime < duration / 2f) return EaseInBounce(currentTime * 2f, duration) * .5f;
    return EaseOutBounce(currentTime * 2f - duration, duration) * .5f + 1f * .5f;
  }
}
