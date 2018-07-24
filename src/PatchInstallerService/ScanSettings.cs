// ------------------------------------------------------------
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ServiceFabric.WindowsPatchInstaller
{
    using System.Fabric.Description;

    class ScanSettings
    {
        private const string SectionName = "ScanSettings";

        private ScanSettings()
        {
            this.ScanIntervalRandomizationSeconds = 120;
            this.ScanIntervalSeconds = 120;
        }

        public int ScanIntervalRandomizationSeconds { get; set; }

        public int ScanIntervalSeconds { get; set; }

        public static ScanSettings FromConfigSettings(ConfigurationSettings settings)
        {
            var scanSettings = new ScanSettings();

            if (!settings.Sections.Contains(SectionName))
            {
                return scanSettings;
            }

            var configSection = settings.Sections[SectionName];
            foreach (var prop in configSection.Parameters)
            {
                ParseConfigurationProperty(prop, scanSettings);
            }

            return scanSettings;
        }

        private static void ParseConfigurationProperty(
            ConfigurationProperty prop,
            ScanSettings scanSettings)
        {
            switch (prop.Name)
            {
                case "ScanIntervalRandomizationSeconds":
                    {
                        int scanIntervalRandomizationSeconds;
                        if (int.TryParse(prop.Value, out scanIntervalRandomizationSeconds))
                        {
                            scanSettings.ScanIntervalRandomizationSeconds = scanIntervalRandomizationSeconds;
                        }
                        break;
                    }

                case "ScanIntervalSeconds":
                    {
                        int scanIntervalSeconds;
                        if (int.TryParse(prop.Value, out scanIntervalSeconds))
                        {
                            scanSettings.ScanIntervalSeconds = scanIntervalSeconds;
                        }
                        break;
                    }
            }
        }
    }
}
