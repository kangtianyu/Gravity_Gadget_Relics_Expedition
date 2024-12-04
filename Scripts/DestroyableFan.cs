using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableFan : MonoBehaviour
{
    // When first collides with another object
    private void OnCollisionEnter(Collision collision)
    {
        MoveableObject info = collision.gameObject.GetComponent<MoveableObject>();
        if (info != null)
        {
            if (info.ObjectMass > 1)
            {
                transform.GetChild(1).GetComponent<AutoRotate>().isRotate = false;
                GetComponent<Collider>().isTrigger = true;
                collision.gameObject.GetComponent<MoveableObject>().Deactive();
                GetComponent<LabeledObject>().label = "<#000000><s>∆∆∑Á…»</s>";
            }
        }
    }
}
