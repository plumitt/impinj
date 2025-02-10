namespace imp;

// PRELIMINARY SalesRecord object. 
// Due to the ad hoc nature of the spec, and the unknown future use of this object, this implementation should
// be considered preliminary. It does not perform all likely useful data validation, storage optimization, or
// transformation steps, as these depend on the future expected use of this object. 
// TODO:  Clarify spec on schema, and determine enumerated types would be helpful for reduced memory footprint or
//        clarity of other code using this object. Candidates for enum are marked with "**ENUM?", below. 
// TODO: When updating to C#11, consider using required constructors. 
// TODO: Consider using nullable value types. 
// TODO: Consider using a builder pattern in future. 
// Assumption: Okay to store likely enums as strings. (See **ENUM below)
// Assumption: No need to confirm all fields are set. 
// Note: All currency values are stored in decimal to ensure decimal fractions (0.xx) are represented exactly. 

public class SalesRecord
{
    public string Region { get; set; }  = string.Empty;     //**ENUM
    public string  Country { get; set;} = string.Empty;     //**ENUM
    public string  ItemType { get; set;  } = string.Empty;   //**ENUM
    public string SalesChannel { get; set; } = string.Empty; //**ENUM - or bool isOnline?
    public string OrderPriority { get; set; } = string.Empty; //**ENUM - (assign int values if priorities have strict ordering)
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public DateTime ShipDate { get; set; } = DateTime.Now;
    public  int OrderId { get; set; } = 0;
    public int UnitsSold { get; set; } = 0;
    public decimal UnitPrice { get; set; } = 0; 
    public decimal UnitCost { get; set; } = 0;
    
    // Note: Renamed from "Total Revenue" in csv spec for clarity and symmetry with Cost
    public decimal TotalPrice  => UnitsSold * UnitPrice; 
    public decimal TotalCost => UnitsSold * UnitCost; 
    public decimal TotalProfit => TotalPrice - TotalCost;
    
    
}