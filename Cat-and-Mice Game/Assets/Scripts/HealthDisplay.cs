using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public UnityStandardAssets.Characters.FirstPerson.MouseController mouse;

    void Update()
    {
        Text instruction = GetComponent<Text>();
        instruction.text = mouse.playerHealth.ToString();
    }
}
