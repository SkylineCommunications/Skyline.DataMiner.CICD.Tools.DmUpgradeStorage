// ReSharper disable ClassNeverInstantiated.Global
namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.GenerateSasUri
{
    using System;
    using System.CommandLine;
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

    internal class GenerateSasUriByNameCommand : GenerateSasUriBaseCommand
    {
        public GenerateSasUriByNameCommand() :
            base(name: "by-name", description: "Generate a SAS URI for downloading a dmupgrade package by name.")
        {
            AddOption(new Option<string>(
                aliases: ["--name", "-n"],
                description: "Retrieve package via name.")
            {
                IsRequired = true
            });
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal class GenerateSasUriByNameCommandHandler(ILogger<GenerateSasUriByNameCommandHandler> logger, IDmUpgradeStorageService storageService) : GenerateSasUriBaseCommandHandler
    {
        public required string Name { get; set; }
        
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

                var result = await storageService.GenerateSasUriByNameAsync(Name, GetExpirationTime() , context.GetCancellationToken());
                if (result == null)
                {
                    logger.LogError("No SAS URI could be created for the provided name: {name}", Name);
                    return (int)ExitCodes.Fail;
                }

                await using FileStream fileStream = OutputFile.Create();
                await JsonSerializer.SerializeAsync(fileStream, result);

                logger.LogInformation("SAS URI generated in {OutputFile}.", OutputFile.FullName);

                return (int)ExitCodes.Ok;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to generate a SAS URI for the provided name '{Name}'.", Name);
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