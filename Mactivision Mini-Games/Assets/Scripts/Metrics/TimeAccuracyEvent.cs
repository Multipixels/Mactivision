// LinearVariableEvent class: designed to be consumed by LinearVeriableMetric class.
public class TimeAccuracyEvent : AbstractMetricEvent {

    // Time in milliseconds away from desired time
    public int accuracy { get; }

    public TimeAccuracyEvent(System.DateTime eventTime, int accuracy) : base(eventTime) {
        this.accuracy = accuracy;
    }
}
