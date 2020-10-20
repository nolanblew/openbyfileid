using System;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "";

            while (input != "e")
            {
                Console.WriteLine("Enter a Path or a FileSystemId and we will try to convert it (or 'e' to exit):");
                Console.Write(" >");
                input = Console.ReadLine().Trim(' ', '\n', '"');

                if (input == "e")
                {
                    break; 
                } else if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (long.TryParse(input, out var fileId))
                {
                    // It's a file ID. Let's try to convert it to a path.
                    var path = FileIdHelper.GetFilePath(fileId);
                    Console.WriteLine($"Your file path: {path}");
                } else
                {
                    var id = FileIdHelper.GetFileSystemId(input);
                    Console.WriteLine($"Your file id is: {id}");
                }
            }
        }
    }
}
