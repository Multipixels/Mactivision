// LinearVariableEvent class: designed to be consumed by LinearVeriableMetric class.
public class TimeAccuracyEvent : AbstractMetricEvent {

    // Time away from desired time
    public float accuracy { get; }

    public TimeAccuracyEvent(System.DateTime eventTime, float accuracy) : base(eventTime) {
        this.accuracy = accuracy;
    }
}
