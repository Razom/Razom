using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Razom.Models;
using System.Web.Security;
using DataModel;
using System.IO;

namespace Razom.Controllers
{
    public class PlaceController : Controller
    {
        //
        // GET: /Place/
        [Authorize]
        public ActionResult Index(int id = 1)
        {
            List<ShortPlace> result = new List<ShortPlace>();
            using (var con = new RazomContext())
            {
                HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                string username = ticket.Name;
                int userID = con.Users.SingleOrDefault(u => u.Login == username).UserID;
                result = (from pl in con.Places
                         join fpl in con.FuturePlace on pl.PlaceID equals fpl.PlaceID
                         //join u in con.Users on fpl.UserID equals u.UserID
                         join p in con.PhotosPlace on pl.PlaceID equals p.PlaceID into gj
                         where fpl.UserID == userID
                         from subres in gj.DefaultIfEmpty()
                         select new ShortPlace
                         {
                             ID = fpl.ID,
                             Name = pl.Name,
                             Rating = pl.Rating ?? 0,
                             PlaceID = pl.PlaceID,
                             PhotoUrl = subres == null ? " ": subres.href,
                             PhotoByte = subres.FotoID,
                             City = con.Region.FirstOrDefault(r => r.CityID == pl.CityID).Name,
                             PlaceType = con.PlaceType.FirstOrDefault(pt => pt.PlaceTypeID == pl.PlaceTypeID).Type
                         }).ToList();
                result.ForEach(r =>
                {
                    r.tags = (from t in con.Tag
                              join ttp in con.TagToPlace on t.TagID equals ttp.TagID
                              where ttp.PlaceID == r.PlaceID
                              select t.NameTag).ToList();
                });
            }

            int pageSize = 5;
            int pCount = 0;
            if(result.Count % pageSize == 0)
            {
                pCount = result.Count / pageSize;
            }
            else
                pCount = result.Count / pageSize + 1;

            List<ShortPlace> list = result.OrderBy(p => p.Name).Skip((id - 1) * pageSize).Take(pageSize).ToList();
            PlaceCollection pc = new PlaceCollection() { Places = list, CurrentPage = id, PagesCount = pCount };
            return View(pc);
        }

        [ChildActionOnly]
        public ActionResult MyPlacesButton()
        {
            if (HttpContext.Request.Cookies["role"] != null && User.Identity.IsAuthenticated)
            {
                return PartialView();
            }
            return null;
        }

        [Authorize]
        public ActionResult DeleteFromList(int id)
        {
            using (var db = new RazomContext())
            {
                FuturePlace ID = db.FuturePlace.Find(id);
                if (ID != null)
                {
                    db.FuturePlace.Remove(ID);
                    db.SaveChanges();
                    ViewBag.Message = "Місце успішно видалено зі списку!"; 
                    return View();
                }
            }
            ViewBag.Message = "Щось пройшло не так...";
            return View();
        }

        public ActionResult DisplayPicture(int id = -1)
        {
            if (id != -1)
            {
                using (var db = new RazomContext())
                {
                    byte[] image = db.PhotosPlace.Find(id).FileFoto;
                    if (image == null)
                    {
                        return null;
                    }
                    else
                        return File(image, "image/jpg");
                }
            }
            return null;
        }

        public ActionResult Show(int id = -1, int page = 1)
        {
            if (id != -1)
            {
                FullPlace place = new FullPlace();
                using (var db = new RazomContext())
                {
                    Places p = db.Places.SingleOrDefault(i => i.PlaceID == id);
                    place.City = db.Region.Find(p.CityID).Name;
                    place.Name = p.Name;
                    place.PlaceID = p.PlaceID;
                    place.PlaceType = db.PlaceType.Find(p.PlaceTypeID) == null ? string.Empty : db.PlaceType.Find(p.PlaceTypeID).Type;
                    place.Rating = p.Rating ?? 0;
                    place.tags = (from pl in db.Places
                                  join ttp in db.TagToPlace on pl.PlaceID equals ttp.PlaceID
                                  join t in db.Tag on ttp.TagID equals t.TagID
                                  where pl.PlaceID == id
                                  select t.NameTag).ToList();
                    place.PhotoUrls = (from  img in db.PhotosPlace 
                                           where img.href != null && img.PlaceID == id
                                           select img.href).ToList();
                    place.PhotoBytes = (from img in db.PhotosPlace
                                        where img.FileFoto != null && img.PlaceID == id
                                        select img.FotoID).ToList();
                    HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    if (cookie != null)
                    {
                        FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                        string username = ticket.Name;
                        Users user = db.Users.SingleOrDefault(u => u.Login == username);
                        place.IsEditable = user.IsAdmin ?? false;
                        place.IsAuthorized = true;
                        place.IsInFavorite = db.FuturePlace.Where(fp => fp.PlaceID == id && fp.UserID == user.UserID).Count() == 0 ? false : true;
                    }
                    else
                    {
                        place.IsEditable = false;
                        place.IsAuthorized = false;
                    }

                    place.Comment = db.Comments.Where(c => c.PlaceID == id).ToList();
                    place.Comment = place.Comment.Reverse();
                    int pageSize = 3;
                    int pCount = 0;
                    if (place.Comment.Count() % pageSize == 0)
                    {
                        pCount = place.Comment.Count() / pageSize;
                    }
                    else
                        pCount = place.Comment.Count() / pageSize + 1;
                    place.Comment = place.Comment.Skip((page-1)*pageSize).Take(pageSize).ToList();
                    place.CurrentPage = page;
                    place.PagesCount = pCount;
                }
                return View(place);
            }
            return View();
        }

        private List<ShortPlace> getSearchList(string token)
        {
            PlaceSearchEngine.PlaceSearchEngine engine = new PlaceSearchEngine.PlaceSearchEngine();
            List<int> ids = engine.searchPlaces(token);
            List<ShortPlace> result = new List<ShortPlace>();
            using (var db = new RazomContext())
            {
                ShortPlace sp;
                foreach (int item in ids)
                {
                    Places p = db.Places.Find(item);
                    List<string> Tags = (from t in db.Tag
                                         join ttp in db.TagToPlace on t.TagID equals ttp.TagID
                                         where ttp.PlaceID == item
                                         select t.NameTag).ToList();
                    var photo = db.PhotosPlace.Where(ph => ph.FileFoto != null && ph.PlaceID == p.PlaceID).ToList();
                    var photoUrl = db.PhotosPlace.Where(ph => ph.href != null && ph.PlaceID == p.PlaceID).ToList();
                    sp = new ShortPlace
                    {
                        Name = p.Name,
                        City = db.Region.Find(p.CityID).Name,
                        PlaceID = p.PlaceID,
                        PhotoByte = photo.Count != 0 ? photo.First().FotoID : 0,
                        PhotoUrl = photoUrl.Count != 0 ? photoUrl.First().href : "",
                        Rating = p.Rating ?? 0,
                        PlaceType = p.PlaceType != null ? db.PlaceType.Find(p.PlaceType).Type : string.Empty,
                        tags = Tags
                    };
                    result.Add(sp);
                }
            }
            return result;
        }

        [HttpPost]
        public ActionResult PlaceSearch(string id = "", int page = 1)
        {
            if (id != "")
            {
                List<ShortPlace> result = getSearchList(id);

                int pageSize = 6;
                int pCount = 0;
                if (result.Count % pageSize == 0)
                {
                    pCount = result.Count / pageSize;
                }
                else
                    pCount = result.Count / pageSize + 1;
                var list = result.Take(pageSize).ToList();
                PlaceCollection pc = new PlaceCollection() { Places = list, CurrentPage = page, PagesCount = pCount, Info=id};
                return View(pc);
            }
            return View();
        }

        [HttpPost]
        public ActionResult PagerPlaceSearch(FormCollection form)
        {
            string id = form["id"].ToString();
            int page = int.Parse(form["page"].ToString());
            List<ShortPlace> result = getSearchList(id);
            int pageSize = 5;
            int pCount = 0;
            if (result.Count % pageSize == 0)
            {
                pCount = result.Count / pageSize;
            }
            else
                pCount = result.Count / pageSize + 1;

            var list = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            PlaceCollection pc = new PlaceCollection() { Places = list, CurrentPage = page, PagesCount = pCount, Info = id };
            return View("PlaceSearch", pc);
        }

        [Authorize]
        public ActionResult Create(string message="")
        {
            if (message != "")
	        {
                ModelState.AddModelError(String.Empty,message);
	        }
            PlaceCreator p = new PlaceCreator();
            p.Place = new FullPlace();
            using(var db = new RazomContext())
	        {
		        p.PlaceTypes = new SelectList(db.PlaceType.ToList(),"PlaceTypeID","Type");
                p.Cities = new SelectList(db.Region.ToList(), "CityID", "Name");
	        }
            return View(p);
        }

        [HttpPost]
        public ActionResult Create(PlaceCreator model, HttpPostedFileBase[] files)
        {
            int res = 0;
            if (ModelState.IsValid)
            {
                if (files.Count() > 5)
                {
                    return Create("Не можна додавати більше 5 фото"); 
                }
                using (var db = new RazomContext())
                {
                    Places p = new Places()
                    {
                        CityID = model.SelectedCity,
                        Name = model.Place.Name,
                        PlaceTypeID = model.SelectedPlaceType,
                        Address = model.Place.Address,
                    };
                    db.Places.Add(p);
                    db.SaveChanges();
                    res = p.PlaceID;
                    foreach (var file in files)
                    {
                        byte[] image = new byte[file.ContentLength];
                        using (BinaryReader r = new BinaryReader(file.InputStream))
                        {
                            image = r.ReadBytes(file.ContentLength);
                        }
                        db.Database.ExecuteSqlCommand("INSERT INTO PhotosPlace(PlaceID,FileFoto) Values({0},{1})", p.PlaceID, image);    
                    }
                }
                return RedirectToAction("Show", "Place", new { id = res });
            }
            return Create("Не введена назва або адреса");
        }

        public ActionResult AddToFavorite(int id)
        {
            HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            string username = ticket.Name;
            using (var db = new RazomContext())
            {
                Users user = db.Users.SingleOrDefault(u => u.Login == username);
                var set = db.FuturePlace.Where(p => p.PlaceID == id && p.UserID == user.UserID);
                if (set.Count() == 0)
                {
                    FuturePlace p = new FuturePlace { PlaceID = id, UserID = user.UserID };
                    db.FuturePlace.Add(p);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Show", "Place", new { id=id});
        }

        [HttpPost]
        public ActionResult AddComment(FormCollection form)
        {
            int id = int.Parse(form["id"].ToString());
            string comment = form["comment"].ToString();
            using (var db = new RazomContext())
            {
                Comments c = new Comments { PlaceID = id, Message = comment };
                db.Comments.Add(c);
                db.SaveChanges();
            }
            return RedirectToAction("Show", new { id=id});
        }
    }
}
