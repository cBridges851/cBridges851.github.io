using Microsoft.JSInterop;
using MyCorner.Commands;

namespace MyCorner.Locations {
    public class Workshop : Location {
        private readonly Dictionary<string, string> projects = new() {
            ["personal website"] = "A text-based game style personal website built with Blazor WebAssembly.",
            ["project mercury"] = "A desktop application that helps developers manage their tasks.",
            ["devhelper bot"] = "A Discord bot that provides programming help and resources."
        };

        public Workshop(IJSRuntime js) : base(js) {
            Name = "Project Workshop";
            Description = """
                You enter a cozy workshop filled with various projects in different stages of development.
                Screens displaying code and project demos line the walls. There's a comfortable workspace
                in the center with multiple monitors showing different programming languages and frameworks.<br><br>

                Various project prototypes and demos are neatly organized on virtual shelves around the room.
                Each project has a small interactive display showing its key features.<br><br>

                Available exits:
                - console: Back to the TARDIS Console Room
                """;
        }

        protected override void RegisterLocationCommands() {
            RegisterCommand("list", _ => {
                var projectList = string.Join("\n", projects.Keys.Select(p => $"- {p}"));
                return Task.FromResult(CommandResult.Succeeded($"""
                    Available projects:
                    {projectList}
                    
                    Use 'view [project-name]' to learn more about each project.
                    """));
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

            RegisterCommand("goto", args => {
                if (args.Length == 0) {
                    return Task.FromResult(CommandResult.Failed("Go where? Try 'help' for available destinations."));
                }
                
                if (args[0].ToLower() == "console") {
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
                Location-specific commands:
                - list: Shows all projects
                - view [project-name]: Get details about a specific project
                - goto console: Return to the TARDIS Console Room
                """;
        }
    }
}