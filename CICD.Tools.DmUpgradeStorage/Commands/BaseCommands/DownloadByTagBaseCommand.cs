namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands.BaseCommands
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib;
    using Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib.Models;

    internal abstract class DownloadByTagBaseCommand : BaseCommand
    {
        protected DownloadByTagBaseCommand(string name, string? description = null) : base(name, description)
        {
            AddOption(new Option<string?>(
                aliases: ["--version", "-v"],
                description: "Filter on DataMiner version."));

            AddOption(new Option<uint?>(
                aliases: ["--build-number", "-bn"],
                description: "Filter on build number."));

            AddOption(new Option<uint?>(
                aliases: ["--cu", "-cu"],
                description: "Filter on CU number."));

            AddOption(new Option<uint?>(
                aliases: ["--gerrit-id", "-gi"],
                description: "Filter on Gerrit ID."));

            AddOption(new Option<uint?>(
                aliases: ["--patch-set", "-ps"],
                description: "Filter on patch set number."));

            AddOption(new Option<PackageType?>(
                aliases: ["--package-type", "-pt"],
                description: "Filter on package type."));

            AddOption(new Option<UpgradeType?>(
                aliases: ["--upgrade-type", "-ut"],
                description: "Filter on the upgrade type."));
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Automatic binding with System.CommandLine.NamingConventionBinder")]
    internal abstract class DownloadByTagBaseCommandHandler : BaseCommandHandler
    {
        public uint? BuildNumber { get; set; }

        public uint? Cu { get; set; }

        public uint? GerritId { get; set; }

        public PackageType? PackageType { get; set; }

        public uint? PatchSet { get; set; }

        public UpgradeType? UpgradeType { get; set; }

        public string? Version { get; set; }

        public abstract override int Invoke(InvocationContext context);

        public abstract override Task<int> InvokeAsync(InvocationContext context);

        protected PackageTagFilter GetFilter()
        {
            PackageTagFilter builder = new PackageTagFilter();
            if (Version != null)
            {
                builder.WithVersion(Version);
            }

            if (BuildNumber != null)
            {
                builder.WithBuildNumber(BuildNumber.Value);
            }

            if (Cu != null)
            {
                builder.WithCu(Cu.Value);
            }

            if (GerritId != null)
            {
                builder.WithGerritId(GerritId.Value);
            }

            if (PatchSet != null)
            {
                builder.WithPatchSet(PatchSet.Value);
            }

            if (PackageType != null)
            {
                builder.WithType(PackageType.Value);
            }

            if (UpgradeType != null)
            {
                builder.WithUpgradeType(UpgradeType.Value);
            }

            return builder;
        }
    }
}