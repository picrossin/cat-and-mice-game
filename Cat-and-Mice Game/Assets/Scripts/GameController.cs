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
        System.Array foodTypes = System.Enum.GetValues(typeof(FoodType));
        for (int i = 0; i < 3; i++)
        {
            FoodType newItem = (FoodType)foodTypes.GetValue(Random.Range(0, foodTypes.Length));
            while (m_ShoppingList.Contains(newItem))
            {
                newItem = (FoodType)foodTypes.GetValue(Random.Range(0, foodTypes.Length));
            }
            m_ShoppingList.Add(newItem);
        }
    }

    void Update()
    {
        if (m_Collected.Count == m_ShoppingList.Count)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
