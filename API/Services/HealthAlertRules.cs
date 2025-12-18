using API.DTOs;
using Model;

namespace API.Services
{
    public static class HealthAlertRules
    {
        public static HealthAlertDto? Evaluate(BPLog log)
        {
            // Simple rules (you can refine)
            if (log.Systolic >= 180 || log.Diastolic >= 120)
                return new("BP", "Danger", "Hypertensive crisis. Consult doctor immediately.", log.LoggedAt);

            if (log.Systolic >= 140 || log.Diastolic >= 90)
                return new("BP", "Warning", "High blood pressure detected.", log.LoggedAt);

            if (log.Systolic < 90 || log.Diastolic < 60)
                return new("BP", "Warning", "Low blood pressure detected.", log.LoggedAt);

            return null;
        }

        public static HealthAlertDto? Evaluate(GlucoseLog log)
        {
            // mg/dL basic examples
            if (log.ValueMgDl >= 250)
                return new("Glucose", "Danger", "Very high glucose level detected.", log.LoggedAt);

            if (log.Type == "Fasting" && log.ValueMgDl >= 126)
                return new("Glucose", "Warning", "High fasting glucose.", log.LoggedAt);

            if (log.ValueMgDl < 70)
                return new("Glucose", "Danger", "Low glucose (hypoglycemia).", log.LoggedAt);

            return null;
        }

        public static HealthAlertDto? Evaluate(WeightLog log)
        {
            // For weight alerts, usually compare trend (optional).
            // Keep it simple for now: no alert.
            return null;
        }
    }
}
