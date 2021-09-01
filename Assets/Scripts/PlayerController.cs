using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public int direction; //1 = Up, 2 = Right, 3 = Down, 4 = Left
    private float pauseControls;


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
            if (Input.GetAxis("z+") > 0) //Up
                direction = 1;
            if (Input.GetAxis("x+") > 0) //Right
                direction = 2;
            if (Input.GetAxis("z-") > 0) //Down
                direction = 3;
            if (Input.GetAxis("x-") > 0) //Left
                direction = 4;
        }
    }

    private void FixedUpdate()
    {

    }
}
