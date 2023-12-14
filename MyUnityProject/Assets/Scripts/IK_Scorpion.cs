using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;

public class IK_Scorpion : MonoBehaviour
{
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

    // Update is called once per frame
    void Update()
    {
        if(animPlaying)
            animTime += Time.deltaTime;

        NotifyTailTarget();
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NotifyStartWalk();
            animTime = 0;
            animPlaying = true;
        }

        if (animTime < animDuration)
        {
            //Body.position = Vector3.Lerp(StartPos.position, EndPos.position, animTime / animDuration);
        }
        else if (animTime >= animDuration && animPlaying)
        {

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
