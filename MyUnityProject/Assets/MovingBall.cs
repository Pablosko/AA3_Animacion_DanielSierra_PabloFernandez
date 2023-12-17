using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBall : MonoBehaviour
{
    [SerializeField]
    IK_tentacles _myOctopus;

    public IK_Scorpion scorpion;
    //movement speed in units per second
    [Range(-1.0f, 1.0f)]
    [SerializeField]
    private float _movementSpeed = 5f;

    Vector3 _dir;
    Vector3 startPos;

    bool shoot = false;

    public Transform target;

    public float shootForce;
    float shootTimeStart;
    float shootTime  = 2; //With force 1
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!shoot)
        {
            transform.rotation = Quaternion.identity;

            //get the Input from Horizontal axis
            float horizontalInput = Input.GetAxis("Horizontal");
            //get the Input from Vertical axis
            float verticalInput = Input.GetAxis("Vertical");

            //update the position
            transform.position = transform.position + new Vector3(-horizontalInput * _movementSpeed * Time.deltaTime, verticalInput * _movementSpeed * Time.deltaTime, 0);
        }
        else
        {
            float lerp = (Time.time - shootTimeStart)* shootForce / shootTime;

            this.transform.position = startPos + (_dir * lerp);
        }


    }
    void restart()
    {
        scorpion.Restart();
        shoot = false;
        this.transform.position = startPos;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(!shoot)
        {
            shootForce = scorpion.getForce();
            shootTimeStart = Time.time;
            shoot = true;
            Invoke("restart", 5);
            _myOctopus.NotifyShoot();
            startPos = this.transform.position;
            _dir = target.transform.position - this.transform.position;
        }


    }
}
