using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Razom.Models;
using DataModel;
using System.Web.Security;

namespace Razom.Controllers
{
    public class TripController : Controller
    {
        //
        // GET: /Trip/

        [Authorize]
        public ActionResult Index(int id = 1)
        {
            List<ShortTrip> result = new List<ShortTrip>();
            bool invites = false;
            using(var db = new RazomContext())
	        {
                HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                int user = db.Users.Where(u => u.Login == ticket.Name).SingleOrDefault().UserID;

                result = (from t in db.Travels
                          join h in db.History on t.TravelID equals h.TravelID
                          where h.UserID == user
                          select new ShortTrip
                        {
                            Date = t.DateFinish < DateTime.Now ? "Минула" : (t.DateStart > DateTime.Now ? "Буде в майбутньому" : "Відбувається зараз"),
                            Name = t.Name,
                            ID = t.TravelID
                        }).ToList();
                   
                if (db.Invite.Where( i => i.UserID == user).Count() > 0)
                {
                    invites = true;
                }
	        }
            int pageSize = 6;
            int pCount = 0;
            if (result.Count % pageSize == 0)
            {
                pCount = result.Count / pageSize;
            }
            else
                pCount = result.Count / pageSize + 1;

            var list = result.Skip((id - 1) * pageSize).Take(pageSize).ToList();
            TripCollection tc = new TripCollection() { Trips = list, PagesCount = pCount, CurrentPage = id, IncomingInvites = invites, ActionUrl="Index"};
            return View(tc);
        }

        public ActionResult Show(int id)
        {
            Trip t = new Trip();
            using (var db = new RazomContext())
            {
                t.Finish = db.Travels.Find(id).DateFinish ?? DateTime.Now.AddDays(1);
                t.Start = db.Travels.Find(id).DateStart ?? DateTime.Now;
                t.Name = db.Travels.Find(id).Name;
                t.Places = (from tr in db.Travels
                            join tp in db.TravelPlaces on tr.TravelID equals tp.TravelID
                            join pl in db.Places on tp.PlaceID equals pl.PlaceID
                            where tr.TravelID == id
                            select pl).ToList();
                t.Users = (from tr in db.Travels
                           join h in db.History on tr.TravelID equals h.TravelID
                           join u in db.Users on h.UserID equals u.UserID
                           where tr.TravelID == id
                           select u).ToList();
                t.ID = id;
                t.Messages = (from tr in db.Message
                              join u in db.Users on tr.UserID equals u.UserID
                              where tr.TravelID == id
                              select new PostedMessage { 
                               AuthorName = u.FirstName + " " + u.SecondName,
                               ID = tr.MessageID,
                               Text = tr.Text
                              }).ToList();
                             
                HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                int user = db.Users.Where(u => u.Login == ticket.Name).SingleOrDefault().UserID;
                t.UserID = user;
                if (db.History.Where(h => h.UserID == user).Count() > 0)
                {
                    t.isEditable = true;
                }
                else
                    t.isEditable = false;
            }
            return View(t);
        }

        [ChildActionOnly]
        public ActionResult MyTripsButton()
        {
            if (HttpContext.Request.Cookies["role"] != null && User.Identity.IsAuthenticated)
            {
                return PartialView();
            }
            return null;
        }

        [Authorize]
        public ActionResult ChangeName(int id)
        {
            ViewBag.ID = id;
            using (var db = new RazomContext())
            {
                ViewBag.Name = db.Travels.Find(id).Name;
            }
            return View();
        }

        [HttpPost]
        public ActionResult ChangeName(FormCollection form)
        {
            string name = form["new_name"].ToString();
            if (!(String.IsNullOrEmpty(name) || String.IsNullOrWhiteSpace(name)))
            {
                using (var db = new RazomContext())
                {
                    Travels t = db.Travels.Find(int.Parse(form["id"].ToString()));
                    t.Name = name;
                    db.SaveChanges();
                }    
            }
            
            return RedirectToAction("Show", "Trip", new { id = form["id"].ToString() });
        }

        public ActionResult ChangeDate(int id)
        {
            TripDate td = new TripDate();
            using (var db = new RazomContext())
            {
                td.ID = id;
                td.Start = db.Travels.Find(id).DateStart ?? DateTime.Now;
                td.Finish = db.Travels.Find(id).DateFinish ?? DateTime.Now.AddDays(1);
            }
            return View(td);
        }

        [HttpPost]
        public ActionResult ChangeDate(TripDate model)
        {
            if (ModelState.IsValid)
            {
                if (model.Start > model.Finish)
                {
                    ModelState.AddModelError("", "Дата початку має бути раніше за дату кінця");
                    return View();
                }
                if (Math.Abs(model.Start.Year - DateTime.Now.Year) > 100)
                {
                    ModelState.AddModelError("", "Неправильно введена дата початку");
                    return View();
                }
                if (Math.Abs(model.Finish.Year - DateTime.Now.Year) > 100)
                {
                    ModelState.AddModelError("", "Неправильно введена дата кінця");
                    return View();
                }
                using (var db = new RazomContext())
                {
                    Travels t = db.Travels.Find(model.ID);
                    t.DateStart = model.Start;
                    t.DateFinish = model.Finish;
                    db.SaveChanges();
                }
                return RedirectToAction("Show", new { id = model.ID });    
            }
            return View();
        }

        public ActionResult AddUser(int id)
        {
            return View(new PeopleAddForm() { TripID = id, AccountsInfo = new AccountCollection() { PagesCount=0, CurrentPage=1, Accounts = new List<AccountModel>() } });
        }

        [HttpPost]
        public ActionResult AddUser(PeopleAddForm model)
        {
            List<AccountModel> people = new List<AccountModel>();
            using (var db = new RazomContext())
            {
                List<int> peopleIDs = PeopleSearchEngine.PeopleSearchEngine.GetSearchResultIndexes(model.Token);
                List<int> peopleInThisTrip = (from u in db.Users
                                              join h in db.History on u.UserID equals h.UserID
                                              join tr in db.Travels on h.TravelID equals tr.TravelID
                                              where tr.TravelID == model.TripID
                                              select u.UserID).ToList();
                List<int> invitedPeople = (from u in db.Users
                                           join inv in db.Invite on u.UserID equals inv.UserID
                                           join tr in db.Travels on inv.TravelID equals tr.TravelID
                                           where tr.TravelID == model.TripID
                                           select u.UserID).ToList();
             
                foreach (var item in peopleIDs)
                {
                    if (!(peopleInThisTrip.Contains(item) || invitedPeople.Contains(item)))
                    {
                        people.Add(db.Users.Where(u => u.UserID == item).Select(u => new AccountModel
                          {
                              Login = u.Phone,
                              ID = u.UserID,
                              FirstName = u.FirstName,
                              SecondName = u.SecondName,
                              EMail = u.Email,
                              Phone = u.Phone
                          }).FirstOrDefault());
                    }
                }
            }

            int pageSize = 6;
            int pCount = 0;
            if (people.Count % pageSize == 0)
            {
                pCount = people.Count / pageSize;
            }
            else
                pCount = people.Count / pageSize + 1;

            people = people.Take(pageSize).ToList();
            PeopleAddForm paf = new PeopleAddForm() { Token = model.Token, TripID = model.TripID, AccountsInfo = new AccountCollection() { Accounts=people, CurrentPage=1, PagesCount=pCount } }; 
            return View(paf);
        }

        [HttpPost]
        public ActionResult PagerAddUser(FormCollection form)
        {
            List<AccountModel> people = new List<AccountModel>();
            int page = int.Parse(form["page"].ToString());
            int travel_id = int.Parse(form["t_id"].ToString());
            string token = form["id"].ToString();
            using (var db = new RazomContext())
            {
                List<int> peopleIDs = PeopleSearchEngine.PeopleSearchEngine.GetSearchResultIndexes(token);
                List<int> peopleInThisTrip = (from u in db.Users
                                              join h in db.History on u.UserID equals h.UserID
                                              join tr in db.Travels on h.TravelID equals tr.TravelID
                                              where tr.TravelID == travel_id
                                              select u.UserID).ToList();
                List<int> invitedPeople = (from u in db.Users
                                           join inv in db.Invite on u.UserID equals inv.UserID
                                           join tr in db.Travels on inv.TravelID equals tr.TravelID
                                           where tr.TravelID == travel_id
                                           select u.UserID).ToList();
                foreach (var item in peopleIDs)
                {
                    if (!(peopleInThisTrip.Contains(item)||invitedPeople.Contains(item)))
                    {
                        people.Add(db.Users.Where(u => u.UserID == item).Select(u => new AccountModel
                        {
                            Login = u.Phone,
                            ID = u.UserID,
                            FirstName = u.FirstName,
                            SecondName = u.SecondName,
                            EMail = u.Email,
                            Phone = u.Phone
                        }).FirstOrDefault());
                    }
                }
            }

            int pageSize = 6;
            int pCount = 0;
            if (people.Count % pageSize == 0)
            {
                pCount = people.Count / pageSize;
            }
            else
                pCount = people.Count / pageSize + 1;

            people = people.Skip((page-1)*pageSize).Take(pageSize).ToList();
            PeopleAddForm paf = new PeopleAddForm() { Token = token, TripID = travel_id, AccountsInfo = new AccountCollection() { Accounts = people, CurrentPage = page, PagesCount = pCount } };
            return View("AddUser",paf);
        }

        public ActionResult ConfirmAddUser(int id, int uid)
        {
            string name = "";
            using (var db = new RazomContext())
            {
                Users user = db.Users.Find(uid);
                
                name = user.FirstName + " " + user.SecondName;
                ViewBag.Name = name;
                ViewBag.id = id;
                Invite inv = new Invite();
                inv.TravelID = id;
                inv.UserID = uid;
                db.Invite.Add(inv);
                db.SaveChanges();
            }
            return View();
        }

        public ActionResult AddPlace(int id)
        {
            return View(new PlaceAddForm() { TripID = id, PlacesInfo= new PlaceCollection() { PagesCount = 0, CurrentPage = 1, Places = new List<ShortPlace>() } });
        }

        [HttpPost]
        public ActionResult AddPlace(PlaceAddForm model)
        {
            List<ShortPlace> places = new List<ShortPlace>();
            using (var db = new RazomContext())
            {
                PlaceSearchEngine.PlaceSearchEngine engine = new PlaceSearchEngine.PlaceSearchEngine();
                List<int> placeIDs = engine.searchPlaces(model.Token);
                List<int> placeInThisTrip = (from pl in db.Places
                                                 join tp in db.TravelPlaces on pl.PlaceID equals tp.PlaceID
                                                 join tr in db.Travels on tp.TravelID equals tr.TravelID
                                                 where tr.TravelID == model.TripID
                                                 select pl.PlaceID).ToList();
                foreach (var item in placeIDs)
                {
                    if (!placeInThisTrip.Contains(item))
                    {
                        places.Add(db.Places.Where(p => p.PlaceID == item).Select(p => new ShortPlace
                        {
                            ID = p.PlaceID,
                            Name = p.Name
                        }).FirstOrDefault());
                    }
                }
            }

            int pageSize = 6;
            int pCount = 0;
            if (places.Count % pageSize == 0)
            {
                pCount = places.Count / pageSize;
            }
            else
                pCount = places.Count / pageSize + 1;

            places = places.Take(pageSize).ToList();
            PlaceAddForm paf = new PlaceAddForm() { Token = model.Token, TripID = model.TripID, PlacesInfo = new PlaceCollection() { Places = places, CurrentPage = 1, PagesCount = pCount } };
            return View(paf);
        }

        [HttpPost]
        public ActionResult PagerAddPlace(FormCollection form)
        {
            List<ShortPlace> places = new List<ShortPlace>();
            int page = int.Parse(form["page"].ToString());
            int travel_id = int.Parse(form["t_id"].ToString());
            string token = form["id"].ToString();
            using (var db = new RazomContext())
            {
                PlaceSearchEngine.PlaceSearchEngine engine = new PlaceSearchEngine.PlaceSearchEngine();
                List<int> placeIDs = engine.searchPlaces(token);
                List<int> placeInThisTrip = (from pl in db.Places
                                             join tp in db.TravelPlaces on pl.PlaceID equals tp.PlaceID
                                             join tr in db.Travels on tp.TravelID equals tr.TravelID
                                             where tr.TravelID == travel_id
                                             select pl.PlaceID).ToList();
                foreach (var item in placeIDs)
                {
                    if (!placeInThisTrip.Contains(item))
                    {
                        places.Add(db.Places.Where(p => p.PlaceID == item).Select(p => new ShortPlace
                        {
                            ID = p.PlaceID,
                            Name = p.Name
                        }).FirstOrDefault());
                    }
                }
            }

            int pageSize = 6;
            int pCount = 0;
            if (places.Count % pageSize == 0)
            {
                pCount = places.Count / pageSize;
            }
            else
                pCount = places.Count / pageSize + 1;

            places = places.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            PlaceAddForm paf = new PlaceAddForm() { Token = token, TripID = travel_id, PlacesInfo = new PlaceCollection() { Places = places, CurrentPage = page, PagesCount = pCount } };
            return View("AddPlace",paf);
        }

        public ActionResult ConfirmAddPlace(int id, int uid)
        {
            string name = "";
            using (var db = new RazomContext())
            {
                Places place = db.Places.Find(uid);
                name = place.Name;
                ViewBag.Name = name;
                ViewBag.id = id;

                TravelPlaces tp = new TravelPlaces();
                tp.TravelID = id;
                tp.PlaceID = uid;
                db.TravelPlaces.Add(tp);
                db.SaveChanges();
            }
            return View();
        }

        public ActionResult LeftTrip(int id)
        {
            HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            string username = ticket.Name;
            using (var db = new RazomContext())
            {
                int userID = db.Users.SingleOrDefault(u => u.Login == username).UserID;
                History hist = db.History.Where(h => h.TravelID == id && h.UserID == userID).First();
                db.History.Remove(hist);
                db.SaveChanges();
                if (db.History.Where(h => h.TravelID == id).Count() == 0)
                {
                    var conect = db.TravelPlaces.Where(t => t.TravelID == id);
                    foreach (var item in conect)
                    {
                        db.TravelPlaces.Remove(item);
                    }
                    db.SaveChanges();
                    Travels tr = db.Travels.Find(id);
                    db.Travels.Remove(tr);
                    db.SaveChanges();
                }
            }
            return View();
        }

        [Authorize]
        public ActionResult Invites(int id = 1)
        {
            HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            string username = ticket.Name;
            List<ShortTrip> trips = new List<ShortTrip>();

            using (var db = new RazomContext())
            {
                Users user = db.Users.Where(u => u.Login == username).FirstOrDefault();
                trips = (from tr in db.Travels
                         join inv in db.Invite on tr.TravelID equals inv.TravelID
                         join u in db.Users on inv.UserID equals u.UserID
                         where u.UserID == user.UserID
                         select new ShortTrip
                         {
                             Date = tr.DateFinish < DateTime.Now ? "Минула" : (tr.DateStart > DateTime.Now ? "Буде в майбутньому" : "Відбувається зараз"),
                             ID = tr.TravelID,
                             Name = tr.Name
                         }).ToList();
            }

            int pageSize = 6;
            int pCount = 0;
            if (trips.Count % pageSize == 0)
            {
                pCount = trips.Count / pageSize;
            }
            else
                pCount = trips.Count / pageSize + 1;

            trips = trips.Skip((id - 1) * pageSize).Take(pageSize).ToList();
            TripCollection tc = new TripCollection { Trips = trips, CurrentPage = id, PagesCount = pCount, IncomingInvites = false };
            
            return View(tc);
        }

        public ActionResult ConfirmTrip(int id)
        {
            HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            string username = ticket.Name;

            using (var db = new RazomContext())
            {
                Users user = db.Users.Where(u => u.Login == username).FirstOrDefault();
                Invite inv = db.Invite.Where(i => i.TravelID == id).FirstOrDefault();
                db.Invite.Remove(inv);
                db.SaveChanges();
                History h = new History { UserID = user.UserID, TravelID = id };
                db.History.Add(h);
                db.SaveChanges();
            }
            return RedirectToAction("Show", "Trip", new { id=id });            
        }

        public ActionResult FutureTrips(int id = 1)
        {
            List<ShortTrip> result = new List<ShortTrip>();
            bool invites = false;
            using (var db = new RazomContext())
            {
                result = (from t in db.Travels
                          join h in db.History on t.TravelID equals h.TravelID
                          where t.DateStart > DateTime.Now
                          select new ShortTrip
                          {
                              Date = "Буде в майбутньому",
                              Name = t.Name,
                              ID = t.TravelID
                          }).ToList();
                HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                int user = db.Users.Where(u => u.Login == ticket.Name).SingleOrDefault().UserID;

                if (db.Invite.Where(i => i.UserID == user).Count() > 0)
                {
                    invites = true;
                }
            }
            int pageSize = 6;
            int pCount = 0;
            if (result.Count % pageSize == 0)
            {
                pCount = result.Count / pageSize;
            }
            else
                pCount = result.Count / pageSize + 1;

            var list = result.Skip((id - 1) * pageSize).Take(pageSize).ToList();
            TripCollection tc = new TripCollection() { Trips = list, PagesCount = pCount, CurrentPage = id, IncomingInvites = invites, ActionUrl = "FututeTrips" };
            return View("Index",tc);
        }

        public ActionResult CurrentTrips(int id = 1)
        {
            List<ShortTrip> result = new List<ShortTrip>();
            bool invites = false;
            using (var db = new RazomContext())
            {
                result = (from t in db.Travels
                          join h in db.History on t.TravelID equals h.TravelID
                          where t.DateStart <= DateTime.Now && t.DateFinish >= DateTime.Now
                          select new ShortTrip
                          {
                              Date = "Відбувається зараз",
                              Name = t.Name,
                              ID = t.TravelID
                          }).ToList();
                HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                int user = db.Users.Where(u => u.Login == ticket.Name).SingleOrDefault().UserID;

                if (db.Invite.Where(i => i.UserID == user).Count() > 0)
                {
                    invites = true;
                }
            }
            int pageSize = 6;
            int pCount = 0;
            if (result.Count % pageSize == 0)
            {
                pCount = result.Count / pageSize;
            }
            else
                pCount = result.Count / pageSize + 1;

            var list = result.Skip((id - 1) * pageSize).Take(pageSize).ToList();
            TripCollection tc = new TripCollection() { Trips = list, PagesCount = pCount, CurrentPage = id, IncomingInvites = invites, ActionUrl = "CurrentTrips" };
            return View("Index",tc);
        }

        public ActionResult PastTrips(int id = 1)
        {
            List<ShortTrip> result = new List<ShortTrip>();
            bool invites = false;
            using (var db = new RazomContext())
            {
                result = (from t in db.Travels
                          join h in db.History on t.TravelID equals h.TravelID
                          where t.DateFinish < DateTime.Now
                          select new ShortTrip
                          {
                              Date = "Минула",
                              Name = t.Name,
                              ID = t.TravelID
                          }).ToList();
                HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                int user = db.Users.Where(u => u.Login == ticket.Name).SingleOrDefault().UserID;

                if (db.Invite.Where(i => i.UserID == user).Count() > 0)
                {
                    invites = true;
                }
            }
            int pageSize = 6;
            int pCount = 0;
            if (result.Count % pageSize == 0)
            {
                pCount = result.Count / pageSize;
            }
            else
                pCount = result.Count / pageSize + 1;

            var list = result.Skip((id - 1) * pageSize).Take(pageSize).ToList();
            TripCollection tc = new TripCollection() { Trips = list, PagesCount = pCount, CurrentPage = id, IncomingInvites = invites, ActionUrl = "PastTrips" };
            return View("Index", tc);
        }

        public ActionResult Create()
        {
            return View(new Trip());
        }

        [HttpPost]
        public ActionResult Create(Trip trip)
        {
            if (ModelState.IsValid)
            {
                if (trip.Start > trip.Finish)
                {
                    ModelState.AddModelError("", "Дата початку має бути раніше за дату кінця");
                    return View();
                }
                if (Math.Abs(trip.Start.Year - DateTime.Now.Year) >100)
                {
                    ModelState.AddModelError("", "Неправильно введена дата початку");
                    return View();
                }
                if (Math.Abs(trip.Finish.Year - DateTime.Now.Year) > 100)
                {
                    ModelState.AddModelError("", "Неправильно введена дата кінця");
                    return View();
                }
                using (var db = new RazomContext())
                {
                    HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                    Users user = db.Users.Where(u => u.Login == ticket.Name).SingleOrDefault();

                    Travels t = new Travels { DateStart = trip.Start, DateFinish = trip.Finish, Name = trip.Name };
                    db.Travels.Add(t);
                    db.SaveChanges();
                    db.History.Add(new History { UserID = user.UserID, TravelID = t.TravelID });
                    db.SaveChanges();
                    return RedirectToAction("Show", "Trip", new { id = t.TravelID });
                }    
            }

            return View();
        }
    }
}
