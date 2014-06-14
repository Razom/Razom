using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataModel;
using System.Web.Mvc;

namespace Razom.Models
{
    public class ShortTrip
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
    }

    public class TripCollection
    {
        public List<ShortTrip> Trips { get; set; }
        public int CurrentPage { get; set; }
        public int PagesCount { get; set; }
        public bool IncomingInvites { get; set; }
        public string ActionUrl { get; set; }
    }

    public class PostedMessage
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string AuthorName { get; set; }
    }

    public class Trip
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public IEnumerable<Users> Users { get; set; }
        public IEnumerable<Places> Places { get; set; }
        public IEnumerable<PostedMessage> Messages { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
        public bool isEditable { get; set; }
        public int UserID { get; set; }
    }

    public class TripDate
    {
        public int ID { get; set; }
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime Start { get; set; }
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime Finish { get; set; }
    }

    public class PeopleAddForm
    {
        public int TripID { get; set; }
        public string Token { get; set; }
        public AccountCollection AccountsInfo { get; set; }
    }

    public class PlaceAddForm
    {
        public int TripID { get; set; }
        public string Token { get; set; }
        public PlaceCollection PlacesInfo { get; set; }
    }
}