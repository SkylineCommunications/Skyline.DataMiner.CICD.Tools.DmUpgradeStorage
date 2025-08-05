// ReSharper disable ClassNeverInstantiated.Global
namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.GenerateSasUri
{
    using System;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.BaseCommands;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Services;

    internal class GenerateSasUriByTagCommand()
        : GenerateSasUriByTagBaseCommand(name: "by-tag", description: "Generate SAS URI(s) for downloading dmupgrade packages filtered on tags.");

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal class GenerateSasUriByTagCommandHandler(ILogger<GenerateSasUriByTagCommandHandler> logger, IDmUpgradeStorageService storageService) : GenerateSasUriByTagBaseCommandHandler
    {
        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            DebugLog.Start(logger);
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                storageService.Authenticate(ConnectionString, AccountName, AccountKey);
                storageService.SetContainer(ContainerName);

                // Create directory first to make sure that it can be created
                OutputFile.Directory.Create();

                // Create a filter to get the latest package
                PackageTagFilter builder = GetFilter();

                var result = await storageService.GenerateSasUriByTagsAsync(builder, GetExpirationTime(), context.GetCancellationToken());

                int nbrOfPackages = result?.SasUris.Count ?? 0;
                if (nbrOfPackages == 0)
                {
                    logger.LogError("No packages found for the provided tags.");
                    return (int)ExitCodes.Fail;
                }

                await using FileStream fileStream = OutputFile.Create();
                await JsonSerializer.SerializeAsync(fileStream, result);

                logger.LogInformation("SAS URI(s) generated in {OutputFile}.", OutputFile.FullName);

                return (int)ExitCodes.Ok;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to generate SAS URI(s) for the provided tags.");
                return (int)ExitCodes.UnexpectedException;
            }
            finally
            {
                sw.Stop();
                DebugLog.End(logger, sw);
            }
        }
    }
}