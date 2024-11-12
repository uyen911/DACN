using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using PayPal.Api;
using CKFinder.Connector;
using CosmeticsStore.VNPay;
using Util = CosmeticsStore.VNPay.Util;
using System.ComponentModel.DataAnnotations;
using Microsoft.Ajax.Utilities;
using CosmeticsStore.Momo;
using Newtonsoft.Json.Linq;
using System.Security.Policy;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using Twilio.Http;

namespace CosmeticsStore.Controllers
{
   
    class GetInfo
    {
        public static string Email;
        public static string toAddress;
        public static decimal shippingFee;
        public static Models.EF.Order OrderInfo;
    }
    public class ShoppingCartController : Controller
    {
        private System.Net.Http.HttpClient _httpClient;
        private ApplicationDbContext db = new ApplicationDbContext();
        private string _apiUrl = "https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee";
        public ShoppingCartController()
        {
            _httpClient = new System.Net.Http.HttpClient();
        }
        // GET: ShoppingCart
        public async Task<ActionResult> Index()
        {
            decimal shippingFee = await ShippingFeeAsync();
            TempData["ShippingFee"] = shippingFee;
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }

        public ActionResult CheckOut()
        {
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }
        public ActionResult CheckOutSuccess(OrderViewModel req)
        {

            return View();
        }
        public ActionResult Partial_Item_ThanhToan()
        {
            
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null && cart.Items.Any())
            {
                if (TempData["ShippingFee"] != null)
                {
                    ViewBag.ship = (decimal)TempData["ShippingFee"];
                }
                else
                {
                    ViewBag.ship = 34000;
                }
                return PartialView(cart.Items);
            }

            return PartialView();
        }
        public ActionResult Partial_Item_Cart()
        {
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }
        public ActionResult ShowCount()
        {
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null)
            {
                return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
        }
        public async Task<decimal> ShippingFeeAsync()
        {
            decimal fee = 0;
            string id = User.Identity.GetUserId();
            var listAddress = db.AddressBooks.Where(x => x.UserID.Contains(id));
            foreach (var item in listAddress)
            {
                if (item.IsDefault)
                {
                    string fromAddress = "Số nhà 10, Phường Trung Hoà, Quận Cầu Giấy, Hà Nội";

                    int serviceId = 53321;
                    string toAddress = item.Address;
                    fee = await CalculateShippingFee(fromAddress, toAddress, serviceId);
                    return fee;
                }
            }
            return fee;
        }
        public ActionResult Partial_CheckOut()
        {
            string id = User.Identity.GetUserId();
            var listAddress = db.AddressBooks.Where(x => x.UserID.Contains(id));
            ViewBag.InfoOrder = listAddress;
            ViewBag.ship = 34000;
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckOut(OrderViewModel req)
        {

            var code = new { Success = false, Code = -1 };
            string id = User.Identity.GetUserId();
            var listAddress = db.AddressBooks.Where(x => x.UserID.Contains(id));
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null)
            {

                var order = new CosmeticsStore.Models.EF.Order();
                if (id != null)
                {
                    foreach (var item in listAddress)
                    {
                        if (item.IsDefault)
                        {
                            order.CustomerName = item.CustomerName;
                            order.Phone = item.Phone;
                            order.Address = item.Address;
                            order.Email = item.Email;
                            GetInfo.Email = item.Email;
                            GetInfo.toAddress = item.Address;
                        }

                    }
                }
                else
                {
                    order.CustomerName = req.CustomerName;
                    order.Phone = req.Phone;
                    order.Address = req.Address;
                    order.Email = req.Email;
                    GetInfo.Email = req.Email;
                    GetInfo.toAddress = req.Address;
                }
                cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Price = x.Price,
                }));
                order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
                order.TypePayment = req.TypePayment;
                if (User.Identity.GetUserId() == null)
                {
                    order.IdUser = null;
                }
                else
                {
                    order.IdUser = User.Identity.GetUserId();
                }
                order.CreatedDate = DateTime.Now;
                order.ModifiedDate = DateTime.Now;
                order.CreatedBy = req.Phone;

                GetInfo.OrderInfo = order;
                Random rd = new Random();
                order.Code = "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
                string fromAddress = "Số nhà 10, Phường Trung Hoà, Quận Cầu Giấy, Hà Nội";

                int serviceId = 53321;
                string toAddress = GetInfo.toAddress;
                decimal shippingFee = await CalculateShippingFee(fromAddress, toAddress, serviceId);
                GetInfo.shippingFee = shippingFee;


                switch (order.TypePayment)
                {
                    case 3:
                        return RedirectToAction("PaymentWithPaypal");
                    case 4:
                        return RedirectToAction("PaymentVNPay");
                    case 5:
                        return RedirectToAction("PaymentMomo");
                    default:
                        break;
                }


                SendMail(GetInfo.Email, cart, order);
                return RedirectToAction("CheckOutSuccess");
            }
            return Json(code);
        }
        private void SendMail(string email, ShoppingCart cart, Models.EF.Order order)
        {
            List<Product> products = db.Products.ToList();
            List<OrderDetail> orderDetails = order.OrderDetails.ToList();
            foreach (var itemProduct in products)
            {
                foreach (var itemOrderDetail in orderDetails)
                {
                    if (itemProduct.Id == itemOrderDetail.ProductId)
                    {
                        itemProduct.Quantity -= itemOrderDetail.Quantity;
                        db.Products.Attach(itemProduct);
                        db.Entry(itemProduct).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            db.Orders.Add(order);
            db.SaveChanges();
            var strSanPham = "";
            var thanhtien = decimal.Zero;
            var phiVanChuyen = decimal.Zero;
            var TongTien = decimal.Zero;
            foreach (var sp in cart.Items)
            {
                strSanPham += "<tr>";
                strSanPham += "<td>" + sp.ProductName + "</td>";
                strSanPham += "<td>" + sp.Quantity + "</td>";
                strSanPham += "<td>" + CosmeticsStore.Common.Common.FormatNumber(sp.TotalPrice) + "</td>";
                strSanPham += "</tr>";
                thanhtien += sp.Quantity * sp.Price;
            }
            phiVanChuyen = GetInfo.shippingFee;
            TongTien = thanhtien + phiVanChuyen;
            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
            contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
            contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
            contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
            contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", "Tên Người Nhận: " + order.CustomerName);
            contentCustomer = contentCustomer.Replace("{{Phone}}", "Số Điện Thoại: " + order.Phone);
            contentCustomer = contentCustomer.Replace("{{Email}}", "Email: " + email);
            contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", "Địa Chỉ Nhận Hàng: " + order.Address);
            switch (order.TypePayment)
            {
                case 1:
                    contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: COD");
                    break;
                case 2:
                    contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: Chuyển Khoản");
                    break;
                case 3:
                    contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: PayPal");
                    break;
                case 4:
                    contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: VNPay");
                    break;
                case 5:
                    contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: Momo");
                    break;
                default:
                    break;
            }
            contentCustomer = contentCustomer.Replace("{{ThanhTien}}", CosmeticsStore.Common.Common.FormatNumber(thanhtien, 0));
            contentCustomer = contentCustomer.Replace("{{PhiVanChuyen}}", CosmeticsStore.Common.Common.FormatNumber(phiVanChuyen, 0));
            contentCustomer = contentCustomer.Replace("{{TongTien}}", CosmeticsStore.Common.Common.FormatNumber(TongTien, 0));
            CosmeticsStore.Common.Common.SendMail("CosmeticsStore", "Đơn hàng #" + order.Code, contentCustomer.ToString(), email);

            string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
            contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Code);
            contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
            contentAdmin = contentAdmin.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
            contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", "Tên Người Nhận: " + order.CustomerName);
            contentAdmin = contentAdmin.Replace("{{Phone}}", "Số Điện Thoại: " + order.Phone);
            contentAdmin = contentAdmin.Replace("{{Email}}", "Emaili: " + email);
            contentAdmin = contentAdmin.Replace("{{DiaChiNhanHang}}", "Địa Chỉ Nhận Hàng: " + order.Address);
            switch (order.TypePayment)
            {
                case 1:
                    contentAdmin = contentAdmin.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: COD");
                    break;
                case 2:
                    contentAdmin = contentAdmin.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: Chuyển Khoản");
                    break;
                case 3:
                    contentAdmin = contentAdmin.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: PayPal");
                    break;
                case 4:
                    contentAdmin = contentAdmin.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: VNPay");
                    break;
                case 5:
                    contentAdmin = contentAdmin.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: Momo");
                    break;
                default:
                    break;
            }
            contentAdmin = contentAdmin.Replace("{{ThanhTien}}", CosmeticsStore.Common.Common.FormatNumber(thanhtien, 0));
            contentAdmin = contentAdmin.Replace("{{PhiVanChuyen}}", CosmeticsStore.Common.Common.FormatNumber(phiVanChuyen, 0));
            contentAdmin = contentAdmin.Replace("{{TongTien}}", CosmeticsStore.Common.Common.FormatNumber(TongTien, 0));
            CosmeticsStore.Common.Common.SendMail("CosmeticsStore", "Đơn hàng mới #" + order.Code, contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);
            cart.ClearCart();
        }
        public ActionResult PaymentMomo()
        {
            var order = new CosmeticsStore.Models.EF.Order();
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Price = x.Price,
            }));
            order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
            //request params need to request to MoMo system
            string endpoint = null;
            string partnerCode = null;
            string accessKey = null;
            string serectkey = null;
            string orderInfo = null;
            string returnUrl = null;
            string notifyurl = null; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

            IEnumerable<PaymentSetting> lst = db.PaymentSettings.OrderByDescending(x => x.Id);
            foreach (var item in lst)
            {
                endpoint = item.EndpointMomo;
                partnerCode = item.PartnerCodeMomo;
                accessKey = item.AccessKeyMomo;
                serectkey = item.SerectkeyMomo;
                orderInfo = item.OrderInfoMomo;
                returnUrl = item.ReturnUrlMomo;
                notifyurl = item.NotifyurlMomo;
            }
            string amount = ((int)(order.TotalAmount + GetInfo.shippingFee)).ToString();
            string orderid = DateTime.Now.Ticks.ToString();//mã đơn hàng
            string requestId = DateTime.Now.Ticks.ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;

            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);
            return Redirect(jmessage.GetValue("payUrl").ToString());
        }

        //Khi thanh toán xong ở cổng thanh toán Momo, Momo sẽ trả về một số thông tin, trong đó có errorCode để check thông tin thanh toán
        //errorCode = 0 : thanh toán thành công (Request.QueryString["errorCode"])
        //Tham khảo bảng mã lỗi tại: https://developers.momo.vn/#/docs/aio/?id=b%e1%ba%a3ng-m%c3%a3-l%e1%bb%97i
        public ActionResult ConfirmPaymentClient(Result result)
        {
            //lấy kết quả Momo trả về và hiển thị thông báo cho người dùng (có thể lấy dữ liệu ở đây cập nhật xuống db)
            string rMessage = result.message;
            string rOrderId = result.orderId;
            string rErrorCode = result.errorCode;
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (rErrorCode == "0")
            {

                //Thanh toán thành công
                SendMail(GetInfo.Email, cart, GetInfo.OrderInfo);
                return View("CheckOutSuccess");
            }
            else
            {
                return View("CheckOutFailure");
            }
        }
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            var db = new ApplicationDbContext();
            var checkProduct = db.Products.FirstOrDefault(x => x.Id == id);

            if (checkProduct != null)
            {
                ShoppingCart cart = (ShoppingCart)Session["Cart"];
                if (cart == null)
                {
                    cart = new ShoppingCart();
                }
                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = checkProduct.Id,
                    ProductName = checkProduct.Title,
                    CategoryName = checkProduct.ProductCategory.Title,
                    Alias = checkProduct.Alias,
                    Quantity = quantity
                };
                if (checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault) != null)
                {
                    item.ProductImg = checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault).Image;
                }
                item.Price = checkProduct.Price;
                if (checkProduct.PriceScale > 0)
                {
                    item.Price = (decimal)checkProduct.PriceScale;
                }
                item.TotalPrice = item.Quantity * item.Price;
                if (cart.CheckQuantityAddtoCart(checkProduct.Quantity, item, quantity))
                {
                    cart.AddToCart(item, quantity);
                    Session["Cart"] = cart;
                    code = new { Success = true, msg = "Thêm sản phẩm thành công!", code = 1, Count = cart.Items.Count };
                }
                else
                {
                    code = new { Success = false, msg = "", code = -1, Count = 0 };
                }

            }
            return Json(code);
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (checkProduct != null)
                {
                    cart.Remove(id);
                    code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };
                }
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult DeleteAll()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.UpdateQuantity(id, quantity);
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
        [HttpPost]
        public ActionResult CheckQuantity(int id, int quantity)
        {
            List<Product> products = db.Products.ToList();

            foreach (var itemProducts in products)
            {
                if (id == itemProducts.Id)
                {
                    if (quantity <= itemProducts.Quantity)
                    {
                        return Json(new { Success = true });
                    }
                    else
                    {
                        return Json(new { Success = false });
                    }

                }

            }
            return Json(new { Success = false });
        }

        // Work with Paypal Payment
        private Payment payment;

        //Create a payment using an APIContext
        private Payment CreatedPayment(APIContext apiContext, string redirectUrl)
        {
            var listItems = new ItemList() { items = new List<Item>() };
            ShoppingCart shoppingCart = Session["cart"] as ShoppingCart;

            List<ShoppingCartItem> listCarts = shoppingCart.Items;

            foreach (var cart in listCarts)
            {
                listItems.items.Add(new Item()
                {
                    name = cart.ProductName,
                    currency = "USD",
                    price = cart.Price.ToString(),
                    quantity = cart.Quantity.ToString(),
                    sku = "sku"
                });
            }

            var payer = new Payer() { payment_method = "paypal" };

            //Do the configuration RedirectURLS here with redirectURLs object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl,
            };

            //Create details object
            var details = new Details()
            {
                tax = "1",
                shipping = "2",
                subtotal = listCarts.Sum(x => x.Quantity * x.Price).ToString()
            };

            // Create amount object
            var amount = new Amount()
            {
                currency = "VND",
                total = (Convert.ToDouble(details.tax) + Convert.ToDouble(details.shipping) + Convert.ToDouble(details.subtotal)).ToString(),//tax + shipping + subtotal
                details = details
            };

            //Create transaction
            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Grotto Testing transaction description",
                invoice_number = Convert.ToString((new Random()).Next(1000000000)),
                amount = amount,
                item_list = listItems
            });

            payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            return payment.Create(apiContext);
        }

        //Create ExecutePayment method
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId,
            };
            payment = new Payment() { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }

        //Create PaymentWithPayPal method
        public ActionResult PaymentWithPaypal()
        {
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            //Gettings context from the paypal bases on clientId and secret for payment
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/ShoppingCart/PaymentWithPaypal?";
                    var guid = Convert.ToString((new Random()).Next(1000000000));
                    var createdPayment = CreatedPayment(apiContext, baseURI + "guid=" + guid);

                    //Get links returned from paypal response to create call funciton
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = string.Empty;

                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = link.href;
                        }
                    }
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // this one will be executed when we have received all the payment params from previous call
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        Session.Remove("cart");
                        return View("CheckOutFailure");

                    }
                }
            }
            catch (Exception ex)
            {
                PaypalLogger.Log("Error: " + ex.Message);
                Session.Remove("cart");
                return View("CheckOutFailure");
                ;
            }
            Session.Remove("cart");
            SendMail(GetInfo.Email, cart, GetInfo.OrderInfo);
            return View("CheckOutSuccess");


        }
        public ActionResult PaymentVNPay()
        {

            var order = new CosmeticsStore.Models.EF.Order();
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Price = x.Price,
            }));
            order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
            PayLib pay = new PayLib();
            IEnumerable<PaymentSetting> lst = db.PaymentSettings.OrderByDescending(x => x.Id);
            string url = null;
            string returnUrl = null;
            string tmnCode = null;
            string hashSecret = null;
            foreach (var item in lst)
            {
                url = item.UrlVNP;
                returnUrl = item.ReturnUrlVNP;
                tmnCode = item.TmnCodeVNP;
                hashSecret = item.HashSecretVNP;
            }
            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", ((int)(order.TotalAmount + GetInfo.shippingFee) * 100).ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang - hpustore"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }
        public ActionResult PaymentVNPConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?
                var order = new CosmeticsStore.Models.EF.Order();
                ShoppingCart cart = (ShoppingCart)Session["cart"];
                if (checkSignature)
                {
                    ViewBag.Code = vnp_ResponseCode;
                    if (vnp_ResponseCode == "00")
                    {

                        //Thanh toán thành công
                        SendMail(GetInfo.Email, cart, GetInfo.OrderInfo);
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                    if (vnp_ResponseCode == "24")
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Giao dịch không thành công do: Khách hàng hủy giao dịch";
                    }

                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            return View();
        }

        //public async Task<decimal> CalculateShippingFee(string fromAddress, string toAddress, int serviceId)
        //{
        //    var apiKey = "47c6c1d4-0f65-11ee-8a8c-6e4795e6d902";
        //    var fromDistrictId = await GetDistrictIdFromAddress(fromAddress);
        //    var toDistrictId = await GetDistrictIdFromAddress(toAddress);
        //    string[] partsFrom = fromDistrictId.Split('-');
        //    string districtIdFrom = partsFrom[0];  // "3696"
        //    string wardIdFrom = partsFrom[1];  // "90835"

        //    string[] partsTo = toDistrictId.Split('-');
        //    string districtIdTo = partsTo[0];  // "3696"
        //    string wardIdTo = partsTo[1];  // "90835"

        //    _httpClient.DefaultRequestHeaders.Add("token", apiKey);

        //    var jsonPayload = $"{{ \"service_id\": {serviceId}, \"insurance_value\": 500000, \"coupon\": null, \"from_district_id\": {districtIdFrom}, \"to_district_id\": {districtIdTo}, \"to_ward_code\": \"20314\", \"height\": 15, \"length\": 15, \"weight\": 1000, \"width\": 15 }}";

        //    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        //    var apiUrl = "https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee";
        //    // Thay đổi _apiUrl thành apiUrl

        //    var response = await _httpClient.PostAsync(apiUrl, content);
        //    var responseContent = await response.Content.ReadAsStringAsync();

        //    if (response.IsSuccessStatusCode)
        //    {
        //        dynamic responseData = JsonConvert.DeserializeObject(responseContent);
        //        decimal shippingFee = responseData.data.total;
        //        return shippingFee;
        //    }
        //    else
        //    {
        //        throw new Exception($"Failed to calculate shipping fee. Error code: {response.StatusCode}");
        //    }

        //}

        public async Task<decimal> CalculateShippingFee(string fromAddress, string toAddress, int serviceId)
        {
            return 34000;

        }
        private async Task<string> GetDistrictIdFromAddress(string address)
        {
            var districtUrl = "https://online-gateway.ghn.vn/shiip/public-api/master-data/district";
            var wardUrl = "https://online-gateway.ghn.vn/shiip/public-api/master-data/ward";
            var apiKey = "47c6c1d4-0f65-11ee-8a8c-6e4795e6d902";

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Token", apiKey);

                // Lấy mã huyện từ địa chỉ
                var districtPayload = JsonConvert.SerializeObject(new { address = address });
                var districtContent = new StringContent(districtPayload, Encoding.UTF8, "application/json");
                var districtResponse = await httpClient.PostAsync(districtUrl, districtContent);

                if (districtResponse.IsSuccessStatusCode)
                {
                    var districtResponseContent = await districtResponse.Content.ReadAsStringAsync();
                    dynamic districtData = JsonConvert.DeserializeObject(districtResponseContent);
                    JArray data = districtData.data;
                    string districtId = districtData.data[0]["DistrictID"].ToString();
                    foreach (var item in data)
                    {
                        var keyword = "Quận";
                        var segments = address.Split(',');
                        foreach (var segment in segments)
                        {
                            if (segment.Contains(keyword))
                            {
                                var district = segment.Trim();
                                var districtName = item["DistrictName"].Value<string>();
                                if (string.Equals(districtName, district, StringComparison.OrdinalIgnoreCase))
                                {
                                    districtId = item["DistrictID"].Value<int>().ToString();
                                    break;
                                }
                            }
                        }

                    }
                    // Lấy mã xã từ địa chỉ
                    var wardPayload = $"{{ \"district_id\": {districtId} }}";
                    var wardContent = new StringContent(wardPayload, Encoding.UTF8, "application/json");
                    var wardResponse = await httpClient.PostAsync(wardUrl, wardContent);

                    if (wardResponse.IsSuccessStatusCode)
                    {
                        var wardResponseContent = await wardResponse.Content.ReadAsStringAsync();
                        dynamic wardData = JsonConvert.DeserializeObject(wardResponseContent);
                        string wardId = wardData.data[0]["WardCode"].ToString();

                        // Trả về mã huyện và xã
                        return $"{districtId}-{wardId}";
                    }

                    else
                    {
                        throw new Exception($"Failed to retrieve ward ID. Error code: {wardResponse.StatusCode}");
                    }
                }
                else
                {
                    throw new Exception($"Failed to retrieve district ID. Error code: {districtResponse.StatusCode}");
                }
            }
        }


    }

}
