using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using PagedList;


namespace CosmeticsStore.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Products
        public ActionResult Index(string Searchtext, int? page)
        {
            IEnumerable<Product> items = db.Products.OrderBy(x => x.Title);
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
            return View(items);
        }
        public ActionResult SortByName(string Searchtext)
        {
            IEnumerable<Product> items = db.Products.OrderBy(x => x.Title).ToList();
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
            IEnumerable<Product> items = db.Products.OrderBy(x => x.Price).ToList();
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
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Price).ToList();
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
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Title).ToList();
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

        public void updateStarRating(int ProductId)
        {
            var reviews = db.Reviews.Where(x => x.ProductId == ProductId);
            float starAverage = (float)Math.Round(reviews.Average(x => x.StarRating) * 2, MidpointRounding.AwayFromZero) / 2;

            // Cập nhật giá trị trung bình đánh giá của sản phẩm
            var product = db.Products.FirstOrDefault(p => p.Id == ProductId);
            if (product != null)
            {
                product.StarRating = starAverage;
                db.SaveChanges();
            }
        }


        public ActionResult Detail(string alias, int id, int page = 1, int pageSize = 3)
        {
            var reviewList = new List<ReviewViewModel>();
            string userId = User.Identity.GetUserId();
            ViewBag.UserId = userId;
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                var name = user.FullName;
                ViewBag.name = name;
                var email = user.Email;
                ViewBag.email = email;
                var image = user.Images;
                ViewBag.image = image;

                Debug.WriteLine("Name: " + name);
                // Perform other operations with the user's name
            }

            var reviews = db.Reviews.Where(_ => _.ProductId == id);

            // Tính số lượng đánh giá và số trang
            int totalReviews = reviews.Count();
            int totalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            // Lấy danh sách đánh giá cho trang hiện tại
            reviews = reviews.OrderByDescending(r => r.Date);
            var pagedReviews = reviews.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Truyền danh sách đánh giá và thông tin phân trang vào ViewBag hoặc Model
            ViewBag.Reviews = pagedReviews;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            var item = db.Products.Find(id);
            if (item != null)
            {
                db.Products.Attach(item);
                item.ViewCount = item.ViewCount + 1;
                db.Entry(item).Property(x => x.ViewCount).IsModified = true;
                db.SaveChanges();
            }

            foreach (var review in pagedReviews)
            {
                var customer = db.Users.FirstOrDefault(u => u.Id == review.CustomerId);
                if (customer != null)
                {
                    var reviewViewModel = new ReviewViewModel
                    {
                        name = customer.FullName,
                        description = review.Description,
                        star = (int)Math.Round(review.StarRating),
                        image = customer.Images,
                        date = review.Date,
                    };
                    reviewList.Add(reviewViewModel);
                }
            }

            reviewList = reviewList.OrderByDescending(r => r.date).ToList();
            ViewBag.Reviews = reviewList;

            return View(item);
        }


        public PartialViewResult ProductCategory(string alias,int? id, string Searchtext)
        {

            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id);
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
            if (id >0)
            {
                items = items.Where(x => x.ProductCategoryId == id).ToList();
            }
            var cate = db.ProductCategories.Find(id);
            if (cate != null)
            {
                ViewBag.CateName = cate.Title;
            }
            ViewBag.CateId = id;
            return PartialView(items);
        }
        public ActionResult Partial_ItemsByCateId()
        {
            var items = db.Products.Where(x => x.IsHome && x.IsActive == true).Take(12).ToList();
            return PartialView(items);
        }
        public ActionResult Partial_ProductSales()
        {
            var items = db.Products.Where(x => x.IsSale && x.IsActive == true).Take(12).ToList();
            return PartialView(items);
        }

        // POST: Product/SaveReview
        [HttpPost]
        public ActionResult SaveReview(Review review)
        {
            try
            {
                // Thực hiện lưu đánh giá vào cơ sở dữ liệu hoặc xử lý theo nhu cầu của bạn
                // Ví dụ:
                using (var dbContext = new ApplicationDbContext())
                {
                    review.Date = DateTime.Now;
                    // Lưu đánh giá vào cơ sở dữ liệu
                    dbContext.Reviews.Add(review);
                    dbContext.SaveChanges();
                    updateStarRating(review.ProductId);
                }

                // Trả về phản hồi thành công
                return Json(new { success = true, message = "Review saved successfully." });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { success = false, message = "An error occurred while saving the review." });
            }
        }
    }
}