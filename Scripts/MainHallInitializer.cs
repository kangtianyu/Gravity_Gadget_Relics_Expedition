using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainHallInitializer : MonoBehaviour, AreaInitializer
{
    public GameObject mainEnemy;
    public GameObject fan;

    private MoveableObjectState mem;
    // Start is called before the first frame update
    void Start()
    {
        mem = new MoveableObjectState
        {
            name = mainEnemy.name,
            position = mainEnemy.transform.position,
            rotation = mainEnemy.transform.rotation,
            isActive = mainEnemy.activeSelf
        };
    }

    public void init()
    {
        mainEnemy.GetComponent<MoveableObject>().Initialize(mem.position, mem.rotation, mem.isActive);


        fan.transform.GetChild(1).GetComponent<AutoRotate>().isRotate = true;
        fan.GetComponent<Collider>().isTrigger = false;
        fan.GetComponent<LabeledObject>().label = "发出异响的破风扇";
    }
}
