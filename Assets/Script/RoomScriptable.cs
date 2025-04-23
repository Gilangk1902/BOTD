using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RoomScriptable", menuName = "Generator/PrefabScriptable/RoomScriptable")]
public class RoomScriptable : ScriptableObject
{
    public GameObject prefab;
    public float weight = 1f;
}
