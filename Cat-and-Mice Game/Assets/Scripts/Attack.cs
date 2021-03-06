﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public int m_PlayerNumber = 3;
    public GameObject explosion;
    private bool inRange = false;
    private bool canAttack = true;
    private Collider mouseCollision;

    private void OnTriggerEnter(Collider collision)
    {
        mouseCollision = collision;
        inRange = true;
    }
    private void OnTriggerExit(Collider collision)
    {
        inRange = false;
    }

    public void Init(int playerNumber)
    {
        m_PlayerNumber = playerNumber;
    }

    void Update()
    {
        if (inRange)
        {
            if (Input.GetButtonDown("Hold" + m_PlayerNumber) && canAttack)
            {
                Debug.Log("Attempting attack");
                if (mouseCollision.gameObject.tag == "Mouse")
                {
                    Debug.Log("Attack landed");
                    UnityStandardAssets.Characters.FirstPerson.MouseController mouse =
                        mouseCollision.gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseController>();
                    mouse.playerHealth--;
                    canAttack = false;
                    Instantiate(explosion, mouse.transform.position, Quaternion.identity);
                }
            }
            if (Input.GetButtonUp("Hold" + m_PlayerNumber))
            {
                canAttack = true;
            }
        }
        
    }
}
