using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FoodType
{
    Cheese
}

public class Interactable : MonoBehaviour, IInteractable
{
    public FoodType m_FoodType;
    public bool m_Held = false;

    private Rigidbody m_ThisRigidbody = null;
    private FixedJoint m_HoldJoint = null;


    private void Start()
    {
        gameObject.tag = "Interactable";
        m_ThisRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // If the holding joint has broken, drop the object
        if (m_HoldJoint == null && m_Held == true)
        {
            m_Held = false;
            m_ThisRigidbody.useGravity = true;
        }
    }

    // Pick up the object, or drop it if it is already being held
    public void Interact(UnityStandardAssets.Characters.FirstPerson.MouseController playerScript)
    {
        // Is the object currently being held?
        if (m_Held)
        {
            Drop();
        }
        else
        {
            m_Held = true;
            m_ThisRigidbody.useGravity = false;

            m_HoldJoint = playerScript.m_HandTransform.gameObject.AddComponent<FixedJoint>();
            m_HoldJoint.breakForce = 1000f; // Play with this value
            m_HoldJoint.connectedBody = m_ThisRigidbody;

        }
    }

    // Throw the object
    public void Action(UnityStandardAssets.Characters.FirstPerson.MouseController playerScript)
    {
        // Is the object currently being held?
        if (m_Held)
        {
            Drop();
            Vector3 forceDir = playerScript.cam.transform.forward;
            m_ThisRigidbody.AddForce(forceDir * playerScript.movementSettings.m_ThrowForce);
        }
    }

    // Drop the object
    private void Drop()
    {
        m_Held = false;
        m_ThisRigidbody.useGravity = true;

        m_HoldJoint.connectedBody = null;
    }
}
