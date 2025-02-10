using Microsoft.AspNetCore.Mvc;

namespace imp.Controllers;

[ApiController]
// Assumption: Only one controller. Append '/[controller]', for example, to differentiate if needed later. 
[Route("api")]

// Reads from a multipart file upload asynchronously. Skipping the first (header) line, each line is passed to
// the  SRFExaminer for parsing and analysis. returning various properties of the sales records in the file.
// Assumption: the first line is a header line
// Assumption: The file provided is a properly formatted CSV file. 
public class SalesRecordFileExaminerController : ControllerBase
{
    private readonly ILogger<SalesRecordFileExaminerController> _logger;

    public SalesRecordFileExaminerController(ILogger<SalesRecordFileExaminerController> logger)
    {
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadCSV([FromForm] IFormFile file)
    {
        // Verify there's something to read. 
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file, or file is empty");
        }

        var srfExaminer = new SalesRecordFileExaminer();

        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        // Read and skip first line. 
        string? headerLine = await reader.ReadLineAsync();

        // Keep count of the number of malformed lines. 
        int numMalformed = 0;
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            // TODO: Consider handling out of memory conditions for very large files.  
            bool success = srfExaminer.addRecordfromString(line);
            if (!success) numMalformed++;
        }
        
        // No valid records means no useful properties can be computed. Error.  
        if (srfExaminer.numRecords() == 0)
        {
            return BadRequest("No records found");
        }
        
        //  Compute the properties and return as part of the OK message. 
        SalesRecordsProperties srp = srfExaminer.getProperties();
        return Ok(new
        {
            Message = "Sales Records CSV file read.",
            
            // Note: Not required by spec, but helpful. 
            RecordsRead = srfExaminer.numRecords(),
            RecordsDropped = numMalformed,
            
            Result = srp
        });
    }
}