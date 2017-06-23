/*============================================================================== 
 * Copyright (c) 2012-2015 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Vuforia;

/// <summary>
/// This class manages the content displayed on top of cloud reco targets in this sample
/// </summary>
public class ContentManager : MonoBehaviour, ITrackableEventHandler
{
    #region PUBLIC_VARIABLES
    public RawImage LoadingSpinnerBackground;
    public RawImage LoadingSpinnerImage;
    public Button CancelButton;

    /// <summary>
    /// The root gameobject that serves as an augmentation for the image targets created by search results
    /// </summary>
    public GameObject AugmentationObject; // Objec that renders on screen, change between BookInformation and Guideshop
    
    /// <summary>
    /// Reference to the script handling animations between 2D and 3D.
    /// </summary>
    public AnimationsManager AnimationsManager;

    /// <summary>
    /// the URL the JSON data should be fetched from
    /// </summary>
    public string JsonServerUrl;
    public string JsonLocalFilename;
    #endregion //PUBLIC_VARIABLES


    #region PRIVATE_MEMBERS
    private bool mIsShowingBookData = false;
    private bool mIsLoadingBookData = false;
    private bool mIsLoadingBookThumb = false;
    private WWW mJsonBookInfo;
    private WWW mBookThumb;
    private BookData mBookData;
    private bool mIsBookThumbRequested = false;
    private BookInformationParser mBookInformationParser;
    private bool mIsShowingMenu = false;
    private CloudRecoBehaviour mCloudRecoBehaviour;
    private string LocalJasonFile;
    #endregion //PRIVATE_MEMBERS


    #region MONOBEHAVIOUR_METHODS
    void Start ()
    {
        // setup BookInformationParser 
        mBookInformationParser = new BookInformationParser();
        mBookInformationParser.SetBookObject(AugmentationObject); // Sets book object as gameobject that holds the content
        // mBookInformationParser.SetGuideshopObject(AugmentationObject); // Sets guideshop object as gameobject that holds the content

        // Checks is parent of Augmentable object has trackable behaviour
        TrackableBehaviour trackableBehaviour = AugmentationObject.transform.parent.GetComponent<TrackableBehaviour>();
        if (trackableBehaviour)
        {
            trackableBehaviour.RegisterTrackableEventHandler(this);
        }
        
        mCloudRecoBehaviour = FindObjectOfType<CloudRecoBehaviour>();
        
        HideObject();

        SetCancelButtonVisible(false);
    }
    
    void Update () 
    {
        // Checks if new target is created
        if ( mIsShowingBookData )
        {
            // Check if user has pressed down
            if (Input.GetMouseButtonUp(0))
            {
                // Raycasting from the center of the main camera
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast (ray, out hit, 1000.0f)) {
                    GameObject hitObject = hit.collider.gameObject;
                    // Checks to see if the hit object is BookInformation or Guideshop
                    if (hitObject != null && hitObject.name == "BookInformation" || hitObject.name == "Guideshop" )                        
                    {
                        // Checks to see if there is anything in mBookData and the menu is being displayed
                        if (mBookData != null && mIsShowingMenu == false)
                        {
                            Application.OpenURL("https://bonobos.com/guideshop");
                        }
                    }
                }
            }
        }
        // Triggered by the JSON parsing the book data
        if (mIsLoadingBookThumb)
        {
            // Goes through process of parsing data to find the image, updating the texture and rendering the object
            LoadBookThumb();
        }

        // Show/hide loading progress spinner if we are loading book data or thumb
        SetLoadingSpinnerVisibile (mIsLoadingBookData || mIsLoadingBookThumb);
        
        // Show cancel button if the Cloud Reco is not enabled, otherwise hide it
        SetCancelButtonVisible(mCloudRecoBehaviour.CloudRecoInitialized && !mCloudRecoBehaviour.CloudRecoEnabled);
    }
    #endregion //MONOBEHAVIOUR_METHODS


    #region PUBLIC_METHODS
    public void OnCancel()
    {
        mCloudRecoBehaviour.CloudRecoEnabled = true;
        TargetDeleted();
    }

    /// <summary>
    /// Method called from the CloudRecoEventHandler
    /// when a new target is created
    /// </summary>
    public void TargetCreated(string targetMetadata)
    {
        // Initialize the showing/loading book data variable
        mIsShowingBookData = true;
        mIsLoadingBookData = true;
        // Setting flag to request flag data to false
        mIsBookThumbRequested = false;

        // Loads the JSON Book Data
        // StartCoroutine( LoadJSONBookData(targetMetadata) );

        // Loads the JSON data from local folder
        StartCoroutine (LoadLocalJSONBookData(JsonLocalFilename));
    }
    
    /// <summary>
    /// Method called when the Close button is pressed
    /// to clean the target Data by setting flags to false and emptying book data
    /// </summary>
    public void TargetDeleted()
    {
        // Initialize the showing book data variable
        mIsShowingBookData = false;
        mIsLoadingBookData = false;
        mIsLoadingBookThumb = false;
        mBookData = null;
    }

    /// <summary>
    /// Implementation of the ITrackableEventHandler function called when the
    /// tracking state changes.
    /// </summary>
    public void OnTrackableStateChanged(
                                    TrackableBehaviour.Status previousStatus,
                                    TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            TargetFound();
        }
        else
        {
            TargetLost();
        }
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
    /// Method to let the ContentManager know if the CloudReco
    /// SampleMenu is being displayed
    /// </summary>
    public void SetIsShowingMenu(bool isShowing)
    {
        mIsShowingMenu = isShowing;
    }
    #endregion //PUBLIC_METHODS


    #region PRIVATE_METHODS
    private void SetLoadingSpinnerVisibile(bool visible)
    {
        if (LoadingSpinnerBackground == null ||
            LoadingSpinnerImage == null) 
            return;

        if (LoadingSpinnerBackground.enabled != visible)
            LoadingSpinnerBackground.enabled = visible;

        if (LoadingSpinnerImage.enabled != visible)
            LoadingSpinnerImage.enabled = visible;

        if (visible)
        {
            LoadingSpinnerImage.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);
        }
    }

    private void SetCancelButtonVisible(bool visible)
    {
        if (CancelButton == null) return;

        if (CancelButton.enabled != visible)
        {
            CancelButton.enabled = visible;
            CancelButton.image.enabled = visible;
        }
    }

    /// <summary>
    /// Method called from the CloudReco Trackable Event Handler
    /// when a target has been found
    /// </summary>
    private void TargetFound()
    {
        // Checks tha the book info is displayed
        if (mIsShowingBookData)
        {
            // Starts playing the animation to 3D
            AnimationsManager.PlayAnimationTo3D(AugmentationObject);
        }
    }

    /// <summary>
    /// Method called from the CloudReco Trackable Event Handler
    /// when a target has been Lost
    /// </summary>
    private void TargetLost()
    {
        // Checks tha the book info is displayed
        if (mIsShowingBookData)
        {
            // Starts playing the animation to 2D
            AnimationsManager.PlayAnimationTo2D(AugmentationObject);
        }
    }

    /// <summary>
    /// Fetches the JSON data from a server, called from TargetCreated()
    /// </summary>
    private IEnumerator LoadJSONBookData(string jsonBookUrl)
    {
        // Gets the full book json url
        string fullBookURL = JsonServerUrl + jsonBookUrl;
    
        // Gets the json book info from the url
        mJsonBookInfo = new WWW(fullBookURL);
        yield return mJsonBookInfo;

        // Loading done
        mIsLoadingBookData = false;
        
        if (mJsonBookInfo.error == null)
        {
            // Parses the json Object
            JSONParser parser = new JSONParser();
            
            // Parses book data from JSON and assigns it to mBookData    
            BookData bookData = parser.ParseString(mJsonBookInfo.text);
            mBookData = bookData;
        
            // Updates the BookData info in the augmented object
            mBookInformationParser.UpdateBookData(bookData);
        
            mIsLoadingBookThumb = true;
        } 
        else
        {
            Debug.LogError("Error downloading json");
        }
    }

    /// <summary>
    /// Fetches the JSON data from a local folder, called from TargetCreated()
    /// </summary>
    private IEnumerator LoadLocalJSONBookData(string filename)
    {
        LocalJasonFile = ConvertLocalJSONToString(filename);

        //mIsLoadingBookThumb = true;
        yield return LocalJasonFile;

        // Loading done
        mIsLoadingBookData = false;

        // Parses the json Object
        JSONParser parser = new JSONParser();

        // Parses book data from JSON and assigns it to mBookData    
        BookData bookData = parser.ParseString(LocalJasonFile);
        mBookData = bookData;
        Debug.Log(bookData);

        // Updates the BookData info in the augmented object
        mBookInformationParser.UpdateBookData(bookData);

        mIsLoadingBookThumb = true;
    }

    /// <summary>
    /// Converts local JSON file and converts it to a string
    /// </summary>
    private static string ConvertLocalJSONToString(string filename)
    {
        TextAsset targetFile = Resources.Load<TextAsset>(filename.Replace(".json", ""));
        return targetFile.text;
    }

    /// <summary>
    /// Loads the texture for the book thumbnail
    /// </summary>
    private void LoadBookThumb()
    {
        // If book thumb hasn't been requested yet and there is book data assign the url to the book
        // thumb to the URL in the JSON data
        if (!mIsBookThumbRequested )            
        {
            if (mBookData != null )
            {
                mBookThumb = new WWW(mBookData.BookThumbUrl);
                mIsBookThumbRequested = true;
            }
        }
        // If it's still in progress update the text of the book image with the texture from the parsed JSON
        // Render the object
        if (mBookThumb.progress >=1)
        {
            if(mBookThumb.error == null && mBookData != null)
            {
                mBookInformationParser.UpdateBookThumb(mBookThumb.texture);
                mIsLoadingBookThumb = false;
                ShowObject();
            }
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
    #endregion //PRIVATE_METHODS
}
