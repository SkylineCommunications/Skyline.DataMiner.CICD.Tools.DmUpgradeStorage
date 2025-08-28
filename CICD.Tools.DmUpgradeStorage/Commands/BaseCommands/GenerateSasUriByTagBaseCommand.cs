namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.BaseCommands
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using Skyline.DataMiner.CICD.FileSystem.FileInfoWrapper;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Services;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.SystemCommandLine;

    internal abstract class GenerateSasUriByTagBaseCommand : DownloadByTagBaseCommand
    {
        protected GenerateSasUriByTagBaseCommand(string name, string? description = null) : base(name, description)
        {
            AddOption(new Option<IFileInfoIO>(
                aliases: ["--output-file", "-of"],
                description: "The output file which will contain the SAS URI.",
                parseArgument: OptionHelper.ParseFileInfo!)
            {
                IsRequired = true
            }.LegalFilePathsOnly());

            AddOption(new Option<double?>(
                aliases: ["--expiration-time", "-et"],
                description: "The expiration time which is how long the URI will be valid. Value is in minutes. Default is 60 minutes."));
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal abstract class GenerateSasUriByTagBaseCommandHandler : DownloadByTagBaseCommandHandler
    {
        public required IFileInfoIO OutputFile { get; set; }

        public double? ExpirationTime { get; set; }

        protected TimeSpan GetExpirationTime()
        {
            return ExpirationTime == null ? DmUpgradeStorageService.SasUriDefaultDuration : TimeSpan.FromMinutes(ExpirationTime.Value);
        }

        public abstract override Task<int> InvokeAsync(InvocationContext context);
    }
}