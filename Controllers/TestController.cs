using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using API.Data;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    // Inject BOTH EF Core DbContext and IConfiguration (for ADO.NET)
    public TestController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // ==========================================
    // 1. EF CORE APPROACH
    // ==========================================
    [HttpGet("check-efcore")]
    public IActionResult CheckEfCore()
    {
        bool canConnect = _context.Database.CanConnect();

        if (canConnect)
        {
            return Ok("EF Core: Successfully connected to the 'Project' database!");
        }

        return StatusCode(500, "EF Core: Could not connect to the database.");
    }

    // ==========================================
    // 2. ADO.NET APPROACH (Raw SQL)
    // ==========================================
    [HttpGet("check-adonet")]
    public IActionResult CheckAdoNet()
    {
        // Step A: Read the connection string from appsettings.json
        string connectionString = _configuration.GetConnectionString("DefaultConnection")!;

        // Step B: Create the SqlConnection (using statement auto-closes the connection)
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                // Step C: Open the physical connection to SQL Server
                connection.Open();

                // Step D: Write a raw SQL query asking SQL Server which database we are in
                string query = "SELECT DB_NAME() AS CurrentDatabase";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Step E: Execute the command and read the result
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string dbName = reader["CurrentDatabase"].ToString()!;

                            return Ok(new
                            {
                                Approach = "ADO.NET",
                                Status = "Connected Successfully!",
                                DatabaseName = dbName,
                                ConnectionState = connection.State.ToString()
                            });
                        }
                    }
                }

                return Ok("ADO.NET: Connected successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"ADO.NET Connection Failed: {ex.Message}");
            }
        }
    }

    // ==========================================
    // Existing test methods
    // ==========================================
    [HttpGet("method-one")]
    public ActionResult<string> GetMethodOne()
    {
        return Ok("METHOD ONE");
    }

    [HttpGet("method-two")]
    [HttpGet("method2")]
    public ActionResult<string> GetMethodTwo()
    {
        return Ok("METHOD 2");
    }
}