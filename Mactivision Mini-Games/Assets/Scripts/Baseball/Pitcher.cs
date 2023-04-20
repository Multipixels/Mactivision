using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitcher : MonoBehaviour
{

    System.Random randomSeed;    // seed of the current game
    int maxBallsThrown;
    float ballSize;
    float averageThrowTime;                    // center of throw time (seconds)
    float throwTimeVariance;                   // variance in the "averageThrowTime"
    float averageInitialVelocity;              // center of initial velocity (1f = no acceleration, 0.5f = start at 50% velocity and accelerate
                                               //                             1.5f = start at 150% velocity and deccelerate)
    float initialVelocityVariance;             // variance in initial velocity

    public GameObject ballObj;   // the ball object thrown
    Ball ball;                   // the ball script on the ball

    AudioSource sound;

    float[] desiredAirTimes;
    Vector2[] desiredThrows;

    public DateTime throwStartDateTime { private set; get; }   // the time the ball is thrown
    public float throwAirTime { private set; get; }        // the desired amount of time ball should be in air
    public float throwStartTime { private set; get; }

    // Initializes the pitcher with the seed.
    // Randomly chooses ball velocities.
    public void Init(string seed, int maxBallsThrown, float ballSize, float averageThrowTime, float throwTimeVariance, float averageInitialVelocity, float initialVelocityVariance) {
        ballObj.SetActive(false);
        ball = ballObj.GetComponent<Ball>();

        sound = gameObject.GetComponent<AudioSource>();

        randomSeed = new System.Random(seed.GetHashCode());

        this.ballSize = ballSize;
        this.averageThrowTime = averageThrowTime;
        this.throwTimeVariance = throwTimeVariance;
        this.averageInitialVelocity = averageInitialVelocity;
        this.initialVelocityVariance = initialVelocityVariance;

        desiredThrows = new Vector2[maxBallsThrown];
        desiredAirTimes = new float[maxBallsThrown];

        float distanceFromCenter = transform.position.x;

        for(int i = 0; i < 10; i++) {

            /*
            d = vt + 0.5at^2
            
            2(d - vt) / t^2 = a
            */


            float airTime = averageThrowTime + throwTimeVariance * (2*(float)randomSeed.NextDouble() - 1);
            desiredAirTimes[i] = airTime;

            float desiredVelocity = 2 * distanceFromCenter / airTime;
            float actualVelocity = desiredVelocity * (averageInitialVelocity + (initialVelocityVariance * (2 * (float)randomSeed.NextDouble() - 1)));
            float acceleration = 2 * (distanceFromCenter * 2 - actualVelocity * airTime) / airTime / airTime;
            desiredThrows[i] = new Vector2(actualVelocity, acceleration);
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
        sound.PlayDelayed(0f);
    }

    public void Result(float acc) {
        ball.Result(acc);
    }
}
