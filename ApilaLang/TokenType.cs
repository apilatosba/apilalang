namespace ApilaLang {
   internal enum TokenType {
      Unknown,
      StackElement,
      Operator,
      Label,
      OpenCurlyBrace,
      CloseCurlyBrace,
      Else,
      ProcedureDefinition,
      ProcedureCall,
   }

   internal partial class Program {
      static TokenType FindTokenType(string word) {
         if (double.TryParse(word, out _)) {
            return TokenType.StackElement;
         } else if (Operator.operators.ContainsKey(word)) {
            return TokenType.Operator;
         } else if (word.EndsWith(':')) {
            return TokenType.Label;
         } else if(word == "{") {
            return TokenType.OpenCurlyBrace;
         } else if(word == "}") {
            return TokenType.CloseCurlyBrace;
         } else if(word == "else") {
            return TokenType.Else;
         } else if (word.StartsWith('.')) {
            return TokenType.ProcedureDefinition;
         } else if (procedures.ContainsKey(word)) {
            return TokenType.ProcedureCall;
         } else {
            return TokenType.Unknown;
         }
      }
   }
}
