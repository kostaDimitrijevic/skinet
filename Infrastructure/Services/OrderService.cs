using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;

namespace Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
        public OrderService(IUnitOfWork unitOfWork, IBasketRepository basketRepo)
        {
            _unitOfWork = unitOfWork;
            _basketRepo = basketRepo;
        }

        public async Task<Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress)
        {
            // get basket from the repo
            var basket = await _basketRepo.GetBasketAsync(basketId);
            // get items from the product repo
            var items = new List<OrderItem>();
            foreach(var item in basket.Items)
            {
                var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                var itemOrdered = new ProductItemOrdered(productItem.Id, productItem.Name, productItem.PictureUrl);
                var orderItem = new OrderItem(itemOrdered, productItem.Price, item.Quantity);
                items.Add(orderItem);
            }
            // get delivery method from repo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);
            // calc subtotal
            var subtotal = items.Sum(item => item.Price * item.Quantity);
            // create order
            var order = new Order(items, buyerEmail, shippingAddress, deliveryMethod, subtotal);
            //TODO: save to db, zasto  prosledjujemo cuvenje u bazu negde drugde: 
            //Posto ovde cuvamo jedan entitet u bazu okej je, ali da imamo inventar 
            //i korisnik moze da napravi product i brand u isto vreme to znaci da cuvamo dva entiteta u dve razlicite tabele
            // a ako koristimo isti genericki repozitorijum to bi znacilo da cemo ovde napraviti dve razlicite instance StoreContexta sto je lose
            // Ovo objasnjenje mi je sumnjivo ja pre mislim da zeli da kaze ako bih cuvao nesto u orderRepo i productRepo imao bih dva StoreContexta
            //Problem su parcijalni update-i
            _unitOfWork.Repository<Order>().Add(order);
            var result = await _unitOfWork.Complete();


            if(result <= 0) return null;
        
            await _basketRepo.DeleteBasketAsync(basketId); 

            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            return await _unitOfWork.Repository<DeliveryMethod>().ListAllAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id, string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(id, buyerEmail);

            return await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);

            return await _unitOfWork.Repository<Order>().ListAsync(spec);
        }
    }
}