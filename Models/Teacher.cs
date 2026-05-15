using DAL;
using Newtonsoft.Json;
using SelectionDemo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Models
{
   public class Teacher : Record
   {
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string Code { get; set; }
      public DateTime StartDate { get; set; } = DateTime.Now;
      public string Email { get; set; }
      public string Phone { get; set; }

      const string Avatars_Folder = @"/App_Assets/teachers/";
      const string Default_Avatar = @"no_avatar.png";
      [ImageAsset(Avatars_Folder, Default_Avatar)]
      public string Avatar { get; set; } = Avatars_Folder + Default_Avatar;

      public void GenerateCode()
      {
         Code = CodeGenerator.GenerateTeacherCode();
      }

      [JsonIgnore]
      public string FullName => LastName + " " + FirstName;

      [JsonIgnore]
      public string Caption => Code + " " + LastName + " " + FirstName;

      [JsonIgnore]
      public string ContactEmail
      {
         get
         {
            if (!string.IsNullOrWhiteSpace(Email)) return Email;
            return (FirstName + "." + LastName + "@clg.qc.ca").ToLower()
               .Replace("é", "e")
               .Replace("è", "e")
               .Replace("ê", "e")
               .Replace("à", "a")
               .Replace("â", "a")
               .Replace("î", "i")
               .Replace("ï", "i")
               .Replace("ô", "o")
               .Replace("ù", "u")
               .Replace("û", "u")
               .Replace("ç", "c")
               .Replace(" ", "");
         }
      }

      [JsonIgnore]
      public double SeniorityYears => Math.Round((DateTime.Now - StartDate).TotalDays / 365.25, 1);

      [JsonIgnore]
      public List<Allocation> Allocations => DB.Allocations.ToList()
         .Where(a => a.TeacherId == Id)
         .OrderByDescending(a => a.Year)
         .ThenBy(a => a.Course.Session)
         .ThenBy(a => a.Course.Code)
         .ToList();

      [JsonIgnore]
      public List<Allocation> NextSessionAllocations => DB.Allocations.ToList()
         .Where(a => a.TeacherId == Id && a.IsNextSession)
         .OrderBy(a => a.Course.Session)
         .ThenBy(a => a.Course.Code)
         .ToList();

      [JsonIgnore]
      public List<Course> Courses
      {
         get
         {
            List<Course> courses = new List<Course>();
            foreach (var allocation in Allocations)
               courses.Add(allocation.Course);
            return courses;
         }
      }

      [JsonIgnore]
      public List<Course> NextSessionCourses
      {
         get
         {
            List<Course> courses = new List<Course>();
            foreach (var allocation in NextSessionAllocations)
               courses.Add(allocation.Course);
            return courses;
         }
      }

      [JsonIgnore]
      public SelectList CoursesSelectList => SelectListUtilities<Course>.Convert(Courses, "Caption");

      [JsonIgnore]
      public SelectList NextSessionCoursesToSelectList => SelectListUtilities<Course>.Convert(NextSessionCourses, "Caption");

      public void DeleteAllAllocations()
      {
         foreach (var allocation in Allocations)
            DB.Allocations.Delete(allocation.Id);
      }

      public void DeleteNextSessionAllocations()
      {
         foreach (var allocation in NextSessionAllocations)
            DB.Allocations.Delete(allocation.Id);
      }

      public void UpdateAllocations(List<int> selectedCoursesId)
      {
         DeleteNextSessionAllocations();
         if (selectedCoursesId != null)
         {
            foreach (int courseId in selectedCoursesId)
               DB.Allocations.Add(new Allocation { TeacherId = Id, CourseId = courseId });
         }
      }

      public override bool IsValid()
      {
         if (string.IsNullOrWhiteSpace(FirstName)) return false;
         if (string.IsNullOrWhiteSpace(LastName)) return false;
         if (!IsAlpha(FirstName)) return false;
         if (!IsAlpha(LastName)) return false;
         return true;
      }
   }
}