using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public List<FoodType> m_Collected;
    public List<FoodType> m_ShoppingList;

    // Start is called before the first frame update
    void Start()
    {
        m_Collected = new List<FoodType>();
        m_ShoppingList = new List<FoodType>();
        m_ShoppingList.Add(FoodType.Cheese);
    }

    void Update()
    {
        if (m_Collected.Count == m_ShoppingList.Count)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
