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


    public fieldOfView fov;
    public fieldOfView blindSpotFOV;

    private static int ctr_id = 0;
    private int id;
    public enemyController() {
        id = ctr_id++;
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (rb == null)
        {
            rb = enemyObj.GetComponent<Rigidbody2D>();
            rbTarget = GameObject.Find("Player").GetComponent<Rigidbody2D>();
            fov = Instantiate(GameObject.Find("FieldOfView").GetComponent<fieldOfView>());
            fov.name = "enemyFOV" + enemyObj.name;
            fov.fov = 140;
            fov.viewDistance = 5;

            blindSpotFOV = Instantiate(GameObject.Find("FieldOfView").GetComponent<fieldOfView>());
            blindSpotFOV.name = "enemyblindSpotFOV" + enemyObj.name;
            blindSpotFOV.fov = 400;
            blindSpotFOV.viewDistance = 2;
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

        if (fov.obj != null || blindSpotFOV.obj != null)
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
            blindSpotFOV.setOrigin(rb.transform.position);
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
        float avoidDistance = 50;
        Vector2 target = Vector2.zero;
        if(fov.obj != null)
        {
            Debug.Log("fov masuk");
            Rigidbody2D rb_target = fov.obj.GetComponent<Rigidbody2D>(); // Cari rb dari object yang intersect dengan Raycast
            target = ((Vector2)rb_target.transform.position) + fov.rayCast.normal * avoidDistance;
        }
        else if(blindSpotFOV.obj != null)
        {
            Debug.Log("blindSpotFOV masuk");
            Rigidbody2D rb_target = blindSpotFOV.obj.GetComponent<Rigidbody2D>();
            target = ((Vector2)rb_target.transform.position) + blindSpotFOV.rayCast.normal * avoidDistance;
        }
        // Debug.DrawLine(rb.transform.position + new Vector3(0,0,-5), rb.transform.position + new Vector3(rb.velocity.x, rb.velocity.y, -5), Color.red, 1, true);

        return target; // Terus manggil Seek
    }

    Steering characterAvoidance() {
        // return variable
        Steering steering = null;
        // Constant
        float radius = 2;
        // Temp
        Rigidbody2D firstRb = null;
        float firstLength = Mathf.Infinity;
        // Iterate Setiap AI
        var list_enemy = GameObject.FindGameObjectsWithTag("AI");
        foreach (var enemy in list_enemy) {
            if (enemy.GetComponent<enemyController>().id == this.id) {
                continue;
            }
            Rigidbody2D rb_target = enemy.GetComponent<Rigidbody2D>();
            // Get Prediction Position (This Enemy)
            Vector2 predThis = new Vector2(rb.transform.position.x, rb.transform.position.y) + rb.velocity;
            // Get Prediction Position (Other Enemy)
            Vector2 predOther = new Vector2(rb_target.transform.position.x, rb_target.transform.position.y) + rb_target.velocity;
            // Dapetin Selisih Posisi
            Vector2 diff = predThis - predOther;
            float length_diff = diff.magnitude;
            if (length_diff > radius) {
                continue;
            } else {
                Debug.Log("Length Diff: " + length_diff);
                Debug.Log("MASUK");
                Debug.DrawLine(predThis, predOther, Color.red, 10000, true);
                if (length_diff < firstLength) {
                    firstRb = rb_target;
                    firstLength = length_diff;
                }
            }
            
        }
        // Check apakah ketemu target
        if (firstRb == null) {
            return null;
        } else {
            // Action AI menjauhi AI lainnya
            Debug.Log("OTW NABRAK");
            rb.AddForce(new Vector2(rb.velocity.y, -rb.velocity.x), ForceMode2D.Impulse);
            Debug.DrawLine(rb.position, rb.position + rb.velocity, Color.green);
        }
        return steering;
    }

}
