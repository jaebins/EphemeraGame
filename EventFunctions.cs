using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventFunctions : MonoBehaviour
{
    public void GameStartTrigger()
    {
        Animator ani = GameObject.Find("Canvas").transform.Find("Panel").Find("LoadingGamePanel").GetComponent<Animator>();
        ani.SetTrigger("gameStart");
        StartCoroutine(GameStart());
    }

    IEnumerator GameStart()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(1);
    }
}
