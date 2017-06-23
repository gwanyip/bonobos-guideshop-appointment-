using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Vuforia;

public class CTAManager : MonoBehaviour {

    // public Button CancelButton;

    /// <summary>
    /// The root gameobject that serves as an augmentation for the image overlay
    /// Going to be a static object that is activated and disactivated
    /// </summary>
    public GameObject AugmentationObject; // First Object that renders on screen, change between BookInformation and Guideshop
    public GameObject AugmentationObject2; // Second Object that renders on screen, change between BookInformation and Guideshop

    /// <summary>
    /// Reference to the script handling animations between 2D and 3D.
    /// </summary>
    public CTAAnimationsManager CTAAnimationsManager;

    // Flags for when the tracking image is loaded
    private bool mIsBookThumbRequested = false;
    private bool mIsShowingMenu = false;

    // Use this for initialization
    void Start () {
        HideObject();
    }
	
	// Update is called once per frame
	void Update () {

        if (Time.time < 2f) {
            HideObject();
            CTANotConfirmed();
        }

        if (Time.time > 7f && Time.time < 7.5f) {
            HideObject2();
            CTAConfirmed();
        }

        //if (Input.GetKeyDown("a")) {
        //    HideObject2();
        //    CTAConfirmed();
        //}
        //if (Input.GetKeyDown("s"))
        //{
        //    CTANotConfirmed();
        //}

        // Check if user has pressed down
        if (Input.GetMouseButtonUp(0))
            {
                // Raycasting from the center of the main camera
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000.0f))
                {
                    GameObject hitObject = hit.collider.gameObject;
                    // Checks to see if the hit object is BookInformation or Guideshop
                    if (hitObject != null && hitObject.name == "Guideshop")
                    {

                        Application.OpenURL("https://bonobos.com/guideshop");
                    }
                }
            }
        // }
    }

    /// <summary>
    /// Hides the augmentation object
    /// </summary>
    public void HideObject()
    {
        Renderer[] rendererComponents = AugmentationObject.GetComponentsInChildren<Renderer>();
        Collider[] colliderComponents = AugmentationObject.GetComponentsInChildren<Collider>();

        // Enable rendering:
        foreach (Renderer component in rendererComponents)
        {
            component.enabled = false;
        }

        // Enable colliders:
        foreach (Collider component in colliderComponents)
        {
            component.enabled = false;
        }
    }

    /// <summary>
    /// Hides the second augmentation object
    /// </summary>
    public void HideObject2()
    {
        Renderer[] rendererComponents = AugmentationObject2.GetComponentsInChildren<Renderer>();
        Collider[] colliderComponents = AugmentationObject2.GetComponentsInChildren<Collider>();

        // Enable rendering:
        foreach (Renderer component in rendererComponents)
        {
            component.enabled = false;
        }

        // Enable colliders:
        foreach (Collider component in colliderComponents)
        {
            component.enabled = false;
        }
    }

    /// <summary>
    /// Shows the augmentation object by enabling all the children compents in augmentation Object
    /// </summary>
    private void ShowObject()
    {
        Renderer[] rendererComponents = AugmentationObject.GetComponentsInChildren<Renderer>();
        Collider[] colliderComponents = AugmentationObject.GetComponentsInChildren<Collider>();

        // Enable rendering:
        foreach (Renderer component in rendererComponents)
        {
            component.enabled = true;
        }

        // Enable colliders:
        foreach (Collider component in colliderComponents)
        {
            component.enabled = true;
        }
    }

    /// <summary>
    /// Method called when CTA button is confirmed
    /// </summary>
    private void CTAConfirmed()
    {
        ShowObject();
        // Starts playing the animation to 3D
        CTAAnimationsManager.PlayAnimationTo3D(AugmentationObject);
    }

    /// <summary>
    /// Method called when CTA button is not confirmed
    /// </summary>
    private void CTANotConfirmed()
    {
        // Starts playing the animation to 2D
        CTAAnimationsManager.PlayAnimationTo2D(AugmentationObject);
        HideObject();
    }



}
