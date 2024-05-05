using UnityEngine.UI;
using UnityEngine;

enum Behavior { Idle, Seek, Evade, Flee }
enum State { Idle, Arrive, Seek, Evade }

[RequireComponent(typeof(Rigidbody2D))]
public class SteeringActor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Behavior behavior = Behavior.Seek;
    [SerializeField] Transform target = null;
    [SerializeField] float maxSpeed = 4f;
    [SerializeField, Range(0.1f, 0.99f)] float decelerationFactor = 0.75f;
    [SerializeField] float arriveRadius = 1.2f;
    [SerializeField] float stopRadius = 0.5f;
    [SerializeField] float evadeRadius = 5f;
    [SerializeField] float fleeRadius = 5f;


    Rigidbody2D physics;
    State state = State.Idle;
    Animator animator;
    SpriteRenderer sprite;

    void FixedUpdate()
    {
        if (target != null)
        {
            switch (behavior)
            {
                case Behavior.Idle: IdleBehavior(); break;
                case Behavior.Seek: SeekBehavior(); break;
                case Behavior.Evade: EvadeBehavior(); break;
                case Behavior.Flee: FleeBehavior(); break;
            }
        }

        physics.velocity = Vector2.ClampMagnitude(physics.velocity, maxSpeed);

    }

    void IdleBehavior()
    {
        physics.velocity = physics.velocity * decelerationFactor;
        
    }

    void SeekBehavior()
    {
        Vector2 delta = target.position - transform.position;
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if (distance < stopRadius)
        {
            state = State.Idle;
        }
        else if (distance < arriveRadius)
        {
            state = State.Arrive;
        }
        else
        {
            state = State.Seek;
        }

        switch (state)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Arrive:
                var arriveFactor = 0.01f + (distance - stopRadius) / (arriveRadius - stopRadius);
                physics.velocity += arriveFactor * steering * Time.fixedDeltaTime;
                
                break;
            case State.Seek:
                physics.velocity += steering * Time.fixedDeltaTime;
                
                break;
        }
    }

    void EvadeBehavior()
    {
        Vector2 delta = target.position - transform.position;
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if (distance > evadeRadius)
        {
            state = State.Idle;
        }
        else
        {
            state = State.Evade;
        }

        switch (state)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Evade:
                physics.velocity -= steering * Time.fixedDeltaTime;
                break;
        }
    }

    void FleeBehavior()
    {
        
        Vector2 desiredVelocity = (transform.position - target.position).normalized * maxSpeed;
        Vector2 steering = desiredVelocity - physics.velocity;
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
      
        if (distanceToTarget <= fleeRadius)
        {
           
            state = State.Evade; 
        }
        else
        {
            state = State.Idle; 
        }

        switch (state)
        {
            case State.Idle:
                IdleBehavior();
                physics.velocity -= steering * Time.fixedDeltaTime;
                break;
            case State.Evade:
                physics.velocity += steering * Time.fixedDeltaTime;
                break;
        }

    }




    void Awake()
    {
        physics = GetComponent<Rigidbody2D>();
        physics.isKinematic = true;
    }

    void OnDrawGizmos()
    {
        if (target == null)
        {
            return;
        }

        switch (behavior)
        {
            case Behavior.Idle:
                break;
            case Behavior.Seek:
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, arriveRadius);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, stopRadius);
                break;
            case Behavior.Evade:
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, evadeRadius);
                break;
            case Behavior.Flee:
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, fleeRadius);
                break;

        }

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(transform.position, target.position);
    }
}
