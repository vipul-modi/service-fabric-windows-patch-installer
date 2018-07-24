// ------------------------------------------------------------
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ServiceFabric.WindowsPatchInstaller
{
    using System.Collections.Generic;
    using System.Fabric.Description;

    internal class PatchInstallationSettings
    {
        private const string SectionName = "PatchInstallationSettings";

        private PatchInstallationSettings()
        {
            this.EnabledPatches = new List<string>();
        }

        public IList<string> EnabledPatches { get; private set; }


        public static PatchInstallationSettings FromConfigSettings(ConfigurationSettings settings)
        {
            var patchInstallationSettings = new PatchInstallationSettings();

            if (!settings.Sections.Contains(SectionName))
            {
                return patchInstallationSettings;
            }

            var configSection = settings.Sections[SectionName];
            foreach (var prop in configSection.Parameters)
            {
                ParseConfigurationProperty(prop, patchInstallationSettings);
            }

            return patchInstallationSettings;
        }

        private static void ParseConfigurationProperty(
          ConfigurationProperty prop,
          PatchInstallationSettings patchInstallationSettings)
        {
            bool enabled;

            if (bool.TryParse(prop.Value, out enabled) && enabled)
            {
                patchInstallationSettings.EnabledPatches.Add(prop.Name);
            }
        }
    }
}
