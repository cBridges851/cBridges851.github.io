using Microsoft.JSInterop;
using MyCorner.Commands;

namespace MyCorner.Locations {
    public class Workshop : Location {
        private readonly Dictionary<string, string> projects = new() {
            ["personal website"] = """
            The site you're looking at now! A text-based game style personal website to showcase my skills and interests.<br>
            Tech: Blazor WebAssembly, C#/.NET, HTML, CSS, JavaScript<br>
            GitHub Repo: <a href="https://github.com/cBridges851/cBridges851.github.io/" target="_blank" rel="noopener noreferrer">https://github.com/cBridges851/cBridges851.github.io/</a><br>
            """
        };

        public Workshop(IJSRuntime js) : base(js) {
            Name = "Project Workshop";
            Description = $"""
                You enter a cosy workshop filled with various projects in different stages of development.
                Screens displaying code and project demos line the walls.<br><br>

                {this.ListKeyProjects()}

                <br><br>

                Available exits:<br>
                - console (east): Back to the TARDIS Console Room
                """;

            this.InitialiseCommands();
        }

        private string ListKeyProjects() {
            return $"""
                    Key projects:<br>
                    {string.Join("<br>\n", projects.Keys.Select(p => $"- {p}"))}
                    <br><br>
                    Use 'view [project-name]' to learn more about each project.
                    GitHub with all projects: <a href="https://github.com/cBridges851/" target="_blank" rel="noopener noreferrer">https://github.com/cBridges851/</a>
                    """;
        }

        protected override void RegisterLocationCommands() {
            RegisterCommand("list", _ => {
                var projectList = string.Join("\n", projects.Keys.Select(p => $"- {p}"));
                return Task.FromResult(CommandResult.Succeeded(this.ListKeyProjects()));
            });

            RegisterCommand("view", args => {
                if (args.Length == 0) {
                    return Task.FromResult(CommandResult.Failed("View what? Try 'list' to see available projects."));
                }

                var project = string.Join(" ", args).ToLower();
                if (projects.TryGetValue(project, out var description)) {
                    return Task.FromResult(CommandResult.Succeeded(description));
                }
                return Task.FromResult(CommandResult.Failed("Project not found. Use 'list' to see available projects."));
            });

            RegisterCommands(["goto", "go"], args => {
                if (args.Length == 0) {
                    return Task.FromResult(CommandResult.Failed("Go where? Try 'help' for available destinations."));
                }
                
                if (args[0].ToLower() == "console" || args[0].ToLower() == "east") {
                    return Task.FromResult(CommandResult.Succeeded(
                        "You walk back to the TARDIS Console Room...",
                        new TARDISConsoleRoom(js)
                    ));
                }
                return Task.FromResult(CommandResult.Failed("You can't go there from here. Try 'help' to see where you can go.")); 
            });
        }

        protected override string GetLocationSpecificHelp() {
            return """
                Location-specific commands:<br>
                - list: Shows all projects<br>
                - view [project-name]: Get details about a specific project<br>
                - goto console: Return to the TARDIS Console Room<br>
                """;
        }
    }
}