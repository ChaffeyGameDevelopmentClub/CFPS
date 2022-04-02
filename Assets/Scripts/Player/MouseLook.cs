using UnityEngine;
public class MouseLook : MonoBehaviour
{
    private GameObject playerCapsule;
    [SerializeField] private float minY = -30;
    [SerializeField] private float maxY = 30;
    public Vector2 targetRotation;
    private Vector2 currentRotation;
    private Vector2 currentRotationVelocity = new Vector2(0, 0);
    [SerializeField] private float sensitivity = 3;
    [SerializeField] private float lookSmoothDamp = 0.1f;
    [SerializeField] private float cameraOffset = 0.679f;

    void Start()
    {
        playerCapsule = GameObject.Find("PlayerCapsule");
    }

    //Process Input
    void Update()
    {
        targetRotation.y += Input.GetAxis("Mouse Y") * sensitivity;
        targetRotation.x += Input.GetAxis("Mouse X") * sensitivity;
        targetRotation.y = Mathf.Clamp(targetRotation.y, minY, maxY);
    }

    //Process Physics
    private void FixedUpdate()
    {
        //Rotate camera
        currentRotation.y = Mathf.SmoothDamp(currentRotation.y, targetRotation.y, ref currentRotationVelocity.y, lookSmoothDamp);
        currentRotation.x = Mathf.SmoothDamp(currentRotation.x, targetRotation.x, ref currentRotationVelocity.x, lookSmoothDamp);
        transform.localEulerAngles = new Vector3(-currentRotation.y, currentRotation.x, 0);

        //Player capsule matches camera rotation
        playerCapsule.transform.localEulerAngles = new Vector3(0, currentRotation.x, 0);
        //Move camera to player position
        transform.position = new Vector3(playerCapsule.transform.position.x, playerCapsule.transform.position.y + cameraOffset, 
            playerCapsule.transform.position.z);
    }
}