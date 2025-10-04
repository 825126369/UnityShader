using System.Collections;
using UnityEngine;

public class GIObjPool : MonoBehaviour
{
    public GameObject goPrefab;

    public void Start()
    {
        StartCoroutine(CreatePool());
    }

    IEnumerator CreatePool()
    {
        int i = 0;
        while(i++ < 10000)
        {
            yield return null;
            for (int j = 0; j < 100; j++)
            {
                GameObject go = Instantiate(goPrefab);
                go.SetActive(true);
            }
        }
        Debug.Log("全部创建完毕");
    }
}
