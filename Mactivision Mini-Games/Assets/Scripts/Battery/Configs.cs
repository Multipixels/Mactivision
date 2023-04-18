using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

public class BatteryConfig
{
    public string StartTime {get; set;}
    public string EndTime {get; set;}

    public IList<GameConfig> Games {get; set;}
}

public interface GameConfig 
{
    string TestName {get; set;}
    string Scene {get; set;}
}

public class DiggerConfig : GameConfig
{
    public string TestName {get; set;}
    public string Scene {get; set;}
    public float MaxGameTime {get; set;}
    public int DigAmount {get; set;}
    public string DigKey {get; set;}
}

public class FeederConfig : GameConfig
{
    public string TestName {get; set;}
    public string Scene {get; set;}
    public string Seed {get; set;}
    public float MaxGameTime {get; set;}
    public int MaxFoodDispensed {get; set;}
    public int UniqueFoods {get; set;}
    public float AverageUpdateFrequency {get; set;}
    public float UpdateFreqVariance {get; set;}
}

public class RockstarConfig : GameConfig
{
    public string TestName {get; set;}
    public string Scene {get; set;}
    public string Seed {get; set;}
    public float MaxGameTime {get; set;}
    public float RockstarChangeFreq {get; set;}
    public float RockstarVelocity {get; set;}
    public float SpotlightVelocity {get; set;}
    public float MeterChangeFreq {get; set;}
    public float MeterMinVel {get; set;}
    public float MeterMaxVel {get; set;}
    public float MeterUpVel {get; set;}
    public float MeterGoodRange {get; set;}
    public string LeftKey {get; set;}
    public string RightKey {get; set;}
    public string UpKey {get; set;}
    public bool EnableAnimations {get; set;}
    public int MusicTrack {get; set;}
}

public class BaseballConfig : GameConfig 
{
    public string TestName { get; set; }
    public string Scene { get; set; }
    public string Seed { get; set; }
    public string SwingKey { get; set; }
    public float MaxGameTime { get; set; }
    public int MaxBallsThrown { get; set; }
    public float ThrowDistance { get; set; }
    public float BallSize { get; set; }
    public float AverageThrowTime { get; set; }
    public float ThrowTimeVariance { get; set; }
    public float AverageInitialVelocity { get; set; }
    public float InitialVelocityVariance { get; set; }
    public bool ResultFeedback { get; set; }
}
