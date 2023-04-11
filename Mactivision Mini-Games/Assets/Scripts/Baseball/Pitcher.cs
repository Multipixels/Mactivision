using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitcher : MonoBehaviour
{

    System.Random randomSeed;  // seed of the current game
    int minTime;               // minimum desired time of ball throw
    int maxTime;               // maximum desired time of ball throw
    bool isLinear;             // linear movement (constant velocity) or quadratic movement (acceleration)

    public GameObject ballObj;           // the ball object thrown
    Ball ball;                    // the ball script on the ball

    Vector2[] desiredThrows;

    public DateTime choiceStartTime { private set; get; }   // the time the current food is dispensed and the player can make a choice

    // Initializes the pitcher with the seed.
    // Randomly chooses ball velocities.
    public void Init(string seed, int minTime, int maxTime, bool isLinear) {
        ballObj.SetActive(false);
        ball = ballObj.GetComponent<Ball>();

        //sound = gameObject.GetComponent<AudioSource>();

        randomSeed = new System.Random(seed.GetHashCode());

        this.minTime = minTime;
        this.maxTime = maxTime;
        this.isLinear = isLinear;

        desiredThrows = new Vector2[10];

        float distanceFromCenter = transform.position.x;

        for(int i = 0; i < 10; i++) {
            int airTime = randomSeed.Next(minTime, maxTime);

            float desiredVelocity = 2 * distanceFromCenter / airTime;

            desiredThrows[i] = new Vector2(desiredVelocity, 0);
        }
    }

    public void Throw() {
        int randIdx;
        randIdx = randomSeed.Next(desiredThrows.Length);
        Vector2 currentThrow = desiredThrows[randIdx];
        choiceStartTime = DateTime.Now;


        ballObj.SetActive(true);
        ball.Init(transform.position, currentThrow.x, currentThrow.y);
        ball.Throw();
    }
}
