using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

namespace OctopusController
{

    public class MyScorpionController : MonoBehaviour
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        public Transform body;

        MyTentacleController _tail;

        float animDuration;
        float currentTime = 0;
        bool isPlaying = false;
        public bool StartTail = false;
        float distanceBetweenFutureBases = 0.5f;
        //LEGS
        Transform[] legTargets = new Transform[6];
        Transform[] legFutureBases = new Transform[6];
        MyTentacleController[] _legs = new MyTentacleController[6];


        private Vector3[] jointsController;
        private float[] distancesBetweenJoints;
        float threeshold = 0.05f;
        float tailRate = 20.0f;
        public float shootForce = 1;

        public bool shoot = false;
        bool[] updateBases;
        float[] updateBasesTime;
        Vector3[] updateBasesPos;

        float deceleration = 1;
        float shootTime;
        #region public
        public void InitLegs(Transform[] LegRoots, Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            updateBases = new bool[LegRoots.Length];
            updateBasesTime = new float[LegRoots.Length];
            updateBasesPos = new Vector3[LegRoots.Length];
            for (int i = 0; i < LegRoots.Length; i++)
            {
                updateBases[i] = false;
                   _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                legFutureBases[i] = LegFutureBases[i];
                legTargets[i] = LegTargets[i];
                //TODO: initialize anything needed for the FABRIK implementation
            }
           distancesBetweenJoints = new float[_legs[0].Bones.Length];
            jointsController = new Vector3[_legs[0].Bones.Length + 1];
        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation

            tailEndEffector = _tail._endEffectorSphere;
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {
            StartTail = false;
               isPlaying = true;
            animDuration = 5;
            currentTime = 0;
        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            UpdateBody();
            UpdateBases();
            UpdateBodyRotation();

            updateTail();

            MovementAnimation();
        }

        void UpdateBases()
        {
            for (int i = 0; i < legFutureBases.Length; i++)
            {
                RaycastHit hit;

                if(Physics.Raycast(legFutureBases[i].position + new Vector3(0,2,0), -Vector3.up, out hit, 8, Physics.AllLayers))
                {
                    legFutureBases[i].position =  new Vector3(legFutureBases[i].position.x, hit.point.y, legFutureBases[i].position.z);
                }


            }
        }
        void UpdateBody()
        {
            float positionMedia = 0;
            for (int i = 0; i < legFutureBases.Length; i++)
            {

                positionMedia += legFutureBases[i].position.y;

            }
            positionMedia /= legFutureBases.Length;

            body.position = new Vector3(body.position.x, positionMedia+0.75f, body.position.z);
        }
        void UpdateBodyRotation()
        {
            //Altura media de las patas de la izquierda
            float positionMedia1 = 0;

            //Altura media de las patas de la derecha
            float positionMedia2 = 0;

            //Altura media de las patas de delante y centrales
            float positionMedia3 = 0;

            //Altura media de las patas de detras y centrales
            float positionMedia4 = 0;


            //EjeX
            for (int i = 0; i < legFutureBases.Length; i+=2)
            {
                positionMedia1 += legFutureBases[i].position.y;
            }
            for (int i = 1; i < legFutureBases.Length; i += 2)
            {
                positionMedia2 += legFutureBases[i].position.y;
            }

            //EjeZ
            for (int i = 0; i < 4; i++)
            {
                positionMedia3 += legFutureBases[i].position.y;
            }

            for (int i = 2; i < legFutureBases.Length; i++)
            {
                positionMedia4 += legFutureBases[i].position.y;
            }


            float a = positionMedia1 - positionMedia2;
            float b = positionMedia3 - positionMedia4;



            body.transform.localEulerAngles = new Vector3(b*15, 0, -a * 15);
        }

        #endregion

        #region private
        //TODO: Implement the leg base animations and logic
        private void MovementAnimation()
        {
            updateLegPos();

            if (isPlaying == true)
            {
                //Play animation only for 5 seconds
                currentTime += Time.deltaTime;
                if (currentTime < animDuration)
                {
                    updateLegPos();

                }
                else
                {
                    StartTail = true;
                    isPlaying = false;
                }
            }
        }
        private void updateLegPos()
        {
            for (int j = 0; j < 6; j++)
            {
                if (Vector3.Distance(_legs[j].Bones[0].position, legFutureBases[j].position) > distanceBetweenFutureBases && !updateBases[j])
                {
                    updateBases[j] = true;
                    updateBasesTime[j] = Time.time;
                    updateBasesPos[j] = legFutureBases[j].position;
                }

                if(updateBases[j])
                {
                    if((Time.time - updateBasesTime[j]) <= 0.1f)
                    {
                        _legs[j].Bones[0].position = Vector3.Lerp(_legs[j].Bones[0].position, legFutureBases[j].position, (Time.time - updateBasesTime[j]));

                        float elapse = (Time.time - updateBasesTime[j]);
                        if (elapse >= 0.05f)
                        {
                            elapse = 0.05f - (elapse - 0.05f);
                        }
                        elapse *= 0.5f;

                        _legs[j].Bones[0].position = new Vector3(_legs[j].Bones[0].position.x, _legs[j].Bones[0].position.y + elapse, _legs[j].Bones[0].position.z);

                    }
                    else
                    {
                        _legs[j].Bones[0].position = Vector3.Lerp(_legs[j].Bones[0].position, legFutureBases[j].position, 1f);
                        updateBases[j] = false;

                    }
                }
                updateLegs(j);
            }

        }
        public void Shoot()
        {
            shoot = true;
            shootTime = Time.time;
        }
        public void restart()
        {
            for (int i = 0; i < _tail.Bones.Length - 2; i++)
            {
                _tail.Bones[i].transform.localEulerAngles = new Vector3(0,0,0);

            }
        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            if((Time.time- shootTime)>2&& shoot)
            {
                StartTail = false;
                   shoot = false;
                restart();
            }


            //Only if tail end position is far away from target and if scorpion arrive to the ball
            if (Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position) > threeshold && StartTail)
            {

                for (int i = 0; i < _tail.Bones.Length - 2; i++)
                {
                    float rotation = 0;

                    if(!shoot)
                    {
                        if (i == 0)
                        {
                            //Rotate first tail joint in x and z axis
                            rotation = CalculateRotation(_tail.Bones[i], new Vector3(0, 0, 1));
                            _tail.Bones[i].transform.Rotate((new Vector3(0, 0, 1) * -rotation) * tailRate * shootForce);

                        }
                    }
                    else
                    {
                        deceleration = 1 - (Math.Clamp(1 - (Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position)/2), 0, 0.75f));
                        deceleration = (float)Math.Pow(deceleration, 2);

                        //Rotate the other joints in only x axis
                        rotation = CalculateRotation(_tail.Bones[i], new Vector3(1, 0, 0));
                        _tail.Bones[i].transform.Rotate((new Vector3(1, 0, 0) * -rotation) * tailRate * shootForce* deceleration);
                    }




                    

                }
            }
        }
        private float CalculateRotation(Transform actualJoint, Vector3 axis)
        {
            float distance = Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position);
            actualJoint.transform.Rotate(axis * 0.01f);
            float distance2 = Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position);
            actualJoint.transform.Rotate(axis * -0.01f);
            return (distance2 - distance) / 0.01f;
        }

        //TODO: implement fabrik method to move legs 
        private void updateLegs(int idPata)
        {

            SetParameters(idPata);

            float targetToRoot = Vector3.Distance(jointsController[0], legTargets[idPata].position);
            if (targetToRoot < distancesBetweenJoints.Sum())
            {

                CalculateRotations(idPata);

                //Set leg bones rotation 

                RotateLegs(idPata);
            }
            #endregion
        }

        void SetParameters(int id)
        {
            for (int i = 0; i <= _legs[id].Bones.Length - 1; i++)
            {
                jointsController[i] = _legs[id].Bones[i].position;
            }
            jointsController[_legs[id].Bones.Length] = _legs[id]._endEffectorSphere.position;

            for (int i = 0; i <= _legs[id].Bones.Length - 2; i++)
            {
                distancesBetweenJoints[i] = Vector3.Distance(_legs[id].Bones[i].position, _legs[id].Bones[i + 1].position);
            }
            distancesBetweenJoints[_legs[id].Bones.Length - 1] = Vector3.Distance(_legs[id].Bones[_legs[id].Bones.Length - 1].position, _legs[id]._endEffectorSphere.position);
        }
        void CalculateRotations(int id)
        {
            while (Vector3.Distance(jointsController[jointsController.Length - 1], legTargets[id].position) != 0 || Vector3.Distance(jointsController[0], _legs[id].Bones[0].position) != 0)
            {
                //From target to base
                jointsController[jointsController.Length - 1] = legTargets[id].position;
                for (int i = jointsController.Length - 2; i >= 0; i--)
                {
                    Vector3 vectorDirector = (jointsController[i + 1] - jointsController[i]).normalized;
                    Vector3 movementVector = vectorDirector * distancesBetweenJoints[i];
                    jointsController[i] = jointsController[i + 1] - movementVector;
                }

                //From base to target
                jointsController[0] = _legs[id].Bones[0].position;
                for (int i = 1; i < jointsController.Length - 1; i++)
                {
                    Vector3 vectorDirector = (jointsController[i - 1] - jointsController[i]).normalized;
                    Vector3 movementVector = vectorDirector * distancesBetweenJoints[i - 1];
                    jointsController[i] = jointsController[i - 1] - movementVector;

                }
            }
        }

        void RotateLegs(int id)
        {
            for (int i = 0; i <= jointsController.Length - 2; i++)
            {
                Vector3 direction = (jointsController[i + 1] - jointsController[i]).normalized;
                Vector3 antDir;
                if (i + 1 >= _legs[id].Bones.Length)
                    antDir = (_legs[id]._endEffectorSphere.position - _legs[id].Bones[i].position).normalized;
                else
                    antDir = (_legs[id].Bones[i + 1].position - _legs[id].Bones[i].position).normalized;

                Quaternion rot = Quaternion.FromToRotation(antDir, direction);
                _legs[id].Bones[i].rotation = rot * _legs[id].Bones[i].rotation;
            }
        }
    }

}
