using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using DataModel;

namespace Razom
{
    public class ChatHub : Hub
    {
        public void Send(int user_id, int travel_id, string message)
        {
            string name = "";
            using (var db = new RazomContext())
            {
                Users u = db.Users.Find(user_id);
                name = u.FirstName + " " + u.SecondName;
                Message m = new Message
                {
                    Time = DateTime.Now,
                    TravelID = travel_id,
                    UserID = user_id,
                    Text = message
                };
                db.Message.Add(m);
                db.SaveChanges();
            }
            Clients.All.addNewMessageToPage(name, travel_id, message);
        }
    }
}