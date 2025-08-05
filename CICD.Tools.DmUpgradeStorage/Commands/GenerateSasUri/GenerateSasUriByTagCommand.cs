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

    using Skyline.DataMiner.CICD.FileSystem.DirectoryInfoWrapper;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.BaseCommands;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Services;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Models;

    internal class GenerateSasUriByTagCommand : GenerateSasUriByTagBaseCommand
    {
        public GenerateSasUriByTagCommand() :
            base(name: "by-tag", description: "Download dmupgrade packages filtered on tags.")
        {
        }
    }

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

                var uris = storageService.GenerateSasUriByTagsAsync(builder, GetExpirationTime(), context.GetCancellationToken());

                int nbrOfPackages = 0;
                GenerateSasUriResult result = new GenerateSasUriResult();
                await foreach (var uri in uris)
                {
                    result.SasUris.Add(uri);
                    nbrOfPackages++;
                }

                if (nbrOfPackages == 0)
                {
                    logger.LogError("No packages found for the provided tags.");
                    return (int)ExitCodes.Fail;
                }

                await using FileStream fileStream = OutputFile.Create();
                await JsonSerializer.SerializeAsync(fileStream, result);

                logger.LogInformation("SAS URIs generated in {OutputFile}.", OutputFile.FullName);

                return (int)ExitCodes.Ok;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to download the packages.");
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