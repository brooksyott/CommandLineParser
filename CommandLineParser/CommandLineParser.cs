using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Reads command line parameters
/// </summary>
namespace CommandLine
{
    /// <summary>
    /// The value read from the commandline after parsing
    /// </summary>
    public class CommandLinePargerEventArgs
    {
        public String parameter = String.Empty;   // The parameter used (must start with "--")
        public String argvalue = String.Empty;    // Value of the parameter
    }

    public class CommandLineParser
    {
        // The delimeter/prefix that determins the value is a parameter or not
        private const String PARAM_PREFIX = "--";

        /// <summary>
        /// The structure that stores the setup information for the defined paramater/values
        /// </summary>
        private struct ArgDefinition
        {
            public CommandLineArgHandler handler;          // Handler (delegate) to call when a commandline parameter has been read
            public Boolean required;                       // defines if the parameter is required
            public String  helpString;                     // help string for the parameter
            public Boolean hadError;                       // returned from the handler, if there was an error with the values passed
            public Boolean parsed;                         // set to true if a parameter was parsed. If a parameter is not optional, and not parsed, parsing is flag as failed
            public String  emptyValueName;                 // One value may not have a parameter called out, so this stores what it is inteaded to be used for
        }

        // This will be set to true when all arguements have been successfully parsed
        public Boolean ArgsParsedSuccessfully = true;

        public delegate void CommandLineArgHandler(Object sender, CommandLinePargerEventArgs e, out Boolean hadError);
        Dictionary<String, ArgDefinition> ArguementHandler = new Dictionary<string, ArgDefinition>();

        // The event handler if the parser has an error, for example detects a parameter that was not defined
        private CommandLineArgHandler ErrorHandler;

        //===========================================================================================
        /// <summary>
        /// Parses a specific parameter, and returns the parameter name, its value
        /// or returns false in that it did not start with prefix.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private void ParseParameter(String arg, out String parameter, out String value)
        {

            String argPrefix = arg.Substring(0, 2);
            parameter = String.Empty;
            value = String.Empty;

            if (argPrefix == PARAM_PREFIX)
            {
                // This is an arguement, so parse it
                // Find the first occurence of the "="
                int eqIndex = arg.IndexOf("=");
                int strLen = arg.Length;
                if (eqIndex == -1)
                {
                    parameter = arg;
                    value = String.Empty;
                }
                else
                {
                    parameter = arg.Substring(0, eqIndex);
                    value = arg.Substring(eqIndex + 1, strLen - eqIndex - 1);
                }
                return;
            }

            value = arg;
        }

        //===========================================================================================
        /// <summary>
        /// Parses the string array and calls handlers registered
        /// </summary>
        /// <param name="args">Arguements to parse</param>
        /// <param name="displayParsing"> Displays debug information. Defaults to false </param>
        public Boolean Parse(String[] args, Boolean displayParsing = false)
        {
            foreach(String a in args)
            {
                // Split the string into the arguement and the value
                String argPrefix = a.Substring(0, 2);
                String key = String.Empty;
                String value = String.Empty;

               ParseParameter(a, out key, out value);
               NotifyCommandLineArgumentParsed(key, value);

                if (displayParsing == true)
                {
                        Console.WriteLine("Parameter: {0} \t Value: {1}", key, value);
                }
            }

            return ArgsPassed();
        }

        //===========================================================================================
        /// <summary>
        /// Prints the parameter list and the associated help
        /// </summary>
        public void PrintHelp()
        {
            String isRequiredString = String.Empty;
            String parameter = String.Empty;

            foreach (KeyValuePair<String, ArgDefinition> ad in ArguementHandler)
            {
                if (ad.Value.required)
                    isRequiredString = "required";
                else
                    isRequiredString = "optional";

                parameter = ad.Key;

                if (String.IsNullOrEmpty(ad.Key))
                {
                    parameter = ad.Value.emptyValueName;
                }

                Console.WriteLine("{0} ({1})\t{2}", parameter, isRequiredString, ad.Value.helpString);
            }
        }

        //===========================================================================================
        /// <summary>
        /// Determins the state of the parsing
        /// </summary>
        /// <param name="print"></param>
        /// <returns>true if there were no errors parsing</returns>
        public Boolean ArgsPassed(Boolean print = false)
        {
            // Make sure none failed

            if (ArgsParsedSuccessfully == false)
            {
                if (print == true)
                    Console.WriteLine("ArgsParsedSuccessfully == false");
                return false;
            }

            String isRequiredString = String.Empty;
            foreach (KeyValuePair<String, ArgDefinition> ad in ArguementHandler)
            {
                if ((ad.Value.required == true) && (ad.Value.parsed == false))
                {
                    if (print)
                       Console.WriteLine("Parameter {0} is required", ad.Key);
                    return false;
                }

                if (ad.Value.hadError == true)
                {
                    if (print)
                        Console.WriteLine("Parameter {0} invalid value", ad.Key);
                    return false;
                }
            }

            return true;
        }

        //===========================================================================================
        /// <summary>
        /// Register a handler for any parsing errors
        /// </summary>
        /// <param name="errorHandler"></param>
        public void RegisterErrorHandler(CommandLineArgHandler errorHandler)
        {
            ErrorHandler = errorHandler;
        }

        //===========================================================================================
        /// <summary>
        /// Internal call to notify any registered handlers of an error
        /// </summary>
        /// <param name="paraameter"></param>
        private void NotifyError(String paraameter)
        {
            
            if (ErrorHandler != null)
            {
                Boolean hadError = false;
                CommandLinePargerEventArgs e = new CommandLinePargerEventArgs();
                e.parameter = paraameter;
                ErrorHandler(this, e, out hadError);
            }
        }

        //===========================================================================================
        /// <summary>
        /// Register a handler for a specific command line arguement
        /// </summary>
        /// <param name="argName"></param>
        /// <param name="argHandler"></param>
        /// <param name="required"></param>
        /// <param name="helpString"></param>
        /// <returns></returns>
        public Boolean RegisterArguement(String argName, CommandLineArgHandler argHandler, Boolean required, String helpString)
        {
            if (String.IsNullOrEmpty(argName))
            {
                Console.WriteLine("Empty paramenter name");
                return false;
            }
            if (ArguementHandler.ContainsKey(argName))
            {
                Console.WriteLine("Argument already registered: " + argName);
                return false;
            }

            ArgDefinition argDef = new ArgDefinition();
            argDef.handler = argHandler;
            argDef.required = required;
            argDef.helpString = helpString;
            argDef.hadError = false;

            String argPrefix = argName.Substring(0, 2);
            if (argPrefix != "--")
            {
                // This is the empty value
                argDef.emptyValueName = argName;
                argName = "";
            }

            ArguementHandler.Add(argName, argDef);
            return true;
        }

        //===========================================================================================
        /// <summary>
        /// Once a command line arguement has been parsed, notify the handler
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="value"></param>
        private void NotifyCommandLineArgumentParsed(String arg, String value)
        {
            CommandLinePargerEventArgs e = new CommandLinePargerEventArgs();
            Boolean hadError = false;


            if (ArguementHandler != null)
            {
                if (ArguementHandler.ContainsKey(arg))
                {
                    if (ArguementHandler[arg].handler != null)
                    {
                        ArgDefinition argResult = ArguementHandler[arg];
                        e.parameter = arg;
                        e.argvalue = value;

                        ArguementHandler[arg].handler(this, e, out hadError);
                        argResult.hadError = hadError;
                        if (hadError == true)
                        {
                            ArgsParsedSuccessfully = false;
                        }
                        argResult.parsed = true;
                        ArguementHandler[arg] = argResult;
                    }
                    else
                    {
                        Console.WriteLine("Arguement had a null handler: " + arg);
                        NotifyError(arg);
                        ArgsParsedSuccessfully = false;
                    }
                }
                else
                {
                    //Console.WriteLine("Arguement was not registerd: " + arg);
                    NotifyError(arg);
                    ArgsParsedSuccessfully = false;
                }
            }
        }
    }
}
