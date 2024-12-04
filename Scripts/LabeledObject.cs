using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LabeledObject : MonoBehaviour
{

    public string label = "Object UI Label";
    public TMP_FontAsset customFont;
    public int fontSize = 24;
    public Color color = Color.blue;

    private GameObject labelObject;
    private TextMeshProUGUI objectLabel;
    private MoveableObject objectInfo;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the label
        labelObject = new GameObject($"{label.Replace(" ", "")}");

        // Set the textObject as a child of the Canvas
        labelObject.transform.SetParent(UIManager.Instance.mainCanvas.transform);

        // Add the TextMeshProUGUI component to the GameObject
        objectLabel = labelObject.AddComponent<TextMeshProUGUI>();

        // Set the text of the TextMeshPro object
        objectLabel.text = label;

        // Set the font of the text
        if (customFont != null)
        {
            objectLabel.font = customFont;
        }

        // Set the font size of the text
        objectLabel.fontSize = fontSize;

        // Set the color of the text
        objectLabel.color = color;

        //Set the width of the text
        RectTransform rectTransform = objectLabel.GetComponent<RectTransform>();

        // Set the width of the RectTransform (TextMeshProUGUI)
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);

        // Set text alignment (optional)
        objectLabel.alignment = TextAlignmentOptions.Center;

        objectInfo = GetComponent<MoveableObject>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        // update label if it's on screen
        // Find the center of Collider
        Vector3 colliderCenter = GetComponent<Collider>().bounds.center;
        // Convert the object's position to viewport space
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(colliderCenter);

        // Check if the position is within the viewport bounds
        bool onScreen = viewportPosition.x >= 0 && viewportPosition.x <= 1 &&
                        viewportPosition.y >= 0 && viewportPosition.y <= 1 &&
                        viewportPosition.z > 0; // Ensure it's in front of the camera

        bool isNotCovered = false;

        if (onScreen)
        {
            // Check if object is covered from other object
            // Perform a raycast from the camera's position to the target colliderCenter
            RaycastHit hit;
            Ray ray = new Ray(Camera.main.transform.position, colliderCenter - Camera.main.transform.position);

            // Ignore Ray cast layer
            int layerMask = 1 << 10;

            // Raycast to check if something is blocking the view
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
            {
                // If the ray hit an object, check if it's the target object
                if (hit.transform.gameObject == this.gameObject)
                {
                    isNotCovered = true; // The object is visible, no occlusion
                }
            }
        }

        if (onScreen && isNotCovered)
        {
            // project the object position to UI canvas 
            // Step 1: Convert the viewport position to screen space
            Vector3 screenPos = Camera.main.ViewportToScreenPoint(viewportPosition);
            //screenPos.z = Camera.main.nearClipPlane;

            // Step 2: Convert the world position to local position relative to the canvas
            RectTransform canvasRect = UIManager.Instance.mainCanvas.GetComponent<RectTransform>();
            Vector3 localPos = canvasRect.InverseTransformPoint(screenPos);
            localPos.y += 50;

            // Step 3: Set the TextMeshPro label's position
            objectLabel.rectTransform.anchoredPosition = localPos;


            // show label
            objectLabel.text = $"{label}";
            if (objectInfo != null)
            {
                if (objectInfo.isDestroyed)
                {
                    objectLabel.text = $"<#000000><s>{objectLabel.text}</s>";
                }
                else
                {
                    if (objectInfo.modifier != "")
                    {
                        objectLabel.text = $"({objectInfo.modifier}){objectLabel.text}";
                    }
                    if (objectInfo.isSelected)
                    {
                        objectLabel.text = $">{objectLabel.text}<";
                    }
                }
            }
            labelObject.SetActive(true);
        }
        else
        {
            if (labelObject != null)
            {
                labelObject.SetActive(false);
            }
        }
    }
    // Called when the object is SetActive(false)
    void OnDisable()
    {
        if(labelObject != null)
        {
            labelObject.SetActive(false);
        }
    }
}
