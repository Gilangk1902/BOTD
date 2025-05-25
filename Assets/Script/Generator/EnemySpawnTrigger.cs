using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    private bool hasTriggered = false;
    private Transform roomRoot;
    private Transform lightsRoot;

    private void Start()
    {
        roomRoot = transform.parent;
        if (roomRoot != null)
        {
            lightsRoot = roomRoot.Find("Lights");
            // Matikan semua torch di awal
            if (lightsRoot != null)
                DeactivateRoomLights();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ActivateRoomLights();

        if (roomRoot != null && roomRoot.name.Contains("Start"))
        {
            Debug.Log("Start room detected, enemy will not spawn.");
            return;
        }

        if (!hasTriggered)
        {
            hasTriggered = true;

            RoomEncounter encounter = roomRoot.GetComponent<RoomEncounter>();
            if (encounter != null)
            {
                encounter.StartEncounter();
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        DeactivateRoomLights();
    }

    private void ActivateRoomLights()
    {
        if (lightsRoot == null) return;

        foreach (Transform lightGroup in lightsRoot)
        {
            foreach (ParticleSystem ps in lightGroup.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.gameObject.SetActive(true);
                ps.Play();
            }

            foreach (Light l in lightGroup.GetComponentsInChildren<Light>(true))
            {
                l.enabled = true;
            }
        }
    }

    private void DeactivateRoomLights()
    {
        if (lightsRoot == null) return;

        foreach (Transform lightGroup in lightsRoot)
        {
            foreach (ParticleSystem ps in lightGroup.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.gameObject.SetActive(false);
            }

            foreach (Light l in lightGroup.GetComponentsInChildren<Light>(true))
            {
                l.enabled = false;
            }
        }
    }
}
