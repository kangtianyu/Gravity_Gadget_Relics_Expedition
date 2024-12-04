using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;  // Static reference to access PlayerController globally

    public CinemachineFreeLook freeLookCamera;
    public Camera playerCamera;
    public GameObject playerAvator;
    public Animator animator;
    public GadgetInstance gadget;
    public LevelMaster currentLevel;
    public Transform headTransform;

    public float speed = 5.0f;
    public float rotationSpeed = 1000.0f;
    public float jumpForce = 4.0f;
    public float gravity = 10.0f;
    public bool isAttached = false;
    public bool isBusy = false;

    public Vector3 testVector = Vector3.zero;

    private CharacterController controller;
    private Vector3 velocity = Vector3.zero;
    private Quaternion faceRotation;
    private bool rotating = false;
    private bool jumping = false;
    private Vector3 surfaceNormal = Vector3.up;
    private float surfaceAngle = 0f;
    private Vector3 moveDirection = Vector3.zero;
    private bool isOnGround = true;
    private bool isRespawning = false;
    private Vector3 respawnPosition = Vector3.zero;

    void Start()
    {
        // Ensure only one instance of PlayerController exists (Singleton pattern)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Makes it persistent across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one PlayerController exists
        }

        // Ensure the cursor starts locked at the center
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; // Hide cursor on start

        // Initialize
        controller = GetComponent<CharacterController>();
        Vector3 faceVector = playerCamera.transform.forward;
        faceVector.y = 0;
        faceRotation = Quaternion.LookRotation(faceVector);
    }

    void Update()
    {
        // Check if the Left Alt key is held down
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.None; // Unlock the cursor
            freeLookCamera.gameObject.SetActive(false); // Disable free look
            Cursor.visible = true; // Show the cursor
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to center
            freeLookCamera.gameObject.SetActive(true); // Enable free look
            Cursor.visible = false; // Hide the cursor
        }


        if (isRespawning)
        {
            ResetVelocity();
            if (transform.position == respawnPosition)
            {
                isRespawning = false;
                controller.enabled = true;
                //Debug.Log($"Respawn to {respawnPosition}");
                return;
            }
            transform.position = respawnPosition;
            return;
        }

        if (isAttached || isBusy)
        {
            animator.SetBool("isWalk", false);
        }
        else
        {
            // Apply gravity
            isOnGround = onGround();
            if (isOnGround)
            {
                // Movement
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");

                moveDirection = VectorProjectOnSurface(playerCamera.transform.right, surfaceNormal) * horizontal + VectorProjectOnSurface(playerCamera.transform.forward, surfaceNormal) * vertical;
                //moveDirection.y = 0;
                moveDirection.Normalize();

                if (!GadgetAttachOnPlayer() || gadget.getMode() < 5)
                {
                    rotating = true;
                }

                if (GadgetAttachOnPlayer() && gadget.getMode() == 0)
                {
                    velocity = moveDirection * speed;
                    velocity.y = 0.1f;
                }

                if (!GadgetAttachOnPlayer() || gadget.getMode() == 1)
                {
                    velocity = moveDirection * speed;
                }

                if (GadgetAttachOnPlayer() && gadget.getMode() > 5)
                {
                    velocity = Vector3.zero;
                }

                // Jumping
                if (Input.GetButtonDown("Jump") && !jumping)
                {
                    velocity.y = jumpForce;
                    jumping = true;
                }
                else if (!(GadgetAttachOnPlayer() && gadget.getMode() == 0))
                {
                    velocity.y = -gravity;
                }

                currentLevel.setCurrentSpawnPoint(transform.position);
            }
            else
            {
                if (!(GadgetAttachOnPlayer() && gadget.getMode() == 0))
                {
                    velocity.y -= gravity * Time.deltaTime;
                }
                else if (GadgetAttachOnPlayer())
                {
                    velocity.y -= gadget.getMode() * gravity * Time.deltaTime;
                }
            }


            // Move the controller
            controller.Move(velocity * Time.deltaTime);
            playerAvator.transform.localPosition = Vector3.zero;

            // Rotate character to face the direction of movement
            if (moveDirection != Vector3.zero)
            {
                if (rotating)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection, surfaceNormal);
                    playerAvator.transform.rotation = Quaternion.RotateTowards(playerAvator.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    faceRotation = targetRotation;
                    rotating = false;
                }

                if (!GadgetAttachOnPlayer() || gadget.getMode() == 1)
                {
                    animator.SetBool("isWalk", true);
                }
                else
                {
                    animator.SetBool("isWalk", false);
                }
            }
            else
            {
                if (GadgetAttachOnPlayer() && gadget.getMode() < 5)
                {
                    playerAvator.transform.rotation = Quaternion.RotateTowards(playerAvator.transform.rotation, faceRotation, rotationSpeed * Time.deltaTime);
                }
                animator.SetBool("isWalk", false);
            }
        }

        
    }

    private bool onGround()
    {
        RaycastHit hit;
        //float detectDistance = 0.75f * Mathf.Clamp(Mathf.Tan(surfaceAngle * Mathf.Deg2Rad), 0.02f, 0.57735f);
        float detectDistance = 0.02f;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, detectDistance))
        {
            //Debug.Log($"On the Ground {detectDistance}");
            if (!jumping)
            {
                return true;
            }
            return false;
        }
        //Debug.Log($"Not on the Ground");
        jumping = false;
        return false;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Get the collider the character is hitting
        Collider collider = hit.collider;

        // Get the normal of the collided surface
        surfaceNormal = hit.normal;
        float slopeAngle = Vector3.Angle(surfaceNormal, Vector3.up);
        surfaceAngle = slopeAngle;

        if (slopeAngle > 30)
        {
            Vector3 slideDirection = new Vector3(surfaceNormal.x, 0, surfaceNormal.z);
            velocity += Mathf.Sqrt(Mathf.Max(surfaceNormal.y,0.01f) * gravity) * slideDirection * Time.deltaTime;
        }
    }

    void OnGUI()
    {
        GUI.color = Color.black;
        GUI.Label(new Rect(10, 10, 500, 20), $"魔法小工具结附于: {gadget.attachedObject}");
        GUI.color = Color.white;
    }

    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }

    public void RespawnToPosition(Vector3 pos)
    {
        isRespawning = true;
        controller.enabled = false;
        respawnPosition = pos;

        if(gadget.attachedObject!= gameObject)
        {
            gadget.attachedObject.GetComponent<MoveableObject>().isAttached = false;
            gadget.attachedObject.GetComponent<MoveableObject>().gadget = null;
            gadget.attachedObject = gadget.gameObject;
            gadget.indicator.transform.SetParent(null);
            gadget.state = 2;
            gadget.indicator.transform.position = pos;
            gadget.synchronizeMoveMode = false;
        }
    }

    public void RestartArea()
    {
        currentLevel.currentArea.RestartArea();
    }

    public void AdditionalVelocity(Vector3 v)
    {
        velocity += v;
    }

    private Vector3 VectorProjectOnSurface(Vector3 v, Vector3 n)
    {
        return v - Vector3.Dot(n, v) * n;
    }

    private bool GadgetAttachOnPlayer()
    {
        return gadget.attachedObject == this.gameObject && gadget.state == 0;
    }


}