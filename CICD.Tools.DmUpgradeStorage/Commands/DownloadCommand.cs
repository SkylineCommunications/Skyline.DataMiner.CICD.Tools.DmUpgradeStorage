namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Commands
{
    using System.CommandLine;

    internal class DownloadCommand : Command
    {
        public DownloadCommand() :
            base(name: "download", description: "Download dmupgrade packages via subcommands.")
        {
            AddCommand(new DownloadByNameCommand());
            AddCommand(new DownloadLatestByTagCommand());
            AddCommand(new DownloadByTagCommand());
        }
    }
}