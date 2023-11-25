namespace ApilaLang {
   internal class CurlyBrace {
      /// <summary>
      /// Closing or opening
      /// </summary>
      public TokenType type;
      public int tokenIndex;
      public int commandIndex;
      public CurlyBrace matchingCurlyBrace;

      public CurlyBrace(TokenType type, int tokenIndex, int commandIndex) {
         this.type = type;
         this.tokenIndex = tokenIndex;
         this.commandIndex = commandIndex;
      }
   }
}
