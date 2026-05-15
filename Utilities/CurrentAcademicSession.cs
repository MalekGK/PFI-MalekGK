using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace SelectionDemo.Utilities
{
    public class CurrentAcademicSession
    {
        const string YearKey = "CurrentAcademicYear";
        const string SeasonKey = "CurrentAcademicSeason";

        public int Year { get; set; }
        public string Season { get; set; }
        public List<int> ValidSessions => Season == "Automne"
           ? new List<int> { 1, 3, 5 }
           : new List<int> { 2, 4, 6 };
        public string Caption => Season + " " + Year;

        public static CurrentAcademicSession Get(HttpSessionStateBase session)
        {
            Ensure(session);
            int year = (int)session[YearKey];
            string season = (string)session[SeasonKey];
            var result = new CurrentAcademicSession { Year = year, Season = season };
            ApplyToNextSession(result);
            return result;
        }

        public static CurrentAcademicSession Set(HttpSessionStateBase session, int year, string season)
        {
            if (year < 2000 || year > 2099)
                year = DateTime.Now.Year;
            if (season != "Automne" && season != "Hiver") season = "Automne";
            session[YearKey] = year;
            session[SeasonKey] = season;
            var result = new CurrentAcademicSession { Year = year, Season = season };
            ApplyToNextSession(result);
            return result;
        }

        public static void Ensure(HttpSessionStateBase session)
        {
            if (session[YearKey] == null)
                session[YearKey] = NextSession.Year;
            if (session[SeasonKey] == null)
                session[SeasonKey] = NextSession.ValidSessions.Contains(1) ? "Automne" : "Hiver";
        }

        public static void ApplyToNextSession(CurrentAcademicSession current)
        {
            int month = current.Season == "Automne" ? 6 : 1;
            NextSession.CurrentDate = new DateTime(current.Year, month, 1);
        }
    }
}
