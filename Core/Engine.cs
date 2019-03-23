namespace FileRemover.Core
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using FileRemover.Common;

    public class Engine
    {

        private const string folderRegexPattern = @"^.?[A-Za-z]+/$";
        private const string fileRegexPattern = @"^.?[A-Za-z]+\.[A-Za-z]+$";

        private const string defaultArgsFilePath = "what_to_delete.txt";

        public Engine()
        {

        }

        public void Run()
        {
            Console.Write(StringConstants.INSERT_START_PATH);
            string pathToTextFile = GetPathToArgumentsFile();

            Console.WriteLine();

            Console.Write("Enter path from where to start searching: ");
            string pathToStartFrom = GetParentDirectory();

            bool searchCase = GetSearchCase();


            IEnumerable<string> filesAndDirectoriesToRemove
                = GetFilesAndDirectoriesToDelete(pathToTextFile);

            if (searchCase == false)
            {
                filesAndDirectoriesToRemove = filesAndDirectoriesToRemove
                    .Select(x => x.ToLower()).ToList();
            }


            while (true)
            {
                try
                {
                    var totalRemoved = DirTraverseRecursive(pathToStartFrom, filesAndDirectoriesToRemove, searchCase);

                    Console.WriteLine();
                    Console.WriteLine($"Total removed directories: {totalRemoved.removedDirectories}");
                    Console.WriteLine($"Total removed files: {totalRemoved.removedFiles}");
                    break;
                }

                catch (IOException ex)
                {
                    Console.WriteLine();
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                    Console.Write("Close all programs that use this file and press Y to try again or N to exit: ");

                    while (true)
                    {
                        var input = Console.ReadKey(true);
                        Console.WriteLine();

                        if (input.Key == ConsoleKey.N)
                            Environment.Exit(0);

                        else if (input.Key == ConsoleKey.Y)
                        {
                            break;
                        }
                        else
                        {
                            Console.Write("Invalid key! Press Y or N!");
                        }
                    }
                }
            }

            Console.Write("Press any key to exit!");

            Console.ReadKey();
            Environment.Exit(0);
        }

        private string GetPathToArgumentsFile()
        {
            bool shouldUseDefaultFile = false;

            string pathToTextFile = Console.ReadLine();

            while (true)
            {

                if (string.IsNullOrWhiteSpace(pathToTextFile))
                {
                    shouldUseDefaultFile = true;

                    break;
                }

                try
                {
                    CheckDoesFileExist(pathToTextFile);
                    break;
                }

                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.Write("Enter valid path or type EXIT: ");
                pathToTextFile = Console.ReadLine();

                CheckIfUserWantsToExit(pathToTextFile);
            }

            if (shouldUseDefaultFile)
            {
                pathToTextFile = GetDefaultFile();
            }

            return pathToTextFile;
        }

        private (int removedDirectories, int removedFiles) DirTraverseRecursive
            (string path, IEnumerable<string> argumentsToRemove, bool @case)
        {

            (int dirs, int files) removed = (0, 0);

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] files = dirInfo.GetFiles();
            DirectoryInfo[] directories = dirInfo.GetDirectories();

            foreach (var file in files)
            {
                var currentFilePath = file.FullName;
                var currentFileName = @case == false ? file.Name.ToLower() : file.Name;

                if (argumentsToRemove.Contains(currentFileName))
                {
                    File.Delete(currentFilePath);
                    removed.files++;
                }
            }

            foreach (var dir in directories)
            {
                var currentDirPath = dir.FullName;
                var currentDirName = @case == false ? dir.Name.ToLower() : dir.Name;

                if (argumentsToRemove.Contains($"{currentDirName}/"))
                {
                    Directory.Delete(currentDirPath, true);
                    removed.dirs++;
                }

                else
                {
                    var innerTuple = DirTraverseRecursive(currentDirPath, argumentsToRemove, @case);

                    removed.dirs += innerTuple.removedDirectories;
                    removed.files += innerTuple.removedFiles;
                }
            }

            return removed;
        }

        private bool GetSearchCase()
        {

            bool @case;

            while (true)
            {
                Console.Write("Case [ 0 - Insensitive | 1 - Sensitive ]: ");

                string temp = Console.ReadLine();

                if (temp == "0")
                {
                    @case = false;
                    break;
                }

                else if (temp == "1")
                {
                    @case = true;
                    break;
                }

                else
                {
                    Console.WriteLine("Please choose a valid argument!");
                }
            }

            return @case;
        }

        private string GetParentDirectory()
        {
            string input = Console.ReadLine();

            while (!Directory.Exists(input))
            {
                Console.WriteLine(StringConstants.DIRECTORY_DOES_NOT_EXIST);
                Console.Write("Please input a valid directory path or type EXIT: ");

                input = Console.ReadLine();

                CheckIfUserWantsToExit(input);
            }

            return input;
        }

        private void CheckDoesFileExist(string path)
        {
            if (File.Exists(path) == false)
            {
                throw new InvalidOperationException(StringConstants.FILE_DOES_NOT_EXIST);
            }
        }

        private IEnumerable<string> GetFilesAndDirectoriesToDelete(string pathToTextFile)
        {

            StreamReader reader = new StreamReader(pathToTextFile);

            List<string> output = new List<string>();

            using (reader)
            {
                string currentLine = reader.ReadLine();

                while (currentLine != null)
                {

                    if (Regex.IsMatch(currentLine, folderRegexPattern)
                        || Regex.IsMatch(currentLine, fileRegexPattern))
                    {
                        output.Add(currentLine);
                    }

                    currentLine = reader.ReadLine();
                }
            }

            return output;
        }

        private void CheckIfUserWantsToExit(string input)
        {
            if (input == "EXIT")
                Environment.Exit(0);
        }

        private string GetDefaultFile()
        {
            string path = string.Empty;

            try
            {
                CheckDoesFileExist(defaultArgsFilePath);
                path = defaultArgsFilePath;
            }
            catch (InvalidOperationException ex)
            {

                Console.WriteLine("Default file (what_to_remove.txt) not found! Generating default file...");

                GenerateDefaultFile(defaultArgsFilePath);
                path = defaultArgsFilePath;
            }

            return path;
        }

        private void GenerateDefaultFile(string path)
        {
            StreamWriter fileConsole = new StreamWriter(path, false);

            using (fileConsole)
            {

                fileConsole.WriteLine(@"// Here you should write which directories and files to remove. //");
                fileConsole.WriteLine(@"//                                                              //");
                fileConsole.WriteLine(@"//                  Use // to comment a line                    //");
                fileConsole.WriteLine(@"//                                                              //");
                fileConsole.WriteLine(@"//              This is an autogenerated blacklist.             //");
                fileConsole.WriteLine(@"//                                                              //");
                fileConsole.WriteLine(@"//////////////////////////////////////////////////////////////////");
                fileConsole.WriteLine(Environment.NewLine);
                fileConsole.WriteLine("bin/");
                fileConsole.WriteLine("obj/");
                fileConsole.WriteLine(".vs/");
            }
        }
    }
}
