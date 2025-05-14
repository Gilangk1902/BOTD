using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Loot/Item Data")]
public class ItemData : ScriptableObject
{
    public GameObject prefab;
    public float weight = 1f;
}
