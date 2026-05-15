using DAL;
using Models;
using SelectionDemo.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static Controllers.AccessControl;

namespace SelectionDemo.Controllers
{
    [UserAccess(Access.View)]
    public class StudentsController : Controller
    {
        public void EnsureState()
        {
            CurrentAcademicSession.Get(Session);
            if (Session["StudentsSearchOn"] == null) Session["StudentsSearchOn"] = false;
            if (Session["StudentsKeywords"] == null) Session["StudentsKeywords"] = "";
            if (Session["StudentsYearFilter"] == null) Session["StudentsYearFilter"] = 0;
        }

        public bool HasWriteAccess()
        {
            return global::Models.User.ConnectedUser != null && global::Models.User.ConnectedUser.CanWrite;
        }

        public string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            string normalized = value.ToLower().Trim().Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public List<Student> FilteredStudents()
        {
            EnsureState();
            string keywords = ((string)Session["StudentsKeywords"] ?? "").Trim();
            int year = (int)Session["StudentsYearFilter"];

            var students = DB.Students.ToList().OrderByDescending(s => s.CohortYear).ThenBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();
            if (year > 0)
                students = students.Where(s => s.CohortYear == year).ToList();
            if (!string.IsNullOrEmpty(keywords))
            {
                string normalizedKeywords = Normalize(keywords);
                students = students.Where(s =>
                    Normalize(s.Code).Contains(normalizedKeywords) ||
                    Normalize(s.FirstName).Contains(normalizedKeywords) ||
                    Normalize(s.LastName).Contains(normalizedKeywords) ||
                    Normalize(s.FullName).Contains(normalizedKeywords)).ToList();
            }
            return students;
        }

        public void PrepareListView()
        {
            EnsureState();
            var current = CurrentAcademicSession.Get(Session);
            ViewBag.PageTitle = "Etudiants";
            ViewBag.SearchOn = (bool)Session["StudentsSearchOn"];
            ViewBag.Keywords = (string)Session["StudentsKeywords"];
            ViewBag.YearFilter = (int)Session["StudentsYearFilter"];
            ViewBag.SessionCaption = current.Caption;
            ViewBag.SessionYear = current.Year;
            ViewBag.SessionSeason = current.Season;
            ViewBag.Years = DB.Students.ToList().Select(s => s.CohortYear).Distinct().OrderByDescending(y => y).ToList();
        }

        public void PrepareEditView(Student student)
        {
            var current = CurrentAcademicSession.Get(Session);
            var currentSessionCourses = DB.Courses.ToList()
                .Where(c => current.ValidSessions.Contains(c.Session))
                .OrderBy(c => c.Session)
                .ThenBy(c => c.Code)
                .ToList();
            ViewBag.Registrations = student.NextSessionCoursesToSelectList;
            ViewBag.Courses = SelectListUtilities<Course>.Convert(currentSessionCourses, "Caption");
            ViewBag.CurrentSessionCaption = current.Caption;
            ViewBag.PageTitle = "Etudiant - Modification";
        }

        public ActionResult List()
        {
            PrepareListView();
            return View();
        }

        public ActionResult GetStudents(bool forceRefresh = false)
        {
            EnsureState();
            if (DB.Students.HasChanged || DB.Registrations.HasChanged || forceRefresh)
            {
                var current = CurrentAcademicSession.Get(Session);
                ViewBag.Keywords = (string)Session["StudentsKeywords"];
                ViewBag.SessionCaption = current.Caption;
                return PartialView("_StudentsList", FilteredStudents());
            }
            return Content("");
        }

        public ActionResult ToggleSearch()
        {
            EnsureState();
            Session["StudentsSearchOn"] = !(bool)Session["StudentsSearchOn"];
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult SetSearch(string keywords = "", int year = 0)
        {
            EnsureState();
            Session["StudentsKeywords"] = keywords ?? "";
            Session["StudentsYearFilter"] = year;
            return Content("ok");
        }

        [HttpPost]
        public ActionResult SetCurrentSession(int year, string season)
        {
            if (!HasWriteAccess()) return Content("blocked");
            var current = CurrentAcademicSession.Set(Session, year, season);
            return Content(current.Caption);
        }

        public ActionResult Details(int id)
        {
            var student = DB.Students.Get(id);
            if (student == null)
                return HttpNotFound();
            var current = CurrentAcademicSession.Get(Session);
            ViewBag.PageTitle = "Etudiant - Details";
            ViewBag.SessionCaption = current.Caption;
            return View(student);
        }

        public ActionResult GetStudentInfo(int id, bool forceRefresh = false)
        {
            var student = DB.Students.Get(id);
            if (student == null)
                return Content("blocked");
            if (DB.Students.HasChanged || forceRefresh)
                return PartialView("_StudentInfo", student);
            return Content("");
        }

        public ActionResult GetStudentRegistrations(int id, bool forceRefresh = false)
        {
            var student = DB.Students.Get(id);
            if (student == null)
                return Content("blocked");
            if (DB.Registrations.HasChanged || DB.Courses.HasChanged || forceRefresh)
                return PartialView("_StudentRegistrations", student);
            return Content("");
        }

        public ActionResult Create()
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            var student = new Student
            {
                AdmissionYear = DateTime.Now.Year,
                BirthDate = DateTime.Now.AddYears(-17)
            };
            ViewBag.PageTitle = "Etudiant - Ajout";
            return View(student);
        }

        [HttpPost]
        public ActionResult Create(Student student)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            if (student.IsValid())
            {
                int id = DB.Students.Add(student);
                return RedirectToAction("Details", new { id });
            }
            ViewBag.PageTitle = "Etudiant - Ajout";
            return View(student);
        }

        public ActionResult Edit(int id)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            var student = DB.Students.Get(id);
            if (student == null)
                return HttpNotFound();
            PrepareEditView(student);
            return View(student);
        }

        [HttpPost]
        public ActionResult Edit(Student student, List<int> selectedCoursesId)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            if (student.IsValid())
            {
                DB.Students.Update(student, selectedCoursesId);
                return RedirectToAction("Details", new { id = student.Id });
            }
            PrepareEditView(student);
            return View(student);
        }

        public ActionResult Delete(int id)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            DB.Students.Delete(id);
            return RedirectToAction("List");
        }
    }
}
