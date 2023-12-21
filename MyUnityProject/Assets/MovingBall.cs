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

    bool moving = false;

    public Transform target;
    public int curvePoints = 20;
    public GameObject curvePoint;
    public List<GameObject> curvePointsGameObjects;
    public float shootForce;
    float shootTimeStart;
    float shootTime  = 2; //With force 1
    void Start()
    {
        for (int i = 0; i < curvePoints; i++)
        {
            curvePointsGameObjects.Add(Instantiate(curvePoint));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!moving)
            CalculateTrajectory();
        else
            MoveBall();
    }
    void MoveBall()
    {
        float lerp = (Time.time - shootTimeStart) * shootForce / shootTime;

        transform.position = startPos + (_dir * lerp);
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

        for (int i = 0; i < curvePoints; i++) 
        {
            //curvePointsGameObjects[i].transform.position = CalculatePointOnParabola(transform.position, target.position, 1, (float)i / (float)curvePoints);
        }


    }
    void restart()
    {
        scorpion.Restart();
        moving = false;
        transform.position = startPos;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(!moving)
        {
            shootForce = scorpion.getForce();
            shootTimeStart = Time.time;
            moving = true;
            Invoke("restart", 5);
            _myOctopus.NotifyShoot();
            startPos = transform.position;
            _dir = target.transform.position - transform.position;
        }
    }
    Vector3 CalculatePointOnParabola(Vector3 pointA, Vector3 pointB, float shootForce, float t)
    {
        // Calcular la dirección y la distancia entre los puntos A y B
        Vector3 direction = (pointB - pointA).normalized;
        float distance = Vector3.Distance(pointA, pointB);

        // Calcular la altura máxima de la parábola (asumiendo que la parábola es simétrica)
        float maxHeight = (pointB.y - pointA.y) + (0.5f * (shootForce * shootForce) / Mathf.Abs(Physics.gravity.y));

        // Calcular el tiempo total de vuelo
        float totalTime = Mathf.Sqrt((2 * maxHeight) / Mathf.Abs(Physics.gravity.y));

        // Calcular el tiempo en función del porcentaje t
        float time = totalTime * t;

        // Calcular la altura en función del tiempo
        float height = pointA.y + (shootForce * time) + (0.5f * Physics.gravity.y * time * time);

        // Calcular la distancia horizontal en función del tiempo
        float horizontalDistance = distance * t;

        // Calcular la posición en la trayectoria parabólica
        Vector3 pointOnParabola = pointA + (direction * horizontalDistance);
        pointOnParabola.y = height;

        return pointOnParabola;
    }
}
