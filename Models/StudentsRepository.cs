using System;
using DAL;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
   public class StudentsRepository : Repository<Student>
   {
      public override int Add(Student data)
      {
         if (string.IsNullOrEmpty(data.Code))
         {
            data.GenerateCode();

            // Pour assurer un code unique
            while (ToList().Any(s => s.Code == data.Code))
            {
               data.GenerateCode();
            }
         }
         return base.Add(data);
      }

      public bool Update(Student data, List<int> selectedCoursesId)
      {
         try
         {
            BeginTransaction();
            if (base.Update(data))
            {
               data.UpdateRegistrations(selectedCoursesId);
               EndTransaction();
               return true;
            }
            EndTransaction();
            return false;
         }
         catch
         {
            EndTransaction();
            return false;
         }
      }

      public override bool Delete(int studentId)
      {
         try
         {
            Student studentToDelete = DB.Students.Get(studentId);
            if (studentToDelete != null)
            {
               BeginTransaction();
               studentToDelete.DeleteAllRegistrations();
               base.Delete(studentId);
               EndTransaction();
               return true;
            }
            return false;
         }
         catch
         {
            EndTransaction();
            return false;
         }
      }
   }
}