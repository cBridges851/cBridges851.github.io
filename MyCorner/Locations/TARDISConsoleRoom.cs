using Microsoft.JSInterop;
using MyCorner.Commands;

namespace MyCorner.Locations {
    public class TARDISConsoleRoom : Location {
        public TARDISConsoleRoom(IJSRuntime js) : base(js) {
            Name = "TARDIS Console Room";
            Description = """
            You are in the TARDIS Console Room. The cluttered console is in front of you, with Christa playing around with a personal development project.<br><br> 

            "As you might gather, this place is under construction right now. It's being built in public though, so you can follow along on my <a href="https://www.linkedin.com/in/christa-bridges/">LinkedIn</a>!"<br><br>

            Available exits:<br>
            - workshop: Through a door marked "Projects"
            """;
        }

        protected override void RegisterLocationCommands() {
            RegisterCommand("rickroll", async _ => {
                await js.InvokeVoidAsync("openInNewTab", "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
                return CommandResult.Succeeded("Of course! I'd never let you down...");
            });

            RegisterCommand("goto", args => {
                if (args.Length == 0) {
                    return Task.FromResult(CommandResult.Failed("Go where? Try 'help' for available destinations."));
                }
                
                if (args[0].ToLower() == "workshop") {
                    return Task.FromResult(CommandResult.Succeeded(
                        "You walk through the door marked 'Projects'...",
                        new Workshop(js)
                    ));
                }
                return Task.FromResult(CommandResult.Failed("You can't go there from here. Try 'help' to see where you can go."));
            });
        }

        protected override string GetLocationSpecificHelp() {
            return """
                Location-specific commands:
                - yo: A special greeting
                - goto workshop: Visit the project workshop
                """;
        }
    }
}