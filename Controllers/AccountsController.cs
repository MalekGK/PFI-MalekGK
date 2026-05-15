using DAL;
using EmailHandling;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Controllers.AccessControl;

namespace SelectionDemo.Controllers
{
    public class AccountsController : Controller
    {
        public JsonResult EmailExist(string Email)
        {
            string email = (Email ?? "").Trim().ToLower();
            bool exists = DB.Users.ToList().Any(u => (u.Email ?? "").ToLower() == email);
            return Json(exists, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EmailAvailable(string Email)
        {
            string email = (Email ?? "").Trim().ToLower();
            int currentId = global::Models.User.ConnectedUser != null ? global::Models.User.ConnectedUser.Id : 0;
            bool notAvailable = DB.Users.ToList().Any(u => (u.Email ?? "").ToLower() == email && u.Id != currentId);
            return Json(notAvailable, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExpiredSession()
        {
            return RedirectToLoginMessage("Session expiree, veuillez vous reconnecter.", false);
        }

        public ActionResult Logout()
        {
            DisconnectCurrentUser("Logout");
            return RedirectToAction("Login");
        }

        public ActionResult Login(string message = "", bool success = true)
        {
            if (global::Models.User.ConnectedUser != null)
            {
                DisconnectCurrentUser(success ? "Logout" : "Expired/blocked");
            }

            Session["LoginMessage"] = message;
            Session["LoginSuccess"] = success;
            if (Session["CurrentLoginEmail"] == null) Session["CurrentLoginEmail"] = "";

            LoginCredential credential = new LoginCredential
            {
                Email = (string)Session["CurrentLoginEmail"]
            };

            ViewBag.LoginMessage = message;
            ViewBag.LoginSuccess = success;
            ViewBag.PageTitle = "PFI";
            return View(credential);
        }

        [HttpPost]
        public ActionResult Login(LoginCredential credential)
        {
            if (credential == null) credential = new LoginCredential();

            DateTime serverDate = DateTime.Now;
            int serverTimeZoneOffset = serverDate.Hour - serverDate.ToUniversalTime().Hour;
            Session["TimeZoneOffset"] = -(credential.TimeZoneOffset + serverTimeZoneOffset);

            credential.Email = (credential.Email ?? "").Trim();
            credential.Password = (credential.Password ?? "").Trim();
            Session["CurrentLoginEmail"] = credential.Email;

            User loginUser = DB.Users.GetUser(credential);
            if (loginUser == null)
            {
                ViewBag.LoginMessage = "Courriel ou mot de passe incorrect.";
                ViewBag.LoginSuccess = false;
                ViewBag.PageTitle = "PFI";
                return View(credential);
            }

            if (loginUser.Online)
                return RedirectToLoginMessage("Il y a deja une session ouverte avec cet usager.", false);

            if (loginUser.Blocked)
                return RedirectToLoginMessage("Votre compte a ete bloque.", false);

            if (!loginUser.Verified)
                return RedirectToLoginMessage("Votre compte n'est pas verifie.", false);

            global::Models.User.ConnectedUser = loginUser;
            global::Models.User.ConnectedUser.Online = true;
            DB.Logins.Add(global::Models.User.ConnectedUser.Id);
            DB.Events.Add("Login");

            return Redirect(RouteConfig.DefaultAction());
        }

        public ActionResult Subscribe()
        {
            global::Models.User.ConnectedUser = null;
            Session["CurrentLoginEmail"] = "";
            ViewBag.PageTitle = "Nouveau compte";
            return View(new User());
        }

        [HttpPost]
        public ActionResult Subscribe(User user, string ConfirmEmail = "", string ConfirmPassword = "", string NotifyCB = "off")
        {
            if (user == null) user = new User();

            user.Name = (user.Name ?? "").Trim();
            user.Email = (user.Email ?? "").Trim();
            user.Password = (user.Password ?? "").Trim();
            user.Notify = NotifyCB == "on";
            user.Access = Access.View;
            user.Verified = false;

            string confirmEmail = (ConfirmEmail ?? "").Trim();
            string confirmPassword = (ConfirmPassword ?? "").Trim();

            if (user.Email != confirmEmail)
            {
                ViewBag.FormError = "Les courriels ne correspondent pas.";
                ViewBag.PageTitle = "Nouveau compte";
                return View(user);
            }

            if (user.Password != confirmPassword)
            {
                ViewBag.FormError = "Les mots de passe ne correspondent pas.";
                ViewBag.PageTitle = "Nouveau compte";
                return View(user);
            }

            if (!user.IsValid())
            {
                ViewBag.FormError = "Le compte contient des donnees invalides ou deja utilisees.";
                ViewBag.PageTitle = "Nouveau compte";
                return View(user);
            }

            int newId = DB.Users.Add(user);
            if (newId <= 0)
            {
                ViewBag.FormError = "Impossible de creer le compte.";
                ViewBag.PageTitle = "Nouveau compte";
                return View(user);
            }

            DB.Events.Add("Subscribe");
            Session["CurrentLoginEmail"] = user.Email;

            try
            {
                AccountsEmailing.SendEmailVerification(Url.Action("VerifyUser", "Accounts", null, Request.Url.Scheme), user);
            }
            catch
            {
                // Keep signup flow stable even when SMTP is unavailable.
            }

            return RedirectToLoginMessage("Creation de compte effectuee avec succes. Un courriel de confirmation d'adresse vous a ete envoye.", true);
        }

        public ActionResult VerifyUser(string code)
        {
            string verificationCode = (code ?? "").Trim();
            UnverifiedEmail unverifiedEmail = DB.UnverifiedEmails.ToList()
                .FirstOrDefault(u => u.VerificationCode == verificationCode);

            if (unverifiedEmail == null)
                return RedirectToLoginMessage("Erreur de verification de courriel.", false);

            User newlySubscribedUser = DB.Users.Get(unverifiedEmail.UserId);
            DB.UnverifiedEmails.Delete(unverifiedEmail.Id);

            if (newlySubscribedUser == null)
                return RedirectToLoginMessage("Erreur de verification de courriel.", false);

            newlySubscribedUser.Verified = true;
            Session["CurrentLoginEmail"] = newlySubscribedUser.Email;
            DB.Users.Update(newlySubscribedUser);
            DB.Events.Add("User verified");

            try
            {
                AccountsEmailing.SendEmailUserStatusChanged("Votre adresse de courriel a ete confirmee.", newlySubscribedUser);
            }
            catch
            {
            }

            return RedirectToLoginMessage("Votre adresse de courriel a ete verifiee avec succes.", true);
        }

        public ActionResult RenewPasswordCommand()
        {
            ViewBag.EmailNotFound = false;
            ViewBag.PageTitle = "Reinitialisation de mot de passe";
            return View(new EmailView());
        }

        [HttpPost]
        public ActionResult RenewPasswordCommand(EmailView emailView)
        {
            if (emailView == null) emailView = new EmailView();

            string email = (emailView.Email ?? "").Trim();
            User user = DB.Users.ToList().FirstOrDefault(u =>
                string.Equals((u.Email ?? "").Trim(), email, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                try
                {
                    AccountsEmailing.SendEmailRenewPasswordCommand(Url.Action("RenewPassword", "Accounts", null, Request.Url.Scheme), email);
                }
                catch
                {
                }

                return RedirectToLoginMessage("Un courriel de commande de changement de mot de passe vous a ete envoye si l'adresse fournie est valide.", true);
            }

            ViewBag.EmailNotFound = true;
            ViewBag.PageTitle = "Reinitialisation de mot de passe";
            emailView.Email = email;
            return View(emailView);
        }

        public ActionResult RenewPassword(string code)
        {
            string verificationCode = (code ?? "").Trim();
            RenewPasswordCommand command = DB.RenewPasswordCommands.ToList()
                .FirstOrDefault(r => r.VerificationCode == verificationCode);

            if (command == null)
                return RedirectToLoginMessage("Commande de changement de mot de passe introuvable.", false);

            ViewBag.PageTitle = "Renouvellement de mot de passe";
            return View(new RenewPasswordView { Code = command.VerificationCode });
        }

        [HttpPost]
        public ActionResult RenewPassword(RenewPasswordView passwordView)
        {
            if (passwordView == null) passwordView = new RenewPasswordView();

            passwordView.Code = (passwordView.Code ?? "").Trim();
            passwordView.Password = (passwordView.Password ?? "").Trim();
            passwordView.ConfirmPassword = (passwordView.ConfirmPassword ?? "").Trim();

            if (passwordView.Password != passwordView.ConfirmPassword)
            {
                ViewBag.FormError = "Les mots de passe ne correspondent pas.";
                ViewBag.PageTitle = "Renouvellement de mot de passe";
                return View(passwordView);
            }

            if (passwordView.Password.Length < 6)
            {
                ViewBag.FormError = "Le mot de passe doit contenir au moins 6 caracteres.";
                ViewBag.PageTitle = "Renouvellement de mot de passe";
                return View(passwordView);
            }

            RenewPasswordCommand command = DB.RenewPasswordCommands.ToList()
                .FirstOrDefault(r => r.VerificationCode == passwordView.Code);
            if (command == null)
                return RedirectToLoginMessage("Commande de changement de mot de passe introuvable.", false);

            User user = DB.Users.Get(command.UserId);
            if (user == null)
            {
                DB.RenewPasswordCommands.Delete(command.Id);
                return RedirectToLoginMessage("Commande de changement de mot de passe introuvable.", false);
            }

            DB.RenewPasswordCommands.Delete(command.Id);
            user.Password = passwordView.Password;
            DB.Users.ChangePassword(user);

            try
            {
                AccountsEmailing.SendEmailUserStatusChanged("Votre mot de passe a ete modifie avec succes.", user);
            }
            catch
            {
                // Keep operation successful even when email cannot be delivered.
            }

            return RedirectToLoginMessage("Votre mot de passe a ete modifie avec succes.", true);
        }

        public ActionResult RenewPasswordCancelled(string code)
        {
            return RedirectToLoginMessage("Commande de changement de mot de passe annulee.", false);
        }

        [UserAccess(Access.View)]
        public ActionResult EditProfil()
        {
            User connectedUser = global::Models.User.ConnectedUser;
            if (connectedUser == null)
                return RedirectToAction("Login");

            ViewBag.PageTitle = "Profil";
            return View(connectedUser);
        }

        [UserAccess(Access.View)]
        [HttpPost]
        public ActionResult EditProfil(User user, string ConfirmEmail = "", string ConfirmPassword = "", string NotifyCB = "off", string ChangePassword = "off")
        {
            User connectedUser = global::Models.User.ConnectedUser;
            if (connectedUser == null)
                return RedirectToAction("Login");

            if (user == null) user = connectedUser.Copy();

            user.Id = connectedUser.Id;
            user.Blocked = connectedUser.Blocked;
            user.Access = connectedUser.Access;
            user.Verified = connectedUser.Verified;
            user.Notify = NotifyCB == "on";

            user.Name = (user.Name ?? "").Trim();
            user.Email = (user.Email ?? "").Trim();
            user.Password = (user.Password ?? "").Trim();

            string confirmEmail = (ConfirmEmail ?? "").Trim();
            string confirmPassword = (ConfirmPassword ?? "").Trim();
            bool changingPassword = ChangePassword == "on";

            if (user.Email != confirmEmail)
            {
                ViewBag.FormError = "Les courriels ne correspondent pas.";
                ViewBag.PageTitle = "Profil";
                return View(user);
            }

            if (!changingPassword)
            {
                user.Password = connectedUser.Password;
            }
            else if (user.Password != confirmPassword)
            {
                ViewBag.FormError = "Les mots de passe ne correspondent pas.";
                ViewBag.PageTitle = "Profil";
                return View(user);
            }

            if (!user.IsValid())
            {
                ViewBag.FormError = "Le profil contient des donnees invalides ou deja utilisees.";
                ViewBag.PageTitle = "Profil";
                return View(user);
            }

            bool updated = DB.Users.Update(user);
            if (!updated)
            {
                ViewBag.FormError = "Impossible de modifier le profil.";
                ViewBag.PageTitle = "Profil";
                return View(user);
            }

            global::Models.User.ConnectedUser = DB.Users.Get(user.Id);
            DB.Events.Add("EditProfil");
            return Redirect(RouteConfig.DefaultAction());
        }

        [UserAccess(Access.View)]
        public ActionResult DeleteProfil()
        {
            User connectedUser = global::Models.User.ConnectedUser;
            if (connectedUser == null)
                return RedirectToAction("Login");

            DB.Events.Add("DeleteProfil");
            DB.Users.Delete(connectedUser.Id);
            global::Models.User.ConnectedUser = null;
            return RedirectToLoginMessage("Votre compte a ete efface.", true);
        }

        [UserAccess(Access.Admin)]
        public ActionResult ManageUsers()
        {
            DB.Events.Add("ManageUsers");
            ViewBag.PageTitle = "Gestion des usagers";
            return View();
        }

        [UserAccess(Access.Admin)]
        public ActionResult GetUsers(bool forceRefresh = false)
        {
            if (DB.Users.HasChanged || DB.Logins.HasChanged || forceRefresh)
            {
                int connectedId = global::Models.User.ConnectedUser != null ? global::Models.User.ConnectedUser.Id : 0;
                List<User> users = DB.Users.ToList()
                    .Where(u => u.Id != connectedId)
                    .OrderBy(u => u.Name)
                    .ToList();

                return PartialView("GetUsers", users);
            }

            return Content("");
        }

        [UserAccess(Access.Admin)]
        public ActionResult SetUserAccess(int userid, int access)
        {
            DB.Events.Add("SetUserAccess");
            if (userid == 1) return Content("");
            if (access < 0 || access > 3) return Content("");

            User user = DB.Users.Get(userid);
            if (user == null) return Content("");

            user.Access = (Access)access;
            DB.Users.Update(user);

            string accessTitle = "Anonyme";
            switch (user.Access)
            {
                case Access.View: accessTitle = "Lecture seule"; break;
                case Access.Write: accessTitle = "Lecture/Ecriture"; break;
                case Access.Admin: accessTitle = "Administrateur"; break;
            }

            try
            {
                AccountsEmailing.SendEmailUserStatusChanged("Vos ayant droits ont ete modifies : " + accessTitle, user);
            }
            catch
            {
                // Keep admin command flow stable even when SMTP is unavailable.
            }

            return Content("ok");
        }

        [UserAccess(Access.Admin)]
        public ActionResult ToggleBlockUser(int id)
        {
            DB.Events.Add("ToggleBlockUser");
            if (id == 1) return Content("");

            User user = DB.Users.Get(id);
            if (user == null) return Content("");

            user.Blocked = !user.Blocked;
            user.Online = false;
            DB.Users.Update(user);

            string message = user.Blocked
                ? "Votre compte a ete bloque par l'administrateur du site."
                : "Votre compte a ete debloque par l'administrateur du site.";

            try
            {
                AccountsEmailing.SendEmailUserStatusChanged(message, user);
            }
            catch
            {
                // Keep admin command flow stable even when SMTP is unavailable.
            }

            return Content("ok");
        }

        [UserAccess(Access.Admin)]
        public ActionResult ForceVerifyUser(int id)
        {
            DB.Events.Add("ForceVerifyUser");
            if (id == 1) return Content("");

            User user = DB.Users.Get(id);
            if (user == null) return Content("");

            user.Verified = true;
            DB.Users.Update(user);

            try
            {
                AccountsEmailing.SendEmailUserStatusChanged("Votre adresse de courriel a ete confirmee par l'administrateur du site.", user);
            }
            catch
            {
                // Keep admin command flow stable even when SMTP is unavailable.
            }

            return Content("ok");
        }

        [UserAccess(Access.Admin)]
        public ActionResult DeleteUser(int id)
        {
            if (id == 1) return Content("");

            User user = DB.Users.Get(id);
            if (user == null) return Content("");

            DB.Events.Add("DeleteUser " + user.Name);
            DB.Users.Delete(id);

            try
            {
                AccountsEmailing.SendEmailUserStatusChanged("Votre compte a ete efface par l'administrateur du site.", user);
            }
            catch
            {
                // Keep admin command flow stable even when SMTP is unavailable.
            }

            return Content("ok");
        }

        private void DisconnectCurrentUser(string eventName = "")
        {
            User connectedUser = global::Models.User.ConnectedUser;
            if (connectedUser == null) return;

            if (!string.IsNullOrWhiteSpace(eventName))
                DB.Events.Add(eventName);

            DB.Logins.UpdateLogoutByUserId(connectedUser.Id);
            connectedUser.Online = false;
            global::Models.User.ConnectedUser = null;
        }

        private ActionResult RedirectToLoginMessage(string message, bool success)
        {
            string encodedMessage = HttpUtility.UrlEncode(message ?? "");
            string encodedSuccess = success ? "true" : "false";
            return Redirect("/Accounts/Login?message=" + encodedMessage + "&success=" + encodedSuccess);
        }
    }
}
