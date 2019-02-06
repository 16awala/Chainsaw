using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ChainsawEngine
{
    static class Utils
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        public static string arrayToString<type>(type[] set)
        {
            return arrayToString(set, " , ");
        }
        public static string arrayToString<type>(type[] set, string separator)
        {
            string retval = "";
            for (int i = 0; i < set.Length; i++)
            {
                retval += Convert.ToString(set[i]);
            }
            return retval;
        }
        public static type[] extendArray<type>(type[] set, int indexes)
        {
            type[] retval = new type[set.Length + indexes];
            set.CopyTo(retval, 0);
            return retval;
        }
        public static type[] removeArrayElement<type>(type[] set, int index)
        {
            int p = 0;
            type[] retval = new type[set.Length - 1];
            for (int i = 0; i < set.Length; i++)
            {
                if (i == index)
                {
                    i++;
                    if (i != set.Length)
                    {
                        retval[p] = set[i];
                        p++;
                    }
                }
                else
                {
                    retval[p] = set[i];
                    p++;
                }
            }
            return retval;
        }
        public static type[] insertArrayElement<type>(type[] set, int index, type value)
        {
            type[] retval = new type[set.Length + 1];
            int p = 0;
            for (int i = 0; i < set.Length; i++)
            {
                if (i == index)
                {
                    retval[p] = value;
                    p++;
                    retval[p] = set[i];
                }
                else
                {
                    retval[p] = set[i];
                }
                p++;
            }
            return retval;
        }
        public static type[] crunchArray<type>(type[] array, int items)
        {
            type[] retval = new type[array.Length - items];
            for (int i = 0; i < retval.Length; i++)
            {
                retval[i] = array[i];
            }
            return retval;
        }
        public static string replaceAll(string raw, string[] match, string[] replace)//replace each instance of match with its associated replacement
        {
            if (match.Length != replace.Length)
            {
                throw new arrayMismatchException("Match and replace arrays must be of equal length.");
            }
            for (int i = 0; i < match.Length; i++)
            {
                raw = raw.Replace(match[i], replace[i]);
            }
            return raw;
        }

        public static DataSet2D Data2DFromExpression(Expression e, double xLowerbound, double xUpperBound)
        {
            return Data2DFromExpression(e, xLowerbound, xUpperBound, 100);
        }
        public static DataSet2D Data2DFromExpression(Expression e, double xLowerbound, double xUpperBound, int xResolution)
        {
            if (e.varCount > 1)
            {
                throw new ArgumentException("Cannot produce a 2D set from a multi variable expression");
            }
            double interval = (xUpperBound - xLowerbound) / xResolution;
            double input;
            DataSet2D set = new DataSet2D(new double[xResolution], new double[xResolution]);
            for (int i = 0; i < xResolution; i++)
            {
                input = xLowerbound + (i * interval);
                set.x[i] = input;
                set.y[i] = e.evaluate(new double[] { input });
            }
            return set;
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
        private double[] data;//Data tape
        private int[] type;//Interpreter tape
        private string[] operatorLookup = { "(", ")", "|", "|", "^", "*", "/", "+", "-", "sin", "cos","tan","sec","csc","cot","abs" };//for string output
        private int[] opTape;//Instructions for evaluating the expression
        private bool tapeEmpty = true;//Has the tape been constructed?
        private int numVariables;//Number of unique variables the expression contains.
        public int steps//Number of steps in evaulating the function sequentially
        {
            get
            {
                if (opTape == null)
                {
                    analysis();
                }
                return opTape.Length / 3;
            }
        }
        private int singleThreadedOPS = -1;//Operations per second as obtained by benchmarking in a single thread
        private int multiThreadedTotalOPS = -1;//Operations per second as obtained by benchmarking on all threads
        private int multiThreadedIndividualOPS;// (multiThreadedTotalOPS / cpucores)
        public int opsSingle
        {
            get
            {
                return singleThreadedOPS;
            }
        }
        public int opsMutliTotal
        {
            get
            {
                return multiThreadedTotalOPS;
            }
        }
        public int opsMultiIndividual
        {
            get
            {
                return multiThreadedIndividualOPS;
            }
        }

        //public getters just in case they are wanted.
        public int varCount
        {
            get
            {
                return numVariables;
            }
        }
        public int[] rawTypes
        {
            get
            {
                return type;
            }
        }
        public double[] rawData
        {
            get
            {
                return data;
            }
        }

        //Constructors
        public Expression(double[] Values, int[] Types)//Constructor using raw data. Can be used to copy or tweak expressions. Does not call clean().
        {
            if (Values.Length == Types.Length)
            {
                data = Values;
                type = Types;
                updateVarCount();
            }
            else
            {
                throw new arrayMismatchException("'Values' and 'Types' must be equal in length.");
            }
        }
        public Expression(string RawExpr)//Attempts to parse the string into an expression. Absolute value signs must be entered with square brackets. Calls clean().
        {
            RawExpr = RawExpr.ToLower();
            //Replace known operations and constants with function symbols and numbers.
            RawExpr = Utils.replaceAll(RawExpr, new string[] { " ", "sin", "cos", "tan", "sec", "csc", "cot", "abs", "–","e","pi" }, new string[] { "", "$1", "$2", "$3", "$4", "$5", "$6","$7", "-","2.718281828459045","3.141592653589793" });
            //Scan string for implicit multiplication
            char[] num = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            for (int i = 1; i < RawExpr.Length; i++)
            {
                if (((RawExpr[i] == '(' || RawExpr[i] == 'x')&& num.Contains(RawExpr[i - 1]) && RawExpr[i-2] != '$')||(num.Contains(RawExpr[i]) && RawExpr[i - 1] == ')'))//WHUT
                {
                    RawExpr = RawExpr.Substring(0,i) + "*" + RawExpr.Substring(i, RawExpr.Length - i);
                }
            }

            //Regex.Split(RawExpr,)

            double[] tempdata = new double[50];
            int[] temptype = new int[50];
            int tmpi = 0;
            string substr;
            int EON;
            for (int i = 0; i < RawExpr.Length; i++)
            {
                char[] operators = { '(', ')', '[',']','^', '*', '/', '+', '-','$' };//List of 'special' characters (minus 'x') that interrupt numbers
                char[] specops = { '1', '2', '3', '4', '5', '6','7' };
                if (RawExpr[i] == 'x')//Found a variable
                {
                    temptype[tmpi] = 2;
                    if (!(i < RawExpr.Length - 1))//If this is the end of the string there's no sub
                    {
                        tempdata[tmpi] = 0;

                    }
                    else
                    {//find (if any) a sub value
                        i++;
                        EON = findEON(RawExpr, i);
                        if (EON==-1)
                        {
                            tempdata[tmpi] = 0;
                        }
                        else
                        {
                            substr = RawExpr.Substring(i,EON-i+1);
                            tempdata[tmpi] = Convert.ToDouble(substr);
                            i += substr.Length;
                        }
                        i--;
                        
                    }
                    tmpi++;
                }
                else
                {
                    char c = RawExpr.ElementAt(i);
                    if (operators.Contains(c))//Found an operator
                    {
                        temptype[tmpi] = 1;
                        if (c == '$')
                        {
                            i++;
                            c = RawExpr.ElementAt(i);
                            for (int j = 0; j < specops.Length; j++)
                            {
                                if (c == specops[j])
                                {
                                    tempdata[tmpi] = j+9;
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < operators.Length; j++)
                            {
                                if ((RawExpr.ElementAt(i)) == operators.ElementAt(j))//Figure out what operator this is
                                {
                                    tempdata[tmpi] = j;
                                }
                            }
                        }
                        tmpi++;
                    }
                    else
                    {//Must be a number
                        temptype[tmpi] = 0;
                        substr = RawExpr.Substring(i, findEON(RawExpr, i) - i + 1);
                        tempdata[tmpi] = Convert.ToDouble(substr);
                        i += substr.Length-1;
                        tmpi++;
                    }
                }
                
                if (tmpi == tempdata.Length - 1)
                {
                    tempdata = Utils.extendArray(tempdata, 500);
                    temptype = Utils.extendArray(temptype, 500);
                }
            }
            data = new double[tmpi];
            type = new int[tmpi];
            for (int i = 0; i < tmpi; i++)
            {
                data[i] = tempdata[i];
                type[i] = temptype[i];
            }
            clean();
        }

        //Utilities
        public string toString(bool condenseMultiplication = true, bool addSpaces = false)//public toStr
        {
            return toStr(condenseMultiplication, addSpaces);
        }
        private string toStr(bool compact, bool padterms)//returns the expression in the readable form of a string.
        {
            string str = "";
            for (int i = 0; i < type.Length; i++)
            {
                if (padterms)
                {
                    str += " ";
                }
                switch (type[i])
                {
                    case 0:
                        str += Convert.ToString(data[i]);
                        break;
                    case 1:
                        if (data[i] >= 0 && data[i] <= operatorLookup.Length-1)
                        {
                            if (!compact)
                            {
                                str += operatorLookup[Convert.ToInt32(data[i])];
                            }
                            else
                            {
                                if (!(data[i] == 5))//If it is not multiplication
                                {
                                    str += operatorLookup[Convert.ToInt32(data[i])];
                                }
                                else
                                {
                                    if (!(i == 1 && type[i + 1] != 0))//if multiplier involves first term not followed by a constant
                                    {
                                        if (!(type[i - 1] == 0 && type[i + 1] == 2))//if it is in the format c*x, drop *
                                        {
                                            str += operatorLookup[Convert.ToInt32(data[i])];
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException("data[" + Convert.ToString(i) + "]", data[i], "Unkown operator");
                        }
                        break;
                    case 2:
                        if (data[i] == 0.0)
                        {
                            str += "x";
                        }
                        else
                        {
                            str += "x" + Convert.ToString(data[i]);
                        }
                        break;
                    default:
                        str += "?";
                        //throw new ArgumentOutOfRangeException("type[" + Convert.ToString(i) + "]", type[i], "Non-valid expression object");
                        break;
                }
            }
            return str;
        }
        public void clean(int operation = 0)
        {
            switch (operation)
            {
                case 0://Run all cleaning operations.
                    clean(1);
                    clean(2);
                    clean(3);
                    break;
                case 1://fix subs and fill gaps. (make x1.2 x3.3 x4 to x1 x2 x3). Updates numVariables when done.
                    fixDecimalSubs();//Truncate decimal subs cause that's weird.
                    updateVarCount();//In case truncated subs condensed unique sub count, update it.
                    bool[] subs = new bool[numVariables];//List of whether subs are used or not
                    double[] assignments = new double[numVariables];//What assignment to give to each sub when done.
                    for (int i = 0; i < numVariables; i++)//assignments start normally, sub 0 becomes sub 0 etc.
                    {
                        assignments[i] = i;
                    }
                    for (int i = 0; i < type.Length; i++)//Find all subs that are in use
                    {
                        if (type[i] == 2)
                        {
                            subs[Convert.ToInt32(data[i])] = true;
                        }
                    }
                    int firstSpace = numVariables + 1;
                    bool solid = true;
                    for (int i = 0; i < numVariables; i++)
                    {
                        if (solid)//If no spaces have yet been encountered
                        {
                            if (!subs[i])//Check for a space
                            {
                                solid = false;
                                if (i < firstSpace)
                                {
                                    firstSpace = i;
                                }
                            }
                        }
                        else
                        {
                            if (subs[i])//If not a space, re-assign this sub to the next available one.
                            {
                                assignments[i] = firstSpace;
                                firstSpace++;
                            }
                        }
                    }
                    for (int i = 0; i < type.Length; i++)
                    {
                        if (type[i] == 2)
                        {
                            data[i] = assignments[Convert.ToInt32(data[i])];
                        }
                    }
                    updateVarCount();
                    break;
                case 2://Remove redundant parenthesis. Eg converts 1+(1) and 1+(((1))) to 1+1
                       //This algorithm is incredibly inefficient and should probably be optimized but
                       //given how little it is used (pretty much only on creation) it's not super important
                       //Also.... //LAZY
                       //As a thought... doesn't simplify (((1+1))) to (1+1), leaves as-is
                       //Also as a thought... since parenthesis are entirely ignored on actual evaluation, the only performance impact this
                       //has is on analysis and readback which means that while technically making things possibly nicer to look at has
                       //no actual benefit in terms of evaluation performance, and could actually lengthen the process of analysis since
                       //The analysis algorithm doesn't take TOO long to deal with parenthesis.
                    bool go = true;
                    while (go)
                    {
                        go = false;
                        for (int i = 0; i < type.Length; i++)
                        {
                            if (type[i] == 1)
                            {
                                if (data[i] == 0.0 || data[i] == 2.0)
                                {
                                    if (findClosing(i) == i + 2)
                                    {
                                        type = Utils.removeArrayElement(type, i+2);
                                        type = Utils.removeArrayElement(type, i);
                                        data = Utils.removeArrayElement(data, i + 2);
                                        data = Utils.removeArrayElement(data, i);
                                        go = true;
                                    }
                                }
                            }
                        }
                    }
                    
                    break;
                case 3://Convert orphaned subtraction operations into negative multipliers
                    int pretype = -1;
                    for (int i = 0; i < type.Length; i++)
                    {
                        if (pretype == 1 && type[i] == 1 && data[i] == 8)//Subtraction sign preceded by an operator
                        {
                            if (type[i+1] == 0)//Next is a constant
                            {
                                data[i + 1] = data[i + 1] * -1;
                                type = Utils.removeArrayElement(type, i);
                                data = Utils.removeArrayElement(data, i);
                            }
                            else
                            {
                                if (type[i+1] == 2)//Next is a variable (turn -x into (-1*x) for analysis/evaluation accuracy)
                                {
                                    data[i] = 5;//Turn '-' into '*'
                                    type = Utils.insertArrayElement(type, i, 1);//  (
                                    data = Utils.insertArrayElement(data, i, 0);
                                    i++;
                                    type = Utils.insertArrayElement(type, i, 0);//  -1
                                    data = Utils.insertArrayElement(data, i, -1);
                                    i+= 3;//  x
                                    type = Utils.insertArrayElement(type, i, 1);//  )
                                    data = Utils.insertArrayElement(data, i, 1);
                                    i++;
                                }
                            }
                        }
                        if (type[i] == 1 && data[i] > 3 )
                        {
                            pretype = 1;
                        }
                        else
                        {
                            pretype = -1;
                        }
                    }
                    break;
                default:
                    break;
            }
        }//Various methods of sprucing up / fixing invalid syntax

        //SBUG: ((x+4)*5)-x3^5 as an input causes output of ((x+4)*5)(-1*x1)^5
        //Note the lack of operators between parenthesis sets!
        // Minus got converted improperly, possibly detecting the 1st term parenthesis as a valid
        // operator. Do we have protection in addition cases such as this?

        private int findClosing(int parent)//Returns index of the closing parenthesis or bracket locally 
        {
            if (type[parent] == 1 && data[parent] < 4 && data[parent] >= 0)
            {
                int i = parent;//current index
                int p;//Direction of tracking
                int l;//Parenthesis layer.
                if (data[parent] == 0 || data[parent] == 2)
                {
                    p = 1;
                    l = 1;
                }
                else
                {
                    p = -1;
                    l = -1;
                }
                while (l != 0)
                {
                    i += p;
                    if (type[i] == 1)
                    {
                        if (data[i] == 0 || data[i] == 2)
                        {
                            l++;
                        }
                        else
                        {
                            if (data[i] == 1 || data[i] == 3)
                            {
                                l--;
                            }
                        }
                    }
                }
                return i;
            }
            else
            {
                return -1;
            }
        }
        private int findClosing(int[] typearray, double[] dataarray, int parent)//Returns index of the closing parenthesis or bracket 
        {
            if (typearray[parent] == 1 && dataarray[parent] < 4 && dataarray[parent] >= 0)
            {
                int i = parent;//current index
                int p;//Direction of tracking
                int l;//Parenthesis layer.
                if (dataarray[parent] == 0 || dataarray[parent] == 2)
                {
                    p = 1;
                    l = 1;
                }
                else
                {
                    p = -1;
                    l = -1;
                }
                while (l != 0)
                {
                    i += p;
                    if (typearray[i] == 1)
                    {
                        if (dataarray[i] == 0 || dataarray[i] == 2)
                        {
                            l++;
                        }
                        else
                        {
                            if (dataarray[i] == 1 || dataarray[i] == 3)
                            {
                                l--;
                            }
                        }
                    }
                }
                return i;
            }
            else
            {
                return -1;
            }
        }
        private int findEOT(int start)//returns index of the end of the term starting at the index <start>
        {
            int i = start;//index
            bool go = true;
            while (go)
            {
                if (i + 1 >= type.Length)//If item i+1 does not exist
                {
                    go = false;//End of term
                }
                else
                {
                    if (type[i] == 0 || type[i] == 2)//If item is a variable or constant
                    {
                        i++;//Not end of term
                    }
                    else
                    {
                        if (data[i] == 0 || data[i] == 2)//If item is a parenthesis
                        {
                            i = findClosing(i) + 1;//Skip to closing paren
                        }
                        else
                        {
                            if (data[i] == 7 || data[i] == 8)//Addition or subtraction (separates terms)
                            {
                                go = false;//End of term
                            }
                            else
                            {
                                i++;//Not end of term
                            }
                        }
                    }
                }
            }
            return i - 1;
        }
        public int findEON(string str, int c)//returns char index of the end of a number starting at index c.
        {
            int i = c;
            char[] nonNumerics = {'(',')','[',']','^','*','/','+','-','x' };
            bool b = true;
            while (b)//WHUT
            {
                if (i == str.Length-1)
                {
                    if (nonNumerics.Contains(str[i]))
                    {
                        i--;
                    }
                    b = false;
                }
                else
                {
                    if (nonNumerics.Contains(str[i]))
                    {
                        b = false;
                        i--;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            if (i<c)
            {
                return -1;
            }
            else
            {
                return i;
            }
        }
        private void fixDecimalSubs()//Makes sure all subs are integers (no x sub 1.5)
        {
            for (int i = 0; i < type.Length; i++)
            {
                if (type[i] == 2)
                {
                    data[i] = Math.Truncate(data[i]);//Chop decimal value
                }
            }
        }
        private void updateVarCount()//returns number of variables accepted by the function. (must have run clean(0) or clean(1) to be accurate)
        {
            double count = -1;//default value to make sure any variable number replaces it
            for (int i = 0; i < type.Length; i++)
            {
                if (type[i] == 2 && data[i] > count)
                {
                    count = data[i];
                }
            }
            numVariables = Convert.ToInt32(count + 1);
        }
        public int findConstant(int[] typeData, int start, int interval)//Small tracking function used to scan left or right to find a constant
        {
            bool search = true;
            int i = start;
            while (search)
            {
                if (typeData[i] == 0)
                {
                    search = false;
                }
                else
                {
                    i = i + interval;
                }
            }
            return i;
        }

        public int[] analysis()//Public function call for anlysis, useful if the raw constructor was used.
        {
            opTape = analyze(data, type,0);
            tapeEmpty = false;
            return opTape;
        }
        private int[] analyze(double[] Dataset, int[] Typeset, int stacklevel)//Analyzes the expression and creates a set of instructions for evaluating it.
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
            double[] exprData = new double[Dataset.Length];
            int[] exprType = new int[Typeset.Length]; ;
            Dataset.CopyTo(exprData, 0);//In the case of self analysis using a different variable is necessary to prevent expression corruption
            Typeset.CopyTo(exprType, 0);
            int[] tape = new int[900];//Tape as required
            int headPos = 0;//Index of tape
            //Beginning with parenthesis
            if (stacklevel == 0) { Console.WriteLine("[Analysis]: Converting Variables..."); }
            for (int i = 0; i < exprType.Length; i++)//Convert all variables into constants for analysis, as that is how they are computed
            {
                if (exprType[i] == 2)
                {
                    exprType[i] = 0;
                }
            }
            double progint = Convert.ToDouble(exprType.Length) / 100;
            double prog = progint;
            for (int i = 0; i < exprType.Length; i++)//Find parenthesis sets
            {
                if (stacklevel==0 && i > prog)
                {
                    Console.WriteLine("[Analysis] {0}% (Scanning parenthesis)", Math.Round(Convert.ToDouble(i) / Convert.ToDouble(exprType.Length), 4) * 20);
                    prog = i + progint;
                }
                if (exprType[i] == 1 && exprData[i] == 0.0)//Opening parenthesis found
                {
                    int endOfTerm = findClosing(exprType, exprData,i);
                    double[] tmpData = new double[endOfTerm - i + 1];//Create new arrays containing the data within the parenthesis
                    int[] tmpType = new int[tmpData.Length];
                    exprType[i] = -1;//Set the parenthesis to null before passing them in to prevent infinite recursion
                    exprType[endOfTerm] = -1;
                    for (int j = 0; j < tmpData.Length; j++)//Copy data into new arrays
                    {
                        tmpData[j] = exprData[i + j];
                        tmpType[j] = exprType[i + j];
                        exprType[i + j] = -1;//When this all comes back it will be null
                    }
                    int[] tmpTape = analyze(tmpData, tmpType,stacklevel+1);
                    for (int j = 0; j < tmpTape.Length; j++)
                    {
                        tape[headPos] = tmpTape[j] + i; //copy temp tape instructions to main tape while updating the offset indexes.
                        headPos++;
                        if (headPos == tape.Length)
                        {
                            tape = Utils.extendArray(tape, 900);//Extend the tape's size if necessary
                        }
                    }
                    exprType[tape[headPos-1]] = 0;//Result from parenthesis evaluation is stored in this index, so it should be a constant instead of null as written earlier
                    i = endOfTerm;//skip to end of parenthesis
                }
            }
            // 0 1 2 3 4 5 6 7 8
            // ( ) | | ^ * / + -
            prog = progint;
            for (int typeIndex = 0; typeIndex < exprType.Length; typeIndex++)//Scan entire expression for built in functions
            {
                if (stacklevel == 0 && typeIndex > prog)
                {
                    Console.WriteLine("[Analysis] {0}% (Scanning built in functions)", 20+Math.Round(Convert.ToDouble(typeIndex) / Convert.ToDouble(exprType.Length), 4) * 20);
                    prog = typeIndex + progint;
                }
                if (exprType[typeIndex] == 1 && exprData[typeIndex] >= 9)
                {
                    if (headPos + 3 >= tape.Length)//Just in case
                    {
                        tape = Utils.extendArray(tape, 900);
                    }
                    tape[headPos + 1] = typeIndex;//operator index
                    tape[headPos + 2] = findConstant(exprType, typeIndex, 1);//righthand constant index
                    tape[headPos] = tape[headPos + 2];//There is no lefthand, so we make it the same as righthand since we only operate on that
                    exprType[tape[headPos + 1]] = -1;//Since we had no lefthand, only need to nullify the operator
                    //exprType[headPos + 2] = 0;//This is probably unecessary. //Update: as it turns out it broke it somehow...bug? ¯\_(ツ)_/¯
                    headPos = headPos + 3;
                }
            }
            prog = progint;
            for (int typeIndex = 0; typeIndex < exprType.Length; typeIndex++)//Scan entire expression for exponents
            {
                if (stacklevel == 0 && typeIndex > prog)
                {
                    Console.WriteLine("[Analysis] {0}% (Scanning exponents)", 40 + Math.Round(Convert.ToDouble(typeIndex) / Convert.ToDouble(exprType.Length), 4) * 20);
                    prog = typeIndex + progint;
                }
                if (exprType[typeIndex] == 1 && Convert.ToInt32(exprData[typeIndex]) == 4)//Operator match
                {
                    if (headPos+3 >= tape.Length)//Just in case
                    {
                        tape = Utils.extendArray(tape, 900);
                    }
                    tape[headPos] = findConstant(exprType, typeIndex, -1);//Lefthand constant index
                    tape[headPos+1] = typeIndex;//operator index
                    tape[headPos+2] = findConstant(exprType, typeIndex, 1);//righthand constant index

                    exprType[tape[headPos]] = -1;
                    exprType[tape[headPos + 1]] = -1;
                    //exprType[headPos + 2] = 0;//This is probably unecessary. Update: Good thing, turns out it breaks stuff hardcore... ¯\_(ツ)_/¯
                    headPos = headPos+3;
                }
            }
            prog = progint;
            for (int typeIndex = 0; typeIndex < exprType.Length; typeIndex++)
            {
                if (stacklevel == 0 && typeIndex > prog)
                {
                    Console.WriteLine("[Analysis] {0}% (Scanning multiplication/division)", 60 + Math.Round(Convert.ToDouble(typeIndex) / Convert.ToDouble(exprType.Length), 4) * 20);
                    prog = typeIndex + progint;
                }
                if (exprType[typeIndex] == 1 && (exprData[typeIndex] == 5 || exprData[typeIndex] == 6))//Multiplication or division
                {
                    if (headPos + 3 >= tape.Length)//Just in case
                    {
                        tape = Utils.extendArray(tape, 900);
                    }
                    tape[headPos] = findConstant(exprType, typeIndex, -1);//Lefthand constant index
                    tape[headPos + 1] = typeIndex;//operator index
                    tape[headPos + 2] = findConstant(exprType, typeIndex, 1);//righthand constant index

                    exprType[tape[headPos]] = -1;
                    exprType[tape[headPos + 1]] = -1;
                    headPos = headPos + 3;
                }
            }
            prog = progint;
            for (int typeIndex = 0; typeIndex < exprType.Length; typeIndex++)
            {
                if (stacklevel == 0 && typeIndex > prog)
                {
                    Console.WriteLine("[Analysis] {0}% (Scanning addition/subtraction)", 80 + Math.Round(Convert.ToDouble(typeIndex) / Convert.ToDouble(exprType.Length), 4) * 20);
                    prog = typeIndex + progint;
                }
                if (exprType[typeIndex] == 1 && (exprData[typeIndex] == 7 || exprData[typeIndex] == 8))//addition or subtraction
                {
                    if (headPos + 3 >= tape.Length)//Just in case
                    {
                        tape = Utils.extendArray(tape, 900);
                    }
                    tape[headPos] = findConstant(exprType, typeIndex, -1);//Lefthand constant index
                    tape[headPos + 1] = typeIndex;//operator index
                    tape[headPos + 2] = findConstant(exprType, typeIndex, 1);//righthand constant index

                    exprType[tape[headPos]] = -1;
                    exprType[tape[headPos + 1]] = -1;
                    headPos = headPos + 3;
                }
            }
            return Utils.crunchArray(tape, tape.Length - headPos);//Cut off empty tape and return
        }

        public double evaluate(double[] variables)//Evaluates the function given variables, returning the result.
        {
            if (variables.Length != numVariables)//double check we have enough / not too many variables
            {
                throw new ArgumentOutOfRangeException("variables", "Array must contain an equal number of variables to the expression");
            }
            if (tapeEmpty)//If the expression has not yet been analyzed, it needs to be done before evaluation can be done.
            {
                opTape = analysis();
                tapeEmpty = false;
            }
            double[] tempData = new double[data.Length];
            for (int i = 0; i < data.Length; i++)//WHUT
            {
                tempData[i] = data[i];
            }
            for (int i = 0; i < type.Length; i++)
            {
                if (type[i] == 2)//fill variables in
                {
                    tempData[i] = variables[Convert.ToInt32(data[i])];
                }

            }
            for (int pos = 0; pos < opTape.Length; pos = pos+3)//Primary loop
            {
                //opTape[pos] -- left constant index
                //opTape[pos+1] -- operator index
                //opTape[pos+2] -- right constant index
                switch (Convert.ToInt32(tempData[opTape[pos+1]]))//switch on operator
                {
                    case 4:// ^
                        tempData[opTape[pos + 2]] = Math.Pow(tempData[opTape[pos]], tempData[opTape[pos + 2]]);
                        break;
                    case 5:// *
                        tempData[opTape[pos + 2]] = tempData[opTape[pos]] * tempData[opTape[pos + 2]];
                        break;
                    case 6:// /
                        tempData[opTape[pos + 2]] = tempData[opTape[pos]] / tempData[opTape[pos + 2]];
                        break;
                    case 7:// +
                        tempData[opTape[pos + 2]] = tempData[opTape[pos]] + tempData[opTape[pos + 2]];
                        break;
                    case 8:// -
                        tempData[opTape[pos + 2]] = tempData[opTape[pos]] - tempData[opTape[pos + 2]];
                        break;
                    case 9:// sin
                        tempData[opTape[pos + 2]] = Math.Sin(tempData[opTape[pos]]);
                        break;
                    case 10:// cos
                        tempData[opTape[pos + 2]] = Math.Cos(tempData[opTape[pos]]);
                        break;
                    case 11:// tan
                        tempData[opTape[pos + 2]] = Math.Tan(tempData[opTape[pos]]);
                        break;
                    case 12:// sec
                        tempData[opTape[pos + 2]] = 1/Math.Cos(tempData[opTape[pos]]);
                        break;
                    case 13:// csc
                        tempData[opTape[pos + 2]] = 1/Math.Sin(tempData[opTape[pos]]);
                        break;
                    case 14:// cot
                        tempData[opTape[pos + 2]] = 1/Math.Tan(tempData[opTape[pos]]);
                        break;
                    case 15:// abs
                        tempData[opTape[pos + 2]] = 1 / Math.Abs(tempData[opTape[pos]]);
                        break;
                    default:
                        break;
                }
            }
            return tempData[opTape.Last()];//Math results propagate to the end of the list, at the end of the loop, the result should be at the very end
        }

        public void Benchmark()//Runs the evaluator through a few tests using synthetic data sets. 
        {
            Stopwatch metric = new Stopwatch();
            //Generate synthetic data set
            double[][] workset = new double[(1000000/steps)+5][];//Generate a workset size inversely related to the number of steps in evaluation so we don't take too long.
            Random randGen = new Random();
            double[] temp = new double[numVariables];
            for (int i = 0; i < workset.Length; i++)
            {
                for (int j = 0; j < numVariables; j++)
                {
                    temp[j] = randGen.NextDouble()*2000-1000;//not necessary restrictions on number size but whatever.
                }
                workset[i] = temp;
            }
            //Start watch and run through synthetic workload on one thread
            metric.Start();
            for (int i = 0; i < workset.Length; i++)
            {
                evaluate(workset[i]);
            }
            metric.Stop();
            singleThreadedOPS = Convert.ToInt32(1000 / (Convert.ToDouble(metric.ElapsedMilliseconds) / workset.Length));
            metric.Restart();
            ParallelOptions pop = new ParallelOptions();
            pop.MaxDegreeOfParallelism = Environment.ProcessorCount;//try to use 1 thread for each processor core. No control over exactly how threads are balanced, but this is about as close as we can get w/o some black magic (aka threading knowledge I don't have)
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                Parallel.For(0, workset.Length, index => { evaluate(workset[index]); });
            }
            metric.Stop();
            multiThreadedTotalOPS = Convert.ToInt32(Environment.ProcessorCount*1000 / (Convert.ToDouble(metric.ElapsedMilliseconds) / workset.Length));
            multiThreadedIndividualOPS = multiThreadedTotalOPS / Environment.ProcessorCount;
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