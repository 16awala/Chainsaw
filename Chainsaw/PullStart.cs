using System;
using System.Collections.Generic;
using System.IO;
using ChainsawEngine;

namespace PullStart
{
    class PullStart
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) { args = new string[] {""}; }
            switch (args[0].ToLower())
            {
                case "debug":
                    break;

                default:
                    string input = "";
                    string[] loopArgs;
                    List<Expression> expList = new List<Expression>();
                    List<string> expNameList = new List<string>();
                    bool debug = false;
                    while (!input.Equals("quit"))
                    {
                        input = Console.ReadLine().ToLower();
                        loopArgs = input.Split(' ') ;//Split into arguments
                        switch (loopArgs[0])
                        {
                            case "help":
                                Console.WriteLine("Commands:\nQuit: Terminates program.\nExpression: Commands for working with expressions.\n\nUse <command> \"help\" to learn more about a command.\n");
                                break;
                            case "expression":

                                switch (loopArgs[1])
                                {
                                    case "help":
                                        Console.WriteLine("\nHelp for Expressions\n\n");
                                        Console.WriteLine("Benchmark\n    Description: Benchmarks an expression and writes results to the console.\n    Syntax: expression benchmark <name of expression>\n\n     Details: Performs basic tests on an expression using random input data to guage evaluation performance in terms of evaluations per second.\n");
                                        Console.WriteLine("Evaluate\n    Description: Evaluates the expression given variabl data.\n    Syntax: expression evaluate <name of expression> <x0> <x1> <x2> ...\n\n     Details: Each variable xn is declared in the expression given. To evaluate a function you must provide exactly enough arguments to satisfy the expression's variable count.\n");
                                        Console.WriteLine("List\n    Description: Lists expressions that have been created.\n    Syntax: expression list\n\n");
                                        Console.WriteLine("New\n    Description: Creates a new expression\n    Syntax: expression new < expression string > < name >\n\n     Details: Creates an expression based on human-readable standard math notation. One example of a valid expression is \"x+2\". The name you give an expression is used to call it back later for other actions. You may create as many as you want.\n");
                                        Console.WriteLine("Remove\n    Description: Removes a previously created expression\n    Syntax: expression remove <name of expression>\n\n     Details: Deletes an expression. Deleted expressions cannot be recovered.\n");
                                        Console.WriteLine("");
                                        break;
                                    case "benchmark":
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
                                        break;

                                    case "new":
                                            try
                                            {
                                                expList.Add(new Expression(loopArgs[2]));
                                                expNameList.Add(loopArgs[3]);
                                                Console.WriteLine("\nNew expression created successfully.");
                                            }
                                            catch (Exception err)
                                            {
                                                Console.WriteLine("\nThere was an error creating the expression. There may be an issue with syntax or the expression entered may not be legal.");
                                                if (debug) { Console.WriteLine("Message: {0}\nTarget: {1}\nStacktrace: {2}\n", err.Message, err.TargetSite, err.StackTrace); }
                                        }
                                        break;
                                    case "list":
                                        Console.WriteLine("Currently declared expressions:");
                                        foreach (var item in expNameList)
                                        {
                                            Console.WriteLine(item);
                                        }
                                        Console.WriteLine("");
                                        break;
                                    case "remove":
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
                                        break;
                                    case "evaluate":
                                        try
                                        {
                                            Expression E  = expList[expNameList.IndexOf(loopArgs[2])];
                                            if (loopArgs.Length-3 != E.varCount)
                                            {
                                                Console.WriteLine("\nEither too many or too few variable values were provided. Check your input!");
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    double[] Einputs = new double[E.varCount];
                                                    for (int i = 0; i < loopArgs.Length-3; i++)
                                                    {
                                                        Einputs[i] = Convert.ToDouble(loopArgs[i + 3]);
                                                    }
                                                    Console.WriteLine("{0}({1}) = {2}",loopArgs[2],Utils.arrayToString(Einputs),E.evaluate(Einputs));
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
                                        break;
                                    default:
                                        Console.WriteLine("Unkown argument \"{0}\"", loopArgs[1]);
                                        break;
                                }
                                break;
                            case "debug":
                                if (loopArgs.Length == 1)
                                {
                                    if (debug)
                                    {
                                        Console.WriteLine("Debug is currently enabled.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Debug is currently disabled.");
                                    }
                                }
                                else
                                {
                                    if (loopArgs[1] == "true")
                                    {
                                        Console.WriteLine("Debug enabled.");
                                        debug = true;
                                    }
                                    else
                                    {
                                        if(loopArgs[1] == "false")
                                        {
                                            Console.WriteLine("Debug disabled.");
                                            debug = false;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Unkown argument \"{0}\"", loopArgs[1]);
                                        }
                                    }
                                }
                                break;


                            default:
                                Console.WriteLine("Unkown argument \"{0}\"", loopArgs[0]);
                                break;
                        }
                    }
                    break;
            }
        }
        static class Parsers
        {
            public static string AssemblyDirectory
            {
                get
                {
                    string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                    UriBuilder uri = new UriBuilder(codeBase);
                    string thispath = Uri.UnescapeDataString(uri.Path);
                    return Path.GetDirectoryName(thispath);
                }
            }
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
                retval += randstr(mathops,gen);//Add operator
                rand = gen.NextDouble();//Add something else
                if (rand < 0.4)
                {
                    retval += Convert.ToString(Math.Round((gen.NextDouble() * 100), 2));//constant
                }
                else
                {
                    if(rand < 0.65)//var
                    {
                        retval += "x";
                    }
                    else
                    {
                        if (rand < 0.8)//oho
                        {
                            retval += randstr(onehandedops,gen);
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
            int[] t = new int[n*2];//Start with x
            double[] d = new double[n*2];
            double[] mathops = new double[] { 4, 5, 6, 7, 8 };
            double[] onehandedops = new double[] { 9,10,11,12,13,14,15 };
            t[0] = 2;
            d[0] = 0;
            int p = 1;
            double progint = n / 100;
            double prog = progint;
            while(p<n)
            {
                if (p > prog)
                {
                    Console.WriteLine("Generating: {0}", Math.Round((double)(p) / (double)(n), 4)*100);
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
                                if(openparens == 0) { distfromstartparen = 0; }
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
                    if (p + openparens+5 > d.Length)
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
                    d = Utils.extendArray(d, n/2);
                    t = Utils.extendArray(t, n/2);
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
        public static type randstr<type>(type[] s,Random r)
        {
            int AGH = (Int32)(Math.Round((r.NextDouble() * (s.Length-1))));
            return s[AGH];
        }
    }
}
