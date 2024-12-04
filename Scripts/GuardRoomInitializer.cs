using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardRoomInitializer : MonoBehaviour, AreaInitializer
{

    public List<GameObject> mainEnemy;
    public GameObject moveObj1;
    public GameObject moveObj2;

    private MoveableObjectState mem1;
    private MoveableObjectState mem2;
    // Start is called before the first frame update
    void Start()
    {
        mem1 = new MoveableObjectState
        {
            name = moveObj1.name,
            position = moveObj1.transform.position,
            rotation = moveObj1.transform.rotation,
            isActive = moveObj1.activeSelf
        };
        mem2 = new MoveableObjectState
        {
            name = moveObj2.name,
            position = moveObj2.transform.position,
            rotation = moveObj2.transform.rotation,
            isActive = moveObj2.activeSelf
        };
    }

    public void init()
    {
        Debug.Log($"{name} restart");
        moveObj1.GetComponent<MoveableObject>().Initialize(mem1.position, mem1.rotation, mem1.isActive);
        moveObj2.GetComponent<MoveableObject>().Initialize(mem2.position, mem2.rotation, mem2.isActive);

        foreach (GameObject golem in mainEnemy)
        {
            DestroyableEnemy dm = golem.GetComponent<DestroyableEnemy>();
            dm.isDestroyed = false;
            golem.GetComponent<LabeledObject>().label = "眼睛发光的巨大石像";
        }

    }
}
