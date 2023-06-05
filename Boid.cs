using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


public class Boid : MovingObjectsRB3D
{
    [SerializeField]
    protected float distanceToBeNeighborBoid = 5f; //Distance to be considered a neighboring boid

    [SerializeField]
    protected float separationDistance = 2; // Distance to maintain separation from other boids, aka PERSONAL SPACE

    [SerializeField]
    [Max(-0.001f)]
    protected float seperationWeight = -25f; // The weight for the value of the seperation

    [SerializeField]
    protected float alightmentDistance = 4; // Distance threshold for alignment influence

    [SerializeField]
    protected float alignmentWeight = 2.0f; //The weight for our alignment force

    [SerializeField]
    protected float cohesionDistance = 1f; // Distance threshold for alignment influence

    [SerializeField]
    protected float cohesionWeight = 3f; //The weight for our cohesion influence

    [SerializeField]
    Bounds bounds; //Bounds for an area we're trapped in or staying out of

    [SerializeField]
    protected float constrainedAreaWeight = 520f; //The weight for our bounds

    [SerializeField]
    protected float minMovementChange = 0f;

    [SerializeField]
    protected bool useMovementAsAddForce = true;

    [SerializeField]
    protected bool useTotalNeighborCount = false;

    [SerializeField]
    protected bool boundsForceModOnBoundsExit = true;

    [SerializeField]
    protected bool addBoundsCenterToCohesion = false;
    
    [SerializeField]
    protected bool addBoundsExtermesToCohesion = false;

    [SerializeField]
    protected bool rotateOnEulerX = false;

    [SerializeField]
    protected bool rotateOnEulerZ = false;

    [SerializeField]
    protected bool clampEulerZ = false;

    [SerializeField]
    protected bool clampVertically = true;

    [SerializeField]
    protected bool teleportWhenOutOfBounds = true;


    Vector3 refMovementValues = Vector3.zero;
    Collider thisCollider;
    GRNG grng = new GRNG();

    private void Awake()
    {
        thisCollider = GetComponent<Collider>();
    }


    void Start()
    {
        Initialize();
        RigidBodySetup();

    
        m_MovementValues = new Vector3(grng.NextFloat() * 2, 0, grng.NextFloat() * 2);

        if(bounds.extents == Vector3.zero)
        {
            bounds = new Bounds(Vector3.zero, new Vector3(5, 0.1f, 5));
        }
    }

    private void Update()
    {
        GetMovements();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Vector3.Distance(refMovementValues, m_MovementValues) > minMovementChange || minMovementChange == 0)
        {
            if(clampVertically)
            {
                m_MovementValues.y = 0;
            }
            Move(m_MovementValues, Time.fixedDeltaTime, useMovementAsAddForce);
            refMovementValues = m_MovementValues;
        }
    }



    /// <summary>
    /// Handles getting the movement values
    /// </summary>
    protected virtual void GetMovements()
    {

        m_CurrentSpeed = m_AttributeController.GetSpeed();

        m_MovementValues = (GetBoidSteering(useTotalNeighborCount) + CalculateBoundsForce());


        if (m_IsAlwaysMoving == true)
        {
            m_MovementValues += transform.forward * m_CurrentSpeed;

            m_MovementValues = m_MovementValues.normalized;

            RotateTowards(transform.position + transform.forward + m_MovementValues, rotateOnEulerX, rotateOnEulerZ, clampEulerZ, -55, 55);
        }
        else
        {
            RotateTowards(transform.position + m_MovementValues.normalized, rotateOnEulerX, rotateOnEulerZ, clampEulerZ, -55, 55);
        }
    }



    /// <summary>
    /// Gets the bounds force for the boid
    /// </summary>
    /// <returns></returns>
    Vector3 CalculateBoundsForce()
    {
        Vector3 boundsForce = Vector3.zero;

        if (constrainedAreaWeight != 0)
        {
            Vector3 clampedPosition = Vector3.zero;
            clampedPosition.x = Mathf.Clamp(transform.position.x, bounds.center.x - bounds.extents.x, bounds.center.x + bounds.extents.x);
            clampedPosition.y = Mathf.Clamp(transform.position.y, bounds.center.y - bounds.extents.y, bounds.center.y + bounds.extents.y);
            clampedPosition.z = Mathf.Clamp(transform.position.z, bounds.center.z - bounds.extents.z, bounds.center.z + bounds.extents.z);



            //If our clamped position is not our current position...
            if (clampedPosition != transform.position)
            {
                boundsForce = clampedPosition - transform.position;
                boundsForce.Normalize();
   
                

                if(boundsForceModOnBoundsExit)
                {
                    if (bounds.Contains(transform.position) == false)
                    {

                        if (teleportWhenOutOfBounds == false)
                        {
                            //Multiply by our constraining weight
                            boundsForce *= constrainedAreaWeight;
                        }
                        else
                        {


                            Vector3 newPos = transform.position;
                            Vector2 xRange = new Vector2(bounds.center.x - bounds.size.x, bounds.center.x + bounds.size.x);
                            Vector2 yRange = new Vector2(bounds.center.y - bounds.size.y, bounds.center.y + bounds.size.y);
                            Vector2 zRange = new Vector2(bounds.center.z - bounds.size.z, bounds.center.z + bounds.size.z);
                            if (transform.position.x < xRange.x)
                            {
                                newPos.x = xRange.y;
                            }
                            else if (transform.position.x > xRange.y)
                            {
                                newPos.x = xRange.x;
                            }

                            if (transform.position.y < yRange.x)
                            {
                                newPos.y = yRange.y;
                            }
                            else if (transform.position.y > yRange.y)
                            {
                                newPos.y = yRange.x;
                            }

                            if (transform.position.z < zRange.x)
                            {
                                newPos.z = zRange.y;
                            }
                            else if (transform.position.z > zRange.y)
                            {
                                newPos.z = zRange.x;
                            }

                            transform.position = newPos;
                        }
                        
                    }
                    
                }
                else
                {
                    //Multiply by our constraining weight
                    boundsForce *= constrainedAreaWeight;
                }
            }

            
        }
        return boundsForce;
    }



    /// <summary>
    /// Gets the steering for the boid
    /// </summary>
    /// <param name="useTotalNeighbors"></param>
    /// <returns></returns>
    Vector3 GetBoidSteering(bool useTotalNeighbors = false)
    {
        // Alignment: steer towards the average velocity of neighboring boids
        Vector3 alignment = Vector3.zero;

        // Cohesion: steer towards the average position of neighboring boids 
        Vector3 cohesion = Vector3.zero;

        // Separation: steer away from neighboring boids that are too close
        Vector3 separation = Vector3.zero;

        Vector3 steering = Vector3.zero;
        Vector3 averageVelocity = Vector3.zero;
        Vector3 centerOfMass = Vector3.zero;

        int totalNeighbors = 0;
        int separationNeighborCount = 0;
        int alignmentNeighborCount = 0;
        int cohesionNeighborCount = 0;

        // Run a phsyics sphere to check for neighboring boids
        Collider[] colliders = Physics.OverlapSphere(transform.position, distanceToBeNeighborBoid);

        // Get the average velocity and center position of neighboring boids as well as the amount of neighbors
        foreach (Collider collider in colliders)
        {

            //If the collider is not null and is not this collider...
            if (collider != null && collider != thisCollider)
            {
                //Get the neighboring boid
                Boid neighborBoid = ((collider.GetComponent<Boid>() != null) ?
                    collider.GetComponent<Boid>()
                    :
                    (collider.gameObject.GetComponentInParent<Boid>() != null) ?
                    collider.gameObject.GetComponentInParent<Boid>()
                    : null);

                //If our boid is not null...
                if (neighborBoid != null && neighborBoid != this)
                {
                    //Add to the total neighbors
                    totalNeighbors++;

                    //Get the distance to the neighbor
                    float distance = Vector3.Distance(transform.position, neighborBoid.transform.position);

                    //If the distance is within range for alignment...
                    if (distance < alightmentDistance)
                    {
                        //Add to the average velocity and the alignment neighbor counts
                        averageVelocity += neighborBoid.m_rb.velocity;
                        alignmentNeighborCount++;
                    }

                    

                    //If the distance is within range for our seperation distance...
                    if (distance < separationDistance)
                    {

                        //Calculate our seperation factor
                        float separationFactor = Mathf.Clamp01(1f - distance / separationDistance);

                        //Add it to our seperation value multiplied by the modifier and the direction to the neighbor
                        separation += transform.position.DirectionTo(neighborBoid.transform.position) * separationFactor * seperationWeight;

                        //Add to our seperation neighbor count
                        separationNeighborCount++;
                    }

                    //If the distance is within range for our cohesion...
                    else if (distance > cohesionDistance)
                    {
                        //Add the neighbors position to the center off mass and add to the neighbor count
                        centerOfMass += neighborBoid.transform.position;
                        cohesionNeighborCount++;
                    }
                    
                    
                }
            }
        }

        if (addBoundsCenterToCohesion)
        {
            centerOfMass += bounds.center;
            cohesionNeighborCount++;
        }

        if(addBoundsExtermesToCohesion)
        {
            centerOfMass += bounds.max + bounds.min;
            cohesionNeighborCount += 2;
        }

        //If we're using the total neighbor count...
        if (useTotalNeighbors == true)
        {
            //If the total number of neighbors is greater than 0...
            if(totalNeighbors > 0)
            {
                //Get our alignment by getting the average velocity and normalizing
                averageVelocity /= totalNeighbors;
                alignment = averageVelocity.normalized;

                //Multiply by our alignment weight
                alignment *= alignmentWeight;

                //Get our cohesion by getting the average center of mass, subtracting the current position, and normalizing
                centerOfMass /= totalNeighbors;
                cohesion = (centerOfMass - transform.position).normalized;

                //Multiply by our cohesion weight
                cohesion *= cohesionWeight;

                //Get the average value for our seperation
                separation /= totalNeighbors;
            }
        }
        //else...
        else
        {
            //If we have alignment neighbors...
            if (alignmentNeighborCount > 0)
            {
                //Get our alignment by getting the average velocity and normalizing
                averageVelocity /= alignmentNeighborCount;
                alignment = averageVelocity.normalized;

                //Multiply by our alignment weight
                alignment *= alignmentWeight;
            }


            //If we have cohesions neighbors...
            if (cohesionNeighborCount > 0)
            {
                //Get our cohesion by getting the average center of mass, subtracting the current position, and normalizing
                centerOfMass /= cohesionNeighborCount;
                cohesion = (centerOfMass - transform.position).normalized;

                //Multiply by our cohesion weight
                cohesion *= cohesionWeight;
            }

            //If we have seperation neighbors...
            if (separationNeighborCount > 0)
            {
                //Get the average value for our seperation
                separation /= separationNeighborCount;
            }
        }

        

        //Add our values to our steering value
        steering = alignment + cohesion + separation;

        return steering;
    }


    

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward * m_CurrentSpeed));
    }
#endif
}
