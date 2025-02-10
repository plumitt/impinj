namespace imp;

// Creates an in-memory list of sales records, and maintains a set of properties, across all
// sales records in the list using SalesRecordsPropertiesMaintainer. 
//  
// TODO: Clairfy spec w.r.t. in-memory list requirement.
//      Spec required "parsing file into an object". "Parsing" suggests a requirement to understand and store
//       all the fields in each record. Hence, the list of records is created here, though it is unused. 
//      If in-memory list is not to be used, then properties below can be computed without storing the list. 
// Assumption: In memory list is needed to facilitate future functionality. 

// TODO: Clarify spec w.r.t. data size. 
// Assumption: Memory is sufficient to hold entire file. 

// TODO: Clarify the sales records spec to allow data verification for fields which appear to be enumerated tyesp
//      As provided, the spec does not define the valid values for strings fields which are clearly enumerated types.
// Assumption: No need to parse into enums for now, string representation, though large, is okay. 
// Assumption: Verification of enumerated type values is not needed for the current spec, and is omitted. 
// Assumption: Prices & costs should be positive. Nothing will break if they aren't, but thorough data validation 
// would check this. 
// Assumption: All dates are in Month/Day/Year
//
// TODO: Clarify the sales records spec to potentially remove or verify derived/denormalized values. 
//      For example, Revenue = Profit + Cost. A well designed SalesRecord object schema would avoid easily 
//       computable derived values. 
// Assumption: The provided CSV is preliminary, but likely to be built upon in the future. As such, this implementation
// will verify the derived fields are consistent, and use normalized SalesRecord objects. 


public class SalesRecordFileExaminer
{
    private List<SalesRecord> salesRecords = new List<SalesRecord>();
    private SalesRecordsPropertiesMaintainer srPropertiesMaintainer = new SalesRecordsPropertiesMaintainer();

    private SalesRecord? createRecordfromParts(string[] parts)
    // TODO: Consider a more thorough data validation plan. 
    {
        var r = new SalesRecord
        {
            Region = parts[0],
            Country = parts[1],
            ItemType = parts[2],
            SalesChannel = parts[3],
            OrderPriority = parts[4],
            OrderDate = DateTime.Parse(parts[5]),
            OrderId = Int32.Parse(parts[6]),
            ShipDate = DateTime.Parse(parts[7]),
            UnitsSold = Int32.Parse(parts[8]),
            UnitPrice = Decimal.Parse(parts[9]),
            UnitCost = Decimal.Parse(parts[10])
        };
        // Validate the price/cost/etc values which can be readily computed from the others. 
        if ((r.TotalPrice != Decimal.Parse(parts[11])) ||
            (r.TotalCost != Decimal.Parse(parts[12])) ||
            (r.TotalProfit != Decimal.Parse(parts[13])))
        {
            return null;
        }

        return r;
    }
    
   
    // Input: Takes a CSV string representation of the sales record
    // Action: 1) creates an internal object representation and adds to list of records. 
    //         2) Updates the properties of the entire list of records as required
    // Note: Enum fields are not validated, but derived fields as noted above are. 
    // Output: true on success, false if any parsing/validation problems are encountered
    // Note: Some parsing may throw an error. These are not caught intelligently. 
    // TODO: Consider using proper built in automatic documentation 
    public Boolean addRecordfromString(string record)
    {
        var parts = record.Split(',');
        if (parts.Count() != 14) return false;

        var salesRecord = createRecordfromParts(parts);
        if (salesRecord != null)
        {
            salesRecords.Add(salesRecord);
            srPropertiesMaintainer.Add(salesRecord);
            
            return true;
        }
        else
        {
            return false;
        }
    }

    public SalesRecordsProperties getProperties()
    {
        return srPropertiesMaintainer.getProperties();
    }
    
    // Note: Not in spec, provided for convenience in debugging. 
    public int numRecords()
    {
        return salesRecords.Count;
    }
}