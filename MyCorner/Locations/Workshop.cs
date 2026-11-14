using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MyCorner.Commands;
using MyCorner.Resources;

namespace MyCorner.Locations {
    public class Workshop : Location {
        private readonly Dictionary<string, string> projectResourceKeys = new() {
            ["personal website"] = "Workshop.Project.PersonalWebsite"
        };

        protected readonly IStringLocalizer Room;

        public Workshop(IJSRuntime js, IStringLocalizerFactory stringLocalizerFactory)
            : base(js, stringLocalizerFactory) {
            this.Room = stringLocalizerFactory.Create(typeof(MyCorner.Resources.Workshop));
            Name = Room["Workshop.Name"].Value;
            Description = Room["Workshop.Description", ListKeyProjects()].Value;
            this.InitialiseCommands();
        }

        private string ListKeyProjects() {
            var projectList = string.Join("<br>\n", projectResourceKeys.Keys.Select(p => $"- {p}"));
            return $"""
                    {Room["Workshop.KeyProjectsHeader"].Value}<br>
                    {projectList}
                    <br><br>
                    {Room["Workshop.ViewProjectHint"].Value}
                    {Room["Workshop.GitHubLink"].Value}
                    """;
        }

        protected override void RegisterLocationCommands() {
            RegisterCommand("list", _ => {
                return Task.FromResult(CommandResult.Succeeded(this.ListKeyProjects()));
            });

            RegisterCommand("view", args => {
                if (args.Length == 0) {
                    return Task.FromResult(CommandResult.Failed(Room["Workshop.ViewWhat"].Value));
                }

                var project = string.Join(" ", args).ToLower();
                if (projectResourceKeys.TryGetValue(project, out var resourceKey)) {
                    return Task.FromResult(CommandResult.Succeeded(Room[resourceKey].Value));
                }
                return Task.FromResult(CommandResult.Failed(Room["Workshop.ProjectNotFound"].Value));
            });

            RegisterCommands(["goto", "go"], args => {
                if (args.Length == 0) {
                    return Task.FromResult(CommandResult.Failed(Common["General.GoWherePrompt"].Value));
                }

                if (args[0].ToLower() == "console" || args[0].ToLower() == "east") {
                    return Task.FromResult(CommandResult.Succeeded(
                        Room["Workshop.GotoConsole"].Value,
                        new TARDISConsoleRoom(js, StringLocalizerFactory)
                    ));
                }
                return Task.FromResult(CommandResult.Failed(Common["General.CantGoThere"].Value));
            });
        }

        protected override string GetLocationSpecificHelp() {
            return Room["Workshop.Help"].Value;
        }
    }
}