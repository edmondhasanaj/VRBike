using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ServerUtils
{
    public static IEnumerator SendHTTPPostRequestEnum(string ip, Action<long> onHTTPReply, params KeyValuePair<string, string>[] postValues)
    {
        WWWForm form = new WWWForm();

        //Add all post data to the form
        foreach(KeyValuePair<string, string> kvp in postValues)
            form.AddField(kvp.Key, kvp.Value);

        using (UnityWebRequest www = UnityWebRequest.Post(ip, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                onHTTPReply(404);
            }
            else
            {
                onHTTPReply(www.responseCode);
            }
        }
    }
}
