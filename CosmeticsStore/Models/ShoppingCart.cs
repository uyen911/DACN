﻿using Antlr.Runtime.Tree;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Services.Description;

namespace CosmeticsStore.Models
{
    public class ShoppingCart
    {
        public List<ShoppingCartItem> Items { get; set; }
        private ApplicationDbContext db = new ApplicationDbContext();
        public ShoppingCart()
        {
            this.Items= new List<ShoppingCartItem>();
        }
        public bool CheckQuantityAddtoCart(int productQuantity, ShoppingCartItem item, int Quantity)
        {
            var checkExits = Items.FirstOrDefault(x => x.ProductId == item.ProductId);
            if (checkExits != null)
            {
                if ((checkExits.Quantity + Quantity) > productQuantity)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            else
            {
                if (Quantity > productQuantity)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }
        public void AddToCart(ShoppingCartItem item, int Quantity)
        {
            var checkExits= Items.FirstOrDefault(x=>x.ProductId==item.ProductId);
            if (checkExits!=null)
            {
                checkExits.Quantity += Quantity;
                checkExits.TotalPrice = checkExits.Price * checkExits.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }
        public void Remove (int id)
        {
            var checkExits = Items.SingleOrDefault(x => x.ProductId == id);
            if (checkExits!=null)
            {
                Items.Remove(checkExits);
            }
        }
        public void UpdateQuantity(int id, int quantity)
        {
            var checkExits = Items.SingleOrDefault(x=>x.ProductId == id);
            if(quantity <= 0)
            {

                Remove(id);
            }    
            if(checkExits!=null)
            {
                checkExits.Quantity = quantity;
                checkExits.TotalPrice = checkExits.Price * checkExits.Quantity;
            }
        }
        public decimal GetTotalPrice()
        {
            return Items.Sum(x => x.TotalPrice);
        }
        public int GetTotalQuntity()
        {
            return Items.Sum(x => x.Quantity);
        }
        public void ClearCart()
        {
            Items.Clear();
        }

    }

    public class ShoppingCartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Alias { get; set; }
        public string CategoryName { get; set; }
        public string ProductImg { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }

    }
}