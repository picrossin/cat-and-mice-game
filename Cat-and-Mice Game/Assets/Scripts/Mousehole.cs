using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mousehole : MonoBehaviour
{
    public GameController m_GameController;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Interactable")
        {
            FoodType item = collision.gameObject.GetComponent<Interactable>().m_FoodType;
            if (m_GameController.m_ShoppingList.Any(i => i == item))
            {
                m_GameController.m_Collected.Add(item);
            }
            Destroy(collision.gameObject);
        }
    }
}
