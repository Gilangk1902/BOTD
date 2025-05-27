using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    private Transform lightsRoot;
    private Transform roomRoot;
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
