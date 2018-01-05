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
        public ElevasionModel elevasionModel(decimal x, decimal y)
        {
            string url = $"maps/api/elevation/json?locations=" + x.ToString().Replace(",", ".") + "," + y.ToString().Replace(",", ".") + "&key=AIzaSyDyecsSf2YPWYz-RUw5UGvN_OPEs5J8DpY";
            var response = new RestClient("https://maps.googleapis.com").Execute(new RestRequest(url, Method.GET)).Content;
            ElevasionModel E = new JavaScriptSerializer().Deserialize<ElevasionModel>(response);
            return E;
        }
        public SunModel sunModel(decimal x, decimal y)
        {
            string url = "/json?lat=" + x.ToString().Replace(",", ".") + "&lng=" + y.ToString().Replace(",", ".");
            var response = new RestClient("https://api.sunrise-sunset.org").Execute(new RestRequest(url, Method.GET)).Content;            
            SunModel S = new JavaScriptSerializer().Deserialize<SunModel>(response);
            return S;

        }

        public GeoModel geoModel(decimal x, decimal y)
        {
            string url = "/maps/api/geocode/json?latlng=" + x.ToString().Replace(",", ".") + "," + y.ToString().Replace(",", ".") + "&key=AIzaSyDyecsSf2YPWYz-RUw5UGvN_OPEs5J8DpY";
            var response = new RestClient("https://maps.googleapis.com").Execute(new RestRequest(url, Method.GET)).Content;
            GeoModel G = new JavaScriptSerializer().Deserialize<GeoModel>(response);
            return G;

            ///https://maps.googleapis.com/maps/api/geocode/json?latlng=67.901128,18.519385&key=AIzaSyDyecsSf2YPWYz-RUw5UGvN_OPEs5J8DpY

        }
        public WeatherModel weatherModel(decimal x, decimal y)
        {
            string url = "/data/2.5/weather?lat="+ x.ToString().Replace(",", ".") + "&lon=" + y.ToString().Replace(",", ".") + "&appid=ee178f0658ef9fb947f5880533754eb2&units=metric";
            var response = new RestClient("http://api.openweathermap.org").Execute(new RestRequest(url, Method.GET)).Content;
            System.Diagnostics.Debug.WriteLine("Sunmodel" + url + response);
            WeatherModel W = new JavaScriptSerializer().Deserialize<WeatherModel>(response);
            return W;
            //http://api.openweathermap.org/data/2.5/weather?lat=63.294882&lon=13.209505&appid=ee178f0658ef9fb947f5880533754eb2&units=metric

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
                List<TourModel> tourList = new JavaScriptSerializer().Deserialize<List<TourModel>>(response);

                foreach (TourModel t in tourList)
                {
                    try
                    {
                        SunModel S = sunModel(t.position.x, t.position.y);
                        var format = "h:mm:ss tt";
                        t.sunrise = DateTime.ParseExact(S.results.sunrise, format, CultureInfo.InvariantCulture);
                        t.sunset = DateTime.ParseExact(S.results.sunset, format, CultureInfo.InvariantCulture);
                        t.day_length = S.results.day_length;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception på SunModel: " + e);
                    }

                    try
                    {
                        ElevasionModel E = elevasionModel(t.position.x, t.position.y);
                        t.elevation = Convert.ToInt32(E.results[0].elevation);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception på ElevationModel: " + e);
                    }

                    try
                    {
                        WeatherModel W = weatherModel(t.position.x, t.position.y);
                        t.temperature = W.main.temp;
                        t.weatherdescription = W.weather[0].description;                        
                        t.weathericon = W.weather[0].icon;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception på WeatherModel: " + e);
                    }
                    try
                    {
                        GeoModel G = geoModel(t.position.x, t.position.y);

                        for (int i = 0; i < G.results.Count; i++)
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
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception på GeoModel: " + e);
                        
                    }
                                        
                }
                return tourList;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                ViewBag.message = e;
                return null;
            }
            


        }

        // GET: Tour
        public ActionResult Index()
        {
            string t = "";
            var tourList = tourModel(t);
            if (tourList != null)
            {
                ViewBag.Count = tourList.Count.ToString();
                foreach (TourModel tm in tourList)
                {
                    System.Diagnostics.Debug.WriteLine("Laddat in: " + tm.name);
                }
                return View(tourList);
            }
            else
            {
                ViewBag.Message = "Application can't connect to the database, please try reloading the page";
                return View();
            }
        }

        // GET: Tour/Details/5
        public ActionResult Details(string id)
        {
            System.Diagnostics.Debug.WriteLine("debug routingvalue: " + id);
            var tourList = tourModel(id);
            if (tourList != null)
            {
                ViewBag.name = tourList[0].name;
                ViewBag.type = tourList[0].type;
                ViewBag.county = tourList[0].county;
                ViewBag.country = tourList[0].country;
                ViewBag.temperature = tourList[0].temperature;
                ViewBag.weatherdescription = tourList[0].weatherdescription;
                ViewBag.icon = "http://openweathermap.org/img/w/"+ tourList[0].weathericon +".png";

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
                foreach (TourModel t in tourList)
                {
                    System.Diagnostics.Debug.WriteLine("Laddat in: " + t.name);
                }
                return View(tourList);
            }
            else
            {
                ViewBag.message = "Could not load database, please try reloading the page";
                return View();
            }
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
            var request = new RestRequest("/api/Tour/", Method.POST);

            TourModel t = new TourModel();
            TourModel.Position pos = new TourModel.Position();
            t.name = collection["name"];
            t.type = collection["type"];
            t.difficulty = Convert.ToInt32(collection["difficulty"]);
            t.comments = collection["comments"];
            pos.x = Convert.ToDecimal(collection["x"].Replace(".", ","));
            pos.y = Convert.ToDecimal(collection["y"].Replace(".", ","));


            string input = "{\"name\":\"" + t.name + "\",\"type\":\"" + t.type + "\",\"difficulty\":" + t.difficulty + ",\"comments\":\"" + t.comments + "\", \"grade\":\"\", \"snowconditions\":\"\", \"position\":{\"x\":" + pos.x.ToString().Replace(",", ".") + ",\"y\":" + pos.y.ToString().Replace(",", ".") + "}}";
            request.AddParameter("application/json; charset=utf-8", input, ParameterType.RequestBody);
            System.Diagnostics.Debug.WriteLine(input);

            try
            {
                var response = client.Execute(request);
                System.Diagnostics.Debug.WriteLine("responsmeddelande" + response.Content);
                Response.Write("<script language=javascript>alert('The activity: " + t.name + " has been added in the database, thank you for your contribution!')</script>");
                return View();

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                Response.Write("<script language=javascript>alert('There has been a problem adding " + collection["name"] + " to the database. Message:" + e + "')</script>");
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
