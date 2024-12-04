using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveableObject : MonoBehaviour
{
    public float ObjectMass = 1f;
    public float gravity = 10f;
    public Vector3 currentVelocity = Vector3.zero;
    public string label = "Moveable Object";
    public bool isSelected = false;
    public bool isDestroyed = false;
    public bool isAttached = false;
    public GadgetInstance gadget = null;
    public string modifier = "";
    public bool canDestroy = true;
    public bool isActive = true;

    private Rigidbody rb;
    private Renderer objectRenderer;
    private Vector3 positionLastFrame;
    private int stableCount = 0;
    private Vector3 forceMovePos = Vector3.zero;
    private Vector3 lastValidPos;

    private Vector3 initPosition;
    private Quaternion initRotation;
    private bool initActive;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.tag = "MoveableObject";

        rb = GetComponent<Rigidbody>();
        if (rb == null) 
        {
            // Add a Rigidbody component to the GameObject
            rb = gameObject.AddComponent<Rigidbody>();

            // Optional: Set Rigidbody properties
            rb.mass = ObjectMass; // Set mass
            rb.drag = 0f; // Set drag
            rb.angularDrag = 0f; // Set angular drag
            rb.useGravity = false; // Disable default gravity
        }

        objectRenderer = GetComponent<Renderer>();
        positionLastFrame = transform.position;

        lastValidPos = transform.position;

        initPosition = transform.position;
        initRotation = transform.rotation;
        initActive = gameObject.activeSelf;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            isSelected = CheckSelected();

            lastValidPos = transform.position;
        }
        //Debug.Log($"{stableCount}");
    }

    // FixedUpdate is called once per physics system frame
    void FixedUpdate()
    {

        if (isActive)
        {
            if (isAttached && !gadget.synchronizeMoveMode)
            {
                ResetStable();
            }

            currentVelocity = (transform.position - positionLastFrame) / Time.deltaTime;
            positionLastFrame = transform.position;


            if (forceMovePos != Vector3.zero)
            {
                rb.MovePosition(forceMovePos);
                forceMovePos = Vector3.zero;
            }
            else
            {
                UpdateVelocity();
            }

            lastValidPos = transform.position;
        }

        if (Vector3.Distance(transform.position, Vector3.zero) > 10000f)
        {
            Deactive();
        }
    }

    public void ResetStable()
    {
        stableCount = 0;
        // Unfreeze all position and rotation constraints
        rb.constraints = RigidbodyConstraints.None;
    }

    public void ForceMove(Vector3 pos)
    {
        forceMovePos = pos;
    }

    public void FreezeRotation()
    {
        stableCount = 0;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void AdditionalVelocity(Vector3 v)
    {
        ResetStable();
        if (rb == null || rb.isKinematic)
        {
            currentVelocity += v;
        }
        else
        {
            rb.velocity += v;
        }
    }

    public void Deactive()
    {
        if (isAttached)
        {
            gadget.attachedObject = gadget.gameObject;
            gadget.indicator.transform.SetParent(null);
            gadget.state = 2;
        }
        currentVelocity = Vector3.zero;
        isDestroyed = false;
        isAttached = false;
        gadget = null;
        modifier = "";
        gameObject.SetActive(false);
    }

    public void Initialize(Vector3 position, Quaternion rotation, bool isActive)
    {
        transform.position = initPosition;
        transform.rotation = initRotation;
        gameObject.SetActive(initActive);
    }

    // Check if the velocity is close to zero
    private bool IsStable()
    {
        bool isStable = currentVelocity.magnitude < 0.01f;

        if (isStable)
        {
            stableCount++;
        }
        else
        {
            stableCount = 0;
        }
        return isStable;
    }

    private void UpdateVelocity()
    {
        // update new velocity with gravity
        if (rb.isKinematic)
        {

        }
        else
        {
            currentVelocity = rb.velocity;
        }
        // Convert the local direction to world space based on the Rigidbody's rotation
        Vector3 worldGravityDirection = Vector3.down;

        bool isStable = IsStable();
        if (isStable)
        {
            if (objectRenderer != null)
            {
                objectRenderer.material.color = Color.yellow;
            }

            currentVelocity = worldGravityDirection * gravity * Time.deltaTime;

            if (rb.isKinematic)
            {
                //rb.MovePosition(transform.position + currentVelocity * Time.deltaTime);
            }
            else
            {
                rb.velocity = currentVelocity;
            }

            if (stableCount > 50)
            {
                // Freeze position on all axes
                rb.constraints = RigidbodyConstraints.FreezePosition;

                // Freeze rotation on all axes
                rb.constraints |= RigidbodyConstraints.FreezeRotation;
            }

        }
        else
        {
            if (objectRenderer != null)
            {
                objectRenderer.material.color = Color.red;
            }

            currentVelocity += worldGravityDirection * gravity * Time.deltaTime;

            if (rb.isKinematic)
            {
                //rb.MovePosition(transform.position + currentVelocity * Time.deltaTime);
            }
            else
            {
                rb.velocity = currentVelocity;
            }
        }
    }

    // When first collides with another object
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log($"Collided with: {collision.gameObject.name} at speed {currentVelocity.magnitude}{currentVelocity}");
        if(canDestroy && currentVelocity.magnitude > 30)
        {
            isDestroyed = true;
            //Debug.Log($"{name} destoried by collision at speed {currentVelocity.magnitude}{currentVelocity}.");
        }

        transform.position = lastValidPos;
    }

    private bool CheckSelected()
    {
        return UIManager.Instance.selectedObjectList.Contains(this.gameObject);
    }

}
