using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Steering
{
    public Vector2 linear { get; set; }
    public float angular { get; set; }
}

public class enemyController : MonoBehaviour
{
    public Rigidbody2D rb;
    public Rigidbody2D rbTarget;
    public GameObject enemyObj;
    public float maxSpeed;
    public float acceleration;


    [SerializeField]
    public fieldOfView fov;

    // Start is called before the first frame update
    void Awake()
    {
        if (rb == null)
        {
            rb = enemyObj.GetComponent<Rigidbody2D>();
            rbTarget = GameObject.Find("Player").GetComponent<Rigidbody2D>();
            fov = Instantiate(GameObject.Find("FieldOfView").GetComponent<fieldOfView>());
            fov.name = "enemyFOV" + enemyObj.name;
            fov.fov = 135;
            fov.viewDistance = 3;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Steering steering = null;
        steering = move_seek();
        updateMovement(steering);

        steering = characterAvoidance();
        updateMovement(steering);

        if (fov.obj != null)
        {
            //Debug.Log(fov.obj.name);
            Vector2 temp = wallCollisionAvoidance();
            if (temp != Vector2.zero)
            {
                steering = move_seek(temp);
                updateMovement(steering);
            }
        }
        else
        {
            // Debug.Log("Null");
        }

        // Check kecepatan
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    void updateMovement(Steering steering)
    {
        if (steering == null)
        {
            rb.AddForce(new Vector2(), ForceMode2D.Impulse);
        }
        else
        {
            rb.AddForce(steering.linear * Time.deltaTime, ForceMode2D.Impulse);
            fov.curAngle = UtilsClass.GetAngleFromVector(rb.velocity.normalized) + fov.fov / 2f;
            fov.setOrigin(rb.transform.position);
        }
    }

    Steering move_seek()
    {
        Steering steering = new Steering();
        steering.linear = rbTarget.position - rb.position;
        steering.linear = steering.linear.normalized * acceleration;
        steering.angular = 0;
        return steering;
    }

    Steering move_seek(Vector2 target)
    {
        Steering steering = new Steering();
        steering.linear = target - rb.position;
        steering.linear = steering.linear.normalized * acceleration;
        steering.angular = 0;
        return steering;
    }

    Vector2 wallCollisionAvoidance()
    {
        // Paramter
        float avoidDistance = 25;
        //
        /*Vector3 rayVector = rb.velocity;
        rayVector = rayVector.normalized;
        rayVector *= lookahead;*/

        Rigidbody2D rb_target = fov.obj.GetComponent<Rigidbody2D>(); // Cari Raycast
        if (fov.obj == null)
        {
            return Vector2.zero;
        }
        Vector2 target = ((Vector2)rb_target.transform.position) + fov.rayCast.normal * avoidDistance;
        // Debug.DrawLine(rb.transform.position + new Vector3(0,0,-5), rb.transform.position + new Vector3(rb.velocity.x, rb.velocity.y, -5), Color.red, 1, true);

        return target; // Terus manggil Seek
    }

    Steering characterAvoidance() {
        float radius = 5; // Parameter
        float maxAcceleration = 3;
        // Store the first collision time
        float shortestTime = Mathf.Infinity;
        // Target Information
        Rigidbody2D firstTarget = null;
        float firstMinSeperation = 0f;
        float firstDistance = 0f;
        Vector3 firstRelativePos = new Vector3();
        Vector3 firstRelativeVel = new Vector3();
        // Search target
        var list_enemy = GameObject.FindGameObjectsWithTag("AI");
        foreach (var enemy in list_enemy)
        {
            if(enemy.GetInstanceID() == this.GetInstanceID())
            {
                continue;
            }
            Rigidbody2D target_rb = enemy.GetComponent<Rigidbody2D>();
            // Calculate the time to collision
            Vector3 relativePos = target_rb.position - rb.position;
            Vector3 relativeVel = target_rb.velocity - rb.velocity;
            float relativeSpeed = relativeVel.magnitude;
            Debug.Log("dot " + Vector3.Dot(relativePos, relativeVel));
            Debug.Log("relativeSpeed " + relativeSpeed);
            float timeToCollision = 0;
            if (Vector3.Dot(relativePos, relativeVel) != 0 && (relativeSpeed * relativeSpeed) != 0)
            {
                timeToCollision = Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);
            }
            Debug.Log("timeToCollision " + timeToCollision);

            float distance = relativePos.magnitude;
            float minSeperation = distance - relativeSpeed * shortestTime;
            if(minSeperation > 2 * radius) {
                continue;
            }
            if (timeToCollision > 0 && timeToCollision < shortestTime)
            {
                shortestTime = timeToCollision;
                firstTarget = target_rb;
                firstMinSeperation = minSeperation;
                firstDistance = distance;
                firstRelativePos = relativePos;
                firstRelativeVel = relativeVel;
            }
        }
        // Check Target
        if (firstTarget == null) {
            return null;
        }
        // Avoid
        if (firstMinSeperation <= 0 || firstDistance < 2 * radius) {
            firstRelativePos = firstTarget.position - rb.position;
        } else {
            firstRelativePos = firstRelativePos + firstRelativeVel * shortestTime;
        }

        firstRelativePos = firstRelativePos.normalized;
        Steering steering = new Steering();
        steering.linear = firstRelativePos * maxAcceleration;
        Debug.DrawLine(rb.transform.position + new Vector3(0, 0, -5), rb.transform.position + new Vector3(steering.linear.x, steering.linear.y, -5) * 10, Color.green, 1, true); ;
        Debug.DrawLine(rb.transform.position + new Vector3(0, 0, -5), rb.transform.position + new Vector3(rb.velocity.x, rb.velocity.y, -5) * 10, Color.red, 1, true); ;
        return steering;

    }

}
