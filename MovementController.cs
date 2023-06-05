using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AttributeController))]
public class MovementController : MonoBehaviour
{

    [Header("The layers that are considered ground")]
    public LayerMask m_GroundLayers;

    //The attribute controller for this object
    protected AttributeController m_AttributeController;

    
    [Header("The speed limiter associated with the object when moving diagonally")]
    [SerializeField]
    protected float m_DiagonalSpeed = 0.75f;

    //The current speed of the object
    protected float m_CurrentSpeed = 0;

    [Header("The value for y axis movement, generally is jumping")]
    [SerializeField]
    protected float m_YAxisForce = 10f;

    [Header("How much movement should be smoothed when smoothing movement")]
    [SerializeField]
    [Min(0)]
    protected float m_MovementSmoothing = 1f;

    [Header("How much movement should be smoothed when smoothing movement and decelerating")]
    [SerializeField]
    [Min(0)]
    protected float m_DecelerationSmoothing = 1f;

    [Header("How much rotation should be smoothed when smoothing rotation")]
    [SerializeField]
    [Min(0)]
    protected float m_RotationSmoothing = 1f;

    [Header("The radius when checking for ground. If checking using a ray, will be the ray distance.")]
    [SerializeField]
    protected float m_GroundCheckRadius = 1f;

    [Header("If this object is a flying object, as in no gravity")]
    [SerializeField]
    protected bool m_IsFlyingObject = false;

    [Header("If this object is always moving")]
    [SerializeField]
    protected bool m_IsAlwaysMoving = false;

    //The movement direction values for moving
    [SerializeField]
    protected Vector3 m_MovementValues = Vector3.zero; 

    //Float reference to velocity
    protected float refVelocity;

    //Vector 3 reference to velocity
    protected Vector3 v3RefVelocity;

    /// <summary>
    /// Initializes the variables and movement
    /// </summary>
    public virtual void Initialize()
	{
        m_AttributeController = transform.GetComponent<AttributeController>();
    }


    /// <summary>
    /// Calculates jump force
    /// </summary>
    /// <returns></returns>
    public float CalculateJump()
    {
        return Mathf.Sqrt(m_YAxisForce * m_AttributeController.GetGravityScale() * m_AttributeController.GetGravity());
    }


    /// <summary>
    /// Gets the velocity of the object using smoothing
    /// </summary>
    /// <param name="movement"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public virtual Vector3 GetVelocity(Vector3 movement, float t)
    {

        Vector3 velocity = Vector3.zero;

        if (movement != Vector3.zero)
        {
            velocity = Vector3.SmoothDamp(v3RefVelocity, movement * m_CurrentSpeed, ref v3RefVelocity, m_MovementSmoothing);
        }
        else
        {
            velocity = Vector3.SmoothDamp(v3RefVelocity, Vector3.zero, ref v3RefVelocity, m_DecelerationSmoothing);
        }

        return velocity;
    }


    public virtual Vector3 LookAtMovementPosition()
    {
        return Vector3.SmoothDamp(transform.position, transform.position + v3RefVelocity, ref v3RefVelocity, m_RotationSmoothing);
    }


    /// <summary>
    /// Calculates falling force
    /// </summary>
    /// <returns></returns>
    public float CalculateFalling()
    {
        return Mathf.Sqrt(m_YAxisForce * m_AttributeController.GetFallingGravityScale() * m_AttributeController.GetFallingGravity());
    }



    public virtual Quaternion Move(Vector3 movement, Vector3 localForward, float t, bool useSimpleMove, Camera cam = null)
	{
        Vector3 movementDirection = Vector3.zero;
        Quaternion movementRotation;

        if (cam != null)
        {
            GetMovementDirection(cam, movement.x, movement.z, localForward, out movementDirection, out movementRotation);
        }
        else
        {
            GetMovementDirection(movement.x, movement.z, localForward, out movementDirection, out movementRotation);
        }

        transform.Translate(movementDirection * t * m_CurrentSpeed);


        return movementRotation;
    }



    public virtual void Move(Vector3 movement, float t, bool useSimpleMove)
    {

        if (!useSimpleMove)
        {
            transform.Translate(movement * t * m_CurrentSpeed * m_AttributeController.GetMass());
        }
        else
        {
            transform.Translate(movement * t * m_CurrentSpeed);
        }

    }



    public virtual Quaternion Move(Vector3 movement, Vector3 localForward, float t, bool useSimpleMove, float speed, Camera cam = null)
	{
        Vector3 movementDirection = Vector3.zero;
        Quaternion movementRotation;

        if (cam != null)
        {
            GetMovementDirection(cam, movement.x, movement.z, localForward, out movementDirection, out movementRotation);
        }
        else
        {
            GetMovementDirection(movement.x, movement.z, localForward, out movementDirection, out movementRotation);
        }

        transform.Translate(movementDirection * t * speed);


        return movementRotation;
    }

    /// <summary>
    /// Gets the values for movement direction and rotation
    /// </summary>
    /// <param name ="cam">The camera being used</param>
    /// <param name="xAxis"></param>
    /// <param name="zAxis"></param>
    /// <param name="localForward"></param>
    /// <param name="movementDirection"></param>
    /// <param name="newRotation"></param>
    public virtual void GetMovementDirection(Camera cam, float xAxis, float zAxis, Vector3 localForward, out Vector3 movementDirection, out Quaternion newRotation)
    {

        //Get the angle between the x axis and the z axis of the object using Atan2
        var targetAngle = Mathf.Atan2(xAxis, zAxis);

        //Convert it to degrees
        targetAngle *= Mathf.Rad2Deg;

        //Add the angle to the target angle so the players angle and camera angle matches up correctly
        targetAngle += cam.transform.eulerAngles.y;

        refVelocity = 0;

        //Get the appropriate angle by applying a smooth damp between the the current angle and the target angle
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref refVelocity, m_RotationSmoothing);

        //Set the direction of movement to the direction and the target angle that includes the cameras 
        movementDirection = Quaternion.Euler(0, angle, 0f) * localForward;

        //Rotate the transform towards the target angle
        newRotation = Quaternion.Euler(0, angle, 0f);


    }


    /// <summary>
    /// Gets the values for movement direction and rotation
    /// </summary>
    /// <param name ="rotationTarget">The target to move towards</param>
    /// <param name="xAxis"></param>
    /// <param name="zAxis"></param>
    /// <param name="localForward"></param>
    /// <param name="movementDirection"></param>
    /// <param name="newRotation"></param>
    public virtual void GetMovementDirection(Transform rotationTarget, float xAxis, float zAxis, Vector3 localForward, out Vector3 movementDirection, out Quaternion newRotation)
    {

        //Get the angle between the x axis and the z axis of the object using Atan2
        var targetAngle = Mathf.Atan2(xAxis, zAxis);

        //Convert it to degrees
        targetAngle *= Mathf.Rad2Deg;

        //Add the angle to the target angle so the players angle and camera angle matches up correctly
        targetAngle += rotationTarget.eulerAngles.y;

        refVelocity = 0;

        //Get the appropriate angle by applying a smooth damp between the the current angle and the target angle
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref refVelocity, m_RotationSmoothing);

        //Set the direction of movement to the direction and the target angle that includes the cameras 
        movementDirection = Quaternion.Euler(0, angle, 0f) * localForward;

        //Rotate the transform towards the target angle
        newRotation = Quaternion.Euler(0, angle, 0f);


    }


    /// <summary>
    /// Gets the values for movement direction and rotation
    /// </summary>
    /// <param name="xAxis"></param>
    /// <param name="zAxis"></param>
    /// <param name="localForward"></param>
    /// <param name="movementDirection"></param>
    /// <param name="newRotation"></param>
    public virtual void GetMovementDirection(float xAxis, float zAxis, Vector3 localForward, out Vector3 movementDirection, out Quaternion newRotation)
    {

        //Get the angle between the x axis and the z axis of the object using Atan2
        var targetAngle = Mathf.Atan2(xAxis, zAxis);

        //Convert it to degrees
        targetAngle *= Mathf.Rad2Deg;

        refVelocity = 0;

        //Get the appropriate angle by applying a smooth damp between the the current angle and the target angle
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref refVelocity, m_RotationSmoothing);

        //Set the direction of movement to the direction and the target angle that includes the cameras 
        movementDirection = Quaternion.Euler(0, angle, 0f) * localForward;

        //Rotate the transform towards the target angle
        newRotation = Quaternion.Euler(0, angle, 0f);


    }




    /// <summary>
    /// Performs a sphere check on the ground layer mask
    /// </summary>
    /// <returns></returns>
    public virtual bool GroundCheckSpere(Vector3 positionToCheck)
    {
        bool isGrounded = true;

        if (m_IsFlyingObject == false)
        {
            isGrounded = (Physics.CheckSphere(positionToCheck, m_GroundCheckRadius, m_GroundLayers)) ? true : false;
            
        }
        

        return isGrounded;

    }




    /// <summary>
    /// Ground check with a raycast
    /// </summary>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    public virtual bool GroundCheckRay(float maxDistance = 1.1f)
    {
        bool grounded = true;

        if (m_IsFlyingObject == false)
        {
            Ray ray = new Ray(transform.position, -transform.up); //Get downwards ray
            RaycastHit hit; //Hit for the raycast
            grounded = (Physics.Raycast(ray, out hit, maxDistance, m_GroundLayers)) ? true : false;

        }
        

        return grounded;
    }


    /// <summary>
	/// Rotates this transform towards the position passed
	/// </summary>
	/// <param name="positionToFace"></param>
    public void RotateTowards(Vector3 positionToFace, bool rotateOnX, bool rotateOnZ = false, bool clampZ = false, float minZClamp = -80, float maxZClamp = 80)
    {
        //Get the direction towards the position
        Vector3 dirTo = positionToFace.DirectionFrom(transform.position);

        //Get the angle between the x axis and the z axis of the object using Atan2
        var targetAngle = Mathf.Atan2(dirTo.x, dirTo.z);

        //Convert it to degrees
        targetAngle *= Mathf.Rad2Deg;

        var targetYAngle = transform.eulerAngles.x + dirTo.y; //Mathf.Atan2(dirTo.y, dirTo.z) * Mathf.Rad2Deg;
        var targetZAngle = transform.eulerAngles.z + dirTo.x; //Mathf.Atan2(dirTo.y, dirTo.z) * Mathf.Rad2Deg;

        //Get the appropriate angle by applying a smooth damp between the the current angle and the target angle
        var angleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref refVelocity, m_RotationSmoothing);
        var angleX = (rotateOnX) ? Mathf.SmoothDampAngle(transform.eulerAngles.x, targetYAngle, ref refVelocity, m_RotationSmoothing) : 0;
        var angleZ = (rotateOnZ) ? Mathf.SmoothDampAngle(transform.eulerAngles.z, targetZAngle, ref refVelocity, m_RotationSmoothing) : 0;
        //var angleZ = (rotateOnZ) ? Mathf.SmoothDampAngle(transform.eulerAngles.z, -targetAngle, ref refVelocity, m_RotationSmoothing) : 0;

        if (clampZ)
        {
            angleZ = Mathf.Clamp(angleZ, minZClamp,maxZClamp);
        }


        Quaternion newRotation; 

        //Set the direction of movement to the direction and the target angle
        newRotation = Quaternion.Euler(angleX, angleY, angleZ);

        //Set our rotation
        transform.rotation = newRotation;
    }


    /// <summary>
	/// Rotates this transform towards the position passed
	/// </summary>
	/// <param name="positionToFace"></param>
	public void RotateTowards(Vector3 positionToFace)
    {
        //Get the direction towards the position
        Vector3 dirTo = positionToFace.DirectionFrom(transform.position);

        //Get the angle between the x axis and the z axis of the object using Atan2
        var targetAngle = Mathf.Atan2(dirTo.x, dirTo.z);
        
        //Convert it to degrees
        targetAngle *= Mathf.Rad2Deg;

        //Get the appropriate angle by applying a smooth damp between the the current angle and the target angle
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref refVelocity, m_RotationSmoothing);

        Quaternion newRotation;

        //Set the direction of movement to the direction and the target angle
        newRotation = Quaternion.Euler(0, angle, 0f);

        //Set our rotation
        transform.rotation = newRotation;

    }



    /// <summary>
	/// Rotates this transform towards the position passed
	/// </summary>
	/// <param name="positionToFace"></param>
	public void RotateTowards(out Quaternion rotation, Vector3 positionToFace)
    {
        //Get the direction towards the position
        Vector3 dirTo = positionToFace.DirectionFrom(transform.position);

        //Get the angle between the x axis and the z axis of the object using Atan2
        var targetAngle = Mathf.Atan2(dirTo.x, dirTo.z);

        //Convert it to degrees
        targetAngle *= Mathf.Rad2Deg;

        //Get the appropriate angle by applying a smooth damp between the the current angle and the target angle
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref refVelocity, m_RotationSmoothing);

        //Set the direction of movement to the direction and the target angle that includes the cameras 
        var newRotation = Quaternion.Euler(0, angle, 0f);

        //Set our rotation
        rotation = newRotation;

    }



    /// <summary>
	/// Rotates this transform towards the position passed
	/// </summary>
	/// <param name="positionToFace"></param>
	public void RotateTowards(Vector3 positionToFace, float smoothingSpeed)
    {
        //Get the direction towards the position
        Vector3 dirTo = positionToFace.DirectionFrom(transform.position);

        //Get the angle between the x axis and the z axis of the object using Atan2
        var targetAngle = Mathf.Atan2(dirTo.x, dirTo.z);

        //Convert it to degrees
        targetAngle *= Mathf.Rad2Deg;

        //Get the appropriate angle by applying a smooth damp between the the current angle and the target angle
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref refVelocity, smoothingSpeed);

        //Set the direction of movement to the direction and the target angle that includes the cameras 
        var newRotation = Quaternion.Euler(0, angle, 0f);

        //Set our rotation
        transform.rotation = newRotation;

    }



    /// <summary>
	/// Rotates this transform towards the position passed
	/// </summary>
	/// <param name="positionToFace"></param>
	public void RotateTowards(out Quaternion rotation, Vector3 positionToFace, float smoothingSpeed)
    {
        //Get the direction towards the position
        Vector3 dirTo = positionToFace.DirectionFrom(transform.position);

        //Get the angle between the x axis and the z axis of the object using Atan2
        var targetAngle = Mathf.Atan2(dirTo.x, dirTo.z);

        //Convert it to degrees
        targetAngle *= Mathf.Rad2Deg;

        //Get the appropriate angle by applying a smooth damp between the the current angle and the target angle
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref refVelocity, smoothingSpeed);

        //Set the direction of movement to the direction and the target angle that includes the cameras 
        var newRotation = Quaternion.Euler(0, angle, 0f);

        //Set our rotation
        rotation = newRotation;

    }


}
