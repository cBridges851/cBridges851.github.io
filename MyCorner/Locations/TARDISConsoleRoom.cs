using Microsoft.JSInterop;
using MyCorner.Commands;

namespace MyCorner.Locations {
    public class TARDISConsoleRoom : Location {
        public TARDISConsoleRoom(IJSRuntime js) : base(js) {
            Name = "TARDIS Console Room";
            Description = """
            You're inside a TARDIS (yes, from Doctor Who) which is bigger on the inside. Or smaller on the outside. Whatever floats your boat. 
            The walls are covered with glowing blue, green, and red hexagons that are all connected like a honeycomb.<br><br>

            At the centre is a hexagonal console cluttered with buttons, levers, and joysticks. 
            Whether they do anything is... unclear. But there is a CD player that seems to be working because it's playing Westlife.<br><br>

            A young brown-haired woman wearing a red neckwarmer/snood/thingy, 
            black long-sleeved shirt, and black cargo trousers peeks from behind the console.<br><br>

            "Oh hey! I'm Christa Bridges. I'm a software developer who primarily works in backend with .NET, but I sometimes dabble in other stuff. Like, I've recently dabbled in frontend with AngularJS and React, and Cloud."<br><br>

            She's interuptted by some very noisy drilling and sawing.<br><br>

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