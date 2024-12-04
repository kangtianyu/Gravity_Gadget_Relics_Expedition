using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableSwitch : MonoBehaviour
{
    public Vector3 attachOffsets = Vector3.zero;
    public Transform anchor = null;

    private bool characterInRange = false;
    private bool attachToCharacter = false;

    // Update is called once per frame
    void Update()
    {
        // "F" is pressed
        if (characterInRange && Input.GetButtonUp("Interact"))
        {
            attachToCharacter = !attachToCharacter;
            // make player controller not react to it's own velocity change
            PlayerController.Instance.isAttached = attachToCharacter;
        }

        // Switch is attached to the player controlled character
        if (attachToCharacter)
        {
            // force change the position and rotation of player controlled character
            PlayerController.Instance.transform.position = anchor.position + attachOffsets;
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(anchor.forward.x, 0, anchor.forward.z));
            PlayerController.Instance.playerAvator.transform.rotation = targetRotation;
            PlayerController.Instance.ResetVelocity();

            // if gadgit is attached to player and in high gravity mode, open the switch
            if (PlayerController.Instance.gadget.attachedObject == PlayerController.Instance.gameObject && PlayerController.Instance.gadget.mode > 5)
            {
                attachToCharacter = false;
                PlayerController.Instance.isAttached = false;
                GetComponent<LabeledObject>().label = "<#000000><s>完全损坏的拉杆</s>";
                transform.GetChild(2).gameObject.SetActive(true);
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerController.Instance.gameObject)
        {
            characterInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerController.Instance.gameObject)
        {
            characterInRange = false;
        }
    }
}
