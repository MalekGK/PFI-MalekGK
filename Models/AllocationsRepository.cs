using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
   // Fait avec un exemple d'internet
   public class AllocationsRepository : Repository<Allocation>
   {
      private bool HasSameTeacherCourseYear(Allocation data)
      {
         return ToList().Any(a =>
            a.Id != data.Id &&
            a.TeacherId == data.TeacherId &&
            a.CourseId == data.CourseId &&
            a.Year == data.Year);
      }

      private bool HasOtherTeacherForCourseYear(Allocation data)
      {
         return ToList().Any(a =>
            a.Id != data.Id &&
            a.CourseId == data.CourseId &&
            a.Year == data.Year &&
            a.TeacherId != data.TeacherId);
      }

      public override int Add(Allocation data)
      {
         if (data == null)
            return 0;

         if (HasSameTeacherCourseYear(data) || HasOtherTeacherForCourseYear(data))
            return 0;

         return base.Add(data);
      }

      public override bool Update(Allocation data)
      {
         if (data == null)
            return false;

         if (HasSameTeacherCourseYear(data) || HasOtherTeacherForCourseYear(data))
            return false;

         return base.Update(data);
      }
   }
}