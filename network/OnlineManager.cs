using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class OnlineManager : NetworkManager
{
   public string IPConnect;
    GameObject player;
    public List<GameObject> players;
    string GetToonInfo = "astamarr.fr/php/GetToonInfos.php";
    int NewPlayerID;
    string ToonName;

    bool IsHostClient;

    int PlayerID;
    // Use this for initialization
    void Start () {
        players = new List<GameObject>();
        IsHostClient = false;
        IPConnect = "127.0.0.1";


    }

    // Update is called once per frame
    void Update () {
	
	}


    public override void OnClientConnect(NetworkConnection conn)
    {
        if (IsHostClient)
        {
           
            return;
        }
        else
        {
            ClientScene.Ready(conn);
            IntegerMessage lol = new IntegerMessage();
            lol.value = PlayerID;
            print("IM GONNA SEND NOW");
            print(lol.value);
     
            ClientScene.AddPlayer(conn, 0, lol); // recu par OnServerAddPlayer !!! 
           

        }

        
        
       
        
    }
    public override void OnClientSceneChanged(NetworkConnection conn)
    {

        return;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
       
           
        int IDValue = extraMessageReader.ReadMessage<IntegerMessage>().value;
        print("HERE WE ARE");
        print(IDValue);
        NewPlayerID = IDValue;


        if (IDValue != 0 )
            {
           StartCoroutine(GetCharInfo(conn, playerControllerId));
            }





        



    }

    public void SetIp(string ip)
    {

        IPConnect = ip;

    }
    IEnumerator GetCharInfo(NetworkConnection conn, short playerControllerId)
    {

        WWWForm CharacterForm = new WWWForm();
        CharacterForm.AddField("IdPost", NewPlayerID);
        WWW CharacterRequest = new WWW(GetToonInfo, CharacterForm);
        yield return CharacterRequest;
        string Chars = CharacterRequest.text.Trim();
        Debug.Log(Chars.ToString());
        string[] toons = Chars.Split('|');

        print(toons[0]);
        print(toons[1]);
        ToonName =  toons[0];

       
        if (toons[2] != "")
        {

            Vector3 Newposition = new Vector3(float.Parse(toons[2]), float.Parse(toons[3]), float.Parse(toons[4]));
    
            player = (GameObject)GameObject.Instantiate(playerPrefab, Newposition, Quaternion.identity);
            player.GetComponent<Player>().SetInfos(NewPlayerID, ToonName);


        }
        else
        {
            player = (GameObject)GameObject.Instantiate(playerPrefab, new Vector3(300,17,703), Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player, (short)(players.Count + 1));
            player.GetComponent<Player>().SetInfos(NewPlayerID, ToonName);

        }

      
        NetworkServer.AddPlayerForConnection(conn, player, (short)(players.Count + 1));
     
        players.Add(player);

        foreach (GameObject i in players){

            i.GetComponent<Player>().RpcTriggerShareName(i.GetComponent<Player>().ToonName);


        }


    }

    public void ConnectClient(int ID)
    {
        PlayerID = ID;

        this.networkAddress =  IPConnect ;
        this.networkPort = 7777;
        this.StartClient();
   

    }


    
    public void LaunchHost()
    {
        IsHostClient = true;
        this.StartHost();


    }


}
