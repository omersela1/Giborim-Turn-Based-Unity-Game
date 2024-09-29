using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    public GameObject frame;

    public void OnMouseDown()
    {
        
        Debug.Log("Inventory slot clicked: " + gameObject.name);
        if (frame != null)
        {
            frame.SetActive(!frame.activeSelf);
        }
    }

}
