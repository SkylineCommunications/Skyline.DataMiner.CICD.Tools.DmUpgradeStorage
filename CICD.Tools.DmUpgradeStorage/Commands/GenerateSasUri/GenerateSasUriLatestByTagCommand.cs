// ReSharper disable ClassNeverInstantiated.Global
namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.Download
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
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Models;

    internal class GenerateSasUriLatestByTagCommand()
        : GenerateSasUriByTagBaseCommand(name: "latest-by-tag", description: "Download the latest dmupgrade package filtered on tags.");

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal class GenerateSasUriLatestByTagCommandHandler(ILogger<GenerateSasUriLatestByTagCommandHandler> logger, IDmUpgradeStorageService storageService) : GenerateSasUriByTagBaseCommandHandler
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

                var uri = await storageService.GenerateSasUriLatestByTagsAsync(builder, GetExpirationTime(), context.GetCancellationToken());

                if (uri == null)
                {
                    logger.LogError("No SAS URI could be created for the provided filters");
                    return (int)ExitCodes.Fail;
                }

                GenerateSasUriResult result = new GenerateSasUriResult();
                result.SasUris.Add(uri);
                await using FileStream fileStream = OutputFile.Create();
                await JsonSerializer.SerializeAsync(fileStream, result);

                logger.LogInformation("SAS URI generated in {OutputFile}.", OutputFile.FullName);

                return (int)ExitCodes.Ok;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to generate a SAS URI for the provided filters.");
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