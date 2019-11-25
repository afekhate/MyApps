using CenturyCars.Domain.Entities;
using CenturyCars.Models;
using CenturyCars.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace CenturyCars.Controllers
{
    public class HomeController : Controller
    {
        public ApplicationDbContext _context;

        public HomeController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }
        public ActionResult Add()
        {
            var manufacturer = _context.Manufacturer.ToList();
            var viewmodel = new CarsFormViewModel()
            {
                Manufacturer = manufacturer,
                Car = new Car()
            };
            return View("Add", viewmodel);
        }

        [HttpPost]
        public ActionResult Add(Car car)
        {

            if (ModelState.IsValid)
            {

                HttpPostedFileBase file = Request.Files[0];

                if (file != null)
                {
                    if (file.ContentLength > 0)
                    {
                        if (Path.GetExtension(file.FileName).ToLower() == ".jpg"
                            || Path.GetExtension(file.FileName).ToLower() == ".png"
                                || Path.GetExtension(file.FileName).ToLower() == ".gif"
                                || Path.GetExtension(file.FileName).ToLower() == ".jpeg")
                        {
                            string filename = Path.GetFileName(file.FileName);
                            car.ImagePath = "~/Image/" + filename;
                            filename = Path.Combine(Server.MapPath("~/Image/"), filename);
                            file.SaveAs(filename);
                        }
                    }
                }
                if (car.CarID == 0)
                    _context.Car.Add(car);
                else
                {
                    var carInDb = _context.Car.Single(m => m.CarID == car.CarID);

                    carInDb.Description = car.Description;
                    carInDb.ImagePath = car.ImagePath;
                    carInDb.ManufacturerID = car.ManufacturerID;
                    carInDb.Name = car.Name;
                    carInDb.Price = car.Price;
                    carInDb.Rating = car.Rating;
                }

                _context.SaveChanges();

            }

            return RedirectToAction("Adminsite", "Home");
        }

        public ActionResult Index()
        {
            var cars = _context.Car.Include(m => m.Manufacturer).Where(m => m.Rating == 5);
            return View(cars.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult List()
        {
            var car = _context.Car.Include(m => m.Manufacturer).ToList();
            return View(car);
        }

        [Authorize]
        public ActionResult Adminsite()
        {
            var cars = _context.Car.Include(m => m.Manufacturer).ToList();
            return View(cars);
        }


        [Authorize]
        public ActionResult Details(int id)
        {
            var car = _context.Car.Include(m => m.Manufacturer).SingleOrDefault(m => m.CarID == id);
            return View(car);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var car = _context.Car.SingleOrDefault(m => m.CarID == id);
            var manufacturer = _context.Manufacturer.ToList();

            var viewmodel = new CarsFormViewModel
            {
                Car = car,
                Manufacturer = manufacturer
            };
            return View("Add", viewmodel);
        }

        public ActionResult Delete(int id)
        {
            var car = _context.Car.Find(id);

            if (car != null)
            {
                _context.Car.Remove(car);
                _context.SaveChanges();

                return View(car);
            }
            return RedirectToAction("Adminsite", "Home");
        }


        [HttpGet]
        public ActionResult Update()
        {
            return PartialView("Update");
        }

        [HttpPost]
        public ActionResult Update(Message message)
        {
            if (message.MessageID == 0)

                _context.Message.Add(message);
            TempData["message"] = "Your Message has been Sent";
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Order()
        {
            return View("Order");
        }

        [HttpPost]
        public ActionResult Order(Order order)
        {

            if (order.OrderId == 0)
            {
                _context.Order.Add(order);
                _context.SaveChanges();
                TempData["message"] = "We have Received your Order";
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult CarSearch(string q)
        {
            var car = GetCars(q);
            return PartialView(car);
        }
        private List<Car> GetCars(string searchString)
        {
            return _context.Car.Include(m => m.Manufacturer)
            .Where(a => a.Manufacturer.Name.Contains(searchString))
            .ToList();
        }

    }
}