using System.Runtime.InteropServices;
using System.Security.Authentication.ExtendedProtection;
using NDesk.Options;
using System.Linq;
using System.Diagnostics.Metrics;
using System.Xml;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Text.Unicode;
using System.Text.RegularExpressions;

namespace dzst
{
    class Program
    {

        #region Strings etc
        public static string helpText =
                "\nAvailable commands are:\n" +
                "'help'         - shows this message.\n" +
                "'types'        - generates barebones types.xml from source.txt file.\n" +
                "'keys'         - simply moves the .bikeys to keys folder easily. :)\n" +
                "'clear|cls'    - clear the console window of any text\n" +
                "'exit'         - stops the executable.\n" +
                
                "\n\nYou can also use this tool via launch arguments to integrate with mod updates / installs!\n"+
                "Please read README.MD for more information on how to use launch options. \n" + 
                "Valid options are: '-k' '-t' '-o' '-d' '-s'\n";
        public static string noKeysText = 
                "\nPlease make sure that this .exe is located in the same folder as DayZ_x64.exe or DayZServer_x86.exe";



        //Args parsing
        static bool doArgs;
        static bool doKeys;
        static bool doTypes;
        static bool doSilent;
        static bool doDelete;
        static string outputInput;

        static string execPath = AppDomain.CurrentDomain.BaseDirectory.ToString();
        static string sourceText = execPath + source;
        static string output;
        static string source = "dzst/source.txt";
        static string outputFile;

        static string typesCfg = "dzst/typesCfg.cfg";
        static string destinationFolder = "keys";
        static int newKeys;
        #endregion
        
        [STAThread]
        static void Main(string[] args)
        {
            #region Start stuff
            if(args.Any())
                doArgs = true; 
            var options = new OptionSet()
            {
                { "o|output="," ", arg => outputInput = arg },
                { "k|keys"," ", arg => doKeys = arg != null },
                { "t|types"," ", arg => doTypes = arg != null },
                { "d|delete"," ", arg => doDelete = arg != null },
                { "s|silent"," ", arg => doSilent = arg != null }
            };
            var extra = options.Parse(args);
            Console.Title = "DayZ Server Tools";
            Console.WriteLine("DayZ Server Tool" + "\ngithub.com/ppyLEK");
            Console.WriteLine(helpText);
            #endregion

            //Check here for string arguments.
            if (string.IsNullOrEmpty(outputInput))
                outputInput = "types_output.xml";
            outputFile = outputInput;
            output = execPath + outputFile;
            Dzst();
        }
        static void Dzst()
        {
            //Set strings
            sourceText = execPath + source;

            if (doKeys)
                Keys();
            if (doTypes)
                Types();

            if (!doArgs)
                while (true)
                { 


                    string option = Console.ReadLine();
                    switch (option)
                    {
                        case "types":
                            Types();
                                continue;

                        case "keys":
                            Keys();
                                continue;

                        case "":
                            Console.WriteLine("Empty input, type 'help' to see the list of avai");
                                continue;

                        case "help":
                            Console.Write(helpText);
                                continue;

                        case "exit":
                            Environment.Exit(0);
                                break;

                        case "clear":
                            Console.Clear();
                                continue;

                        case "cls":
                            Console.Clear();
                            continue;

                        default:
                            Console.WriteLine("Unknown command, please refer to 'help'");
                            continue;
                    }
                    break;
                }
            
        }
        #region Types

        static string GetTypesCfg(int lineNumber)
        {
            string lineReturn = File.ReadLines(execPath + typesCfg).Skip(lineNumber).Take(1).First();
            return lineReturn;
        }
        static void Types()
        {
            if(!File.Exists(typesCfg))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No typesCfg.cfg detected, please make sure that typesCfg.cfg is in /dzst/ folder!");
                Console.ReadKey(true);
                Environment.Exit(0);
            }
            if (File.Exists(outputFile))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Output file already exists! This program will not overwrite it");
                Console.WriteLine("please remove it or rename it to something different.");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }
            if(!File.Exists(outputFile))   
                File.Create(outputFile).Close();
            
            XmlTextWriter xlW = new XmlTextWriter(outputFile, System.Text.Encoding.UTF8);

            XmlWriterSettings wSettings = new XmlWriterSettings();
            wSettings.Indent = true;
            wSettings.ConformanceLevel = ConformanceLevel.Fragment;
            wSettings.OmitXmlDeclaration = true;

            xlW.Formatting = Formatting.Indented;
            xlW.WriteStartDocument();
            xlW.WriteComment("TEST - CHANGE ME - LINE 177");

            xlW.WriteStartElement("types");
            foreach (string line in File.ReadLines(sourceText))
            {
                Console.WriteLine("Adding item: "+line);
                xlW.WriteStartElement("type");
                xlW.WriteAttributeString("name", line.ToString());
                xlW.WriteElementString("nominal"         , GetTypesCfg(0));
                xlW.WriteElementString("lifetime"        , GetTypesCfg(1));
                xlW.WriteElementString("restock"         , GetTypesCfg(2));
                xlW.WriteElementString("min"             , GetTypesCfg(3));
                xlW.WriteElementString("quantmin"        , GetTypesCfg(4));
                xlW.WriteElementString("quantmax"        , GetTypesCfg(5));
                xlW.WriteElementString("cost"            , GetTypesCfg(6));

                
                xlW.WriteElementString("flags count_in_cargo=\"0\" count_in_hoarder=\"0\" count_in_map=\"1\" count_in_player=\"0\" crafted=\"0\" deloot=\"0\"", null);
                
                xlW.WriteStartElement("category");
                xlW.WriteAttributeString("name", GetTypesCfg(7));
                xlW.WriteEndElement();


                xlW.WriteStartElement("usage");
                xlW.WriteAttributeString("name", GetTypesCfg(8));
                xlW.WriteEndElement();
                xlW.WriteStartElement("value");
                xlW.WriteAttributeString("name", GetTypesCfg(9));
                xlW.WriteEndElement();
                xlW.WriteStartElement("value");
                xlW.WriteAttributeString("name", GetTypesCfg(10));
                xlW.WriteEndElement();

                xlW.WriteEndElement();
            }
            xlW.WriteEndElement();
            xlW.Flush(); xlW.Close();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(outputFile);
            xmlDoc.Save(Console.Out);
            
            if (doDelete)
                File.Delete(execPath + source);
            if (doSilent)
                Environment.Exit(0);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nSuccesfully created: " + outputFile.ToString());
            Console.ResetColor();
            Console.WriteLine($"You can exit now by typing 'exit' or continue running scripts.");
            Console.WriteLine($"Type 'help' to see available commands.");
            return;
        }
        #endregion

        #region Keys
        
        static void Keys()
        {
            Console.Clear();
            Directory.CreateDirectory(execPath + "keys");
            var files = Directory.GetFiles(execPath, "*.bikey", SearchOption.AllDirectories);

            foreach (string bikey in files)
            {
                foreach (string s in files)
                {
                    string fileName = Path.GetFileName(s);
                    string destFile = Path.Combine(execPath + destinationFolder, fileName);

                    Boolean result = File.Exists(destFile);
                    if (!result)
                    {
                        newKeys++;
                        File.Copy(s, destFile, true);
                        Console.WriteLine("Moved key:'" + fileName.ToLower() + " to the keys folder.");
                    }
                }
            }

            if (newKeys == 0)
            {
                
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No new keys found.");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done moving keys.");
            Console.ResetColor();

            if (files.Length < 1)
                Console.WriteLine(noKeysText);



            
            if (doTypes)
                Types();
            if(doSilent)
                Environment.Exit(0);
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("If you get .bikey related errors that might be caused by mod creator changing key but naming it the same");
            Console.WriteLine("this is highly unlikely but worth to keeping in mind! (This tool only checks if existing key names match)");
            Console.ResetColor();
            Console.WriteLine($"You can exit now by typing 'exit' or continue running scripts.");
            Dzst();
        }
        #endregion
    }
}