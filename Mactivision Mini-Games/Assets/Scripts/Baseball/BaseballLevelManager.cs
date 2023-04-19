using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class BaseballLevelManager : LevelManager
{
    public Pitcher pitcher;             // the ball pitcher
    public Batter batter;               // the batter player
    public new Camera camera;           // the camera
    public Ball ball;                   // the ball
    public TMP_Text swingKeyText;       // text that contain instructions for swingKey bind  

    public AudioClip hit;               // pong

    int maxBallsThrown;
    int ballsThrown;

    KeyCode swingKey;                   // keyboard key used to swing

    float throwDistance;                // distance that the ball has to travel in game units
    float ballSize;                     // size of the ball (1f = 50% of player size, 2f = 100%...)
    float averageThrowTime;             // center of throw time (seconds)
    float throwTimeVariance;            // variance in the "averageThrowTime"
    float averageInitialVelocity;       // center of initial velocity (1f = no acceleration, 0.5f = start at 50% velocity and accelerate
                                        //                             1.5f = start at 150% velocity and deccelerate)
    float initialVelocityVariance;      // variance in initial velocity
    bool resultFeedback;                // toggle ball angle variance based on accuracy

    TimeAccuracyMetric taMetric;        // records time accuracy data during the game
    MetricJSONWriter metricWriter;      // outputs recording metric (bpMetric) as a json file

    // Represents the state of the game cycle
    enum GameState {
        BallReady,
        WaitingForPlayer,
        Result,
        WaitForResult
    }
    GameState gameState;

    // Start is called before the first frame update
    void Start() {
        Setup(); // run initial setup, inherited from parent class
        InitConfigurable(); // initialize configurable values

        ballsThrown = 0;
        gameState = GameState.BallReady;

        // set the swingKey for the intro instructions
        int tempIdx = swingKeyText.text.IndexOf("SKEY");
        swingKeyText.text = swingKeyText.text.Substring(0, tempIdx) + KeyCodeDict.toString[swingKey] + swingKeyText.text.Substring(tempIdx + 4);

        countDoneText = "Swing!";

        taMetric = new TimeAccuracyMetric();

        pitcher.transform.position = new Vector2(throwDistance / 2, pitcher.transform.position.y);
        batter.transform.position = new Vector2(-throwDistance / 2, batter.transform.position.y);
        camera.orthographicSize = 1 + (throwDistance - 2) / 2 * 0.55f;
        ball.transform.localScale = new Vector2(1 + (0.5f * (ballSize - 1)), 1 + (0.5f * (ballSize - 1)));
        pitcher.Init(seed, maxBallsThrown, ballSize, averageThrowTime, throwTimeVariance, averageInitialVelocity, initialVelocityVariance);
        
    }

    // Initialize values using config file, or default values if config values not specified
    void InitConfigurable() {
        BaseballConfig baseballConfig = new BaseballConfig();

        // if running the game from the battery, override `baseballConfig` with the config class from Battery
        BaseballConfig tempConfig = (BaseballConfig)Battery.Instance.GetCurrentConfig();
        if (tempConfig != null) {
            baseballConfig = tempConfig;
        } else {
            Debug.Log("Battery not found, using default values");
        }

        // use battery's config values, or default values if running game by itself
        try {
            swingKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), baseballConfig.SwingKey);
            if (!KeyCodeDict.toString.ContainsKey(swingKey)) throw new Exception();
        } catch (Exception) {
            swingKey = Default(KeyCode.B, "SwingKey");
        }

        seed = !String.IsNullOrEmpty(baseballConfig.Seed) ? baseballConfig.Seed : DateTime.Now.ToString(); // if no seed provided, use current DateTime
        maxGameTime = baseballConfig.MaxGameTime > 0 ? baseballConfig.MaxGameTime : Default(120f, "MaxGameTime");
        maxBallsThrown = baseballConfig.MaxBallsThrown > 0 ? baseballConfig.MaxBallsThrown: Default(10, "MaxBallsThrown");
        throwDistance = baseballConfig.ThrowDistance >= 2 && baseballConfig.ThrowDistance <= 12f ? baseballConfig.ThrowDistance : Default(8f, "ThrowDistance");
        ballSize = baseballConfig.BallSize > 0 ? baseballConfig.BallSize : Default(2f, "BallSize");
        averageThrowTime = baseballConfig.AverageThrowTime > 0 ? baseballConfig.AverageThrowTime : Default(2f, "AverageThrowTime");
        throwTimeVariance = baseballConfig.ThrowTimeVariance >= 0 ? baseballConfig.ThrowTimeVariance : Default(0f, "ThrowTimeVariance");
        averageInitialVelocity = baseballConfig.AverageInitialVelocity > 0 ? baseballConfig.AverageInitialVelocity : Default(1f, "AverageInitialVelocity");
        initialVelocityVariance = baseballConfig.InitialVelocityVariance >= 0 ? baseballConfig.InitialVelocityVariance : Default(0f, "InitialVelocityVariance");
        resultFeedback = tempConfig != null ? baseballConfig.ResultFeedback : Default(true, "ResultFeedback");

        // update battery config with actual/final values being used
        baseballConfig.Seed = seed;
        baseballConfig.MaxGameTime = maxGameTime;
        baseballConfig.MaxBallsThrown = maxBallsThrown;
        baseballConfig.ThrowDistance = throwDistance;
        baseballConfig.BallSize = ballSize;
        baseballConfig.AverageThrowTime = averageThrowTime;
        baseballConfig.ThrowTimeVariance = throwTimeVariance;
        baseballConfig.AverageInitialVelocity= averageInitialVelocity;
        baseballConfig.InitialVelocityVariance = initialVelocityVariance;
        baseballConfig.ResultFeedback = resultFeedback;
        baseballConfig.SwingKey = swingKey.ToString();
    }

    // Handles GUI events (keyboard, mouse, etc events)
    void OnGUI() {
        Event e = Event.current;
        // navigate through the instructions before the game starts
        if (lvlState == 0 && e.type == EventType.KeyUp) {
            if (e.keyCode == KeyCode.B && instructionCount > 0) {
                ShowInstruction(--instructionCount);
            } else if (e.keyCode == KeyCode.N && instructionCount < instructions.Length) {
                ShowInstruction(++instructionCount);
            }
        }

        // game is over, go to next game/finish battery
        if (lvlState == 4 && e.type == EventType.KeyUp) {
            Battery.Instance.LoadNextScene();
        }
    }

    // Update is called once per frame
    void Update() {

        if (lvlState == 2) {
            // begin game, begin recording 
            if (!taMetric.isRecording) StartGame();

            // game automatically ends after maxGameTime seconds
            if (ballsThrown >= maxBallsThrown) {
                EndGame();
                return;
            }

            // The game cycle
            switch (gameState) {
                case GameState.BallReady:
                    pitcher.Throw();
                    gameState = GameState.WaitingForPlayer;
                    break;
                case GameState.WaitingForPlayer:
                    WaitForPlayer();
                    break;
                case GameState.Result:
                    StartCoroutine(WaitForResult(2f + pitcher.throwAirTime - (Time.time - pitcher.throwStartTime)));
                    gameState = GameState.WaitForResult;
                    break;
                case GameState.WaitForResult:
                    break;
            }
        }
    }

    // Begin the actual game, start recording metrics
    public void StartGame() {
        taMetric.startRecording();
        metricWriter = new MetricJSONWriter("Baseball", DateTime.Now); // initialize metric data writer
        gameStartTime = Time.time;
    }

    // End game, finish recording metrics
    public void EndGame() {
        taMetric.finishRecording();
        var str = metricWriter.GetLogMetrics(
                    DateTime.Now,
                    new List<AbstractMetric>() { taMetric }
                );
        StartCoroutine(Post("baseball_" + DateTime.Now.ToFileTime() + ".json", str));
        EndLevel(0f);
    }

    // This function is called each frame the game is waiting for input from the player.
    // When the player makes a choice, it plays appropriate animations and  
    // records the metric event, and starts the choice wait coroutine.
    void WaitForPlayer() {
        if (Time.time - pitcher.throwStartTime > pitcher.throwAirTime + 0.5f) {
            Debug.Log("Failed");
            pitcher.Result(-999);

            gameState = GameState.Result;
        }

        if (Input.GetKeyDown(swingKey) || Input.GetKeyDown(swingKey)) {

            // animate player

            // record the metric
            float acc = (pitcher.throwAirTime) - (Time.time - pitcher.throwStartTime);
            taMetric.recordEvent(new TimeAccuracyEvent(
                pitcher.throwStartDateTime,
                acc
            ));

            Debug.Log(acc);
            
            if (resultFeedback) pitcher.Result(acc);
            else if (Math.Abs(acc) <= 0.05) pitcher.Result(0);

            if (Math.Abs(acc) <= 0.05) {
                sound.PlayOneShot(hit);
            }

            // animate choice and play plate sound
            gameState = GameState.Result;
        }
    }

    IEnumerator WaitForResult(float wait) {
        yield return new WaitForSeconds(wait);
        ballsThrown++;
        gameState = GameState.BallReady;
    }
}
