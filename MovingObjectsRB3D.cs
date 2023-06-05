using System.Collections;
using UnityEngine;



[RequireComponent(typeof(Rigidbody))]
public class MovingObjectsRB3D : MovementController
{

    protected Rigidbody m_rb;


    public void AddJumpForce(ForceMode mode = ForceMode.Impulse)
	{
        
        m_rb.AddForce(CalculateJump()*Vector3.up, mode);
	}


    public void AddGravityForce()
	{
        m_rb.AddForce(CalculateFalling() * -Vector3.up);
    }

    public Quaternion AddMovementForce(Vector3 movement, Vector3 localForward, float t, Camera cam = null)
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


        var veloc = GetVelocity(movementDirection * m_CurrentSpeed, t);
        m_rb.AddForce(veloc * m_rb.mass);

        return movementRotation;
    }








    /// <summary>
    /// Gets the velocity of the object using smoothing
    /// </summary>
    /// <param name="movement"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public override Vector3 GetVelocity(Vector3 movement, float t)
    {

        Vector3 velocity = Vector3.zero;

        if (movement != Vector3.zero)
        {
            velocity = Vector3.SmoothDamp(m_rb.velocity, movement, ref v3RefVelocity, m_MovementSmoothing);
        }
        else
        {
            velocity = Vector3.SmoothDamp(m_rb.velocity, Vector3.zero, ref v3RefVelocity, m_DecelerationSmoothing);
        }

        return velocity;
    }






    public override Quaternion Move(Vector3 movement, Vector3 localForward, float t, bool useSimpleMove, Camera cam = null)
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


        m_rb.velocity = GetVelocity(movementDirection * m_CurrentSpeed, t);

        return movementRotation;
    }


    

    public override void Move(Vector3 movement, float t, bool useSimpleMove = true)
    {
        var veloc = GetVelocity(movement * m_CurrentSpeed, t);
        
        if (!useSimpleMove)
        {
            m_rb.AddForce(veloc.Clamp(m_CurrentSpeed) * m_rb.mass);
            //m_rb.AddForce(veloc * m_rb.mass);
        }
        else
        {
            m_rb.velocity = (veloc.Clamp(m_CurrentSpeed));
            //m_rb.velocity = (veloc);
        }

    }


    public override Vector3 LookAtMovementPosition()
    {
        return Vector3.SmoothDamp(transform.position, transform.position + m_rb.velocity, ref v3RefVelocity, m_RotationSmoothing);
    }

    


    public override Quaternion Move(Vector3 movement, Vector3 localForward, float t, bool useSimpleMove, float speed, Camera cam = null)
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


        m_rb.velocity = GetVelocity(movementDirection * speed, t);

        return movementRotation;
    }


    public override void Initialize()
    {
        base.Initialize();
        m_rb = GetComponent<Rigidbody>();
    }


    // <summary>
    /// Sets up the values for the rigid body
    ///</summary>
    public virtual void RigidBodySetup(bool freezeRotation = true)
    {
        if (m_rb == null)
        {
            if (gameObject.GetComponent<Rigidbody>() != null)
            {
                m_rb = gameObject.GetComponent<Rigidbody>();
            }
            else
            {
                m_rb = gameObject.AddComponent<Rigidbody>();
            }
        }

        m_rb.useGravity = !this.m_IsFlyingObject;
        m_rb.freezeRotation = freezeRotation;
        if (freezeRotation)
        {
            m_rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }



    // <summary>
    /// Sets up the values for the rigid body
    ///</summary>
    public virtual void RigidBodySetup(bool isKinematic, bool freezeRotation, bool useGravity)
    {
        if (m_rb == null)
        {
            if (gameObject.GetComponent<Rigidbody>() != null)
            {
                m_rb = gameObject.GetComponent<Rigidbody>();
            }
            else
            {
                m_rb = gameObject.AddComponent<Rigidbody>();
            }
        }

        m_rb.useGravity = useGravity;
        m_rb.mass = m_AttributeController.GetMass();
        if (freezeRotation)
        {
            m_rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }


    // <summary>
    /// Sets up the values for the rigid body
    ///</summary>
    public virtual void RigidBodySetup(bool isKinematic, bool freezeRotation, bool useGravity, float mass, RigidbodyInterpolation interpolationMode)
    {
        if (m_rb == null)
        {
            if (gameObject.GetComponent<Rigidbody>() != null)
            {
                m_rb = gameObject.GetComponent<Rigidbody>();
            }
            else
            {
                m_rb = gameObject.AddComponent<Rigidbody>();
            }
        }


        m_rb.useGravity = useGravity;
        m_rb.interpolation = interpolationMode;
        m_rb.mass = mass;
        m_rb.isKinematic = isKinematic;
        if (freezeRotation) m_rb.constraints = RigidbodyConstraints.FreezeRotation;
    }





}