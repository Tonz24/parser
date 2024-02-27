using System.Data;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;

namespace cv1;

public enum TokenType {
    Number,
    Operator
}

public class Token {
    public TokenType TokenType { get; protected set; }

    protected Token(String value) {
    }
}

public class Number : Token {
    public double Value { get; set; }
    
    public Number(String value) : base(value) {
        TokenType = TokenType.Number;

        if (Double.TryParse(value, out double result)) 
            Value = result;
        else {
            throw new InvalidCastException("Non valid format number");
        }
    }
    
}

public class Operator : Token {
    public char Shape { get; private set; }
    public int Priority { get; private set; }
    
    public int StrIndex { get; private set; }

    public static bool IsValidShape(String value) {
        return value == "+" || value == "-" || value == "*" || value == "/" || value == "(" || value == ")";
    }

    public Operator(String value, int index) : base(value) {
        TokenType = TokenType.Operator;

        StrIndex = index;
        
        if (!IsValidShape(value)) throw new InvalidConstraintException();

        Shape = value[0];
        
        switch (Shape) {
            case '+':
            case '-':
                Priority = 0;
                break;
            
            case '*':
            case '/':
                Priority = 1;
                break;
            case '(':
            case ')':
                Priority = 2;
                break;
        }
    }

    public double Evaluate(double v1, double v2) {
        switch (Shape) {
            case '+':
                return v1 + v2;
            case '-':
                return v1 - v2;
            case '*':
                return v1 * v2;
            case '/':
                return v1 / v2;
            case '(':
            case ')':
                throw new Exception("parentheses cannot be evaluated!");
            default:
                return 0;
        }
    }
}


public static class Program {

    static void Tokenize(String line, out List<string> tokens, out List<Operator> operators, out List<Number> numbers) {
        tokens = line.Split(' ').ToList();
        operators = new List<Operator>();
        numbers = new List<Number>();
        
        
        for (int i = 0; i < tokens.Count; i++) {
            String token = tokens[i];
            
            if(Operator.IsValidShape(token))
                operators.Add(new Operator(token,i));
            if (Double.TryParse(token,out double res))
                numbers.Add(new Number(token));
        }
    }
    
    static void Tokenize(in List<string> tokens, out List<Operator> operators, out List<Number> numbers) {
        operators = new List<Operator>();
        numbers = new List<Number>();
        
        
        for (int i = 0; i < tokens.Count; i++) {
            String token = tokens[i];
            
            if(Operator.IsValidShape(token))
                operators.Add(new Operator(token,i));
            if (Double.TryParse(token,out double res))
                numbers.Add(new Number(token));
        }
    }

    static void ScanParentheses(List<Operator> operators, out int openPar, out int closePar) {
        openPar = 0;
        closePar = 0;

        int parCtr = 0;
        int openParCount = 0;

        bool foundAny = false;
        bool firstIter = true;

        bool foundClose = false;
        for (int i = 0; i < operators.Count; i++) {
            Operator op = operators[i];

            if (op.Shape == '(') {
                parCtr++;
                openPar = i;
                foundAny = true;
            }
            if (op.Shape == ')') {
                parCtr--;
                if (!foundClose ) {
                    closePar = i;
                    foundClose = true;
                }
                foundAny = true;
            }

            if (parCtr == 0 && !firstIter && foundAny) {
                //closePar = i;
                break;
            }
            firstIter = false;
        }

        if (parCtr != 0) throw new Exception("Parenthesis do not match");
    }
    
    
    static double EvalStr(String line) {
        Tokenize(line, out List<string> tokens, out List<Operator> operators, out List<Number> numbers );
        
        while (operators.Count > 0) {
            int maxIndex = 0;
            bool openParFound = false; 
            int closeParIndex = 0;

            for (int i = 0; i < operators.Count; i++) {
                if (operators[i].Priority > operators[maxIndex].Priority)
                    maxIndex = i;
            }
            
            ScanParentheses(operators, out int openPar, out int closePar);

            if (openPar != closePar) {
                int startIndex = operators[openPar].StrIndex;
                int len = operators[closePar].StrIndex;
                string sub = String.Join(' ',tokens.ToArray()[(startIndex + 1)..len]);
                double subResult = EvalStr(sub);
                
                tokens.RemoveRange(startIndex,len - startIndex + 1);
                tokens.Insert(startIndex,Convert.ToString(subResult));
                
                Tokenize(tokens, out operators, out numbers);
            }
            else {
                Operator foundOp = operators[maxIndex];

                Number foundL = numbers[maxIndex];
                Number foundR = numbers[maxIndex + 1];

                double res = foundOp.Evaluate(foundL.Value, foundR.Value);

                operators.Remove(foundOp);
                numbers.Remove(foundL);
                numbers.Remove(foundR);

                numbers.Insert(maxIndex, new Number(Convert.ToString(res)));
            }
        }
        return numbers[0].Value;
    }
    
    public static void Main(string[] args) {

        if (args.Length == 0) return;

        int exprCount = Convert.ToInt32(args[0]);

        if (exprCount > args.Length - 1) return;
        
        for (int i = 0; i < exprCount; i++) {
            Console.WriteLine(EvalStr(args[i + 1]));
        }
    }
}