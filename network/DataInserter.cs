using UnityEngine;
using System.Collections;

public class DataInserter : MonoBehaviour {

    string CreateClientURL = "http://127.0.0.1/jeu/Createclient.php";
    string CheckClientURL = "http://127.0.0.1/jeu/CheckClient.php";
    public string InputUserName;
    public string InputPassword;
    public string InputEmail;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Space))
            {
         
            StartCoroutine(CreateUser(InputUserName, InputPassword, InputEmail));
       
        }
	
	}

    IEnumerator CreateUser(string Username, string Password, string Email)
    {

     
        WWWForm TestUser = new WWWForm();
      
        TestUser.AddField("UsernamePost", Username);
    
        WWW CheckUserRequest = new WWW(CheckClientURL, TestUser);

        yield return CheckUserRequest;
        string LoginData = CheckUserRequest.text;
        print(LoginData);
        if (CheckUserRequest.text == "false")
        {

            WWWForm NewUser = new WWWForm();
            NewUser.AddField("UsernamePost", Username);
            NewUser.AddField("PasswordPost", Password);
            NewUser.AddField("EmailPost", Email);

            WWW NewUserRequest = new WWW(CreateClientURL, NewUser);
            print("Client DONE");


        }
        else
        {
            print("Client deja pris lolol");
        }

    }

    }

   

