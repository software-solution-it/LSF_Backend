using System;

public class Purchase
{
    public string Id { get; set; }
    public long CreationDate { get; set; }
    public string Event { get; set; }
    public string Version { get; set; }
    public DataModel Data { get; set; }
}

public class DataModel
{
    public ProductModel Product { get; set; }
    public AffiliateModel[] Affiliates { get; set; }
    public BuyerModel Buyer { get; set; }
    public ProducerModel Producer { get; set; }
    public CommissionModel[] Commissions { get; set; }
    public PurchaseModel Purchase { get; set; }
    public SubscriptionModel Subscription { get; set; }
}

public class ProductModel
{
    public int Id { get; set; }
    public string Ucode { get; set; }
    public string Name { get; set; }
    public bool HasCoProduction { get; set; }
}

public class AffiliateModel
{
    public string AffiliateCode { get; set; }
    public string Name { get; set; }
}

public class BuyerModel
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string CheckoutPhone { get; set; }
    public AddressModel Address { get; set; }
}

public class AddressModel
{
    public string Country { get; set; }
    public string CountryIso { get; set; }
}

public class ProducerModel
{
    public string Name { get; set; }
}

public class CommissionModel
{
    public double Value { get; set; }
    public string Source { get; set; }
    public string CurrencyValue { get; set; }
}

public class PurchaseModel
{
    public long ApprovedDate { get; set; }
    public PriceModel FullPrice { get; set; }
    public PriceModel Price { get; set; }
    public AddressModel CheckoutCountry { get; set; }
    public OrderBumpModel OrderBump { get; set; }
    public PriceModel OriginalOfferPrice { get; set; }
    public long OrderDate { get; set; }
    public string Status { get; set; }
    public string Transaction { get; set; }
    public PaymentModel Payment { get; set; }
    public OfferModel Offer { get; set; }
    public string SckPaymentLink { get; set; }
}

public class PriceModel
{
    public double Value { get; set; }
    public string CurrencyValue { get; set; }
}

public class OrderBumpModel
{
    public bool IsOrderBump { get; set; }
    public string ParentPurchaseTransaction { get; set; }
}

public class PaymentModel
{
    public int InstallmentsNumber { get; set; }
    public string Type { get; set; }
}

public class OfferModel
{
    public string Code { get; set; }
}

public class SubscriptionModel
{
    public string Status { get; set; }
    public PlanModel Plan { get; set; }
    public SubscriberModel Subscriber { get; set; }
}

public class PlanModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class SubscriberModel
{
    public string Code { get; set; }
}
