using System;

namespace PurchaseObjects
{
    public class PurchaseObject
    {
        public string? Id { get; set; }
        public long? CreationDate { get; set; }
        public string? Event { get; set; }
        public string? Version { get; set; }
        public Data? Data { get; set; }
        public string? HotTok { get; set; }
    }

    public class Data
    {
        public Product? Product { get; set; }
        public Affiliate[]? Affiliates { get; set; }
        public Buyer? Buyer { get; set; }
        public Producer? Producer { get; set; }
        public Commission[]? Commissions { get; set; }
        public Purchase? Purchase { get; set; }
    }

    public class Product
    {
        public int? Id { get; set; }
        public string? Ucode { get; set; }
        public string? Name { get; set; }
        public bool? HasCoProduction { get; set; }
    }

    public class Affiliate
    {
        public string? AffiliateCode { get; set; }
        public string? Name { get; set; }
    }

    public class Buyer
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? CheckoutPhone { get; set; }
        public Address? Address { get; set; }
        public string? Document { get; set; }
    }

    public class Address
    {
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? CountryIso { get; set; }
        public string? State { get; set; }
        public string? Neighborhood { get; set; }
        public string? Zipcode { get; set; }
        public string? FullAddress { get; set; }
        public string? Number { get; set; }
    }

    public class Producer
    {
        public string? Name { get; set; }
    }

    public class Commission
    {
        public double? Value { get; set; }
        public string? Source { get; set; }
        public string? CurrencyValue { get; set; }
    }

    public class Purchase
    {
        public long? ApprovedDate { get; set; }
        public Price? FullPrice { get; set; }
        public Price? Price { get; set; }
        public CheckoutCountry? CheckoutCountry { get; set; }
        public OrderBump? OrderBump { get; set; }
        public string? BuyerIp { get; set; }
        public Price? OriginalOfferPrice { get; set; }
        public long? OrderDate { get; set; }
        public string? Status { get; set; }
        public string? Transaction { get; set; }
        public Payment? Payment { get; set; }
        public Offer? Offer { get; set; }
        public string? InvoiceBy { get; set; }
        public bool? SubscriptionAnticipationPurchase { get; set; }
    }

    public class Price
    {
        public double? Value { get; set; }
        public string? CurrencyValue { get; set; }
    }

    public class CheckoutCountry
    {
        public string? Name { get; set; }
        public string? Iso { get; set; }
    }

    public class OrderBump
    {
        public bool? IsOrderBump { get; set; }
    }

    public class Payment
    {
        public int? InstallmentsNumber { get; set; }
        public string? Type { get; set; }
    }

    public class Offer
    {
        public string? Code { get; set; }
    }
}
