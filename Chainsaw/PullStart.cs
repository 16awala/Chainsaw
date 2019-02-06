using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
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
            }//LEARN HOW THIS WORKS
        }
        public class coordinate
        {
            private int x_;
            private int y_;
            public int X
            {
                get
                {
                    return x_;
                }
                set
                {
                    x_ = value;
                }
            }
            public int Y
            {
                get
                {
                    return y_;
                }
                set
                {
                    y_ = value;
                }
            }
            public coordinate()
            {
                X = 0;
                Y = 0;
            }
            public coordinate(int x, int y)
            {
                x_ = x;
                y_ = y;
            }

        }
        public class graphData
        {
            public Int32 resolutionX;
            public Int32 resolutionY;
            public double minimumX;
            public double maximumX;
            public double minimumY;
            public double maximumY;
            public double rangeX;
            public double rangeY;
            public DataSet2D data;
            public graphData(DataSet2D dataPoints, Int32 bitmapWidth, Int32 bitmapHeight, double lowerBound, double upperBound)
            {
                resolutionX = bitmapWidth;
                resolutionY = bitmapHeight;
                minimumX = dataPoints.x.Min();
                maximumX = dataPoints.x.Max();
                minimumY = lowerBound;
                maximumY = upperBound;
                rangeX = maximumX - minimumX;
                rangeY = maximumY - minimumY;
                data = dataPoints;
            }
        }
        public static coordinate DataToCoordinate(int dataPoint, graphData meta)
        {
            return DataToCoordinate(meta.data.x[dataPoint],meta.data.y[dataPoint],meta);
        }
        public static coordinate DataToCoordinate(double x, double y, graphData meta)
        {
            //A particularly nasty looking pile of conversions
            //Resolution: Number of pixels in row / column
            //Min: Double value mapped to pixel 0
            //Max: Double value mapped to the highest pixel coordinate
            coordinate retval = new coordinate();
            retval.X = (Int32)(Math.Round((double)(meta.resolutionX) * ((x + Math.Abs(meta.minimumX)) / meta.rangeX)));
            retval.Y = (Int32)(meta.resolutionY - Math.Round((double)meta.resolutionY * ((y + Math.Abs(meta.minimumY)) / meta.rangeY)));
            return retval;
        }
        public static Bitmap DrawMeAGraph(graphData gData, Color lineColor, int lineSize, double tickmarkIntervals, bool connected)
        {
            //TODO - Document this shit, it's about as readable as alphabet soup that's been through a blender
            Bitmap b = new Bitmap(gData.resolutionX, gData.resolutionY);
            coordinate origin = DataToCoordinate(0, 0, gData);

            coordinate c1;
            if (connected)
            {
                coordinate c0 = DataToCoordinate(0, gData);
                for (int i = 1; i < gData.data.points-1; i++)
                {
                    c1 = DataToCoordinate(i, gData);
                    b = DrawMeALine(b, c0,c1, lineSize, lineColor);
                    c0 = c1;
                }
            }
            else
            {
                for (int i = 0; i < gData.data.points; i++)
                {
                    if (gData.data.y[i] >= gData.minimumY && gData.data.y[i] <= gData.maximumY)
                    {
                        c1 = DataToCoordinate(i, gData);
                        b.SetPixel(c1.X,c1.Y,lineColor);
                    }
                }
            }
            //TODO Add axis drawing
            return b;
        }
        public static Bitmap DrawMeALine(Bitmap bmp, coordinate c1, coordinate c2, int brushSize, Color brushColor)
        {
            return DrawMeALine(bmp, c1.X, c1.Y, c2.X, c2.Y, brushSize, brushColor);
        }
        public static Bitmap DrawMeALine(Bitmap bmp, int x1, int y1, int x2, int y2, int brushSize, Color brushColor)
        {
            brushSize = bound(brushSize, 1, 1000);
            bool walkX = (Math.Abs(x1 - x2) > Math.Abs(y1 - y2));
            //INPR needed a rewrite cause I was an idiot and wrote it accidentally such that the coordinates given defined a rectangle in which to draw a line from top left to bottom right instead of an actual line as intended

            if (walkX)
            {
                if (x1 > x2)//We want to go from small X to large X, so swap the coordinates if necessary
                {
                    int tmp = x1;
                    x1 = x2;
                    x2 = tmp;
                    tmp = y1;
                    y1 = y2;
                    y2 = tmp;
                }
            }
            else
            {
                if (y1 > y2)//We want to go from small Y to large XY, so swap the coordinates if necessary
                {
                    int tmp = x1;
                    x1 = x2;
                    x2 = tmp;
                    tmp = y1;
                    y1 = y2;
                    y2 = tmp;
                }
            }
            //Round out end points
            bmp = drawMeACircle(bmp, x1, y1, brushSize, brushColor);
            bmp = drawMeACircle(bmp, x2, y2, brushSize, brushColor);
            int x;
            int y;
            double m;
            int b;
            if (walkX)
            {//Walk along x dimension
                //Get line equation for coordinate grid
                m = ((double)(x2-x1))/ ((y2 - y1));
                b = (int)Math.Round(y1 - m * x1, 0);
                for ( x = x1; x < x2; x++)
                {
                    bmp = drawMeACross(bmp, x, (int)Math.Round((m * x) + b,0), brushSize, brushColor);
                }
            }
            else
            {//Walk along y dimension
                //Get line equation for coordinate grid
                m = ((double)(y2 - y1)) / ((x2 - x1));//Adjusted slope formula
                b = (int)Math.Round(y1 - m * x1, 0);
                for (y = y1; y < y2; y++)
                {
                    bmp = drawMeACross(bmp, (int)Math.Round((y - b) / m, 0),y , brushSize, brushColor);
                }
            }
            return bmp;
        }

        public static Bitmap drawMeARectangle(Bitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            int temp;
            if(x1 > x2)
            {
                temp = x2;
                x2 = x1;
                x1 = temp;
            }
            if (y1 > y2)
            {
                temp = y2;
                y2 = y1;
                y1 = temp;
            }
            for (int i = x1; i < x2; i++)
            {
                for (int j = y1; j < y2; j++)
                {
                    if (isInBound(i,0,bmp.Width) && isInBound(j,0,bmp.Height))
                    {
                        
                        bmp.SetPixel(i, j, color);
                    }
                }
            }
            return bmp;
        }

        public static Bitmap drawMeACircle(Bitmap bmp, int x, int y, int radius, Color color)
        {
            if(radius == 1)
            {
                if (isInBound(x,0,bmp.Width-1) && isInBound(y,0,bmp.Height-1))
                {
                    bmp.SetPixel(x, y, color);
                }
            }
            else
            {
                if(radius == 2)
                {
                    return drawMeACross(bmp, x, y, 2, color);
                }
                else
                {
                    double xd = x;
                    double yd = y;
                    for (int i = x - radius; i < x + radius; i++)
                    {
                        for (int j = y - radius; j < y + radius; j++)
                        {
                            if (Math.Sqrt((Math.Pow(x - i, 2)) + (Math.Pow(y - j, 2))) < radius)//If within radius of circle
                            {
                                if (isInBound(i, 0, bmp.Width - 1) && isInBound(j, 0, bmp.Height - 1))
                                {
                                    bmp.SetPixel(i, j, color);
                                }
                            }
                        }
                    }
                }
            }
            
            return bmp;
        }

        public static Bitmap drawMeACross(Bitmap bmp,int x, int y, int size, Color color)
        {
            if (size <= 1)
            {
                if (isInBound(x, 0, bmp.Width - 1) && isInBound(y, 0, bmp.Height - 1))
                {
                    bmp.SetPixel(x, y, color);
                }
            }
            else
            {
                int XuB = bmp.Width - 1;
                int YuB = bmp.Height - 1;
                int offset = bound(size / 2, 1, 1000);
                for (int i = 0; i < size; i++)
                {
                    if (isInBound(x - offset + i, 0, XuB) && isInBound(y, 0, YuB))
                    {
                        bmp.SetPixel(x - offset + i, y, color);
                    }
                    if (isInBound(x, 0, XuB) && isInBound(y - offset + i, 0, YuB))
                    {
                        bmp.SetPixel(x, y - offset + i, color);
                    }
                }
            }
            return bmp;
        }
        public static Int32 bound(Int32 number, Int32 lower, Int32 upper)
        {
            if (number > upper)
            {
                return upper;
            }
            if (number < lower)
            {
                return lower;
            }
            return number;
        }
        public static bool isInBound(int number, int lower, int upper)
        {
            if (number < lower || number > upper)
            {
                return false;
            }
            else
            {
                return true;
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
