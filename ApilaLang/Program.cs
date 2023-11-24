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
      static HashSet<Label> labels = new HashSet<Label>();
      static List<Action> commandBuffer = new List<Action>();
      static int indexOfCommandToExecute = 0;

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

         // Read the file
         {
            try {
               string rawContents = File.ReadAllText(apilaFile);
               Regex regex = new Regex(@"\s+");
               apilaCode = regex.Split(rawContents).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            } catch (Exception e) {
               Console.WriteLine($"Error: {e.Message}");
               return;
            }
         }

         // Parse the file into commandBuffer
         {
            for (int i = 0; i < apilaCode.Length; i++) {
               TokenType tokenType = FindTokenType(apilaCode[i]);

               switch (tokenType) {
                  case TokenType.Unknown: {
                     int ii = i; /* This shouldnt matter. It is crazy. I dont even know, it may be a bug in programming language */

                     // TODO: Print line and column number
                     Console.WriteLine($"Error: Unknown token \"{apilaCode[ii]}\".");
                     return;
                  }
                  case TokenType.StackElement: {
                     int ii = i; /* This shouldnt matter. It is crazy. I dont even know, it may be a bug in programming language */
                     commandBuffer.Add(() => stack.Push(double.Parse(apilaCode[ii])));
                     break;
                  }
                  case TokenType.Operator: {
                     OperatorType operatorType = GetOperatorType(apilaCode[i]);

                     switch (operatorType) {
                        case OperatorType.Add: {
                           commandBuffer.Add(() => stack.Push(stack.Pop() + stack.Pop()));
                           break;
                        }
                        case OperatorType.Substract: {
                           commandBuffer.Add(() => {
                              double first = stack.Pop();
                              double second = stack.Pop();
                              stack.Push(second - first);
                           });
                           break;
                        }
                        case OperatorType.Multiply: {
                           commandBuffer.Add(() => stack.Push(stack.Pop() * stack.Pop()));
                           break;
                        }
                        case OperatorType.Divide: {
                           commandBuffer.Add(() => {
                              double first = stack.Pop();
                              double second = stack.Pop();
                              stack.Push(second / first);
                           });
                           break;
                        }
                        case OperatorType.Print: {
                           commandBuffer.Add(() => Console.WriteLine(stack.Pop()));
                           break;
                        }
                        case OperatorType.Switch: {
                           commandBuffer.Add(() => {
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
                                 Environment.Exit(1);
                                 return;
                              }

                              stack[firstIndex] = second;
                              stack[secondIndex] = first;
                           });
                           break;
                        }
                        case OperatorType.Duplicate: {
                           commandBuffer.Add(() => stack.Push(stack.Peek()));
                           break;
                        }
                        case OperatorType.Goto: {
                           int ii = i; /* This shouldnt matter. It is crazy. I dont even know, it may be a bug in programming language */

                           commandBuffer.Add(() => {
                              string label = apilaCode[ii + 1];
                              indexOfCommandToExecute = labels.Where(l => l.name == $"{label}:").First().commandIndex - 1; // Substract 1 because the for loop will add 1 to it
                           });

                           i++; // Skip the label name
                           break;
                        }
                        case OperatorType.Sleep: {
                           commandBuffer.Add(() => System.Threading.Thread.Sleep((int)stack.Pop()));
                           break;
                        }
                     }

                     break;
                  }
                  case TokenType.Label: {
                     if (labels.Where(l => l.name == apilaCode[i]).Count() > 0) {
                        Console.WriteLine($"Error: You have two labels with the same name. The label: \"{apilaCode[i]}\"");
                        return;
                     } else {
                        labels.Add(new Label(apilaCode[i], i, commandBuffer.Count));
                     }

                     break;
                  }
               }
            }
         }

         // Execute the commands
         {
            try {
               for (; indexOfCommandToExecute < commandBuffer.Count; indexOfCommandToExecute++) {
                  commandBuffer[indexOfCommandToExecute]();
               }
            } catch (Exception e) when (e is InvalidOperationException || e is ArgumentOutOfRangeException) {
               // TODO: Print line and column number
               Console.WriteLine($"Error: Not enough elements in the stack to perform the operation.");
               return;
            } catch (Exception e) {
               Console.WriteLine($"Error: {e.Message}");
               return;
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
