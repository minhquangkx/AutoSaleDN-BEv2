using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Models;
using Microsoft.AspNetCore.Authorization;
<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations;
using AutoSaleDN.Services;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Macs;
>>>>>>> 0e4a76f (Final Backend)

namespace AutoSaleDN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        private readonly AutoSaleDbContext _context;
        public CustomerController(AutoSaleDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = await _context.Users.FindAsync(userId);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] User model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Mobile = model.Mobile;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated" });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
                return BadRequest("Old password incorrect");
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Password changed" });
        }

        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var addresses = await _context.DeliveryAddresses.Where(a => a.UserId == userId).ToListAsync();
            return Ok(addresses);
        }

        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] DeliveryAddress model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            model.UserId = userId;
            _context.DeliveryAddresses.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Address added" });
        }

        [HttpPut("addresses/{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] DeliveryAddress model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var address = await _context.DeliveryAddresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);
            if (address == null) return NotFound();
            address.Address = model.Address;
            address.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Address updated" });
        }

        [HttpGet("cars")]
        public async Task<IActionResult> GetCars(
             [FromQuery] string? keyword = null,
             [FromQuery] string? paymentType = null,
             [FromQuery] decimal? priceFrom = null,
             [FromQuery] decimal? priceTo = null,
             [FromQuery] bool? vatDeduction = null,
             [FromQuery] bool? discountedCars = null,
             [FromQuery] bool? premiumPartners = null,
             [FromQuery] int? registrationFrom = null,
             [FromQuery] int? registrationTo = null,
             [FromQuery] int? mileageFrom = null,
             [FromQuery] int? mileageTo = null,
             [FromQuery] string? transmission = null,
             [FromQuery] string? fuel = null,
             [FromQuery] string? powerUnit = null,
             [FromQuery] double? powerFrom = null,
             [FromQuery] double? powerTo = null,
             [FromQuery] string? vehicleType = null,
             [FromQuery] bool? driveType4x4 = null,
             [FromQuery] string? exteriorColor = null,
             [FromQuery] List<string>? features = null
         )
        {
            var query = _context.CarListings
                .Include(c => c.Model)
                .ThenInclude(m => m.Manufacturer)
                .Include(c => c.Specifications)
                .Include(c => c.CarImages)
                .Include(c => c.CarListingFeatures)
                    .ThenInclude(clf => clf.Feature)
                .Include(c => c.CarServiceHistories)
                .Include(c => c.CarPricingDetails)
                .Include(c => c.CarSales)
                .ThenInclude(s => s.SaleStatus)
                .Include(c => c.Reviews)
                .ThenInclude(r => r.User)
                .AsQueryable();

            // Apply Keyword Filter
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c =>
                    c.Model.Name.Contains(keyword) ||
                    c.Model.Manufacturer.Name.Contains(keyword) ||
                    c.Description.Contains(keyword) ||
                    c.Year.ToString().Contains(keyword)
                );
            }
            if (priceFrom.HasValue)
            {
                query = query.Where(c => c.Price >= priceFrom.Value);
            }
            if (priceTo.HasValue)
            {
                query = query.Where(c => c.Price <= priceTo.Value);
            }
            if (vatDeduction.HasValue && vatDeduction.Value)
            {
                query = query.Where(c => c.CarPricingDetails.Any());
            }
            if (discountedCars.HasValue && discountedCars.Value)
            {
            }
            if (premiumPartners.HasValue && premiumPartners.Value)
            {
            }

            if (registrationFrom.HasValue)
            {
                query = query.Where(c => c.Year >= registrationFrom.Value);
            }
            if (registrationTo.HasValue)
            {
                query = query.Where(c => c.Year <= registrationTo.Value);
            }

            if (mileageFrom.HasValue)
            {
                query = query.Where(c => c.Mileage >= mileageFrom.Value);
            }
            if (mileageTo.HasValue)
            {
                query = query.Where(c => c.Mileage <= mileageTo.Value);
            }
            if (!string.IsNullOrEmpty(transmission))
            {
                query = query.Where(c => c.Specifications.Any(s => s.Transmission == transmission));
            }

            if (!string.IsNullOrEmpty(fuel))
            {
                query = query.Where(c => c.Specifications.Any(s => s.FuelType == fuel));
            }

            


            if (!string.IsNullOrEmpty(vehicleType))
            {
                query = query.Where(c => c.Specifications.Any(s => s.CarType == vehicleType));
            }

            if (driveType4x4.HasValue && driveType4x4.Value)
            {
                query = query.Where(c => c.CarListingFeatures.Any(clf => clf.Feature.Name == "4x4"));
            }

            if (!string.IsNullOrEmpty(exteriorColor))
            {
                query = query.Where(c => c.Specifications.Any(s => s.ExteriorColor == exteriorColor));
            }

            if (features != null && features.Any())
            {
                foreach (var featureName in features)
                {
                    query = query.Where(c => c.CarListingFeatures.Any(clf => clf.Feature.Name == featureName));
                }
            }


            var cars = await query
                .Select(c => new
                {
                    c.ListingId,
                    c.ModelId,
                    c.UserId,
                    c.Year,
                    c.Mileage,
                    c.Price,
                    c.Condition,
                    c.DatePosted,
                    Model = new
                    {
                        c.Model.ModelId,
                        c.Model.Name,
                        Manufacturer = new
                        {
                            c.Model.Manufacturer.ManufacturerId,
                            c.Model.Manufacturer.Name
                        }
                    },
                    Specifications = c.Specifications != null ? c.Specifications.Select(s => new
                    {
                        s.SpecificationId,
                        s.Engine,
                        s.Transmission,
                        s.FuelType,
                        s.SeatingCapacity,
                        s.InteriorColor,
                        s.ExteriorColor,
                        s.CarType,
                    }).ToList() : null,
                    Images = c.CarImages != null ? c.CarImages.Select(i => new
                    {
                        i.ImageId,
                        i.Url,
                        i.Filename
                    }) : null,
                    Features = c.CarListingFeatures != null ? c.CarListingFeatures.Select(f => new
                    {
                        f.Feature.FeatureId,
                        f.Feature.Name
                    }) : null,
                    ServiceHistory = c.CarServiceHistories != null ? c.CarServiceHistories.Select(sh => new
                    {
                        sh.RecentServicing,
                        sh.NoAccidentHistory,
                        sh.Modifications
                    }) : null,
                    Pricing = c.CarPricingDetails != null ? c.CarPricingDetails.Select(
                        shh => new
                        {
                            shh.TaxRate,
                            shh.RegistrationFee,
                        }
                        ).ToList() : null,
                    SalesHistory = c.CarSales != null ? c.CarSales.Select(s => new
                    {
                        s.SaleId,
                        s.FinalPrice,
                        s.SaleDate,
                        s.SaleStatus.StatusName
                    }) : null,
                    Reviews = c.Reviews != null ? c.Reviews.Select(r => new
                    {
                        r.ReviewId,
                        r.UserId,
                        r.Rating,
                        r.User.FullName,
                        r.CreatedAt
                    }) : null
                })
                .ToListAsync();

            return Ok(cars);
        }
        [HttpGet("cars/{id}")]
        public async Task<IActionResult> GetCarDetail(int id)
        {
            var car = await _context.CarListings
                .Include(c => c.Model)
                    .ThenInclude(m => m.Manufacturer)
                .Include(c => c.Specifications)
                .Include(c => c.CarImages)
                .Include(c => c.CarListingFeatures)
                    .ThenInclude(clf => clf.Feature)
                .Include(c => c.CarServiceHistories)
                .Include(c => c.CarPricingDetails)
                .Include(c => c.CarSales)
                    .ThenInclude(clf => clf.SaleStatus)
                .Include(c => c.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.ListingId == id);

            if (car == null)
                return NotFound();

            var carDetail = new
            {
                car.ListingId,
                car.ModelId,
                car.UserId,
                car.Year,
                car.Mileage,
                car.Price,
                car.Condition,
                car.DatePosted,
                Model = new
                {
                    car.Model.ModelId,
                    car.Model.Name,
                    Manufacturer = new
                    {
                        car.Model.Manufacturer.ManufacturerId,
                        car.Model.Manufacturer.Name
                    }
                },
                Specification = car.Specifications != null ? car.Specifications.Select(s => new

                    {
                        s.SpecificationId,
                        s.Engine,
                        s.Transmission,
                        s.FuelType,
                        s.SeatingCapacity,
                        s.InteriorColor,
                        s.ExteriorColor,
                        s.CarType
                    }).ToList() : null,
                Images = car.CarImages != null ? car.CarImages.Select(i => new
                {
                    i.ImageId,
                    i.Url,
                    i.Filename
                }) : null,
                Features = car.CarListingFeatures != null ? car.CarListingFeatures.Select(f => new
                {
                    f.Feature.FeatureId,
                    f.Feature.Name
                }) : null,
                ServiceHistory = car.CarServiceHistories != null ? car.CarServiceHistories.Select(sh => new
                {
                    sh.RecentServicing,
                    sh.NoAccidentHistory,
                    sh.Modifications
                }) : null,
                Pricing = car.CarPricingDetails != null ? car.CarPricingDetails.Select(shh => new
                    {
                        shh.TaxRate,
                        shh.RegistrationFee
                    }).ToList() : null,
                SalesHistory = car.CarSales != null ? car.CarSales.Select(s => new
                {
                    s.SaleId,
                    s.FinalPrice,
                    s.SaleDate,
                    s.SaleStatus.StatusName
                }) : null,
                Reviews = car.Reviews != null ? car.Reviews.Select(r => new
                {
                    r.ReviewId,
                    r.UserId,
                    r.Rating,
                    r.User.FullName,
                    r.CreatedAt
                }) : null
            };

            return Ok(carDetail);
        }

        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CarSale model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            model.CustomerId = userId;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            _context.CarSales.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Order created" });
        }

        [HttpGet("orders")]
<<<<<<< HEAD
        public async Task<IActionResult> GetOrders([FromQuery] int? statusId = null)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var query = _context.CarSales.Where(s => s.CustomerId == userId);
            if (statusId.HasValue)
                query = query.Where(s => s.SaleStatusId == statusId.Value);
            var orders = await query.ToListAsync();
            return Ok(orders);
=======
        public async Task<IActionResult> GetMyOrders(
    [FromQuery] int? statusId = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10) // Thêm phân trang
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "User not authenticated or user ID is invalid." });
                }

                // Bắt đầu câu truy vấn, AsNoTracking() giúp tăng hiệu năng cho các truy vấn chỉ đọc
                var query = _context.CarSales.AsNoTracking();

                // Lọc theo CustomerId (luôn cần)
                query = query.Where(s => s.CustomerId == userId);

                // Lọc theo statusId nếu có
                if (statusId.HasValue)
                {
                    query = query.Where(s => s.SaleStatusId == statusId.Value);
                }

                // Lấy tổng số lượng đơn hàng trước khi phân trang để trả về cho client
                var totalItems = await query.CountAsync();

                var orders = await query
                    .OrderByDescending(cs => cs.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize) // Bỏ qua các trang trước
                    .Take(pageSize) // Lấy số lượng item cho trang hiện tại
                    .Select(cs => new // Định hình dữ liệu (projection) để chỉ lấy các cột cần thiết
                    {
                        // Basic Order Details
                        OrderId = cs.SaleId,
                        cs.OrderNumber,
                        cs.FinalPrice,
                        cs.DepositAmount,
                        cs.RemainingBalance,
                        OrderDate = cs.CreatedAt,
                        cs.DeliveryOption,
                        cs.ExpectedDeliveryDate,
                        cs.ActualDeliveryDate,
                        cs.OrderType,

                        // Sale Status
                        CurrentSaleStatus = cs.SaleStatus.StatusName, // Lấy trực tiếp, không cần logic phức tạp

                        // Car Details
                        CarDetails = cs.StoreListing.CarListing != null ? new
                        {
                            ListingId = cs.StoreListing.CarListing.ListingId,
                            Make = cs.StoreListing.CarListing.Model.CarManufacturer.Name,
                            Model = cs.StoreListing.CarListing.Model.Name,
                            Year = cs.StoreListing.CarListing.Year,
                            Mileage = cs.StoreListing.CarListing.Mileage,
                            Condition = cs.StoreListing.CarListing.Condition,
                            // Lấy thông số kỹ thuật hiệu quả hơn
                            Engine = cs.StoreListing.CarListing.Specifications.Select(spec => spec.Engine).FirstOrDefault(),
                            Transmission = cs.StoreListing.CarListing.Specifications.Select(spec => spec.Transmission).FirstOrDefault(),
                            FuelType = cs.StoreListing.CarListing.Specifications.Select(spec => spec.FuelType).FirstOrDefault(),
                            ImageUrl = cs.StoreListing.CarListing.CarImages.Select(img => img.Url).FirstOrDefault()
                        } : null,

                        // Seller Details
                        SellerDetails = cs.StoreListing.StoreLocation != null ? new
                        {
                            SellerInfo = cs.StoreListing.StoreLocation.Users
                                .Select(u => new
                                {
                                    u.UserId,
                                    u.FullName,
                                    u.Email,
                                    PhoneNumber = u.Mobile
                                }).FirstOrDefault()
                        } : null,

                        // Không cần lấy chi tiết payment/address ở danh sách, chỉ cần ở trang chi tiết
                    })
                    .ToListAsync();

                // Trả về kết quả kèm thông tin phân trang
                return Ok(new
                {
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMyOrders: {ex.Message}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
>>>>>>> 3ece2b4 (Final Backend)
        }


        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
<<<<<<< HEAD
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var order = await _context.CarSales.FirstOrDefaultAsync(s => s.SaleId == id && s.CustomerId == userId);
            return order == null ? NotFound() : Ok(order);
=======
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                // Dùng Select để tạo DTO trực tiếp từ database, hiệu quả hơn dùng nhiều Include
                var orderDetail = await _context.CarSales
                    .AsNoTracking() // Tăng hiệu năng cho truy vấn chỉ đọc
                    .Where(s => s.SaleId == id && s.CustomerId == userId)
                    .Select(cs => new // Projection
                    {
                        cs.SaleId,
                        cs.OrderNumber,
                        CarDetails = new
                        {
                            ListingId = cs.StoreListing.CarListing.ListingId,
                            ModelName = cs.StoreListing.CarListing.Model.Name,
                            ManufacturerName = cs.StoreListing.CarListing.Model.CarManufacturer.Name,
                            Price = cs.StoreListing.CarListing.Price,
                            Year = cs.StoreListing.CarListing.Year,
                            Mileage = cs.StoreListing.CarListing.Mileage,
                            Condition = cs.StoreListing.CarListing.Condition,
                            Vin = cs.StoreListing.CarListing.Vin,
                            // SỬA LỖI: Thêm các trường còn thiếu từ Specifications
                            Engine = cs.StoreListing.CarListing.Specifications.Select(s => s.Engine).FirstOrDefault(),
                            Transmission = cs.StoreListing.CarListing.Specifications.Select(s => s.Transmission).FirstOrDefault(),
                            FuelType = cs.StoreListing.CarListing.Specifications.Select(s => s.FuelType).FirstOrDefault(),
                            // Tối ưu: Lấy danh sách ảnh và video trực tiếp trong câu truy vấn chính
                            ImageUrls = cs.StoreListing.CarListing.CarImages.Select(ci => ci.Url).ToList(),
                            VideoUrls = cs.StoreListing.CarListing.CarVideos.Select(cv => cv.Url).ToList()
                        },
                        cs.FinalPrice,
                        cs.DepositAmount,
                        cs.RemainingBalance,
                        Status = cs.SaleStatus.StatusName,
                        cs.DeliveryOption,
                        ShippingAddress = cs.ShippingAddress != null ? new
                        {
                            cs.ShippingAddress.AddressId,
                            cs.ShippingAddress.Address,
                            cs.ShippingAddress.RecipientName,
                            cs.ShippingAddress.RecipientPhone,
                            cs.ShippingAddress.AddressType
                        } : null,
                        PickupLocation = cs.PickupStoreLocation != null ? new
                        {
                            cs.PickupStoreLocation.StoreLocationId,
                            cs.PickupStoreLocation.Name,
                            cs.PickupStoreLocation.Address,
                        } : null,
                        cs.ExpectedDeliveryDate,
                        cs.ActualDeliveryDate,
                        DepositPayment = cs.DepositPayment != null ? new
                        {
                            cs.DepositPayment.PaymentId,
                            cs.DepositPayment.Amount,
                            cs.DepositPayment.PaymentMethod,
                            cs.DepositPayment.PaymentStatus,
                            cs.DepositPayment.DateOfPayment
                        } : null,
                        FullPayment = cs.FullPayment != null ? new
                        {
                            cs.FullPayment.PaymentId,
                            cs.FullPayment.Amount,
                            cs.FullPayment.PaymentMethod,
                            cs.FullPayment.PaymentStatus,
                            cs.FullPayment.DateOfPayment
                        } : null,
                        StatusHistory = cs.StatusHistory
                                        .OrderBy(sh => sh.Timestamp)
                                        .Select(sh => new {
                                            Id = sh.SaleStatusId,
                                            Name = sh.SaleStatus.StatusName,
                                            Date = sh.Timestamp,
                                            Notes = sh.Notes
                                        }).ToList(),
                        cs.OrderType,
                        cs.CreatedAt,
                        cs.UpdatedAt,
                        SellerDetails = cs.StoreListing.StoreLocation != null ? new
                        {
                            SellerInfo = cs.StoreListing.StoreLocation.Users
                                .Select(u => new
                                {
                                    u.UserId,
                                    u.FullName,
                                    u.Email,
                                    PhoneNumber = u.Mobile
                                }).FirstOrDefault()
                        } : null,
                    })
                    .FirstOrDefaultAsync();

                if (orderDetail == null)
                {
                    return NotFound(new { message = "Order not found or you do not have permission to view it." });
                }

                return Ok(orderDetail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetOrderDetail: {ex.Message}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
>>>>>>> 3ece2b4 (Final Backend)
        }

        [HttpPut("orders/{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var sale = await _context.CarSales.FirstOrDefaultAsync(s => s.SaleId == id && s.CustomerId == userId);
            if (sale == null) return NotFound();
            sale.SaleStatusId = 3;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Order cancelled" });
        }

        [HttpPost("reviews")]
        public async Task<IActionResult> AddReview([FromBody] Review model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            model.UserId = userId;
            model.CreatedAt = DateTime.UtcNow;
            _context.Reviews.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Review added" });
        }

        [HttpGet("blogs")]
        public async Task<IActionResult> GetBlogs()
        {
            var blogs = await _context.BlogPosts.ToListAsync();
            return Ok(blogs);
        }

        [HttpGet("blogs/{id}")]
        public async Task<IActionResult> GetBlogDetail(int id)
        {
            var blog = await _context.BlogPosts.FindAsync(id);
            return blog == null ? NotFound() : Ok(blog);
        }

<<<<<<< HEAD
        [HttpGet("chats")]
        public IActionResult GetChats() => Ok(new { message = "Chat list (implement as needed)" });

        [HttpGet("chats/{id}")]
        public IActionResult GetChatDetail(int id) => Ok(new { message = "Chat detail (implement as needed)" });

        [HttpPost("chats/{id}/send")]
        public IActionResult SendChat(int id, [FromBody] string message) => Ok(new { message = "Message sent (implement as needed)" });

        // 13. Xem khuyến mãi
        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotions()
        {
            var promotions = await _context.Promotions.ToListAsync();
            return Ok(promotions);
=======
        [HttpGet("test-drives")]
        public async Task<IActionResult> GetTestDriveBookings()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var bookings = await _context.TestDriveBookings
                .Where(b => b.UserId == userId)
                .Include(b => b.StoreListing)
                    .ThenInclude(sl => sl.CarListing)
                        .ThenInclude(cl => cl.Model)
                            .ThenInclude(m => m.CarManufacturer)
                .Include(b => b.StoreListing)
                    .ThenInclude(sl => sl.StoreLocation)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var bookingDtos = bookings.Select(b => new
            {
                b.BookingId,
                b.BookingDate,
                b.Status,
                Car = new
                {
                    b.StoreListing.CarListing.ListingId,
                    // FIX: Explicitly name the properties to avoid conflict
                    ManufacturerName = b.StoreListing.CarListing.Model.CarManufacturer.Name,
                    ModelName = b.StoreListing.CarListing.Model.Name,
                    b.StoreListing.CarListing.Year,
                },
                Showroom = new
                {
                    ShowroomName = b.StoreListing.StoreLocation.Name,
                    b.StoreListing.StoreLocation.Address
                }
            });

            return Ok(bookingDtos);
        }

        [HttpGet("test-drives/{id}")]
        public async Task<IActionResult> GetTestDriveBookingDetail(int id)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var booking = await _context.TestDriveBookings
                .Include(b => b.StoreListing)
                    .ThenInclude(sl => sl.CarListing)
                        .ThenInclude(cl => cl.Model)
                            .ThenInclude(m => m.CarManufacturer)
                .Include(b => b.StoreListing)
                    .ThenInclude(sl => sl.StoreLocation)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            var bookingDetail = new
            {
                booking.BookingId,
                booking.BookingDate,
                booking.Status,
                booking.HasLicense,
                booking.Notes,
                Car = new
                {
                    booking.StoreListing.CarListing.ListingId,
                    // FIX: Explicitly name the properties to avoid conflict
                    ManufacturerName = booking.StoreListing.CarListing.Model.CarManufacturer.Name,
                    ModelName = booking.StoreListing.CarListing.Model.Name,
                    booking.StoreListing.CarListing.Year,
                    booking.StoreListing.CarListing.Price,
                    booking.StoreListing.CarListing.Mileage,
                    booking.StoreListing.CarListing.Condition,
                },
                Showroom = new
                {
                    // FIX: Explicitly name the property for clarity
                    ShowroomName = booking.StoreListing.StoreLocation.Name,
                    booking.StoreListing.StoreLocation.Address
                }
            };

            return Ok(bookingDetail);
        }


        [HttpPost("test-drive")]
        public async Task<IActionResult> CreateTestDriveBooking([FromBody] TestDriveBookingDto bookingDto)
        {
            // === VALIDATE DỮ LIỆU ĐẦU VÀO ===
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy UserId từ token (bạn cần có logic này)
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Kiểm tra xem xe có tồn tại không
            var storeListing = await _context.StoreListings
                .FirstOrDefaultAsync(sl => sl.StoreListingId == bookingDto.StoreListingId && !sl.IsCurrent);

            if (storeListing == null)
            {
                return NotFound("The selected car is not available for a test drive.");
            }

            var newBooking = new TestDriveBooking
            {
                UserId = userId,
                StoreListingId = bookingDto.StoreListingId,
                BookingDate = bookingDto.BookingDate,
                HasLicense = bookingDto.HasLicense,
                Notes = bookingDto.Notes,
                Status = "Pending"
            };

            _context.TestDriveBookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Test drive booked successfully! We will contact you shortly to confirm." });
        }

        [HttpPut("test-drives/{id}/cancel")]
        public async Task<IActionResult> CancelTestDriveBooking(int id)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var booking = await _context.TestDriveBookings
                .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            if (booking.Status != "Pending")
            {
                return BadRequest("Only pending bookings can be canceled.");
            }

            booking.Status = "Canceled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Test drive booking has been canceled." });
        }
        [HttpPost("financing")]
        public async Task<IActionResult> SubmitFinancingApplication([FromBody] FinancingApplicationDto applicationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // You can also get the logged-in user's ID here for linking the application
            // var userId = GetUserId(); 

            // In a real application, you would:
            // 1. Save this applicationDto data to a new `FinancingApplications` table in your database.
            // 2. Potentially integrate with a third-party service or notify staff.
            // 3. Generate a more formal PDF document.

            // For now, we will generate a simple text-based contract.
            string contractText = GenerateLoanContract(applicationDto);
            try
            {
                await _emailService.SendEmailAsync("anhtuyettranthi1988@gmail.com", "LOAN AGREEMENT AUTOSALEDN", contractText);
                Console.WriteLine($"Email sent successfully to: anhtuyettranthi1988@gmail.com");
            }
            catch (Exception emailEx)
            {
                Console.WriteLine($"Failed to send email to anhtuyettranthi1988@gmail.com: {emailEx.Message}");
            }

            // You could save this contract to the database or return it directly.
            return Ok(new
            {
                message = "Financing application submitted successfully!",
                contract = contractText
            });
>>>>>>> 0e4a76f (Final Backend)
        }
        private string GenerateLoanContract(FinancingApplicationDto dto)
        {
            return $@"
    <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 800px; margin: 0 auto; padding: 20px; border: 1px solid #ccc; border-radius: 8px;'>
        <h2 style='text-align:center; text-transform: uppercase; margin-bottom: 20px;'>Loan Agreement</h2>

        <p style='text-align:right;'><strong>Date:</strong> {DateTime.UtcNow:dd/MM/yyyy}</p>

        <h3 style='margin-top:30px; color:#2c3e50;'>Lender:</h3>
        <p>{dto.PartnerName}</p>

        <h3 style='margin-top:20px; color:#2c3e50;'>Borrower:</h3>
        <p><strong>{dto.FullName}</strong></p>

        <h3 style='margin-top:30px; color:#2c3e50;'>Borrower Details</h3>
        <table style='width:100%; border-collapse: collapse; margin-top:10px;'>
            <tr>
                <td style='padding:6px; border:1px solid #ddd;'>Date of Birth</td>
                <td style='padding:6px; border:1px solid #ddd;'>{dto.DateOfBirth:dd/MM/yyyy}</td>
            </tr>
            <tr>
                <td style='padding:6px; border:1px solid #ddd;'>Address</td>
                <td style='padding:6px; border:1px solid #ddd;'>{dto.Address}</td>
            </tr>
            <tr>
                <td style='padding:6px; border:1px solid #ddd;'>Email</td>
                <td style='padding:6px; border:1px solid #ddd;'>{dto.Email}</td>
            </tr>
            <tr>
                <td style='padding:6px; border:1px solid #ddd;'>Phone</td>
                <td style='padding:6px; border:1px solid #ddd;'>{dto.PhoneNumber}</td>
            </tr>
        </table>

        <h3 style='margin-top:30px; color:#2c3e50;'>Loan Terms</h3>
        <table style='width:100%; border-collapse: collapse; margin-top:10px;'>
            <tr>
                <td style='padding:6px; border:1px solid #ddd;'>Principal Loan Amount</td>
                <td style='padding:6px; border:1px solid #ddd;'>{dto.LoanAmount:C}</td>
            </tr>
            <tr>
                <td style='padding:6px; border:1px solid #ddd;'>Annual Interest Rate</td>
                <td style='padding:6px; border:1px solid #ddd;'>{dto.InterestRate}%</td>
            </tr>
            <tr>
                <td style='padding:6px; border:1px solid #ddd;'>Loan Term</td>
                <td style='padding:6px; border:1px solid #ddd;'>{dto.PaybackPeriodMonths} months</td>
            </tr>
        </table>

        <p style='margin-top:30px;'>
            This document confirms the submission of a loan application. 
            The lender, <strong>{dto.PartnerName}</strong>, will review this application 
            and contact the borrower, <strong>{dto.FullName}</strong>, regarding the final decision. 
            This is <u>not a guaranteed approval</u> of the loan.
        </p>

        <div style='margin-top:50px; text-align:right;'>
            <p>Signed (Electronically),</p>
            <p style='font-weight:bold; margin-top:20px;'>{dto.FullName}</p>
        </div>
    </div>
    ";
        }

        public class FinancingApplicationDto
        {
            [Required]
            public string FullName { get; set; }

            [Required]
            [Phone]
            public string PhoneNumber { get; set; }

            [Required]
            public DateTime DateOfBirth { get; set; }

            [Required]
            public string Address { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            // Information about the loan itself
            [Required]
            public int CarListingId { get; set; }

            [Required]
            public string PartnerName { get; set; } // e.g., "HSBC Bank"

            [Required]
            public decimal LoanAmount { get; set; }

            [Required]
            public decimal InterestRate { get; set; }

            [Required]
            public int PaybackPeriodMonths { get; set; }
        }
        public class TestDriveBookingDto
        {
            [Required]
            public int StoreListingId { get; set; }
            [Required]
            public DateTime BookingDate { get; set; }
            [Required]
            public bool HasLicense { get; set; }
            public string? Notes { get; set; }
        }

    }
}
