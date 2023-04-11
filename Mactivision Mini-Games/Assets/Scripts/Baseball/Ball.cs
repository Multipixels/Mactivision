using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    Vector3 initPosition;
    float velocity;
    float acceleration;

    float startTime;

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

    public void Update() {
        if(ballState == BallState.Motion) {
            float timeDifference = Time.time - startTime;

            if(initPosition.x > 0) {
                transform.position = initPosition - new Vector3(velocity * timeDifference + 0.5f * acceleration * timeDifference * timeDifference, 0, 0);
            } else {
                transform.position = initPosition + new Vector3(velocity * timeDifference + 0.5f * acceleration * timeDifference * timeDifference, 0, 0);
            }
            
        }
    }
}
