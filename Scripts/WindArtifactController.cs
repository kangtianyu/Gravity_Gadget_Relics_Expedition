using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindArtifactController : MonoBehaviour
{
    public List<GameObject> fanList;
    public List<GameObject> doorList;
    public List<GameObject> GolemList;

    private StateMachine state;
    private MoveableObject objectInfo;

    // Start is called before the first frame update
    void Start()
    {
        state = GetComponent<StateMachine>();
        if (state == null)
        {
            Debug.Log($"{name} state is not found.");
        }

        objectInfo = GetComponent<MoveableObject>();
        if (objectInfo == null)
        {
            Debug.Log($"{name} Moveable Object is not found.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!objectInfo.isAttached && Input.GetButtonDown("Interact") && Vector3.Distance(transform.position, PlayerController.Instance.transform.position) < 8f)
        {
            if (state.state == 2)
            {
                state.state = 0;
                trigger();
                GetComponent<MoveableObject>().isActive = true;
                GetComponent<LabeledObject>().label = "平静的古代造物";
            }
            else if (state.state == 1)
            {
                state.state = 0;
                GetComponent<LabeledObject>().label = "平静的古代造物";
            }
            else
            {
                state.state = 1;
                GetComponent<LabeledObject>().label = "被风环绕的古代造物";
            }
        }

        if (state.state == 1)
        {
            blow();
        }
    }


    private void trigger()
    {
        foreach (GameObject fan in fanList)
        {
            if (fan.transform.GetChild(1).GetComponent<AutoRotate>().isRotate)
            {
                fan.transform.GetChild(1).GetComponent<AutoRotate>().isRotate = false;
                fan.GetComponent<Collider>().isTrigger = true;
                LabeledObject l = fan.GetComponent<LabeledObject>();
                if (l != null)
                {
                    l.label = "<#000000><s>停下的风扇</s>";
                }
            }
        }
        foreach (GameObject door in doorList)
        {
            if (door.GetComponent<StateMachine>().state == 0)
            {
                door.GetComponent<StateMachine>().state = 1;
            }
        }
        foreach (GameObject golem in GolemList)
        {
            if (!golem.GetComponent<DestroyableEnemy>().isDestroyed)
            {
                golem.GetComponent<DestroyableEnemy>().ProcessDestroy();
            }
        }

        List<GameObject> moveableObjects = PlayerController.Instance.currentLevel.GetAllMoveableObjects();
        foreach (GameObject moveableObject in moveableObjects)
        {
            EnemyAI eai = moveableObject.GetComponent<EnemyAI>();
            if (eai != null)
            {
                StateMachine sm = moveableObject.GetComponent<StateMachine>();
                if (sm != null)
                {
                    if(sm.state == 3)
                    {
                        eai.isEnabled = true;
                        eai.speed = 5;
                    }
                }
            }
        }


    }

    private void blow()
    {
        List<GameObject> moveableObjects = PlayerController.Instance.currentLevel.GetAllMoveableObjects();
        foreach (GameObject moveableObject in moveableObjects)
        {
            if (moveableObject != gameObject)
            {
                MoveableObject ObjectInfo = moveableObject.GetComponent<MoveableObject>();
                if (ObjectInfo != null)
                {
                    ObjectInfo.gravity = 0f;
                    ObjectInfo.AdditionalVelocity(new Vector3(10f, 1f, 0f));
                }

                EnemyAI eai = moveableObject.GetComponent<EnemyAI>();
                if (eai != null)
                {
                    StateMachine sm = moveableObject.GetComponent<StateMachine>();
                    if (sm != null)
                    {
                        if (sm.state == 3)
                        {
                            eai.isEnabled = false;
                        }
                    }
                }
            }
        }
        if (PlayerController.Instance.gadget.attachedObject != PlayerController.Instance.gameObject || PlayerController.Instance.gadget.mode <= 1)
        {
            PlayerController.Instance.AdditionalVelocity(new Vector3(1f, 0.1f, 0f));
        }
    }
}
