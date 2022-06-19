using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPrefabPair
{
    public GameObject Button { get; set; }
    public GameObject Prefab { get; set; }
    public string Price { get; set; }
    public string CompanyName { get; set; }


    public ButtonPrefabPair()
    {
    }

    public ButtonPrefabPair(GameObject button, GameObject prefab)
    {
        Price = "0.0";
        CompanyName = "";
        Button = button;
        Prefab = prefab;
    }

    public ButtonPrefabPair(GameObject button, GameObject prefab, string price, string companyName)
    {
        Button = button;
        Prefab = prefab;
        Price = price;
        CompanyName = companyName;
    }
}
