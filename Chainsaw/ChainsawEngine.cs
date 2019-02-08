using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

namespace ChainsawEngine
{
    internal static class Utils
    {
        public static string AssemblyDirectory
        {
            get
            {
                return Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
            }
        }

        public static string arrayToString<type>(type[] set)
        {
            return Utils.arrayToString<type>(set, " , ");
        }

        public static string arrayToString<type>(type[] set, string separator)
        {
            string str = Convert.ToString((object)set[0]);
            for (int index = 1; index < set.Length; ++index)
                str = str + separator + Convert.ToString((object)set[index]);
            return str;
        }

        public static type[] extendArray<type>(type[] set, int indexes)
        {
            type[] typeArray = new type[set.Length + indexes];
            set.CopyTo((Array)typeArray, 0);
            return typeArray;
        }

        public static type[] trimArray<type>(type[] set, int start, int end)
        {
            int index1 = 0;
            type[] typeArray = new type[end - start];
            for (int index2 = start; index2 <= end; ++index2)
                typeArray[index1] = set[index2];
            return typeArray;
        }

        public static type[] removeArrayElement<type>(type[] set, int index)
        {
            int index1 = 0;
            type[] typeArray = new type[set.Length - 1];
            for (int index2 = 0; index2 < set.Length; ++index2)
            {
                if (index2 == index)
                {
                    ++index2;
                    if (index2 != set.Length)
                    {
                        typeArray[index1] = set[index2];
                        ++index1;
                    }
                }
                else
                {
                    typeArray[index1] = set[index2];
                    ++index1;
                }
            }
            return typeArray;
        }

        public static type[] insertArrayElement<type>(type[] set, int index, type value)
        {
            type[] typeArray = new type[set.Length + 1];
            int index1 = 0;
            if (index == set.Length)
            {
                set.CopyTo((Array)typeArray, 0);
                typeArray[index] = value;
            }
            else
            {
                for (int index2 = 0; index2 < set.Length; ++index2)
                {
                    if (index2 == index)
                    {
                        typeArray[index1] = value;
                        ++index1;
                        typeArray[index1] = set[index2];
                    }
                    else
                        typeArray[index1] = set[index2];
                    ++index1;
                }
            }
            return typeArray;
        }

        public static type[] crunchArray<type>(type[] array, int items)
        {
            type[] typeArray = new type[array.Length - items];
            for (int index = 0; index < typeArray.Length; ++index)
                typeArray[index] = array[index];
            return typeArray;
        }

        public static string replaceAll(string raw, string[] match, string[] replace)
        {
            if (match.Length != replace.Length)
                throw new arrayMismatchException("Match and replace arrays must be of equal length.");
            for (int index = 0; index < match.Length; ++index)
                raw = raw.Replace(match[index], replace[index]);
            return raw;
        }

        public static string stripExtension(string str)
        {
            for (int startIndex = str.Length - 1; startIndex > 0; --startIndex)
            {
                if ((int)str[startIndex] == 46)
                    return str.Remove(startIndex);
            }
            throw new FormatException("The given string does not contain an extension");
        }

        public static DataSet2D Data2DFromExpression(Expression e, double xLowerbound, double xUpperBound)
        {
            return Utils.Data2DFromExpression(e, xLowerbound, xUpperBound, 100);
        }

        public static DataSet2D Data2DFromExpression(Expression e, double xLowerbound, double xUpperBound, int xResolution)
        {
            if (e.varCount > 1)
                throw new ArgumentException("Cannot produce a 2D set from a multi variable expression");
            double num1 = (xUpperBound - xLowerbound) / (double)xResolution;
            DataSet2D dataSet2D = new DataSet2D(new double[xResolution], new double[xResolution]);
            for (int index = 0; index < xResolution; ++index)
            {
                double num2 = xLowerbound + (double)index * num1;
                dataSet2D.x[index] = num2;
                dataSet2D.y[index] = e.evaluate(new double[1]
                {
          num2
                }, false);
            }
            return dataSet2D;
        }
    }

    public class DataSet2D
    {
        public double[] x;
        public double[] y;
        public int points
        {
            get
            {
                return x.Length;
            }
        }
        public DataSet2D(double[] xSet, double[] ySet)
        {
            if (xSet.Length == ySet.Length)
            {
                x = xSet;
                y = ySet;
            }
            else
            {
                throw new arrayMismatchException("Data sets must match in length!");
            }
        }

        public string toString()
        {
            string str = "";
            for (int i = 0; i < points; i++)
            {
                str = str + "[" + Convert.ToString(x[i]) + "," + Convert.ToString(y[i]) + "]";
            }
            return str;
        }
    }
    public class DataSet3D
    {
        public double[] x;
        public double[] y;
        public double[] z;
        public int points
        {
            get
            {
                return x.Length * y.Length;
            }
        }
        public DataSet3D(double[] xSet, double[] ySet, double[] zSet)
        {
            if (xSet.Length == ySet.Length && xSet.Length == zSet.Length)
            {
                x = xSet;
                y = ySet;
                z = zSet;
            }
            else
            {
                throw new arrayMismatchException("Data sets must match in length!");
            }
        }

        public string toString()
        {
            string str = "";
            for (int i = 0; i < points; i++)
            {
                str = str + "[" + Convert.ToString(x[i]) + "," + Convert.ToString(y[i]) + "," + Convert.ToString(z[i]) + "]";
            }
            return str;
        }
    }

    public class Expression
    {

        /* Note: due to an unfortunate mishap, all of this code has been decompiled from an exe.
         * Because of pre-compiler operations, much of the code is not very readable, and comments have been stripped.
         * Some of the code has been reformatted and documented, but beware.
         * /

        /*Instructions for custom operations
          Custom functions are processed at the same time as trigonometric functions (after parenthesis, before exponents)
          They are evaluated by being applied to the item to their immediate right, be that a constant, a variable, or a parenthesis group.
            
          INSTRUCTIONS FOR ADDING A CUSTOM FUNCTION
        
          1. Add its string equivalent to the end of operatorLookup
          2. In the expression constructor that uses a string, add a rule for replacing occurences with a '$' followed by any unused character.
                  For example, "cos" is replaced with "$b"
          3. In the same method, add the character you chose to the specops array.
          4. In the switch statement at the bottom of the evaluate method, add a case option with the index of your function (probably just the next number after what is already there).
                  This is where the code for the custom function goes. The number being evaluated by the custom function is represented by tempData[opTape[pos + 2]]
                  Once your function is done evaluating, the result should be stored in the same variable tempData[opTape[pos + 2]]
        */
        //BUG: giving an expression containing only a constant or a variable (or containing at any point a set of parenthesis containing only a constant or a variable) will crash the program

        //Data Structure and properties
        private string[] operatorLookup = new string[17]  {"(",")","|","|","^","*","/","+","-","sin","cos","tan","sec","csc","cot","abs","sqrt"};//for string output
        private bool tapeEmpty = true;
        private int singleThreadedOPS = -1;//Operations per second as obtained by benchmarking in a single thread
        private int multiThreadedTotalOPS = -1;//Operations per second as obtained by benchmarking on all threads
        private int multiThreadedIndividualOPS;// (multiThreadedTotalOPS / cpucores)
        protected double[] data;
        protected int[] type;
        private int[] opTape;//Instructions for evaluating the expression
        private int numVariables;//Number of unique variables the expression contains.
        private bool trimVars;

        public int steps //Number of steps in evaulating the function sequentially
        {
            get
            {
                if (this.opTape == null)
                    this.analysis(false);
                return this.opTape.Length / 3;
            }
        }

        public int opsSingle
        {
            get
            {
                return this.singleThreadedOPS;
            }
        }

        public int opsMutliTotal
        {
            get
            {
                return this.multiThreadedTotalOPS;
            }
        }

        public int opsMultiIndividual
        {
            get
            {
                return this.multiThreadedIndividualOPS;
            }
        }

        public int varCount
        {
            get
            {
                return this.numVariables;
            }
        }

        public int[] rawTypes
        {
            get
            {
                return this.type;
            }
        }

        public double[] rawData
        {
            get
            {
                return this.data;
            }
        }

        public int[] instructionTape
        {
            get
            {
                return this.opTape;
            }
        }

        public Expression(double[] Values, int[] Types) //Constructor using raw data. Can be used to copy or tweak expressions. Does not call clean().
        {
            this.initArrays(Values, Types);
            this.trimVars = true;
        }

        public Expression(double[] Values, int[] Types, bool trimVariables)
        {
            this.initArrays(Values, Types);
            this.trimVars = trimVariables;
        }

        private void initArrays(double[] Values, int[] Types)
        {
            if (Values.Length != Types.Length)
                throw new arrayMismatchException("'Values' and 'Types' must be equal in length.");
            this.data = Values;
            this.type = Types;
            this.updateVarCount();
        }

        public Expression()
        {
            this.trimVars = true;
        }

        public Expression(string RawExpr)//Attempts to parse the string into an expression. Absolute value signs must be entered with square brackets. Calls clean().
        {
            this.trimVars = true;
            this.initString(RawExpr);
        }

        public Expression(string RawExpr, bool trimVariables)
        {
            this.trimVars = trimVariables;
            this.initString(RawExpr);
        }

        protected void initString(string RawExpr)
        {
            RawExpr = RawExpr.ToLower();
            RawExpr = Utils.replaceAll(RawExpr, new string[] { " ","sin","cos","tan","sec","csc","cot","abs","sqrt","–","pi","e"}, new string[] {"","$1","$2","$3","$4","$5","$6","$7","$8","-","3.141592653589793","2.718281828459045"});
            char[] operators = new char[] {'(',')', '[',']','^','*','/','+','-','$'};//List of special characters that interrupt numbers
            char[] SpecOps = new char[] {'1','2','3','4','5','6','7','8'};

            RawExpr = this.fixImplicitMultiplication(RawExpr);
            double[] tempdata = new double[50];
            int[] temptype = new int[50];
            int tmpi = 0;
            for (int i = 0; i < RawExpr.Length; ++i)
            {
                if (RawExpr[i] == 'x')//Found a variable
                {
                    temptype[tmpi] = 2;
                    if (i >= RawExpr.Length - 1)//If this is the end of the string there is no sub
                    {
                        tempdata[tmpi] = 0.0;
                    }
                    else
                    {
                        i++;
                        int eon = this.findEON(RawExpr, i);
                        if (eon == -1)
                        {
                            tempdata[tmpi] = 0.0;
                        }
                        else
                        {
                            string str = RawExpr.Substring(i, eon - i + 1);
                            tempdata[tmpi] = Convert.ToDouble(str);
                            i += str.Length;
                        }
                        i--;
                    }
                    ++tmpi;
                }
                else
                {
                    char c = RawExpr.ElementAt<char>(i);
                    if (((IEnumerable<char>)operators).Contains<char>(c))
                    {
                        temptype[tmpi] = 1;
                        if (c == '$')
                        {
                            ++i;
                            c = RawExpr.ElementAt<char>(i);
                            for (int j = 0; j < SpecOps.Length; ++j)
                            {
                                if (c == SpecOps[j])
                                    tempdata[tmpi] = (j + 9);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < operators.Length; ++j)
                            {
                                if (RawExpr.ElementAt(i) == ((IEnumerable<char>)operators).ElementAt(j))
                                    tempdata[tmpi] = j;
                            }
                        }
                        tmpi++;
                    }
                    else
                    {//Must be a number
                        temptype[tmpi] = 0;
                        string str = RawExpr.Substring(i, this.findEON(RawExpr, i) - i + 1);
                        try
                        {
                            tempdata[tmpi] = Convert.ToDouble(str);
                        }
                        catch (FormatException ex)
                        {
                            throw;
                        }
                        i += str.Length - 1;
                        tmpi++;
                    }
                }
                if (tmpi == tempdata.Length - 1)
                {
                    tempdata = Utils.extendArray(tempdata, 500);
                    temptype = Utils.extendArray(temptype, 500);
                }
            }
            this.data = new double[tmpi];
            this.type = new int[tmpi];
            for (int index = 0; index < tmpi; ++index)
            {
                this.data[index] = tempdata[index];
                this.type[index] = temptype[index];
            }
            this.clean(0);
        }

        protected string fixImplicitMultiplication(string Raw)
        {
            string str = Raw;
            char[] chArray = new char[] {'0','1','2','3','4','5','6','7','8','9'};
            for (int i = 1; i < str.Length; ++i)
            {
                if (i < 2)
                {
                    if (i == 1 && ((str[i] == '(' || str[i] == 'x') && (chArray.Contains(str[i - 1]) || chArray.Contains(str[i]) && str[i - 1] == ')')))
                        str = str.Substring(0, i) + "*" + str.Substring(i, str.Length - i);
                }
                else if ((str[i] == '(' || str[i] == 'x') && (chArray.Contains(str[i - 1]) && str[i - 2] != '$') || (chArray).Contains(str[i]) && str[i - 1] == ')')
                    str = str.Substring(0, i) + "*" + str.Substring(i, str.Length - i);
            }
            return str;
        }

        public string toString(bool condenseMultiplication = true, bool addSpaces = false)
        {
            return this.toStr(condenseMultiplication, addSpaces);
        }

        public string getFormattedInstructionSet()
        {
            string str = "{ ";
            if (this.tapeEmpty)
                this.analysis(false);
            int index = 0;
            while (index < this.opTape.Length)
            {
                str = str + "( " + opTape[index] + "," + opTape[index + 1] + "," + opTape[index + 2] + " )";
                index += 3;
            }
            return str + " }";
        }

        private string toStr(bool compact, bool padterms)
        {
            string str = "";
            for (int index = 0; index < this.type.Length; ++index)
            {
                if (padterms)
                    str += " ";
                switch (type[index])
                {
                    case 0:
                        str += Convert.ToString(data[index]);
                        break;
                    case 1:
                        if (data[index] < 0.0 || data[index] > operatorLookup.Length - 1)
                            throw new ArgumentOutOfRangeException("data[" + Convert.ToString(index) + "]", data[index], "Unkown operator");
                        if (!compact)
                        {
                            str += operatorLookup[Convert.ToInt32(data[index])];
                            break;
                        }
                        if (this.data[index] != 5.0)
                            str += operatorLookup[Convert.ToInt32(data[index])];
                        else if ((index != 1 || type[index + 1] == 0) && (type[index - 1] != 0 || type[index + 1] != 2))
                            str += operatorLookup[Convert.ToInt32(data[index])];
                        break;
                    case 2:
                        str = data[index] != 0.0 ? str + "x" + Convert.ToString(data[index]) : str + "x";
                        break;
                    default:
                        str += "?";
                        break;
                }
            }
            return str;
        }

        private void trimVariableSubs()
        {
            bool[] flagArray = new bool[numVariables];
            double[] numArray = new double[numVariables];
            for (int index = 0; index < numVariables; ++index)
                numArray[index] = index;
            for (int index = 0; index < type.Length; ++index)
            {
                if (type[index] == 2)
                    flagArray[Convert.ToInt32(data[index])] = true;
            }
            int num = numVariables + 1;
            bool flag = true;
            for (int index = 0; index < numVariables; ++index)
            {
                if (flag)
                {
                    if (!flagArray[index])
                    {
                        flag = false;
                        if (index < num)
                            num = index;
                    }
                }
                else if (flagArray[index])
                {
                    numArray[index] = num;
                    ++num;
                }
            }
            for (int index = 0; index < type.Length; ++index)
            {
                if (type[index] == 2)
                    data[index] = numArray[Convert.ToInt32(data[index])];
            }
            this.updateVarCount();
        }

        public void clean(int operation = 0)
        {
            switch (operation)
            {
                case 0:
                    this.clean(1);
                    this.clean(3);
                    break;
                case 1:
                    this.fixDecimalSubs();
                    this.trimVariableSubs();
                    break;
                case 2:
                    bool flag = true;
                    while (flag)
                    {
                        flag = false;
                        for (int index = 0; index < type.Length; ++index)
                        {
                            if (type[index] == 1 && ((data[index] == 0.0 || data[index] == 2.0) && findClosing(index) == index + 2))
                            {
                                type = Utils.removeArrayElement(type, index + 2);
                                type = Utils.removeArrayElement(type, index);
                                data = Utils.removeArrayElement(data, index + 2);
                                data = Utils.removeArrayElement(data, index);
                                flag = true;
                            }
                        }
                    }
                    break;
                case 3:
                    int num = -1;
                    for (int index1 = 0; index1 < this.type.Length; ++index1)
                    {
                        if (num == 1 && this.type[index1] == 1 && this.data[index1] == 8.0)
                        {
                            if (this.type[index1 + 1] == 0)
                            {
                                this.data[index1 + 1] = this.data[index1 + 1] * -1.0;
                                this.type = Utils.removeArrayElement<int>(this.type, index1);
                                this.data = Utils.removeArrayElement<double>(this.data, index1);
                            }
                            else if (this.type[index1 + 1] == 2)
                            {
                                this.data[index1] = 5.0;
                                this.type = Utils.insertArrayElement<int>(this.type, index1, 1);
                                this.data = Utils.insertArrayElement<double>(this.data, index1, 0.0);
                                int index2 = index1 + 1;
                                this.type = Utils.insertArrayElement<int>(this.type, index2, 0);
                                this.data = Utils.insertArrayElement<double>(this.data, index2, -1.0);
                                int index3 = index2 + 3;
                                this.type = Utils.insertArrayElement<int>(this.type, index3, 1);
                                this.data = Utils.insertArrayElement<double>(this.data, index3, 1.0);
                                index1 = index3 + 1;
                            }
                        }
                        if (index1 < this.type.Length)
                            num = this.type[index1] != 1 || this.data[index1] <= 3.0 ? -1 : 1;
                    }
                    break;
            }
        }

        private int findClosing(int parent)
        {
            if (this.type[parent] != 1 || this.data[parent] >= 4.0 || this.data[parent] < 0.0)
                return -1;
            int index = parent;
            int num1;
            int num2;
            if (this.data[parent] == 0.0 || this.data[parent] == 2.0)
            {
                num1 = 1;
                num2 = 1;
            }
            else
            {
                num1 = -1;
                num2 = -1;
            }
            while ((uint)num2 > 0U)
            {
                index += num1;
                if (this.type[index] == 1)
                {
                    if (this.data[index] == 0.0 || this.data[index] == 2.0)
                        ++num2;
                    else if (this.data[index] == 1.0 || this.data[index] == 3.0)
                        --num2;
                }
            }
            return index;
        }

        private int findClosing(int[] typearray, double[] dataarray, int parent)
        {
            if (typearray[parent] != 1 || dataarray[parent] >= 4.0 || dataarray[parent] < 0.0)
                return -1;
            int index = parent;
            int num1;
            int num2;
            if (dataarray[parent] == 0.0 || dataarray[parent] == 2.0)
            {
                num1 = 1;
                num2 = 1;
            }
            else
            {
                num1 = -1;
                num2 = -1;
            }
            while ((uint)num2 > 0U)
            {
                index += num1;
                if (typearray[index] == 1)
                {
                    if (dataarray[index] == 0.0 || dataarray[index] == 2.0)
                        ++num2;
                    else if (dataarray[index] == 1.0 || dataarray[index] == 3.0)
                        --num2;
                }
            }
            return index;
        }

        private int findEOT(int start)
        {
            int parent = start;
            bool flag = true;
            while (flag)
            {
                if (parent + 1 >= this.type.Length)
                    flag = false;
                else if (this.type[parent] == 0 || this.type[parent] == 2)
                    ++parent;
                else if (this.data[parent] == 0.0 || this.data[parent] == 2.0)
                    parent = this.findClosing(parent) + 1;
                else if (this.data[parent] == 7.0 || this.data[parent] == 8.0)
                    flag = false;
                else
                    ++parent;
            }
            return parent - 1;
        }

        public int findEON(string str, int c)
        {
            int index = c;
            char[] chArray = new char[10]
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
        'x'
            };
            bool flag = true;
            while (flag)
            {
                if (index == str.Length - 1)
                {
                    if (((IEnumerable<char>)chArray).Contains<char>(str[index]))
                        --index;
                    flag = false;
                }
                else if (((IEnumerable<char>)chArray).Contains<char>(str[index]))
                {
                    flag = false;
                    --index;
                }
                else
                    ++index;
            }
            if (index < c)
                return -1;
            return index;
        }

        private void fixDecimalSubs()
        {
            for (int index = 0; index < this.type.Length; ++index)
            {
                if (this.type[index] == 2)
                    this.data[index] = Math.Truncate(this.data[index]);
            }
            this.updateVarCount();
        }

        private void updateVarCount()
        {
            double num = -1.0;
            for (int index = 0; index < this.type.Length; ++index)
            {
                if (this.type[index] == 2 && this.data[index] > num)
                    num = this.data[index];
            }
            this.numVariables = Convert.ToInt32(num + 1.0);
        }

        public int findConstant(int[] typeData, int start, int interval)
        {
            bool flag = true;
            int index = start;
            while (flag)
            {
                if (typeData[index] == 0)
                    flag = false;
                else
                    index += interval;
            }
            return index;
        }

        public int[] analysis(bool verbose = false)
        {
            this.opTape = this.analyse(this.data, this.type, 0, verbose);
            this.tapeEmpty = false;
            return this.opTape;
        }

        private int[] analyse(double[] Dataset, int[] Typeset, int stacklevel, bool verbose = false)
        {
            /*This algorithm performs several passes over the entirety of the function with the intent to determine
             * an acceptable order for evaluating the expression.
             * The order of operations used is as follows:
             * 1. Parenthesis (when encountered, the group contained by the parenthesis is passed into this function to be evaluated separately
             * 2. Custom functions (all built in functions, as they only take one value to operate on.
             * 3. Exponents
             * 4. Multiplication/Division. There is no prioritization here, first come first serve left to right.
             * 5. Addition / subtraction. There is no prioritization here, first come first serve left to right.
             * 
             * This could all be done each time the expression is called, however we are not expecting the expression to change,
             * meaning the order in which everything happens will be the same every time. Evaluating an expression once the order of evaluation
             * is known for it can take around 1/10th of the time it takes to determine that order, so with a very small amount of up-front computation
             * evaluating massive or numerous sets of data is drastically reduced in CPU cost.
             * 
             * Analysis is stored in a tape-like fashion with each necessary operation taking up 3 digits: lefthand index , operator , righthand index
             * This means the evaluation function needs no understanding of the desired order of operations, it simply reads the tape
             * from left to right (index 0 upwards). The tape serves as a step-by-step how-to guide on evaluating the function.
            */
            double[] dataarray = new double[Dataset.Length];
            int[] numArray1 = new int[Typeset.Length];
            Dataset.CopyTo((Array)dataarray, 0);
            Typeset.CopyTo((Array)numArray1, 0);
            int[] numArray2 = new int[900];
            int index1 = 0;
            if (verbose)
                Console.WriteLine("[Analysis]: Converting Variables...");
            for (int index2 = 0; index2 < numArray1.Length; ++index2)
            {
                if (numArray1[index2] == 2)
                    numArray1[index2] = 0;
            }
            double num1 = Convert.ToDouble(numArray1.Length) / 100.0;
            double num2 = num1;
            for (int parent = 0; parent < numArray1.Length; ++parent)
            {
                if (verbose && (double)parent > num2)
                {
                    Console.WriteLine("[Analysis Level {0}] {0}% (Scanning parenthesis)", (object)stacklevel, (object)(Math.Round(Convert.ToDouble(parent) / Convert.ToDouble(numArray1.Length), 4) * 20.0));
                    num2 = (double)parent + num1;
                }
                if (numArray1[parent] == 1 && dataarray[parent] == 0.0)
                {
                    int closing = this.findClosing(numArray1, dataarray, parent);
                    double[] Dataset1 = new double[closing - parent + 1];
                    int[] Typeset1 = new int[Dataset1.Length];
                    numArray1[parent] = -1;
                    numArray1[closing] = -1;
                    for (int index2 = 0; index2 < Dataset1.Length; ++index2)
                    {
                        Dataset1[index2] = dataarray[parent + index2];
                        Typeset1[index2] = numArray1[parent + index2];
                        numArray1[parent + index2] = -1;
                    }
                    foreach (int num3 in this.analyse(Dataset1, Typeset1, stacklevel + 1, false))
                    {
                        numArray2[index1] = num3 + parent;
                        ++index1;
                        if (index1 == numArray2.Length)
                            numArray2 = Utils.extendArray<int>(numArray2, 900);
                    }
                    numArray1[numArray2[index1 - 1]] = 0;
                    parent = closing;
                }
            }
            double num4 = num1;
            for (int start = 0; start < numArray1.Length; ++start)
            {
                if (verbose && (double)start > num4)
                {
                    Console.WriteLine("[Analysis Level {0}] {1}% (Scanning built in functions)", (object)stacklevel, (object)(20.0 + Math.Round(Convert.ToDouble(start) / Convert.ToDouble(numArray1.Length), 4) * 20.0));
                    num4 = (double)start + num1;
                }
                if (numArray1[start] == 1 && dataarray[start] >= 9.0)
                {
                    if (index1 + 3 >= numArray2.Length)
                        numArray2 = Utils.extendArray<int>(numArray2, 900);
                    numArray2[index1 + 1] = start;
                    numArray2[index1 + 2] = this.findConstant(numArray1, start, 1);
                    numArray2[index1] = numArray2[index1 + 2];
                    numArray1[numArray2[index1 + 1]] = -1;
                    index1 += 3;
                }
            }
            double num5 = num1;
            for (int start = 0; start < numArray1.Length; ++start)
            {
                if (verbose && (double)start > num5)
                {
                    Console.WriteLine("[Analysis Level {0}] {1}% (Scanning exponents)", (object)stacklevel, (object)(40.0 + Math.Round(Convert.ToDouble(start) / Convert.ToDouble(numArray1.Length), 4) * 20.0));
                    num5 = (double)start + num1;
                }
                if (numArray1[start] == 1 && Convert.ToInt32(dataarray[start]) == 4)
                {
                    if (index1 + 3 >= numArray2.Length)
                        numArray2 = Utils.extendArray<int>(numArray2, 900);
                    numArray2[index1] = this.findConstant(numArray1, start, -1);
                    numArray2[index1 + 1] = start;
                    numArray2[index1 + 2] = this.findConstant(numArray1, start, 1);
                    numArray1[numArray2[index1]] = -1;
                    numArray1[numArray2[index1 + 1]] = -1;
                    index1 += 3;
                }
            }
            double num6 = num1;
            for (int start = 0; start < numArray1.Length; ++start)
            {
                if (verbose && (double)start > num6)
                {
                    Console.WriteLine("[Analysis Level {0}] {1}% (Scanning multiplication/division)", (object)stacklevel, (object)(60.0 + Math.Round(Convert.ToDouble(start) / Convert.ToDouble(numArray1.Length), 4) * 20.0));
                    num6 = (double)start + num1;
                }
                if (numArray1[start] == 1 && (dataarray[start] == 5.0 || dataarray[start] == 6.0))
                {
                    if (index1 + 3 >= numArray2.Length)
                        numArray2 = Utils.extendArray<int>(numArray2, 900);
                    numArray2[index1] = this.findConstant(numArray1, start, -1);
                    numArray2[index1 + 1] = start;
                    numArray2[index1 + 2] = this.findConstant(numArray1, start, 1);
                    numArray1[numArray2[index1]] = -1;
                    numArray1[numArray2[index1 + 1]] = -1;
                    index1 += 3;
                }
            }
            double num7 = num1;
            for (int start = 0; start < numArray1.Length; ++start)
            {
                if (verbose && (double)start > num7)
                {
                    Console.WriteLine("[Analysis Level {0}] {1}% (Scanning addition/subtraction)", (object)stacklevel, (object)(80.0 + Math.Round(Convert.ToDouble(start) / Convert.ToDouble(numArray1.Length), 4) * 20.0));
                    num7 = (double)start + num1;
                }
                if (numArray1[start] == 1 && (dataarray[start] == 7.0 || dataarray[start] == 8.0))
                {
                    if (index1 + 3 >= numArray2.Length)
                        numArray2 = Utils.extendArray<int>(numArray2, 900);
                    numArray2[index1] = this.findConstant(numArray1, start, -1);
                    numArray2[index1 + 1] = start;
                    numArray2[index1 + 2] = this.findConstant(numArray1, start, 1);
                    numArray1[numArray2[index1]] = -1;
                    numArray1[numArray2[index1 + 1]] = -1;
                    index1 += 3;
                }
            }
            return Utils.crunchArray<int>(numArray2, numArray2.Length - index1);
        }

        public double evaluate(double var)
        {
            return this.evaluate(new double[1] { var }, false);
        }

        public double evaluate(double[] variables, bool debug = false)
        {
            if (variables.Length < this.numVariables)
                throw new ArgumentOutOfRangeException(nameof(variables), "Array must contain an equal number of variables to the expression");
            if (debug)
                return this.evalWithDebug(variables, false);
            if (this.tapeEmpty)
                this.analysis(false);
            double[] numArray = new double[this.data.Length];
            for (int index = 0; index < this.data.Length; ++index)
                numArray[index] = this.data[index];
            for (int index = 0; index < this.type.Length; ++index)
            {
                if (this.type[index] == 2)
                    numArray[index] = variables[Convert.ToInt32(this.data[index])];
            }
            int index1 = 0;
            while (index1 < this.opTape.Length)
            {
                double num;
                switch (Convert.ToInt32(numArray[this.opTape[index1 + 1]]))
                {
                    case 4:
                        num = Math.Pow(numArray[this.opTape[index1]], numArray[this.opTape[index1 + 2]]);
                        break;
                    case 5:
                        num = numArray[this.opTape[index1]] * numArray[this.opTape[index1 + 2]];
                        break;
                    case 6:
                        num = numArray[this.opTape[index1]] / numArray[this.opTape[index1 + 2]];
                        break;
                    case 7:
                        num = numArray[this.opTape[index1]] + numArray[this.opTape[index1 + 2]];
                        break;
                    case 8:
                        num = numArray[this.opTape[index1]] - numArray[this.opTape[index1 + 2]];
                        break;
                    case 9:
                        num = Math.Sin(numArray[this.opTape[index1 + 2]]);
                        break;
                    case 10:
                        num = Math.Cos(numArray[this.opTape[index1 + 2]]);
                        break;
                    case 11:
                        num = Math.Tan(numArray[this.opTape[index1 + 2]]);
                        break;
                    case 12:
                        num = 1.0 / Math.Cos(numArray[this.opTape[index1 + 2]]);
                        break;
                    case 13:
                        num = 1.0 / Math.Sin(numArray[this.opTape[index1 + 2]]);
                        break;
                    case 14:
                        num = 1.0 / Math.Tan(numArray[this.opTape[index1 + 2]]);
                        break;
                    case 15:
                        num = 1.0 / Math.Abs(numArray[this.opTape[index1 + 2]]);
                        break;
                    case 16:
                        num = Math.Sqrt(numArray[this.opTape[index1 + 2]]);
                        break;
                    default:
                        throw new NotImplementedException("Encountered an operator code (" + (object)Convert.ToInt32(numArray[this.opTape[index1 + 1]]) + ") that does not have a defined case.");
                }
                numArray[this.opTape[index1 + 2]] = num;
                index1 += 3;
            }
            return numArray[((IEnumerable<int>)this.opTape).Last<int>()];
        }

        private string formatString(string format, int min, int max, bool rightJustify = false)
        {
            if (format.Length > max)
                return format.Substring(0, max);
            if (format.Length == max)
                return format;
            if (rightJustify)
                return new string(' ', min - format.Length) + format;
            return format + new string(' ', min - format.Length);
        }

        private void outputFunctionVerbose(double[] tData, int[] tType, bool[] nullTracker)
        {
            string str = "";
            for (int index = 0; index < this.type.Length; ++index)
            {
                if (nullTracker[index])
                    str = tType[index] != 1 ? str + Convert.ToString(tData[index]) + " " : str + this.operatorLookup[Convert.ToInt32(tData[index])] + " ";
            }
            Console.WriteLine("{0}", (object)str);
        }

        private void outputFunctionSuperVerbose(double[] tData, int[] tType, bool[] nullTracker)
        {
            Console.WriteLine();
            string str1 = "Expression     ";
            string str2 = "      Type   { ";
            string str3 = "      Data   { ";
            for (int index = 0; index < this.type.Length; ++index)
            {
                if (nullTracker[index])
                {
                    if (tType[index] == 1)
                    {
                        str1 = str1 + this.formatString(this.operatorLookup[Convert.ToInt32(tData[index])], 5, 5, false) + " ";
                        str2 += "op    ";
                        str3 = str3 + this.formatString(Convert.ToString(tData[index]), 5, 5, false) + " ";
                    }
                    else
                    {
                        str1 = str1 + this.formatString(Convert.ToString(tData[index]), 5, 5, false) + " ";
                        str3 = str3 + this.formatString(Convert.ToString(tData[index]), 5, 5, false) + " ";
                        str2 = tType[index] != 0 ? str2 + "var   " : str2 + "const ";
                    }
                }
                else
                {
                    str1 += "  ";
                    str2 += "# ";
                    str3 += "# ";
                }
            }
            Console.WriteLine("{0}", (object)str1);
            Console.WriteLine("{0}", (object)(str2 + " }"));
            Console.WriteLine("{0}", (object)(str3 + " }"));
        }

        public double evalWithDebug(double[] variables, bool superDebug = false)
        {
            if (this.tapeEmpty)
            {
                if (superDebug)
                {
                    Console.WriteLine("[PRE PROCESSING] Operation tape is empty, performing analysis...");
                    this.analysis(true);
                }
                else
                    this.analysis(false);
            }
            double[] numArray1 = new double[this.data.Length];
            if (superDebug)
                Console.WriteLine("[PRE PROCESSING] Creating working data set...");
            this.data.CopyTo((Array)numArray1, 0);
            if (superDebug)
                Console.WriteLine("[PRE PROCESSING] Substituting variable values into working set...");
            for (int index = 0; index < this.type.Length; ++index)
            {
                if (this.type[index] == 2)
                    numArray1[index] = variables[Convert.ToInt32(this.data[index])];
            }
            if (superDebug)
                Console.WriteLine("[PRE PROCESSING] Creating working type set (debug step only)");
            int[] numArray2 = new int[this.type.Length];
            this.type.CopyTo((Array)numArray2, 0);
            if (superDebug)
                Console.WriteLine("[PRE PROCESSING] Initializing nulltracker (debug step only)");
            bool[] nullTracker = new bool[this.type.Length];
            for (int index = 0; index < this.type.Length; ++index)
                nullTracker[index] = true;
            if (superDebug)
                Console.WriteLine("[PRE PROCESSING] Pre processing done, beginning evalution.");
            Console.WriteLine();
            if (superDebug)
                this.outputFunctionSuperVerbose(numArray1, numArray2, nullTracker);
            else
                this.outputFunctionVerbose(numArray1, numArray2, nullTracker);
            int index1 = 0;
            while (index1 < this.opTape.Length)
            {
                bool flag1 = true;
                if (superDebug)
                    Console.WriteLine("[EVALUATION {0}] Instruction: Operate {1} ({2}) with operator {3} ({4}) on {5} ({6})", (object)(1 + index1 / 3), (object)this.opTape[index1], (object)numArray1[this.opTape[index1]], (object)this.opTape[index1 + 1], (object)this.operatorLookup[Convert.ToInt32(numArray1[this.opTape[index1 + 1]])], (object)this.opTape[index1 + 2], (object)numArray1[this.opTape[index1 + 2]]);
                double num;
                switch (Convert.ToInt32(numArray1[this.opTape[index1 + 1]]))
                {
                    case 4:
                        num = Math.Pow(numArray1[this.opTape[index1]], numArray1[this.opTape[index1 + 2]]);
                        break;
                    case 5:
                        num = numArray1[this.opTape[index1]] * numArray1[this.opTape[index1 + 2]];
                        break;
                    case 6:
                        num = numArray1[this.opTape[index1]] / numArray1[this.opTape[index1 + 2]];
                        break;
                    case 7:
                        num = numArray1[this.opTape[index1]] + numArray1[this.opTape[index1 + 2]];
                        break;
                    case 8:
                        num = numArray1[this.opTape[index1]] - numArray1[this.opTape[index1 + 2]];
                        break;
                    case 9:
                        num = Math.Sin(numArray1[this.opTape[index1]]);
                        flag1 = false;
                        break;
                    case 10:
                        num = Math.Cos(numArray1[this.opTape[index1]]);
                        flag1 = false;
                        break;
                    case 11:
                        num = Math.Tan(numArray1[this.opTape[index1]]);
                        flag1 = false;
                        break;
                    case 12:
                        num = 1.0 / Math.Cos(numArray1[this.opTape[index1]]);
                        flag1 = false;
                        break;
                    case 13:
                        num = 1.0 / Math.Sin(numArray1[this.opTape[index1]]);
                        flag1 = false;
                        break;
                    case 14:
                        num = 1.0 / Math.Tan(numArray1[this.opTape[index1]]);
                        flag1 = false;
                        break;
                    case 15:
                        num = 1.0 / Math.Abs(numArray1[this.opTape[index1]]);
                        flag1 = false;
                        break;
                    case 16:
                        num = Math.Sqrt(numArray1[this.opTape[index1]]);
                        flag1 = false;
                        break;
                    default:
                        throw new NotImplementedException("Encountered an operator code (" + (object)Convert.ToInt32(numArray1[this.opTape[index1 + 1]]) + ") that does not have a defined case.");
                }
                numArray1[this.opTape[index1 + 2]] = num;
                numArray2[this.opTape[index1 + 2]] = 0;
                if (superDebug)
                    Console.WriteLine("[EVALUATION {0}] Instruction result: {1} stored in index {2}", (object)(1 + index1 / 3), (object)num, (object)this.opTape[index1 + 2]);
                if (flag1)
                    nullTracker[this.opTape[index1]] = false;
                nullTracker[this.opTape[index1 + 1]] = false;
                for (int index2 = 0; index2 < this.type.Length; ++index2)
                {
                    if (numArray2[index2] == 1 && (numArray1[index2] == 0.0 || numArray1[index2] == 2.0))
                    {
                        bool flag2 = false;
                        int parent = index2;
                        int closing = this.findClosing(numArray2, numArray1, parent);
                        for (int index3 = parent + 1; index3 < closing; ++index3)
                        {
                            if (nullTracker[index3])
                            {
                                flag2 = true;
                                break;
                            }
                        }
                        if (!flag2)
                        {
                            nullTracker[parent] = false;
                            nullTracker[closing] = false;
                        }
                    }
                }
                if (superDebug)
                    this.outputFunctionSuperVerbose(numArray1, numArray2, nullTracker);
                else
                    this.outputFunctionVerbose(numArray1, numArray2, nullTracker);
                index1 += 3;
            }
            return numArray1[((IEnumerable<int>)this.opTape).Last<int>()];
        }

        public void Benchmark()
        {
            Stopwatch stopwatch = new Stopwatch();
            double[][] workset = new double[1000000 / this.steps + 5][];
            Random random = new Random();
            double[] numArray = new double[this.numVariables];
            for (int index1 = 0; index1 < workset.Length; ++index1)
            {
                for (int index2 = 0; index2 < this.numVariables; ++index2)
                    numArray[index2] = random.NextDouble() * 2000.0 - 1000.0;
                workset[index1] = numArray;
            }
            stopwatch.Start();
            for (int index = 0; index < workset.Length; ++index)
                this.evaluate(workset[index], false);
            stopwatch.Stop();
            this.singleThreadedOPS = Convert.ToInt32(1000.0 / (Convert.ToDouble(stopwatch.ElapsedMilliseconds) / (double)workset.Length));
            stopwatch.Restart();
            new ParallelOptions().MaxDegreeOfParallelism = Environment.ProcessorCount;
            for (int index1 = 0; index1 < Environment.ProcessorCount; ++index1)
                Parallel.For(0, workset.Length, (Action<int>)(index => this.evaluate(workset[index], false)));
            stopwatch.Stop();
            this.multiThreadedTotalOPS = Convert.ToInt32((double)(Environment.ProcessorCount * 1000) / (Convert.ToDouble(stopwatch.ElapsedMilliseconds) / (double)workset.Length));
            this.multiThreadedIndividualOPS = this.multiThreadedTotalOPS / Environment.ProcessorCount;
        }
    }

    [Serializable]
    public class arrayMismatchException : Exception
    {
        public arrayMismatchException() { }
        public arrayMismatchException(string message) : base(message) { }
        public arrayMismatchException(string message, Exception inner) : base(message, inner) { }
        protected arrayMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}