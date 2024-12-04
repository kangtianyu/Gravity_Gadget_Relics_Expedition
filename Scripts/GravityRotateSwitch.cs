using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityRotateSwitch : MonoBehaviour
{
    // types:
    // 0: vertical door
    public int type = 0;
    public float speed = 200f;
    public float variable = 0f;
    public Vector3 attachOffsets = Vector3.zero;
    public Transform anchor = null;
    public List<StateMachine> linkedDevices = new List<StateMachine>();

    private StateMachine state;
    private Quaternion defaultRotation;
    private Quaternion targetRotation;
    private bool characterInRange = false;
    private bool attachToCharacter = false;

    // Start is called before the first frame update
    void Start()
    {
        defaultRotation = transform.rotation;
        //Vector3 tmpVec = transform.TransformPoint();
        targetRotation = Quaternion.Euler(transform.eulerAngles.x + variable, transform.eulerAngles.y, transform.eulerAngles.z);
        state = GetComponent<StateMachine>();
        if (state == null)
        {
            Debug.Log($"{name} state is not found.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (type == 0)
        {
            // states:
            // 0: close
            // 1: open
            if (state.state == 0)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, defaultRotation, speed * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
            }

            foreach (StateMachine stateMachine in linkedDevices)
            {
                stateMachine.state = state.state;
            }
        }

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
                state.state = 1;
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
