using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

class LoginFormJson
{
    public string email;
    public string password;

    public LoginFormJson(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
}

class SignupFormJson
{
    public string name;
    public string email;
    public string password;

    public SignupFormJson(string name, string email, string password)
    {
        this.name = name;
        this.email = email;
        this.password = password;
    }
}

class OTPFormJson
{
    public string token;
    public int authCode;

    public OTPFormJson(string token, int authCode)
    {
        this.token = token;
        this.authCode = authCode;
    }
}

class TokenJson
{
    public string token;

    public TokenJson(string token)
    {
        this.token = token;
    }
}

[SelectionBase]
class ServerResponseData
{
    public string message;
    public string token;
    public bool auth;
    public string name;
    public string email;
    public int balance;
}



public class ApplicationStates : MonoBehaviour
{
    [SerializeField]
    RectTransform signupPanal;
    [SerializeField]
    RectTransform signupPanalElements;
    [SerializeField]
    RectTransform loginPanal;
    [SerializeField]
    RectTransform loginPanalElements;
    [SerializeField]
    RectTransform otpPanal;

    [SerializeField]
    float easing = .5f;

    [SerializeField]
    string token = null;

    //login page inputfield
    [SerializeField]
    TMP_InputField loginEmail;
    [SerializeField]
    TMP_InputField loginPassword;
    [SerializeField]
    Toggle loginRememberMe;

    
   
    

    //Signup page inputfield
    [SerializeField]
    TMP_InputField signupName;
    [SerializeField]
    TMP_InputField signupEmail;
    [SerializeField]
    TMP_InputField signupPassword;

    //OPT page inputfield
    [SerializeField]
    TMP_InputField OTPCode;

    //application signup complete
    [SerializeField]
    GameObject applicationCanvas;
    [SerializeField]
    ApplicationsBehavior applicationsBehavior;
    //application signin login canvas
    [Header("Application Canvas Objects")]

    [SerializeField]
    GameObject applicationMainCanvas;

    [SerializeField]
    GameObject applicationSigninLoginCanvas;

    //CanvasBehaviorController Script to update user data;
    [SerializeField]
    CanvasBehaviorController canvasBehaviorController;

    [Header("Splash Canvas")]
    [SerializeField]
    GameObject splashCanvas;

    // Start is called before the first frame update
    void Start()
    {

        Invoke("AppSplash",2f);
        
    }

    void AppSplash()
    {

        if (!PlayerPrefs.HasKey("Login"))
        {
            PlayerPrefs.SetInt("Login", 0);
            applicationSigninLoginCanvas.SetActive(true);
            splashCanvas.SetActive(false);
        }
        else
        {
            int applicationLoginStatus = PlayerPrefs.GetInt("Login");

            if (applicationLoginStatus == 1)
            {
                //if player is loged in check if it have a valid token
                token = PlayerPrefs.GetString("Token");
                Debug.Log("Checking application session in backend");
                //check the token if it ended given session
                StartCoroutine(TokenSessionCheck(token));
            }

            if (applicationLoginStatus == 0)
            {
                Debug.Log("Application opened for 1st time");
                applicationSigninLoginCanvas.SetActive(true);
                splashCanvas.SetActive(false);
            }
        }
    }

    IEnumerator TokenSessionCheck(string token)
    {
        string jsonData = JsonUtility.ToJson(new TokenJson(token));

        UnityWebRequest req = UnityWebRequest.Put("http://127.0.0.1:8080/auth/validate-login-session", jsonData);
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.isNetworkError || req.isHttpError)
        {
            Debug.Log("Application session changed Activating login signup canvas");
            PlayerPrefs.SetString("Token", null);
            PlayerPrefs.SetInt("Login",0);
            applicationSigninLoginCanvas.SetActive(true);
            splashCanvas.SetActive(false);
        }
        else
        {
            if (req.isDone)
            {
                Debug.Log("Application session is ok starting application");
                ServerResponseData serverResponseData = new ServerResponseData();
                serverResponseData = JsonUtility.FromJson<ServerResponseData>(req.downloadHandler.text);
                token = serverResponseData.token;

                PlayerPrefs.SetInt("Login", 1);
                PlayerPrefs.SetString("Token", token);
                applicationSigninLoginCanvas.SetActive(false);
                applicationCanvas.SetActive(true);
                applicationsBehavior.enabled = true;
                canvasBehaviorController.enabled = true;
                splashCanvas.SetActive(false);
            }
        }
    }

    //Login Request
    public void Login()
    {
        StartCoroutine(LoginWebReq());
    }
    //Login network request
    IEnumerator LoginWebReq()
    {

        string json = JsonUtility.ToJson(new LoginFormJson(loginEmail.text, loginPassword.text));
        Debug.Log(json);
        UnityWebRequest req = UnityWebRequest.Put("http://127.0.0.1:8080/auth/login", json);
        req.SetRequestHeader("Content-Type", "application/json");
        //req.method = "POST";

        yield return req.SendWebRequest();

        if (req.isNetworkError || req.isHttpError)
        {
            Debug.Log("Application Login failed Try again");
            print("Error downloading: " + req.error + "   " + req.downloadHandler.text);
            loginPassword.text = null;
        }
        else
        {
            if (req.isDone)
            {
                loginPassword.text = null;
                Debug.Log("Application Login sucessful starting application");
               
                // Debug.Log(req.downloadHandler.text);
                ServerResponseData serverResponseData = new ServerResponseData();
                serverResponseData = JsonUtility.FromJson<ServerResponseData>(req.downloadHandler.text);
                
                token = serverResponseData.token;

                Debug.Log(req.downloadHandler.text+"\n "+serverResponseData.auth);

                if (serverResponseData.auth)
                {
                    //Debug.Log(loginRememberMe.isOn.));
                    if (!loginRememberMe.isOn)
                    {
                        PlayerPrefs.SetInt("Login", 1);
                        PlayerPrefs.SetString("Token", token);
                        
                    }
                    PlayerPrefs.SetString("ProfileData", req.downloadHandler.text);
                    canvasBehaviorController.enabled = true;
                    Invoke("Hide",.2f);
                    applicationSigninLoginCanvas.SetActive(false);

                }
                else
                {
                    ResendOTP();
                }    
            }

        }
    }

    void Hide()
    {
        canvasBehaviorController.enabled = true;
        applicationCanvas.SetActive(true);
        applicationsBehavior.enabled = true;
    }


    //Resend OTP Code
    public void ResendOTP()
    {
        StartCoroutine(ResendOTPWebReq());
    }
    //Resend OTP network request
    IEnumerator ResendOTPWebReq()
    {
        string jsonData = JsonUtility.ToJson(new TokenJson(token));

        UnityWebRequest req = UnityWebRequest.Put("http://127.0.0.1:8080/auth/resend-otp", jsonData);
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.isHttpError || req.isNetworkError)
        {
            Debug.Log("Error Code: " + req.error + "   " + req.downloadHandler.text);
        }
        else
        {
            if (req.isDone)
            {
                Debug.Log(req.downloadHandler.text);

                ServerResponseData serverResponseData = new ServerResponseData();
                serverResponseData = JsonUtility.FromJson<ServerResponseData>(req.downloadHandler.text);
                token = serverResponseData.token;

                 OTP_To_Login();
            }
        }

        Debug.Log(jsonData);
        signupPassword.text = null;

    }

    //Signup request
    public void Signup()
    {
        StartCoroutine(SignupWebReq());
    }

    //Signup network request
    IEnumerator SignupWebReq()
    {
        string jsonData = JsonUtility.ToJson(new SignupFormJson(signupName.text, signupEmail.text, signupPassword.text));

        UnityWebRequest req = UnityWebRequest.Put("http://127.0.0.1:8080/auth/signup", jsonData);
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if(req.isHttpError || req.isNetworkError)
        {
            Debug.Log("Error Code: "+ req.error + "   " +  req.downloadHandler.text);
        }
        else
        {
            if (req.isDone)
            {
                Debug.Log(req.downloadHandler.text);

                ServerResponseData serverResponseData = new ServerResponseData();
                serverResponseData = JsonUtility.FromJson<ServerResponseData>(req.downloadHandler.text);
                token = serverResponseData.token;

                OTP_To_Signup();
            }
        }

        Debug.Log(jsonData);
        signupPassword.text = null;
        
    }


    //OTP Request
    public void OTP()
    {
        StartCoroutine(OTPWebRequest());
    }
    //OTP network request
    IEnumerator OTPWebRequest()
    {
        string jsonData = JsonUtility.ToJson( new OTPFormJson(token, int.Parse(OTPCode.text)) );

        UnityWebRequest req = UnityWebRequest.Put("http://127.0.0.1:8080/auth/auth-user", jsonData);
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.isHttpError || req.isNetworkError)
        {
            Debug.Log("Error Code: " + req.error + "   " + req.downloadHandler.text);
        }
        else
        {
            if (req.isDone)
            {
                Debug.Log("OTP Confirmed Sucessfull: " + req.downloadHandler.text);

                OTP_To_Login();
            }
        }

        Debug.Log(jsonData);
        signupPassword.text = null;
    }

    

  

    /*IEnumerator LoginReq()
    {
        WWWForm body = new WWWForm();
        body.AddField("email", "nazmulimm@gmail.com");
        body.AddField("password","test123");
        using(UnityWebRequest req = UnityWebRequest.Post("http://127.0.0.1:8080/auth/login", body))
        {
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.Send();

            if(req.isNetworkError || req.isHttpError)
            {
                Debug.Log("req URI "+ req.uri);
                Debug.Log("req Method "+ req.method);
                Debug.Log("req responsecode"+ req.responseCode);
                Debug.Log("req isDone" + req.isDone);
                Debug.Log("error: "+ req.error);
               // Debug.Log("req isDone" + req.isDone);

            }
            else
            {
                Debug.Log("Req Success: " + req);
            }
        }
    }
    */

    //login to signup <> signup to login
    public void SignupPage()
    {
        float xpos = 0;
        float scale = .5f;
        Vector3 presentPosition = signupPanal.localPosition;
        Vector3 presentScale = loginPanalElements.localScale;
        if (presentPosition.x == 0)
        {
            xpos = Screen.width;
            scale = 1;
        }
        Vector3 newPos = new Vector3(xpos, presentPosition.y, presentPosition.z);
        Vector3 newScale = new Vector3(scale, scale, 1);

        StartCoroutine(SmoothTransation(presentPosition, newPos, easing , presentScale, newScale));
    }

    IEnumerator SmoothTransation(Vector3 startPos, Vector3 endPos, float secounds, Vector3 startScale, Vector3 endScale)
    {
        float t = 0f;

        while( t <= 1)
        {
            t += Time.deltaTime / secounds;
            signupPanal.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep( 0f, 1f, t));
            loginPanalElements.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }

    //signup to otp <> otp to signup panal
    public void OTP_To_Signup()
    {
        float xpos = 0;
        float scale = .5f;
        Vector3 presentPosition = otpPanal.localPosition;
        Vector3 presentScale = signupPanalElements.localScale;

        if (presentPosition.x == 0)
        {
            xpos = Screen.width;
            scale = 1;
        }
        Vector3 newPos = new Vector3(xpos, presentPosition.y, presentPosition.z);
        Vector3 newScale = new Vector3(scale, scale, 1);

        StartCoroutine(SmoothTransationOTP(presentPosition, newPos, easing, presentScale, newScale));
    }

    IEnumerator SmoothTransationOTP(Vector3 startPos, Vector3 endPos, float secounds, Vector3 startScale, Vector3 endScale)
    {
        float t = 0f;

        while (t <= 1)
        {
            t += Time.deltaTime / secounds;
            otpPanal.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            signupPanalElements.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }



    //OTP to Login panal
    public void OTP_To_Login()
    {
        float xpos = 0;
        float scale = .5f;

        Vector3 presentPosition = otpPanal.localPosition;
        Vector3 presentScale = loginPanalElements.localScale;

        //Change signup panal position and scale
        signupPanal.localPosition = new Vector3(Screen.width, signupPanal.localPosition.y , signupPanal.localPosition.z);
        signupPanalElements.localScale = new Vector3(1, 1, 1);

        if (presentPosition.x == 0)
        {
            xpos = Screen.width;
            scale = 1f;
        }

        Vector3 newPos = new Vector3(xpos, presentPosition.y, presentPosition.z);
        Vector3 newScale = new Vector3(scale, scale, 1);

        StartCoroutine(SmoothTransationOTPToLogin(presentPosition, newPos, easing, presentScale, newScale));
    }


    IEnumerator SmoothTransationOTPToLogin(Vector3 startPos, Vector3 endPos, float secounds, Vector3 startScale, Vector3 endScale)
    {
        float t = 0f;

        while (t <= 1)
        {
            t += Time.deltaTime / secounds;
            otpPanal.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            loginPanalElements.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }
}

