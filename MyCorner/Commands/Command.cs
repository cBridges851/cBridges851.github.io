using MyCorner.Locations;

namespace MyCorner.Commands {
    public record Command(string Verb, string[] Args);

    public class CommandResult {
        public string Message { get; init; } = string.Empty;
        public Location? NewLocation { get; init; }
        public bool Success { get; init; }

        public static CommandResult Succeeded(string message, Location? newLocation = null) {
            return new() { Message = message, NewLocation = newLocation, Success = true };
        }

        public static CommandResult Failed(string message) {
            return new() { Message = message, Success = false };
        }
    }
}