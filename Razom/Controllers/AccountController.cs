using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Razom.Filters;
using Razom.Models;
using System.IO;
using DataModel;
using PeopleSearchEngine;
using FoursquareLib;
using TwitterLib;
using VKLib;
using System.Threading;

namespace Razom.Controllers
{
    public class AccountController : Controller
    {
        [ChildActionOnly]
        public ActionResult AccountButton()
        {
            if (HttpContext.Request.Cookies["role"] != null && User.Identity.IsAuthenticated)
            {
                return PartialView("AccountButton");
            }
            else
                return PartialView("EnterButton");
        }

        public ActionResult Login(string returnUrl)
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new RazomContext())
                {
                    Users user = db.Users.SingleOrDefault(u => u.Login == model.UserName);

                    if (null != user)
                    {
                        FormsAuthentication.SetAuthCookie(model.UserName, true);
                        string role;
                        if (user.IsAdmin ?? false)
	                    {
		                    role = "admin";
	                    }
                        else
                            role = "user";

                        FormsAuthentication.SetAuthCookie(model.UserName, false);
                        var roleCoockie = new HttpCookie("role", role);
                        HttpContext.Response.Cookies.Add(roleCoockie);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Неправильный пароль чи логін");
                    }

                }

            }
            return View(model);
        }

        [Authorize]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            HttpContext.Response.Cookies.Remove("role");
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Register()
        {
            return View("RegisterOne",new FirstRegisterModel());
        }
        
        [HttpPost]
        public ActionResult RegisterOne(FirstRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                int id = 0;
                using (var db = new RazomContext())
                {
                    Users copyUser = db.Users.Where(u => u.Login == model.UserName).SingleOrDefault();
                    if (copyUser != null)
                    {
                        ModelState.AddModelError("key", "Користувач з таким логіном уже існує!");
                        return View();
                    }
                    Users user = new Users
                    {
                        Login = model.UserName,
                        IsAdmin = false,
                        Password = model.Password,
                        Email = model.EMail,
                        FirstName = model.FirstName,
                        SecondName = model.SecondName
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                    user = db.Users.FirstOrDefault(u => u.Login == model.UserName);
                    id = user.UserID;

                    FormsAuthentication.SetAuthCookie(model.UserName, false);
                    var roleCoockie = new HttpCookie("role", "user");
                    HttpContext.Response.Cookies.Add(roleCoockie);
                }
                return View("RegisterTwo",new SecondRegisterModel { ID = id });
            }
            return View();
        }

        [HttpPost]
        public ActionResult RegisterTwo(SecondRegisterModel model, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                using (var db = new RazomContext())
                {
                    if (file != null)
                    {
                        byte[] image = new byte[file.ContentLength];
                        using (BinaryReader r = new BinaryReader(file.InputStream))
                        {
                            image = r.ReadBytes(file.ContentLength);
                        }
                        db.Database.ExecuteSqlCommand("UPDATE Users SET Phone={0}, Avatar={1} WHERE UserID={2}", model.Phone, image, model.ID);    
                    }
                    
                    NetworkAccounts a1 = new NetworkAccounts { UserID = model.ID, NetworkID = db.Network.SingleOrDefault(i => i.Name == "foursquare").NetworkID, ProfileURL = model.FourSquareAccount.Substring(model.FourSquareAccount.LastIndexOf('/')+1) };
                    NetworkAccounts a2 = new NetworkAccounts { UserID = model.ID, NetworkID = db.Network.SingleOrDefault(i => i.Name == "twitter").NetworkID, ProfileURL = model.TwitterAccount.Replace("@","") };
                    NetworkAccounts a3 = new NetworkAccounts { UserID = model.ID, NetworkID = db.Network.SingleOrDefault(i => i.Name == "vk").NetworkID, ProfileURL = model.VkAccount.Substring(model.VkAccount.LastIndexOf('/')+1) };
                    if(!string.IsNullOrEmpty(model.FourSquareAccount))
                        db.NetworkAccounts.Add(a1);
                    if (!string.IsNullOrEmpty(model.TwitterAccount))
                        db.NetworkAccounts.Add(a2);
                    if(!string.IsNullOrEmpty(model.VkAccount))
                        db.NetworkAccounts.Add(a3);
                    db.SaveChanges();
                    return View("EndReg");
                }
            }
            return View();
        }

        [Authorize]
        public ActionResult MyAccount()
        {
            Users user = new Users();
            using (var db = new RazomContext())
            {
                HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                string username = ticket.Name;
                user = db.Users.SingleOrDefault(u => u.Login == username);
                if (user == null)
                {
                    return RedirectToAction("LogOff"); 
                }
                NetworkAccounts fs = db.NetworkAccounts.Where(i => i.NetworkID == db.Network.FirstOrDefault(j => j.Name == "foursquare").NetworkID).FirstOrDefault(i => i.UserID == user.UserID);
                NetworkAccounts tw = db.NetworkAccounts.Where(i => i.NetworkID == db.Network.FirstOrDefault(j => j.Name == "twitter").NetworkID).FirstOrDefault(i => i.UserID == user.UserID);
                NetworkAccounts vk = db.NetworkAccounts.Where(i => i.NetworkID == db.Network.FirstOrDefault(j => j.Name == "vk").NetworkID).FirstOrDefault(i => i.UserID == user.UserID);
                AccountModel a = new AccountModel()
                {
                 ID = user.UserID,
                 Login = user.Login,
                 FirstName = user.FirstName ?? "",
                 SecondName = user.SecondName ?? "",
                 EMail = user.Email ?? "",
                 Phone = user.Phone ?? "",
                 FoursquareAccount = fs != null? fs.ProfileURL: "",
                 TwitterAccount = tw != null? tw.ProfileURL: "",
                 VKAccount = vk != null? vk.ProfileURL: ""
                };
                return View(a);
            }
        }

        public ActionResult DisplayAvatar(int id = -1)
        {
            if (id != -1)
            {
                using (var db = new RazomContext())
                {
                    byte[] image = db.Users.Find(id).Avatar;
                    if (image == null)
                    {
                        return File("/Images/no-photo-available.jpg","image/jpg");
                    }
                    else
                       return File(image, "image/jpg");
                }
            }
            return null;
        }

        [Authorize]
        public ActionResult DeleteAccount()
        {
            HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            string username = ticket.Name;
            using(var db = new RazomContext())
	        {
		        Users user = db.Users.SingleOrDefault(u => u.Login == username);
                db.Database.ExecuteSqlCommand("delete from FuturePlace where UserID={0}", user.UserID);
                db.Database.ExecuteSqlCommand("delete from NetworkAccounts where UserID={0}", user.UserID);
                db.Database.ExecuteSqlCommand("delete from UsersData where UserID={0}", user.UserID);
                db.Database.ExecuteSqlCommand("delete from TestData where UserID={0}", user.UserID);
                db.Database.ExecuteSqlCommand("delete from History where userID={0}", user.UserID);
                db.Database.ExecuteSqlCommand("delete from Message where userID={0}", user.UserID);
                db.Users.Remove(user);
                db.SaveChanges();
	        }
            FormsAuthentication.SignOut();
            HttpContext.Response.Cookies.Remove("role");
            return View();
        }

        private List<AccountModel> getSearchList(string token)
        {
            List<int> ids = PeopleSearchEngine.PeopleSearchEngine.GetSearchResultIndexes(token);
            List<AccountModel> result = new List<AccountModel>();
            using (var db = new RazomContext())
            {
                AccountModel a;
                foreach (int item in ids)
                {
                    Users p = db.Users.Find(item);
                    int photo = db.Users.Where(ph => ph.Avatar != null).SingleOrDefault(ph => ph.UserID == p.UserID) != null ? db.Users.Where(ph => ph.Avatar != null).SingleOrDefault(ph => ph.UserID == p.UserID).UserID : 0;
                    NetworkAccounts fs = db.NetworkAccounts.Where(ac => ac.NetworkID == 1).FirstOrDefault(ac => ac.UserID == p.UserID);
                    NetworkAccounts tw = db.NetworkAccounts.Where(ac => ac.NetworkID == 2).FirstOrDefault(ac => ac.UserID == p.UserID);
                    NetworkAccounts vk = db.NetworkAccounts.Where(ac => ac.NetworkID == 3).FirstOrDefault(ac => ac.UserID == p.UserID);
                    a = new AccountModel
                    {
                        ID = p.UserID,
                        FirstName = p.FirstName,
                        SecondName = p.SecondName,
                        EMail = p.Email,
                        Login = p.Login,
                        Phone = p.Phone,
                        FoursquareAccount = fs != null ? fs.ProfileURL : "",
                        TwitterAccount = tw != null ? tw.ProfileURL : "",
                        VKAccount = vk != null ? vk.ProfileURL : ""
                    };
                    result.Add(a);
                }
            }
            return result;
        }

        [HttpPost]
        public ActionResult PeopleSearch(string id)
        {
            if (id != "")
            {
                List<AccountModel> result = getSearchList(id);

                int pageSize = 6;
                int pCount = 0;
                if (result.Count % pageSize == 0)
                {
                    pCount = result.Count / pageSize;
                }
                else
                    pCount = result.Count / pageSize + 1;
                var list = result.Take(pageSize).ToList();
                AccountCollection ac = new AccountCollection { Accounts = list, CurrentPage = 1, Info = id, PagesCount = pCount };
                return View(ac);
            }
            return View();
        }


        [HttpPost]
        public ActionResult PagerPeopleSearch(FormCollection form)
        {
            string id = form["id"].ToString();
            int page = int.Parse(form["page"].ToString());
            List<AccountModel> result = getSearchList(id);
            int pageSize = 6;
            int pCount = 0;
            if (result.Count % pageSize == 0)
            {
                pCount = result.Count / pageSize;
            }
            else
                pCount = result.Count / pageSize + 1;

            var list = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            AccountCollection pc = new AccountCollection() { Accounts = list, CurrentPage = page, PagesCount = pCount, Info = id };
            return View("PeopleSearch", pc);
        }

        public ActionResult Account(int id)
        {
            Users user = new Users();
            using (var db = new RazomContext())
            {
                user = db.Users.Find(id);
                if (user == null)
                {
                    return View("Error", new HandleErrorInfo(new Exception("Користувача не знайдено!"),"Account","Account"));
                }
                NetworkAccounts fs = db.NetworkAccounts.Where(i => i.NetworkID == db.Network.FirstOrDefault(j => j.Name == "foursquare").NetworkID).FirstOrDefault(i => i.UserID == user.UserID);
                NetworkAccounts tw = db.NetworkAccounts.Where(i => i.NetworkID == db.Network.FirstOrDefault(j => j.Name == "twitter").NetworkID).FirstOrDefault(i => i.UserID == user.UserID);
                NetworkAccounts vk = db.NetworkAccounts.Where(i => i.NetworkID == db.Network.FirstOrDefault(j => j.Name == "vk").NetworkID).FirstOrDefault(i => i.UserID == user.UserID);
                AccountModel a = new AccountModel()
                {
                    ID = user.UserID,
                    Login = user.Login,
                    FirstName = user.FirstName ?? "",
                    SecondName = user.SecondName ?? "",
                    EMail = user.Email ?? "",
                    Phone = user.Phone ?? "",
                    FoursquareAccount = fs != null ? fs.ProfileURL : "",
                    TwitterAccount = tw != null ? tw.ProfileURL : "",
                    VKAccount = vk != null ? vk.ProfileURL : ""
                };
                return View(a);
            }
        }

        public ActionResult ActicateAccount(string id)
        {
            if (id == "fs")
            {
                return Redirect("https://foursquare.com/oauth2/authenticate?client_id=YRRHVMJV1QOGLS23RLWMKN2S3NF1B13C1ROF22N5Z1UT2YYN&response_type=code&redirect_uri=http://brainiac09-001-site1.myasp.net/Account/ConfirmNetworkAccount");
            }
            else
                if (id == "tw")
                {
                    HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                    int userID = 0;
                    using(var db = new RazomContext())
	                {
		                userID = db.Users.Where(u => u.Login == ticket.Name).SingleOrDefault().UserID;
	                }
                    Thread task = new Thread(t => Twitter.WriteTwitterDataToDB(userID));
                    task.Start();
                    ViewBag.Network = id;
                    ViewBag.UserID = userID;
                    return View();
                }
                else
                    if (id == "vk")
                    {
                        HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                        FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                        int userID = 0;
                        using (var db = new RazomContext())
                        {
                            userID = db.Users.Where(u => u.Login == ticket.Name).SingleOrDefault().UserID;
                        }
                        Thread task = new Thread(t => VK.WriteUserVkDataToDB(userID));
                        task.Start();
                        ViewBag.Network = id;
                        ViewBag.UserID = userID;
                        return View();    
                    }
            ViewBag.Network = "Така соціальна мережа не підтримуєтсья!";
            return View();
        }

        public ActionResult ConfirmNetworkAccount(string code)
        {
            HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            int userID = 0;
            using(var db = new RazomContext())
	        {
		        userID = db.Users.Where(u => u.Login == ticket.Name).SingleOrDefault().UserID;
	        }
            ViewBag.Network = "Foursquare";
            ViewBag.UserID = userID;
            Foursquare.WriteAccessTokenToDB(code, userID);
            System.Threading.Thread task = new System.Threading.Thread(t => Foursquare.WriteVisitedPlacesToDB(userID));
            task.Start();
            return View();
        }
    }
}
