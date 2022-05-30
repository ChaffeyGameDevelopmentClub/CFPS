using UnityEngine;
using InputType = InputProcess.InputType;


public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float jumpMultiplier = 500;
    private float xInput, yInput; //Used for input processing

    //Specific Bindings
    private InputProcess sprintControlProcess = new InputProcess("Sprint", InputType.CONSTANT, KeyCode.LeftShift);
    private InputProcess crouchControlProcess = new InputProcess("Crouch", InputType.CONSTANT, KeyCode.LeftControl);

    private Camera playerCamera;

    CapsuleCollider cc;
    Rigidbody rb;
    PlayerState playerState = PlayerState.Grounded;

    enum PlayerState 
    { 
        Jumping,
        Airborne,
        Grounded
    }


    //Initialize Player
    void Start()
    {
        GameObject.Find("PlayerCamera");
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CapsuleCollider>();
    }

    //Process Input
    void Update()
    {
        sprintControlProcess.Process();
        crouchControlProcess.Process();

        if (Physics.Raycast(transform.position, Vector3.down, cc.bounds.extents.y + 0.01f) && playerState != PlayerState.Jumping)
            playerState = PlayerState.Grounded;

        xInput = Input.GetAxis("Horizontal") * speed;
        xInput *= (sprintControlProcess.inputDown) ? sprintMultiplier : 1;
        yInput = Input.GetAxis("Vertical") * speed;
        yInput *= (sprintControlProcess.inputDown) ? sprintMultiplier : 1;

        if (Input.GetKeyDown("escape"))
            Cursor.lockState = CursorLockMode.None;

        if (Input.GetButtonDown("Jump") && playerState == PlayerState.Grounded) {
            playerState = PlayerState.Jumping;
        }
    }

    //Process Physics
    private void FixedUpdate()
    {
        transform.Translate(xInput * Time.deltaTime, 0, yInput * Time.deltaTime);
        
        if (playerState == PlayerState.Jumping)
        {
            rb.AddForce(Vector3.up * jumpMultiplier, ForceMode.Impulse);
            playerState = PlayerState.Airborne;
        }
    }
}