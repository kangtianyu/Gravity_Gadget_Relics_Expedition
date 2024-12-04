using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaController : MonoBehaviour
{
    public Vector3 spawnPoint = Vector3.zero;
    
    private LevelMaster lm;
    private int checkPointRegistered = 0;
    private int checkPointFinished = 0;
    private AreaInitializer initializer;

    // Start is called before the first frame update
    void Start()
    {
        lm = transform.parent.GetComponent<LevelMaster>();
        if (lm == null)
        {
            Debug.Log($"{name} Level Master is not found.");
        }

        initializer = transform.GetComponent<AreaInitializer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerController.Instance.gameObject)
        {
            lm.setLastCheckPoint(spawnPoint);
            lm.currentArea = this;
            //Debug.Log($"{name} is current area");
            Destroy(GetComponent<Collider>());
        }
    }

    public void RegisterCheckPoint()
    {
        checkPointRegistered += 1;
    }

    public void FinishCheckPoint()
    {
        checkPointFinished += 1;
    }

    public bool isFinished()
    {
        return checkPointFinished >= checkPointRegistered;
    }

    public void RestartArea(string respawnReason = "")
    {
        if (initializer != null)
        {
            initializer.init();
        }
        PlayerController.Instance.RespawnToPosition(spawnPoint);
        //Debug.Log($"{name} is restarted because {respawnReason}");
        UIManager.Instance.DeathSentence = respawnReason;
    }
}
