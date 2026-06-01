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
    public int Number { get; init;}
    public string Neighborhood { get; init;}
    public string City { get; init;}
    public string State { get; set; } = null!;

    public Address(string zipCode, string street, int number, string neighborhood, string city, string state)
    {
        ZipCode = zipCode;
        Street = street;
        Number = number;
        Neighborhood = neighborhood;
        City = city;
        State = state;
    }

    /// <summary>
    /// Equality is based on data, not reference.
    /// Two addresses with the same fields are the same address.
    /// <param name="obj">The object to compare with the current address.</param>
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Address other)
            return false;

        return ZipCode == other.ZipCode &&
               Street == other.Street &&
               Number == other.Number &&
               Neighborhood == other.Neighborhood &&
               City == other.City &&
               State == other.State;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ZipCode, Street, Number, Neighborhood, City, State);
    }

    public static bool operator ==(Address? left, Address? right) => Equals(left, right);

    public static bool operator !=(Address? left, Address? right) => !Equals(left, right);
}