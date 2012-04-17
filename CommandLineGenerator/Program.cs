using System;
using BFSchema;
using BFSchema.CodeGenerators.CSharp;

namespace CommandLineGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
			//Command line code generator 

            if (args.Length == 0)
            {
                Console.WriteLine("Error: No input schema...");
                return;
            }

            ConsoleErrorHandler errorHandler = new ConsoleErrorHandler();
            BinaryFileSchema schema = new BinaryFileSchema(args[0], errorHandler);
            if (schema == null || errorHandler.GotError)
            {
                Console.WriteLine("Could not generate C# code because of schema errors!");
                return;
            }
            CSharpGenerator csgenerator = new CSharpGenerator();
            string code = csgenerator.GenerateCode(schema);
            Console.WriteLine(code);
        }
    }
}
