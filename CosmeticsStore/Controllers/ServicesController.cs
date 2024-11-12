using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using Geocoding;
using Geocoding.Google;
using Hangfire;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using PagedList;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Location = CosmeticsStore.Models.Location;

namespace CosmeticsStore.Controllers
{
    public class ServicesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Branchs nearestBranch = new Branchs();
        double longtitude = 0;
        double latitude = 0;
        // GET: Services
        public ActionResult Index(string Searchtext, int? page)
        {
            IEnumerable<Models.EF.Service> items = db.Services.OrderBy(x => x.Title);
            var pageSize = 12;
            if (page == null)
            {
                page = 1;
            }
            if (!string.IsNullOrEmpty(Searchtext))
            {
                char[] charArray = Searchtext.ToCharArray();
                bool foundSpace = true;
                //sử dụng vòng lặp for lặp từng phần tử trong mảng
                for (int i = 0; i < charArray.Length; i++)
                {
                    //sử dụng phương thức IsLetter() để kiểm tra từng phần tử có phải là một chữ cái
                    if (Char.IsLetter(charArray[i]))
                    {
                        if (foundSpace)
                        {
                            //nếu phải thì sử dụng phương thức ToUpper() để in hoa ký tự đầu
                            charArray[i] = Char.ToUpper(charArray[i]);
                            foundSpace = false;
                        }
                    }
                    else
                    {
                        foundSpace = true;
                    }
                }
                //chuyển đổi kiểu mảng char thàng string
                Searchtext = new string(charArray);
                items = items.Where(x => x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;


            string userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            // Kiểm tra trạng thái cấm đặt lịch của khách hàng
           if(user != null)
            {
                if (user.IsBanned)
                {
                    // Tính số ngày bị cấm
                    int bannedDays = user.BanExpirationDate.Value.Subtract(DateTime.Now).Days;

                    // Hiển thị thông báo cảnh báo
                    string message = $"Bạn đã bị cấm đặt lịch trong {bannedDays} ngày.";
                    ViewBag.Script = message;
                }
            }
            BackgroundJob.Schedule(() => UpdateBookingStatus(), TimeSpan.FromMinutes(1));
            BackgroundJob.Schedule(() => UpdateBookingStatusForBan(), TimeSpan.FromMinutes(1));

            // Định cấu hình công việc lập lịch
            RecurringJob.AddOrUpdate(() => CheckCustomerBookingStatus(), Cron.Daily);
            return View(items);
        }

        public ActionResult SortByName(string Searchtext)
        {
            IEnumerable<Models.EF.Service> items = db.Services.OrderBy(x => x.Title).ToList();
            if (!string.IsNullOrEmpty(Searchtext))
            {
                char[] charArray = Searchtext.ToCharArray();
                bool foundSpace = true;
                //sử dụng vòng lặp for lặp từng phần tử trong mảng
                for (int i = 0; i < charArray.Length; i++)
                {
                    //sử dụng phương thức IsLetter() để kiểm tra từng phần tử có phải là một chữ cái
                    if (Char.IsLetter(charArray[i]))
                    {
                        if (foundSpace)
                        {
                            //nếu phải thì sử dụng phương thức ToUpper() để in hoa ký tự đầu
                            charArray[i] = Char.ToUpper(charArray[i]);
                            foundSpace = false;
                        }
                    }
                    else
                    {
                        foundSpace = true;
                    }
                }
                //chuyển đổi kiểu mảng char thàng string
                Searchtext = new string(charArray);
                items = items.Where(x => x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext));
            }
            return View(items);
        }
        public ActionResult SortByPrice(string Searchtext)
        {
            IEnumerable<Models.EF.Service> items = db.Services.OrderBy(x => x.Price).ToList();
            if (!string.IsNullOrEmpty(Searchtext))
            {
                char[] charArray = Searchtext.ToCharArray();
                bool foundSpace = true;
                //sử dụng vòng lặp for lặp từng phần tử trong mảng
                for (int i = 0; i < charArray.Length; i++)
                {
                    //sử dụng phương thức IsLetter() để kiểm tra từng phần tử có phải là một chữ cái
                    if (Char.IsLetter(charArray[i]))
                    {
                        if (foundSpace)
                        {
                            //nếu phải thì sử dụng phương thức ToUpper() để in hoa ký tự đầu
                            charArray[i] = Char.ToUpper(charArray[i]);
                            foundSpace = false;
                        }
                    }
                    else
                    {
                        foundSpace = true;
                    }
                }
                //chuyển đổi kiểu mảng char thàng string
                Searchtext = new string(charArray);
                items = items.Where(x => x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext));
            }
            return View(items);
        }
        public ActionResult SortByPriceGiam(string Searchtext)
        {
            IEnumerable<Models.EF.Service> items = db.Services.OrderByDescending(x => x.Price).ToList();
            if (!string.IsNullOrEmpty(Searchtext))
            {
                char[] charArray = Searchtext.ToCharArray();
                bool foundSpace = true;
                //sử dụng vòng lặp for lặp từng phần tử trong mảng
                for (int i = 0; i < charArray.Length; i++)
                {
                    //sử dụng phương thức IsLetter() để kiểm tra từng phần tử có phải là một chữ cái
                    if (Char.IsLetter(charArray[i]))
                    {
                        if (foundSpace)
                        {
                            //nếu phải thì sử dụng phương thức ToUpper() để in hoa ký tự đầu
                            charArray[i] = Char.ToUpper(charArray[i]);
                            foundSpace = false;
                        }
                    }
                    else
                    {
                        foundSpace = true;
                    }
                }
                //chuyển đổi kiểu mảng char thàng string
                Searchtext = new string(charArray);
                items = items.Where(x => x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext));
            }
            return View(items);
        }
        public ActionResult SortByNameZA(string Searchtext)
        {
            IEnumerable<Models.EF.Service> items = db.Services.OrderByDescending(x => x.Title).ToList();
            if (!string.IsNullOrEmpty(Searchtext))
            {
                char[] charArray = Searchtext.ToCharArray();
                bool foundSpace = true;
                //sử dụng vòng lặp for lặp từng phần tử trong mảng
                for (int i = 0; i < charArray.Length; i++)
                {
                    //sử dụng phương thức IsLetter() để kiểm tra từng phần tử có phải là một chữ cái
                    if (Char.IsLetter(charArray[i]))
                    {
                        if (foundSpace)
                        {
                            //nếu phải thì sử dụng phương thức ToUpper() để in hoa ký tự đầu
                            charArray[i] = Char.ToUpper(charArray[i]);
                            foundSpace = false;
                        }
                    }
                    else
                    {
                        foundSpace = true;
                    }
                }
                //chuyển đổi kiểu mảng char thàng string
                Searchtext = new string(charArray);
                items = items.Where(x => x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext));
            }
            return View(items);
        }
        public ActionResult Detail(string alias, int id)
        {
            var item = db.Services.Find(id);
            if (item != null)
            {
                db.Services.Attach(item);
                /*item.ViewCount = item.ViewCount + 1;
                db.Entry(item).Property(x => x.ViewCount).IsModified = true;
                db.SaveChanges();*/
            }

            return View(item);
        }
        public List<HourModel> CurentDay(DateTime currentDate)
        {
            var startHour = 8;
            var endHour = 18;
            var hoursOfDay = new List<HourModel>();
            for (int i = 0; i < 96; i++) // 96 slot = 1 ngày (24 giờ x 4 slot/giờ)
            {
                var currentHour = currentDate.AddMinutes(i * 15);

                if (currentHour.Hour >= startHour && currentHour.Hour < endHour)
                {
                    var hourModel = new HourModel
                    {
                        Hour = currentHour,
                        IsPastTime = currentDate.Date == DateTime.Now.Date && currentHour <= DateTime.Now && currentHour.Date == DateTime.Now.Date
                    };
                    hoursOfDay.Add(hourModel);
                }
            }

            return hoursOfDay;
        }

        public bool IsBookingSlotAvailable(DateTime selectedHour, List<HourModel> bookedHours)
        {
            DateTime endHour = selectedHour.AddHours(1);

            foreach (var bookedHour in bookedHours)
            {
                // Kiểm tra nếu giờ được chọn nằm trong khoảng thời gian đã đặt
                if (selectedHour >= bookedHour.Hour && selectedHour < bookedHour.Hour.AddHours(1))
                {
                    return false;
                }

                // Kiểm tra nếu khoảng thời gian đã đặt nằm trong khoảng thời gian được chọn
                if (bookedHour.Hour >= selectedHour && bookedHour.Hour < endHour)
                {
                    return false;
                }
            }

            return true;
        }


        public ActionResult BookingTime()
        {
            String lati = Request.Form["param1"];
            String longti = Request.Form["param2"];
            Debug.WriteLine("long:" + Request.Form["param1"]);
            List<Branchs> branchs = db.Branchs.ToList();
            if(lati == null || longti == null)
            {
                nearestBranch = FindNearestBranch(106.8286851, 10.8428402, branchs);
            }
            else
            {
                nearestBranch = FindNearestBranch(double.Parse(longti), double.Parse(lati), branchs);
            }
            //dat lịch
            string userId = User.Identity.GetUserId();
            ViewBag.UserId = userId;

            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                var name = user.FullName;
                ViewBag.name = name;
                Debug.WriteLine("Name: " + name);
                var email = user.Email;
                ViewBag.email = email;
                var phone = user.Phone;
                ViewBag.phone = phone;
                Debug.WriteLine("Phone: " + phone);
                // Thực hiện các thao tác khác với tên của user
            }
            DateTime currentDate = DateTime.Today.AddDays(0);
            List<List<HourModel>> hoursForDays = new List<List<HourModel>>();
            for (int i = 0; i < 7; i++)
            {
                var date = currentDate.AddDays(i);
                var hours = CurentDay(date);
                hoursForDays.Add(hours);
            }
            Debug.WriteLine(" Count: " + hoursForDays.Count());
            Debug.WriteLine(" IsPastTimne: " + hoursForDays[0][1].IsPastTime);
            ViewBag.HoursOfDay = hoursForDays;
            ViewBag.Branch = nearestBranch.Location;
            ViewBag.BranchId = nearestBranch.Id;
            return View();
        }

        public ActionResult ConfirmBooking()
        {

            return View();
        }

        public ActionResult History()
        {
            string userId = User.Identity.GetUserId();
            ViewBag.UserId = userId;
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            var phone = user.Phone;
            var appointments = db.Bookings.Where(b => b.Phone == phone)
                                           .OrderByDescending(b => b.Date)
                                           .ToList();

            // Chuyển đổi các bản ghi thành danh sách các đối tượng AppointmentViewModel
            var appointmentViewModels = new List<AppointmentViewModel>();
            foreach (var appointment in appointments)
            {
                var appointmentViewModel = new AppointmentViewModel
                {
                    Id = appointment.Id,
                    ServiceName = db.Services.FirstOrDefault(b => b.Id.ToString() == appointment.serviceId).Title,
                    Date = DateTime.Parse(appointment.Date),
                    Status = appointment.Status,
                    Code = appointment.Code,
                };
                appointmentViewModels.Add(appointmentViewModel);
            }

            // Truyền danh sách các đối tượng AppointmentViewModel vào ViewBag.History
            ViewBag.History = appointmentViewModels;

            return View();
        }


        public JsonResult CancelAppointment(string code)
        {
            var appointment = db.Bookings.FirstOrDefault(b => b.Code == code);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đặt lịch." });
            }
            appointment.Status = "Đã hủy";
            db.SaveChanges();
            return Json(new { success = true, message = "Hủy đặt lịch thành công." });
        }

        public void UpdateBookingStatus()
        {
            // Lấy thời gian hiện tại
            DateTime currentTime = DateTime.Now;

            // Lấy danh sách các đặt lịch chưa hoàn thành
            var bookings = db.Bookings.Where(b => b.Status != "Hoàn thành").ToList();

            // So sánh thời gian đặt lịch với thời gian hiện tại và cập nhật trạng thái
            foreach (var booking in bookings)
            {
                DateTime bookingDateTime;
                if (DateTime.TryParseExact(booking.Date, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out bookingDateTime))
                {
                    if (bookingDateTime < currentTime)
                    {
                        // Cập nhật trạng thái của đặt lịch
                        booking.Status = "Không hoàn thành";
                    }
                }
            }
            db.SaveChanges();
        }


        //-----------Cấm đặt lịch cho user không hoàn thành đặt lịch quá 3 lần
        // Hàm UpdateBookingStatus
        public void UpdateBookingStatusForBan()
        {
            // Lấy thời gian hiện tại
            DateTime currentTime = DateTime.Now;

            // Lấy danh sách khách hàng
            var customers = db.Users.ToList();

            foreach (var customer in customers)
            {
                // Lấy danh sách đặt lịch chưa hoàn thành của khách hàng
                var bookings = db.Bookings.Where(b => b.Phone == customer.Phone && b.Status == "Không hoàn thành").ToList();

                if (bookings.Count > 3)
                {
                    // Cấm khách hàng đặt lịch trong vòng 1 tháng
                    customer.IsBanned = true;
                    customer.BanExpirationDate = currentTime.AddMonths(1);
                    foreach (var booking in bookings)
                    {
                        if(booking.Status == "Đã xác nhận" || booking.Status =="Chờ xác nhận")
                        {
                            booking.Status = "Bị hủy";
                        }
                    }
                }
            }

            db.SaveChanges();
        }

        // Phương thức kiểm tra trạng thái đặt lịch của khách hàng và áp dụng cấm đặt lịch trong vòng 1 tháng
        public void CheckCustomerBookingStatus()
        {
            DateTime currentTime = DateTime.Now;

            var customers = db.Users.Where(c => c.IsBanned && c.BanExpirationDate < currentTime).ToList();

            foreach (var customer in customers)
            {
                customer.IsBanned = false;
                customer.BanExpirationDate = null;
            }

            db.SaveChanges();
        }


        public static Branchs FindNearestBranch(double longitudeSource, double latitudeSource, List<Branchs> branches)
        {
            List<string> addresses = branches.Select(b => b.Location).ToList();
            string nearestAddress = FindNearestAddress(longitudeSource, latitudeSource, addresses);

            return branches.FirstOrDefault(b => b.Location == nearestAddress);
        }

        public static string FindNearestAddress(double longitudeSource, double latitudeSource, List<string> addresses)
        {
            double nearestDistance = double.MaxValue;
            string nearestAddress = string.Empty;

            foreach (string address in addresses)
            {
                var location = GetLocationFromAddress(address);

                if (location != null)
                {
                    var sourceLocation = new Location(latitudeSource, longitudeSource);
                    double distance = CalculateDistance(sourceLocation, location);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestAddress = address;
                    }
                }
            }

            return nearestAddress;
        }

        private static Location GetLocationFromAddress(string address)
        {
            try
            {
                string encodedAddress = Uri.EscapeDataString(address);
                string apiUrl = "https://nominatim.openstreetmap.org/search?format=geojson&q=" + encodedAddress;

                using (HttpClient client = new HttpClient())
                {       
                    try
                    {
                        string jsonResult = SendHttpRequest(apiUrl);
                        JObject result = JObject.Parse(jsonResult);

                        JArray features = result["features"] as JArray;

                        if (features != null && features.Count > 0)
                        {
                            double latitude = features[0]["geometry"]["coordinates"][1].Value<double>();
                            double longitude = features[0]["geometry"]["coordinates"][0].Value<double>();

                            return new Location(latitude, longitude);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        Debug.WriteLine("Lỗi khi gửi yêu cầu HTTP: " + ex.Message);
                    }


                }
            }
            catch (Exception ex)
            {
                // Handle exception here
            }

            return null;
        }

        private static string SendHttpRequest(string url)
        {
            string responseContent = string.Empty;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = "OSMDroid";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseContent = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                Debug.Write(ex.ToString());
            }

            return responseContent;
        }


        private static double CalculateDistance(Location location1, Location location2)
        {
            var dLat = (location2.Latitude - location1.Latitude) * Math.PI / 180.0;
            var dLon = (location2.Longitude - location1.Longitude) * Math.PI / 180.0;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(location1.Latitude * Math.PI / 180.0) * Math.Cos(location2.Latitude * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = 6371 * c;

            return distance;
        }



    }
}