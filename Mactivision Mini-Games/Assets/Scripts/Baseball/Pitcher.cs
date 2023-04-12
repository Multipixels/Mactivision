using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitcher : MonoBehaviour
{

    System.Random randomSeed;    // seed of the current game
    float minTime;               // minimum desired time of ball throw
    float maxTime;               // maximum desired time of ball throw
    bool isLinear;               // linear movement (constant velocity) or quadratic movement (acceleration)

    public GameObject ballObj;   // the ball object thrown
    Ball ball;                   // the ball script on the ball

    float[] desiredAirTimes;
    Vector2[] desiredThrows;

    public DateTime throwStartDateTime { private set; get; }   // the time the ball is thrown
    public float throwAirTime { private set; get; }        // the desired amount of time ball should be in air
    public float throwStartTime { private set; get; }

    // Initializes the pitcher with the seed.
    // Randomly chooses ball velocities.
    public void Init(string seed, float minTime, float maxTime, bool isLinear) {
        ballObj.SetActive(false);
        ball = ballObj.GetComponent<Ball>();

        //sound = gameObject.GetComponent<AudioSource>();

        randomSeed = new System.Random(seed.GetHashCode());

        this.minTime = minTime;
        this.maxTime = maxTime;
        this.isLinear = isLinear;

        desiredThrows = new Vector2[10];
        desiredAirTimes = new float[10];

        float distanceFromCenter = transform.position.x;

        for(int i = 0; i < 10; i++) {
            float airTime = minTime + ((maxTime-minTime)*(float)randomSeed.NextDouble());

            float desiredVelocity = 2 * distanceFromCenter / airTime;

            desiredAirTimes[i] = airTime;
            desiredThrows[i] = new Vector2(desiredVelocity, 0);
        }
    }

    public void Throw() {
        int randIdx;
        randIdx = randomSeed.Next(desiredThrows.Length);
        Vector2 currentThrow = desiredThrows[randIdx];
        throwAirTime = desiredAirTimes[randIdx];
        throwStartTime = Time.time;
        throwStartDateTime = DateTime.Now;
        
        ballObj.SetActive(true);
        ball.Init(transform.position, currentThrow.x, currentThrow.y);
        ball.Throw();
    }

    public void Result(float acc) {
        ball.Result(acc);
    }
}
