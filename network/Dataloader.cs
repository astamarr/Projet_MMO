using UnityEngine;
using System.Collections;


public class Dataloader : MonoBehaviour {
    public string[] logins;
	// Use this for initialization
	IEnumerator Start () {

        WWW login = new WWW("http://127.0.0.1/jeu/login.php");
        yield return login;
        string LoginData = login.text;
        print(LoginData);
        logins = LoginData.Split('|');
	
	}
	

}
