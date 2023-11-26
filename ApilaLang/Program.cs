using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApilaLang {
   internal unsafe partial class Program {
      static ApilaStack<double> stack = new ApilaStack<double>();
      static string apilaFile;
      static string[] apilaCode;
      /// <summary>
      /// Label name, index pairs. The index is the index of the label in the apilaCode array.
      /// </summary>
      static HashSet<Label> labels = new HashSet<Label>();
      static List<CurlyBrace> curlyBraces = new List<CurlyBrace>();
      static HashSet<Else> elses = new HashSet<Else>();
      /// <summary>
      /// Name of the procedure is stored without the leading dot
      /// </summary>
      static Dictionary<string, List<Action>> procedures = new Dictionary<string, List<Action>>();
      /// <summary>
      /// The key is the index of the open curly brace and the value is the index of the close curly brace.
      /// This buffer may be unnecessary. I am not sure.
      /// </summary>
      //static Dictionary<CurlyBrace, CurlyBrace> curlyBracesOfIfs = new Dictionary<CurlyBrace, CurlyBrace>();
      static List<Action> commandBuffer = new List<Action>();
      static List<Action> procedureBuffer = new List<Action>();
      static List<Action>* commandBufferToWriteTo;
      static List<Action> commandBufferToWriteTo2;
      static int indexOfCommandToExecute = 0;
      static bool shouldExecuteElseBlock;

      static void Main(string[] args) {
         fixed(List<Action>* ptr = &commandBuffer) {
            commandBufferToWriteTo = ptr;
         }

         commandBufferToWriteTo2 = commandBuffer;

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

         // Doing one iteration just to to match braces.
         {
            for(int i = 0; i < apilaCode.Length; i++) {
               TokenType tokenType = FindTokenType(apilaCode[i]);

               switch (tokenType) {
                  case TokenType.OpenCurlyBrace: {
                     curlyBraces.Add(new CurlyBrace(TokenType.OpenCurlyBrace, i, commandBuffer.Count));
                     break;
                  }
                  case TokenType.CloseCurlyBrace: {
                     curlyBraces.Add(new CurlyBrace(TokenType.CloseCurlyBrace, i, commandBuffer.Count));
                     break;
                  }
                  case TokenType.Else: {
                     if (apilaCode[i - 1] != "}") {
                        Console.WriteLine("Error: The keyword else must have been preceded by \"}\"");
                        return;
                     } else if (apilaCode[i + 1] != "{") {
                        Console.WriteLine("Error: The keyword else must have been succeeded by \"{\"");
                        return;
                     } else {
                        int ii = curlyBraces.Count; /* Maybe this ii thingy is not be a bug. I have to think about it */

                        elses.Add(new Else(ii - 1, ii, i)); // Putting i here is no problem because it is executed directly here but not after in the commandBuffer

                        commandBufferToWriteTo2.Add(() => {
                           if (shouldExecuteElseBlock) {
                              //indexOfCommandToExecute = curlyBraces[ii].commandIndex;
                              // Or do nothing which does the same thing. Both of them continue the execution without jumping.
                           } else {
                              indexOfCommandToExecute = curlyBraces[ii].matchingCurlyBrace.commandIndex - 1; // Substract 1 because the for loop will add 1 to it
                           }
                        });
                     }
                     break;
                  }
               }
            }
         }

         // Match the curly braces
         {
            for (int i = 0; i < curlyBraces.Count; i++) {
               if (curlyBraces[i].type == TokenType.OpenCurlyBrace) {
                  int matchingIndex;
                  try {
                     matchingIndex = FindCurlyBracePair(i, curlyBraces);
                  } catch (KeyNotFoundException) {
                     // TODO: Print line and column number
                     Console.WriteLine("Error: You are missing curly braces. The program was unable to find one of the matching curly brace pair.");
                     return;
                  }
                  CurlyBrace matchingCurlyBrace = curlyBraces[matchingIndex];

                  curlyBraces[i].matchingCurlyBrace = matchingCurlyBrace;
                  matchingCurlyBrace.matchingCurlyBrace = curlyBraces[i];
               } else if (curlyBraces[i].type == TokenType.CloseCurlyBrace) {
                  if (curlyBraces[i].matchingCurlyBrace == null) {
                     Console.WriteLine("Error: You are missing curly braces. The program was unable to find one of the matching curly brace pair.");
                     return;
                  }
               } else {
                  Debug.Assert(false, "The curly brace is not a curly brace");
               }
            }
         }

         // Check the elses whether they are in correct syntax. Half of the check is done in the parsing phase.
         {
            foreach (Else e in elses) {
               if (!(apilaCode[curlyBraces[e.indexOfPrecedingCurlyBrace].matchingCurlyBrace.tokenIndex - 1] == Operator.operators.First(pair => pair.Value == OperatorType.If).Key /* Assumes that only one keyword exist per operator */)) {
                  // TODO: Print line and column number
                  Console.WriteLine("Error: Where is the if of your else lil bro");
                  return;
               }
            }
         }

         // Parse the file into commandBuffer
         {
            for (int i = 0; i < apilaCode.Length; i++) {
               TokenType tokenType = FindTokenType(apilaCode[i]);

               switch (tokenType) {
                  case TokenType.Unknown: {
                     // TODO: Print line and column number
                     Console.WriteLine($"Error: Unknown token \"{apilaCode[i]}\".");
                     return;
                  }
                  case TokenType.StackElement: {
                     int ii = i; /* This shouldnt matter. It is crazy. I dont even know, it may be a bug in programming language */
                     commandBufferToWriteTo2.Add(() => stack.Push(double.Parse(apilaCode[ii])));
                     break;
                  }
                  case TokenType.Operator: {
                     OperatorType operatorType = GetOperatorType(apilaCode[i]);

                     switch (operatorType) {
                        case OperatorType.Add: {
                           commandBufferToWriteTo2.Add(() => stack.Push(stack.Pop() + stack.Pop()));
                           break;
                        }
                        case OperatorType.Substract: {
                           commandBufferToWriteTo2.Add(() => {
                              double first = stack.Pop();
                              double second = stack.Pop();
                              stack.Push(second - first);
                           });
                           break;
                        }
                        case OperatorType.Multiply: {
                           commandBufferToWriteTo2.Add(() => stack.Push(stack.Pop() * stack.Pop()));
                           break;
                        }
                        case OperatorType.Divide: {
                           commandBufferToWriteTo2.Add(() => {
                              double first = stack.Pop();
                              double second = stack.Pop();
                              stack.Push(second / first);
                           });
                           break;
                        }
                        case OperatorType.Print: {
                           commandBufferToWriteTo2.Add(() => Console.WriteLine(stack.Pop()));
                           break;
                        }
                        case OperatorType.Switch: {
                           commandBufferToWriteTo2.Add(() => {
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
                           commandBufferToWriteTo2.Add(() => stack.Push(stack.Peek()));
                           break;
                        }
                        case OperatorType.Goto: {
                           int ii = i; /* This shouldnt matter. It is crazy. I dont even know, it may be a bug in programming language */

                           commandBufferToWriteTo2.Add(() => {
                              string label = apilaCode[ii + 1];
                              indexOfCommandToExecute = labels.Where(l => l.name == $"{label}:").First().commandIndex - 1; // Substract 1 because the for loop will add 1 to it
                           });

                           i++; // Skip the label name
                           break;
                        }
                        case OperatorType.Sleep: {
                           commandBufferToWriteTo2.Add(() => System.Threading.Thread.Sleep((int)stack.Pop()));
                           break;
                        }
                        case OperatorType.If: {
                           if (apilaCode[i + 1] != "{") {
                              Console.WriteLine("Error: The keyword if must have been followed by \"{\"");
                              Environment.Exit(1);
                           } else {
                              int ii = curlyBraces.Count; /* Maybe this ii thingy is not be a bug. I have to think about it */

                              commandBufferToWriteTo2.Add(() => {
                                 double top = stack.Pop();

                                 if (top == 0) {
                                    indexOfCommandToExecute = curlyBraces[ii].matchingCurlyBrace.commandIndex - 1; // Substract 1 because the for loop will add 1 to it
                                    shouldExecuteElseBlock = true;
                                 } else {
                                    //indexOfCommandToExecute = curlyBraces[ii].commandIndex;
                                    // Or do nothing which does the same thing. Both of them continue the execution without jumping.
                                    shouldExecuteElseBlock = false;
                                 }
                              });
                           }
                           break;
                        }
                        case OperatorType.Equals: {
                           commandBufferToWriteTo2.Add(() => stack.Push(stack.Pop() == stack.Pop() ? 1 : 0));
                           break;
                        }
                        case OperatorType.LessThan: {
                           commandBufferToWriteTo2.Add(() => {
                              double first = stack.Pop();
                              double second = stack.Pop();
                              stack.Push(second < first ? 1 : 0);
                           });
                           break;
                        }
                        case OperatorType.GreaterThan: {
                           commandBufferToWriteTo2.Add(() => {
                              double first = stack.Pop();
                              double second = stack.Pop();
                              stack.Push(second > first ? 1 : 0);
                           });
                           break;
                        }
                        case OperatorType.Not: {
                           commandBufferToWriteTo2.Add(() => stack.Push(stack.Pop() == 0 ? 1 : 0));
                           break;
                        }
                        case OperatorType.Drop: {
                           commandBufferToWriteTo2.Add(() => stack.Pop());
                           break;
                        }
                     }
                     break;
                  }
                  case TokenType.Label: {
                     if (labels.Where(l => l.name == apilaCode[i]).Any()) {
                        Console.WriteLine($"Error: You have two labels with the same name. The label: \"{apilaCode[i]}\"");
                        return;
                     } else {
                        labels.Add(new Label(apilaCode[i], i, commandBuffer.Count));
                     }

                     break;
                  }
                  //case TokenType.OpenCurlyBrace: {
                  //   curlyBraces.Add(new CurlyBrace(TokenType.OpenCurlyBrace, i, commandBuffer.Count));
                  //   break;
                  //}
                  case TokenType.CloseCurlyBrace: {
                     curlyBraces.Add(new CurlyBrace(TokenType.CloseCurlyBrace, i, commandBuffer.Count));

                     int ii = curlyBraces.Count;

                     //fixed (List<Action>* ptr2 = &commandBuffer) {
                     //   fixed (List<Action>* ptr = &procedureBuffer) {
                     //      if(commandBufferToWriteTo == ptr) {
                     //         if (FindTokenType(apilaCode[curlyBraces[ii - 1].matchingCurlyBrace.tokenIndex - 1]) is TokenType.ProcedureDefinition) {
                     //            procedures[apilaCode[curlyBraces[ii - 1].matchingCurlyBrace.tokenIndex - 1].Substring(1)] = procedureBuffer;
                     //            commandBufferToWriteTo = ptr2;
                     //         } 
                     //      }
                     //   }
                     //}

                     if(commandBufferToWriteTo2 == procedureBuffer) {
                        if(FindTokenType(apilaCode[curlyBraces[ii - 1].matchingCurlyBrace.tokenIndex - 1]) is TokenType.ProcedureDefinition) {
                           procedures[apilaCode[curlyBraces[ii - 1].matchingCurlyBrace.tokenIndex - 1].Substring(1)] = procedureBuffer;
                           procedureBuffer.Clear();
                           commandBufferToWriteTo2 = commandBuffer;
                        }
                     }
                     break;
                  }
                  //case TokenType.Else: {
                  //   if (apilaCode[i - 1] != "}") {
                  //      Console.WriteLine("Error: The keyword else must have been preceded by \"}\"");
                  //      return;
                  //   } else if (apilaCode[i + 1] != "{") {
                  //      Console.WriteLine("Error: The keyword else must have been succeeded by \"{\"");
                  //      return;
                  //   } else {
                  //      int ii = curlyBraces.Count; /* Maybe this ii thingy is not be a bug. I have to think about it */

                  //      elses.Add(new Else(ii - 1, ii, i)); // Putting i here is no problem because it is executed directly here but not after in the commandBuffer

                  //      commandBufferToWriteTo2.Add(() => {
                  //         if (shouldExecuteElseBlock) {
                  //            //indexOfCommandToExecute = curlyBraces[ii].commandIndex;
                  //            // Or do nothing which does the same thing. Both of them continue the execution without jumping.
                  //         } else {
                  //            indexOfCommandToExecute = curlyBraces[ii].matchingCurlyBrace.commandIndex - 1; // Substract 1 because the for loop will add 1 to it
                  //         }
                  //      });
                  //   }
                  //   break;
                  //}
                  case TokenType.ProcedureDefinition: {
                     string procedureName = apilaCode[i].Substring(1);

                     if (procedures.ContainsKey(procedureName)) {
                        Console.WriteLine($"Error: You have two procedure definitons with the same name. The procedure: \"{apilaCode[i]}\"");
                        return;
                     } else if (apilaCode[i + 1] != "{") {
                        Console.WriteLine("Error: The procedure definition must be within a scope so it should start with \"{\"");
                        return;
                     } else {
                        procedures.Add(procedureName, null /* Will be assigned once i hit the closing bracket */);

                        commandBufferToWriteTo2.Add(() => {
                           fixed (List<Action>* ptr = &procedureBuffer) {
                              commandBufferToWriteTo = ptr;
                           }

                           commandBufferToWriteTo2 = procedureBuffer;
                        });
                     }
                     break;
                  }
                  case TokenType.ProcedureCall: {
                     int ii = i;

                     commandBufferToWriteTo2.Add(() => {
                        List<Action> actions = procedures[apilaCode[ii]];

                        for (int j = 0; j < actions.Count; j++) {
                           actions[j]();
                        }
                     });
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
               Console.WriteLine($"   indexOfCommandToExecute = {indexOfCommandToExecute}");
               return;
            }
         }
      }

      static void PrintHelp() {
         Console.WriteLine("Usage:");
         Console.WriteLine("   apila {file} [-h | --help]");
         Console.WriteLine();
         Console.WriteLine("file: The file that includes the source code.");
         Console.WriteLine("-h, --help: Print this help and exit.");
      }

      static OperatorType GetOperatorType(string word) {
         return Operator.operators[word];
      }

      /// <summary>
      /// You give the index of the first curly brace and it returns the index of the matching curly brace.
      /// </summary>
      static int FindCurlyBracePair(int index, List<CurlyBrace> curlyBraces) {
         CurlyBrace curlyBrace = curlyBraces[index];

         switch (curlyBrace.type) {
            case TokenType.OpenCurlyBrace: {
               int curlyBraceCount = 1;

               for (int i = index + 1; i < curlyBraces.Count; i++) {
                  if (curlyBraces[i].type == TokenType.OpenCurlyBrace) {
                     curlyBraceCount++;
                  } else if (curlyBraces[i].type == TokenType.CloseCurlyBrace) {
                     curlyBraceCount--;
                  }

                  if (curlyBraceCount == 0) {
                     return i;
                  }
               }

               throw new KeyNotFoundException("No matching curly brace found.");
            }
            case TokenType.CloseCurlyBrace: {
               int curlyBraceCount = 1;

               for (int i = index - 1; i >= 0; i--) {
                  if (curlyBraces[i].type == TokenType.OpenCurlyBrace) {
                     curlyBraceCount--;
                  } else if (curlyBraces[i].type == TokenType.CloseCurlyBrace) {
                     curlyBraceCount++;
                  }

                  if (curlyBraceCount == 0) {
                     return i;
                  }
               }

               throw new KeyNotFoundException("No matching curly brace found.");
            }
            default: {
               Debug.Assert(false, "The curly brace is not a curly brace");
               return -1;
            }
         }
      }
   }
}
