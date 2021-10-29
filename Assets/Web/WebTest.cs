using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine.Networking;
using System.Text;

public class WebTest : MonoBehaviour
{
    public enum LoginType : uint
    {
        YouKe = 0,
        MailAccount = 1,
        PhoneNumber = 2,
        FaceBook = 3,
        WeChat = 4,
    }

    private string url = "https://localhost:5001/LoginVerificationHandler/RequestVerificationCode";

    private void Start()
    {
        StartCoroutine(Request());
    }

    IEnumerator Request()
    {
        WWWForm wWWForm = new WWWForm();
        wWWForm.AddField("nLoginType", (int)LoginType.MailAccount);
        //wWWForm.AddField("UniqueIdentifier", "xuke825126369@gmail.com");
        wWWForm.AddField("UniqueIdentifier", "1426186059@qq.com");
        UnityWebRequest www = UnityWebRequest.Post(url, wWWForm);
        www.SendWebRequest();
        while (!www.isDone)
        {
            yield return null;
        }

        if (www.isHttpError || www.isNetworkError)
        {
            Debug.LogError(www.error + " : " + www.responseCode);
        }
        else
        {
            Debug.Log("Success : " + www.downloadHandler.text);
        }
    }
}

