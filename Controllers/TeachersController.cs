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
    public class TeachersController : Controller
    {
        public void EnsureState()
        {
            CurrentAcademicSession.Get(Session);
            if (Session["TeachersSearchOn"] == null) Session["TeachersSearchOn"] = false;
            if (Session["TeachersKeywords"] == null) Session["TeachersKeywords"] = "";
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

        public List<Teacher> FilteredTeachers()
        {
            EnsureState();
            string keywords = ((string)Session["TeachersKeywords"] ?? "").Trim();
            var teachers = DB.Teachers.ToList().OrderBy(t => t.LastName).ThenBy(t => t.FirstName).ToList();
            if (!string.IsNullOrEmpty(keywords))
            {
                string normalizedKeywords = Normalize(keywords);
                teachers = teachers.Where(t =>
                    Normalize(t.Code).Contains(normalizedKeywords) ||
                    Normalize(t.FirstName).Contains(normalizedKeywords) ||
                    Normalize(t.LastName).Contains(normalizedKeywords) ||
                    Normalize(t.FullName).Contains(normalizedKeywords)).ToList();
            }
            return teachers;
        }

        public void PrepareListView()
        {
            EnsureState();
            var current = CurrentAcademicSession.Get(Session);
            ViewBag.PageTitle = "Profs";
            ViewBag.SearchOn = (bool)Session["TeachersSearchOn"];
            ViewBag.Keywords = (string)Session["TeachersKeywords"];
            ViewBag.SessionCaption = current.Caption;
            ViewBag.SessionYear = current.Year;
            ViewBag.SessionSeason = current.Season;
        }

        public void PrepareEditView(Teacher teacher)
        {
            var current = CurrentAcademicSession.Get(Session);
            var currentSessionCourses = DB.Courses.ToList()
                .Where(c => current.ValidSessions.Contains(c.Session))
                .OrderBy(c => c.Session)
                .ThenBy(c => c.Code)
                .ToList();
            ViewBag.Allocations = teacher.NextSessionCoursesToSelectList;
            ViewBag.Courses = SelectListUtilities<Course>.Convert(currentSessionCourses, "Caption");
            ViewBag.CurrentSessionCaption = current.Caption;
            ViewBag.PageTitle = "Prof - Modification";
        }

        public ActionResult List()
        {
            PrepareListView();
            return View();
        }

        public ActionResult GetTeachers(bool forceRefresh = false)
        {
            EnsureState();
            if (DB.Teachers.HasChanged || forceRefresh)
            {
                var current = CurrentAcademicSession.Get(Session);
                ViewBag.Keywords = (string)Session["TeachersKeywords"];
                ViewBag.SessionCaption = current.Caption;
                return PartialView("_TeachersList", FilteredTeachers());
            }
            return Content("");
        }

        public ActionResult ToggleSearch()
        {
            EnsureState();
            Session["TeachersSearchOn"] = !(bool)Session["TeachersSearchOn"];
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult SetSearch(string keywords = "")
        {
            EnsureState();
            Session["TeachersKeywords"] = keywords ?? "";
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
            var teacher = DB.Teachers.Get(id);
            if (teacher == null)
                return HttpNotFound();
            var current = CurrentAcademicSession.Get(Session);
            ViewBag.PageTitle = "Prof - Details";
            ViewBag.SessionCaption = current.Caption;
            return View(teacher);
        }

        public ActionResult GetTeacherInfo(int id, bool forceRefresh = false)
        {
            var teacher = DB.Teachers.Get(id);
            if (teacher == null)
                return Content("blocked");
            if (DB.Teachers.HasChanged || forceRefresh)
                return PartialView("_TeacherInfo", teacher);
            return Content("");
        }

        public ActionResult GetTeacherAllocations(int id, bool forceRefresh = false)
        {
            var teacher = DB.Teachers.Get(id);
            if (teacher == null)
                return Content("blocked");
            if (DB.Allocations.HasChanged || DB.Courses.HasChanged || forceRefresh)
                return PartialView("_TeacherAllocations", teacher);
            return Content("");
        }

        public ActionResult Create()
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            var teacher = new Teacher();
            ViewBag.PageTitle = "Prof - Ajout";
            return View(teacher);
        }

        [HttpPost]
        public ActionResult Create(Teacher teacher)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            if (teacher.IsValid())
            {
                int id = DB.Teachers.Add(teacher);
                return RedirectToAction("Details", new { id });
            }
            ViewBag.PageTitle = "Prof - Ajout";
            return View(teacher);
        }

        public ActionResult Edit(int id)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            var teacher = DB.Teachers.Get(id);
            if (teacher == null)
                return HttpNotFound();
            PrepareEditView(teacher);
            return View(teacher);
        }

        [HttpPost]
        public ActionResult Edit(Teacher teacher, List<int> selectedCoursesId)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            if (teacher.IsValid())
            {
                DB.Teachers.Update(teacher, selectedCoursesId);
                return RedirectToAction("Details", new { id = teacher.Id });
            }
            PrepareEditView(teacher);
            return View(teacher);
        }

        public ActionResult Delete(int id)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            DB.Teachers.Delete(id);
            return RedirectToAction("List");
        }
    }
}
