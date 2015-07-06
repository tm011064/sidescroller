using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadOnClick : MonoBehaviour
{
  private AsyncOperation async;
  public GameObject loadingImage;
  public Text loadingText;

  public void LoadScene(int level)
  {
    loadingImage.SetActive(true);
    StartCoroutine(LoadLevelWithBar(level));
  }

  IEnumerator LoadLevelWithBar(int level)
  {
    async = Application.LoadLevelAsync(level);
    while (!async.isDone)
    {
      loadingText.text = "Loading " + async.progress;
      yield return null;
    }
  }
}
