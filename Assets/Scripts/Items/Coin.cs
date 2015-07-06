using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
  void OnTriggerEnter2D(Collider2D col)
  {
    GameManager.instance.AddCoin();
    Destroy(this.gameObject);
  }
}
