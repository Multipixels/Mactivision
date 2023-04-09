using Newtonsoft.Json.Linq;

// TimeAccuracyMetric class records TimeAccuracyEvents which occur during a game.
public class TimeAccuracyMetric : AbstractMetric<TimeAccuracyEvent> {

    public TimeAccuracyMetric() { }

    public override JObject getJSON() {
        JObject json = new JObject();

        json["metricName"] = JToken.FromObject("linearVariable");
        json["eventList"] = JToken.FromObject(this.eventList);
        return json;
    }
}