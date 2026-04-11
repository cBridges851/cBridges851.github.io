using Microsoft.JSInterop;
using MyCorner.Commands;

namespace MyCorner.Locations {
    public abstract class Location {
        protected readonly IJSRuntime js;
        private readonly Dictionary<string, Func<string[], Task<CommandResult>>> commands;

        protected Location(IJSRuntime js) {
            this.js = js;
            commands = new();
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
                return CommandResult.Succeeded("Of course! I'd never let you down...");
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
                return await handler(args);
            }
            return CommandResult.Failed("Sorry, I'm not sure what you mean. Try 'help' for available commands.");
        }

        protected virtual string GetHelpText() {
            return $"""
                Available commands in {Name}:<br>
                - help: Show this help text<br>
                {GetLocationSpecificHelp()}
                """;
        }

        protected abstract string GetLocationSpecificHelp();
    }
}