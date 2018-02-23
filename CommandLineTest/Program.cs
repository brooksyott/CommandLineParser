using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Peamel.CommandLineParser;

namespace CommandLineTest
{

    class Program
    {
        public class ConfigService
        {
            private CommandLineParser _clp = new CommandLineParser();


            public ConfigService()
            {
                _clp.RegisterArguement("--help", PrintHelp, false, "Displays help");
                _clp.RegisterArguement("--valueA", ValueAOptionHandler, false, "Processes Option A, which is optional");
                _clp.RegisterArguement("--valueB", ValueBManditoryHandler, true, "This is a manditory argument");
                _clp.RegisterErrorHandler(ArgErrorHandler);
            }

            public void ArgErrorHandler(object sender, CommandLinePargerEventArgs e, out Boolean hadError)
            {
                Console.WriteLine("Invalid Parameter: {0}", e.parameter);
                hadError = false;
            }

            public Boolean ProcessArgs(String[] args)
            {
                return _clp.Parse(args);
            }

            public void ValueAOptionHandler(object sender, CommandLinePargerEventArgs e, out Boolean hadError)
            {
                Console.WriteLine("Hit Value A ptionHandler, Value is: " + e.argvalue);

                hadError = false;
            }

            public void ValueBManditoryHandler(object sender, CommandLinePargerEventArgs e, out Boolean hadError)
            {
                Console.WriteLine("Hit Value B Manditory Handler, Value is: " + e.argvalue);

                hadError = false;
            }


            public void PrintHelp(object sender, CommandLinePargerEventArgs e, out Boolean hadError)
            {
                Console.WriteLine("HELP");
                Console.WriteLine("----");
                _clp.PrintHelp();
                hadError = false;
            }

        }

        static void Main(string[] args)
        {
            ConfigService _configService = new ConfigService();

            Boolean allOk = _configService.ProcessArgs(args);
            if (allOk == false)
            {
                Console.WriteLine("Failed processing args");
                Console.ReadLine();
            }

            Console.WriteLine("All Args passed");
            Console.ReadLine();


        }
    }
}
