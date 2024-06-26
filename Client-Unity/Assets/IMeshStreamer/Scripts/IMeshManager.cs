using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class IMeshManager : MonoBehaviour
{
    [SerializeField] public StreamHandler streamHandler;
    [SerializeField] public StreamContainer streamContainer;
    [SerializeField] public StreamPlayer streamPlayer;
    [SerializeField] public InputField streamDebugger;

    public void Debug(string message)
    {
        streamDebugger.text += message + "\n";
    }

}
