using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace TourApplication.Models
{
    public class TourModel
    {
      
            public string _id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public int difficulty { get; set; }
            public string comments { get; set; }
            public string grade { get; set; }
            public string snowconditions { get; set; }
            public double elevation { get; set; }
            public DateTime sunrise { get; set; }
            public DateTime sunset { get; set; }
            public string day_length { get; set; }
            public string county { get; set; }
            public string country { get; set; }
            public string weatherdescription { get; set; }
            public string weathericon { get; set; }
            public decimal temperature { get; set; }





        public Position position { get; set; }

        public static implicit operator List<object>(TourModel v)
        {
            throw new NotImplementedException();
        }

        public class Position
        {
          
            public decimal x { get; set; }
          
            public decimal y { get; set; }
        }

    }
}