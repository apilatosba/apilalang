namespace ApilaLang {
   internal class Label {
      public string name;
      public int tokenIndex;
      /// <summary>
      /// The index of the command, in the commandBuffer, that the label points to ie. the command that comes after the label.
      /// </summary>
      public int commandIndex;

      public Label(string name, int tokenIndex, int commandIndex) {
         this.name = name;
         this.tokenIndex = tokenIndex;
         this.commandIndex = commandIndex;
      }
   }
}
