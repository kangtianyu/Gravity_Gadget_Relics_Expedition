using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMaster : MonoBehaviour
{
    public bool selectArea = false;
    public Vector3 initialSpawnPoint = Vector3.zero;
    public AreaController currentArea;

    private AreaController initArea;
    private PlayerController player;
    private Vector3 currentSpawnPoint;
    private Vector3 lastCheckPoint;
    private List<GameObject> moveableObjects;

    // Start is called before the first frame update
    void Start()
    {
        initArea = currentArea;
        player = PlayerController.Instance;
        currentSpawnPoint = initialSpawnPoint;
        FindAllMoveableObjects();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.position.y < -20)
        {
            player.RespawnToPosition(currentSpawnPoint);
        }

        if (Input.GetButtonDown("Respawn"))
        {
            if (selectArea)
            {
                player.RespawnToPosition(initArea.spawnPoint);
            }
            else
            {
                currentArea.RestartArea("按下了重生键");
            }
        }
    }

    public void setCurrentSpawnPoint(Vector3 pos)
    {
        currentSpawnPoint = pos;
    }

    public void setLastCheckPoint(Vector3 pos)
    {
        //lastCheckPoint = pos;
    }

    public List<GameObject> GetAllMoveableObjects()
    {
        return moveableObjects;
    }

    public List<GameObject> GetAllMoveableObjectsInCurrentArea()
    {
        // Get all child objects with the tag "MoveableObject", including nested children
        return FindObjectsWithMoveableObjectTagUnderParent(currentArea.transform);
    }

    private void FindAllMoveableObjects()
    {
        moveableObjects = new List<GameObject>();
        foreach (Transform area in transform)
        {
            moveableObjects.AddRange(FindObjectsWithMoveableObjectTagUnderParent(area));
        }
    }

    private List<GameObject> FindObjectsWithMoveableObjectTagUnderParent(Transform parent)
    {
        List<GameObject> moveableObjects = new List<GameObject>();

        // Loop through all children of the parent
        foreach (Transform child in parent)
        {
            // Check if this child has the specified tag
            if (child.CompareTag("MoveableObject"))
            {
                moveableObjects.Add(child.gameObject);
            }

            // Recursively search in this child's children
            moveableObjects.AddRange(FindObjectsWithMoveableObjectTagUnderParent(child));
        }

        return moveableObjects;
    }
}
