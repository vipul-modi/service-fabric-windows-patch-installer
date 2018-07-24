// ------------------------------------------------------------
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ServiceFabric.WindowsPatchInstaller
{
    using System;
    using System.Fabric.Description;
    using System.Fabric.Health;

    public class HealthReportSettings
    {
        private const string SectionName = "HealthReportSettings";

        private HealthReportSettings()
        {
            this.RemoveWhenExpired = true;
            this.TimeToLiveSeconds = 300;
            this.ReportMissingAs = HealthState.Warning;
        }

        public bool RemoveWhenExpired { get; private set; }

        public int TimeToLiveSeconds { get; private set; }

        public HealthState ReportMissingAs { get; private set; }

        public static HealthReportSettings FromConfigSettings(ConfigurationSettings settings)
        {
            var healthReportSettings = new HealthReportSettings();

            if (!settings.Sections.Contains(SectionName))
            {
                return healthReportSettings;
            }

            var configSection = settings.Sections[SectionName];
            foreach (var prop in configSection.Parameters)
            {
                ParseConfigurationProperty(prop, healthReportSettings);
            }

            return healthReportSettings;
        }

        private static void ParseConfigurationProperty(
            ConfigurationProperty prop,
            HealthReportSettings healthReportSettings)
        {
            switch (prop.Name)
            {
                case "RemoveWhenExpired":
                    {
                        var removeWhenExpired = false;
                        if (bool.TryParse(prop.Value, out removeWhenExpired))
                        {
                            healthReportSettings.RemoveWhenExpired = removeWhenExpired;
                        }
                        break;
                    }

                case "TimeToLiveSeconds":
                    {
                        var timeToLiveSeconds = 0;
                        if (int.TryParse(prop.Value, out timeToLiveSeconds))
                        {
                            healthReportSettings.TimeToLiveSeconds = timeToLiveSeconds;
                        }
                        break;
                    }

                case "ReportMissingAs":
                    {
                        var reportMissingAs = HealthState.Unknown;
                        if (Enum.TryParse(prop.Value, out reportMissingAs))
                        {
                            healthReportSettings.ReportMissingAs = reportMissingAs;
                        }
                        break;
                    }
            }
        }
    }
}
