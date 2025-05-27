using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class RebindKey : MonoBehaviour
{
    public string actionName;
    public TextMeshProUGUI buttonText;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(StartRebinding);
        UpdateButtonLabel();
    }

    void StartRebinding()
    {
        StartCoroutine(WaitForKey());
    }

    IEnumerator WaitForKey()
    {
        buttonText.text = "Press any key...";
        yield return null;

        bool keySet = false;
        while (!keySet)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    SetNewKey(key);
                    keySet = true;
                    break;
                }
            }
            yield return null;
        }
    }

    void SetNewKey(KeyCode newKey)
    {
        var bindings = InputManager.Instance.keyBindings;
        switch (actionName)
        {
            case "moveForward": bindings.moveForward = newKey; break;
            case "moveBackward": bindings.moveBackward = newKey; break;
            case "moveLeft": bindings.moveLeft = newKey; break;
            case "moveRight": bindings.moveRight = newKey; break;
            case "shoot": bindings.shoot = newKey; break;
            case "reload": bindings.reload = newKey; break;
            case "interact": bindings.interact = newKey; break;

        }

        InputManager.Instance.SaveKeyBindings();
        UpdateButtonLabel();
    }

    void UpdateButtonLabel()
    {
        var bindings = InputManager.Instance.keyBindings;
        KeyCode key = KeyCode.None;

        switch (actionName)
        {
            case "moveForward": key = bindings.moveForward; break;
            case "moveBackward": key = bindings.moveBackward; break;
            case "moveLeft": key = bindings.moveLeft; break;
            case "moveRight": key = bindings.moveRight; break;
            case "shoot": key = bindings.shoot; break;
            case "reload": key = bindings.reload; break;
            case "interact": key = bindings.interact; break;
        }

        buttonText.text = $"{actionName}: {key}";
    }
}
