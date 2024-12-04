using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableEnemy : MonoBehaviour
{
    public string labelAfterDestroy;
    public bool isDestroyed = false;

    public void ProcessDestroy()
    {
        isDestroyed = true;
        GetComponent<LabeledObject>().label = labelAfterDestroy;
    }

    // When first collides with another object
    private void OnCollisionEnter(Collision collision)
    {
        MoveableObject info = collision.gameObject.GetComponent<MoveableObject>();
        if (info != null)
        {
            if (info.ObjectMass > 1 && info.currentVelocity.magnitude > 30f)
            {
                collision.gameObject.GetComponent<MoveableObject>().Deactive();
                ProcessDestroy();
            }
        }
    }
}
