using CosmeticsStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Security.Policy;
using CosmeticsStore.Models.EF;
using OfficeOpenXml;
using System.Data.Entity;

namespace CosmeticsStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, StaffOrder")]
    public class OrderController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Order
        public ActionResult Index(string Searchtext, int? page)
        {
            IEnumerable<Order> items = db.Orders.OrderByDescending(x => x.Id);
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
                items = items.Where(x => x.Code.Contains(Searchtext) || x.CustomerName.Contains(Searchtext) || x.Phone.Contains(Searchtext));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }

        public ActionResult View(int id)
        {
            var item = db.Orders.Find(id);
            return View(item);
        }

        public ActionResult Partial_SanPham(int id)
        {
            var items = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            return PartialView(items);
        }

        [HttpPost]
        public ActionResult UpdateTT(int id, int trangthai)
        {
            var item = db.Orders.Find(id);
            if (item != null)
            {
                db.Orders.Attach(item);
                item.TypePayment = trangthai;
                db.Entry(item).Property(x => x.TypePayment).IsModified = true;
                db.SaveChanges();
                return Json(new { message = "Thành công", Success = true });
            }
            return Json(new { message = "Thất bại!", Success = false });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Orders.Find(id);
            if (item != null)
            {
                db.Orders.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
        public void ExportExcel_EPPLUS()
        {
            // Thiết lập LicenseContext cho EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Lấy danh sách Orders
            IEnumerable<Order> items = db.Orders.OrderBy(x => x.Id);

            using (ExcelPackage ep = new ExcelPackage())
            {
                // Tạo worksheet mới
                ExcelWorksheet Sheet = ep.Workbook.Worksheets.Add("Report");

                // Ghi tiêu đề cho các cột
                Sheet.Cells["A1"].Value = "STT";
                Sheet.Cells["B1"].Value = "Mã Đơn Hàng";
                Sheet.Cells["C1"].Value = "Tên Khách Hàng";
                Sheet.Cells["D1"].Value = "Số Điện Thoại";
                Sheet.Cells["E1"].Value = "Tiền";
                Sheet.Cells["F1"].Value = "Ngày Tạo";

                int row = 2; // dòng bắt đầu ghi dữ liệu
                int i = 1;

                // Duyệt qua danh sách đơn hàng và ghi vào Excel
                foreach (var item in items)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = i;
                    Sheet.Cells[string.Format("B{0}", row)].Value = item.Code;
                    Sheet.Cells[string.Format("C{0}", row)].Value = item.CustomerName;
                    Sheet.Cells[string.Format("D{0}", row)].Value = item.Phone;
                    Sheet.Cells[string.Format("E{0}", row)].Value = item.TotalAmount;
                    Sheet.Cells[string.Format("F{0}", row)].Value = item.CreatedDate.ToString("dd/MM/yyyy");

                    row++;
                    i++;
                }

                // Tự động căn chỉnh độ rộng của các cột
                Sheet.Cells["A:AZ"].AutoFitColumns();

                // Gửi file về client
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=DanhSachDonHang.xlsx");
                Response.BinaryWrite(ep.GetAsByteArray());
                Response.End();
            }
        }


    }
}