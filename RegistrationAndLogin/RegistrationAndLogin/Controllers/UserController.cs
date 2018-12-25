using RegistrationAndLogin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RegistrationAndLogin.Controllers
{
    public class UserController : Controller
    {
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //Registration POST Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude= "IsEmailVerified,ActivationCode")] User user) //this two properties are not in user table so this bind operation automatically add this data in result
        {
            bool Status = false;
            string message = "";
            //
            //Model validation
            if(ModelState.IsValid)
            {
                #region //Email alredy exist or not
                var isExist = IsEmailExist(user.EmailID);
                if(isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                #endregion

                #region //Generate Activation Code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion
                user.IsEmailVerified = false;

                #region Save to Database
                using (MyDatabaseEntities dc = new MyDatabaseEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();
                }
                //Send Email to user
                SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString());

                #endregion
            }
            else
            {
                message = "Invalid Request";
            }


            //Passwod Hashing


            return View(user);
        }

        //Verify Email

        //Verify Email Link

        //Login

        //Login POST

        //Logout
        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return v != null;

            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID,string activationCode)
        {
           
        }
    }
    
}