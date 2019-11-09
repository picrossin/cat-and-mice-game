using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact(UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController script);
    void Action(UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController script);
}