using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yssmet.Services;

namespace Yssmet
{
    public class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();
        private static DiscordSocketClient? _client;      
        private static CommandService? _commands;
        private static IServiceProvider? _services;
        private IConfiguration? _config;
        public async Task MainAsync()
        {
            // Initialize configuration
            _config = BuildConfig();

            // Initialize DiscordSocketClient
            var discordconfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };
            _client = new DiscordSocketClient(discordconfig);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
            _client.Log += Log;
            // Register the MessageReceived event
            _client.MessageReceived += MessageReceived;
            Console.WriteLine("MessageReceived event registered.");
            Console.WriteLine("MainAsync Method is starting.");
            await _services.GetRequiredService<CommandHandlingService>().InitializeAsync(_services);
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, _config["Token"]);
            await _client.StartAsync();
            // Block the program until it is closed
            await Task.Delay(-1);
        }

        private async Task RegisterCommandsAsync()
        {
            //await _commands.AddModulesAsync(typeof(InfoModule).GetTypeInfo().Assembly, _services);
            //await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            //Console.WriteLine("Modules loaded successfully.");
            var module = _commands.Modules.FirstOrDefault(m => m.Name.Equals("InfoModule", StringComparison.OrdinalIgnoreCase));
            Console.WriteLine($"InfoModule found: {module != null}");

            await _commands.AddModulesAsync(typeof(InfoModule).GetTypeInfo().Assembly, _services);
            Console.WriteLine("Modules loaded successfully.");
        }

         private static async Task MessageReceived(SocketMessage arg)
        {
            Exception? ex = null;
            try
            {
                // Check if the message is from a user
                if (arg is not SocketUserMessage message) return;

                // Check if the message is from a bot
                if (message.Author.IsBot) return;

                // Log the command
                Console.WriteLine($"Received message: {message.Content}");

                // Attempt to execute the command
                var result = await _commands.ExecuteAsync(new SocketCommandContext(_client, message), 0, _services);
        
                // Log the command execution result
                Console.WriteLine($"Command execution result: {result}");
                if (message.Content.ToLower() == "!help")
                {
                    await message.Channel.SendMessageAsync("Got it, Boss!");
                }
            }
            catch
            {
                Console.WriteLine($"Exception in MessageReceived: {ex}");
            }            
        } 

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }
        private Task Log(LogMessage msg)
        {
	        Console.WriteLine(msg.ToString());
	        return Task.CompletedTask;
        }
        
    }
}