using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;
using UnityEngine.UI;

public class IK_Scorpion : MonoBehaviour
{
    public Slider force;
    MyScorpionController _myController= new MyScorpionController();
    public Animator anim;

    public IK_tentacles _myOctopus;

    [Header("Body")]
    float animTime;
    public float animDuration = 5;
    bool animPlaying = false;
    public Transform Body;
    public Transform StartPos;
    public Transform EndPos;

    [Header("Tail")]
    public Transform tailTarget;
    public Transform tail;

    [Header("Legs")]
    public Transform[] legs;
    public Transform[] legTargets;
    public Transform[] futureLegBases;

    public Vector3[] futureLegBasesStart;
    public GameObject[] bases;

    int direction = 1;
    // Start is called before the first frame update
    void Start()
    {
        _myController.InitLegs(legs,futureLegBases,legTargets);
        _myController.InitTail(tail);
        _myController.body = Body;



        for (int i = 0; i < bases.Length;i++)
        {
        futureLegBasesStart[i] = bases[i].transform.localPosition;

        }
    }
    public float getForce()
    {
        return _myController.shootForce;
    }
    // Update is called once per frame
    void Update()
    {
        force.value = ((_myController.shootForce - 1f) / 4f);
        if (animPlaying)
            animTime += Time.deltaTime;

        NotifyTailTarget();
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NotifyStartWalk();
            animTime = 0;
            animPlaying = true;
        }
        if (Input.GetKey(KeyCode.Space) && !_myController.shoot)
        {
            _myController.StartTail = true;
            if (_myController.shootForce < 1)
                direction = 1;
            if (_myController.shootForce > 5)
                direction = -1;

            _myController.shootForce += Time.deltaTime * direction;
        }
        if ((Input.GetKeyUp(KeyCode.Space)) && !_myController.shoot)
        {
            _myController.Shoot();
        }

   

        if (animTime < animDuration)
        {
            //Body.position = Vector3.Lerp(StartPos.position, EndPos.position, animTime / animDuration);
        }
        else if (animTime >= animDuration && animPlaying)
        {
            _myController.shoot = false;

            Body.position = EndPos.position;
            for (int i = 0; i < bases.Length; i++)
            {
                bases[i].transform.localPosition = futureLegBasesStart[i];
            }
            animPlaying = false;
        }

        _myController.UpdateIK();
    }
    public void Restart()
    {
        direction = 1;
        _myController.shootForce = 1;
        anim.SetTrigger("Restart");
        Body.parent.localPosition = Vector3.zero;


        Body.position = StartPos.position;
        for (int i = 0; i < bases.Length; i++)
        {
            bases[i].transform.localPosition = futureLegBasesStart[i];
        }
    }
    //Function to send the tail target transform to the dll
    public void NotifyTailTarget()
    {
        _myController.NotifyTailTarget(tailTarget);
    }

    //Trigger Function to start the walk animation
    public void NotifyStartWalk()
    {

        anim.Play("Walk");
        _myController.NotifyStartWalk();
    }
}
