namespace ApilaLang {
   internal class Else {
      public int indexOfPrecedingCurlyBrace;
      public int indexOfFollowingCurlyBrace;
      public int tokenIndex;

      public Else(int indexOfPrecedingCurlyBrace, int indexOfFollowingCurlyBrace, int tokenIndex) {
         this.indexOfPrecedingCurlyBrace = indexOfPrecedingCurlyBrace;
         this.indexOfFollowingCurlyBrace = indexOfFollowingCurlyBrace;
         this.tokenIndex = tokenIndex;
      }
   }
}
