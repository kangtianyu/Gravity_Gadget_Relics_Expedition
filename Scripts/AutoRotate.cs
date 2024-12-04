using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public bool isSmooth = true;
    public bool isRotate = true;
    public Vector3 rotateSpeed = Vector3.zero;
    public float decayRate = 10f;
    public float dampingDuration = 10f;

    private Vector3 actualRotateSpeed;
    private float elapsedTime;
    // states
    // 0: stop
    // 1: run
    // 2: slowing down to stop
    // 3: speeding up to run
    private int state;
    private int lastFrameState;

    // Start is called before the first frame update
    void Start()
    {
        if (isRotate)
        {
            actualRotateSpeed = rotateSpeed;
            state = 1;
        }
        else
        {
            actualRotateSpeed = Vector3.zero;
            state = 0;
        }
        lastFrameState = state;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotate)
        {
            if (isSmooth && actualRotateSpeed != rotateSpeed)
            {
                state = 3;
            }
            else
            {
                state = 1;
            }
        }
        else
        {
            if (isSmooth && actualRotateSpeed != Vector3.zero)
            {
                state = 2;
            }
            else
            {
                state = 0;
            }
        }
        if (lastFrameState != state)
        {
            elapsedTime = 0;
            lastFrameState = state;
        }

        if (state == 1)
        {
            transform.Rotate(Time.deltaTime * rotateSpeed);
        }
        else if (state > 1)
        {
            elapsedTime += Time.deltaTime;
            if(elapsedTime < dampingDuration)
            {
                if (state == 2)
                {
                    float decay = DecayFactor(elapsedTime / dampingDuration);
                    actualRotateSpeed.x = Mathf.Clamp(actualRotateSpeed.x - rotateSpeed.x * decay / dampingDuration * Time.deltaTime, 0f, rotateSpeed.x);
                    actualRotateSpeed.y = Mathf.Clamp(actualRotateSpeed.y - rotateSpeed.y * decay / dampingDuration * Time.deltaTime, 0f, rotateSpeed.y);
                    actualRotateSpeed.z = Mathf.Clamp(actualRotateSpeed.z - rotateSpeed.z * decay / dampingDuration * Time.deltaTime, 0f, rotateSpeed.z);
                    transform.Rotate(Time.deltaTime * actualRotateSpeed);
                }
                else // state == 3
                {
                    float decay = DecayFactor(1 - elapsedTime / dampingDuration);
                    actualRotateSpeed.x = Mathf.Clamp(actualRotateSpeed.x + rotateSpeed.x * decay / dampingDuration * Time.deltaTime, 0f, rotateSpeed.x);
                    actualRotateSpeed.y = Mathf.Clamp(actualRotateSpeed.y + rotateSpeed.y * decay / dampingDuration * Time.deltaTime, 0f, rotateSpeed.y);
                    actualRotateSpeed.z = Mathf.Clamp(actualRotateSpeed.z + rotateSpeed.z * decay / dampingDuration * Time.deltaTime, 0f, rotateSpeed.z);
                    transform.Rotate(Time.deltaTime * actualRotateSpeed);
                }
            }
            else
            {
                if (state == 2)
                {
                    actualRotateSpeed = Vector3.zero;
                }
                else
                {
                    actualRotateSpeed = rotateSpeed;
                }
            }
        }
    }

    float DecayFactor(float x)
    {
        float C = decayRate / (1 - Mathf.Exp(-decayRate)); // Normalization constant
        return C * Mathf.Exp(-decayRate * x);
    }
}
