using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationObject : MonoBehaviour
{

    public BuildingController ConnectedBuilding { get; private set; }
    public bool IsConnected { get; private set; } = false;
    public string Type;

    private void Awake()
    {
    }

    public void Show(bool value)
    {
        gameObject.SetActive(value);
    }

    public void Connect(BuildingController BuildingController)
    {
        ConnectedBuilding = BuildingController;
        IsConnected = true;
    }

    public void Disconnect()
    {
        ConnectedBuilding = null;
        IsConnected = false;
    }
}