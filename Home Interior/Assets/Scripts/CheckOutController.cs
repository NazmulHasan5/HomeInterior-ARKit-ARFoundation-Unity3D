using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

[SelectionBase]
class PlaceOrderJson
{
    public float price;
    public string token;

    public PlaceOrderJson(float price,string token)
    {
        this.price = price;
        this.token = token;
    }
}
[System.Serializable]
class Order
{
    public string orderDate;
    public int orderId;
    public int price;

}
[System.Serializable]
class IncomingPlaceOrderJson
{
    public string token;
    public Order[] orders;
    public string name;
    public string balance;
    public string message;
    public bool auth;
    public string email;

}

public class CheckOutController : MonoBehaviour
{
    

    [Header("Checkout Panal Elements")]

    [SerializeField]
    RectTransform checkoutPanel;

    [SerializeField]
    RectTransform itemsContainer;

    [SerializeField]
    Vector2 itemsContainerSizeDelta;

    [SerializeField]
    TMP_Text checkoutBtnTMP;

    [SerializeField]
    TMP_Text totalPriceTMP;

    [Header("Items Prefab")]
    [SerializeField]
    GameObject itemPrefab;

    [Header("Translation Time")]
    [SerializeField]
    float easing = .5f;

    [Header("Main Screen Elements")]
    [SerializeField]
    GameObject profileBTN;
    [SerializeField]
    GameObject snapShotBTN;
    [SerializeField]
    GameObject addObjectBTN;
    [SerializeField]
    GameObject checkoutBTN;


    [Header("Cart Item details")]
    [SerializeField]
     List<Sprite> itemsImage;
    [SerializeField]
    List<float> itemsPrice;
    [SerializeField]
    int itemCounter = 0;

    //total purchased price;
    float totalPrice;

    // Start is called before the first frame update
    void Start()
    {
        itemsContainerSizeDelta = itemsContainer.sizeDelta;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Add product to checkout
    public void AddProductToCart(ItemDetails itemDetails)
    {
      
            itemsImage.Add(itemDetails.itemImage);
            itemsPrice.Add(itemDetails.itemPrice);
            itemCounter++;
            checkoutBtnTMP.text = itemCounter.ToString();
            checkoutBTN.SetActive(true);

    }

    public void Main_To_Checkout()
    {
        float xPos = 0f;
        Vector3 presentPosition = checkoutPanel.localPosition;

        if(presentPosition.x == 0f)
        {
            xPos = Screen.width;
        }
        else
        {
            LoadCartItems();
        }

        Vector3 newPosition = new Vector3(xPos, presentPosition.y, presentPosition.z);

        StartCoroutine(SmoothTranslationMainToCheckout(presentPosition, newPosition, easing, xPos));

    }

    IEnumerator SmoothTranslationMainToCheckout(Vector3 startPos, Vector3 endPos, float seconds, float xPos)
    {
        float t = 0f;
        while(t <= 1f)
        {
            t += Time.deltaTime / seconds;

            checkoutPanel.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        if (xPos == Screen.width)
        {
            Image[] items = itemsContainer.gameObject.GetComponentsInChildren<Image>();
            int childCount = itemsContainer.gameObject.transform.childCount;

            for(int i = 0; i< childCount; i++)
            {
                Destroy(itemsContainer.gameObject.transform.GetChild(i).gameObject); 
            }
             itemsContainer.sizeDelta = itemsContainerSizeDelta ;
            addObjectBTN.SetActive(true);
            snapShotBTN.SetActive(true);
            profileBTN.SetActive(true);
        }

    }

    void LoadCartItems()
    {
        float price = 0;

        for(int i = 0; i< itemCounter; i++)
        {
            if (i > 1)
            {
                itemsContainer.sizeDelta = new Vector2(itemsContainer.sizeDelta.x, itemsContainer.sizeDelta.y + 200f);
            }
            GameObject item = Instantiate(itemPrefab, itemsContainer.transform);
            item.GetComponent<Image>().sprite = itemsImage[i];
            price += itemsPrice[i];
            
        }

        totalPriceTMP.text = "Total Price: " + price + " BDT";
        totalPrice = price;
    }

    public void PlaceOrderDone()
    {
        //webrequest to backend


        Main_To_Checkout();
        itemsImage.RemoveRange(0, itemCounter);
        itemsPrice.RemoveRange(0, itemCounter);
        itemCounter = 0;
        checkoutBTN.SetActive(false);
        
        
    }


    public void PlaceOrder()
    {
        StartCoroutine(PlaceOrderWebReq());
    }

    IEnumerator PlaceOrderWebReq()
    {
        string token = PlayerPrefs.GetString("Token");
        string jsonData = JsonUtility.ToJson(new PlaceOrderJson(totalPrice, token));
        Debug.Log(jsonData);

        UnityWebRequest req = UnityWebRequest.Put("/order/add-order", jsonData);
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.isNetworkError || req.isHttpError)
        {
            Debug.Log("Somthing went wrong: " + req.error);
        }
        else
        {
            IncomingPlaceOrderJson response = JsonUtility.FromJson<IncomingPlaceOrderJson>(req.downloadHandler.text);
            Debug.Log("Token: " + response.token+"\n" + response.orders.Length);
           for (int i = 0; i< response.orders.Length; i++)
            {
                Debug.Log(response.orders[i].orderId +"  "+ response.orders[i].orderDate + "  "+ response.orders[i].price);
            }
           
            if (req.isDone)
            {
                PlaceOrderDone();
                PlayerPrefs.SetString("ProfileData", req.downloadHandler.text);
            }
        }

    }
}
