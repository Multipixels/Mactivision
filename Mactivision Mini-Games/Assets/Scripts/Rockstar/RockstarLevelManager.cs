﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

// This class manages the majority of the game functionality
public class RockstarLevelManager : LevelManager
{
    public TMP_Text introText2;     // a second text block b/c lots of info
    bool showIntro2;

    public Rockstar rockstar;       // responsible for moving the rockstar
    public Spotlight spotlight;     // used to move the spotlight
    public Meter meter;             // drops and raises the meter
    
    string seed;                    // optional manually entered seed
    System.Random randomSeed;       // seed of the current game

    float maxGameTime;              // maximum length of the game
    float gameStartTime;

    float rockstarChangeFreq;       // how often the rockstar's destination changes
    float rockstarVelocity;         // speed the rockstar moves

    float spotlightVelocity;        // speed the spotlight can move
    
    float meterChangeFreq;          // how often the meter velocity changes
    float meterMinVel;              // minimum velocity the meter drops
    float meterMaxVel;              // maximum velocity the meter drops
    float meterUpVel;               // velocity the meter is raised by player

    KeyCode leftKey;                // key that moves the spotlight left
    KeyCode rightKey;               // key that moves the spotlight right
    KeyCode upKey;                  // key that raises the meter

    PositionMetric pMetric;         // records position data during the game
    LinearVariableMetric lvMetric;  // records linear data during the game
    MetricJSONWriter metricWriter;  // outputs recording metric (pMetric and lvMetric) as a json file

    // Start is called before the first frame update
    void Start()
    {
        Setup(); // run initial setup, inherited from parent class

        InitConfigurable(); // initialize configurable values
        
        randomSeed = new System.Random(seed.GetHashCode());

        // set the leftKey for the intro instructions
        int tempIdx = introText.text.IndexOf("LKEY");
        introText.text = introText.text.Substring(0, tempIdx) + leftKey.ToString() + introText.text.Substring(tempIdx+4);

        // set the rightKey for the intro instructions
        tempIdx = introText.text.IndexOf("RKEY");
        introText.text = introText.text.Substring(0, tempIdx) + rightKey.ToString() + introText.text.Substring(tempIdx+4);

        // set the upKey for the intro instructions
        tempIdx = introText2.text.IndexOf("UKEY");
        introText2.text = introText2.text.Substring(0, tempIdx) + upKey.ToString() + introText2.text.Substring(tempIdx+4);

        introText2.enabled = false;
        showIntro2 = false;
        countDoneText = "Rock!";

        pMetric = new PositionMetric(new string[]{"rockstar", "spotlight"}); // initialize position metric recorder
        lvMetric = new LinearVariableMetric(0f, 100f, 75f, new string[]{"gameDrop", "playerRaise"}); // initialize linear variable metric recorder
        metricWriter = new MetricJSONWriter("Rockstar", DateTime.Now, seed); // initialize metric data writer

        rockstar.Init(seed, rockstarChangeFreq, rockstarVelocity);
        spotlight.Init(spotlightVelocity);
        meter.Init(seed, meterChangeFreq, meterMinVel, meterMaxVel, meterUpVel, 0f, 100f, 75f);
        rockstar.enabled = false;
        spotlight.enabled = false;
        meter.enabled = false;
    }

    // Initialize values using config file, or default values if config values not specified
    void InitConfigurable()
    {
        RockstarConfig rockstarConfig = new RockstarConfig();

        // if running the game from the battery, override `rockstarConfig` with the config class from Battery
        RockstarConfig tempConfig = (RockstarConfig)Battery.Instance.GetCurrentConfig();
        if (tempConfig!=null) {
            rockstarConfig = tempConfig;
            outputPath = Battery.Instance.GetOutputPath();
        } else {
            Debug.Log("Battery not found, using default values");
        }

        // use battery's config values, or default values if running game by itself
        seed = !String.IsNullOrEmpty(rockstarConfig.Seed) ? rockstarConfig.Seed : DateTime.Now.ToString(); // if no seed provided, use current DateTime
        maxGameTime = rockstarConfig.MaxGameTime > 0 ? rockstarConfig.MaxGameTime : 120f;
        rockstarChangeFreq = rockstarConfig.RockstarChangeFreq > 0 ? rockstarConfig.RockstarChangeFreq : 1f;
        rockstarVelocity = rockstarConfig.RockstarVelocity > 0 ? rockstarConfig.RockstarVelocity : 2.5f;
        spotlightVelocity = rockstarConfig.SpotlightVelocity > 0 ? rockstarConfig.SpotlightVelocity : 3f;
        meterChangeFreq = rockstarConfig.MeterChangeFreq > 0 ? rockstarConfig.MeterChangeFreq : 2f;
        meterMinVel = rockstarConfig.MeterMinVel > 0 ? rockstarConfig.MeterMinVel : 5f;
        meterMaxVel = rockstarConfig.MeterMaxVel > 0 ? rockstarConfig.MeterMaxVel : 30 f;
        meterUpVel = rockstarConfig.MeterUpVel > 0 ? rockstarConfig.MeterUpVel : 60f;
        // use default key if string cannot be parsed to keycode (or is null)
        try {
            leftKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), rockstarConfig.LeftKey);
        } catch (Exception) {
            Debug.Log("Invalid KeyCode, using default value for leftkey");
            leftKey = KeyCode.A;
        }
        try {
            rightKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), rockstarConfig.RightKey);
        } catch (Exception) {
            Debug.Log("Invalid KeyCode, using default value for rightkey");
            rightKey = KeyCode.D;
        }
        try {
            upKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), rockstarConfig.UpKey);
        } catch (Exception) {
            Debug.Log("Invalid KeyCode, using default value for upkey");
            upKey = KeyCode.L;
        }

        // udpate battery config with actual/final values being used
        rockstarConfig.Seed = seed;
        rockstarConfig.MaxGameTime = maxGameTime;
        rockstarConfig.RockstarChangeFreq = rockstarChangeFreq;
        rockstarConfig.RockstarVelocity = rockstarVelocity;
        rockstarConfig.SpotlightVelocity = spotlightVelocity;
        rockstarConfig.MeterChangeFreq = meterChangeFreq;
        rockstarConfig.MeterMinVel = meterMinVel;
        rockstarConfig.MeterMaxVel = meterMaxVel;
        rockstarConfig.MeterUpVel = meterUpVel;
        rockstarConfig.LeftKey = leftKey.ToString();
        rockstarConfig.RightKey = rightKey.ToString();
        rockstarConfig.UpKey = upKey.ToString();
    }

    // Handles GUI events (keyboard, mouse, etc events)
    void OnGUI()
    {
        Event e = Event.current;
        // this is the "Press any key to continue" before the start of the game
        if (lvlState==0 && e.type == EventType.KeyUp) {
            if (!showIntro2) {
                showIntro2 = true;
                introText.enabled = false;
                introText2.enabled = true;
                ResizeTextBG(GetRect(introText2));
            } else {
                introText2.enabled = false;
                StartLevel();
            }
        }

        if (lvlState==4 && e.type == EventType.KeyUp) {
            Battery.Instance.LoadNextScene();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lvlState==2) {
            // begin game, begin recording 
            if (!pMetric.isRecording) StartGame(); 
                
            // game automatically ends after maxGameTime seconds
            if (Time.time-gameStartTime >= maxGameTime) { 
                EndGame();
                return;
            }

            InputHandler();
            RecordMetricEvents();
        }
    }

    // Begin the actual game, start recording metrics
    void StartGame()
    {
        pMetric.startRecording();
        // lvMetric.startRecording();
        gameStartTime = Time.time;
        rockstar.enabled = true;
        spotlight.enabled = true;
        meter.enabled = true;
    }

    // End game, finish recording metrics
    void EndGame()
    {
        pMetric.finishRecording();
        metricWriter.logMetrics(
            outputPath+"rockstarP_"+DateTime.Now.ToFileTime()+".json", 
            DateTime.Now, 
            new List<AbstractMetric>(){pMetric}
        );
        lvMetric.finishRecording();
        metricWriter.logMetrics(
            outputPath+"rockstarLV_"+DateTime.Now.ToFileTime()+".json", 
            DateTime.Now, 
            new List<AbstractMetric>(){lvMetric}
        );
        EndLevel(0f);
    }

    // Handles player keyboard input
    void InputHandler()
    {
        if (Input.GetKey(upKey) || Input.GetKeyDown(upKey)) MeterUp();
        if (Input.GetKey(leftKey) || Input.GetKeyDown(leftKey)) SpotlightLeft();
        if (Input.GetKey(rightKey) || Input.GetKeyDown(rightKey)) SpotlightRight();
    }

    // Raise the meter and add a linear variable event
    void MeterUp()
    {
        meter.Raise();
        lvMetric.recordEvent(new LinearVariableEvent(DateTime.Now, meter.level, meterUpVel*Time.deltaTime, 1));
    }

    // Move the spotlight left
    void SpotlightLeft()
    {
        spotlight.Move(false);
    }

    // Move the spotlight right
    void SpotlightRight()
    {
        spotlight.Move(true);
    }

    // Called each frame to add metric event
    void RecordMetricEvents()
    {
        pMetric.recordEvent(new PositionEvent(DateTime.Now, new Vector2[]{rockstar.GetPosition(), spotlight.GetPosition()}));
        lvMetric.recordEvent(new LinearVariable(DateTime.Now, meter.level, meter.velocity*Time.deltaTime, 0));
    }
}