using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CubeController : MonoBehaviour
{
    //Editor Vars
    public int rotationSpeed;
    public float gravityScale;

    //Editor Refrence Vars
    public Transform[] vertices; //0 = BOTTOM BACK, 1 = BOTTOM RIGHT, 2 = BOTTOM FRONT, 3 = BOTTOM LEFT, 4 = TOP BACK, 5 = TOP RIGHT, 6 = TOP FRONT, 7 = TOP LEFT
    public Transform[] sides;

    //Private Refrence
    private Vector3[] rotationAxis = new Vector3[] { new Vector3(0, 0, 1), new Vector3(1, 0, 0) };

    //Private
    public bool[] touchingSides; //0=RIGHT, 1=LEFT, 2=FRONT, 3=BACK, 4=TOP, 5=BOTTOM
    private int direction; //1 = Up & Left, -1 = Down & Right
    private int physicsDirectionMult;
    public float rotationAmount;
    private bool climbing;
    private bool falling;
    public float currentFallSpeed;
    private float toRotate;

    //Public
    public float pauseControls; //Pause Controls for n frames
    public Vector3 rotationPoint;
    public int rotating; //1 = Horizontal, 2 = Vertical

    void Start()
    {

    }

    void Update()
    {
        if (pauseControls > 0)
            pauseControls -= Time.deltaTime;

        direction = 0;
        if (pauseControls <= 0)
        {
            //Up
            if (Input.GetAxis("Vertical") > 0 && rotating != 1)
            {
                direction = 1;
                InitVertex(1);
            }
            //Right
            else if (Input.GetAxis("Horizontal") > 0 && rotating != 2)
            {
                direction = -1;
                InitVertex(2);
            }
            //Down
            else if (Input.GetAxis("Vertical") < 0 && rotating != 1)
            {
                direction = -1;
                InitVertex(3);
            }
            //Left
            else if (Input.GetAxis("Horizontal") < 0 && rotating != 2)
            {
                direction = 1;
                InitVertex(4);
            }
            if (Input.GetAxis("Rewind") != 0)
            {
                falling = true;
                currentFallSpeed = 0.0005f;
                transform.position = (new Vector3(transform.position.x, 1, transform.position.z));
            }
        }
    }

    //Get vertex to rotate around
    private void InitVertex(int dir)
    {
        //Only run if starting to rotate from neutral position
        if (rotationPoint == Vector3.zero)
        {
            rotating = dir % 2 + 1; //Variable for tracking if the rotation is Vertical (2) or Horizontal (1)
            physicsDirectionMult = direction;

            //Choose a point to rotate around based on direction pressed and adjacent colliders
            if (!touchingSides[dir - 1]) //Rotate on the ground when no collider is in intended direction
            {
                rotationPoint = vertices[dir - 1].position;
            }
            else //Climb up a block if there is a collider in the intended direction
            {
                //Checks the block behind
                int index = dir - 1 - direction * 2;
                if (index < 0)
                    index += 5;
                if (!touchingSides[index])
                {
                    //Climb the block in front
                    rotationPoint = vertices[dir + 3].position;
                }
            }
        }
    }

    //Convert eulers to usable floats
    private float GetRotationAmount()
    {
        float rot = transform.eulerAngles.z;
        if (Mathf.Round(transform.eulerAngles.x) != 0)
            rot = transform.eulerAngles.x;

        if (transform.eulerAngles.y == 180)
            rot = Mathf.Abs(rot - 180);

        if (rot > 180)
            rot -= 360;

        return Mathf.Abs(rot);
    }

    private void FixedUpdate()
    {
        //Get the rotation of the cube normalized to start at zero
        rotationAmount = GetRotationAmount();

        toRotate = 0;

        //Update list for sides touching other blocks
        for (int i = 0; i < 6; i++)
            touchingSides[i] = Physics.CheckSphere(sides[i].position, 0);

        //Apply rotation to the cube around a vertex
        if (direction != 0 && pauseControls <= 0 && rotationPoint != Vector3.zero)
        {
            toRotate = rotationSpeed * direction;
        }

        //Apply mock physics rotation to the cube
        if (rotationAmount > 0 && rotating != 0)
        {
            if (rotationAmount <= 45)
                toRotate += -(1 / rotationAmount) * gravityScale * physicsDirectionMult;//transform.RotateAround(rotationPoint, rotationAxis[rotating - 1], (-1 / rotationAmount) * 10);
            if (rotationAmount > 45)
                toRotate += (1 / (90 - rotationAmount)) * gravityScale * physicsDirectionMult;//transform.RotateAround(rotationPoint, rotationAxis[rotating - 1], (1 / -(rotationAmount)) * 10);
        }

        //Apply calculated rotation
        if (rotating != 0)
            transform.RotateAround(rotationPoint, rotationAxis[rotating - 1], toRotate);

        rotationAmount = GetRotationAmount();

        //Rotate Cube to normal position if it has finished the rotation or returns to previous position
        if (rotationAmount >= 89 || (Physics.CheckSphere(sides[5].position, 0) && rotationAmount > 0) || (rotating != 0 && direction == 0 && rotationAmount == 0))
        {
            transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
            transform.rotation = new Quaternion(Mathf.Round(transform.rotation.x / 10) * 10, Mathf.Round(transform.rotation.y / 10) * 10, Mathf.Round(transform.rotation.x / 10) * 10, Mathf.Round(transform.rotation.x / 10) * 10);
            rotationPoint = Vector3.zero;
            rotating = 0;
            pauseControls = 0.04f;
            physicsDirectionMult = 0;

            //Check for ground after rotating, if not fall
            if (!Physics.CheckSphere(sides[5].position, 0))
            {
                pauseControls = 999;
                currentFallSpeed = 0.03f;
                falling = true;
            }
        }

        //Falling physics
        if (falling)
        {
            //Fall
            transform.Translate(new Vector3(0, -currentFallSpeed, 0));
            if (currentFallSpeed < 0.4f)
                currentFallSpeed += gravityScale / 3000;

            //Hit ground
            if (Physics.CheckSphere(sides[5].position, 0))
            {
                transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
                rotationPoint = Vector3.zero;
                rotating = 0;
                pauseControls = 0.04f;
                falling = false;
                physicsDirectionMult = 0;
            }
        }
    }
}
