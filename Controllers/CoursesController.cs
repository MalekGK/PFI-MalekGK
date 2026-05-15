using DAL;
using Models;
using SelectionDemo.Utilities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using static Controllers.AccessControl;

namespace SelectionDemo.Controllers
{
    [UserAccess(Access.View)]
    public class CoursesController : Controller
    {
        public void EnsureState()
        {
            CurrentAcademicSession.Get(Session);
            if (Session["CoursesSearchOn"] == null) Session["CoursesSearchOn"] = false;
            if (Session["CoursesKeywords"] == null) Session["CoursesKeywords"] = "";
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

        public List<Course> FilteredCourses()
        {
            EnsureState();
            string keywords = ((string)Session["CoursesKeywords"] ?? "").Trim();
            var courses = DB.Courses.ToList().OrderBy(c => c.Session).ThenBy(c => c.Code).ToList();
            if (!string.IsNullOrEmpty(keywords))
            {
                string normalizedKeywords = Normalize(keywords);
                courses = courses.Where(c =>
                   Normalize(c.Code).Contains(normalizedKeywords) ||
                   Normalize(c.Title).Contains(normalizedKeywords)).ToList();
            }
            return courses;
        }

        public void PrepareListView()
        {
            EnsureState();
            var current = CurrentAcademicSession.Get(Session);
            ViewBag.PageTitle = "Cours";
            ViewBag.SearchOn = (bool)Session["CoursesSearchOn"];
            ViewBag.Keywords = (string)Session["CoursesKeywords"];
            ViewBag.SessionCaption = current.Caption;
            ViewBag.SessionYear = current.Year;
            ViewBag.SessionSeason = current.Season;
        }

        public void PrepareEditView(Course course)
        {
            var current = CurrentAcademicSession.Get(Session);
            var allStudents = DB.Students.ToList().OrderBy(s => s.Code).ToList();
            ViewBag.Registrations = course.NextSessionStudentsToSelectList;
            ViewBag.Students = SelectListUtilities<Student>.Convert(allStudents, "Caption");
            ViewBag.CurrentSessionCaption = current.Caption;
            ViewBag.PageTitle = "Cours - Modification";
        }

        public ActionResult List()
        {
            PrepareListView();
            return View();
        }

        public ActionResult GetCourses(bool forceRefresh = false)
        {
            EnsureState();
            if (DB.Courses.HasChanged || DB.Registrations.HasChanged || forceRefresh)
            {
                var current = CurrentAcademicSession.Get(Session);
                ViewBag.Keywords = (string)Session["CoursesKeywords"];
                ViewBag.SessionCaption = current.Caption;
                return PartialView("_CoursesList", FilteredCourses());
            }
            return Content("");
        }

        public ActionResult ToggleSearch()
        {
            EnsureState();
            Session["CoursesSearchOn"] = !(bool)Session["CoursesSearchOn"];
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult SetSearch(string keywords = "")
        {
            EnsureState();
            Session["CoursesKeywords"] = keywords ?? "";
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
            var course = DB.Courses.Get(id);
            if (course == null)
                return HttpNotFound();
            var current = CurrentAcademicSession.Get(Session);
            ViewBag.PageTitle = "Cours - Details";
            ViewBag.SessionCaption = current.Caption;
            return View(course);
        }

        public ActionResult GetCourseInfo(int id, bool forceRefresh = false)
        {
            var course = DB.Courses.Get(id);
            if (course == null)
                return Content("blocked");
            if (DB.Courses.HasChanged || forceRefresh)
                return PartialView("_CourseInfo", course);
            return Content("");
        }

        public ActionResult GetCourseRegistrations(int id, bool forceRefresh = false)
        {
            var course = DB.Courses.Get(id);
            if (course == null)
                return Content("blocked");
            if (DB.Registrations.HasChanged || DB.Allocations.HasChanged || DB.Students.HasChanged || forceRefresh)
                return PartialView("_CourseRegistrations", course);
            return Content("");
        }

        public ActionResult Create()
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            ViewBag.PageTitle = "Cours - Ajout";
            return View(new Course { Session = 1 });
        }

        [HttpPost]
        public ActionResult Create(Course course)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            if (course.IsValid())
            {
                int id = DB.Courses.Add(course);
                return RedirectToAction("Details", new { id });
            }
            ViewBag.PageTitle = "Cours - Ajout";
            return View(course);
        }

        public ActionResult Edit(int id)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            var course = DB.Courses.Get(id);
            if (course == null)
                return HttpNotFound();
            PrepareEditView(course);
            return View(course);
        }

        [HttpPost]
        public ActionResult Edit(Course course, List<int> selectedStudentsId)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            if (course.IsValid())
            {
                DB.Courses.Update(course, selectedStudentsId);
                return RedirectToAction("Details", new { id = course.Id });
            }
            PrepareEditView(course);
            return View(course);
        }

        public ActionResult Delete(int id)
        {
            if (!HasWriteAccess()) return RedirectToAction("List");
            DB.Courses.Delete(id);
            return RedirectToAction("List");
        }
    }
}
