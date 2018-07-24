// ------------------------------------------------------------
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ServiceFabric.WindowsPatchInstaller
{
    using System.Fabric.Description;

    internal class Settings
    {
        public ScanSettings Scan { get; private set; }

        public HealthReportSettings HealthReport { get; private set; }

        public PatchVerificationSettings PatchVerification { get; private set; }

        public PatchInstallationSettings PatchInstallation { get; private set; }

        public static Settings FromConfigSettings(ConfigurationSettings configSettings)
        {
            return new Settings()
            {
                Scan = ScanSettings.FromConfigSettings(configSettings),
                HealthReport = HealthReportSettings.FromConfigSettings(configSettings),
                PatchVerification = PatchVerificationSettings.FromConfigSettings(configSettings),
                PatchInstallation = PatchInstallationSettings.FromConfigSettings(configSettings)
            };
        }
    }
}
