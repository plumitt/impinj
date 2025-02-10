using System.Diagnostics;

namespace SRFExaminerTests;
using RestSharp;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

// NOTE: I did not succeed in getting these to run. I beleive I'm not forming the request correctly here, or not
// giving the correct path to the csv file. I was able to do tests manually using CURL, ie.
// curl -X POST "https://localhost:7161/api/upload" -F "file=@/Users/barryb/Desktop/PreScreen/Input/SalesRecords3.csv" 
// {"message":"Sales Records CSV file read.","recordsRead":3,"recordsDropped":0,"result":{"medianUnitCost":30,"mostCommonRegion":"B","firstOrderDate":"10/8/2014","lastOrderDate":"10/8/2015","daysBetweenOrders":365,"totalTotalRevenue":200}
//
// Im chosing to include the not-running tests rather than go 
public class SrfExaminerTests
{
    private readonly RestClient _client;

    public SrfExaminerTests()
    {
        _client = new RestClient("http://localhost:7161");
    }
    // Example result object
    // 
    //{"medianUnitCost":30,"mostCommonRegion":"B","firstOrderDate":"10/8/2014","lastOrderDate":"10/8/2015","daysBetweenOrders":365,"totalTotalRevenue":200}}

    [Theory]
    [InlineData("testdata-singlerecord.csv", 30, "B", "10/8.2014", "10/8/2015", 365, 200)]
    // more test cases would go here

    public async Task SRFE_ReturnsCorrectResponse(string filename, decimal muc, string mcr, string fod, string lod,
        int sbo, decimal ttr)
    {
        var request = new RestRequest("/api/upload", Method.Post);
        request.AddFile("file", filename, "multipart/form-data");

        var response = await _client.ExecuteAsync(request);
        Assert.NotNull(response);
        Console.WriteLine(response);
        Assert.True(response.IsSuccessful);
        Assert.Equal(200, (int) response.StatusCode);
        
        //Debug.Assert(response.Content != null, "response.Content != null");
        var jsonResponse = JObject.Parse(response.Content);
        Assert.NotNull(jsonResponse["result"]);
        Assert.Equal(muc, jsonResponse["result"]?["medianUnitCost"]);
        Assert.Equal(mcr, jsonResponse["result"]?["mostCommonRegion"]);
        Assert.Equal(fod, jsonResponse["result"]?["firstOrderDate"]);
        Assert.Equal(lod, jsonResponse["result"]?["lastOrderDate"]);
        Assert.Equal(sbo, jsonResponse["result"]?["daysBetweenOrders"]);
        Assert.Equal(ttr, jsonResponse["result"]?["totalTotalRevenue"]);
    }
}