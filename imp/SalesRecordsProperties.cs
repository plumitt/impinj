using imp.Controllers;

namespace imp;

// For returning the key result from the SalesRecordPropertiesMaintainer below.
public class SalesRecordsProperties
{
    public decimal MedianUnitCost { get; set; }
    public string? MostCommonRegion { get; set; }
    public string FirstOrderDate { get; set; }
    public string LastOrderDate { get; set; }
    public int DaysBetweenOrders { get; set; }
    public decimal TotalTotalRevenue { get; set; }

}

// Used in computation of the median unit cost.
public class UnitSale
{
    public int unitsSold {get; set;}
    public decimal unitCost {get; set;}
}

// The SalesRecordsPropertiesMaintainer acdepts SalesRecords provided one at at time, and maintains the state
// necessary to compute, upon request, these four properties across all such provided records:
// A) The median unit cost
// B) The most common region
// C) The first and last order date and the days between them
// D) the total revenue
//
//
// A) Median Unit Cost:
// TODO: Clarify definition of median unit cost
// Assumption: Assume "Median Unit Cost" is to be determined on a PER UNIT SOLD basis.
//  The spec "median unit cost" could be interpreted 2 ways:
//      a) look at the cost per individual unit on a PER SALES RECORD basis.
//      b) account for the number of units sold on PER UNIT SOLD basis.
//  For example, if these are the two orders: O1: 20 units sold at $1, 1 unit solid at $2, and 1 unit sold at $3  
//  then (a) would return $2, while (b) would return $1. Both are reasonable.
//  (a) would be appropriate if the sales records were for, say, custom items, and the desire was to understand median
//      cost from the perspective of the purchaser, reflecting the median cost for producing a unit for a new purchaser. 
//  (b) is appropriate if the desire is to understand the cost point from the perspective of the manufacturer,
//      reflecting the median cost for all units for all sales.
//  If (b) is implemented, it is easy to produce (a) by making units sold = 1 for all sales. 
//  Given this, it makes sense to use intepretation (b).
//
// Implementation choices:
//   Determination of the median can be performed in two different ways: 
//      1) with two heaps (one for lower half, one for upper half, with O(log n) insert &  O(1) median determination
//      2) use a List , appending one record at a time O(1), and, on request, sort, and find the median with O(n log n)
//   It's trickier to implement (b) with method (1), as more operations are required to maintain the upper half,
//   lower half invariant of the two heaps. Both are O(nlog(n)) given the spec'd single request for the mediuan.
//   So, method (2) is chosen. 
// Note: A separate "unitSale" record is used for memory efficiency in the event all sales records are not retained
//    in memory elsewhere. 
// Initialize an empty list of unitSales (which contain a numUnits and unit Cost), and a total units sold accumulator.
// For each record, add a unit sales, and add the units solid to the accumulator. O(1)
// Upon Request, , we can sort the list by unitCost, and then linearly probe through the list   
//   counting units solid, unit we find the record which brings this to half the total units sold, then this item
//   has the median cost. See discussion of even vs. odd numbers of records in the implementation below. O(n log n)
// Assumption: There is sufficient memory for this. 
//
// B) Most Common Region:
// Initialize an empty <string, int> dictionary
// For each record,  increment a counter in a dictionary key'd on the region. (When a new region is
//   encountered, i.e. no entry in the dictionary for this key, an entry is added with counter = 1.) O(1)
// Upon request,  iterate through all entries in the dictionary, and return the key for 
//   the entry with the greatest counter value. O(m) where m is number of distinct regions.
// TODO: Clarify spec in terms of how to handle ties.
//       Right now, ties are broken arbitrarily as the spec requests "the most common Region", 
//       which cannot be done if there are ties. Corrections could be "the most common regionS". 
//       or "a most common region". 
//
// C) First/Last order date + days between: 
// Initialize firstOrderData to DataTime.minValue and lastOrderDate to DateTime.maxValue.
// For each record, compare the order date to these and update appropriately if needed. O(1)
// Upon request, compute days between dates using the DateTime class. O(1)
// Note: first and last order date could mean the dates of first and last order, AS STORED IN THE FILE, but this 
// seems like an illogical thing to be extracting. 
// Assumption: "days between" has the standard meaning, such that the number days Between 1/1 and 1/3 is 2, i.e.
//   i.e the difference in time between the purchases measured in days, vs. the number of distinct days
//   that fall strictly between the two dates (which would be 1 in given example)
//
// D)  Total total revenue
// Note: Since the CVS representation has a column "total revenue", the request for total revenue across the 
//   whole list of sales records is really the sum of those total revenues, or the "Total" "total revenue". 
// Initialize a decimal accumulator to zero.
// For each record, add its total revenue to this accumulator. O(1)
// Upon request, return this accumulator. O(1)
// Assumption: The decimal type (with about 28 digits of precision) is sufficient. Decimal will overflow and 
//   throw an error, but for expediency, assume this isn't going to happen. 
//
// TODO: Consider a big refactor such that each property is maintained and retrieved from a separate class to 
//       facilitate testing and readability of additional functionality. 

public class SalesRecordsPropertiesMaintainer
{
    // Median Unit Cost
    private List<UnitSale> unitSales = new List<UnitSale>();
    private int totalUnitsSold = 0;

    // Most Common Region 
    private Dictionary<string, int> regionCountDict = new Dictionary<string, int>();

    // First/Last Order Dates
    private DateTime firstOrderDate = DateTime.MaxValue;
    private DateTime lastOrderDate = DateTime.MinValue;

    // Total Total Revenue
    private Decimal totalTotalRevenue = 0;

    public void Add(SalesRecord r)
    {
        // TODO: Consider refactoring these into different methods. 
        // Update unit sales for computing median unit cost.
        unitSales.Add(new UnitSale() { unitCost = r.UnitCost, unitsSold = r.UnitsSold });
        totalUnitsSold += r.UnitsSold;

        // Update region dictionary for computing most common region.
        if (!regionCountDict.ContainsKey(r.Region))
            regionCountDict[r.Region] = 0;
        regionCountDict[r.Region]++;

        // Update order dates, for computing first, last etc. 
        if (r.OrderDate < firstOrderDate) firstOrderDate = r.OrderDate;
        if (r.OrderDate > lastOrderDate) lastOrderDate = r.OrderDate;

        // Update total total revenue
        totalTotalRevenue += r.TotalPrice;
    }

    public SalesRecordsProperties getProperties()
    {
        SalesRecordsProperties properties = new SalesRecordsProperties();
        
        // TODO: Consider refactoring these into different methods. 
       
        // Median Unit Cost
        // Sort in place.
        unitSales.Sort((x, y) => x.unitCost.CompareTo(y.unitCost));
        int midpoint = (totalUnitsSold / 2);
        bool isEven = (totalUnitsSold % 2) == 0;
        if (!isEven) midpoint++; // eg. 5 units sold means the 3rd unit is the median unit, i.e. (5/2)+1
        
        int unitSoldCount = 0;
        // Iterate to find midpoint.
        // Note: Iteration is done by index rather than using foreach due to the need to handle even #'s of records.
        for (int i = 0; i < unitSales.Count; i++)
        {
            unitSoldCount += unitSales[i].unitsSold;
            if ((unitSoldCount > midpoint) ||    
                ((unitSoldCount == midpoint) && (!isEven)))
            {
                // If the midpoint is included in units in the salesRecord, OR
                //  the midpoint is the last unit in this salesRecord AND an odd number of total units were sold, 
                // Then, the median is the unitCost of this sale. 
                properties.MedianUnitCost = unitSales[i].unitCost;
                break;
            } 
            else if ((unitSoldCount == midpoint) && isEven)
            {
                // Edge case: If there are an even number of total units sold, and the midpoint is the 
                // last unit in this sale, then the unit cost is the average of the cost in this 
                // and the next sale. eg. 1 at $10 , 2 at $20, 3 at $30 --> median = 25. 
                // Assumption: No need to round to whole cents. 
                properties.MedianUnitCost = (unitSales[i].unitCost + unitSales[i + 1].unitCost) / 2;
                break;
            }
        }

        // Most Common Region
        int maxRegionCount = 0;
        foreach (KeyValuePair<string, int> kvp in regionCountDict)
        {
            if (kvp.Value > maxRegionCount)
            {
                maxRegionCount = kvp.Value;
                properties.MostCommonRegion = kvp.Key;
            }
        }
        
        // Order Dates
        properties.FirstOrderDate = firstOrderDate.ToShortDateString();
        properties.LastOrderDate = lastOrderDate.ToShortDateString();
        properties.DaysBetweenOrders = (lastOrderDate - firstOrderDate).Days;
        
        
        // Total total revenue
        properties.TotalTotalRevenue = totalTotalRevenue;
        
        return properties;
    }

}