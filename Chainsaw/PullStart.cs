using System;
using System.Collections.Generic;
using System.IO;
using ChainsawEngine;
using System.Linq;
using System.Text;
using System.Threading;



namespace PullStart
{
    public static class DualOut
    {
        public static bool isOpen = false;
        private static TextWriter _current;
        private static StreamWriter dualS;
        private static DualOut.OutputWriter myOutput;

        public static void Init(string fpath)
        {
            DualOut._current = Console.Out;
            DualOut.dualS = new StreamWriter(fpath, false, Encoding.ASCII);
            DualOut.dualS.AutoFlush = true;
            DualOut.myOutput = new DualOut.OutputWriter(fpath);
            Console.SetOut((TextWriter)DualOut.myOutput);
            DualOut.isOpen = true;
            Console.WriteLine("Begin File Output");
        }

        public static void close()
        {
            if (!DualOut.isOpen)
                return;
            Console.WriteLine("End File Output");
            Console.SetOut(DualOut._current);
            DualOut.dualS.Flush();
            DualOut.dualS.Close();
            DualOut.isOpen = false;
        }

        private class OutputWriter : TextWriter
        {
            private string fpath;

            public override Encoding Encoding
            {
                get
                {
                    return DualOut._current.Encoding;
                }
            }

            public OutputWriter(string filepath)
            {
                this.fpath = filepath;
            }

            public override void WriteLine(string value)
            {
                string str = "[" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] " + value;
                DualOut._current.WriteLine(str);
                DualOut.dualS.WriteLine(str);
            }

            public void WriteLineFileOnly(string value)
            {
                if (!DualOut.isOpen)
                    return;
                string str = "[" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "] " + value;
                DualOut.dualS.WriteLine(str);
            }
        }
    }
    public class PullStart
    {
        private static List<Expression> expList = new List<Expression>();
        private static List<string> expNameList = new List<string>();
        private static List<double[][]> dataList = new List<double[][]>();
        private static List<string> dataNameList = new List<string>();
        private static bool debug = false;
        private static Random randGen = new Random();
        private static TextWriter defOut = Console.Out;
        private static string input = "";
        private static string[] loopArgs;

        private static void Main(string[] args)
        {
            if (args.Length == 0)
                args = new string[1] { "" };
            if (args[0].ToLower() == "debug")
                Console.Read();
            else
                PullStart.loopBody();
            DualOut.close();
        }

        public static void loopBody()
        {
            while (!PullStart.input.Equals("quit") && !PullStart.input.Equals("exit"))
            {
                PullStart.input = Console.ReadLine().ToLower();
                if (DualOut.isOpen)
                    Console.WriteLine("Input command: " + PullStart.input);
                PullStart.loopArgs = PullStart.input.Split(' ');
                Console.WriteLine("");
                switch (PullStart.loopArgs[0])
                {
                    case "":
                        continue;
                    case "clear":
                        Console.Clear();
                        goto case "";
                    case "debug":
                        PullStart.commands.Debug();
                        goto case "";
                    case "expression":
                        PullStart.commands.functionBranch();
                        goto case "";
                    case "file":
                        PullStart.commands.fileBranch();
                        goto case "";
                    case "help":
                        Console.WriteLine("Commands:\nQuit: Terminates program.\nExpression: Commands for working with expressions.\nOutput: Commands for configuring program output.\nFile: Commands for file execution\n\nUse <command> \"help\" to learn more about a command.\n");
                        goto case "";
                    case "output":
                        PullStart.commands.outputBranch();
                        goto case "";
                    default:
                        Console.WriteLine("Unkown argument \"{0}\"", (object)PullStart.loopArgs[0]);
                        goto case "";
                }
            }
        }

        public static Expression ExpressionWithProgress(string exp)
        {
            ThreadedParser threadedParser = new ThreadedParser(exp);
            double num1 = 0.0;
            int num2 = 0;
            double num3 = 0.0;
            double num4 = 0.0;
            threadedParser.parse();
            Console.Write(threadedParser.getStatus());
            while (threadedParser.getJob().Equals("Parsing"))
            {
                double num5 = (double)(num2 / 10) / threadedParser.getProgress() * (1.0 - threadedParser.getProgress());
                if (num2 % 10 == 0)
                    num3 = num4 - num5 - 1.0;
                num1 = num2 != 9 ? (num1 * 9.0 + num5 + num5 * -num3) / 10.0 : num5;
                if (num2 % 10 == 0)
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds((double)(int)num1);
                    Console.WriteLine(threadedParser.getStatus() + " ETA: " + string.Format("{0:D2}h:{1:D2}m:{2:D2}s", (object)timeSpan.Hours, (object)timeSpan.Minutes, (object)timeSpan.Seconds));
                }
                Thread.Sleep(100);
                ++num2;
                num4 = num5;
            }
            return (Expression)threadedParser;
        }

        public static void defineExpression(string name, Expression exp)
        {
            if (PullStart.expNameList.Contains(name))
            {
                PullStart.expList.RemoveAt(PullStart.expNameList.IndexOf(name));
                PullStart.expNameList.Remove(name);
            }
            PullStart.expList.Add(exp);
            PullStart.expNameList.Add(name);
        }

        public static string[] squishArgs(string[] args, int start)
        {
            return PullStart.squishArgs(args, start, args.Length - 1);
        }

        public static string[] squishArgs(string[] args, int start, int end)
        {
            string[] strArray = new string[args.Length - (end - start)];
            string empty = string.Empty;
            int index1 = 0;
            for (int index2 = 0; index2 < start; ++index2)
            {
                strArray[index1] = args[index2];
                ++index1;
            }
            for (int index2 = start; index2 <= end; ++index2)
                empty += args[index2];
            strArray[index1] = empty;
            int index3 = index1 + 1;
            for (int index2 = end + 1; index2 < args.Length - 1; ++index2)
            {
                strArray[index3] = args[index2];
                ++index3;
            }
            return strArray;
        }

        public static string stupidExpression(int n, Random gen)
        {
            int num1 = 0;
            int num2 = 0;
            string str1 = "x";
            string[] s1 = new string[6]
            {
        "^",
        "*",
        "+",
        "-",
        "*",
        "/"
            };
            string[] s2 = new string[7]
            {
        "sin",
        "cos",
        "tan",
        "sec",
        "csc",
        "cot",
        "abs"
            };
            for (int index1 = 0; index1 < n; ++index1)
            {
                string str2 = str1 + PullStart.randstr<string>(s1, gen);
                double num3 = gen.NextDouble();
                if (num3 < 0.4)
                    str1 = str2 + Convert.ToString(Math.Round(gen.NextDouble() * 100.0, 2));
                else if (num3 < 0.65)
                    str1 = str2 + "x";
                else if (num3 < 0.8)
                {
                    string str3 = str2 + PullStart.randstr<string>(s2, gen);
                    str1 = gen.NextDouble() >= 0.5 ? str3 + "x" : str3 + Convert.ToString(Math.Round(gen.NextDouble() * 100.0, 2));
                }
                else if (gen.NextDouble() < 0.5)
                {
                    string str3 = str2 + "(";
                    ++num1;
                    num2 = 0;
                    str1 = gen.NextDouble() >= 0.5 ? str3 + "x" : str3 + Convert.ToString(Math.Round(gen.NextDouble() * 100.0, 2));
                }
                else
                {
                    str1 = gen.NextDouble() >= 0.5 ? str2 + "x" : str2 + Convert.ToString(Math.Round(gen.NextDouble() * 100.0, 2));
                    if (num2 > 1 && num1 > 0)
                    {
                        str1 += ")";
                        --num1;
                    }
                }
                ++num2;
                if (num2 > 100 && num1 > 0)
                {
                    for (int index2 = 0; index2 < num1; ++index2)
                        str1 += ")";
                    num1 = 0;
                }
            }
            if (num1 > 0)
            {
                for (int index = 0; index < num1; ++index)
                    str1 += ")";
            }
            return str1;
        }

        public static Expression stupidExpressionRaw(int n, Random gen)
        {
            int num1 = 0;
            int num2 = 0;
            int num3 = 0;
            int[] numArray1 = new int[n * 2];
            double[] numArray2 = new double[n * 2];
            double[] s1 = new double[5] { 4.0, 5.0, 6.0, 7.0, 8.0 };
            double[] s2 = new double[7]
            {
        9.0,
        10.0,
        11.0,
        12.0,
        13.0,
        14.0,
        15.0
            };
            numArray1[0] = 2;
            numArray2[0] = 0.0;
            int index1 = 1;
            double num4 = (double)(n / 100);
            double num5 = num4;
            while (index1 < n)
            {
                if ((double)index1 > num5)
                {
                    Console.WriteLine("Generating: {0}", (object)(Math.Round((double)index1 / (double)n, 4) * 100.0));
                    num5 = (double)index1 + num4;
                }
                numArray1[index1] = 1;
                numArray2[index1] += PullStart.randstr<double>(s1, gen);
                int index2 = index1 + 1;
                double num6 = gen.NextDouble();
                if (num6 < 0.4)
                {
                    numArray1[index2] = 0;
                    numArray2[index2] = Math.Round(gen.NextDouble() * 100.0, 2);
                }
                else if (num6 < 0.65)
                {
                    numArray1[index2] = 2;
                    numArray2[index2] = 0.0;
                }
                else if (num6 < 0.8)
                {
                    numArray1[index2] = 1;
                    numArray2[index2] = PullStart.randstr<double>(s2, gen);
                    ++index2;
                    if (gen.NextDouble() < 0.5)
                    {
                        numArray1[index2] = 0;
                        numArray2[index2] = Math.Round(gen.NextDouble() * 100.0, 2);
                    }
                    else
                    {
                        numArray1[index2] = 2;
                        numArray2[index2] = 0.0;
                    }
                }
                else if (gen.NextDouble() < 0.5)
                {
                    numArray1[index2] = 1;
                    numArray2[index2] = 0.0;
                    ++index2;
                    if (num1 == 0)
                        num2 = 0;
                    num3 = 0;
                    ++num1;
                    if (gen.NextDouble() < 0.5)
                    {
                        numArray1[index2] = 0;
                        numArray2[index2] = Math.Round(gen.NextDouble() * 100.0, 2);
                    }
                    else
                    {
                        numArray1[index2] = 2;
                        numArray2[index2] = 0.0;
                    }
                }
                else
                {
                    if (gen.NextDouble() < 0.5)
                    {
                        numArray1[index2] = 0;
                        numArray2[index2] = Math.Round(gen.NextDouble() * 100.0, 2);
                    }
                    else
                    {
                        numArray1[index2] = 2;
                        numArray2[index2] = 0.0;
                    }
                    if (num3 > 1 && num1 > 0)
                    {
                        ++index2;
                        numArray1[index2] = 1;
                        numArray2[index2] = 1.0;
                        --num1;
                    }
                }
                ++num2;
                if (num2 > 100 && num1 > 0)
                {
                    if (index2 + num1 + 5 > numArray2.Length)
                    {
                        numArray2 = Utils.extendArray<double>(numArray2, n / 2);
                        numArray1 = Utils.extendArray<int>(numArray1, n / 2);
                    }
                    numArray1[index2] = 0;
                    numArray2[index2] = 1.0;
                    int index3 = index2 + 1;
                    numArray1[index3] = 1;
                    numArray2[index3] = 7.0;
                    int index4 = index3 + 1;
                    numArray1[index4] = 0;
                    numArray2[index4] = 1.0;
                    index2 = index4 + 1;
                    for (int index5 = 0; index5 < num1; ++index5)
                    {
                        ++index2;
                        numArray1[index2] = 1;
                        numArray2[index2] = 1.0;
                    }
                    num1 = 0;
                }
                index1 = index2 + 1;
                if (index1 + 5 > numArray2.Length)
                {
                    numArray2 = Utils.extendArray<double>(numArray2, n / 2);
                    numArray1 = Utils.extendArray<int>(numArray1, n / 2);
                }
            }
            if (num1 > 0)
            {
                numArray1[index1] = 0;
                numArray2[index1] = 1.0;
                int index2 = index1 + 1;
                numArray1[index2] = 1;
                numArray2[index2] = 7.0;
                int index3 = index2 + 1;
                numArray1[index3] = 0;
                numArray2[index3] = 1.0;
                index1 = index3 + 1;
                for (int index4 = 0; index4 < num1; ++index4)
                {
                    ++index1;
                    numArray1[index1] = 1;
                    numArray2[index1] = 1.0;
                }
            }
            return new Expression(Utils.crunchArray<double>(numArray2, numArray2.Length - index1 - 1), Utils.crunchArray<int>(numArray1, numArray1.Length - index1 - 1));
        }

        public static type randstr<type>(type[] s, Random r)
        {
            int index = (int)Math.Round(r.NextDouble() * (double)(s.Length - 1));
            return s[index];
        }

        public static class commands
        {
            public static void fileBranch()
            {
                string loopArg = PullStart.loopArgs[1];
                if (!(loopArg == "execute"))
                {
                    if (!(loopArg == "execute2"))
                    {
                        if (loopArg == "help")
                            PullStart.commands.printFileHelp();
                        else
                            Console.WriteLine("Unknown argument \"{0}\"", (object)PullStart.loopArgs[1]);
                    }
                    else
                    {
                        Console.WriteLine("Beginning Execution");
                        PullStart.commands.executeFile2(PullStart.loopArgs[2]);
                    }
                }
                else
                {
                    Console.WriteLine("Beginning Execution");
                    PullStart.commands.executeFile();
                }
            }

            public static void outputBranch()
            {
                string loopArg = PullStart.loopArgs[1];
                if (!(loopArg == "set"))
                {
                    if (!(loopArg == "unset"))
                    {
                        if (loopArg == "help")
                            PullStart.commands.printOutputHelp();
                        else
                            Console.WriteLine("Unkown argument \"{0}\"", (object)PullStart.loopArgs[1]);
                    }
                    else
                        PullStart.commands.unsetOutput();
                }
                else
                    PullStart.commands.setNewOutput();
            }

            public static void functionBranch()
            {
                switch (PullStart.loopArgs[1])
                {
                    case "analyse":
                        PullStart.commands.analyseExpression();
                        break;
                    case "benchmark":
                        PullStart.commands.benchmarkExpression();
                        break;
                    case "details":
                        PullStart.commands.printExpressionDetails();
                        break;
                    case "evaluate":
                        PullStart.commands.evaluateExpression();
                        break;
                    case "help":
                        PullStart.commands.printExpressionHelp();
                        break;
                    case "list":
                        PullStart.commands.printAllExpressions();
                        break;
                    case "new":
                        PullStart.commands.newExpression();
                        break;
                    case "print":
                        PullStart.commands.printExpression();
                        break;
                    case "remove":
                        PullStart.commands.removeExpression();
                        break;
                    case "steps":
                        PullStart.commands.evaluateExpressionWithSteps();
                        break;
                    default:
                        Console.WriteLine("Unkown argument \"{0}\"", (object)PullStart.loopArgs[1]);
                        break;
                }
            }

            public static void Debug()
            {
                if (PullStart.loopArgs.Length == 1)
                {
                    if (PullStart.debug)
                        Console.WriteLine("Debug is currently enabled.\n");
                    else
                        Console.WriteLine("Debug is currently disabled.\n");
                }
                else if (PullStart.loopArgs[1] == "true")
                {
                    Console.WriteLine("Debug enabled.\n");
                    PullStart.debug = true;
                }
                else if (PullStart.loopArgs[1] == "false")
                {
                    Console.WriteLine("Debug disabled.\n");
                    PullStart.debug = false;
                }
                else
                    Console.WriteLine("Unkown argument \"{0}\"", (object)PullStart.loopArgs[1]);
            }

            private static void newExpression()
            {
                if (PullStart.loopArgs.Length > 3)
                {
                    try
                    {
                        if (PullStart.loopArgs[2] == "random")
                        {
                            string exp = PullStart.stupidExpression(Convert.ToInt32(PullStart.loopArgs[3]), PullStart.randGen);
                            if (PullStart.debug)
                                Console.WriteLine("Generated expression text: {0}", (object)exp);
                            PullStart.expList.Add(PullStart.ExpressionWithProgress(exp));
                            PullStart.expNameList.Add(PullStart.loopArgs[4]);
                        }
                        else
                        {
                            PullStart.expList.Add(new Expression(PullStart.loopArgs[2]));
                            PullStart.expNameList.Add(PullStart.loopArgs[3]);
                        }
                        Console.WriteLine("New expression created successfully.\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nThere was an error creating the expression. There may be an issue with syntax or the expression entered may not be legal.");
                        if (!PullStart.debug)
                            return;
                        Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", (object)ex.Message, (object)ex.TargetSite, (object)ex.StackTrace);
                    }
                }
                else
                    Console.WriteLine("\nNot enough arguments! Did you forget to name your new expression?\n");
            }

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
            }

            private static void benchmarkExpression()
            {
                try
                {
                    int index = PullStart.expNameList.IndexOf(PullStart.loopArgs[2]);
                    Console.WriteLine("Benchmarking expression \"{0}\"", (object)PullStart.loopArgs[2]);
                    PullStart.expList[index].Benchmark();
                    Console.WriteLine("\n           Expression: {0}", (object)PullStart.expList[index].toString(false, false));
                    Console.WriteLine("       Steps to solve: {0}", (object)PullStart.expList[index].steps);
                    Console.WriteLine("           Single OPS: {0}", (object)PullStart.expList[index].opsSingle);
                    Console.WriteLine("         Threaded OPS: {0}", (object)PullStart.expList[index].opsMutliTotal);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nThere was an error benchmarking the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (!PullStart.debug)
                        return;
                    Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", (object)ex.Message, (object)ex.TargetSite, (object)ex.StackTrace);
                }
            }

            private static void printAllExpressions()
            {
                Console.WriteLine("Currently declared expressions:");
                foreach (string expName in PullStart.expNameList)
                    Console.WriteLine(expName);
                Console.WriteLine("");
            }

            private static void printExpression()
            {
                try
                {
                    int index = PullStart.expNameList.IndexOf(PullStart.loopArgs[2]);
                    Console.WriteLine(PullStart.expList[index].toString(true, false));
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nThere was an error printing the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (!PullStart.debug)
                        return;
                    Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", (object)ex.Message, (object)ex.TargetSite, (object)ex.StackTrace);
                }
            }

            private static void evaluateExpression()
            {
                try
                {
                    Expression exp = PullStart.expList[PullStart.expNameList.IndexOf(PullStart.loopArgs[2])];
                    if (PullStart.loopArgs.Length - 3 != exp.varCount)
                    {
                        Console.WriteLine("\nEither too many or too few variable values were provided. Check your input!");
                    }
                    else
                    {
                        try
                        {
                            double[] numArray = new double[exp.varCount];
                            for (int index = 0; index < PullStart.loopArgs.Length - 3; ++index)
                                numArray[index] = Convert.ToDouble(PullStart.loopArgs[index + 3]);
                            if (exp.varCount == 0)
                            {
                                if (PullStart.debug)
                                    Console.WriteLine("{0} = {2}", (object)PullStart.loopArgs[2], (object)exp.evalWithDebug(numArray, false));
                                else
                                    Console.WriteLine("{0} = {2}", (object)PullStart.loopArgs[2], (object)exp.evaluate(numArray, false));
                            }
                            else if (PullStart.debug)
                                Console.WriteLine("{0}({1}) = {2}", (object)PullStart.loopArgs[2], (object)Utils.arrayToString<double>(numArray), (object)exp.evalWithDebug(numArray, false));
                            else
                                Console.WriteLine("{0}({1}) = {2}", (object)PullStart.loopArgs[2], (object)Utils.arrayToString<double>(numArray), (object)exp.evaluate(numArray, false));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\nThere was an error evaluating the expression. There may be an issue with syntax or the values entered are illegal.");
                            if (PullStart.debug)
                                Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", (object)ex.Message, (object)ex.TargetSite, (object)ex.StackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nThere was an error initializing the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (!PullStart.debug)
                        return;
                    Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", (object)ex.Message, (object)ex.TargetSite, (object)ex.StackTrace);
                }
            }

            private static void evaluateExpressionWithSteps()
            {
                try
                {
                    Expression exp = PullStart.expList[PullStart.expNameList.IndexOf(PullStart.loopArgs[2])];
                    if (PullStart.loopArgs.Length - 3 != exp.varCount)
                    {
                        Console.WriteLine("\nEither too many or too few variable values were provided. Check your input!");
                    }
                    else
                    {
                        try
                        {
                            double[] numArray = new double[exp.varCount];
                            for (int index = 0; index < PullStart.loopArgs.Length - 3; ++index)
                                numArray[index] = Convert.ToDouble(PullStart.loopArgs[index + 3]);
                            if (exp.varCount == 0)
                            {
                                if (PullStart.debug)
                                    Console.WriteLine("{0} = {2}", (object)PullStart.loopArgs[2], (object)exp.evalWithDebug(numArray, true));
                                else
                                    Console.WriteLine("{0} = {2}", (object)PullStart.loopArgs[2], (object)exp.evalWithDebug(numArray, false));
                            }
                            else if (PullStart.debug)
                                Console.WriteLine("{0}({1}) = {2}", (object)PullStart.loopArgs[2], (object)Utils.arrayToString<double>(numArray), (object)exp.evalWithDebug(numArray, true));
                            else
                                Console.WriteLine("{0}({1}) = {2}", (object)PullStart.loopArgs[2], (object)Utils.arrayToString<double>(numArray), (object)exp.evalWithDebug(numArray, false));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\nThere was an error evaluating the expression. There may be an issue with syntax or the values entered are illegal.");
                            if (PullStart.debug)
                                Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", (object)ex.Message, (object)ex.TargetSite, (object)ex.StackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nThere was an error initializing the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (!PullStart.debug)
                        return;
                    Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", (object)ex.Message, (object)ex.TargetSite, (object)ex.StackTrace);
                }
            }

            private static void analyseExpression()
            {
                try
                {
                    int index = PullStart.expNameList.IndexOf(PullStart.loopArgs[2]);
                    PullStart.expList[index].analysis(PullStart.debug);
                    Console.WriteLine("Expression: {0}\nEvaluation steps: {1}\nVariable count: {2}\n", (object)PullStart.expList[index].toString(true, false), (object)PullStart.expList[index].steps, (object)PullStart.expList[index].varCount);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nThere was an error analyzing the expression. The expression may not exist, or a translation error may have occured");
                    if (!PullStart.debug)
                        return;
                    Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", (object)ex.Message, (object)ex.TargetSite, (object)ex.StackTrace);
                }
            }

            private static void printExpressionDetails()
            {
                try
                {
                    int index = PullStart.expNameList.IndexOf(PullStart.loopArgs[2]);
                    Console.WriteLine("Details for expression \"" + PullStart.loopArgs[2] + "\".\n");
                    Expression exp = PullStart.expList[index];
                    Console.WriteLine("         Human readable: " + exp.toString(true, false));
                    Console.WriteLine("         Raw Type array: " + Utils.arrayToString<int>(exp.rawTypes, " , "));
                    Console.WriteLine("         Raw Data array: " + Utils.arrayToString<double>(exp.rawData, " , "));
                    Console.WriteLine("        Instruction set: " + exp.getFormattedInstructionSet());
                    Console.WriteLine("\n         Steps to solve: {0}", (object)PullStart.expList[index].steps);
                    Console.WriteLine("             Single OPS: {0}", (object)PullStart.expList[index].opsSingle);
                    Console.WriteLine("           Threaded OPS: {0}", (object)PullStart.expList[index].opsMutliTotal);
                    Console.WriteLine("\n\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nThere was an error printing the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (!PullStart.debug)
                        return;
                    Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", (object)ex.Message, (object)ex.TargetSite, (object)ex.StackTrace);
                }
            }

            public static void removeExpression()
            {
                try
                {
                    int index = PullStart.expNameList.IndexOf(PullStart.loopArgs[2]);
                    PullStart.expList.RemoveAt(index);
                    PullStart.expNameList.RemoveAt(index);
                    Console.WriteLine("\nExpression removed.\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nThere was an error removing the expression. There may be an issue with syntax or the expression entered may not exist.");
                    if (!PullStart.debug)
                        return;
                    Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", (object)ex.Message, (object)ex.TargetSite, (object)ex.StackTrace);
                }
            }

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
                PullStart.commands.loadData(filePath, Utils.stripExtension(filePath));
            }

            public static void loadData(string filePath, string name)
            {
                List<double[]> numArrayList = new List<double[]>();
                StreamReader streamReader = new StreamReader(filePath);
                while (!streamReader.EndOfStream)
                {
                    string[] strArray = streamReader.ReadLine().Replace(" ", "").Split(',');
                    double[] numArray = new double[strArray.Length];
                    for (int index = 0; index < strArray.Length; ++index)
                        numArray[index] = double.Parse(strArray[index]);
                    numArrayList.Add(numArray);
                }
                PullStart.commands.defineData(name, numArrayList.ToArray());
            }

            public static void loadData(string filePath, string name, int colStart, int colEnd, int rowStart, int rowEnd)
            {
                List<double[]> numArrayList = new List<double[]>();
                int num = 0;
                StreamReader streamReader = new StreamReader(filePath);
                if (rowStart > 0)
                {
                    for (num = 0; num < colStart; num = num + 1 + 1)
                        streamReader.ReadLine();
                }
                for (; !streamReader.EndOfStream && num <= colEnd; ++num)
                {
                    string[] strArray = Utils.trimArray<string>(streamReader.ReadLine().Remove(32).Split(','), colStart, colEnd);
                    double[] numArray = new double[strArray.Length];
                    for (int index = 0; index < strArray.Length; ++index)
                        numArray[index] = double.Parse(strArray[index]);
                    numArrayList.Add(numArray);
                }
                PullStart.commands.defineData(name, numArrayList.ToArray());
            }

            public static void defineData(string name, double[][] set)
            {
                if (PullStart.dataNameList.Contains(name))
                {
                    PullStart.dataList.RemoveAt(PullStart.dataNameList.IndexOf(name));
                    PullStart.dataNameList.Remove(name);
                }
                PullStart.dataNameList.Add(name);
                PullStart.dataList.Add(set);
            }

            public static void processData(string dataName, string expName)
            {
                PullStart.commands.processData(dataName, expName, dataName);
            }

            public static void processData(string dataName, string expName, string outputName)
            {
                PullStart.commands.processData(PullStart.dataList.ElementAt<double[][]>(PullStart.dataNameList.IndexOf(dataName)), PullStart.expList.ElementAt<Expression>(PullStart.expNameList.IndexOf(expName)), outputName);
            }

            private static void processData(double[][] input, Expression func, string outputName)
            {
                double[][] set = new double[input.Length][];
                for (int index = 0; index < input.Length; ++index)
                    set[index] = new double[1]
                    {
            func.evaluate(input[index], false)
                    };
                PullStart.commands.defineData(outputName, set);
            }

            private static void processData(double[][] input, PolyExpression func, string outputName)
            {
                double[][] set = new double[input.Length][];
                for (int index = 0; index < input.Length; ++index)
                    set[index] = func.evaluate(input[index]);
                PullStart.commands.defineData(outputName, set);
            }

            public static void saveData(string path, double[][] data)
            {
                StreamWriter streamWriter = new StreamWriter(path);
                for (int index1 = 0; index1 < data.Length; ++index1)
                {
                    string empty = string.Empty;
                    for (int index2 = 0; index2 < data[index1].Length; ++index2)
                        empty += data[index1][index2].ToString();
                    streamWriter.WriteLine(empty);
                }
                streamWriter.Close();
            }

            public static void saveData(string path, double[][] data, int colStart, int colEnd, int rowStart, int rowEnd)
            {
                StreamWriter streamWriter = new StreamWriter(path);
                for (int index1 = rowStart; index1 <= rowEnd; ++index1)
                {
                    string empty = string.Empty;
                    for (int index2 = colStart; index2 < colEnd; ++index2)
                        empty += data[index1][index2].ToString();
                    streamWriter.WriteLine(empty);
                }
                streamWriter.Close();
            }

            public static void executeFile()
            {
                if (PullStart.debug)
                    Console.WriteLine("Prep for file evaluation");
                StreamReader streamReader1 = new StreamReader(PullStart.loopArgs[2], true);
                string str = Utils.stripExtension(PullStart.loopArgs[2]);
                int num1 = 1;
                if (PullStart.debug)
                    Console.WriteLine("[{0}] {1} Read expression from file", (object)PullStart.loopArgs[1], (object)num1);
                Expression expression = new Expression(streamReader1.ReadLine());
                if (PullStart.debug)
                {
                    ++num1;
                    Console.WriteLine("[{0}] Expression Read as \"{1}\"", (object)PullStart.loopArgs[1], (object)expression.toString(true, false));
                }
                PullStart.expList.Add(expression);
                PullStart.expNameList.Add(str);
                while (!streamReader1.EndOfStream)
                {
                    if (PullStart.debug)
                    {
                        Console.WriteLine("[{0}] {1} Read input file path from expression file", (object)PullStart.loopArgs[1], (object)num1);
                        ++num1;
                    }
                    string path1 = streamReader1.ReadLine().Replace("\"", string.Empty);
                    string path2 = path1 + " output.txt";
                    StreamReader streamReader2 = new StreamReader(path1, true);
                    StreamWriter streamWriter = new StreamWriter(path2, false, Encoding.ASCII);
                    streamWriter.AutoFlush = true;
                    int num2 = 1;
                    while (!streamReader2.EndOfStream)
                    {
                        if (PullStart.debug)
                            Console.WriteLine("[{0}] {1} Getting variables...", (object)path1, (object)num2);
                        string[] strArray = streamReader2.ReadLine().Replace(" ", string.Empty).Split(',');
                        double[] variables = new double[strArray.Length];
                        for (int index = 0; index < strArray.Length; ++index)
                            variables[index] = Convert.ToDouble(strArray[index]);
                        if (PullStart.debug)
                        {
                            Console.WriteLine("[{0}] {1} Evaluating variables...", (object)path1, (object)num2);
                            ++num2;
                        }
                        if (variables.Length != expression.varCount)
                        {
                            streamWriter.WriteLine("Bad input, unmatched var count");
                            if (PullStart.debug)
                                Console.WriteLine("[{0}] {1} Bad variable count!!!", (object)path1, (object)num2);
                        }
                        else if (PullStart.debug)
                            streamWriter.WriteLine("{0}", (object)expression.evalWithDebug(variables, false));
                        else
                            streamWriter.WriteLine("{0}", (object)expression.evaluate(variables, false));
                    }
                    if (PullStart.debug)
                        Console.WriteLine("[{0}] {1} End of File.", (object)path1, (object)num2);
                    streamWriter.Flush();
                    streamWriter.Close();
                    streamReader2.Close();
                }
                streamReader1.Close();
                Console.WriteLine("Execution completed.\n");
            }

            public static void executeFile2(string scriptPath)
            {
                PullStart.commands.executeFile2(scriptPath, true);
            }

            public static void executeFile2(string scriptPath, bool throwErrors)
            {
                List<string> source = new List<string>();
                if (PullStart.debug)
                    Console.WriteLine("Loading Script...");
                StreamReader streamReader = new StreamReader(scriptPath, true);
                while (!streamReader.EndOfStream)
                    source.Add(streamReader.ReadLine());
                if (PullStart.debug)
                    Console.WriteLine("Beginning script execution...");
                for (int index = 0; index < source.Count<string>(); ++index)
                {
                    string[] args = source.ElementAt<string>(index).Split(' ');
                    string str = args[0];
                    if (!(str == "define"))
                    {
                        if (!(str == "load"))
                        {
                            if (!(str == "execute"))
                            {
                                if (!(str == "process"))
                                {
                                    if (str == "save")
                                        PullStart.commands.saveData(args[2], PullStart.dataList.ElementAt<double[][]>(PullStart.dataNameList.IndexOf(args[1])));
                                }
                                else
                                {
                                    switch (args.Length)
                                    {
                                        case 3:
                                            PullStart.commands.processData(args[1], args[2]);
                                            continue;
                                        case 4:
                                            PullStart.commands.processData(args[1], args[2], args[3]);
                                            continue;
                                        default:
                                            if (throwErrors)
                                                throw new FormatException();
                                            continue;
                                    }
                                }
                            }
                            else
                                PullStart.commands.executeFile2(args[1], throwErrors);
                        }
                        else
                        {
                            switch (args.Length)
                            {
                                case 2:
                                    PullStart.commands.loadData(args[1]);
                                    continue;
                                case 3:
                                    PullStart.commands.loadData(args[1], args[2]);
                                    continue;
                                case 7:
                                    PullStart.commands.loadData(args[1], args[2], int.Parse(args[3]), int.Parse(args[4]), int.Parse(args[5]), int.Parse(args[6]));
                                    continue;
                                default:
                                    if (throwErrors)
                                        throw new FormatException();
                                    continue;
                            }
                        }
                    }
                    else
                    {
                        string[] strArray = PullStart.squishArgs(args, 2);
                        PullStart.defineExpression(strArray[1], new Expression(strArray[2]));
                    }
                }
            }

            private static void setNewOutput()
            {
                try
                {
                    if (PullStart.loopArgs.Length < 3)
                        DualOut.Init("log.txt");
                    else
                        DualOut.Init(PullStart.loopArgs[2]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was an unexpected issue in intitializing the file output.");
                    if (PullStart.debug)
                    {
                        Console.WriteLine("Message: {0}", (object)ex.Message);
                        Console.WriteLine("Stacktrace: ", (object)ex.StackTrace);
                    }
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
                catch (Exception ex)
                {
                    Console.WriteLine("There was an unexpected issue in closing the file stream.");
                    if (PullStart.debug)
                    {
                        Console.WriteLine("Message: {0}", (object)ex.Message);
                        Console.WriteLine("Stacktrace: ", (object)ex.StackTrace);
                    }
                    throw;
                }
            }
        }
    }
    internal class ThreadedParser : Expression
    {
        private string input;
        private int methodCode;
        private int parseposition;

        public ThreadedParser(string inputString)
        {
            this.parseposition = -3;
            this.methodCode = 0;
            this.input = inputString;
        }

        public string getJob()
        {
            switch (this.methodCode)
            {
                case -1:
                    return "No current job";
                case 0:
                    return "Parsing";
                default:
                    return "Unkown State";
            }
        }

        public string getStatus()
        {
            switch (this.methodCode)
            {
                case -1:
                    return "No current job";
                case 0:
                    if (this.parseposition == -3)
                        return "The parser has not yet been started.";
                    if (this.parseposition == -2)
                        return "Preprocessing...";
                    if (this.parseposition == -1)
                        return "Postprocessing...";
                    return "Parser at position " + (object)this.parseposition + " of " + (object)this.input.Length + " (" + (object)(this.getProgress() * 100.0) + "%)";
                default:
                    return "Unkown State";
            }
        }

        public double getProgress()
        {
            switch (this.methodCode)
            {
                case -1:
                    return 1.0;
                case 0:
                    if (this.parseposition == -3 || this.parseposition == -2)
                        return 0.0;
                    if (this.parseposition == -1)
                        return 1.0;
                    return Math.Round((double)this.parseposition / (double)this.input.Length, 6);
                default:
                    return 0.0;
            }
        }

        public void parse()
        {
            new Thread(new ThreadStart(this.initString)).Start();
        }

        private void initString()
        {
            this.initString(this.input);
        }

        private new void initString(string RawExpr)
        {
            this.parseposition = -2;
            RawExpr = RawExpr.ToLower();
            RawExpr = Utils.replaceAll(RawExpr, new string[11]
            {
        " ",
        "sin",
        "cos",
        "tan",
        "sec",
        "csc",
        "cot",
        "abs",
        "–",
        "pi",
        "e"
            }, new string[11]
            {
        "",
        "$1",
        "$2",
        "$3",
        "$4",
        "$5",
        "$6",
        "$7",
        "-",
        "3.141592653589793",
        "2.718281828459045"
            });
            RawExpr = this.fixImplicitMultiplication(RawExpr);
            double[] set1 = new double[50];
            int[] set2 = new int[50];
            int length = 0;
            this.input = RawExpr;
            for (int index1 = 0; index1 < RawExpr.Length; ++index1)
            {
                this.parseposition = index1;
                char[] chArray1 = new char[10]
                {
          '(',
          ')',
          '[',
          ']',
          '^',
          '*',
          '/',
          '+',
          '-',
          '$'
                };
                char[] chArray2 = new char[7]
                {
          '1',
          '2',
          '3',
          '4',
          '5',
          '6',
          '7'
                };
                if ((int)RawExpr[index1] == 120)
                {
                    set2[length] = 2;
                    if (index1 >= RawExpr.Length - 1)
                    {
                        set1[length] = 0.0;
                    }
                    else
                    {
                        int num = index1 + 1;
                        int eon = this.findEON(RawExpr, num);
                        if (eon == -1)
                        {
                            set1[length] = 0.0;
                        }
                        else
                        {
                            string str = RawExpr.Substring(num, eon - num + 1);
                            set1[length] = Convert.ToDouble(str);
                            num += str.Length;
                        }
                        index1 = num - 1;
                    }
                    ++length;
                }
                else
                {
                    char ch1 = RawExpr.ElementAt<char>(index1);
                    if (((IEnumerable<char>)chArray1).Contains<char>(ch1))
                    {
                        set2[length] = 1;
                        if ((int)ch1 == 36)
                        {
                            ++index1;
                            char ch2 = RawExpr.ElementAt<char>(index1);
                            for (int index2 = 0; index2 < chArray2.Length; ++index2)
                            {
                                if ((int)ch2 == (int)chArray2[index2])
                                    set1[length] = (double)(index2 + 9);
                            }
                        }
                        else
                        {
                            for (int index2 = 0; index2 < chArray1.Length; ++index2)
                            {
                                if ((int)RawExpr.ElementAt<char>(index1) == (int)((IEnumerable<char>)chArray1).ElementAt<char>(index2))
                                    set1[length] = (double)index2;
                            }
                        }
                        ++length;
                    }
                    else
                    {
                        set2[length] = 0;
                        string str = RawExpr.Substring(index1, this.findEON(RawExpr, index1) - index1 + 1);
                        try
                        {
                            set1[length] = Convert.ToDouble(str);
                        }
                        catch (FormatException ex)
                        {
                            throw;
                        }
                        index1 += str.Length - 1;
                        ++length;
                    }
                }
                if (length == set1.Length - 1)
                {
                    set1 = Utils.extendArray<double>(set1, 500);
                    set2 = Utils.extendArray<int>(set2, 500);
                }
            }
            this.data = new double[length];
            this.type = new int[length];
            for (int index = 0; index < length; ++index)
            {
                this.data[index] = set1[index];
                this.type[index] = set2[index];
            }
            this.parseposition = -1;
            this.clean(0);
            this.methodCode = -1;
        }
    }
    public class PolyExpression
    {
        private Expression[] outputs;
        private int VarCount;

        public int varCount
        {
            get
            {
                return this.VarCount;
            }
        }

        private PolyExpression(string[] expressions)
        {
            this.outputs = new Expression[expressions.Length];
            for (int index = 0; index < expressions.Length; ++index)
            {
                this.outputs[index] = new Expression(expressions[index], false);
                if (this.outputs[index].varCount > this.varCount)
                    this.VarCount = this.outputs[index].varCount;
            }
        }

        public double[] evaluate(double[] vars)
        {
            double[] numArray = new double[this.outputs.Length];
            for (int index = 0; index < this.outputs.Length; ++index)
                numArray[index] = this.outputs[index].evaluate(vars, false);
            return numArray;
        }

        public Expression getFunction(int returnIndex)
        {
            return this.outputs[returnIndex];
        }
    }
}
