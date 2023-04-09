using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseballLevelManager : LevelManager
{
    public TMP_Text swingKeyText;      // text that contain instructions for swingKey bind  

    KeyCode swingKey;                  // keyboard key used to swing

    TimeAccuracyMetric taMetric;       // records time accuracy data during the game
    MetricJSONWriter metricWriter;     // outputs recording metric (bpMetric) as a json file

    // Start is called before the first frame update
    void Start() {
        Setup(); // run initial setup, inherited from parent class

        InitConfigurable(); // initialize configurable values

        // set the swingKey for the intro instructions
        int tempIdx = swingKeyText.text.IndexOf("SKEY");
        swingKeyText.text = swingKeyText.text.Substring(0, tempIdx) + KeyCodeDict.toString[swingKey] + swingKeyText.text.Substring(tempIdx + 4);

        countDoneText = "Swing!";

        taMetric = new TimeAccuracyMetric();
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

        // update battery config with actual/final values being used
        baseballConfig.SwingKey = swingKey.ToString();
        baseballConfig.Seed = seed;
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

        if (lvlState == 2 && e.keyCode != KeyCode.None) EndGame();

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
}
