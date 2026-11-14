using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MyCorner.Commands;
using MyCorner.Resources;

namespace MyCorner.Locations {
    public class TARDISConsoleRoom : Location {
        protected readonly IStringLocalizer Room;

        public TARDISConsoleRoom(IJSRuntime js, IStringLocalizerFactory stringLocalizerFactory)
            : base(js, stringLocalizerFactory) {
            this.Room = stringLocalizerFactory.Create(typeof(MyCorner.Resources.TardisConsoleRoom));
            Name = Room["TardisConsoleRoom.Name"].Value;
            Description = Room["TardisConsoleRoom.Description"].Value;
            this.InitialiseCommands();
        }

        protected override void RegisterLocationCommands() {
            RegisterCommands(["goto", "go"], args => {
                if (args.Length == 0) {
                    return Task.FromResult(CommandResult.Failed(Common["General.GoWherePrompt"].Value));
                }

                if (args[0].ToLower() == "workshop" || args[0].ToLower() == "west") {
                    return Task.FromResult(CommandResult.Succeeded(
                        Room["TardisConsoleRoom.GotoWorkshop"].Value,
                        new Workshop(js, StringLocalizerFactory)
                    ));
                }
                return Task.FromResult(CommandResult.Failed(Common["General.CantGoThere"].Value));
            });
        }

        protected override string GetLocationSpecificHelp() {
            return Room["TardisConsoleRoom.Help"].Value;
        }
    }
}