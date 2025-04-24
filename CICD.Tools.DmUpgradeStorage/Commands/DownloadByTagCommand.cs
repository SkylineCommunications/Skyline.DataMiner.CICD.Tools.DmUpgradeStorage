// ReSharper disable ClassNeverInstantiated.Global
namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Skyline.DataMiner.CICD.FileSystem.DirectoryInfoWrapper;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.BaseCommands;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Services;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.SystemCommandLine;

    internal class DownloadByTagCommand : DownloadByTagBaseCommand
    {
        public DownloadByTagCommand() :
            base(name: "by-tag", description: "Download dmupgrade packages filtered on tags.")
        {
            AddOption(new Option<IDirectoryInfoIO>(
                aliases: ["--output-directory", "-od"],
                description: "The directory where the package(s) will be stored.",
                parseArgument: OptionHelper.ParseDirectoryInfo!)
            {
                IsRequired = true
            }.LegalFilePathsOnly());
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal class DownloadByTagCommandHandler(ILogger<DownloadByTagCommandHandler> logger, IDmUpgradeStorageService storageService) : DownloadByTagBaseCommandHandler
    {
        public required IDirectoryInfoIO OutputDirectory { get; set; }

        public override int Invoke(InvocationContext context)
        {
            return (int)ExitCodes.NotImplemented;
        }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            DebugLog.Start(logger);
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                storageService.Authenticate(ConnectionString, AccountName, AccountKey);
                storageService.SetContainer(ContainerName);

                // Create directory first to make sure that it can be created
                OutputDirectory.Create();

                // Create a filter to get the latest package
                PackageTagFilter builder = GetFilter();

                var packages = storageService.DownloadPackagesByTagsAsync(builder, context.GetCancellationToken());

                await foreach (var package in packages)
                {
                    (string? name, Stream? content) = await package;

                    await using Stream stream = content;
                    string outputFilePath = Path.Combine(OutputDirectory.FullName, name);
                    await using FileStream fileStream = new FileStream(outputFilePath, FileMode.Create);
                    await stream.CopyToAsync(fileStream, context.GetCancellationToken());

                    logger.LogInformation("Downloaded package {packageName} to {outputDirectory}.", name, OutputDirectory.FullName);
                }

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