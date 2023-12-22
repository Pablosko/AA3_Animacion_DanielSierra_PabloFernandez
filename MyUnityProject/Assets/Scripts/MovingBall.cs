using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovingBall : MonoBehaviour
{

    public Slider slider;
    [HideInInspector] public Vector3 shootDirection;

    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public float angularVelocity = 0.1f;
    [HideInInspector] public Vector3 acceleration;
    [HideInInspector] public bool forceApplied;

    private float magnusForce = 0;


    [SerializeField]
    IK_tentacles _myOctopus;

    public IK_Scorpion scorpion;


    [Range(-1.0f, 1.0f)]
    [SerializeField]
    private float _movementSpeed = 5f;

    Vector3 _dir;
    Vector3 startPos;

    bool moving = false;

    public Transform target;
    public int curvePoints = 20;
    public GameObject curvePoint;
    public List<GameObject> curvePointsGameObjects;
    public float shootForce;
    float shootTimeStart;
    float shootTime  = 2; //With force 1

    int shootCount = 0;
    void Start()
    {
        slider.minValue = -1;
        slider.maxValue = 1;


        for (int i = 0; i < curvePoints; i++)
        {
            curvePointsGameObjects.Add(Instantiate(curvePoint));
        }
    }

    // Update is called once per frame
    void Update()
    {

        slider.value = magnusForce;

        if (Input.GetKey(KeyCode.Z))
        {            
            if(magnusForce > -1)
                magnusForce -= Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.X))
        {
            if (magnusForce < 1)
                magnusForce += Time.deltaTime;

        }


        if (!moving)
            CalculateTrajectory();
        else
            MoveBall();
    }
    void MoveBall()
    {
            Vector2 auxMagnus = new Vector2(velocity.x, velocity.z).normalized; 

            auxMagnus = -Vector2.Perpendicular(auxMagnus).normalized;
            acceleration = new Vector3(auxMagnus.x, 0, auxMagnus.y);

            Vector3 gravity = new Vector3(0, -1, 0);
            acceleration *= magnusForce * 2;

            acceleration += gravity;

            velocity += acceleration * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.rotation *= Quaternion.AngleAxis(angularVelocity* magnusForce * Time.deltaTime, new Vector3(0, 1, 0));
            
    }
    void CalculateTrajectory() 
    {
        transform.rotation = Quaternion.identity;

        //get the Input from Horizontal axis
        float horizontalInput = Input.GetAxis("Horizontal");
        //get the Input from Vertical axis
        float verticalInput = Input.GetAxis("Vertical");

        //update the position
        transform.position = transform.position + new Vector3(-horizontalInput * _movementSpeed * Time.deltaTime, verticalInput * _movementSpeed * Time.deltaTime, 0);




    }
    void restart()
    {
        scorpion.Restart();
        moving = false;
        transform.position = startPos;
        shootDirection = Vector3.zero;
        velocity = Vector3.zero;
        angularVelocity = 0;
        acceleration = Vector3.zero;

    }
    private void OnCollisionEnter(Collision collision)
    {
        if(!moving)
        {
            shootCount++;
            shootDirection = (target.transform.position - transform.position).normalized;

            shootForce = scorpion.getForce();

            velocity += shootDirection * shootForce * 3;

            shootTimeStart = Time.time;
            moving = true;
            Invoke("restart", 5);
            _myOctopus.NotifyShoot(shootCount);
            startPos = transform.position;
        }
    }
}
