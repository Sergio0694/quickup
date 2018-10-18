using System;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using quickup.Core;
using quickup.Enums;
using quickup.Options;

namespace quickup
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Setup
            ConsoleColor color = Console.ForegroundColor;
            int code;
            bool beep = false, parsed = false;

            // Try to execute the requested action
            try
            {
                ParserResult<QuickupOptions> result = new Parser(setting => setting.CaseInsensitiveEnumValues = true).ParseArguments<QuickupOptions>(args);

                // Only display ==== START ==== if the parsing is successful, to avoid changing colors for the --help auto-screen
                if (result.Tag == ParserResultType.Parsed)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    ConsoleHelper.WriteLine($"{Environment.NewLine}==== START ====");
                    parsed = true;
                }

                // Actual execution of the requested command
                code = result.MapResult(
                    options =>
                    {
                        // Execute the operation and display the info
                        Console.ForegroundColor = ConsoleColor.White; // To display the progress bar
                        options.Validate();
                        foreach (string info in QuickupEngine.Run(options).ExtractStatistics(options.Verbose))
                            ConsoleHelper.WriteTaggedMessage(MessageType.Info, info);
                        beep = options.Beep;
                        return 0;
                    },
                    errors => { Console.Write(HelpText.AutoBuild(result)); return 1; });
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteTaggedMessage(MessageType.Error, e.Message);
                code = 1;
            }

            // Exit code feedback
            if (code == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                ConsoleHelper.WriteLine("==== SUCCESS ====");
            }
            else if (parsed) // Avoid showing the error if the operation never actually started
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                ConsoleHelper.WriteLine("==== FAILURE ====");
            }
            Console.ForegroundColor = color; // Reset to the default color

            // Sound notification
            if (beep)
            {
                if (code == 0)
                {
                    Console.Beep(); Thread.Sleep(150); Console.Beep(); // Two high-pitched beeps to indicate success
                }
                else Console.Beep(320, 500);
            }

#if DEBUG
            Console.ReadKey();
#endif
            return code;
        }
    }
}
