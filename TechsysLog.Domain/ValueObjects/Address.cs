namespace TechsysLog.Domain.ValueObjects;

/// <summary>
/// Represents a value object for an address, encapsulating all relevant properties.
/// 
/// Design decision: Address is modeled as a Value Object rather than an Entity
/// because it has no identity of its own — it only exists as part of an Order.
/// Immutability is enforced by init-only properties, ensuring the address
/// cannot be partially mutated after creation.
/// </summary>
public sealed class Address
{
    public string ZipCode { get; init;}
    public string Street { get; init;}
    public string Number { get; init;}
    public string Neighborhood { get; init;}
    public string City { get; init;}
    public string State { get; set; } = null!;

    public Address(string zipCode, string street, string number, string neighborhood, string city, string state)
    {
        ZipCode = zipCode;
        Street = street;
        Number = number;
        Neighborhood = neighborhood;
        City = city;
        State = state;
    }

}