using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


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

    [Header("Profile Panel Elements")]
    [SerializeField]
    TMP_Text profileName;
    [SerializeField]
    TMP_Text profileBalance;

    
    

    //profile info panal
    [Header("Application Main Canvas GameObjects Elements")]
    
    [SerializeField]
    GameObject applicationMainCanvas;

    [Tooltip("Profile panal")]
    [SerializeField]
    RectTransform profilePanal;


    [Tooltip("Catagory panal")]
    [SerializeField]
    GameObject catagoryPanal;
    [SerializeField]
    RectTransform categoryPanalElements;

    [Header("Item Panal Elements")]

    [Tooltip("Item Panal")]
    [SerializeField]
    RectTransform itemPanal;

    [SerializeField]
    TMP_Text itemPanalTitle;

    [SerializeField]
    Image[] ItemsContainerImages;

    [Header("Login Signup Canvas")]

    [Tooltip("Application Login Signup Canvas GameObject")]
    [SerializeField]
    GameObject applicationLoginSignupCanves;

    

    [Tooltip("Login panal")]
    [SerializeField]
    RectTransform loginPanal;

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

    [Header("Application Data Status")]
    [SerializeField]
    int loginStatus;
    [SerializeField]
    float tokenStatus;
    [SerializeField]
    string name;
    [SerializeField]
    int balance;

    [Header("Controller Scripts")]
    [SerializeField]
    ApplicationsBehavior applicationsBehavior;

    [Header("3D Object to augment")]
    [SerializeField]
    GameObject arObject;

    [Header("3D Models")]

    [SerializeField]
    GameObject sofa;

    [SerializeField]
    GameObject chair;

    [SerializeField]
    GameObject bed;

    [SerializeField]
    GameObject wardrope;

    [SerializeField]
    GameObject light;

    [Header("Items Images")]
    public Sprite[] chairTexture;
    public Sprite[] bedTexture;
    public Sprite[] lightTexture;
    public Sprite[] sofaTexture;
    public Sprite[] wardropeTexture;

    public Sprite[] ii;

    void Start()
    {
        /*
        //get profile data from server
        //need a coroutine for this task
        ProfileData profileData = JsonUtility.FromJson<ProfileData>(PlayerPrefs.GetString("ProfileData"));

        Debug.Log(profileData.auth+ " " + profileData.name + " " + profileData.email + " " + profileData.balance + " " );
        */
    }

    // Update is called once per frame
    void Update()
    {
        loginStatus = PlayerPrefs.GetInt("Login");
        tokenStatus = PlayerPrefs.GetFloat("Token");
        /*
        ProfileData profileData = JsonUtility.FromJson<ProfileData>(PlayerPrefs.GetString("ProfileData"));
        if(profileData.name == null)
        {
            name = "Logout";
            balance = 0001;
        }
        else
        {
            name = profileData.name;
            balance = profileData.balance;
        }
        
       
        Debug.Log("Testing");
        */

    }

    //Logout
    public void Logout()
    {
        PlayerPrefs.SetInt("Login", 0);
        PlayerPrefs.SetString("Token", null);
        PlayerPrefs.SetString("ProfileData", null);
        ShowProfile();
    }

    public void SetProfilePanalData()
    {
        //Set name and belance in profile panal
        ProfileData profileData = JsonUtility.FromJson<ProfileData>(PlayerPrefs.GetString("ProfileData"));
        profileName.text = profileData.name;
        profileBalance.text = "Balance: "+ profileData.balance + " BDT";
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

        if(PlayerPrefs.GetString("ProfileData")== "")
        {
            applicationMainCanvas.SetActive(false);
            applicationLoginSignupCanves.SetActive(true);
            applicationsBehavior.enabled = false;
            gameObject.GetComponent<CanvasBehaviorController>().enabled = false;
        }
    }

    //Category panal to item panal <> itme panal to Catagory panal
    public void Category_To_Item_Panel()
    {
        float xpos = 0;
        float scale = .5f;
        Vector3 presentPosition = itemPanal.localPosition;
        Vector3 presentScale = categoryPanalElements.localScale;
        if (presentPosition.x == 0)
        {
            xpos = Screen.width;
            scale = 1;
        }
        Vector3 newPos = new Vector3(xpos, presentPosition.y, presentPosition.z);
        Vector3 newScale = new Vector3(scale, scale, 1);

        StartCoroutine(SmoothTransationCategoryToItem(presentPosition, newPos, easing, presentScale, newScale));
    }

    IEnumerator SmoothTransationCategoryToItem(Vector3 startPos, Vector3 endPos, float secounds, Vector3 startScale, Vector3 endScale)
    {
        float t = 0f;

        while (t <= 1)
        {
            t += Time.deltaTime / secounds;
            itemPanal.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            categoryPanalElements.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }

    public void SetItems(string itemName)
    {
        
        string itemPanalName;

        if (itemName == "Sofa")
        {
            arObject = sofa;
            ChangeImageOfItemsContainer(sofaTexture);
        }
        else if (itemName == "Bed")
        {
            arObject = bed;
            ChangeImageOfItemsContainer(bedTexture);
        }
        else if (itemName == "Aram Chair")
        {
            arObject = chair;
            ChangeImageOfItemsContainer(chairTexture);
        }
        else if (itemName == "Wardrope")
        {
            arObject = wardrope;
            ChangeImageOfItemsContainer(wardropeTexture);
        }
        else if (itemName == "Light")
        {
            arObject = light;
            ChangeImageOfItemsContainer(lightTexture);
            
        }

        itemPanalTitle.text = itemName;
        

    }

    void ChangeImageOfItemsContainer(Sprite[] images)
    {
        for (int i = 0; i < ItemsContainerImages.Length; i++)
        {
            ItemsContainerImages[i].sprite = images[i];
        }
    }

    public void AugmentItem()
    {
        categoryPanalElements.localScale = new Vector3(1, 1, 1);
        Debug.Log("Augment Object: "+ arObject.name);
        
        
        applicationsBehavior.SelectObjReference(arObject);



        float xpos = 0;
       
        Vector3 presentPosition = itemPanal.localPosition;
        
       
        if (presentPosition.x == 0)
        {
            xpos = Screen.width;
           
        }
        
        Vector3 newPos = new Vector3(xpos, presentPosition.y, presentPosition.z);

        StartCoroutine(SmoothTransationItemToMain(presentPosition, newPos, easing));
    }


    IEnumerator SmoothTransationItemToMain(Vector3 startPos, Vector3 endPos, float secounds)
    {
        float t = 0f;

        while (t <= 1)
        {
            t += Time.deltaTime / secounds;
            itemPanal.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }

}
