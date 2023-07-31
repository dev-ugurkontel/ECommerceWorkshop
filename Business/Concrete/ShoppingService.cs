﻿using Business.Abstract;
using Core.Utils;
using DataAccess.EF.Abstract;
using DataAccess.EF.Concrete;
using Entities.Surrogate.Request;
using Entities.Surrogate.Response;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Business.Concrete
{
    public class ShoppingService : IShoppingService
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly CartItemRepositoryBase _cartItemRepository;

        public ShoppingService(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        public IDataResult<CartResponse> AddToCart(int userId, CartItemRequest cartItemRequest)
        {
            var product = _productService.Get(cartItemRequest.ProductId).Data;

            CartRequest cartRequest = new()
            {
                UserId = userId,
                TotalItemQuantity = cartItemRequest.ItemQuantity,
                TotalItemPrice = cartItemRequest.ItemQuantity * product.ProductPrice,
                CartItems = new List<CartItemRequest>() { cartItemRequest }
            };

            var cartResult = _cartService.Add(cartRequest);
            if (cartResult.Status == 0)
            {
                return new ErrorDataResult<CartResponse>(default, "Sepet oluşturulamadı.");
            }

            ProductRequest productRequest = new()
            {
                ProductStock = product.ProductStock -= cartItemRequest.ItemQuantity,
                ProductCategoryId = product.Category.CategoryId,
                ProductCampaignId = product.Campaign.CampaignId,
                ProductDescription = product.ProductDescription,
                ProductPrice = product.ProductPrice,
                ProductStatus = product.ProductStatus,
                ProductImagePath = product.ProductImagePath,
                ProductName = product.ProductName
            };

            _productService.Update(product.ProductId, productRequest);

            CartResponse cartResponse = new()
            {
                CartId = cartResult.Data.CartId,
                UserId = cartResult.Data.UserId,
                TotalItemQuantity = cartResult.Data.TotalItemQuantity,
                TotalItemPrice = cartResult.Data.TotalItemPrice,
                CreateDate = cartResult.Data.CreateDate,
                EditDate = cartResult.Data.EditDate,
                CartItems = cartResult.Data.CartItems
            };

            return new SuccessDataResult<CartResponse>(cartResponse, "Ürün sepete eklendi.");
        }

        public IDataResult<CartResponse> UpdateCart(int userId, CartItemRequest cartItemRequest)
        {
            throw new NotImplementedException();
        }

        public IResult RemoveFromCart(int cartItemId)
        {
            var getCartItem = _cartItemRepository.Get(cartItemId);
            var getCart = _cartService.Get(cartItemId).Data;
            _cartService.DeleteCartItem(cartItemId);

            foreach (var item in getCart.CartItems)
            {
                var product = _productService.Get(item.ProductId);
                ProductRequest productRequest = new()
                {
                    ProductStock = product.Data.ProductStock += 1,
                    ProductCategoryId = product.Data.Category.CategoryId,
                    ProductCampaignId = product.Data.Campaign.CampaignId,
                    ProductDescription = product.Data.ProductDescription,
                    ProductPrice = product.Data.ProductPrice,
                    ProductStatus = product.Data.ProductStatus,
                    ProductImagePath = product.Data.ProductImagePath,
                    ProductName = product.Data.ProductName

                };
                _productService.Update(item.ProductId, productRequest);
            }

            
            //TODO: Silme işleminden sonra ürün stounu arttırma işlemi yapılacak.
   

            return new SuccessResult("Ürün sepetten silindi.");
        }

        public IResult ClearCart(int cartId)
        {
            throw new NotImplementedException();
        }
    }
}
