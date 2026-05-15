using SelectionDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models;
// Auteur: Nicolas Chourot 2026
namespace SelectionDemo.Controllers
{
    public class ColorsSelectionController : Controller
    {
        public ActionResult Index()
        {
            return View(DB.ColorChoices.ToList());
        }

        public ActionResult Edit()
        {
            // prepare a selectlist that contain colors choices
            ViewBag.ColorChoices = SelectListUtilities<Color>.Convert(DB.ColorChoices.ToList(), "Caption");
            // prepare a selectlist that contain all the colors
            ViewBag.Colors = SelectListUtilities<Color>.Convert(DB.Colors.ToList(), "Caption");
            return View();
        }
        [HttpPost]
        public ActionResult Edit(List<int> ColorChoicesId)
        {
            DB.ColorChoices.Update(ColorChoicesId);
            return RedirectToAction("Index");
        }
    }
}