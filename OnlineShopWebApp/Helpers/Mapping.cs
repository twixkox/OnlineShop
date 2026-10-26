using OnlineShop.Db.Models;
using OnlineShopWebApp.Areas.Client.Models;
using OnlineShopWebApp.Models;

namespace OnlineShopWebApp.Helpers
{
    public static class Mapping
    {
        #region Category
        public static CategoryViewModel ToCategoryViewModel(this Category category)
        {
            if (category == null) { return null; }

            else
            {
                var existingCategory = new CategoryViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IdentityUrl = category.IdentityUrl,
                    PhotoPath = category.PhotoPath,
                };
                return existingCategory;
            }
        }

        public static Category ToCategory(this CategoryViewModel categoryViewModel)
        {
            if (categoryViewModel == null) { return null; }
            else
            {
                var existingCategory = new Category
                {
                    Name = categoryViewModel.Name,
                    Description = categoryViewModel.Description,
                    Id = categoryViewModel.Id,
                };
                return existingCategory;
            }
        }

        public static List<Category> ToListCategory(this List<CategoryViewModel> categoryViewModels)
        {
            if (categoryViewModels == null) { return new List<Category>(); }
            else
            {
                var existingCategories = new List<Category>();

                foreach (var categoryViewModel in categoryViewModels)
                {

                    existingCategories.Add(ToCategory(categoryViewModel));
                }
                return existingCategories;
            }
        }

        public static List<CategoryViewModel> ToListCategoryViewModels(this List<Category> categories)
        {
            if (categories == null) { return new List<CategoryViewModel>(); }
            else
            {
                var existingCageroriesViewModels = new List<CategoryViewModel>();
                foreach (var category in categories)
                {
                    existingCageroriesViewModels.Add(ToCategoryViewModel(category));
                }
                return existingCageroriesViewModels;
            }
        }

        #endregion
        #region User
        public static EditUserInfoViewModel ToEditUserInfoViewModel(this User user)
        {
            if (user == null) { return null; }
            else
            {
                var existingUser = new EditUserInfoViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Phone = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                };
                return existingUser;
            }
        }

        public static UserViewModel ToUserViewModel(this User user)
        {
            if (user == null) { return null; }

            else
            {
                var existingUser = new UserViewModel
                {
                    Phone = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.Email,
                    Password = user.PasswordHash,
                    Id = user.Id,
                    CreationDateTime = user.CreationDateTime,

                };
                return existingUser;
            }
        }
        public static List<UserViewModel> ToListUserViewModels(this List<User> user)
        {
            if (user == null) { return null; }

            else
            {
                var result = new List<UserViewModel>();
                foreach (var item in user)
                {
                    var existingUser = new UserViewModel
                    {
                        Id = item.Id,
                        Phone = item.PhoneNumber,

                        UserName = item.Email,
                        FirstName = item.FirstName,

                        Password = item.PasswordHash

                    };
                    result.Add(existingUser);
                }
                return result;
            }
        }



        #endregion
        #region Product
        public static ProductViewModel ToProductViewModel(this Product product)
        {
            return new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Cost = product.Cost,
                PhotoPath = product.PhotoPath,
                ThumbnailsPhotoPath = product.ThumbnailPath!,
                CategoryId = product.CategoryId,
                CurrentCategoryName = product.CategoryName,
                Stock = product.Stock,
                Height = product.Height,
                FrostZone = product.FrostZone,
                Light = product.Light,
                Watering = product.Watering,
                FloweringPeriod = product.FloweringPeriod,
            };
        }

        public static Product ToProduct(this ProductViewModel productViewModel)
        {
            return new Product
            {
                Id = productViewModel.Id,
                Name = productViewModel.Name,
                Description = productViewModel.Description,
                Cost = productViewModel.Cost,
                PhotoPath = productViewModel.PhotoPath,
                ThumbnailPath = productViewModel.ThumbnailsPhotoPath,
                CategoryId = productViewModel.CategoryId,
                CategoryName = productViewModel.CurrentCategoryName,
                Stock = productViewModel.Stock,
                Height = productViewModel.Height,
                FrostZone = productViewModel.FrostZone,
                Light = productViewModel.Light,
                Watering = productViewModel.Watering,
                FloweringPeriod = productViewModel.FloweringPeriod,
            };
        }

        public static List<ProductViewModel> ToProductsViewModels(this List<Product> products)
        {
            var existingProducts = new List<ProductViewModel>();

            foreach (var product in products)
            {
                existingProducts.Add(ToProductViewModel(product));
            }
            return existingProducts;
        }

        /// <summary>
        /// Category.Name у товара не хранится в БД ([NotMapped]), поэтому подставляем названия категорий для карточек.
        /// </summary>
        public static List<ProductViewModel> WithRatings(this List<ProductViewModel> products, Dictionary<Guid, (double Average, int Count)> ratings)
        {
            foreach (var product in products)
            {
                if (ratings.TryGetValue(product.Id, out var r))
                {
                    product.Rating = r.Average;
                    product.ReviewsCount = r.Count;
                }
            }
            return products;
        }

        public static List<ProductViewModel> WithCategoryNames(this List<ProductViewModel> products, IEnumerable<Category> categories)
        {
            var names = categories.ToDictionary(c => c.Id, c => c.Name);
            foreach (var product in products)
            {
                if (names.TryGetValue(product.CategoryId, out var name)) product.CurrentCategoryName = name;
            }
            return products;
        }
        #endregion

        #region Cart
        public static List<CartItemViewModel> ToCartViewModels(this List<CartItem> cartDbItems)
        {
            var cartItems = new List<CartItemViewModel>();

            foreach (var cartDbItem in cartDbItems)
            {
                var cartItem = new CartItemViewModel
                {
                    Id = cartDbItem.Id,
                    Quantity = cartDbItem.Quantity,
                    Product = ToProductViewModel(cartDbItem.Product)
                };
                cartItems.Add(cartItem);
            }
            return cartItems;
        }

        public static CartViewModel ToCartViewModel(this Cart cart)
        {
            if (cart == null) return null;

            return new CartViewModel
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = ToCartViewModels(cart.Items),
                PromoCode = cart.PromoCode,
                Discount = cart.Discount,
            };
        }

        public static List<CartItem> ToCartItems(this List<CartItemViewModel> cartItems)
        {
            if (cartItems == null) return null;

            var items = new List<CartItem>();

            foreach (var cartItem in cartItems)
            {
                var existingCartItem = new CartItem
                {
                    Id = cartItem.Id,
                    Product = ToProduct(cartItem.Product),
                };
                items.Add(existingCartItem);
            }
            return items;
        }
        #endregion

        #region Favorite
        public static FavoriteViewModel ToFavoriteViewModel(this Favorite favorite)
        {
            if (favorite == null) return null;

            return new FavoriteViewModel
            {
                UserId = favorite.UserId,
                Items = favorite.Products
            };
        }
        #endregion

        #region Order
        public static DeliveryUserInfoViewModel ToDeliveryUserInfoViewModel(this DeliveryUserInfo deliveryUserInfo)
        {
            if (deliveryUserInfo == null) return null;

            return new DeliveryUserInfoViewModel
            {
                Adress = deliveryUserInfo.Adress,
                Apartment = deliveryUserInfo.Apartment,
                Phone = deliveryUserInfo.Phone,
                UserName = deliveryUserInfo.UserName,
                DeliveryDate = deliveryUserInfo.DeliveryDate,
                Comment = deliveryUserInfo.Comment,
                Id = deliveryUserInfo.Id,
            };
        }

        public static DeliveryUserInfo ToDeliveryUserInfo(this DeliveryUserInfoViewModel deliveryUserInfo)
        {
            if (deliveryUserInfo == null) return null;

            return new DeliveryUserInfo
            {
                Adress = deliveryUserInfo.Adress,
                Apartment = deliveryUserInfo.Apartment,
                Phone = deliveryUserInfo.Phone,
                UserName = deliveryUserInfo.UserName,
                DeliveryDate = deliveryUserInfo.DeliveryDate,
                Comment = deliveryUserInfo.Comment,
                Id = deliveryUserInfo.Id,
            };
        }

        public static OrderViewModel ToOrderViewModel(this Order order)
        {
            if (order == null) return null;

            var viewModel = new OrderViewModel
            {
                Id = order.Id,
                UserId = order.UserId,
                DeliveryUserInfo = ToDeliveryUserInfoViewModel(order.DeliveryUserInfo),

                CreationDateOrder = order.CreationDateOrder,
                Status = (OrderStatusViewModel)(int)order.Status,
                Items = ToCartViewModels(order.Items),
                PromoCode = order.PromoCode,
                DiscountPercent = order.DiscountPercent,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
            };
            return viewModel;
        }


        public static List<OrderViewModel> ToOrdersViewModels(this List<Order> DbOrders)
        {
            if (DbOrders == null) return null;

            var viewModels = new List<OrderViewModel>();

            foreach (var order in DbOrders)
            {
                var viewModel = order.ToOrderViewModel();
                viewModels.Add(viewModel);
            }
            return viewModels;
        }

        public static Order ToOrder(this OrderViewModel order)
        {
            if (order == null) return null;

            var existingOrder = new Order
            {

                UserId = order.UserId,
                DeliveryUserInfo = ToDeliveryUserInfo(order.DeliveryUserInfo),
                CreationDateOrder = order.CreationDateOrder,
                Items = ToCartItems(order.Items),
            };
            return existingOrder;
        }
        #endregion
    }
}
