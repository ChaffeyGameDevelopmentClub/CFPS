using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public float speed = 5;
    [SerializeField] public float jumpMultiplier = 500;
    private float xInput, yInput, zInput = 0; //Used for input processing

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
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CapsuleCollider>();
    }

    //Process Input
    void Update()
    {
        if (Physics.Raycast(transform.position, Vector3.down, cc.bounds.extents.y + 0.01f) && playerState != PlayerState.Jumping)
            playerState = PlayerState.Grounded;

        xInput = Input.GetAxis("Horizontal") * speed;
        yInput = Input.GetAxis("Vertical") * speed;

        if (Input.GetKeyDown("escape"))
            Cursor.lockState = CursorLockMode.None;

        if (Input.GetButtonDown("Jump") && playerState == PlayerState.Grounded) {
            playerState = PlayerState.Jumping;
        }
        Debug.Log(playerState);
    }

    //Process Physics
    private void FixedUpdate()
    {
        transform.Translate(xInput *= Time.deltaTime, 0, yInput * Time.deltaTime);
        
        if (playerState == PlayerState.Jumping)
        {
            rb.AddForce(Vector3.up * jumpMultiplier, ForceMode.Impulse);
            playerState = PlayerState.Airborne;
        }
    }
}