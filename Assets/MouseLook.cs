using UnityEngine;
public class MouseLook : MonoBehaviour
{
    private GameObject player;
    [SerializeField] private float minY = -45;
    [SerializeField] private float maxY = 45;
    public Vector2 targetRotation;
    private Vector2 currentRotation;
    private Vector2 currentRotationVelocity = new Vector2(0, 0);
    [SerializeField] public float sensitivity = 3;
    [SerializeField] public float lookSmoothDamp = 0.1f;

    void Start()
    {
        player = transform.parent.gameObject;
    }

    //Process Input
    void Update()
    {
        targetRotation.y += Input.GetAxis("Mouse Y") * sensitivity;
        targetRotation.y = Mathf.Clamp(targetRotation.y, minY, maxY);
    }

    //Process Physics
    private void FixedUpdate()
    {
        player.transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * sensitivity);
        currentRotation.y = Mathf.SmoothDamp(currentRotation.y, targetRotation.y, ref currentRotationVelocity.y, lookSmoothDamp);
        transform.localEulerAngles = new Vector3(-currentRotation.y, 0, 0);
    }
}