using DAL;
using Newtonsoft.Json;
using SelectionDemo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Models
{
   public class Student : Record
   {
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string Code { get; set; }
      public DateTime BirthDate { get; set; }
      public string Email { get; set; }
      public string Phone { get; set; }
      public int AdmissionYear { get; set; }

      public void GenerateCode()
      {
         Code = CodeGenerator.GenerateStudentCode();
      }

      [JsonIgnore]
      public string FullName => LastName + " " + FirstName;

      [JsonIgnore]
      public string Caption => Code + " " + LastName + " " + FirstName;

      [JsonIgnore]
      public int Year
      {
         get
         {
            if (!string.IsNullOrEmpty(Code) && Code.Length >= 4)
            {
               int parsedYear;
               if (int.TryParse(Code.Substring(0, 4), out parsedYear))
                  return parsedYear;
            }
            return AdmissionYear;
         }
      }

      [JsonIgnore]
      public int CohortYear => AdmissionYear > 0 ? AdmissionYear : Year;

      [JsonIgnore]
      public List<Registration> Registrations => DB.Registrations.ToList()
         .Where(r => r.StudentId == Id)
         .OrderByDescending(r => r.Year)
         .ThenBy(r => r.Course.Session)
         .ThenBy(r => r.Course.Code)
         .ToList();

      [JsonIgnore]
      public List<Registration> NextSessionRegistrations => DB.Registrations.ToList()
         .Where(r => r.StudentId == Id && r.IsNextSession)
         .OrderBy(r => r.Course.Session)
         .ThenBy(r => r.Course.Code)
         .ToList();

      [JsonIgnore]
      public List<Course> Courses
      {
         get
         {
            List<Course> courses = new List<Course>();
            foreach (var registration in Registrations)
               courses.Add(registration.Course);
            return courses;
         }
      }

      [JsonIgnore]
      public List<Course> NextSessionCourses
      {
         get
         {
            List<Course> courses = new List<Course>();
            foreach (var registration in NextSessionRegistrations)
               courses.Add(registration.Course);
            return courses;
         }
      }

      [JsonIgnore]
      public SelectList CoursesSelectList => SelectListUtilities<Course>.Convert(Courses, "Caption");

      [JsonIgnore]
      public SelectList NextSessionCoursesToSelectList => SelectListUtilities<Course>.Convert(NextSessionCourses, "Caption");

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

      public void UpdateRegistrations(List<int> selectedCoursesId)
      {
         DeleteNextSessionRegistrations();
         if (selectedCoursesId != null)
         {
            foreach (int courseId in selectedCoursesId)
               DB.Registrations.Add(new Registration { StudentId = Id, CourseId = courseId });
         }
      }

      public override bool IsValid()
      {
         if (string.IsNullOrWhiteSpace(FirstName)) return false;
         if (string.IsNullOrWhiteSpace(LastName)) return false;
         if (string.IsNullOrWhiteSpace(Email)) return false;
         if (!IsAlpha(FirstName)) return false;
         if (!IsAlpha(LastName)) return false;
         if (!IsEmail(Email)) return false;
         return true;
      }
   }
}