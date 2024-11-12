using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using Dapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace CosmeticsStore.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index()
        {
            string userId = User.Identity.GetUserId();
            ViewBag.UserId = userId;
            return View();
        }

        public ActionResult Partial_Subcrise()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult Subscribe(Subscribe req)
        {
            if (ModelState.IsValid)
            {
                db.Subscribes.Add(new Subscribe { Email = req.Email, CreatedDate = DateTime.Now });
                db.SaveChanges();
                return Json(new { Success = true });
            }
            return View("Partial_Subcrise", req);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult Refesh()
        {
            var item = new ThongKeModel();
            ViewBag.Visitors_online = HttpContext.Application["visitors_online"];
            var hn = HttpContext.Application["HomNay"];
            item.HomNay = HttpContext.Application["HomNay"].ToString();
            item.HomQua = HttpContext.Application["HomQua"].ToString();
            item.TuanNay = HttpContext.Application["TuanNay"].ToString();
            item.TuanTruoc = HttpContext.Application["TuanTruoc"].ToString();
            item.ThangNay = HttpContext.Application["ThangNay"].ToString();
            item.ThangTruoc = HttpContext.Application["ThangTruoc"].ToString();
            item.TatCa = HttpContext.Application["TatCa"].ToString();
            return PartialView(item);
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        public ActionResult BookAppointment(string selectedDate, string selectedHour, string name, string phone, string email, string serviceId, string branchId)
        {
            // Chuyển đổi giá trị hour thành kiểu DateTime
            DateTime bookingTime = DateTime.ParseExact(selectedDate + " " + selectedHour, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
            List<Bookings> userBookings = db.Bookings.Where(b => b.Phone == phone).ToList();
            bool isBooked = userBookings.Any(b => b.Date == bookingTime.ToString());

            List<Bookings> bookedHours = GetBookedHours(phone); // Lấy danh sách các giờ đã đặt
            bool isSlotAvailable = IsBookingSlotAvailable(bookingTime, bookedHours);

            if (!isSlotAvailable)
            {
                return Json(new { success = false, message = "Khoảng thời gian đã được đặt." });
                Debug.WriteLine("isBooked " + isBooked);
            }
            Debug.WriteLine("isBooked " + isBooked);
            if (isBooked)
            {
                return Json(new { success = false, message = "Bạn đã có lịch hẹn vào thời gian này." });
            }
            else
            {
                string code = phone + "_" + bookingTime.ToString("yyyyMMddHHmm");
                // Lưu thông tin booking vào database
                Bookings booking = new Bookings
                {
                    Code = code,
                    CustomerName = HttpUtility.UrlDecode(name, Encoding.UTF8),
                    Phone = phone,
                    Date = bookingTime.ToString(),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Status = "Chờ xác nhận",
                    serviceId = serviceId,
                };
                // Thêm đối tượng Booking vào database
                db.Bookings.Add(booking);
                db.SaveChanges();
                var service = db.Services.FirstOrDefault(s => s.Id.ToString() == serviceId);
                if (service != null)
                {
                    var bookingDetails = new BookingDetails
                    {
                        ServiceId = service.Id,
                        BookingId = code,
                        Price = service.Price,
                        ServiceDetail = service.Detail,
                        ServiceName = service.Title,
                        BranchId = (int)Convert.ToInt32(branchId)
                };

                    db.BookingDetails.Add(bookingDetails);
                    db.SaveChanges();
                }
                return Json(new { success = true });
            }
            
        }

        public bool IsBookingSlotAvailable(DateTime selectedHour, List<Bookings> bookedHours)
        {
            DateTime endHour = selectedHour.AddHours(1);

            foreach (var bookedHour in bookedHours)
            {
                DateTime bookingTime = DateTime.Parse(bookedHour.Date);

                if ((selectedHour >= bookingTime && selectedHour < bookingTime.AddHours(1)) ||
                    (bookingTime >= selectedHour && bookingTime < endHour))
                {
                    return false;
                }
            }

            return true;
        }


        public List<Bookings> GetBookedHours(string phone)
        {
            List<Bookings> bookedHours = db.Bookings.Where(p => p.Phone == phone).ToList();
            return bookedHours;
        }





    }
}