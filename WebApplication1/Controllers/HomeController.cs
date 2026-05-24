using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using WebApplication1.Model;
using WebApplication1.DAL;

namespace WebApplication1.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration
                .GetConnectionString("DefaultConnection");
        }

        [Route("Test")]
        [HttpPost]
        public async Task<IActionResult> Test([FromForm] Student student, AttachmentFile attached)
        {
            byte[] fileBytes = null;
            string fileName = null;

            if (attached.File != null)
            {
                fileName = attached.File.FileName;

                using (var ms = new MemoryStream())
                {
                    await attached.File.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SaveRecords", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", student.Name);
                cmd.Parameters.AddWithValue("@Position", student.Position);
                cmd.Parameters.AddWithValue("@Salary", student.Salary);

                cmd.Parameters.Add("@Attachment", SqlDbType.VarBinary).Value =
                    (object)fileBytes ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@AttachmentName",
                    (object)fileName ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            return Ok("Uploaded Successfully");

        }


        [HttpGet]
        public IActionResult Get(int id)
        {
            List<object> result = new List<object>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("GetList", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.Add(new
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Position = reader["Position"].ToString(),
                        Salary = Convert.ToDecimal(reader["Salary"]),


                        AttachmentName = reader["AttachmentName"] == DBNull.Value
                            ? null
                            : reader["AttachmentName"].ToString()
                    });
                }
            }

            return Ok(result);
        }

    }

}

    