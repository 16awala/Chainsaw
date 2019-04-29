using System;
using System.Collections.Generic;
using System.IO;
using ChainsawEngine;
using System.Linq;
using System.Text;
using System.Threading;



namespace PullStart
{
    static class DualOut//Much of DualOut is not my code, borrowed from StackOverflow. //TODO FIGURE OUT HOW THIS WORKS EXACTLY.
    {
        private static TextWriter _current;
        private static StreamWriter dualS;//Added as an explicit streamwriter for file output
        private static OutputWriter myOutput;//Added
        public static bool isOpen = false;//Added to make sure that we only attempt to close if stuff has been initialized.

        private class OutputWriter : TextWriter
        {
            private string fpath;
            public override Encoding Encoding
            {
                get
                {
                    return _current.Encoding;
                }
            }

            public OutputWriter(string filepath)//Modified this to accept a filepath
            {
                this.fpath = filepath;
            }

            public override void WriteLine(string value)//Modified this to add timestamps to output.
            {
                string temp = "[" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] ";
                temp += value;
                _current.WriteLine(temp);//Write to console
                dualS.WriteLine(temp);//Write to file
            }

            public void WriteLineFileOnly(string value)
            {
                if (isOpen)
                {
                    string temp = "[" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] ";
                    temp += value;
                    dualS.WriteLine(temp);
                }
            }
        }

        public static void Init(string fpath)//Modified this to accept a custom file path, as well as to use an explicit output for the console rather than an anonymous one so we can handle it easily. Original code used an anonymous constructor in the SetOut mutator.
        {
            _current = Console.Out;
            dualS = new StreamWriter(fpath, false, Encoding.ASCII);//Leave buffer up to the streamwriter. Overwrite preexisting files.
            dualS.AutoFlush = true;//Don't want to deal with flushing manually.
            myOutput = new OutputWriter(fpath);
            Console.SetOut(myOutput);
            isOpen = true;
            Console.WriteLine("Begin File Output");
        }

        public static void close()//Added this to allow us to switch back to the original console output and close the file properly.
        {
            if (isOpen)
            {
                Console.WriteLine("End File Output");
                Console.SetOut(_current);
                dualS.Flush();
                dualS.Close();
                isOpen = false;
            }
        }
    }
    class Program
    {
        //Global vars
        private static string[] loopArgs;//String array that stores arguments when in console command mode.
        private static List<Expression> expList = new List<Expression>();//List of expressions
        private static List<string> expNameList = new List<string>();//List of strings used to refer to expressions.
        private static List<double[][]> dataList = new List<double[][]>();
        private static List<string> dataNameList = new List<string>();


        private static bool debug = false;//Global toggle for debug messages and output.
        private static Random randGen = new Random();
        private static TextWriter defOut = Console.Out;//Keep default output here for good measure.
        private static string input = "";

        static void Main(string[] args)
        {
            if (args.Length == 0) { args = new string[] { "" }; }//To keep an empty array from messing with the switch statement (which assumes at least one argument is present).
            switch (args[0].ToLower())
            {
                case "debug":
                    Console.Read();
                    //TEST CODE GOES HERE
                    break;
                default:
                    loopBody();//Continuous loop for user input.
                    break;
            }
            DualOut.close();//In case we are quitting without ending the file.
        }

        public static void loopBody()
        {
            Console.WriteLine("Dnet v0.3.0 r0 11/26/2017");
            Console.WriteLine("Type \"Help\"");
            Console.WriteLine("");
            while (!(input.Equals("quit") || input.Equals("exit")))
            {
                input = Console.ReadLine().ToLower();
                if (DualOut.isOpen) { Console.WriteLine("Input command: " + input); }
                loopArgs = input.Split(' ');//Split into arguments
                Console.WriteLine("");
                switch (loopArgs[0])
                {
                    case "help":
                        Console.WriteLine("Commands:\nQuit: Terminates program.\nExpression: Commands for working with expressions.\nOutput: Commands for configuring program output.\nFile: Commands for file execution\n\nUse <command> \"help\" to learn more about a command.\n");
                        break;
                    case "file":
                        commands.fileBranch();
                        break;
                    case "output":
                        commands.outputBranch();
                        break;
                    case "expression":
                        commands.functionBranch();

                        break;
                    case "debug":
                        commands.Debug();

                        break;
                    case "clear":
                        Console.Clear();
                        break;

                    case "":
                        break;
                    default:
                        Console.WriteLine("Unkown argument \"{0}\"", loopArgs[0]);
                        break;
                }
            }
        }

        public static class commands
        {
            public static void fileBranch()
            {
                switch (loopArgs[1])
                {
                    case "execute":
                        Console.WriteLine("Beginning Execution");
                        executeFile();
                        break;

                    case "execute2":
                        Console.WriteLine("Beginning Execution");
                        executeFile2(loopArgs[2]);
                        break;

                    case "help":
                        printFileHelp();
                        break;
                    default:
                        Console.WriteLine("Unknown argument \"{0}\"", loopArgs[1]);
                        break;
                }
            }

            public static void outputBranch()
            {
                switch (loopArgs[1])
                {
                    case "set":
                        setNewOutput();
                        break;

                    case "unset":
                        unsetOutput();
                        break;
                    case "help":
                        printOutputHelp();
                        break;
                    default:
                        Console.WriteLine("Unkown argument \"{0}\"", loopArgs[1]);
                        break;
                }
            }

            public static void functionBranch()
            {
                switch (loopArgs[1])
                {
                    case "help":
                        printExpressionHelp();
                        break;

                    case "benchmark":
                        benchmarkExpression();
                        break;

                    case "new":
                        newExpression();

                        break;
                    case "list":
                        printAllExpressions();
                        break;

                    case "remove":
                        removeExpression();
                        break;

                    case "evaluate":
                        evaluateExpression();
                        break;

                    case "steps":
                        evaluateExpressionWithSteps();
                        break;

                    case "print":
                        printExpression();
                        break;

                    case "analyse":
                        analyseExpression();
                        break;

                    case "details":
                        printExpressionDetails();
                        break;

                    default:
                        Console.WriteLine("Unkown argument \"{0}\"", loopArgs[1]);
                        break;
                }
            }


            //Debug has two possible arguments and none additional, it is not necessary to create a switch statement for this.
            public static void Debug()
            {
                if (loopArgs.Length == 1)
                {
                    if (debug)
                    {
                        Console.WriteLine("Debug is currently enabled.\n");
                    }
                    else
                    {
                        Console.WriteLine("Debug is currently disabled.\n");
                    }
                }
                else
                {
                    if (loopArgs[1] == "true")
                    {
                        Console.WriteLine("Debug enabled.\n");
                        debug = true;
                    }
                    else
                    {
                        if (loopArgs[1] == "false")
                        {
                            Console.WriteLine("Debug disabled.\n");
                            debug = false;
                        }
                        else
                        {
                            Console.WriteLine("Unkown argument \"{0}\"", loopArgs[1]);
                        }
                    }
                }
            }

            //Various methods that perform processes for the expression command.
            private static void newExpression()
            {
                if (loopArgs.Length > 3)
                {
                    try
                    {
                        if (loopArgs[2] == "random")
                        {
                            string temp = stupidExpression(Convert.ToInt32(loopArgs[3]), randGen);
                            if (debug) { Console.WriteLine("Generated expression: {0}", temp); }
                            expList.Add(new Expression(temp));
                            expNameList.Add(loopArgs[4]);
                        }
                        else
                        {
                            expList.Add(new Expression(loopArgs[2]));
                            expNameList.Add(loopArgs[3]);
                        }
                        Console.WriteLine("New expression created successfully.\n");

                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("\nThere was an error creating the expression. There may be an issue with syntax or the expression entered may not be legal.");
                        if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                    }
                }
                else
                {
                    Console.WriteLine("\nNot enough arguments! Did you forget to name your new expression?\n");
                }
            }//new
            private static void printExpressionHelp()
            {
                Console.WriteLine("\nHelp for Expressions\n\n");
                Console.WriteLine("Analyse\n    Description: Runs pre-analysis on a non-analysed expression.\n    Syntax: expression analyse <name of expression>\n\n     Details: Runs the process required to generate the instructions necessary for evaluation. In most cases this is done automatically, but it can be used to ensure the validity and functionality of an expression before attempting to feed it data. If debug is enabled, executing this command will output analysis information to the console as it is performed. \n");
                Console.WriteLine("Details\n    Description: Shows detailed information about an expression.\n    Syntax: expression benchmark <name of expression>\n\n     Details: Performs basic tests on an expression using random input data to guage evaluation performance in terms of evaluations per second.\n");

                Console.WriteLine("Benchmark\n    Description: Benchmarks an expression and writes results to the console.\n    Syntax: expression benchmark <name of expression>\n\n     Details: Performs basic tests on an expression using random input data to guage evaluation performance in terms of evaluations per second.\n");
                Console.WriteLine("Evaluate\n    Description: Evaluates the expression given variabl data.\n    Syntax: expression evaluate <name of expression> <x0> <x1> <x2> ...\n\n     Details: Each variable xn is declared in the expression given. To evaluate a function you must provide exactly enough arguments to satisfy the expression's variable count.\n");
                Console.WriteLine("List\n    Description: Lists expressions that have been created.\n    Syntax: expression list\n\n");
                Console.WriteLine("New\n    Description: Creates a new expression\n    Syntax: expression new < expression string > < name >\n\n     Details: Creates an expression based on human-readable standard math notation. One example of a valid expression is \"x+2\". The name you give an expression is used to call it back later for other actions. You may create as many as you want.\n");
                Console.WriteLine("Print\n    Description: Prints an expression in standard notation\n    Syntax: expression list <name of expression>\n\n");
                Console.WriteLine("Remove\n    Description: Removes a previously created expression\n    Syntax: expression remove <name of expression>\n\n     Details: Deletes an expression. Deleted expressions cannot be recovered.\n");
                Console.WriteLine("Steps\n    Description: Evaluates an expression and displays each step of the evaluation\n    Syntax: expression steps <name of expression> <x0> <x1> <x2> ...\n\n     Details: Works exactly like the evaluate command, but will thoroughly spam output especially with debug enabled.\n");

                Console.WriteLine("");
            }//help
            private static void benchmarkExpression()
            {
                try
                {
                    int temp = expNameList.IndexOf(loopArgs[2]);
                    Console.WriteLine("Benchmarking expression \"{0}\"", loopArgs[2]);
                    expList[temp].Benchmark();
                    Console.WriteLine("\n           Expression: {0}", expList[temp].toString(false));
                    Console.WriteLine("       Steps to solve: {0}", expList[temp].steps);
                    Console.WriteLine("           Single OPS: {0}", expList[temp].opsSingle);
                    Console.WriteLine("         Threaded OPS: {0}", expList[temp].opsMutliTotal);
                }
                catch (Exception err)
                {
                    Console.WriteLine("\nThere was an error benchmarking the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                }
            }//benchmark
            private static void printAllExpressions()
            {
                Console.WriteLine("Currently declared expressions:");
                foreach (var item in expNameList)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("");
            }//list
            private static void printExpression()
            {
                try
                {
                    int temp = expNameList.IndexOf(loopArgs[2]);
                    Console.WriteLine(expList[temp].toString());
                    Console.WriteLine();
                }
                catch (Exception err)
                {
                    Console.WriteLine("\nThere was an error printing the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                }
            }//print
            private static void evaluateExpression()
            {
                try
                {
                    Expression E = expList[expNameList.IndexOf(loopArgs[2])];
                    if (loopArgs.Length - 3 != E.varCount)
                    {
                        Console.WriteLine("\nEither too many or too few variable values were provided. Check your input!");
                    }
                    else
                    {
                        try
                        {
                            double[] Einputs = new double[E.varCount];
                            for (int i = 0; i < loopArgs.Length - 3; i++)
                            {
                                Einputs[i] = Convert.ToDouble(loopArgs[i + 3]);
                            }
                            if (E.varCount == 0)
                            {
                                if (debug)
                                {
                                    Console.WriteLine("{0} = {2}", loopArgs[2], E.evalWithDebug(Einputs));
                                }
                                else
                                {
                                    Console.WriteLine("{0} = {2}", loopArgs[2], E.evaluate(Einputs));
                                }
                            }
                            else
                            {
                                if (debug)
                                {
                                    Console.WriteLine("{0}({1}) = {2}", loopArgs[2], Utils.arrayToString(Einputs), E.evalWithDebug(Einputs));
                                }
                                else
                                {
                                    Console.WriteLine("{0}({1}) = {2}", loopArgs[2], Utils.arrayToString(Einputs), E.evaluate(Einputs));
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine("\nThere was an error evaluating the expression. There may be an issue with syntax or the values entered are illegal.");
                            if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                        }
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine("\nThere was an error initializing the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                }
            }//evaluate
            private static void evaluateExpressionWithSteps()
            {
                try
                {
                    Expression E = expList[expNameList.IndexOf(loopArgs[2])];
                    if (loopArgs.Length - 3 != E.varCount)
                    {
                        Console.WriteLine("\nEither too many or too few variable values were provided. Check your input!");
                    }
                    else
                    {
                        try
                        {
                            double[] Einputs = new double[E.varCount];
                            for (int i = 0; i < loopArgs.Length - 3; i++)
                            {
                                Einputs[i] = Convert.ToDouble(loopArgs[i + 3]);
                            }
                            if (E.varCount == 0)
                            {
                                if (debug)
                                {
                                    Console.WriteLine("{0} = {2}", loopArgs[2], E.evalWithDebug(Einputs, true));
                                }
                                else
                                {
                                    Console.WriteLine("{0} = {2}", loopArgs[2], E.evalWithDebug(Einputs));
                                }
                            }
                            else
                            {
                                if (debug)
                                {
                                    Console.WriteLine("{0}({1}) = {2}", loopArgs[2], Utils.arrayToString(Einputs), E.evalWithDebug(Einputs, true));
                                }
                                else
                                {
                                    Console.WriteLine("{0}({1}) = {2}", loopArgs[2], Utils.arrayToString(Einputs), E.evalWithDebug(Einputs));
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine("\nThere was an error evaluating the expression. There may be an issue with syntax or the values entered are illegal.");
                            if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                        }
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine("\nThere was an error initializing the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                }
            }//steps
            private static void analyseExpression()
            {
                try
                {
                    int temp = expNameList.IndexOf(loopArgs[2]);
                    expList[temp].analysis(debug);
                    Console.WriteLine("Expression: {0}\nEvaluation steps: {1}\nVariable count: {2}\n", expList[temp].toString(), expList[temp].steps, expList[temp].varCount);
                }
                catch (Exception err)
                {
                    Console.WriteLine("\nThere was an error analyzing the expression. The expression may not exist, or a translation error may have occured");
                    if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                }
            }//analyse
            private static void printExpressionDetails()
            {
                try
                {
                    int temp = expNameList.IndexOf(loopArgs[2]);
                    Console.WriteLine("Details for expression \"" + loopArgs[2] + "\".\n");
                    Expression temp2 = expList[temp];
                    Console.WriteLine("         Human readable: " + temp2.toString());
                    Console.WriteLine("         Raw Type array: " + Utils.arrayToString(temp2.rawTypes, " , "));
                    Console.WriteLine("         Raw Data array: " + Utils.arrayToString(temp2.rawData, " , "));
                    Console.WriteLine("        Instruction set: " + temp2.getFormattedInstructionSet());
                    Console.WriteLine("\n         Steps to solve: {0}", expList[temp].steps);
                    Console.WriteLine("             Single OPS: {0}", expList[temp].opsSingle);
                    Console.WriteLine("           Threaded OPS: {0}", expList[temp].opsMutliTotal);
                    Console.WriteLine("\n\n");
                }
                catch (Exception err)
                {
                    Console.WriteLine("\nThere was an error printing the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                }
            }//details
            public static void removeExpression()
            {
                try
                {
                    int temp = expNameList.IndexOf(loopArgs[2]);
                    expList.RemoveAt(temp);
                    expNameList.RemoveAt(temp);
                    Console.WriteLine("\nExpression removed.\n");
                }
                catch (Exception err)
                {
                    Console.WriteLine("\nThere was an error removing the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                }
            }//remove
            public static void printFileHelp()
            {
                Console.WriteLine("\nExecute\n    Description: Reads a file into an expression and outputs results\n    Syntax: file execute <filename>\n\n     Details: Reads an expression from the first line of a text file and passes the rest of the file in as data. \n");
            }
            public static void printOutputHelp()
            {
                Console.WriteLine("\nSet\n    Description: Sets auxillary file output\n    Syntax: output set [filename]\n\n     Details: Duplicates console output to a text file. If unspecified, the file will be named \"log.txt\" \n");
                Console.WriteLine("\nUnset\n    Description: unmounts auxillary file output\n    Syntax: output unset \n\n     Details: If applicable, closes set output file and reverts back to console-only output. \n");
            }

            public static void loadData(string filePath)
            {
                loadData(filePath, Utils.stripExtension(filePath));
            }

            //TODO Streamline how data sets are organized to simplify statistical analysis?
            //TODO Implement the ability to use dynamic constants such as the sum or average of a datast or its sample size
            public static void loadData(string filePath, string name)
            {
                List<double[]> dataSet = new List<double[]>();
                double[] dataRow;
                string[] dataLine;
                string stream;

                StreamReader datastream = new StreamReader(filePath);
                while (!datastream.EndOfStream)
                {
                    stream = datastream.ReadLine();
                    stream = stream.Replace(" ", "");
                    dataLine = stream.Split(',');
                    dataRow = new double[dataLine.Length];

                    for (int i = 0; i < dataLine.Length; i++)
                    {
                        dataRow[i] = Double.Parse(dataLine[i]);
                    }

                    dataSet.Add(dataRow);
                }

                defineData(name, dataSet.ToArray());
            }

            public static void loadData(string filePath, string name, int colStart, int colEnd, int rowStart, int rowEnd)
            {
                List<double[]> dataSet = new List<double[]>();
                double[] dataRow;
                string[] dataLine;
                int lineCounter = 0;

                StreamReader datastream = new StreamReader(filePath);

                if (rowStart > 0)//Skip to first column
                {
                    for (lineCounter = 0; lineCounter < colStart; lineCounter++)
                    {
                        datastream.ReadLine();
                        lineCounter++;
                    }
                }


                while (!datastream.EndOfStream && lineCounter <= colEnd)
                {
                    dataLine = datastream.ReadLine().Remove(' ').Split(',');
                    dataLine = Utils.trimArray(dataLine, colStart, colEnd);//Select from row
                    dataRow = new double[dataLine.Length];

                    for (int i = 0; i < dataLine.Length; i++)
                    {
                        dataRow[i] = Double.Parse(dataLine[i]);
                    }

                    dataSet.Add(dataRow);
                    lineCounter++;
                }

                defineData(name, dataSet.ToArray());
            }

            public static void defineData(string name, double[][] set)
            {
                if (dataNameList.Contains(name))
                {
                    dataList.RemoveAt(dataNameList.IndexOf(name));
                    dataNameList.Remove(name);
                }
                dataNameList.Add(name);
                dataList.Add(set);
            }

            public static void processData(string dataName, string expName)
            {
                processData(dataName, expName, dataName);
            }

            public static void processData(string dataName, string expName, string outputName)
            {
                double[][] input = dataList.ElementAt(dataNameList.IndexOf(dataName));

                Expression func = expList.ElementAt(expNameList.IndexOf(expName));
                //TODO replace expList etc with linked list objects for simplicity
                processData(input, func, outputName);
            }

            private static void processData(double[][] input, Expression func, string outputName)
            {
                double[][] output = new double[input.Length][];
                for (int i = 0; i < input.Length; i++)
                {
                    output[i] = new double[] { func.evaluate(input[i]) };
                }

                defineData(outputName, output);
            }

            private static void processData(double[][] input, PolyExpression func, string outputName)
            {
                double[][] output = new double[input.Length][];
                for (int i = 0; i < input.Length; i++)
                {
                    output[i] = func.evaluate(input[i]);
                }

                defineData(outputName, output);
            }

            public static void saveData(string path, double[][] data)
            {
                string line;
                StreamWriter outstream = new StreamWriter(path);
                for (int i = 0; i < data.Length; i++)
                {
                    line = String.Empty;
                    for (int j = 0; j < data[i].Length; j++)
                    {
                        line += data[i][j].ToString();
                    }
                    outstream.WriteLine(line);
                }
                outstream.Close();
            }

            public static void saveData(string path, double[][] data, int colStart, int colEnd, int rowStart, int rowEnd)
            {
                string line;
                StreamWriter outstream = new StreamWriter(path);

                for (int i = rowStart; i <= rowEnd; i++)//For each row
                {
                    line = String.Empty;
                    for (int j = colStart; j < colEnd; j++)//Create a row from col selection
                    {
                        line += data[i][j].ToString();//single value
                    }
                    outstream.WriteLine(line);//write the row
                }
                outstream.Close();

            }

            public static void executeFile()
            {
                if (debug)
                {
                    Console.WriteLine("Prep for file evaluation");
                }
                StreamReader expStream = new StreamReader(loopArgs[2], true);
                StreamReader dataStream;
                StreamReader dataStream2;//Necessary when using two input files.
                StreamWriter outputStream;
                string name = Utils.stripExtension(loopArgs[2]);
                string inputPath;
                string outputPath;
                string temp;
                string[] tempstrings;
                double[] tempvars;
                int linecounter = 1;
                int linecounter2;

                if (debug)
                {
                    Console.WriteLine("[{0}] {1} Read expression from file", loopArgs[1], linecounter);
                }
                Expression filexp = new Expression(expStream.ReadLine());//First line is our expression
                if (debug)
                {
                    linecounter++;
                    Console.WriteLine("[{0}] Expression Read as \"{1}\"", loopArgs[1], filexp.toString());
                }
                //Register our expression because why not
                expList.Add(filexp);
                expNameList.Add(name);

                while (!expStream.EndOfStream)
                {
                    if (debug)
                    {
                        Console.WriteLine("[{0}] {1} Read input file path from expression file", loopArgs[1], linecounter);
                        linecounter++;
                    }
                    inputPath = expStream.ReadLine().Replace("\"", string.Empty);
                    outputPath = inputPath + " output.txt";
                    dataStream = new StreamReader(inputPath, true);
                    outputStream = new StreamWriter(outputPath, false, Encoding.ASCII);
                    outputStream.AutoFlush = true;
                    linecounter2 = 1;

                    while (!dataStream.EndOfStream)
                    {
                        if (debug)
                        {
                            Console.WriteLine("[{0}] {1} Getting variables...", inputPath, linecounter2);
                        }
                        temp = dataStream.ReadLine().Replace(" ", string.Empty);//Get a line and strip out spaces
                        tempstrings = temp.Split(',');//Split the string by commas
                        tempvars = new double[tempstrings.Length];
                        for (int i = 0; i < tempstrings.Length; i++)//Convert tempstrings to tempvars
                        {
                            tempvars[i] = Convert.ToDouble(tempstrings[i]);
                        }

                        if (debug)
                        {
                            Console.WriteLine("[{0}] {1} Evaluating variables...", inputPath, linecounter2);
                            linecounter2++;
                        }

                        if (tempvars.Length != filexp.varCount)
                        {
                            outputStream.WriteLine("Bad input, unmatched var count");
                            if (debug)
                            {
                                Console.WriteLine("[{0}] {1} Bad variable count!!!", inputPath, linecounter2);
                            }
                        }
                        else
                        {
                            if (debug)
                            {
                                outputStream.WriteLine("{0}", filexp.evalWithDebug(tempvars));
                            }
                            else
                            {
                                outputStream.WriteLine("{0}", filexp.evaluate(tempvars));
                            }
                        }
                    }
                    if (debug)
                    {
                        Console.WriteLine("[{0}] {1} End of File.", inputPath, linecounter2);
                    }
                    outputStream.Flush();
                    outputStream.Close();
                    dataStream.Close();
                }
                expStream.Close();
                Console.WriteLine("Execution completed.\n");
            }

            public static void executeFile2(string scriptPath)
            {
                executeFile2(scriptPath, true);
            }

            public static void executeFile2(string scriptPath, bool throwErrors)
            {
                List<string> script = new List<string>();
                int lineCounter;
                string[] instruction;

                //Read script file into memory
                if (debug)
                {
                    Console.WriteLine("Loading Script...");
                }
                StreamReader scriptStream = new StreamReader(scriptPath, true);

                while (!scriptStream.EndOfStream)
                {
                    script.Add(scriptStream.ReadLine());
                }

                if (debug)
                {
                    Console.WriteLine("Beginning script execution...");
                }

                for (lineCounter = 0; lineCounter < script.Count(); lineCounter++)
                {
                    //Get instruction at this line
                    instruction = script.ElementAt(lineCounter).Split(' ');
                    switch (instruction[0])
                    {
                        case "define"://define expName expression stuff (spaces are removed and concated)
                            instruction = squishArgs(instruction, 2);
                            defineExpression(instruction[1], new Expression(instruction[2]));
                            break;

                        case "load"://load dataPath [dataname] [<row> <row> <column> <column>]
                            switch (instruction.Length)
                            {
                                case 2:
                                    loadData(instruction[1]);
                                    break;
                                case 3:
                                    loadData(instruction[1], instruction[2]);
                                    break;
                                case 7:
                                    loadData(instruction[1], instruction[2], int.Parse(instruction[3]), int.Parse(instruction[4]), int.Parse(instruction[5]), int.Parse(instruction[6]));
                                    break;
                                default:
                                    if (throwErrors)
                                    {
                                        throw new FormatException();
                                    }
                                    break;
                            }
                            break;
                        case "execute":
                            executeFile2(instruction[1], throwErrors);
                            break;

                        case "process"://process <datasetName> <expressionName> [output]
                            switch (instruction.Length)
                            {
                                case 3:
                                    processData(instruction[1], instruction[2]);
                                    break;
                                case 4:
                                    processData(instruction[1], instruction[2], instruction[3]);
                                    break;

                                default:
                                    if (throwErrors)
                                    {
                                        throw new FormatException();
                                    }
                                    break;
                            }
                            break;
                        case "save":
                            saveData(instruction[2], dataList.ElementAt(dataNameList.IndexOf(instruction[1])));
                            break;
                        default:
                            break;
                    }
                }

            }

            //Output handlers
            private static void setNewOutput()
            {
                try
                {
                    if (loopArgs.Length < 3)
                    {
                        DualOut.Init("log.txt");
                    }
                    else
                    {
                        DualOut.Init(loopArgs[2]);
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine("There was an unexpected issue in intitializing the file output.");
                    if (debug) { Console.WriteLine("Message: {0}", err.Message); Console.WriteLine("Stacktrace: ", err.StackTrace); }
                    throw;
                }

            }
            private static void unsetOutput()
            {
                try
                {
                    DualOut.close();
                    Console.WriteLine("File Closed Successfully");
                }
                catch (Exception err)
                {
                    Console.WriteLine("There was an unexpected issue in closing the file stream.");
                    if (debug) { Console.WriteLine("Message: {0}", err.Message); Console.WriteLine("Stacktrace: ", err.StackTrace); }
                    throw;
                }
            }

        }

        public static void defineExpression(string name, Expression exp)
        {
            if (expNameList.Contains(name))
            {
                expList.RemoveAt(expNameList.IndexOf(name));
                expNameList.Remove(name);
            }

            expList.Add(exp);
            expNameList.Add(name);
        }

        public static string[] squishArgs(string[] args, int start)
        {
            return squishArgs(args, start, args.Length - 1);
        }

        public static string[] squishArgs(string[] args, int start, int end)
        {
            string[] retVal = new string[args.Length - (end - start)];
            string squishedString = String.Empty;
            int retValIndex = 0;

            for (int i = 0; i < start; i++)
            {
                retVal[retValIndex] = args[i];
                retValIndex++;
            }

            for (int i = start; i <= end; i++)
            {
                squishedString += args[i];
            }

            retVal[retValIndex] = squishedString;
            retValIndex++;

            for (int i = end + 1; i < args.Length - 1; i++)
            {
                retVal[retValIndex] = args[i];
                retValIndex++;
            }

            return retVal;

        }

        public static string stupidExpression(int n, Random gen)//makes a random function with one x variable
        {
            int openparens = 0;
            int distfromparen = 0;
            double rand;
            string retval = "x";//Start with x
            string[] mathops = new string[] { "^", "*", "+", "-", "*", "/" };
            string[] onehandedops = new string[] { "sin", "cos", "tan", "sec", "csc", "cot", "abs" };
            for (int i = 0; i < n; i++)
            {
                retval += randstr(mathops, gen);//Add operator
                rand = gen.NextDouble();//Add something else
                if (rand < 0.4)
                {
                    retval += Convert.ToString(Math.Round((gen.NextDouble() * 100), 2));//constant
                }
                else
                {
                    if (rand < 0.65)//var
                    {
                        retval += "x";
                    }
                    else
                    {
                        if (rand < 0.8)//oho
                        {
                            retval += randstr(onehandedops, gen);
                            if (gen.NextDouble() < 0.5)
                            {
                                retval += Convert.ToString(Math.Round((gen.NextDouble() * 100), 2));//constant
                            }
                            else
                            {
                                retval += "x";
                            }
                        }
                        else//paren
                        {
                            if (gen.NextDouble() < 0.5)
                            {
                                //add
                                retval += "(";
                                openparens++;
                                distfromparen = 0;
                                if (gen.NextDouble() < 0.5)
                                {
                                    retval += Convert.ToString(Math.Round((gen.NextDouble() * 100), 2));//constant
                                }
                                else
                                {
                                    retval += "x";
                                }
                            }
                            else
                            {
                                //subtract
                                if (gen.NextDouble() < 0.5)
                                {
                                    retval += Convert.ToString(Math.Round((gen.NextDouble() * 100), 2));//constant
                                }
                                else
                                {
                                    retval += "x";
                                }
                                if (distfromparen > 1 && openparens > 0)
                                {
                                    retval += ")";
                                    openparens--;
                                }
                            }
                        }
                    }
                }
                distfromparen++;
                if (distfromparen > 100 && openparens > 0)
                {
                    for (int k = 0; k < openparens; k++)
                    {
                        retval += ")";
                    }
                    openparens = 0;
                }
            }
            if (openparens > 0)
            {
                for (int i = 0; i < openparens; i++)
                {
                    retval += ")";
                }
            }
            return retval;
        }

        public static Expression stupidExpressionRaw(int n, Random gen)//makes a random function with one x variable in raw data
        {
            int openparens = 0;
            int distfromstartparen = 0;
            int distfromparen = 0;
            double rand;
            int[] t = new int[n * 2];//Start with x
            double[] d = new double[n * 2];
            double[] mathops = new double[] { 4, 5, 6, 7, 8 };
            double[] onehandedops = new double[] { 9, 10, 11, 12, 13, 14, 15 };
            t[0] = 2;
            d[0] = 0;
            int p = 1;
            double progint = n / 100;
            double prog = progint;
            while (p < n)
            {
                if (p > prog)
                {
                    Console.WriteLine("Generating: {0}", Math.Round((double)(p) / (double)(n), 4) * 100);
                    prog = p + progint;
                }
                t[p] = 1;
                d[p] += randstr(mathops, gen);//Add operator
                p++;
                rand = gen.NextDouble();//Add something else
                if (rand < 0.4)
                {
                    t[p] = 0;
                    d[p] = Math.Round((gen.NextDouble() * 100), 2);//constant
                }
                else
                {
                    if (rand < 0.65)//var
                    {
                        t[p] = 2;
                        d[p] = 0;
                    }
                    else
                    {
                        if (rand < 0.8)//oho
                        {
                            t[p] = 1;
                            d[p] = randstr(onehandedops, gen);
                            p++;
                            if (gen.NextDouble() < 0.5)
                            {
                                t[p] = 0;
                                d[p] = Math.Round((gen.NextDouble() * 100), 2);//constant
                            }
                            else
                            {
                                t[p] = 2;
                                d[p] = 0;
                            }
                        }
                        else//paren
                        {
                            if (gen.NextDouble() < 0.5)
                            {
                                //add
                                t[p] = 1;
                                d[p] = 0;
                                p++;
                                if (openparens == 0) { distfromstartparen = 0; }
                                distfromparen = 0;
                                openparens++;
                                if (gen.NextDouble() < 0.5)
                                {
                                    t[p] = 0;
                                    d[p] = Math.Round((gen.NextDouble() * 100), 2);//constant
                                }
                                else
                                {
                                    t[p] = 2;
                                    d[p] = 0;
                                }
                            }
                            else
                            {
                                //subtract
                                if (gen.NextDouble() < 0.5)
                                {
                                    t[p] = 0;
                                    d[p] = Math.Round((gen.NextDouble() * 100), 2);//constant
                                }
                                else
                                {
                                    t[p] = 2;
                                    d[p] = 0;
                                }
                                if (distfromparen > 1 && openparens > 0)
                                {
                                    p++;
                                    t[p] = 1;
                                    d[p] = 1;
                                    openparens--;
                                }
                            }
                        }
                    }
                }
                distfromstartparen++;
                if (distfromstartparen > 100 && openparens > 0)
                {
                    if (p + openparens + 5 > d.Length)
                    {
                        d = Utils.extendArray(d, n / 2);
                        t = Utils.extendArray(t, n / 2);
                    }
                    t[p] = 0;
                    d[p] = 1;
                    p++;
                    t[p] = 1;
                    d[p] = 7;
                    p++;
                    t[p] = 0;
                    d[p] = 1;
                    p++;
                    for (int k = 0; k < openparens; k++)
                    {
                        p++;
                        t[p] = 1;
                        d[p] = 1;
                    }
                    openparens = 0;
                }
                p++;
                if (p + 5 > d.Length)
                {
                    d = Utils.extendArray(d, n / 2);
                    t = Utils.extendArray(t, n / 2);
                }
            }
            if (openparens > 0)
            {
                t[p] = 0;
                d[p] = 1;
                p++;
                t[p] = 1;
                d[p] = 7;
                p++;
                t[p] = 0;
                d[p] = 1;
                p++;
                for (int i = 0; i < openparens; i++)
                {
                    p++;
                    t[p] = 1;
                    d[p] = 1;
                }
            }
            d = Utils.crunchArray(d, (d.Length - p) - 1);
            t = Utils.crunchArray(t, (t.Length - p) - 1);
            return new Expression(d, t);
        }

        public static type randstr<type>(type[] s, Random r)
        {
            int AGH = (Int32)(Math.Round((r.NextDouble() * (s.Length - 1))));
            return s[AGH];
        }
    }
}