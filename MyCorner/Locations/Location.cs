using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MyCorner.Commands;
using MyCorner.Resources;

namespace MyCorner.Locations {
    public abstract class Location {
        protected readonly IJSRuntime js;
        protected readonly IStringLocalizerFactory StringLocalizerFactory;
        protected readonly IStringLocalizer Common;
        private readonly Dictionary<string, Func<string[], Task<CommandResult>>> commands;

        protected Location(IJSRuntime js, IStringLocalizerFactory stringLocalizerFactory) {
            this.js = js;
            this.StringLocalizerFactory = stringLocalizerFactory;
            this.Common = stringLocalizerFactory.Create(typeof(MyCorner.Resources.Common));
            commands = new();
        }

        protected void InitialiseCommands() {
            RegisterBaseCommands();
            RegisterLocationCommands();
        }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        protected abstract void RegisterLocationCommands();

        private void RegisterBaseCommands() {
            RegisterCommand("help", _ => Task.FromResult(CommandResult.Succeeded(GetHelpText())));
            RegisterCommand("rickroll", async _ => {
                await js.InvokeVoidAsync("openInNewTab", "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
                return CommandResult.Succeeded(Common["General.RickrollResponse"].Value);
            });
        }

        protected void RegisterCommands(string[] verbs, Func<string[], Task<CommandResult>> handler) {
            foreach (var verb in verbs) {
                this.RegisterCommand(verb, handler);
            }
        }

        protected void RegisterCommand(string verb, Func<string[], Task<CommandResult>> handler) {
            commands[verb.ToLower()] = handler;
        }

        public virtual async Task<CommandResult> ExecuteAction(string verb, string[] args) {
            verb = verb.ToLower();
            if (commands.TryGetValue(verb, out var handler)) {
                try {
                    return await handler(args);
                } catch (Exception) {
                    return CommandResult.Failed(Common["General.CommandError"].Value);
                }
            }
            return CommandResult.Failed(Common["General.UnknownCommand"].Value);
        }

        protected virtual string GetHelpText() {
            return Common["General.HelpTemplate", Name, GetLocationSpecificHelp()].Value;
        }

        protected abstract string GetLocationSpecificHelp();
    }
}