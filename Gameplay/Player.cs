// Ce script gère tous les déplacements du personnage ainsi que sa synchronisation réseau et les appels serveur.


using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;


public class Player : NetworkBehaviour
{

    // Character infos
    public string ToonName;
    public int ToonId;
    Text TextInput;


    public GlobalValues.Seperate MovementSpeed;
    public GameObject Camera;
    public Chat ServerChat;



    //moving forwards and backwards
    public float ForwardSpeed = 15;
    public float BackwardSpeed = 5;

    public float AerialLerpModifier = 4;
    public float MovementLerpSpeed = 0.5f;
    private bool _backPedaling;

    public GlobalValues.Seperate RotatingSpeed;
    //moving left and right turning left and right
    public float RotationSpeed = 0.05f;

    public GlobalValues.Seperate Gravity;
    public Vector3 GravityDrop = new Vector3(0, -0.3f, 0);
    public float MaximumGravity = 30;
    public bool IsGrounded;

    public GlobalValues.Seperate Jump;
    public float JumpSpeed = 10;
    public float ConsideredFloor = 0.5f;
    public float JumpLerpSpeed = 0.1f;
    private bool _jumpWasPressed;

    public Vector3 Velocity;
    public Animator animator;

    //on the off hand we need to retain movment (air)
    private Vector3 _currentMovement;
    private Vector3 _currentGravity;
    private bool _hasJumped;

    void Start()
    {
       
        print(ToonId);
        print(ToonName);
        if (!isLocalPlayer)
        {

            this.GetComponentInChildren<TextMesh>().text = ToonName;
          


            return;
        }

      
        CmdSavePosition();
        this.GetComponentInChildren<TextMesh>().text = ToonName;
        Camera = GameObject.FindGameObjectsWithTag("MainCamera")[0];
            wowcamera CameraBehaviour = Camera.GetComponent<wowcamera>();
            print(CameraBehaviour);
            CameraBehaviour.GetPlayer(gameObject.transform);


        TextInput = GameObject.FindGameObjectsWithTag("ChatInput")[0].GetComponent<Text>();

        animator = GetComponent<Animator>();
        animator.SetBool("NonCombat", false);
        GlobalValues.CharacterTransform = transform;
        GlobalValues.CharacterRigid = GetComponent<Rigidbody>();
        GlobalValues.CharacterRigid.freezeRotation = true;
        GlobalValues.CharacterRigid.useGravity = false;
    }
    void OnCollisionStay(Collision other)
    {
        IsGrounded = false;
        foreach (ContactPoint contact in other.contacts)
        {
            if (contact.normal.y < ConsideredFloor) continue;
            IsGrounded = true;
        }
    }
    void OnCollisionExit(Collision other)
    {
        IsGrounded = false;
    }

    void Update()
    {

       // CmdShareName(ToonName);
        if (!isLocalPlayer)
        {
            this.GetComponentInChildren<TextMesh>().transform.rotation = GameObject.FindGameObjectsWithTag("MainCamera")[0].transform.rotation;

            return;
        }

        animator.SetBool("Walking", false);
            GlobalValues.ForwardAxis = Input.GetAxis(GlobalValues.MoveForward);
            GlobalValues.BackwardAxis = Input.GetAxis(GlobalValues.MoveBackward);
            GlobalValues.StrafeAxis = Input.GetAxis(GlobalValues.Strafing);
            GlobalValues.TurningAxis = Input.GetAxis(GlobalValues.Turning);

            if (GlobalValues.ForwardAxis == 0 && GlobalValues.BackwardAxis == 0 && GlobalValues.StrafeAxis == 0 && GlobalValues.TurningAxis == 0)
            {

                animator.SetBool("Idling", true);
            }

            //is a button (if prev state pressed)
            _jumpWasPressed = GlobalValues.JumpAxis > 0;
            GlobalValues.JumpAxis = Input.GetAxis(GlobalValues.Jump);

            Quaternion currentRotation = GlobalValues.CharacterTransform.rotation;

            _currentMovement = Vector3.zero;
            //not back pedaling
            _backPedaling = false;

            //forward by mouse
            if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
            {
                animator.SetBool("Idling", false);


                currentRotation = Camera.transform.rotation;
                _currentMovement += currentRotation * Vector3.forward;

            GlobalValues.CharacterTransform.rotation = Quaternion.Euler(0f, (currentRotation.eulerAngles.y), 0f);
            }
            
            else if (GlobalValues.ForwardAxis > 0)
            {
                //if moving forward
                animator.SetBool("Idling", false);
                _currentMovement += currentRotation * Vector3.forward;
            }
            if (GlobalValues.BackwardAxis > 0)
            {
                animator.SetBool("Walking", true);
                _backPedaling = true;
                _currentMovement += currentRotation * Vector3.back;
            
            }

        if (Input.GetKeyDown(KeyCode.Return))
         {
         
            string Message = TextInput.text;
            TextInput.text = "";
            TextInput.GetComponentInParent<InputField>().text = "";

            if (Message != "")
            {

                CmdSendMessage(Message);
            }
            


        }
        

        //if strafing
        if (GlobalValues.StrafeAxis != 0)
            {

               
                if (GlobalValues.StrafeAxis > 0)
                {
                    _currentMovement += currentRotation * Vector3.right;
                }
                else
                {
                    _currentMovement += currentRotation * Vector3.left;
                }
            }
            //apply final direction
            _currentMovement = _currentMovement.normalized * (_backPedaling ? BackwardSpeed : ForwardSpeed);
            //if in air, we have little control over overall direction
            if (!IsGrounded)
            {

                animator.SetBool("Idling", true);
                _currentMovement = Vector3.Lerp(GlobalValues.CharacterRigid.velocity, _currentMovement, Time.deltaTime * AerialLerpModifier);
            }

            //if turning
            if (GlobalValues.TurningAxis != 0)
            {



                if (GlobalValues.TurningAxis > 0)
                {
                    Debug.Log("turning");
                    GlobalValues.CharacterTransform.RotateAround(Vector3.up, RotationSpeed);
                }
                else
                {
                    GlobalValues.CharacterTransform.RotateAround(Vector3.up, -RotationSpeed);

                }
            }

            //if jumping
            if (!_jumpWasPressed && GlobalValues.JumpAxis > 0)
            {
                animator.SetBool("Idling", true);
                if (IsGrounded || !IsGrounded)
                {
                    _hasJumped = true;
                }
            }
            if (IsGrounded)
            {
                _currentGravity = Vector3.zero;
            }
            else
            {
                _currentGravity += GravityDrop;
            }
        
    }
    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        float airVelocity = Mathf.Lerp(GlobalValues.CharacterRigid.velocity.y, _currentGravity.y, JumpLerpSpeed);
        if (_hasJumped)
        {
            _hasJumped = false;
            airVelocity += JumpSpeed;
        }
        if (airVelocity < -MaximumGravity) airVelocity = -MaximumGravity;
        GlobalValues.CharacterRigid.velocity = new Vector3(_currentMovement.x, airVelocity, _currentMovement.z);
        Velocity = GlobalValues.CharacterRigid.velocity;
    }



    public void SetInfos(int ID, string name)
    {

        ToonId = ID;
        ToonName = name;
    }

    [ClientRpc]
    public void RpcTriggerShareName(string Name)
    {
        CmdShareName(Name);

    }

    [ClientRpc]
    public void RpcName(string Name)
    {
        this.GetComponentInChildren<TextMesh>().text = Name;

    }

    [Command]
    public void CmdShareName(string Name)
    {

        RpcName(Name);

    }

    [Command]
    public void CmdSavePosition()
    {

        StartCoroutine(SavePosition());
        print("I AM SERVER");

    }

    IEnumerator SavePosition()
    {

        string PostToonInfo = "astamarr.fr/php/PostToonPos.php";


        while (this.gameObject)
        {


            yield return new WaitForSeconds(15);
            // on peut pas update le field.......
            WWWForm CharacterForm = new WWWForm();
            CharacterForm.AddField("XPost", this.gameObject.transform.position.x.ToString());
            CharacterForm.AddField("YPost", this.gameObject.transform.position.y.ToString());
            CharacterForm.AddField("ZPost", this.gameObject.transform.position.z.ToString());
            CharacterForm.AddField("IdPost", ToonId);

            WWW CharacterRequest = new WWW(PostToonInfo, CharacterForm);
            yield return CharacterRequest;







        }
    }
    [Command]
    void CmdSendMessage( string chatMessage)
    {
        Debug.Log(ClientScene.FindLocalObject(netId).name);
        chatMessage = ToonName + " : " + chatMessage;
        RpcReceiveChat(chatMessage);
    }

    [ClientRpc]
    public void RpcReceiveChat(string msg)
    {
        GameObject.FindGameObjectsWithTag("Chat")[0].GetComponentInChildren<Chat>().DrawMessage(msg);
 
    }




}

