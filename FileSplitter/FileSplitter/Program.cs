using MetadataExtractor;
using System.Globalization;

internal class Program
{
    private static void Main(string[] args)
    {
        int NumberOfFilesToCopy = 0;

        Console.WriteLine("FileSplitter 2023 made by Aaron Coltof");
        string currentDirectory = System.IO.Directory.GetCurrentDirectory();
        IEnumerable<string> filePaths = System.IO.Directory.GetFiles($@"{currentDirectory}", "*.jpg");
        HashSet<string> extensions = new HashSet<string>();

        //delete the FileSplitter.exe from the filepaths
        string pathToRemove = $"{currentDirectory}\\FileSplitter.exe";
        filePaths = filePaths.Where(val => val != pathToRemove);
        string outputFolder = $"{currentDirectory}\\output";
        System.IO.Directory.CreateDirectory(outputFolder);

        foreach (string filePath in filePaths)
        {
            string extension = Path.GetExtension(filePath);
            if (!extensions.Contains(extension))
            {
                extensions.Add(extension);
            }

            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);
            var tag = directories.FirstOrDefault(d => d.Name == "Exif IFD0")?.Tags.FirstOrDefault(t => t.Name == "Date/Time");
            if (tag != null && tag.Description != null)
            {
                NumberOfFilesToCopy++;
            }
        }

        Console.WriteLine($"{NumberOfFilesToCopy} files will be affected, including the types {string.Join(",", extensions)}");
        Console.WriteLine("Press a key to continue");
        Console.ReadLine();

        foreach (string filePath in filePaths) 
        {
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);
            var tag = directories.FirstOrDefault(d => d.Name == "Exif IFD0")?.Tags.FirstOrDefault(t => t.Name == "Date/Time");
            if (tag != null && tag.Description != null)
            {
                DateTime dateTaken = DateTime.ParseExact(tag.Description, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                Console.WriteLine($"{tag.DirectoryName} - {tag.Name} = {tag.Description}:{dateTaken:o}");

                var destinationFolder = $"{outputFolder}\\{dateTaken.Year:0000} {dateTaken.Month:00} {dateTaken.Day:00}";
                System.IO.Directory.CreateDirectory(destinationFolder);

                File.Move(filePath, Path.Combine(destinationFolder, Path.GetFileName(filePath)), true);
            }
        }

        Console.WriteLine("Done, results can be found in the output directory");
        Console.WriteLine("Files that didn't have a time tag on them will stay where they were");
        Console.WriteLine("Press enter to exit the program");

        Console.ReadLine();
    }
}