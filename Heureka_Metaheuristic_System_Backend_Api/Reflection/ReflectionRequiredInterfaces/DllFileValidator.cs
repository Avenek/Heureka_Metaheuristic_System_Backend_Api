using Heureka_Metaheuristic_System_Backend_Api.Exceptions;

namespace Heureka_Metaheuristic_System_Backend_Api.Reflection.ReflectionRequiredInterfaces
{
    public static class DllFileValidator
    {
        public static void ValidateFile(IFormFile file, string filePath)
        {
            if (file == null || file.Name.Length == 0)
            {
                throw new BadRequestException("File is empty.");
            }
            if (Path.GetExtension(file.FileName) != ".dll")
            {
                throw new BadRequestException("File has an invalid extension.");
            }
            if (File.Exists(filePath))
            {
                throw new BadRequestException("A file with the same name already exists on the server.");
            }
        }
    }
}
