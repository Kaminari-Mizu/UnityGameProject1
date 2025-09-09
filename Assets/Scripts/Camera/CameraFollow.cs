using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; //Player to follow
    public Transform lookAtTarget; //Special looking point object
    public Vector3 offset = new Vector3(0.5f, 3.5f, -5);// Over-the-shoulder effect
    public float smoothSpeed = 0.125f;
    public float rotationSpeed = 5f;

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) return;

        //Rotate target with mouse X
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        target.Rotate(Vector3.up * mouseX);

        //Position camera
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        //Look at target
        transform.LookAt(lookAtTarget.position); //Focus on Player's center
        //target.position + Vector3.up * 1f
    }
}
