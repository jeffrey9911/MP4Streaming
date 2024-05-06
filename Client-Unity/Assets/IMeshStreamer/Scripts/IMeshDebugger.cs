using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IMeshDebugger : MonoBehaviour
{
    public TMP_InputField inputField;

    public void Debug(string message)
    {
        inputField.text += message + "\n";
    }
}
