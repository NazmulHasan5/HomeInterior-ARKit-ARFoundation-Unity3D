using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[SelectionBase]
class ProfileData
{
    public string message;
    public string token;
    public bool auth;
    public string name;
    public string email;
    public int balance;
}


public class CanvasBehaviorController : MonoBehaviour
{
    
    //profile info panal
    [Header("Canvas Panals")]
    
    [Tooltip("Profile panal")]
    [SerializeField]
    RectTransform profilePanal;

    [Tooltip("Catagory panal")]
    [SerializeField]
    GameObject catagoryPanal;

    [Tooltip("productPanal")]
    [SerializeField]
    GameObject productPanal;

    [TextArea]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "This component shouldn't be removed, it does important stuff.";

    [Tooltip("Panal transation time")]
    [SerializeField]
    float easing = .5f;

    [Tooltip("Show Profile Button")]
    [SerializeField]
    GameObject profileViewButton;

    [Tooltip("Show Category Button")]
    [SerializeField]
    GameObject categoryViewButton;

    void Start()
    {
        //get profile data from server
        //need a coroutine for this task
        ProfileData profileData = JsonUtility.FromJson<ProfileData>(PlayerPrefs.GetString("ProfileData"));

        Debug.Log(profileData.auth+ " " + profileData.name + " " + profileData.email + " " + profileData.balance + " " );
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowProfile()
    {
        bool profileView = true;
        
        if (profileViewButton.active)
        {
            profileView = false;
            profileViewButton.SetActive(false);
        }
        

        float yPos = -100;
        Vector3 presentPositon = profilePanal.localPosition;

        if(presentPositon.y == -100)
        {
            yPos = -Screen.height;
        }

        Vector3 newPosition =new Vector3(presentPositon.x, yPos, presentPositon.z);

        StartCoroutine(ProfileSmoothTransation(presentPositon, newPosition, easing, profileView));

    }

    IEnumerator ProfileSmoothTransation(Vector3 prePos, Vector3 newPos, float secounds, bool profileView)
    {
        float t = 0f;
        while( t<=1 )
        {
            t += Time.deltaTime / secounds;
            profilePanal.localPosition = Vector3.Lerp(prePos, newPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        if (profileView)
        {
            profileViewButton.SetActive(true);
        }

    }
}
