using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DAL;
using SelectionDemo.Models;

namespace Models
{
   public class TeachersRepository : Repository<Teacher>
   {
      public override int Add(Teacher data)
      {
         if (string.IsNullOrEmpty(data.Code))
         {
            data.GenerateCode();


            // Pour assurer un code unique
            while (ToList().Any(t => t.Code == data.Code))
            {
               data.GenerateCode();
            }
         }
         return base.Add(data);
      }

      public bool Update(Teacher data, List<int> selectedCoursesId)
      {
         try
         {
            BeginTransaction();
            if (base.Update(data))
            {
               data.UpdateAllocations(selectedCoursesId);
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

      public override bool Delete(int teacherId)
      {
         try
         {
            Teacher teacherToDelete = global::DAL.DB.Teachers.Get(teacherId);
            if (teacherToDelete != null)
            {
               BeginTransaction();
               teacherToDelete.DeleteAllAllocations();
               base.Delete(teacherId);
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