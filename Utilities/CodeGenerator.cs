using System;

namespace SelectionDemo.Utilities
{
   public static class CodeGenerator
   {
      private static readonly Random random = new Random();

      public static string GenerateStudentCode()
      {
         int currentYear = DateTime.Now.Year;
         int randomNumber = random.Next(100000, 999999);
         return $"{currentYear}{randomNumber}";
      }

      public static string GenerateTeacherCode()
      {
         int randomNumber = random.Next(10000, 99999);
         return $"CLG-420-{randomNumber}";
      }
   }
}
