using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingList : MonoBehaviour
{
    public GameController controller;

    void Update()
    {
        Text instruction = GetComponent<Text>();
        string list = "To steal:\n";
        foreach (FoodType food in controller.m_ShoppingList.Except(controller.m_Collected))
        {
            list += food.ToString() + "\n";
        }
        instruction.text = list;
    }
}
