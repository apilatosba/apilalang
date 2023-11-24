using System.Collections.Generic;

namespace ApilaLang {
   /// <summary>
   /// A stack that can be indexed from the top
   /// </summary>
   internal class ApilaStack<T> {
      List<T> stack = new List<T>();

      public void Push(T value) {
         stack.Add(value);
      }

      public T Pop() {
         T value = stack[stack.Count - 1];
         stack.RemoveAt(stack.Count - 1);
         return value;
      }

      public T this[int i] {
         get => stack[stack.Count - 1 - i];
         set => stack[stack.Count - 1 - i] = value;
      }

      public T Peek() {
         return this[0];
      }
   }
}
