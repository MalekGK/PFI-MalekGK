using EmailHandling;
using Models;
using SelectionDemo.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;

namespace DAL
{
   public sealed class DB
   {
      #region singleton setup
      private static readonly DB instance = new DB();
      public static DB Instance { get { return instance; } }
      #endregion

      static public UsersRepository Users { get; set; }
            = new UsersRepository();

      static public EventsRepository Events { get; set; }
            = new EventsRepository();

      static public NotificationsRepository Notifications { get; set; }
            = new NotificationsRepository();

      static public StudentsRepository Students { get; set; }
            = new StudentsRepository();

      static public CoursesRepository Courses { get; set; }
            = new CoursesRepository();

      static public RegistrationsRepository Registrations { get; set; }
            = new RegistrationsRepository();

      static public AllocationsRepository Allocations { get; set; }
            = new AllocationsRepository();

      static public TeachersRepository Teachers { get; set; }
            = new TeachersRepository();

      static public Repository<RenewPasswordCommand> RenewPasswordCommands { get; set; }
            = new Repository<RenewPasswordCommand>();

      static public Repository<UnverifiedEmail> UnverifiedEmails { get; set; }
            = new Repository<UnverifiedEmail>();

      static public LoginsRepository Logins { get; set; }
            = new LoginsRepository();
   }
}