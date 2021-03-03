using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class playerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject playerObj;
    public float maxSpeed;
    public Animator animator;
    private Vector2 force;
    private int look;


    public Light2D light;

   [SerializeField]
    public fieldOfView fov;

    void Awake()
    {
        if (rb == null)
        {
            playerObj = GameObject.Find("Player");
            rb = playerObj.GetComponent<Rigidbody2D>();
            animator = playerObj.GetComponent<Animator>();
            fov = Instantiate(GameObject.Find("FieldOfView").GetComponent<fieldOfView>());
            light = playerObj.GetComponent<Light2D>();
           
        }
        look = 4;
    }

    private void Update()
    {
        processInput();
    }

    void FixedUpdate()
    {
        move();
        walkAnim();
        updateOrientationPlayer();
        spriteOrientation();
    }

    private float moveX, moveY;

    void processInput()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        float xForce = moveX * maxSpeed;
        float yForce = moveY * maxSpeed;

        force = new Vector2(xForce, yForce);
    }

    void move()
    {
        if (moveX == 0 && moveY == 0)
        {
            rb.drag = 1;
        }
        rb.AddForce(force);
    }

    public void updateOrientationPlayer()
    {
        Vector3 targetPosition = UtilsClass.GetWorldPositionFromUI();
        fov.setAimDirection((targetPosition - transform.position).normalized);
        fov.setOrigin(transform.position);
        light.transform.rotation = Quaternion.Euler(0, 0, fov.curAngle - fov.fov * 2f);
    }

    void spriteOrientation()
    {
        if (fov.curAngle > 60 && fov.curAngle < 170) look = 1;
        else if (fov.curAngle > 170 && fov.curAngle < 250) look = 3;
        else if (fov.curAngle > 250 && fov.curAngle < 330) look = 4;
        else if (fov.curAngle < 60 || fov.curAngle > 330) look = 2;

        animator.SetInteger("Look", look);
        look = 0;
    }

    void walkAnim()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x + rb.velocity.y));
        if (Mathf.Abs(rb.velocity.x + rb.velocity.y) > 2) animator.speed = 1.3f;
        else animator.speed = 0.6f;
    }
}
