using CuaHangHoa.Data;
using CuaHangHoa.Models;
using CuaHangHoa.Services;
using CuaHangHoa.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using webapi.Services;

namespace CuaHangHoa.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly MyDbContext _dbcontext;
        private readonly ISendMailSerVice _sendMailService;
        private readonly IVnPayService _vnPayService;

        public CartController(MyDbContext dbcontext, ISendMailSerVice sendMailSerVice, IVnPayService vnPayService)
        {
            _dbcontext = dbcontext;
            _sendMailService = sendMailSerVice;
            _vnPayService = vnPayService;
        }

        public async Task<IActionResult> ShoppingCart()
        {

            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = user.Value;

            var listItem = await _dbcontext.ChiTietGioHangs
                .Where(e => e.UserId == userId)
                .Include(e => e.SanPham)
                .Include(e => e.SanPham.Hinhs)
                .ToListAsync();

            return View(listItem);
        }

        [HttpGet]
        public int CountCartItem()
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                return 0;
            }
            string userId = user.Value;

            var quantity = _dbcontext.ChiTietGioHangs
                .Where(e => e.UserId == userId).Count();
            return quantity;
        }

        [HttpPost]
        public async Task<IActionResult> ChangeQuantity(int sanphamId, int sl)
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = user.Value;

            var cartItem = await _dbcontext.ChiTietGioHangs
                .Where(e => e.UserId == userId && e.SanPhamId == sanphamId)
                .SingleOrDefaultAsync();
            if (cartItem == null || sl < 1)
            {
                return BadRequest("Số lượng phải lớn hơn 0");
            }

            // Lấy số lượng sản phẩm trong kho
            var product = await _dbcontext.SanPhams.FirstOrDefaultAsync(p => p.Id == sanphamId);
            if (product == null)
            {
                return NotFound(); // Không tìm thấy sản phẩm trong kho
            }

            // Kiểm tra số lượng sản phẩm trong giỏ hàng với số lượng sản phẩm trong kho
            if (sl > product.Soluongkho)
            {
                return BadRequest("Số lượng sản phẩm vượt quá số lượng trong kho!");
            }
            cartItem.SoLuong = sl;
            await _dbcontext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int sanphamId, int sl)
        {
            try
            {
                var user = User.FindFirst(ClaimTypes.NameIdentifier);
                if (user == null)
                {
                    return Unauthorized();
                }
                string userId = user.Value;

                var product = await _dbcontext.SanPhams.FirstOrDefaultAsync(p => p.Id == sanphamId);
                if (product == null)
                {
                    return NotFound(); // Không tìm thấy sản phẩm trong kho
                }

                // Kiểm tra số lượng sản phẩm trong kho
                if (sl > product.Soluongkho)
                {
                    return BadRequest("Số lượng sản phẩm vượt quá số lượng trong kho!");
                }

                var cartItem = await _dbcontext.ChiTietGioHangs
                    .SingleOrDefaultAsync(e => e.UserId == userId && e.SanPhamId == sanphamId);
                if (cartItem == null)
                {
                    var model = new ChiTietGioHang
                    {
                        UserId = userId,
                        SanPhamId = sanphamId,
                        SoLuong = sl
                    };
                    await _dbcontext.AddAsync(model);
                }
                else
                {
                    cartItem.SoLuong += sl;
                }
                await _dbcontext.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCartItem(int sanphamId)
        {
            try
            {
                var user = User.FindFirst(ClaimTypes.NameIdentifier);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");

                }
                string userId = user.Value;
                var cartItem = await _dbcontext.ChiTietGioHangs
                    .Where(e => e.UserId == userId && e.SanPhamId == sanphamId)
                    .SingleOrDefaultAsync();
                if (cartItem == null)
                {
                    RedirectToAction("Index", "Home");
                }
                _dbcontext.ChiTietGioHangs.Remove(cartItem);
                await _dbcontext.SaveChangesAsync();
                return RedirectToAction("ShoppingCart");
            }
            catch
            {
                return View("ShoppingCart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllCartItem()
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = user.Value;

            var cartItem = await _dbcontext.ChiTietGioHangs
                .Where(e => e.UserId == userId)
                .ToListAsync();
            if (cartItem.IsNullOrEmpty())
            {
                return RedirectToAction("Index", "Home");

            }
            _dbcontext.ChiTietGioHangs.RemoveRange(cartItem);
            await _dbcontext.SaveChangesAsync();
            return RedirectToAction("ShoppingCart");
        }

        public async Task<IActionResult> Checkout()
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = user.Value;

            var listItem = await _dbcontext.ChiTietGioHangs
                .Where(e => e.UserId == userId)
                .Include(e => e.SanPham)
                .Include(e => e.SanPham.Hinhs)
                .ToListAsync();

            if (listItem.IsNullOrEmpty())
            {
                return NotFound();
            }
            var u = await _dbcontext.Users.FindAsync(userId);
            ViewBag.Name = u.FirstName + " " + u.LastName;
            ViewBag.Phone = u.PhoneNumber;
            var address = await _dbcontext.DiaChis
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync(e => e.UserId == userId);
            if (address != null)
            {
                ViewBag.Address = address.Dchi ?? string.Empty;
            }
            // Tính tổng tiền và phí vận chuyển
            double subtotal = listItem.Sum(item =>
            (item.SanPham.PhanTramGiamGia > 0 ? item.SanPham.GiaSauGiamGia : item.SanPham.Dongia) * item.SoLuong);
            //double subtotal = listItem.Sum(item => item.SanPham.Dongia * item.SoLuong);
            double shippingFee = 30000;
            ViewBag.Subtotal = subtotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Total = subtotal + shippingFee;
            return View(listItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckoutCOD(IFormCollection forms)
        {
            try
            {
                var user = User.FindFirst(ClaimTypes.NameIdentifier);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                string userId = user.Value;

                var listCart = await _dbcontext.ChiTietGioHangs
                    .Include(ci => ci.SanPham)
                    .Where(ci => ci.UserId == userId)
                    .ToListAsync();
                double total = 0;
                foreach (var cartItem in listCart)
                {
                    double price = cartItem.SanPham.PhanTramGiamGia > 0 ? cartItem.SanPham.GiaSauGiamGia : cartItem.SanPham.Dongia;
                    total += price * cartItem.SoLuong;
                }

                //double total = 0;
                //foreach (var cartItem in listCart)
                //    total += (cartItem.SanPham.Dongia) * cartItem.SoLuong;
                //total = Math.Round(total);

                const double shippingFee = 30000;
                total += shippingFee;

                var address = forms["address"] + "; " + forms["ward"] + "; "
                    + forms["district"] + "; " + forms["city"];
                var rec = forms["phone"] + "; " + forms["name"];

                var user_address = await _dbcontext.DiaChis
                    .Where(e => e.UserId == userId && e.Dchi == address)
                    .ToListAsync();
                if (!user_address.IsNullOrEmpty())
                {
                    _dbcontext.DiaChis.RemoveRange(user_address);
                }
                var model = new DiaChi
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Dchi = address
                };
                await _dbcontext.DiaChis.AddAsync(model);

                var order = new DonHang
                {
                    UserId = userId,
                    TenTrangThai = "Đang xử lý",
                    NgayTao = DateTime.Now,
                    PhiVanChuyen = shippingFee,
                    TongTien = total,
                    TenCachThanhToan = "COD",
                    DchiGiaoHang = address,
                    NguoiNhan = rec,
                    ChiTietDHs = new List<ChiTietDH>()
                };

                await _dbcontext.DonHangs.AddAsync(order);
                await _dbcontext.SaveChangesAsync();

                foreach (var orderDetail in listCart)
                {
                    // Lấy thông tin sản phẩm từ cơ sở dữ liệu
                    var product = await _dbcontext.SanPhams.FindAsync(orderDetail.SanPhamId);
                    if (product != null)
                    {
                        // Trừ số lượng sản phẩm đặt hàng khỏi số lượng sản phẩm trong kho
                        product.Soluongkho -= orderDetail.SoLuong;
                    }
                }

                // Lưu lại thay đổi vào cơ sở dữ liệu
                await _dbcontext.SaveChangesAsync();

                foreach (var cartItem in listCart)
                {
                    var orderDetail = new ChiTietDH
                    {
                        DonHangId = order.Id,
                        SanPhamId = cartItem.SanPhamId,
                        SoLuong = cartItem.SoLuong,
                        //TongGia = (cartItem.SanPham.Dongia) * cartItem.SoLuong,
                        //GiaSP = cartItem.SanPham.Dongia
                        GiaSP = cartItem.SanPham.PhanTramGiamGia > 0 ? cartItem.SanPham.GiaSauGiamGia : cartItem.SanPham.Dongia,
                        TongGia = (cartItem.SanPham.PhanTramGiamGia > 0 ? cartItem.SanPham.GiaSauGiamGia : cartItem.SanPham.Dongia) * cartItem.SoLuong
                    };
                    await _dbcontext.ChiTietDHs.AddAsync(orderDetail);
                }

                _dbcontext.ChiTietGioHangs.RemoveRange(listCart);
                await _dbcontext.SaveChangesAsync();

                // Gửi email xác nhận đặt hàng
                var email = User.FindFirstValue(ClaimTypes.Email);
                var subject = "FLOSUNSHOP - Xác nhận đặt hàng ( phương thức thanh toán COD)";
                var expectedDeliveryDate = DateTime.Now.AddDays(5);

                // Lấy thông tin chi tiết đơn hàng
                var orderDetails = "";
                foreach (var cartItem in listCart)
                {
                    orderDetails += "<tr>" +
                                    "<td>" + cartItem.SanPham.Ten + "</td>" +
                                    "<td>" + cartItem.SoLuong + "</td>" +
                                    "<td>" + (cartItem.SanPham.Dongia).ToString("N0") + " đ</td>" +
                                    "<td>" + (cartItem.SanPham.Dongia * cartItem.SoLuong).ToString("N0") + " đ</td>" +
                                    "</tr>";
                }

                // Tạo HTML cho bảng chi tiết đơn hàng
                var orderDetailsHtml =
                    "<table>" +
                    "<tr>" +
                    "<th>Sản phẩm</th>" +
                    "<th>Số lượng</th>" +
                    "<th>Giá</th>" +
                    "<th>Tổng cộng</th>" +
                    "</tr>" +
                    orderDetails +
                    "</table>";

                var htmlMessage =
                     "<p>Xin chào!,</p>\r\n   " +
                     "<p>Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi. Đơn hàng của bạn đã được nhận và đang được xử lý.</p>" +
                     "<p>Mã đơn hàng: " + order.Id + "</p>" +
                     "<p>Ngày giao hàng dự kiến: " + expectedDeliveryDate.ToString("dd/MM/yyyy") + "</p>" +
                     "<p>Thông tin chi tiết đơn hàng:</p>" +
                     orderDetailsHtml +
                     "<p>Địa chỉ giao hàng: " + address + "</p>" +
                     "<p>Người nhận: " + rec + "</p>" +
                     "<p>Phí vạn chuyển: " + shippingFee.ToString("N0") + "</p>" +
                     "<p>Tổng tiền: " + total.ToString("N0") + " đ</p>" +
                     "<p>Chúng tôi hy vọng bạn sẽ hài lòng về sản phẩm.</p>" +
                     "<p>Chúc bạn một ngày tốt lành❤️</p>" +
                    "<br>" +
                    "<p>Thân mến,</p>" +
                    "<p>FloSun_Shop</p>";

                await _sendMailService.SendEmailAsync(email, subject, htmlMessage);



                return RedirectToAction("PaymentSuccess", "Cart");
            }
            catch
            {

                return View("ShoppingCart"); // Hoặc hiển thị một view lỗi cụ thể
            }
        }


        //vn pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckoutVNPay(IFormCollection forms)
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = user.Value;

            var listCart = await _dbcontext.ChiTietGioHangs
                    .Include(ci => ci.SanPham)
                    .Where(ci => ci.UserId == userId)
                    .ToListAsync();

            double total = 0;
            foreach (var cartItem in listCart)
            {
                double price = cartItem.SanPham.PhanTramGiamGia > 0 ? cartItem.SanPham.GiaSauGiamGia : cartItem.SanPham.Dongia;
                total += price * cartItem.SoLuong;
            }

            //double total = 0;
            //foreach (var cartItem in listCart)
            //    total += (cartItem.SanPham.Dongia ) * cartItem.SoLuong;
            //total = Math.Round(total);
            const double shippingFee = 30000;
            total += shippingFee;

            var address = forms["address"] + "; " + forms["ward"] + "; "
                    + forms["district"] + "; " + forms["city"];
            var rec = forms["phone"] + "; " + forms["name"];

            var user_address = await _dbcontext.DiaChis
                    .Where(e => e.UserId == userId && e.Dchi == address)
                    .ToListAsync();
            if (!user_address.IsNullOrEmpty())
            {
                _dbcontext.DiaChis.RemoveRange(user_address);
            }
            var model = new DiaChi
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Dchi = address
            };
            await _dbcontext.DiaChis.AddAsync(model);

            var order = new DonHang
            {
                UserId = userId,
                TenTrangThai = "Đang xử lý",
                NgayTao = DateTime.Now,
                PhiVanChuyen = shippingFee,
                TongTien = total,
                NguoiNhan = rec,
                DchiGiaoHang = address,
                DaThanhToan = true,
                TenCachThanhToan = "VNPay",
                ChiTietDHs = new List<ChiTietDH>()
            };

            await _dbcontext.DonHangs.AddAsync(order);
            await _dbcontext.SaveChangesAsync();

            foreach (var orderDetail in listCart)
            {
                // Lấy thông tin sản phẩm từ cơ sở dữ liệu
                var product = await _dbcontext.SanPhams.FindAsync(orderDetail.SanPhamId);
                if (product != null)
                {
                    // Trừ số lượng sản phẩm đặt hàng khỏi số lượng sản phẩm trong kho
                    product.Soluongkho -= orderDetail.SoLuong;
                }
            }

            // Lưu lại thay đổi vào cơ sở dữ liệu
            await _dbcontext.SaveChangesAsync();

            foreach (var cartItem in listCart)
            {
                var orderDetail = new ChiTietDH
                {
                    DonHangId = order.Id,
                    SanPhamId = cartItem.SanPhamId,
                    SoLuong = cartItem.SoLuong,
                    //TongGia = (cartItem.SanPham.Dongia) * cartItem.SoLuong,
                    //GiaSP = cartItem.SanPham.Dongia

                    GiaSP = cartItem.SanPham.PhanTramGiamGia > 0 ? cartItem.SanPham.GiaSauGiamGia : cartItem.SanPham.Dongia,
                    TongGia = (cartItem.SanPham.PhanTramGiamGia > 0 ? cartItem.SanPham.GiaSauGiamGia : cartItem.SanPham.Dongia) * cartItem.SoLuong
                };
                await _dbcontext.ChiTietDHs.AddAsync(orderDetail);
            }

            _dbcontext.ChiTietGioHangs.RemoveRange(listCart);
            await _dbcontext.SaveChangesAsync();


            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = total,
                CreatedDate = DateTime.Now,
                OrderId = order.Id,
            };

            return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repayment(int orderId)
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = user.Value;

            var order = await _dbcontext.DonHangs
                .SingleOrDefaultAsync(e => e.Id == orderId);
            if (order == null) { return NotFound(); }

            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = order.TongTien,
                CreatedDate = DateTime.Now,
                OrderId = order.Id,
            };
            return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
        }

        // Lưu ý do phương thức này sử dụng URL của VNPAY nên đừng thêm HttpPost
        // hay HttpGet và ValidateAntiForgeryToken ở phía trước nó sẽ khiến nó hoạt động không được
        // Lưu ý: Đừng thêm HttpPost hay HttpGet và ValidateAntiForgeryToken ở phía trước nó
        // Lưu ý: Đừng thêm HttpPost hay HttpGet và ValidateAntiForgeryToken ở phía trước nó
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                var subject = "FLOSUNSHOP - Thanh toán thất bại (VNPay)";
                var htmlMessage =
                    "<p>Xin chào!,</p>\r\n   " +
                    "<p>Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi. Đơn hàng của bạn đã được nhận và đang được xử lý.</p>" +
                    "<p>Vui lòng thanh toán lại sớm nhất có thể. Đơn hàng sẽ bị hủy sau 1 ngày nếu không được thanh toán</p>" +
                    "<p>Chúc bạn một ngày tốt lành❤️</p>" +
                "<br>" +
                "<p>Thân mến,</p>" +
                    "<p>FloSun_Shop</p>";
                await _sendMailService.SendEmailAsync(email, subject, htmlMessage);
                return RedirectToAction("OrderList", "User");
            }
            else
            {
                try
                {
                    var index = response.OrderDescription.LastIndexOf(":") + 1;
                    var orderId = int.Parse(response.OrderDescription.Substring(index));
                    var order = await _dbcontext.DonHangs
                        .Include(o => o.ChiTietDHs)
                        .ThenInclude(od => od.SanPham)
                        .SingleOrDefaultAsync(e => e.Id == orderId);
                    if (order == null)
                    {
                        return NotFound();
                    }

                    // Lấy thông tin chi tiết đơn hàng
                    var orderDetails = "";
                    foreach (var orderDetail in order.ChiTietDHs)
                    {
                        orderDetails += "<tr>" +
                                        "<td>" + orderDetail.SanPham.Ten + "</td>" +
                                        "<td>" + orderDetail.SoLuong + "</td>" +
                                        "<td>" + orderDetail.GiaSP.ToString("N0") + " đ</td>" +
                                        "<td>" + (orderDetail.TongGia).ToString("N0") + " đ</td>" +
                                        "</tr>";
                    }

                    // Tạo HTML cho bảng chi tiết đơn hàng
                    var orderDetailsHtml =
                        "<table>" +
                        "<tr>" +
                        "<th>Sản phẩm</th>" +
                        "<th>Số lượng</th>" +
                        "<th>Giá</th>" +
                        "<th>Tổng cộng</th>" +
                        "</tr>" +
                        orderDetails +
                        "</table>";

                    // Gửi email xác nhận đặt hàng
                    var email = User.FindFirstValue(ClaimTypes.Email);
                    var subject = "FLOSUNSHOP - Xác nhận đặt hàng (phương thức thanh toán VNPay)";
                    var expectedDeliveryDate = DateTime.Now.AddDays(5);

                    var htmlMessage =
                        "<p>Xin chào!,</p>\r\n   " +
                        "<p>Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi. Đơn hàng của bạn đã được nhận và đang được xử lý.</p>" +
                        "<p>Mã đơn hàng: " + order.Id + "</p>" +
                        "<p>Ngày giao hàng dự kiến: " + expectedDeliveryDate.ToString("dd/MM/yyyy") + "</p>" +
                        "<p>Thông tin chi tiết đơn hàng:</p>" +
                        orderDetailsHtml +
                        "<p>Địa chỉ giao hàng: " + order.DchiGiaoHang + "</p>" +
                        "<p>Người nhận: " + order.NguoiNhan + "</p>" +
                        "<p>Phí cận chuyển: " + order.PhiVanChuyen + "</p>" +
                        "<p>Tổng tiền: " + order.TongTien.ToString("N0") + " đ</p>" +
                        "<p>Chúng tôi hy vọng bạn sẽ hài lòng về sản phẩm.</p>" +
                        "<p>Chúc bạn một ngày tốt lành❤️</p>" +
                        "<br>" +
                        "<p>Thân mến,</p>" +
                        "<p>FloSun_Shop</p>";
                    await _sendMailService.SendEmailAsync(email, subject, htmlMessage);

                    return RedirectToAction("PaymentSuccess", "Cart");
                }
                catch
                {
                    throw new Exception();
                }
            }
        }

        public IActionResult PaymentSuccess()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _dbcontext.DonHangs
                .Include(e => e.ChiTietDHs)
                .SingleOrDefaultAsync(e => e.Id == orderId);
            if (order == null)
            {
                return NotFound();

            }
            else
            {
                if (order.TenTrangThai == "Đang xử lý")
                {
                    foreach (var item in order.ChiTietDHs)
                    {
                        var sanpham = await _dbcontext.SanPhams.SingleOrDefaultAsync(e => e.Id == item.SanPhamId);
                        if (sanpham != null)
                        {
                            sanpham.Soluongkho += item.SoLuong;
                        }
                    }
                    order.TenTrangThai = "Đã hủy";
                    await _dbcontext.SaveChangesAsync();

                    return RedirectToAction("OrderList", "User");
                }
                else return NotFound();
            }
        }
    }
}
