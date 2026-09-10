using TMPro;
using UnityEngine;

public class MobileInputField : MonoBehaviour
{
    private TMP_InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        inputField.onSelect.AddListener(OpenKeyboard);
    }

    private void OnDisable()
    {
        inputField.onSelect.RemoveListener(OpenKeyboard);
    }

    private void OpenKeyboard(string text)
    {
        inputField.ActivateInputField();
    }
}