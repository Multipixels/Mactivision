using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    Vector3 initPosition;
    Vector3 resultInitPosition;
    float velocity;
    float acceleration;

    float startTime;
    float resultStartTime;
    float resultAngle;

    enum BallState {
        Ready,
        Motion,
        Result
    }
    BallState ballState;

    public void Init(Vector3 initPosition, float velocity, float acceleration) {
        this.initPosition = initPosition;
        this.velocity = velocity;
        this.acceleration = acceleration;
        ballState = BallState.Ready;
    }

    public void Throw() {
        transform.position = initPosition;
        startTime = Time.time;
        ballState = BallState.Motion;
    }

    public void Result(float acc) {

        float percentage = acc / 0.05f;

        if (Mathf.Abs(percentage) < 1) {

            resultAngle = (Mathf.PI / 4) + (Mathf.PI / 8 * percentage);
            resultInitPosition = transform.position;
            resultStartTime = Time.time;
            ballState = BallState.Result;
        }
    }

    public void Update() {
        switch(ballState) {
            case BallState.Motion:
                float timeDifference = Time.time - startTime;

                if(initPosition.x > 0) {
                    transform.position = initPosition - new Vector3(velocity * timeDifference + 0.5f * acceleration * timeDifference * timeDifference, 0, 0);
                } else {
                    transform.position = initPosition + new Vector3(velocity * timeDifference + 0.5f * acceleration * timeDifference * timeDifference, 0, 0);
                }

                break;

            case BallState.Result:
                float resultTimeDifference = Time.time - resultStartTime;
                transform.position = resultInitPosition + new Vector3(10 * resultTimeDifference * Mathf.Cos(resultAngle), 10 * resultTimeDifference * Mathf.Sin(resultAngle), 0);

                break;
        } 
    }
}
