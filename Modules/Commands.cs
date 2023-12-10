using Discord.Commands;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Yssmet
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
	// ~say hello world -> hello world
	    [Command("say")]
	    [Summary("Echoes a message.")]
	    public static Task SayAsync()
        {
            Console.WriteLine("MainAsync Method is starting.");
            return Task.CompletedTask;
        }
		
	    // ReplyAsync is a method on ModuleBase 
    } 

}