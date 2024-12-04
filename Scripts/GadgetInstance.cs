using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GadgetInstance : MonoBehaviour
{
    //public GameObject playerCharacter;
    public GameObject indicator;
    public Transform anchor;
    public Camera playerCamera;
    public float launchAngle = 0f;
    public float launchSpeed = 10f;
    [HideInInspector]
    public GameObject attachedObject;
    public bool synchronizeMoveMode = false;
    // states
    // 0: attached on object
    // 1: throw mode
    // 2: just changed attached object and on the way to it
    // 3: gadget is thrown out and in parabola trace
    public int state = 0;
    public int mode = 1;

    private int steps = 128;
    private LineRenderer lineRenderer;
    private float gravity = 10f;
    private Renderer indicatorRenderer;
    private Vector3 gadgetPositionOffset = Vector3.zero;
    private float gadgetButtonHoldDuration = 0f;
    private bool attachedObjectChanged = false;
    private GameObject selectedObject = null;
    private Vector3 relativePos = Vector3.zero;
    private Vector3 indicatorTargetPos;
    private float parabolaTravelTime = 0f;
    private Vector3 parabolaTravelOrigin;
    private Vector3 parabolaTravelDirection;
    private float parabolaTargetDistance;
    // offset from the attached object position to the attached point
    private Vector3 attachOffset = Vector3.zero;
    private bool allowThrowMode = false;

    // Start is called before the first frame update
    void Start()
    {
        indicatorRenderer = indicator.GetComponent<Renderer>();
        indicatorRenderer.material.color = Color.green;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = steps + 1; // Set the number of points
        lineRenderer.enabled = false;

        attachedObject = this.gameObject;
        indicatorTargetPos = indicator.transform.position;
        parabolaTravelOrigin = indicator.transform.position;
        parabolaTravelDirection = playerCamera.transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        // control gravity
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            mode = 0;
            indicatorRenderer.material.color = Color.white;
            if (state == 0 && attachedObject!= this.gameObject)
            {
                MoveableObject objectInfo = attachedObject.GetComponent<MoveableObject>();
                if (objectInfo.ObjectMass > 1f)
                {
                    objectInfo.gravity = 0f;
                }
                else
                {
                    objectInfo.gravity = -2f;
                }
                objectInfo.ResetStable();
            }
            if (synchronizeMoveMode)
            {
                Vector3 tmpPos = attachedObject.transform.position;
                tmpPos.y += 0.1f;
                attachedObject.transform.position = tmpPos;
            }

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            mode = 1;
            indicatorRenderer.material.color = Color.green;
            if (state == 0 && attachedObject != this.gameObject)
            {
                MoveableObject objectInfo = attachedObject.GetComponent<MoveableObject>();
                objectInfo.gravity = gravity;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            mode = 10;
            indicatorRenderer.material.color = Color.red;
            if (state == 0 && attachedObject != this.gameObject)
            {
                MoveableObject objectInfo = attachedObject.GetComponent<MoveableObject>();
                objectInfo.gravity = mode * gravity;
                objectInfo.ResetStable();
            }
        }

        // "G" button
        if (Input.GetButtonDown("Gadget"))
        {
            if (synchronizeMoveMode)
            {
                synchronizeMoveMode = false;
                attachedObject.GetComponent<MoveableObject>().ResetStable();
            }

            if (state == 0 && attachedObject == this.gameObject)
            {
                gadgetButtonHoldDuration = 0f;
                attachedObject = null;
                attachedObjectChanged = false;
                indicator.transform.SetParent(null);
                state = 1;
            }
            else
            {
                if (attachedObject != null && attachedObject != this.gameObject)
                {
                    attachedObject.GetComponent<MoveableObject>().gravity = gravity;
                    MoveableObject objectInfo = attachedObject.GetComponent<MoveableObject>();
                    objectInfo.isAttached = false;
                    objectInfo.gadget = null;
                }
                attachedObject = this.gameObject;
                indicator.transform.SetParent(null);
                state = 2;
            }
        }


        if (Input.GetButton("Gadget"))
        {
            if (state == 1)
            {

                gadgetButtonHoldDuration += Time.deltaTime;

                if (gadgetButtonHoldDuration > 0.2f)
                {
                    gadgetPositionOffset = playerCamera.transform.right * 0.4f;
                    lineRenderer.enabled = true;
                    selectedObject = RaycastParabola();

                }
                else
                {
                    selectedObject = GetFrontObject();
                }

                if (selectedObject != null)
                {
                    MoveableObject objectContent = selectedObject.GetComponent<MoveableObject>();
                    if (objectContent != null)
                    {
                        UIManager.Instance.selectedObjectList.Add(selectedObject);
                        if (Input.GetMouseButtonDown(0) && objectContent.isActive)
                        {
                            state = 3;
                            attachedObject = selectedObject;
                            indicatorTargetPos = attachedObject.transform.position;
                            parabolaTravelTime = 0f;
                            indicator.transform.SetParent(null);
                            attachedObjectChanged = true;
                            lineRenderer.enabled = false;
                        }
                    }
                }
            }
        }
        else
        {
            gadgetPositionOffset = Vector3.zero;
            lineRenderer.enabled = false;
        }

        if (Input.GetButtonUp("Gadget"))
        {
            if (state == 1)
            {
                if (gadgetButtonHoldDuration > 0.2f)
                {
                    if (attachedObjectChanged)
                    {
                        attachedObjectChanged = false;
                    }
                    else
                    {
                        state = 0;
                        attachedObject = this.gameObject;
                        indicator.transform.SetParent(anchor);
                    }
                }
                else
                {
                    if (selectedObject != null)
                    {
                        state = 2;
                        attachedObject = selectedObject;
                        indicator.transform.SetParent(null);
                        attachedObjectChanged = true;
                    }
                    else
                    {
                        state = 0;
                        attachedObject = this.gameObject;
                        indicator.transform.SetParent(anchor);
                    }
                }
            }
            selectedObject = null;
        }

        // "F" button
        if (Input.GetButtonDown("Interact"))
        {
            if (state == 0 && attachedObject != this.gameObject)
            {
                gadgetButtonHoldDuration = 0f;

                Vector3 objDirection = attachedObject.GetComponent<Collider>().bounds.center - anchor.transform.position;
                float dotProduct = Vector3.Dot(anchor.forward, objDirection);
                if (dotProduct > 0)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(anchor.position, objDirection, out hit, 2f))
                    {
                        if (hit.collider.gameObject == attachedObject)
                        {
                            allowThrowMode = true;
                            PlayerController.Instance.isBusy = true;
                        }
                    }
                }
            }
        }
        if (Input.GetButton("Interact"))
        {
            if (state == 0 && attachedObject != this.gameObject)
            {
                gadgetButtonHoldDuration += Time.deltaTime;

                if (gadgetButtonHoldDuration > 0.2f && allowThrowMode)
                {

                    lineRenderer.enabled = true;
                    EnemyAI enemyAI = attachedObject.GetComponent<EnemyAI>();
                    if (enemyAI != null)
                    {
                        attachedObject.transform.GetChild(1).gameObject.SetActive(false);
                    }
                    else
                    {
                        attachedObject.SetActive(false);
                    }
                    Vector3 throwDirection = ThrowParabola();
                    if (Input.GetMouseButtonDown(0))
                    {
                        throwDirection.Normalize();
                        attachedObject.GetComponent<MoveableObject>().ResetStable();
                        attachedObject.GetComponent<MoveableObject>().AdditionalVelocity(throwDirection * 10f);
                        lineRenderer.enabled = false;
                        allowThrowMode = false;
                        PlayerController.Instance.isBusy = false;
                        if (enemyAI != null)
                        {
                            attachedObject.transform.GetChild(1).gameObject.SetActive(true);
                        }
                        else
                        {
                            attachedObject.SetActive(true);
                        }
                    }
                }
            }
        }

        if (Input.GetButtonUp("Interact"))
        {
            if (synchronizeMoveMode)
            {
                synchronizeMoveMode = false;
                attachedObject.GetComponent<MoveableObject>().ResetStable();
            }
            else if (state == 0 && attachedObject != this.gameObject)
            {
                Vector3 objDirection = attachedObject.GetComponent<Collider>().bounds.center - anchor.transform.position;
                float dotProduct = Vector3.Dot(anchor.forward, objDirection);
                if (dotProduct > 0)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(anchor.position, objDirection, out hit, 2f))
                    {
                        if (hit.collider.gameObject == attachedObject)
                        {
                            if (gadgetButtonHoldDuration < 0.2f)
                            {
                                synchronizeMoveMode = true;
                                Vector3 tmpPos = attachedObject.transform.position;
                                tmpPos.y += 0.1f;
                                attachedObject.transform.position = tmpPos;
                                attachedObject.GetComponent<MoveableObject>().FreezeRotation();
                                relativePos = transform.InverseTransformPoint(attachedObject.transform.position);
                            }
                        }
                    }
                }
            }
            lineRenderer.enabled = false;
            allowThrowMode = false;
            PlayerController.Instance.isBusy = false;
            EnemyAI enemyAI = attachedObject.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                attachedObject.transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                attachedObject.SetActive(true);
            }
        }

        // Update gadget position
        Vector3 newPos;
        if (state == 0)
        {
            if (attachedObject == this.gameObject)
            {
                newPos = Vector3.MoveTowards(indicator.transform.position, anchor.position + gadgetPositionOffset, 1f * Time.deltaTime);
            }
            else
            {
                newPos = Vector3.MoveTowards(indicator.transform.position, attachedObject.transform.TransformPoint(attachOffset), 1f * Time.deltaTime);
            }
        }
        else if (state == 1)
        {
            float distiance = Vector3.Distance(indicator.transform.position, anchor.position + gadgetPositionOffset);
            if (distiance > 1f)
            {
                newPos = Vector3.MoveTowards(indicator.transform.position, anchor.position + gadgetPositionOffset, 10f * Time.deltaTime);
            }
            else if (distiance < 0.4f)
            {
                newPos = Vector3.MoveTowards(indicator.transform.position, anchor.position + gadgetPositionOffset, 1f * Time.deltaTime);
            }
            else
            {
                newPos = Vector3.MoveTowards(indicator.transform.position, anchor.position + gadgetPositionOffset, (1f + 15f * (distiance -0.4f)) * Time.deltaTime);
            }
        }
        else if (state == 3)
        {
            parabolaTravelTime += Time.deltaTime;

            float x = launchSpeed * parabolaTravelDirection.x * parabolaTravelTime;
            float y = launchSpeed * parabolaTravelDirection.y * parabolaTravelTime - (mode * gravity * parabolaTravelTime * parabolaTravelTime) / 2;
            float z = launchSpeed * parabolaTravelDirection.z * parabolaTravelTime;

            newPos = parabolaTravelOrigin + new Vector3(x, y, z);

            Debug.Log($"{Vector3.Distance(newPos, attachedObject.transform.TransformPoint(attachOffset))}, {parabolaTargetDistance} ");
            if (Vector3.Distance(newPos, attachedObject.transform.TransformPoint(attachOffset)) > parabolaTargetDistance)
            {
                state = 2;
                Debug.Log($"{name} state 2");
            }
            else
            {
                parabolaTargetDistance = Vector3.Distance(newPos, attachedObject.transform.TransformPoint(attachOffset));
            }
        }
        else
        {
            if (attachedObject == this.gameObject)
            {
                // gadget is attached back to player
                newPos = Vector3.MoveTowards(indicator.transform.position, anchor.position + gadgetPositionOffset, 20f * Time.deltaTime);
                if (Vector3.Distance(indicator.transform.position, anchor.position + gadgetPositionOffset) < 0.2f)
                {
                    indicator.transform.SetParent(anchor);
                    state = 0;
                }
            }
            else
            {
                // gadget is attached to target.
                newPos = Vector3.MoveTowards(indicator.transform.position, attachedObject.transform.TransformPoint(attachOffset), 20f * Time.deltaTime);
                if (Vector3.Distance(indicator.transform.position, attachedObject.transform.TransformPoint(attachOffset)) < 0.2f)
                {
                    // gadget finished travel and start effect
                    indicator.transform.SetParent(attachedObject.transform);
                    MoveableObject objectInfo = attachedObject.GetComponent<MoveableObject>();
                    objectInfo.isAttached = true;
                    objectInfo.gadget = this;

                    if (mode > 0 || objectInfo.ObjectMass > 1f)
                    {
                        objectInfo.gravity = mode * gravity;
                    }
                    else
                    {
                        objectInfo.gravity = -2f;
                    }

                    if(mode != 1)
                    {
                        objectInfo.ResetStable();
                    }
                    state = 0;
                }
            }
        }
        indicator.transform.position = newPos;
    }

    void FixedUpdate()
    {
        if (synchronizeMoveMode)
        {
            attachedObject.GetComponent<MoveableObject>().FreezeRotation();
            attachedObject.GetComponent<MoveableObject>().ForceMove(transform.TransformPoint(relativePos));
        }
    }

    void OnGUI()
    {
        GUI.color = Color.black;
        GUI.Label(new Rect(10, 30, 500, 40), $"是否在抓取模式: {synchronizeMoveMode}");
        GUI.color = Color.white;
    }

    public int getMode()
    {
        return mode;
    }

    private GameObject RaycastParabola()
    {
        GameObject selectedObject = null;

        parabolaTravelOrigin = indicator.transform.position;
        parabolaTravelDirection = playerCamera.transform.forward;
        // raise the direction at y axis by launch angle
        parabolaTravelDirection.y += Mathf.Sin(launchAngle * Mathf.Deg2Rad);
        parabolaTravelDirection.z -= 0.06f;
        parabolaTravelDirection.Normalize();


        // calculation first step
        float t = 0.001f;

        // first line
        Vector3 lineStartPoint = parabolaTravelOrigin;
        lineRenderer.SetPosition(0, lineStartPoint);
        float x = launchSpeed * parabolaTravelDirection.x * t;
        float y = launchSpeed * parabolaTravelDirection.y * t - (mode * gravity * t * t) / 2;
        float z = launchSpeed * parabolaTravelDirection.z * t;
        Vector3 lastLineDirection = new Vector3(x, y, z);
        lineStartPoint = lineStartPoint + lastLineDirection;
        lineRenderer.SetPosition(1, lineStartPoint);

        // point index
        int i = 2;

        // calculate next step
        Vector3 newDirection = Vector3.zero;
        while (i <= steps)
        {
            // calculate a next small step 
            t += 0.001f;
            x = launchSpeed * parabolaTravelDirection.x * t;
            y = launchSpeed * parabolaTravelDirection.y * t - (mode * gravity * t * t) / 2;
            z = launchSpeed * parabolaTravelDirection.z * t;

            // new direction = new parabola point (x,y,z) + origin - line start point (last line end point)
            Vector3 newParabolaPoint = new Vector3(x, y, z) + parabolaTravelOrigin;
            newDirection = newParabolaPoint - lineStartPoint;

            // angle limit is 1 degree, length limit is 10
            // when reach either the limit, make a new line
            if (Vector3.Angle(lastLineDirection, newDirection) > 1 || newDirection.sqrMagnitude > 100f)
            {
                Vector3 lineNextPoint = newParabolaPoint;
                lineRenderer.SetPosition(i, lineNextPoint);
                i++;

                // Find hit object
                RaycastHit hit;
                if (Physics.Raycast(lineStartPoint, newDirection, out hit, newDirection.magnitude)) 
                {
                    //Debug.Log("Hit: " + hit.collider.name);
                    selectedObject = hit.collider.gameObject;
                    //attachOffset = hit.point - hit.collider.transform.position;
                    attachOffset = hit.collider.transform.InverseTransformPoint(hit.point);
                    parabolaTargetDistance = Vector3.Distance(hit.point, parabolaTravelOrigin);
                    lineStartPoint = lineNextPoint;
                    break;
                }

                lineStartPoint = lineNextPoint;
                lastLineDirection = newDirection;
            }
        }

        //Debug.Log($"{i},{t}");

        while (i <= steps)
        {
            lineRenderer.SetPosition(i, lineStartPoint);
            i++;
        }

        return selectedObject;
    }


    private GameObject GetFrontObject()
    {
        GameObject selectedObject = null;

        List<GameObject> moveableObjects = PlayerController.Instance.currentLevel.GetAllMoveableObjectsInCurrentArea();

        float minDistance = 2f;
        foreach (GameObject obj in moveableObjects)
        {
            // Check if the direction to target is in front of the reference's forward direction
            Vector3 objDirection = obj.GetComponent<Collider>().bounds.center - anchor.transform.position;
            float dotProduct = Vector3.Dot(anchor.forward, objDirection);
            if (dotProduct > 0)
            {
                RaycastHit hit;
                if (Physics.Raycast(anchor.position, objDirection, out hit, 2f))
                {
                    if(hit.collider.gameObject == obj && hit.distance < minDistance)
                    {
                        minDistance = hit.distance;
                        selectedObject = obj;
                        //attachOffset = hit.point - hit.collider.transform.position;
                        attachOffset = hit.collider.transform.InverseTransformPoint(hit.point);

                    }
                }
            }
        }

        return selectedObject;
    }

    private Vector3 ThrowParabola()
    {
        Vector3 parabolaTravelOrigin = attachedObject.transform.position;
        Vector3 parabolaTravelDirection = playerCamera.transform.forward;
        // raise the direction at y axis by launch angle
        parabolaTravelDirection.y += Mathf.Sin(launchAngle * Mathf.Deg2Rad);
        parabolaTravelDirection.Normalize();


        // calculation first step
        float t = 0.001f;

        // first line
        Vector3 lineStartPoint = parabolaTravelOrigin;
        lineRenderer.SetPosition(0, lineStartPoint);
        float x = launchSpeed * parabolaTravelDirection.x * t;
        float y = launchSpeed * parabolaTravelDirection.y * t - (mode * gravity * t * t) / 2;
        float z = launchSpeed * parabolaTravelDirection.z * t;
        Vector3 lastLineDirection = new Vector3(x, y, z);
        Vector3 throwDirection = lastLineDirection;
        lineStartPoint = lineStartPoint + lastLineDirection;
        lineRenderer.SetPosition(1, lineStartPoint);

        // point index
        int i = 2;

        // calculate next step
        Vector3 newDirection = Vector3.zero;
        while (i <= steps)
        {
            // calculate a next small step 
            t += 0.001f;
            x = launchSpeed * parabolaTravelDirection.x * t;
            y = launchSpeed * parabolaTravelDirection.y * t - (mode * gravity * t * t) / 2;
            z = launchSpeed * parabolaTravelDirection.z * t;

            // new direction = new parabola point (x,y,z) + origin - line start point (last line end point)
            Vector3 newParabolaPoint = new Vector3(x, y, z) + parabolaTravelOrigin;
            newDirection = newParabolaPoint - lineStartPoint;

            // angle limit is 1 degree, length limit is 10
            // when reach either the limit, make a new line
            if (Vector3.Angle(lastLineDirection, newDirection) > 1 || newDirection.sqrMagnitude > 100f)
            {
                Vector3 lineNextPoint = newParabolaPoint;
                lineRenderer.SetPosition(i, lineNextPoint);
                i++;

                RaycastHit hit;
                // Ignore Ray cast layer
                int layerMask = 1 << 11;
                // Raycast to check if something is blocking the view
                if (Physics.Raycast(lineStartPoint, newDirection, out hit, newDirection.magnitude, ~layerMask))
                {
                    lineStartPoint = lineNextPoint;
                    break;
                }

                lineStartPoint = lineNextPoint;
                lastLineDirection = newDirection;
            }
        }

        while (i <= steps)
        {
            lineRenderer.SetPosition(i, lineStartPoint);
            i++;
        }

        return throwDirection;
    }

}
