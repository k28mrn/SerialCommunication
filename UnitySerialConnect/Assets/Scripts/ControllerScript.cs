using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerScript : MonoBehaviour
{
    [SerializeField]
    SerialHandler handler;
    // Start is called before the first frame update
    void Start()
    {
        //受信用メソッドを追加
        handler.OnDataReceived += OnDataReceived;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) {
            handler.Write("D");
        } else if (Input.GetKeyDown(KeyCode.U)) {
            handler.Write("U");
        }
    }

    ///<summary>
    /// シリアルデータ受信
    ///</summary>
    void OnDataReceived(string message) {
        // Arduinoから受信したメッセージをとりあえずログに表示
        Debug.Log("Get Message : " + message);
    }
}
