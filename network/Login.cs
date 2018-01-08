using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.UI;
public class Login : MonoBehaviour {

    public class Toons
    {
        public string name;
        public string classe;


    }
    
    string LoginClient = "astamarr.fr/php/LoginClient.php";
    string CreateClientURL = "astamarr.fr/php/CreateClient.php";
    string CheckClientURL = "astamarr.fr/php/CheckClient.php";
    string GetCharacters = "astamarr.fr/php/GetPersos.php";
    string CreateCharacters = "astamarr.fr/php/ToonCreation.php";
    OnlineManager ConnectManager;
    public string InputUserName;
    public string InputPassword;
    public string Inputemail;

    // Character Creation
    public string CharName;
    public string Class;

    public Canvas MainCanvas;
    public Canvas CharCreation;
    public Canvas CharSelection;
    public  Canvas AccountCreationCanvas;
    public Transform CharPanel;
    public Object CharLine;

    private int ID;
    public int SessionID;
    public Text Monitor;
    public Text Monitor2;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        


    }

    public void FireLog()
    {

        StartCoroutine(LogUser(InputUserName, InputPassword));

    }



    IEnumerator LogUser(string Username, string Password)
    {
        
        WWWForm TestUser = new WWWForm();
       
        TestUser.AddField("UsernamePost", Username);
        TestUser.AddField("PasswordPost", Password);
        WWW CheckUserRequest = new WWW(LoginClient, TestUser);
        yield return CheckUserRequest;
        string LoginData = CheckUserRequest.text.Trim();
        bool IsID = Regex.IsMatch(LoginData, @"^\d+$");
        if (IsID)
        {

            ID = int.Parse(LoginData);
          
            SessionID = ID;
            Monitor.text = "  ID : " + LoginData + " choose your Character" ;
            Monitor2.text = "  ID : " + LoginData + " choose your Character";
          
            WWWForm CharacterForm = new WWWForm();
            CharacterForm.AddField("IdPost", ID);
            WWW CharacterRequest = new WWW(GetCharacters, CharacterForm);
            yield return CharacterRequest;
            string Chars = CharacterRequest.text.Trim();
            string[] toons = Chars.Split('&');
            if ( Chars !="no")
            {
                MainCanvas.gameObject.SetActive(false);
                CharSelection.gameObject.SetActive(true);
                GenerateCharacterList(toons);
            }
            else
            {
                MainCanvas.gameObject.SetActive(false);
                CharCreation.gameObject.SetActive(true);
            }
          


            

        }
        else
        {
            Monitor.text = LoginData;
            yield return new WaitForSeconds(5);

        }

 





    }

    public void CreateChar()
    {

    

        Class = "Warrior";
        StartCoroutine(CharCreate());
        StartCoroutine(LogUser(InputUserName, InputPassword));
    }

    IEnumerator CharCreate()
    {
        WWWForm CreateChar = new WWWForm();

        CreateChar.AddField("IdPost", SessionID);
        CreateChar.AddField("NamePost", CharName);
        CreateChar.AddField("ClassPost", Class);
        WWW CheckUserRequest = new WWW(CreateCharacters, CreateChar);
        yield return CheckUserRequest;

        print(CheckUserRequest.text);


    }

    public void GenerateCharacterList(string[] toons)
    {

       for (int i =0; i< toons.Length - 1; i++)
        {
          
            string[] temp = toons[i].Split('|');
            print(temp[0]);
            print(temp[1]);
            
            print(temp[2]);
            GameObject Class = (GameObject)Instantiate(CharLine, CharPanel, false);
            Class.GetComponentInChildren<Text>().text = temp[1];
            Class.GetComponentInChildren<Button>().onClick.AddListener(delegate { ConnectToServer(Class.GetComponentInChildren<Button>()); }); 
           Class.GetComponentInChildren<ToonId>().ID = int.Parse(temp[0]);
        }
    }

   

    public void SetLogin(string login)
    {
        InputUserName = login;


    }
    public void SetPassword(string password)
    {
        InputPassword = password;


    }

    public void SetToonName(string ToonName)
    {
        CharName = ToonName;
    }


    public void SetEmail(string mail)
    {
        Inputemail = mail;


    }

   

    public void SwitchCanvas()
    {
        if (MainCanvas.isActiveAndEnabled)
        {

            MainCanvas.gameObject.SetActive(false);
            AccountCreationCanvas.gameObject.SetActive(true);
           
        }
        else
        {
            MainCanvas.gameObject.SetActive(true);
            AccountCreationCanvas.gameObject.SetActive(false);

        }


    }


    public void OpenCreateChar()
    {

        if (CharSelection.isActiveAndEnabled)
        {



            CharSelection.gameObject.SetActive(false);
            CharCreation.gameObject.SetActive(true);


        }
        else if (CharCreation.isActiveAndEnabled)
        {

            CharSelection.gameObject.SetActive(true);
            CharCreation.gameObject.SetActive(false);

        }


    }


    public int GetPlayerID ()
    {

        return ID;
    }

  

    public void UserCreation()
    {

        StartCoroutine(CreateUser(InputUserName, InputPassword,Inputemail));
        SwitchCanvas();
    }

    IEnumerator CreateUser(string Username, string Password, string Email)
    {

     
        WWWForm TestUser = new WWWForm();
 
        TestUser.AddField("UsernamePost", Username);
        print(Username);

        WWW CheckUserRequest = new WWW(CheckClientURL, TestUser);

        yield return CheckUserRequest;
 
        string LoginData = CheckUserRequest.text.Trim();
       
        print(LoginData);
        print(LoginData.ToString());

        if (LoginData == "false")
        {
           
            WWWForm NewUser = new WWWForm();
            NewUser.AddField("UsernamePost", Username);
            NewUser.AddField("PasswordPost", Password);
            NewUser.AddField("EmailPost", Email);

            WWW NewUserRequest = new WWW(CreateClientURL, NewUser);
            Monitor.text = "  Account : " + Username + " Created ! ";
            Monitor2.text = "  Account : " + Username + " Created ! "; ;
            yield return NewUserRequest;

            string zz = NewUserRequest.text.Trim();
            print(zz);

        }
        else
        {

            Monitor.text = "  Account name already taken";
            Monitor2.text = "  Account name already taken";
        }
    }

    public void ConnectToServer(Button button)
    {
        ConnectManager = this.gameObject.GetComponent<OnlineManager>();
        int IDToFire = button.GetComponentInParent<ToonId>().ID;
        ConnectManager.ConnectClient(IDToFire);


    }
}

