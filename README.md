# 🛍️ MiniSmartstore

MiniSmartstore là một hệ thống website thương mại điện tử được xây dựng bằng ASP.NET Core MVC, hỗ trợ đầy đủ các chức năng mua sắm trực tuyến cho khách hàng và cung cấp khu vực quản trị riêng dành cho Admin.

Hệ thống cho phép người dùng tìm kiếm sản phẩm, lọc sản phẩm, quản lý giỏ hàng, danh sách yêu thích, so sánh sản phẩm, đặt hàng, theo dõi đơn hàng và đánh giá sản phẩm.

Bên cạnh đó, Admin có thể quản lý sản phẩm, danh mục, tồn kho, khách hàng, đơn hàng, đánh giá, quy tắc sản phẩm và theo dõi các số liệu thống kê thông qua Dashboard.

---

# 📌 Mục lục

- [Giới thiệu](#-giới-thiệu)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Kiến trúc hệ thống](#-kiến-trúc-hệ-thống)
- [Chức năng khách hàng](#-chức-năng-khách-hàng)
- [Chức năng quản trị](#-chức-năng-quản-trị)
- [Quản lý sản phẩm](#-quản-lý-sản-phẩm)
- [Quản lý thời gian bán sản phẩm](#-quản-lý-thời-gian-bán-sản-phẩm)
- [Giỏ hàng](#-giỏ-hàng)
- [Thanh toán và đặt hàng](#-thanh-toán-và-đặt-hàng)
- [Wishlist và Compare](#-wishlist-và-compare)
- [Đánh giá sản phẩm](#-đánh-giá-sản-phẩm)
- [Xác thực người dùng](#-xác-thực-người-dùng)
- [Quản lý danh mục](#-quản-lý-danh-mục)
- [Dashboard Admin](#-dashboard-admin)
- [Cấu trúc dự án](#-cấu-trúc-dự-án)
- [Database](#-database)
- [Cấu hình dự án](#-cấu-hình-dự-án)
- [User Secrets](#-user-secrets)
- [Cách chạy dự án](#-cách-chạy-dự-án)
- [Git và GitHub](#-git-và-github)
- [Bảo mật](#-bảo-mật)
- [Giao diện](#-giao-diện)
- [Hướng phát triển](#-hướng-phát-triển)
- [Tác giả](#-tác-giả)
- [License](#-license)

---

# 📖 Giới thiệu

MiniSmartstore được xây dựng nhằm mô phỏng một hệ thống thương mại điện tử thực tế.

Website bao gồm hai khu vực chính:

### Storefront

Dành cho khách hàng và người dùng mua sắm.

### Admin Dashboard

Dành cho quản trị viên để quản lý toàn bộ hoạt động của cửa hàng.

Mục tiêu của dự án là xây dựng một website bán hàng có đầy đủ các nghiệp vụ cơ bản của một hệ thống E-Commerce như:

- Quản lý tài khoản.
- Quản lý sản phẩm.
- Quản lý danh mục.
- Quản lý tồn kho.
- Tìm kiếm và lọc sản phẩm.
- Giỏ hàng.
- Wishlist.
- So sánh sản phẩm.
- Đặt hàng.
- Thanh toán.
- Quản lý đơn hàng.
- Đánh giá sản phẩm.
- Quản trị hệ thống.
- Dashboard thống kê.

---

# 🛠️ Công nghệ sử dụng

## Backend

- C#
- ASP.NET Core MVC
- Entity Framework Core
- ASP.NET Core Identity
- LINQ
- Razor Pages
- Dependency Injection

## Frontend

- HTML5
- CSS3
- JavaScript
- Bootstrap
- Razor View
- Responsive Design

## Database

- Microsoft SQL Server
- Entity Framework Core

## Authentication

- ASP.NET Core Identity
- Email / Password
- Google OAuth
- Email OTP
- Role-based Authorization

## Công cụ phát triển

- Visual Studio
- SQL Server Management Studio
- Git
- GitHub
- Microsoft Edge / Google Chrome

---

# 🏗️ Kiến trúc hệ thống

MiniSmartstore sử dụng kiến trúc ASP.NET Core MVC.

```text
Client
   │
   ▼
Controller
   │
   ▼
Service / Business Logic
   │
   ▼
Entity Framework Core
   │
   ▼
SQL Server
```

Các thành phần chính:

```text
Model
│
├── Product
├── Category
├── CartItem
├── Order
├── OrderDetail
├── Payment
├── ProductReview
├── ProductImage
├── ProductColor
└── ApplicationUser

Controller
│
├── HomeController
├── ProductController
├── CartController
├── CheckoutController
├── OrderController
├── WishlistController
└── Admin Controllers

View
│
├── Home
├── Product
├── Cart
├── Checkout
├── Order
├── Shared
└── Admin Views
```

---

# 👤 Chức năng khách hàng

Khách hàng có thể sử dụng các chức năng:

### Tài khoản

- Đăng ký tài khoản.
- Đăng nhập.
- Đăng xuất.
- Đăng nhập bằng Google.
- Quên mật khẩu.
- Nhận mã OTP qua Email.
- Xác thực OTP.
- Đặt lại mật khẩu.

### Sản phẩm

- Xem tất cả sản phẩm.
- Xem sản phẩm theo danh mục.
- Xem chi tiết sản phẩm.
- Xem sản phẩm nổi bật.
- Xem sản phẩm khuyến mãi.
- Xem sản phẩm mới.
- Xem sản phẩm liên quan.

### Tìm kiếm

- Tìm kiếm theo tên sản phẩm.
- Tìm kiếm thông minh.
- Tìm kiếm theo từ khóa.
- Tìm kiếm theo danh mục.

### Bộ lọc

Người dùng có thể lọc sản phẩm theo:

- Danh mục.
- Khoảng giá.
- Tình trạng.
- Đánh giá.
- Giá tối thiểu.
- Giá tối đa.

### Giỏ hàng

- Thêm sản phẩm.
- Tăng số lượng.
- Giảm số lượng.
- Cập nhật số lượng.
- Xóa sản phẩm.
- Xóa toàn bộ giỏ hàng.
- Kiểm tra tồn kho.
- Kiểm tra trạng thái sản phẩm.

### Wishlist

- Thêm sản phẩm yêu thích.
- Xóa sản phẩm yêu thích.
- Xem danh sách yêu thích.

### So sánh

- Thêm sản phẩm vào danh sách so sánh.
- Xóa sản phẩm.
- So sánh thông tin sản phẩm.

### Đơn hàng

- Đặt hàng.
- Xem đơn hàng.
- Theo dõi trạng thái đơn hàng.
- Xem chi tiết đơn hàng.

### Đánh giá

- Xem đánh giá sản phẩm.
- Gửi đánh giá sản phẩm.
- Chấm điểm sản phẩm.

---

# 👨‍💼 Chức năng quản trị

Admin có khu vực quản trị riêng.

Các chức năng chính gồm:

- Dashboard.
- Quản lý sản phẩm.
- Quản lý danh mục.
- Quản lý khách hàng.
- Quản lý đơn hàng.
- Quản lý đánh giá.
- Quản lý tồn kho.
- Quản lý quy tắc sản phẩm.
- Báo cáo thống kê.
- Theo dõi sản phẩm bán chạy.
- Theo dõi khách hàng nổi bật.

Khu vực Admin được bảo vệ bằng:

```text
Role = Admin
```

---

# 📦 Quản lý sản phẩm

Admin có thể:

- Thêm sản phẩm mới.
- Chỉnh sửa sản phẩm.
- Xóa sản phẩm.
- Ẩn sản phẩm.
- Hiện sản phẩm.
- Cập nhật giá.
- Cập nhật giá cũ.
- Cập nhật tồn kho.
- Cập nhật hình ảnh.
- Quản lý ảnh phụ.
- Quản lý màu sắc.
- Đánh dấu sản phẩm nổi bật.
- Thiết lập thời gian bán.
- Quản lý SEO.
- Thiết lập thứ tự hiển thị.

Một số thuộc tính chính:

```text
Id
Name
Alias
ProductCode
Description
Price
BasePrice
OldPrice
StockQuantity
ImageUrl
CategoryId
IsActive
IsFeatured
AvailableStartDate
AvailableEndDate
CreatedAt
UpdatedAt
```

---

# ⏰ Quản lý thời gian bán sản phẩm

Hệ thống hỗ trợ thiết lập thời gian bán riêng cho từng sản phẩm.

Hai trường chính:

```text
AvailableStartDate
AvailableEndDate
```

## Quy tắc

Sản phẩm được phép bán khi:

```text
IsActive = true
```

và:

```text
AvailableStartDate <= CurrentTime
```

nếu có ngày bắt đầu.

Đồng thời:

```text
AvailableEndDate > CurrentTime
```

nếu có ngày kết thúc.

### Không thiết lập ngày bắt đầu

Sản phẩm có thể bán ngay.

### Không thiết lập ngày kết thúc

Sản phẩm được bán không giới hạn thời gian.

### Chưa đến ngày bắt đầu

Sản phẩm chưa được phép bán.

### Đã quá ngày kết thúc

Sản phẩm ngừng bán.

### Admin ẩn thủ công

Nếu:

```text
IsActive = false
```

thì sản phẩm không được phép bán dù vẫn còn thời gian.

---

# 🛒 Giỏ hàng

Hệ thống hỗ trợ giỏ hàng cho:

- Người dùng chưa đăng nhập.
- Người dùng đã đăng nhập.

## Guest Cart

Giỏ hàng của khách được lưu bằng:

```text
Session
```

## User Cart

Sau khi đăng nhập, dữ liệu giỏ hàng được lưu trong Database.

Model:

```text
CartItem
```

Một số thông tin:

```text
UserId
ProductId
Quantity
SelectedColor
CreatedAt
UpdatedAt
```

---

# ⚠️ Sản phẩm không còn khả dụng trong giỏ hàng

Nếu sản phẩm đã nằm trong giỏ nhưng sau đó:

- Admin ẩn sản phẩm.
- Sản phẩm hết thời gian bán.
- Sản phẩm hết hàng.
- Tồn kho không đủ.

thì sản phẩm vẫn được giữ lại trong giỏ để khách hàng nhận biết.

Hệ thống sẽ hiển thị trạng thái như:

```text
Đã ngừng bán
```

hoặc:

```text
Hết hàng
```

Sản phẩm không còn khả dụng:

- Không được tính vào tổng tiền thanh toán.
- Không được tạo OrderDetail.
- Không được trừ tồn kho.
- Người dùng vẫn có thể xóa khỏi giỏ.

---

# 💳 Thanh toán và đặt hàng

Quy trình cơ bản:

```text
Cart
  ↓
Billing Address
  ↓
Shipping
  ↓
Payment
  ↓
Confirm
  ↓
Place Order
  ↓
Completed
```

Trước khi đặt hàng, hệ thống kiểm tra:

- Sản phẩm còn tồn tại.
- Sản phẩm đang hoạt động.
- Sản phẩm còn trong thời gian bán.
- Sản phẩm còn tồn kho.
- Số lượng yêu cầu không vượt tồn kho.

Sau khi đặt hàng:

```text
StockQuantity = StockQuantity - Quantity
```

Hệ thống tạo:

```text
Order
OrderDetail
Payment
```

Chỉ các sản phẩm thực sự được đặt hàng mới bị xóa khỏi giỏ.

Các sản phẩm không khả dụng vẫn được giữ trong giỏ hàng.

---

# ❤️ Wishlist và Compare

## Wishlist

Cho phép người dùng lưu sản phẩm quan tâm.

Chức năng:

- Thêm vào Wishlist.
- Xóa khỏi Wishlist.
- Xem Wishlist.

## Compare

Cho phép người dùng đưa nhiều sản phẩm vào danh sách so sánh.

Có thể so sánh:

- Tên sản phẩm.
- Giá.
- Danh mục.
- Tình trạng.
- Thông tin sản phẩm.

---

# ⭐ Đánh giá sản phẩm

Người dùng có thể:

- Xem đánh giá.
- Xem điểm đánh giá.
- Gửi đánh giá.
- Chấm điểm sản phẩm.

Admin có thể:

- Xem đánh giá.
- Ẩn đánh giá.
- Hiện đánh giá.
- Xóa đánh giá.
- Lọc đánh giá theo trạng thái.

---

# 🔐 Xác thực người dùng

MiniSmartstore sử dụng:

```text
ASP.NET Core Identity
```

Các phương thức đăng nhập:

```text
Email + Password
Google OAuth
```

Các chức năng:

- Đăng ký.
- Đăng nhập.
- Đăng xuất.
- Quên mật khẩu.
- Email OTP.
- Reset Password.

---

# 📧 Email OTP

Quy trình quên mật khẩu:

```text
Quên mật khẩu
      ↓
Nhập Email
      ↓
Gửi OTP
      ↓
Xác minh OTP
      ↓
Đặt mật khẩu mới
```

Các thông tin nhạy cảm của Email không được lưu trực tiếp trong repository.

---

# 🔑 Phân quyền

Hệ thống sử dụng Role-based Authorization.

Ví dụ:

```text
Admin
Customer
```

Controller Admin sử dụng:

```csharp
[Authorize(Roles = "Admin")]
```

Nhờ đó người dùng thông thường không thể truy cập khu vực quản trị.

---

# 📂 Quản lý danh mục

Admin có thể:

- Tạo danh mục.
- Chỉnh sửa danh mục.
- Xóa danh mục.
- Upload ảnh danh mục.
- Ẩn / hiện danh mục.
- Thiết lập thứ tự hiển thị.
- Thiết lập danh mục xuất hiện ở trang chủ.
- Thiết lập danh mục xuất hiện trong Menu.

Một số thuộc tính:

```text
Id
ParentCategoryId
Name
Alias
Description
ImageUrl
IsActive
ShowOnHomePage
IncludeInMenu
DisplayOrder
BadgeText
MetaTitle
MetaDescription
MetaKeywords
```

Ảnh danh mục được lưu tại:

```text
wwwroot/images/categories
```

Trang chủ lấy ảnh trực tiếp từ:

```text
Category.ImageUrl
```

---

# 📊 Dashboard Admin

Dashboard cung cấp các thông tin tổng quan:

- Tổng số sản phẩm.
- Tổng số danh mục.
- Tổng số đơn hàng.
- Tổng số khách hàng.
- Đơn hàng chưa hoàn tất.
- Doanh thu.
- Sản phẩm bán chạy.
- Khách hàng nổi bật.
- Đơn hàng gần đây.

Một số khoảng thời gian thống kê:

```text
Hôm nay
Hôm qua
7 ngày qua
28 ngày qua
Năm nay
```

---

# 📜 Product Rule

Hệ thống có chức năng quản lý quy tắc sản phẩm.

Một số loại hành động:

```text
Discount
MarkAsFeatured
HideProduct
ShowProduct
```

Quy tắc có thể sử dụng:

```text
StartDate
EndDate
Priority
IsActive
```

để kiểm soát thời gian áp dụng.

---

# 🗂️ Cấu trúc dự án

```text
MiniSmartstoreMvc
│
├── Areas
│   │
│   ├── Admin
│   │   │
│   │   ├── Controllers
│   │   │   ├── CategoryController.cs
│   │   │   ├── CustomerController.cs
│   │   │   ├── DashboardController.cs
│   │   │   ├── OrderController.cs
│   │   │   ├── ProductController.cs
│   │   │   ├── ProductReviewController.cs
│   │   │   ├── ProductRuleController.cs
│   │   │   └── ReportController.cs
│   │   │
│   │   └── Views
│   │       ├── Category
│   │       ├── Customer
│   │       ├── Dashboard
│   │       ├── Order
│   │       ├── Product
│   │       ├── ProductReview
│   │       ├── ProductRule
│   │       ├── Report
│   │       └── Shared
│   │
│   └── Identity
│       │
│       └── Pages
│           └── Account
│               ├── Login.cshtml
│               ├── Register.cshtml
│               ├── ForgotPassword.cshtml
│               ├── VerifyOtp.cshtml
│               └── ResetPassword.cshtml
│
├── Controllers
│   ├── HomeController.cs
│   ├── ProductController.cs
│   ├── CartController.cs
│   ├── CheckoutController.cs
│   ├── WishlistController.cs
│   ├── CompareProductsController.cs
│   ├── OrderController.cs
│   ├── CustomerController.cs
│   ├── ServiceController.cs
│   └── ContactController.cs
│
├── Data
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs
│
├── Extensions
│   ├── ExternalAuthenticationExtensions.cs
│   └── ProductAvailabilityExtensions.cs
│
├── Helpers
│   └── ProductSeoSearchHelper.cs
│
├── Models
│   ├── ApplicationUser.cs
│   ├── Product.cs
│   ├── Category.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   ├── OrderDetail.cs
│   ├── Payment.cs
│   ├── ProductReview.cs
│   ├── ProductImage.cs
│   ├── ProductColor.cs
│   └── ProductRule.cs
│
├── Services
│   ├── EmailSender.cs
│   └── ProductRuleService.cs
│
├── ViewComponents
│   ├── CartCountViewComponent.cs
│   └── CategoryMenuViewComponent.cs
│
├── ViewModels
│
├── Views
│   │
│   ├── Home
│   ├── Product
│   ├── Cart
│   ├── Checkout
│   ├── Wishlist
│   ├── CompareProducts
│   ├── Order
│   └── Shared
│
├── wwwroot
│   │
│   ├── css
│   │   ├── site.css
│   │   └── auth-double.css
│   │
│   ├── images
│   │   │
│   │   ├── banners
│   │   ├── categories
│   │   ├── logo
│   │   ├── payment
│   │   └── products
│   │
│   ├── js
│   ├── lib
│   └── favicon.ico
│
├── Program.cs
├── appsettings.json
├── MiniSmartstoreMvc.csproj
└── README.md
```

---

# 🗄️ Database

Hệ thống sử dụng:

```text
Microsoft SQL Server
```

Database:

```text
MiniSmartstoreMvc
```

Một số bảng chính:

```text
AspNetUsers
AspNetRoles
AspNetUserRoles

Products
Categories

CartItems

Orders
OrderDetails
Payments

ProductReviews
ProductImages
ProductColors
ProductRules
```

Entity Framework Core chịu trách nhiệm:

- Mapping Model.
- Query dữ liệu.
- Thêm dữ liệu.
- Sửa dữ liệu.
- Xóa dữ liệu.
- Quản lý quan hệ.
- Migration.

---

# 🔗 Connection String

Ví dụ cấu hình trong:

```text
appsettings.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MiniSmartstoreMvc;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Tùy môi trường máy tính có thể thay:

```text
Server=localhost
```

bằng SQL Server Instance tương ứng.

---

# ⚙️ Cấu hình dự án

Ứng dụng được cấu hình tại:

```text
Program.cs
```

Một số service chính:

```csharp
builder.Services.AddControllersWithViews();
```

Entity Framework:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});
```

Identity:

```text
ASP.NET Core Identity
```

Session được sử dụng để quản lý dữ liệu tạm thời như:

- Guest Cart.
- Checkout.
- Một số trạng thái người dùng.

---

# 🔐 User Secrets

Không nên lưu thông tin nhạy cảm trong source code.

Các dữ liệu như:

```text
Google Client ID
Google Client Secret

Email Username
Email Password
SMTP Password

Admin Password

API Key
Secret Key
```

nên được lưu bằng:

```text
User Secrets
```

Khởi tạo:

```bash
dotnet user-secrets init
```

Ví dụ:

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_CLIENT_ID"
```

```bash
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_CLIENT_SECRET"
```

Không commit giá trị Secret thật lên GitHub.

---

# 💻 Yêu cầu môi trường

Để chạy project cần:

```text
Visual Studio
.NET SDK
SQL Server
SQL Server Management Studio
Git
```

Khuyến nghị:

```text
Windows 10 / Windows 11
Visual Studio 2022 hoặc mới hơn
Microsoft SQL Server
```

---

# 🚀 Cách chạy dự án

## Bước 1: Clone Repository

```bash
git clone https://github.com/YOUR_USERNAME/MiniSmartstoreMvc.git
```

## Bước 2: Di chuyển vào thư mục project

```bash
cd MiniSmartstoreMvc
```

## Bước 3: Restore NuGet Package

```bash
dotnet restore
```

## Bước 4: Kiểm tra Connection String

Mở:

```text
appsettings.json
```

và cấu hình SQL Server.

## Bước 5: Khởi tạo Database

Nếu project sử dụng EF Core Migration:

```bash
dotnet ef database update
```

## Bước 6: Build

```bash
dotnet build
```

## Bước 7: Chạy

```bash
dotnet run
```

Hoặc chạy trực tiếp bằng Visual Studio:

```text
Ctrl + F5
```

---

# 🌐 Storefront

Storefront bao gồm:

```text
Trang chủ
Tất cả sản phẩm
Điện thoại
Phụ kiện
Laptop
Đồng hồ
Sale
Tìm kiếm
Wishlist
Compare
Cart
Checkout
Order
Account
```

---

# 🖥️ Admin Dashboard

Khu vực Admin bao gồm:

```text
Dashboard
Sản phẩm
Danh mục
Bán hàng
Khách hàng
Đơn hàng
Đánh giá
Product Rule
Báo cáo
```

Admin Dashboard cung cấp giao diện riêng và yêu cầu tài khoản có quyền:

```text
Admin
```

---

# 🎨 Giao diện

MiniSmartstore sử dụng thiết kế hiện đại với tone màu chủ đạo:

```text
Blue
Navy
Purple
White
```

Website hỗ trợ Responsive Design để hiển thị tốt trên nhiều kích thước màn hình.

Logo MiniSmartstore được đặt trong:

```text
wwwroot/images/logo
```

---

# 🔒 Bảo mật

Hệ thống áp dụng một số cơ chế bảo mật:

- ASP.NET Core Identity.
- Password Hashing.
- Role-based Authorization.
- Anti-Forgery Token.
- Server-side Validation.
- Google OAuth.
- Email OTP.
- User Secrets.
- Kiểm soát quyền truy cập Admin.
- Kiểm tra sản phẩm trước Checkout.
- Kiểm tra tồn kho trước đặt hàng.
- Transaction khi tạo đơn hàng.

---

# 🧪 Kiểm tra nghiệp vụ

Một số trường hợp hệ thống cần kiểm tra:

### Product

```text
IsActive
AvailableStartDate
AvailableEndDate
StockQuantity
```

### Cart

```text
Sản phẩm tồn tại
Sản phẩm được phép bán
Còn tồn kho
Số lượng hợp lệ
```

### Checkout

```text
Cart không trống
Có ít nhất một sản phẩm hợp lệ
Tồn kho đủ
Thông tin giao hàng hợp lệ
Phương thức thanh toán hợp lệ
```

---

# 📈 Báo cáo và thống kê

Admin có thể theo dõi:

- Tổng số sản phẩm.
- Tổng số danh mục.
- Tổng số khách hàng.
- Tổng số đơn hàng.
- Doanh thu.
- Đơn hàng chưa hoàn tất.
- Sản phẩm bán chạy.
- Khách hàng nổi bật.
- Đơn hàng gần đây.

---

# 📋 Quy trình mua hàng

```text
Khách hàng
    │
    ▼
Tìm kiếm sản phẩm
    │
    ▼
Xem chi tiết
    │
    ▼
Thêm vào giỏ hàng
    │
    ▼
Kiểm tra giỏ hàng
    │
    ▼
Thanh toán
    │
    ▼
Thông tin giao hàng
    │
    ▼
Phương thức thanh toán
    │
    ▼
Xác nhận
    │
    ▼
Đặt hàng
    │
    ▼
Theo dõi đơn hàng
```

---

# 📋 Quy trình quản lý sản phẩm

```text
Admin
  │
  ▼
Danh sách sản phẩm
  │
  ├── Thêm sản phẩm
  │
  ├── Sửa sản phẩm
  │
  ├── Upload ảnh
  │
  ├── Quản lý tồn kho
  │
  ├── Thiết lập thời gian bán
  │
  ├── Ẩn / Hiện
  │
  └── Xóa sản phẩm
```

---

# 🔄 Git và GitHub

## Kiểm tra trạng thái

```bash
git status
```

## Thêm file

```bash
git add .
```

## Commit

```bash
git commit -m "Update MiniSmartstore"
```

## Push

```bash
git push origin main
```

---

# 🚫 File không nên push lên GitHub

`.gitignore` nên chứa:

```gitignore
.vs/
bin/
obj/

*.user
*.suo

appsettings.Secret.json

*.pfx
*.key
```

Không được đưa lên GitHub:

```text
Google Client Secret
Email Password
SMTP Password
Admin Password
Database Password
API Secret
```

---

# 📁 Ví dụ .gitignore

```gitignore
## Visual Studio
.vs/

## Build
bin/
obj/

## User files
*.user
*.suo

## Secrets
appsettings.Secret.json

## Certificates
*.pfx
*.key

## Logs
*.log
```

---

# ⚠️ Lưu ý

Repository không nên chứa tài khoản hoặc mật khẩu thật.

Nếu cần tài khoản Admin mặc định để chạy project, nên cấu hình thông qua:

```text
User Secrets
```

hoặc:

```text
Environment Variables
```

thay vì ghi trực tiếp vào README.

---

# 🔮 Hướng phát triển

Trong tương lai hệ thống có thể được mở rộng thêm:

- Thanh toán trực tuyến.
- VNPay.
- MoMo.
- PayPal.
- Stripe.
- Voucher.
- Coupon.
- Flash Sale.
- Loyalty Point.
- Recommendation System.
- AI Product Recommendation.
- Chatbot hỗ trợ khách hàng.
- Notification.
- Email xác nhận đơn hàng.
- Theo dõi giao hàng.
- Quản lý nhà cung cấp.
- Quản lý nhập kho.
- Phân tích doanh thu nâng cao.
- Dashboard thời gian thực.
- REST API.
- Mobile Application.
- Docker.
- Cloud Deployment.
- CI/CD.

---
#Tài khoản Admin default:
admin@mini.com
Admin@123456
