using System.Collections.Generic;

namespace ApilaLang {
   internal static class Operator {
      /// <summary>
      /// Keyword, OperatorType pairs
      /// </summary>
      public static Dictionary<string, OperatorType> operators = new Dictionary<string, OperatorType>() {
         { "+", OperatorType.Add },
         { "-", OperatorType.Substract },
         { "*", OperatorType.Multiply },
         { "/", OperatorType.Divide },
         { "print", OperatorType.Print },
         { "switch", OperatorType.Switch },
         { "dup", OperatorType.Duplicate },
         { "goto", OperatorType.Goto },
         { "sleep", OperatorType.Sleep },
         { "if", OperatorType.If },
         { "=", OperatorType.Equals },
         { "<", OperatorType.LessThan },
         { ">", OperatorType.GreaterThan },
         { "!", OperatorType.Not },
         { "drop", OperatorType.Drop },
         { "%", OperatorType.Modulo },
      };
   }

   internal enum OperatorType {
      Add,
      Substract,
      Multiply,
      Divide,
      Print, /* Prints with the trailing new line */
      Switch, /* Switches two elements in the stack with given indexes */
      Duplicate, /* Duplicates the top element in the stack */
      Goto, /* Jumps to the specified label. Label is specified AFTER the goto keyword */
      Sleep, /* Sleeps for the specified amount of milliseconds */
      If, /* Curly braces */
      Equals, /* Checks if the top two elements in the stack are equal */
      LessThan, /* Checks if the second element is less than top element */
      GreaterThan, /* Checks if the second element is greater than top element */
      Not, /* Negates the top element in the stack */
      Drop, /* Removes the top element in the stack */
      Modulo,
   }
}
