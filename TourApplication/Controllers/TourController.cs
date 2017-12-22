using RestSharp;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TourApplication.Models;



namespace TourApplication.Controllers
{
    public class TourController : Controller
    {
        public ElevasionModel elevasionModel(double x, double y)
        {

            string url = $"maps/api/elevation/json?locations=" + x.ToString().Replace(",", ".") + "," + y.ToString().Replace(",", ".") + "&key=AIzaSyDyecsSf2YPWYz-RUw5UGvN_OPEs5J8DpY";
            var response = new RestClient("https://maps.googleapis.com").Execute(new RestRequest(url, Method.GET)).Content;
            ElevasionModel E = new JavaScriptSerializer().Deserialize<ElevasionModel>(response);
            return E;
        }
        public SunModel sunModel(double x, double y)
        {
            string url = "/json?lat=" + x.ToString().Replace(",", ".") + "&lng=" + y.ToString().Replace(",", ".");
            var response = new RestClient("https://api.sunrise-sunset.org").Execute(new RestRequest(url, Method.GET)).Content;
            SunModel S = new JavaScriptSerializer().Deserialize<SunModel>(response);

            return S;
        }
        //public PlaceNameModel placeName (double x, double y)
       // {
           // string url = "/maps/api/geocode/json?latlng=" + x.ToString().Replace(",", ".") + "," + y.ToString().Replace(",", ".") + "&key=AIzaSyDyecsSf2YPWYz-RUw5UGvN_OPEs5J8DpY";
          //  var response = new RestClient(" https://maps.googleapis.com").Execute(new RestRequest(url, Method.GET)).Content;
         
       // }

        public List<TourModel> tourModel(string id)
        {
            try
            {
                string poststring ="";
                if  (id == "tom")
                {
                    System.Diagnostics.Debug.WriteLine("if");
                    poststring = "api/Tour/";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("else");

                    poststring = "api/Tour/Siluettleden";
                }
                System.Diagnostics.Debug.WriteLine(poststring);

                var response = new RestClient("http://localhost:58696").Execute(new RestRequest(poststring, Method.GET)).Content;

                List<TourModel> tourModel = new JavaScriptSerializer().Deserialize<List<TourModel>>(response);
                foreach (TourModel t in tourModel)
                {
                    SunModel S = sunModel(t.position.x, t.position.y);
                    ElevasionModel E = elevasionModel(t.position.x, t.position.y);
                    t.elevation = Convert.ToInt32(E.results[0].elevation);
                    var format = "h:mm:ss tt";
                    t.sunrise = DateTime.ParseExact(S.results.sunrise, format, CultureInfo.InvariantCulture);
                    t.sunset = DateTime.ParseExact(S.results.sunset, format, CultureInfo.InvariantCulture);
                    t.day_length = S.results.day_length;
                    return tourModel;
                }
            }
            catch (Exception e)
            {
                ViewBag.message = e;
                return null;
            }
            return null;


        }

        // GET: Tour
        public ActionResult Index()
        {
            string t = "tom";
            var tourList = tourModel(t);
            ViewBag.Count = tourList.Count.ToString();
            return View(tourList);
        }

        // GET: Tour/Details/5
        public ActionResult Details(string name)
        {
            var tourList = tourModel(name);
            ViewBag.name = tourList[0].name;
            ViewBag.description = tourList[0].comments;
            return View(tourList);

                  
        }

        // GET: Tour/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tour/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Tour/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Tour/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Tour/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Tour/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
