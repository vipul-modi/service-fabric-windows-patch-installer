// ------------------------------------------------------------
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ServiceFabric.WindowsPatchInstaller
{
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Fabric;
    using System.Fabric.Health;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class PatchInstallerService : StatelessService
    {
        private const string HealthReportSource = "PatchIsntallerService";
        private const string InstallFileName = "install.cmd";
        private const string VerifyFileName = "verify.cmd";
        private const string ConfigPackageName = "Config";

        private readonly Random rand;
        private FabricClient client;
        private Settings settings;

        public PatchInstallerService(StatelessServiceContext context)
            : base(context)
        {
            this.rand = new Random();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (this.VerifyAndInstallPatches(cancellationToken))
                {
                    this.VerifyAndInstallPatches(cancellationToken);
                }

                var sleepInterval = TimeSpan.FromSeconds(
                    this.settings.Scan.ScanIntervalSeconds +
                    this.rand.Next(this.settings.Scan.ScanIntervalRandomizationSeconds));

                ServiceEventSource.Current.ServiceMessage(this.Context, $"Sleeping for {sleepInterval}");

                await Task.Delay(sleepInterval, cancellationToken);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void LoadSettings()
        {
            try
            {
                var configPackage = this.Context.CodePackageActivationContext.GetConfigurationPackageObject(ConfigPackageName);
                this.settings = Settings.FromConfigSettings(configPackage.Settings);
            }
            catch
            {
                ServiceEventSource.Current.ServiceMessage(
                    this.Context,
                    $"failed to load configuration package {ConfigPackageName}. Using default settings.");
                this.settings = new Settings();
            }
        }

        private bool VerifyAndInstallPatches(CancellationToken cancellationToken)
        {
            bool installed = false;

            this.LoadSettings();

            if (this.settings.PatchVerification.EnabledPatches.Count == 0)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "There are no patches to verify and install.");
                return false;
            }

            IList<PatchInformation> installedPatches;
            if (!this.TryGetInstalledPatches(out installedPatches))
            {
                // failed to load the installed patch information
                // TBD: report health warning
                return false;
            }

            foreach (var patchId in this.settings.PatchVerification.EnabledPatches)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool verified;
                string installedOn;
                if (!this.TryVerifyPatch(patchId, installedPatches, out verified, out installedOn))
                {
                    // TBD: change reporting
                    this.ReportMissingPatch(patchId);
                    continue;
                }

                if (!verified)
                {
                    this.ReportMissingPatch(patchId);

                    if (this.settings.PatchInstallation.EnabledPatches.Contains(patchId))
                    {
                        this.InstallPatch(patchId);
                        installed = true;
                    }
                }
                else
                {
                    this.ReportInstalledPatch(patchId, installedOn);
                }
            }

            return installed;
        }


        #region Health Reporting

        private void ReportMissingPatch(string patchId)
        {
            var healthInformation = new HealthInformation(
                HealthReportSource,
                patchId,
                this.settings.HealthReport.ReportMissingAs)
            {
                Description = $"Patch {patchId} is NOT installed.",
                RemoveWhenExpired = this.settings.HealthReport.RemoveWhenExpired,
                TimeToLive = TimeSpan.FromSeconds(this.settings.HealthReport.TimeToLiveSeconds),
                SequenceNumber = 0
            };

            var healthReport = new NodeHealthReport(this.Context.NodeContext.NodeName, healthInformation);
            this.SendHealthReport(healthReport);
        }

        private void ReportInstalledPatch(string patchId, string installedOn)
        {
            var healthInformation = new HealthInformation(
                HealthReportSource,
                patchId,
                HealthState.Ok)
            {
                Description = $"Patch {patchId} was installed on {installedOn}",
                RemoveWhenExpired = this.settings.HealthReport.RemoveWhenExpired,
                TimeToLive = TimeSpan.FromSeconds(this.settings.HealthReport.TimeToLiveSeconds),
                SequenceNumber = 0
            };

            var healthReport = new NodeHealthReport(this.Context.NodeContext.NodeName, healthInformation);
            this.SendHealthReport(healthReport);
        }

        private void SendHealthReport(NodeHealthReport healthReport)
        {
            if (this.client == null)
            {
                this.client = new FabricClient();
            }

            this.client.HealthManager.ReportHealth(healthReport);
            ServiceEventSource.Current.ServiceMessage(
                this.Context,
                $"Reported {healthReport.HealthInformation.HealthState} on {healthReport.NodeName} for patch {healthReport.HealthInformation.Property}");
        }

        #endregion

        #region Patch Installation

        private void InstallPatch(string patchId)
        {
            var dataPackage = this.GetPatchDataPackage(patchId);
            if (dataPackage == null)
            {
                return;
            }

            var installfilePath = Path.Combine(dataPackage.Path, InstallFileName);
            if (!File.Exists(installfilePath))
            {
                ServiceEventSource.Current.PatchInstallFileNotFound(InstallFileName, patchId);
            }

            this.ExecutePatchInstall(patchId, installfilePath, dataPackage.Path);
        }

        private void ExecutePatchInstall(
            string patchId,
            string installFilePath,
            string workingDirectory)
        {
            try
            {
                var p = Process.Start(
                new ProcessStartInfo(installFilePath)
                {
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = true
                });

                if (p != null)
                {
                    p.WaitForExit();
                    ServiceEventSource.Current.PatchProcessCompleted(installFilePath, patchId, p.ExitCode);
                }
                else
                {
                    ServiceEventSource.Current.FailedToStartPatchProcess(installFilePath, patchId);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.FailedToStartPatchProcess(installFilePath, patchId, e.ToString());
            }
        }

        #endregion

        #region Patch Verification 

        private bool TryVerifyPatch(
            string patchId,
            IList<PatchInformation> installedPatches,
            out bool verified,
            out string installedOn)
        {
            verified = false;
            installedOn = string.Empty;

            var dataPackage = this.GetPatchDataPackage(patchId);
            string verifyScriptPath = string.Empty;
            if (dataPackage != null)
            {
                verifyScriptPath = Path.Combine(
                    dataPackage.Path,
                    VerifyFileName);
            }

            if ((dataPackage == null) || (!File.Exists(verifyScriptPath)))
            {
                // check in the list of installed patches
                var installedPatch = installedPatches.FirstOrDefault(
                    (p) => string.Compare(p.Id, patchId, StringComparison.OrdinalIgnoreCase) == 0);

                if (installedPatch != null)
                {
                    verified = true;
                    installedOn = installedPatch.InstalledOn;
                }

                return true;
            }
            else
            {
                // execute the verify script to check if the patch is installed or not
                int exitCode;

                if (this.TryExecutePatchScript(
                    patchId,
                    verifyScriptPath,
                    this.Context.CodePackageActivationContext.WorkDirectory,
                    out exitCode))
                {
                    if (exitCode == 0)
                    {
                        verified = true;
                        installedOn = "Unknown";
                        return true;
                    }

                    if (exitCode == 1)
                    {
                        verified = false;
                        installedOn = string.Empty;
                        return true;
                    }
                }

                return false;
            }
        }

        #endregion

        #region Patch DataPackage and Script Execution

        private DataPackage GetPatchDataPackage(string patchId)
        {
            // locate the data package for the patch
            try
            {
                var dataPackage = this.Context.CodePackageActivationContext.GetDataPackageObject(patchId);
                return dataPackage;
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.PatchDataPackageNotFound(patchId, e.ToString());
            }

            return null;
        }

        private bool TryExecutePatchScript(
          string patchId,
          string scriptFilePath,
          string workingDirectory,
          out int exitCode)
        {
            exitCode = -1;

            try
            {
                var p = Process.Start(
                new ProcessStartInfo(scriptFilePath)
                {
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = true
                });

                if (p != null)
                {
                    p.WaitForExit();
                    exitCode = p.ExitCode;
                    ServiceEventSource.Current.PatchProcessCompleted(scriptFilePath, patchId, p.ExitCode);
                    return true;
                }
                else
                {
                    ServiceEventSource.Current.FailedToStartPatchProcess(scriptFilePath, patchId);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.FailedToStartPatchProcess(scriptFilePath, patchId, e.ToString());
            }

            return false;
        }

        #endregion

        #region Patch Information 
        private bool TryGetInstalledPatches(out IList<PatchInformation> installedPatches)
        {
            try
            {
                installedPatches = GetInstalledPatches();
                return true;
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Failed to load installed patches: {e}");
                installedPatches = null;
                return false;
            }
        }

        private static IList<PatchInformation> GetInstalledPatches()
        {
            const string query = "SELECT HotFixID,InstalledOn FROM Win32_QuickFixEngineering";
            var search = new ManagementObjectSearcher(query);
            var collection = search.Get();
            var patchInfoList = new List<PatchInformation>();

            foreach (var quickFix in collection)
            {
                var id = quickFix["HotFixID"]?.ToString() ?? string.Empty;
                var installedOn = quickFix["InstalledOn"]?.ToString() ?? string.Empty;

                patchInfoList.Add(
                    new PatchInformation()
                    {
                        Id = id,
                        InstalledOn = installedOn
                    });
            }
            return patchInfoList;
        }

        class PatchInformation
        {
            public string InstalledOn { get; set; }

            public string Id { get; set; }
        }
        #endregion
    }
}
