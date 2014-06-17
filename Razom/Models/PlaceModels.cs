using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataModel;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Razom.Models
{
    public class ShortPlace
    {
        public int ID { get; set; }
        public int PlaceID { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> tags { get; set; }
        public int Rating { get; set; }
        public string PhotoUrl { get; set; }
        public int PhotoByte { get; set; }
        public string City { get; set; }
        public string PlaceType { get; set; }
    }

    public class PlaceCollection
    {
        public List<ShortPlace> Places { get; set; }
        public int CurrentPage { get; set; }
        public int PagesCount { get; set; }
        public string Info { get; set;}
    }

    public class FullPlace
    {
        public int PlaceID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        public IEnumerable<string> tags { get; set; }
		public string Coordinates { get; set; }
        public int Rating { get; set; }
        public string City { get; set; }
        public string PlaceType { get; set; }
       public List<Comments> Comment { get; set; }
        public List<string> PhotoUrls { get; set; }
        public List<int> PhotoBytes { get; set; }
        public bool IsEditable { get; set; }
        public bool IsAuthorized { get; set; }
        public bool IsInFavorite { get; set; }

        public int CurrentPage { get; set; }
        public int PagesCount { get; set; }
    }


    public class PlaceCreator
    {
        public FullPlace Place { get; set; }
        
        public int SelectedPlaceType { get; set; }
        public SelectList PlaceTypes { get; set; }

        public int SelectedCity { get; set; }
        public SelectList Cities { get; set; }
    }
}