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


        public GeoModel geoModel(double x, double y)
        {
            string url = "/maps/api/geocode/json?latlng=" + x.ToString().Replace(",", ".") + "," + y.ToString().Replace(",", ".") + "&key=AIzaSyDyecsSf2YPWYz-RUw5UGvN_OPEs5J8DpY";
            var response = new RestClient("https://maps.googleapis.com").Execute(new RestRequest(url, Method.GET)).Content;
            GeoModel G = new JavaScriptSerializer().Deserialize<GeoModel>(response);

            ///https://maps.googleapis.com/maps/api/geocode/json?latlng=67.901128,18.519385&key=AIzaSyDyecsSf2YPWYz-RUw5UGvN_OPEs5J8DpY



            return G;
        }

        public List<TourModel> tourModel(string id)
        {
            System.Diagnostics.Debug.WriteLine("debug apifindvalue:" + id);
            try
            {
                string poststring = "";
                if (id == "")
                {
                    System.Diagnostics.Debug.WriteLine("if");
                    poststring = "api/Tour/";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("else");

                    poststring = "api/Tour/" + id;
                }
                System.Diagnostics.Debug.WriteLine(poststring);

                var response = new RestClient("http://localhost:58696").Execute(new RestRequest(poststring, Method.GET)).Content;

                List<TourModel> tourModel = new JavaScriptSerializer().Deserialize<List<TourModel>>(response);
                foreach (TourModel t in tourModel)
                {
                    SunModel S = sunModel(t.position.x, t.position.y);
                    ElevasionModel E = elevasionModel(t.position.x, t.position.y);
                    GeoModel G = geoModel(t.position.x, t.position.y);
                    t.elevation = Convert.ToInt32(E.results[0].elevation);
                    var format = "h:mm:ss tt";
                    t.sunrise = DateTime.ParseExact(S.results.sunrise, format, CultureInfo.InvariantCulture);
                    t.sunset = DateTime.ParseExact(S.results.sunset, format, CultureInfo.InvariantCulture);
                    t.day_length = S.results.day_length;

                    try
                    {


                        for (int i = 0; i < 15; i++)
                        {
                            System.Diagnostics.Debug.WriteLine("loop:" + i + G.results[0].address_components[i].types[0]);
                            if (G.results[0].address_components[i].types[0] == "administrative_area_level_1")
                            {
                                t.county = G.results[0].address_components[i].long_name;
                            }
                            else if (G.results[0].address_components[i].types[0] == "country")
                            {
                                t.country = G.results[0].address_components[i].long_name;
                            }


                        }

                    }
                    catch (Exception) { }

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
            string t = "";
            var tourList = tourModel(t);
            ViewBag.Count = tourList.Count.ToString();
            return View(tourList);
        }

        // GET: Tour/Details/5
        public ActionResult Details(string id)
        {

            System.Diagnostics.Debug.WriteLine("debug routingvalue: " + id);
            var tourList = tourModel(id);
            ViewBag.name = tourList[0].name;
            ViewBag.type = tourList[0].type;
            ViewBag.county = tourList[0].county;
            ViewBag.country = tourList[0].country;
            if (tourList[0].type == "Klippklättring")
            {
                ViewBag.BG = "climber2.jpg";
            }
            else if (tourList[0].type == "Alpin klättring")
            {
                ViewBag.BG = "alpinism.jpg";
            }
            else if (tourList[0].type == "Topptur skidor")
            {
                ViewBag.BG = "skier.jpg";
            }
            else
            {
                ViewBag.BG = "nature.jpg";
            }

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
            var client = new RestClient("http://localhost:58696");
            var request= new RestRequest("/api/Tour/", Method.POST);        
            
            string input = "{\"name\":\""+ collection["name"] + "\",\"type\":\""+ collection["type"] + "\",\"difficulty\":"+collection["difficulty"]+",\"comments\":\"" + collection["comments"] + "\", \"grade\":\"\", \"snowconditions\":\"\", \"position\":{\"x\":"+ collection["x"]+",\"y\":"+ collection["y"]+"}}";
            request.AddParameter("application/json; charset=utf-8", input, ParameterType.RequestBody);
            System.Diagnostics.Debug.WriteLine(input);

            try
            {
            var response = client.Execute(request);

                System.Diagnostics.Debug.WriteLine("responsmeddelande"+response.Content);
               

                Response.Write("<script language=javascript>alert('The activity: " + collection["name"]+" has been added in the database, thank you for your contribution!')</script>");
                          

                return View();
                
             
            }
            catch(Exception e)
            {
                Response.Write("<script language=javascript>alert('There has been a problem adding " + collection["name"] + " to the database. Message:"+e+"')</script>");
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
