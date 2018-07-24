// ------------------------------------------------------------
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ServiceFabric.WindowsPatchInstaller
{
    using System.Collections.Generic;
    using System.Fabric.Description;

    class PatchVerificationSettings
    {
        private const string SectionName = "PatchVerificationSettings";

        private PatchVerificationSettings()
        {
            this.EnabledPatches = new List<string>();
        }

        public IList<string> EnabledPatches { get; private set; }


        public static PatchVerificationSettings FromConfigSettings(ConfigurationSettings settings)
        {
            var patchVerificationListSettings = new PatchVerificationSettings();

            if (!settings.Sections.Contains(SectionName))
            {
                return patchVerificationListSettings;
            }

            var configSection = settings.Sections[SectionName];
            foreach (var prop in configSection.Parameters)
            {
                ParseConfigurationProperty(prop, patchVerificationListSettings);
            }

            return patchVerificationListSettings;
        }


        private static void ParseConfigurationProperty(
          ConfigurationProperty prop,
          PatchVerificationSettings patchVerificationSettings)
        {
            bool enabled;

            if (bool.TryParse(prop.Value, out enabled) && enabled)
            {
                patchVerificationSettings.EnabledPatches.Add(prop.Name);
            }
        }
    }
}
