using System.Text.Json;

namespace WebActionResults.Services;

public interface ILocalizationService
{
    string CurrentLanguage { get; }
    string Get(string key);
    string Get(string key, params object[] args);
    void SetLanguage(string languageCode);
}

public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        ["en"] = new Dictionary<string, string>
        {
            ["AppName"] = "Furnish",
            ["Home"] = "Home",
            ["Shop"] = "Shop",
            ["Categories"] = "Categories",
            ["Cart"] = "Cart",
            ["Checkout"] = "Checkout",
            ["Login"] = "Login",
            ["Logout"] = "Logout",
            ["Register"] = "Register",
            ["Profile"] = "Profile",
            ["Orders"] = "Orders",
            ["Wishlist"] = "Wishlist",
            ["SearchPlaceholder"] = "Search products...",
            ["AddToCart"] = "Add to Cart",
            ["AddedToCart"] = "Added to cart",
            ["RemoveFromCart"] = "Remove from cart",
            ["ClearCart"] = "Clear Cart",
            ["ContinueShopping"] = "Continue Shopping",
            ["ProceedToCheckout"] = "Proceed to Checkout",
            ["ShoppingCart"] = "Shopping Cart",
            ["YourCartIsEmpty"] = "Your cart is empty",
            ["StartShopping"] = "Start Shopping",
            ["Product"] = "Product",
            ["Price"] = "Price",
            ["Quantity"] = "Quantity",
            ["Total"] = "Total",
            ["Subtotal"] = "Subtotal",
            ["Shipping"] = "Shipping",
            ["Discount"] = "Discount",
            ["VariantAdjustments"] = "Variant Adjustments",
            ["OrderSummary"] = "Order Summary",
            ["PlaceOrder"] = "Place Order",
            ["BackToCart"] = "Back to Cart",
            ["HaveCoupon"] = "Have a coupon?",
            ["ApplyCoupon"] = "Apply",
            ["RemoveCoupon"] = "Remove",
            ["CouponApplied"] = "Coupon applied",
            ["InvalidCoupon"] = "Invalid coupon code",
            ["Success"] = "Success",
            ["Error"] = "Error",
            ["Warning"] = "Warning",
            ["Info"] = "Information",
            ["ViewDetails"] = "View details",
            ["ProductDetails"] = "Product Details",
            ["Description"] = "Description",
            ["Reviews"] = "Reviews",
            ["RelatedProducts"] = "Related Products",
            ["OutOfStock"] = "Out of stock",
            ["PriceBreakdown"] = "Price breakdown",
            ["UnitsAvailable"] = "units available",
            ["Options"] = "Options",
            ["SelectedOptions"] = "Selected Options",
            ["BasePrice"] = "Base",
            ["Variant"] = "Variant",
            ["SecureCheckout"] = "Secure Checkout",
            ["FastDelivery"] = "Fast Delivery",
            ["EasyReturns"] = "Easy Returns",
            ["LoginToCheckout"] = "Login to Checkout",
            ["WelcomeBack"] = "Welcome Back",
            ["DontHaveAccount"] = "Don't have an account?",
            ["AlreadyHaveAccount"] = "Already have an account?",
            ["SignUp"] = "Sign Up",
            ["Email"] = "Email",
            ["Password"] = "Password",
            ["FullName"] = "Full Name",
            ["Phone"] = "Phone",
            ["Address"] = "Address",
            ["City"] = "City",
            ["District"] = "District",
            ["Ward"] = "Ward",
            ["SaveAddress"] = "Save Address",
            ["EditAddress"] = "Edit Address",
            ["AddAddress"] = "Add Address",
            ["ShippingAddress"] = "Shipping Address",
            ["PaymentMethod"] = "Payment Method",
            ["OrderNotes"] = "Order Notes (optional)",
            ["PlaceOrderConfirm"] = "Place Order",
            ["OrderPlaced"] = "Order placed successfully!",
            ["OrderNumber"] = "Order Number",
            ["OrderDate"] = "Order Date",
            ["OrderStatus"] = "Status",
            ["OrderDetails"] = "Order Details",
            ["MyOrders"] = "My Orders",
            ["NoOrdersYet"] = "You haven't placed any orders yet",
            ["Admin"] = "Admin",
            ["Dashboard"] = "Dashboard",
            ["Products"] = "Products",
            ["Accounts"] = "Accounts",
            ["Coupons"] = "Coupons",
            ["Language"] = "Language",
            ["English"] = "English",
            ["Vietnamese"] = "Vietnamese",
            ["NewArrivals"] = "NEW ARRIVALS",
            ["ExchangeOldFurniture"] = "Exchange Your Old Furniture",
            ["SaveUpTo100"] = "Save up to $100 on quality furniture",
            ["Explore"] = "Explore",
            ["PremiumCollection"] = "PREMIUM COLLECTION",
            ["RoyalComfortSofa"] = "Royal Comfort Sofa",
            ["TimelessCraftsmanship"] = "Timeless craftsmanship meets modern design",
            ["ViewCollection"] = "View Collection",
            ["ShopbyCategory"] = "Shop by Category",
            ["FindPerfectFurniture"] = "Find the perfect furniture for every room",
            ["OurFavouriteCollection"] = "Our Favourite Collection",
            ["InspiredByLife"] = "We are inspired by the realities of life today, in which traditional divides between personal and professional space are more fluid.",
            ["LimitedTimeOffer"] = "Limited Time Offer",
            ["Get25PercentOff"] = "Get 25% Off on Premium Sofas",
            ["TransformYourSpace"] = "Transform your living space with our exclusive collection of premium furniture.",
            ["ShopNow"] = "Shop Now",
            ["FreeShipping"] = "Free Shipping",
            ["SecurePayment"] = "Secure Payment",
            ["Support247"] = "24/7 Support",
            ["WhatOurCustomersSay"] = "What Our Customers Say",
            ["TrustedByThousands"] = "Trusted by thousands of happy customers",
            ["Testimonial1"] = "Amazing quality furniture! The delivery was fast and the assembly was easy. Highly recommend Furnish.",
            ["Testimonial2"] = "Great customer service and beautiful products. The sofa I purchased is comfortable and looks stunning.",
            ["Testimonial3"] = "The furniture exceeded my expectations. Solid construction and elegant design.",
            ["EnterYourEmail"] = "Enter your email address",
            ["Subscribe"] = "Subscribe"
        },
        ["vi"] = new Dictionary<string, string>
        {
            ["AppName"] = "Furnish",
            ["Home"] = "Trang chủ",
            ["Shop"] = "Cửa hàng",
            ["Categories"] = "Danh mục",
            ["Cart"] = "Giỏ hàng",
            ["Checkout"] = "Thanh toán",
            ["Login"] = "Đăng nhập",
            ["Logout"] = "Đăng xuất",
            ["Register"] = "Đăng ký",
            ["Profile"] = "Hồ sơ",
            ["Orders"] = "Đơn hàng",
            ["Wishlist"] = "Yêu thích",
            ["SearchPlaceholder"] = "Tìm sản phẩm...",
            ["AddToCart"] = "Thêm vào giỏ",
            ["AddedToCart"] = "Đã thêm vào giỏ",
            ["RemoveFromCart"] = "Xóa khỏi giỏ",
            ["ClearCart"] = "Xóa giỏ hàng",
            ["ContinueShopping"] = "Tiếp tục mua sắm",
            ["ProceedToCheckout"] = "Tiến hành thanh toán",
            ["ShoppingCart"] = "Giỏ hàng",
            ["YourCartIsEmpty"] = "Giỏ hàng của bạn trống",
            ["StartShopping"] = "Bắt đầu mua sắm",
            ["Product"] = "Sản phẩm",
            ["Price"] = "Giá",
            ["Quantity"] = "Số lượng",
            ["Total"] = "Tổng cộng",
            ["Subtotal"] = "Tạm tính",
            ["Shipping"] = "Vận chuyển",
            ["Discount"] = "Giảm giá",
            ["VariantAdjustments"] = "Phí biến thể",
            ["OrderSummary"] = "Tóm tắt đơn hàng",
            ["PlaceOrder"] = "Đặt hàng",
            ["BackToCart"] = "Quay lại giỏ hàng",
            ["HaveCoupon"] = "Có mã giảm giá?",
            ["ApplyCoupon"] = "Áp dụng",
            ["RemoveCoupon"] = "Xóa",
            ["CouponApplied"] = "Đã áp dụng mã giảm giá",
            ["InvalidCoupon"] = "Mã giảm giá không hợp lệ",
            ["Success"] = "Thành công",
            ["Error"] = "Lỗi",
            ["Warning"] = "Cảnh báo",
            ["Info"] = "Thông tin",
            ["ViewDetails"] = "Xem chi tiết",
            ["ProductDetails"] = "Chi tiết sản phẩm",
            ["Description"] = "Mô tả",
            ["Reviews"] = "Đánh giá",
            ["RelatedProducts"] = "Sản phẩm liên quan",
            ["OutOfStock"] = "Hết hàng",
            ["UnitsAvailable"] = "sản phẩm có sẵn",
            ["PriceBreakdown"] = "Chi tiết giá",
            ["Options"] = "Tùy chọn",
            ["SelectedOptions"] = "Tùy chọn đã chọn",
            ["BasePrice"] = "Giá gốc",
            ["Variant"] = "Biến thể",
            ["SecureCheckout"] = "Thanh toán bảo mật",
            ["FastDelivery"] = "Giao hàng nhanh",
            ["EasyReturns"] = "Đổi trả dễ dàng",
            ["LoginToCheckout"] = "Đăng nhập để thanh toán",
            ["WelcomeBack"] = "Chào mừng trở lại",
            ["DontHaveAccount"] = "Chưa có tài khoản?",
            ["AlreadyHaveAccount"] = "Đã có tài khoản?",
            ["SignUp"] = "Đăng ký",
            ["Email"] = "Email",
            ["Password"] = "Mật khẩu",
            ["FullName"] = "Họ và tên",
            ["Phone"] = "Số điện thoại",
            ["Address"] = "Địa chỉ",
            ["City"] = "Thành phố",
            ["District"] = "Quận/Huyện",
            ["Ward"] = "Phường/Xã",
            ["SaveAddress"] = "Lưu địa chỉ",
            ["EditAddress"] = "Sửa địa chỉ",
            ["AddAddress"] = "Thêm địa chỉ",
            ["ShippingAddress"] = "Địa chỉ giao hàng",
            ["PaymentMethod"] = "Phương thức thanh toán",
            ["OrderNotes"] = "Ghi chú đơn hàng (tùy chọn)",
            ["PlaceOrderConfirm"] = "Đặt hàng",
            ["OrderPlaced"] = "Đặt hàng thành công!",
            ["OrderNumber"] = "Mã đơn hàng",
            ["OrderDate"] = "Ngày đặt",
            ["OrderStatus"] = "Trạng thái",
            ["OrderDetails"] = "Chi tiết đơn hàng",
            ["MyOrders"] = "Đơn hàng của tôi",
            ["NoOrdersYet"] = "Bạn chưa có đơn hàng nào",
            ["Admin"] = "Quản trị",
            ["Dashboard"] = "Bảng điều khiển",
            ["Products"] = "Sản phẩm",
            ["Accounts"] = "Tài khoản",
            ["Coupons"] = "Mã giảm giá",
            ["Language"] = "Ngôn ngữ",
            ["English"] = "English",
            ["Vietnamese"] = "Tiếng Việt",
            ["NewArrivals"] = "HÀNG MỚI VỀ",
            ["ExchangeOldFurniture"] = "Đổi Nội Thất Cũ",
            ["SaveUpTo100"] = "Tiết kiệm đến 100$ cho nội thất chất lượng",
            ["Explore"] = "Khám phá",
            ["PremiumCollection"] = "BỘ SƯU TẬP CAO CẤP",
            ["RoyalComfortSofa"] = "Sofa Cao Cấp Hoàng Gia",
            ["TimelessCraftsmanship"] = "Thủ công vượt thời gian kết hợp thiết kế hiện đại",
            ["ViewCollection"] = "Xem Bộ Sưu Tập",
            ["ShopbyCategory"] = "Mua theo Danh Mục",
            ["FindPerfectFurniture"] = "Tìm nội thất hoàn hảo cho mọi phòng",
            ["OurFavouriteCollection"] = "Bộ Sưu Tập Ưa Thích",
            ["InspiredByLife"] = "Chúng tôi lấy cảm hứng từ thực tế cuộc sống ngày nay, nơi ranh giới giữa không gian cá nhân và chuyên nghiệp ngày càng trở nên linh hoạt.",
            ["LimitedTimeOffer"] = "Ưu Đãi Giới Hạn",
            ["Get25PercentOff"] = "Giảm 25% Sofa Cao Cấp",
            ["TransformYourSpace"] = "Biến không gian sống của bạn với bộ sưu tập nội thất cao cấp độc quyền của chúng tôi.",
            ["ShopNow"] = "Mua Ngay",
            ["FreeShipping"] = "Miễn Phí Vận Chuyển",
            ["SecurePayment"] = "Thanh Toán Bảo Mật",
            ["Support247"] = "Hỗ Trợ 24/7",
            ["WhatOurCustomersSay"] = "Khách Hàng Nói Gì",
            ["TrustedByThousands"] = "Được hàng nghìn khách hàng tin tưởng",
            ["Testimonial1"] = "Nội thất chất lượng tuyệt vời! Giao hàng nhanh và lắp ráp dễ dàng. Highly recommend Furnish.",
            ["Testimonial2"] = "Dịch vụ khách hàng tuyệt vời và sản phẩm đẹp. Sofa tôi mua rất thoải mái và trông tuyệt vời.",
            ["Testimonial3"] = "Nội thất vượt quá kỳ vọng của tôi. Kết cấu chắc chắn và thiết kế thanh lịch.",
            ["EnterYourEmail"] = "Nhập địa chỉ email của bạn",
            ["Subscribe"] = "Đăng ký"
        }
    };

    public LocalizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string CurrentLanguage => DetectLanguage();

    private string DetectLanguage()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "en";

        var sessionLang = context.Session.GetString("Language");
        if (!string.IsNullOrEmpty(sessionLang) && _translations.ContainsKey(sessionLang))
            return sessionLang;

        var cookieLang = context.Request.Cookies["Language"];
        if (!string.IsNullOrEmpty(cookieLang) && _translations.ContainsKey(cookieLang))
            return cookieLang;

        var acceptLang = context.Request.Headers["Accept-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(acceptLang))
        {
            if (acceptLang.Contains("vi", StringComparison.OrdinalIgnoreCase))
                return "vi";
            if (acceptLang.Contains("en", StringComparison.OrdinalIgnoreCase))
                return "en";
        }

        return "en";
    }

    public string Get(string key)
    {
        var lang = DetectLanguage();
        if (_translations.TryGetValue(lang, out var langDict))
        {
            if (langDict.TryGetValue(key, out var value))
                return value;
        }
        if (_translations.TryGetValue("en", out var enDict))
        {
            if (enDict.TryGetValue(key, out var value))
                return value;
        }
        return key;
    }

    public string Get(string key, params object[] args)
    {
        var template = Get(key);
        return string.Format(template, args);
    }

    public void SetLanguage(string languageCode)
    {
        if (_translations.ContainsKey(languageCode))
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Session.SetString("Language", languageCode);
                context.Response.Cookies.Append("Language", languageCode, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddYears(1),
                    SameSite = SameSiteMode.Lax
                });
            }
        }
    }
}