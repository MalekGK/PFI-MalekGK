using DAL;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Models
{
   public class Course : Record
   {
      public string Code { get; set; }
      public string Title { get; set; }
      public int Session { get; set; }

      [JsonIgnore]
      public string Caption => "[" + Session + "] " + Code + " " + Title;

      [JsonIgnore]
      public string ShortCaption => Code + " " + Title;

      [JsonIgnore]
      public List<Registration> Registrations => DB.Registrations.ToList()
         .Where(r => r.CourseId == Id)
         .OrderByDescending(r => r.Year)
         .ThenBy(r => r.Student.Code)
         .ToList();

      [JsonIgnore]
      public List<Registration> NextSessionRegistrations => DB.Registrations.ToList()
         .Where(r => r.CourseId == Id && r.IsNextSession)
         .OrderBy(r => r.Student.Code)
         .ToList();

      [JsonIgnore]
      public List<Allocation> Allocations => DB.Allocations.ToList()
         .Where(a => a.CourseId == Id)
         .OrderByDescending(a => a.Year)
         .ThenBy(a => a.Teacher.LastName)
         .ToList();

      [JsonIgnore]
      public List<Student> Students
      {
         get
         {
            List<Student> students = new List<Student>();
            foreach (var registration in Registrations)
               students.Add(registration.Student);
            return students;
         }
      }

      [JsonIgnore]
      public List<Student> NextSessionStudents
      {
         get
         {
            List<Student> students = new List<Student>();
            foreach (var registration in NextSessionRegistrations)
               students.Add(registration.Student);
            return students;
         }
      }

      [JsonIgnore]
      public SelectList StudentsSelectList => SelectListUtilities<Student>.Convert(Students, "Caption");

      [JsonIgnore]
      public SelectList NextSessionStudentsToSelectList => SelectListUtilities<Student>.Convert(NextSessionStudents, "Caption");

      public Teacher TeacherByYear(int year)
      {
         var allocation = Allocations.Where(a => a.Year == year).FirstOrDefault();
         if (allocation != null) return allocation.Teacher;
         return null;
      }

      public void DeleteAllRegistrations()
      {
         foreach (var registration in Registrations)
            DB.Registrations.Delete(registration.Id);
      }

      public void DeleteNextSessionRegistrations()
      {
         foreach (var registration in NextSessionRegistrations)
            DB.Registrations.Delete(registration.Id);
      }

      public void DeleteAllAllocations()
      {
         foreach (var allocation in Allocations)
            DB.Allocations.Delete(allocation.Id);
      }

      public void UpdateRegistrations(List<int> selectedStudentsId)
      {
         DeleteNextSessionRegistrations();
         if (selectedStudentsId != null)
         {
            foreach (int studentId in selectedStudentsId)
               DB.Registrations.Add(new Registration { StudentId = studentId, CourseId = Id });
         }
      }

      public override bool IsValid()
      {
         if (string.IsNullOrWhiteSpace(Code)) return false;
         if (string.IsNullOrWhiteSpace(Title)) return false;
         if (Session < 1 || Session > 6) return false;
         return true;
      }
   }
}