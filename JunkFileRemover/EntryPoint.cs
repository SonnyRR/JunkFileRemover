namespace FileRemover
{
    using System;

    using FileRemover.Core;

    public class EntryPoint
    {
        public static void Main()
        {
            Console.Title = "File remover";
            
            Engine engine = new Engine();
            engine.Run();

        }
    }
}
