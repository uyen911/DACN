using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CosmeticsStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,StaffBooking")]
    public class ServicesController : Controller
    {
        // GET: Admin/Services
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index(string Searchtext, int? page)
        {
            IEnumerable<Service> items = db.Services.OrderByDescending(x => x.Id);
            var pageSize = 10;
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

           

            return View(items);
        }
        public ActionResult Add()
        {
            return View();
        }

        public ActionResult ListBooking()
        {
            string userId = User.Identity.GetUserId();
            ViewBag.UserId = userId;
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            var phone = user.Phone;
            var appointments = db.Bookings.Where(b => b.Status != "Đã hủy" && b.Status != "Hoàn thành")
                                           .OrderByDescending(b => b.Date)
                                           .ToList();

            // Chuyển đổi các bản ghi thành danh sách các đối tượng AppointmentViewModel
            var appointmentViewModels = new List<AppointmentWithCustomerViewModel>();
            foreach (var appointment in appointments)
            {
                var appointmentViewModel = new AppointmentWithCustomerViewModel
                {
                    Id = appointment.Id,
                    ServiceName = db.Services.FirstOrDefault(b => b.Id.ToString() == appointment.serviceId).Title,
                    Date = appointment.Date,
                    Status = appointment.Status,
                    Code = appointment.Code,
                    Name = GetLastWord(appointment.CustomerName), 
                    Phone = appointment.Phone,
                };
                appointmentViewModels.Add(appointmentViewModel);
            }

            // Truyền danh sách các đối tượng AppointmentViewModel vào ViewBag.History
            ViewBag.History = appointmentViewModels;
            return View();
        }

        public string GetLastWord(string input)
        {
            string[] words = input.Split(' ');
            string lastWord = words[words.Length - 1];
            return lastWord;
        }

        public JsonResult GetPendingAppointments()
        {
            var pendingAppointments = db.Bookings.Where(b => b.Status == "Chờ xác nhận")
                                                 .OrderByDescending(b => b.Date)
                                                 .ToList();

            var appointmentViewModels = new List<AppointmentWithCustomerViewModel>();
            foreach (var appointment in pendingAppointments)
            {
                var serviceName = db.Services.FirstOrDefault(b => b.Id.ToString() == appointment.serviceId)?.Title;
                var appointmentViewModel = new AppointmentWithCustomerViewModel
                {
                    Id = appointment.Id,
                    ServiceName = serviceName,
                    Date = appointment.Date,
                    Status = appointment.Status,
                    Code = appointment.Code,
                    Name = GetLastWord(appointment.CustomerName),
                    Phone = appointment.Phone,
                };
                appointmentViewModels.Add(appointmentViewModel);
            }

            return Json(new
            {
                success = true,
                appointments = appointmentViewModels
            });
        }


        public JsonResult ConfirmAppointment(string code)
        {
            var appointment = db.Bookings.FirstOrDefault(b => b.Code == code);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đặt lịch." });
            }
            appointment.Status = "Đã xác nhận";
            db.SaveChanges();
            string accountSid = System.Configuration.ConfigurationManager.AppSettings["TwilioAccountSid"];
            string authToken = System.Configuration.ConfigurationManager.AppSettings["TwilioAuthToken"];
            string phoneNumber = appointment.Phone;
            phoneNumber = phoneNumber.TrimStart('0');
            phoneNumber = "+84" + phoneNumber;



            // Khởi tạo đối tượng TwilioClient với thông tin xác thực
            TwilioClient.Init(accountSid, authToken);

            // Gửi tin nhắn SMS
            /*var message = MessageResource.Create(
                body: "Đặt lịch của bạn đã được xác nhận! " +
                "Lịch của bạn vào lúc: " + appointment.Date.ToString()+". " +
                "Chúc bạn có một trải nghiệm tốt!",
                from: new Twilio.Types.PhoneNumber("+13613043356"),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );

            // Xử lý kết quả
            if (message != null)
            {
                ViewBag.Message = "Tin nhắn đã được gửi thành công!";
            }
            else
            {
                ViewBag.Message = "Đã xảy ra lỗi khi gửi tin nhắn.";
            }*/
            return Json(new { success = true, message = "Xác nhận đặt lịch thành công." });
        }

        public JsonResult RescheduleAppointment(string code, string timeRes)
        {
            var appointment = db.Bookings.FirstOrDefault(b => b.Code == code);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đặt lịch." });
            }

            DateTime dateTime = DateTime.Parse(appointment.Date);

            // Lấy ngày
            string date = dateTime.ToShortDateString();

            string dateTimeString = date + ' ' + timeRes;
            Debug.WriteLine("date time string: " + dateTimeString);

            // Chuyển đổi chuỗi thành đối tượng DateTime
            DateTime dateTimeConvert = DateTime.ParseExact(dateTimeString, "M/d/yyyy H:mm", CultureInfo.InvariantCulture);
            string originalDateTimeString = dateTimeConvert.ToString("M/d/yyyy h:mm:ss tt");


            Debug.WriteLine("date time: " + originalDateTimeString);
            appointment.Date = originalDateTimeString;
            db.SaveChanges();
            return Json(new { success = true, message = "Xác nhận đặt lịch thành công." });
        }

        [HttpPost]
        public JsonResult GetAppointmentsByDate(DateTime date)
        {
            Debug.WriteLine("Date input: " + date);

            var appointments = db.Bookings.Where(b => b.Status != "Đã hủy" && b.Status != "Hoàn thành")
                                           .OrderByDescending(b => b.Date)
                                           .ToList();
            var filteredAppointments = appointments.Where(a => ConvertToDate(a.Date) == ConvertToDate(date.Date.ToString())).ToList();
            var appointmentViewModels = new List<AppointmentWithCustomerViewModel>();
            foreach (var appointment in filteredAppointments)
            {
                var serviceName = db.Services.FirstOrDefault(b => b.Id.ToString() == appointment.serviceId)?.Title;
                var appointmentViewModel = new AppointmentWithCustomerViewModel
                {
                    Id = appointment.Id,
                    ServiceName = serviceName,
                    Date = appointment.Date,
                    Status = appointment.Status,
                    Code = appointment.Code,
                    Name = GetLastWord(appointment.CustomerName),
                    Phone = appointment.Phone,
                };
                appointmentViewModels.Add(appointmentViewModel);
            }
            return Json(new
            {
                success = true,
                appointments = appointmentViewModels
            });
        }


        public DateTime ConvertToDate(string dateString)
        {
            DateTime parsedDate = DateTime.ParseExact(dateString, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
            return parsedDate.Date;
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

        public JsonResult GetCustomerInfo(string phoneNumber)
        {
            List<Bookings> sortedBookings = db.Bookings
    .Where(b => b.Phone == phoneNumber && b.Status == "Đã xác nhận")
    .ToList()
    .OrderBy(b => DateTime.ParseExact(b.Date, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)).ToList();

            var booking = sortedBookings.FirstOrDefault();


            if (booking != null)
            {
                var serviceName = db.Services.FirstOrDefault(b => b.Id.ToString() == booking.serviceId)?.Title;
                var customerName = GetLastWord(booking.CustomerName);
                var date = booking.Date;
                var code = booking.Code;

                return Json(new
                {
                    success = true,
                    serviceName = serviceName,
                    customerName = customerName,
                    date = date,
                    code = code
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy thông tin khách hàng."
                });
            }
        }

        public JsonResult DoneAppointment(string code)
        {
            var appointment = db.Bookings.FirstOrDefault(b => b.Code == code);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đặt lịch." });
            }
            appointment.Status = "Hoàn thành";
            db.SaveChanges();
            return Json(new { success = true, message = "Hoàn thành đặt lịch thành công." });
        }

        public ActionResult ConfirmDone()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Service model, List<string> Images, List<int> rDefault)
        {
            if (ModelState.IsValid)
            {
                if (Images != null && Images.Count > 0)
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        if (i + 1 == rDefault[0])
                        {
                            model.Image = Images[i];
                            model.ServiceImage.Add(new ServiceImages
                            {
                                ServiceId = model.Id,
                                Image = Images[i],
                                IsDefault = true
                            });
                        }
                        else
                        {
                            model.ServiceImage.Add(new ServiceImages
                            {
                                ServiceId = model.Id,
                                Image = Images[i],
                                IsDefault = false
                            });
                        }
                    }
                }
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                if (string.IsNullOrEmpty(model.SeoTitle))
                {
                    model.SeoTitle = model.Title;
                }
                if (string.IsNullOrEmpty(model.Alias))
                    model.Alias = CosmeticsStore.Models.Common.Filter.FilterChar(model.Title);
                db.Services.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }


        public ActionResult Edit(int id)
        {
            var item = db.Services.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Service model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.Alias = CosmeticsStore.Models.Common.Filter.FilterChar(model.Title);
                db.Services.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Services.Find(id);
            if (item != null)
            {
                db.Services.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',');
                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        var obj = db.Services.Find(Convert.ToInt32(item));
                        db.Services.Remove(obj);
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.Services.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isAcive = item.IsActive });
            }

            return Json(new { success = false });
        }

        

    }
}
