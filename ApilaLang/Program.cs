using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApilaLang {
   internal class Program {
      static ApilaStack<double> stack = new ApilaStack<double>();
      static string apilaFile;
      static string[] apilaCode;
      /// <summary>
      /// Label name, index pairs. The index is the index of the label in the apilaCode array.
      /// </summary>
      static Dictionary<string, int> labels = new Dictionary<string, int>();
      static List<Action> commandBuffer = new List<Action>();

      static void Main(string[] args) {
         // Command line arguments
         {
            if (args.Length == 0) {
               Console.WriteLine("Error: No file specified.");
               PrintHelp();
               return;
            }

            for (int i = 0; i < args.Length; i++) {
               if (args[i] == "-h" || args[i] == "--help") {
                  PrintHelp();
                  return;
               } else {
                  if (apilaFile != null) {
                     Console.WriteLine("Error: Too many arguments.");
                     PrintHelp();
                     return;
                  }

                  apilaFile = args[i];
               }
            }

            if (apilaFile == null) {
               Console.WriteLine("Error: No file specified.");
               PrintHelp();
               return;
            }

            if (!File.Exists(apilaFile)) {
               Console.WriteLine($"Error: File \"{Path.GetFullPath(apilaFile)}\" does not exist.");
               return;
            }
         }

         try {
            string rawContents = File.ReadAllText(apilaFile);
            Regex regex = new Regex(@"\s+");
            apilaCode = regex.Split(rawContents).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            //apilaCode = Array.FindAll(apilaCode, s => !string.IsNullOrEmpty(s));
            //apilaCode = rawContents.Split(new char[] { ' ', '\n' /*other white space characters go here*/}, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
         } catch (Exception e) {
            Console.WriteLine($"Error: {e.Message}");
            return;
         }

         for (int i = 0; i < apilaCode.Length; i++) {
            TokenType tokenType = FindTokenType(apilaCode[i]);

            switch (tokenType) {
               case TokenType.Unknown: {
                  // TODO: Print line and column number
                  Console.WriteLine($"Error: Unknown token \"{apilaCode[i]}\".");
                  return;
               }
               case TokenType.StackElement: {
                  stack.Push(double.Parse(apilaCode[i]));
                  break;
               }
               case TokenType.Operator: {
                  OperatorType operatorType = GetOperatorType(apilaCode[i]);

                  try {
                     switch (operatorType) {
                        case OperatorType.Add: {
                           stack.Push(stack.Pop() + stack.Pop());
                           break;
                        }
                        case OperatorType.Substract: {
                           double first = stack.Pop();
                           double second = stack.Pop();
                           stack.Push(second - first);
                           break;
                        }
                        case OperatorType.Multiply: {
                           stack.Push(stack.Pop() * stack.Pop());
                           break;
                        }
                        case OperatorType.Divide: {
                           double first = stack.Pop();
                           double second = stack.Pop();
                           stack.Push(second / first);
                           break;
                        }
                        case OperatorType.Print: {
                           Console.WriteLine(stack.Pop());
                           break;
                        }
                        case OperatorType.Switch: {
                           int firstIndex = (int)stack.Pop();
                           int secondIndex = (int)stack.Pop();

                           double first;
                           double second;

                           try {
                              first = stack[firstIndex];
                              second = stack[secondIndex];
                           } catch (ArgumentOutOfRangeException) {
                              // TODO: Print line and column number
                              Console.WriteLine($"Error: At least one of the specified indexes were out of range in \"switch\". The specified indexes: \"{firstIndex}\" and \"{secondIndex}\"");
                              return;
                           }

                           stack[firstIndex] = second;
                           stack[secondIndex] = first;

                           break;
                        }
                        case OperatorType.Duplicate: {
                           stack.Push(stack.Peek());
                           break;
                        }
                        case OperatorType.Goto: {
                           throw new NotImplementedException();
                           //string label = apilaCode[i + 1];
                           //int labelIndex = Array.FindIndex(apilaCode, i + 1, s => s == label);

                           //if (labelIndex == -1) { }
                           break;
                        }
                     }
                  } catch (InvalidOperationException) {
                     // TODO: Print line and column number
                     Console.WriteLine($"Error: Not enough elements in the stack to perform the operation \"{apilaCode[i]}\".");
                     return;
                  }

                  break;
               }
               case TokenType.Label: {
                  if (!labels.TryAdd(apilaCode[i], i)) {
                     Console.WriteLine($"Error: You have two labels with the same name. The label: \"{apilaCode[i]}\"");
                     return;
                  }

                  break;
               }
            }
         }
      }

      static void PrintHelp() {
         Console.WriteLine("Usage:");
         Console.WriteLine("  apila {file} [-h | --help]");
         Console.WriteLine();
         Console.WriteLine("file: The file that includes the source code.");
         Console.WriteLine("-h, --help: Print this help and exit.");
      }

      static TokenType FindTokenType(string word) {
         if (double.TryParse(word, out _)) {
            return TokenType.StackElement;
         } else if (Operator.operators.ContainsKey(word)) {
            return TokenType.Operator;
         } else if (word.EndsWith(":")) { 
            return TokenType.Label;
         } else {
            return TokenType.Unknown;
         }
      }

      static OperatorType GetOperatorType(string word) {
         return Operator.operators[word];
      }
   }
}
