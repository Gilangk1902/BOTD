using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Branch : MonoBehaviour 
{
    public Connector connectedOn;

    public void Init(Connector connectedOn)
    {
        this.connectedOn = connectedOn; 
    }

    public void RevertConnector()
    {
        if (connectedOn != null)
        {
            connectedOn.isConnected = false;
        }
    }
}
