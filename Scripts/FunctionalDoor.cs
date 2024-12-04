using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionalDoor : MonoBehaviour
{
    // types:
    // 0: vertical door
    public int type = 0;
    public float speed = 10f;
    public float variable = 0f;

    private StateMachine state;
    private Vector3 defaultPos;
    private Vector3 targetPos;

    // Start is called before the first frame update
    void Start()
    {
        defaultPos = transform.position;
        targetPos = defaultPos;
        targetPos.y += variable;
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
                transform.position = Vector3.MoveTowards(transform.position, defaultPos, speed * Time.deltaTime);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            }
        }
    }
}
