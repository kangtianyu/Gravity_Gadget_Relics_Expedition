using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform eyeTransform;
    public float searchRange = 10f;
    public float speed = 4f;
    public float timeToReinstateFromSuppressed = 5f;
    public string killSentence = "";
    public bool isEnabled = true;

    private StateMachine state;
    private Rigidbody rb;
    private MoveableObject info;
    private bool isSuppressed = false;
    private float reinstateTimer = 0f;
    private DestroyableEnemy enemyInfo;
    private UnityEngine.AI.NavMeshPath path;               // The path to follow.
    private int currentPathIndex = 0;       // Current path index we are navigating.

    // Start is called before the first frame update
    void Start()
    {
        state = GetComponent<StateMachine>();
        if (state == null)
        {
            Debug.Log($"{name} State is not found.");
        }

        if (state.state == 1)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.Log($"{name} RigidBody is not found.");
            }

            info = GetComponent<MoveableObject>();
            if (info == null)
            {
                Debug.Log($"{name} Moveable Object is not found.");
            }
        }
        else if (state.state == 2)
        {
            enemyInfo = GetComponent<DestroyableEnemy>();
            if (enemyInfo == null)
            {
                Debug.Log($"{name} Enemy Info is not found.");
            }
        }
        else if (state.state == 3)
        {
            path = new UnityEngine.AI.NavMeshPath();
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.Log($"{name} RigidBody is not found.");
            }

            info = GetComponent<MoveableObject>();
            if (info == null)
            {
                Debug.Log($"{name} Moveable Object is not found.");
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isEnabled)
        {
            if (state.state == 1)
            {
                if (info.isAttached)
                {
                    if (info.gadget.mode == 0)
                    {
                        info.modifier = "漂浮";
                        info.gravity = 0;
                        info.currentVelocity.y += 0.01f;
                        rb.isKinematic = false;
                    }
                    else if (info.gadget.mode > 1)
                    {
                        info.modifier = "压制";
                        info.gravity = PlayerController.Instance.gravity * info.gadget.mode;
                        info.currentVelocity = Vector3.zero;
                        isSuppressed = true;
                        reinstateTimer = timeToReinstateFromSuppressed;
                        rb.isKinematic = false;
                    }
                    else
                    {
                        info.modifier = "";
                        rb.isKinematic = true;
                    }
                }
                else
                {
                    info.modifier = "";
                    rb.isKinematic = true;
                }

                if (characterInRange() && !info.isDestroyed && !isSuppressed)
                {
                    // unfreeze rigidbody
                    info.ResetStable();

                    if (rb.isKinematic)
                    {
                        // move to player position
                        rb.MovePosition(Vector3.MoveTowards(transform.position, PlayerController.Instance.transform.position, speed * Time.deltaTime));
                    }

                    // face to player
                    Vector3 direction = PlayerController.Instance.transform.position - transform.position;
                    float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    Vector3 eulerAngle = new Vector3(0, angle, 0);
                    rb.MoveRotation(Quaternion.Euler(eulerAngle));

                    //Debug.Log($"{name}: Player in Range");

                    // Slay player if close enough
                    if (Vector3.Distance(transform.position, PlayerController.Instance.transform.position) < 15f)
                    {
                        PlayerController.Instance.currentLevel.currentArea.RestartArea(killSentence);
                    }
                }

            }
            else if (state.state == 2)
            {
                //Debug.Log($"{name}: distance {Vector3.Distance(transform.position, PlayerController.Instance.transform.position)}");
                if (!enemyInfo.isDestroyed && characterInRange())
                {
                    PlayerController.Instance.currentLevel.currentArea.RestartArea(killSentence);
                }
            }
            else if (state.state == 3)
            {

                UnityEngine.AI.NavMesh.CalculatePath(transform.position, PlayerController.Instance.transform.position, UnityEngine.AI.NavMesh.AllAreas, path);

                // unfreeze rigidbody
                info.ResetStable();

                if (path.corners.Length > 1)
                {
                    Vector3 targetPosition = path.corners[currentPathIndex];

                    // Move the enemy towards the current corner of the path
                    Vector3 direction = (targetPosition - transform.position + new Vector3(0, 1.0f, 0)).normalized;

                    // Move using Rigidbody to apply physics
                    rb.velocity = direction * speed;

                    // Rotate towards the direction we're moving
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 200f * Time.deltaTime);

                    // If we're close enough to the current path corner, move to the next one
                    if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
                    {
                        currentPathIndex++;
                    }
                }
            }
        }
        


        if (reinstateTimer > 0)
        {
            reinstateTimer -= Time.deltaTime;
        }
        else
        {
            isSuppressed = false;
        }
    }

    private bool characterInRange()
    {
        float distance = Vector3.Distance(eyeTransform.position, PlayerController.Instance.headTransform.position);
        if (distance < searchRange)
        {
            RaycastHit hit;
            Vector3 direction = PlayerController.Instance.headTransform.position - eyeTransform.position;
            direction.Normalize();
            Ray ray = new Ray(eyeTransform.position, direction);

            // Raycast to check if something is blocking the view
            int currentHit = 0;
            while (Physics.Raycast(ray, out hit, distance) && currentHit++ < 10)
            {
                //Debug.Log($"{name} {currentHit} hit {hit.collider.gameObject} at {hit.point}");
                //ignore self
                if (hit.collider.gameObject == this.gameObject)
                {
                    ray = new Ray(hit.point + direction * 0.01f, direction);
                    distance = Vector3.Distance(hit.point, PlayerController.Instance.headTransform.position);
                    continue;
                }
                else
                {
                    // If the ray hit an object, check if it's the target object
                    if (hit.collider.gameObject == PlayerController.Instance.transform.GetChild(0).gameObject)
                    {
                        //Debug.Log($"{hit.collider.gameObject}");
                        return true; // The object is visible, no occlusion
                    }
                }
                return false;
            }

        }
        return false;
    }
}
