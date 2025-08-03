using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public KeyBindings keyBindings = new KeyBindings();
    public float mouseSensitivity = 120f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveKeyBindings()
    {
        PlayerPrefs.SetString("KeyBindings", JsonUtility.ToJson(keyBindings));
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("KeyBindings"))
        {
            var loaded = JsonUtility.FromJson<KeyBindings>(PlayerPrefs.GetString("KeyBindings"));

            // fallback default
            if (loaded.shoot == KeyCode.None) loaded.shoot = KeyCode.Mouse0;
            if (loaded.reload == KeyCode.None) loaded.reload = KeyCode.R;
            if (loaded.interact == KeyCode.None) loaded.interact = KeyCode.E;

            keyBindings = loaded;
        }

        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 120f);
    }
}


[System.Serializable]
public class KeyBindings
{
    public KeyCode moveForward = KeyCode.W;
    public KeyCode moveBackward = KeyCode.S;
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode shoot = KeyCode.Mouse0;
    public KeyCode reload = KeyCode.R;
    public KeyCode interact = KeyCode.E;
}
