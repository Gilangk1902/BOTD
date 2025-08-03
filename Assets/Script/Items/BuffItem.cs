using UnityEngine;

public abstract class BuffItem : MonoBehaviour, InteractPrompt
{
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    private Transform player;

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    protected virtual void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= interactionDistance && Input.GetKeyDown(InputManager.Instance.keyBindings.interact))
        {
            ApplyBuff(player.gameObject);
            Destroy(gameObject);
        }
    }

    protected abstract void ApplyBuff(GameObject player);

    public string GetPromptText()
    {
        return "Pick Up Item";
    }
}
