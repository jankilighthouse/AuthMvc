using RegistrationAndLogin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Net;

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
                message = "Registration successfully done.Account activation link " +
                    "has been sent to your email id:" + user.EmailID;
                Status = true;
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }
        //verify account
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
            var verifyUrl = "User/verifyAccount/" + activationCode;
            var link = Request.Url.AbsolutePath.Replace(Request.Url.PathAndQuery, verifyUrl);
            var fromEmail = new MailAddress("jenpatel6890@gmail.com","Dotnet Great");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "J@nkit8488"; // Replace with original password
            string subject = "Your account is successfully created";

            string body = "<br/><br/>We are excited to tell you taht your Dotnet Great account is" +
                "successfully created.Please click on the below link to verify your account" +
                "<br/><br/><a href='"+link+"'>"+link+"</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address,fromEmailPassword)

            };
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);



        }
    }
    
}