using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using NiceAdmin.Models;
using OfficeOpenXml;

namespace NiceAdmin.Controllers
{
    public class CountryController : Controller
    {
        private readonly IConfiguration _configuration;

        #region configuration
        public CountryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Index
        public IActionResult Index()
        {
            string connectionstr = this._configuration.GetConnectionString("ConnectionString");
            //PrePare a connection
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(connectionstr);
            conn.Open();

            //Prepare a Command
            SqlCommand objCmd = conn.CreateCommand();
            objCmd.CommandType = CommandType.StoredProcedure;
            objCmd.CommandText = "PR_LOC_Country_SelectAll";

            SqlDataReader objSDR = objCmd.ExecuteReader();
            dt.Load(objSDR);
            conn.Close();
            return View("Index", dt);
        }
        #endregion

        #region CountryDelete
        public IActionResult CountryDelete(int CountryID)
        {
            try
            {
                string connectionString = this._configuration.GetConnectionString("ConnectionString");
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandText = "PR_LOC_Country_Delete";
                        sqlCommand.Parameters.Add("@CountryID", SqlDbType.Int).Value = CountryID;
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Console.WriteLine(ex.ToString());
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region ExcelCode
        public IActionResult ExportToExcel()
        {
            string connectionString = this._configuration.GetConnectionString("ConnectionString");
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_LOC_Country_SelectAll";

                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(sqlDataReader);

                        using (var package = new ExcelPackage())
                        {
                            var worksheet = package.Workbook.Worksheets.Add("Countries");
                            worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                            var stream = new MemoryStream();
                            package.SaveAs(stream);
                            stream.Position = 0;
                            var fileName = "Countries.xlsx";
                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                        }
                    }
                }
            }
        }
        #endregion

        #region Add
        // This action displays the Country Add/Edit form
        public IActionResult CountryAddEdit(int? CountryID)
        {
            // Check if an edit operation is requested
            if (CountryID.HasValue)
            {
                string connectionstr = _configuration.GetConnectionString("ConnectionString");
                DataTable dt = new DataTable();

                // Fetch country details by ID
                using (SqlConnection conn = new SqlConnection(connectionstr))
                {
                    conn.Open();
                    using (SqlCommand objCmd = conn.CreateCommand())
                    {
                        objCmd.CommandType = CommandType.StoredProcedure;
                        objCmd.CommandText = "PR_LOC_Country_SelectByPK";
                        objCmd.Parameters.Add("@CountryID", SqlDbType.Int).Value = CountryID;

                        using (SqlDataReader objSDR = objCmd.ExecuteReader())
                        {
                            dt.Load(objSDR); // Load data into DataTable
                        }
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    // Map data to CountryModel
                    CountryModel model = new CountryModel();
                    foreach (DataRow dr in dt.Rows)
                    {
                        model.CountryID = Convert.ToInt32(dr["CountryID"]);
                        model.CountryName = dr["CountryName"].ToString();
                        model.CountryCode = dr["CountryCode"].ToString();
                    }
                    return View("CountryAddEdit", model); // Return populated model to view
                }
            }

            return View("CountryAddEdit"); // For adding a new country
        }
        #endregion

        #region Save
        // Save action handles both insert and update operations for Country
        [HttpPost]
        public IActionResult SaveCountry(CountryModel modelCountry)
        {
            if (ModelState.IsValid)
            {
                string connectionstr = _configuration.GetConnectionString("ConnectionString");
                using (SqlConnection conn = new SqlConnection(connectionstr))
                {
                    conn.Open();
                    using (SqlCommand objCmd = conn.CreateCommand())
                    {
                        objCmd.CommandType = CommandType.StoredProcedure;

                        // Choose procedure based on operation (insert or update)
                        if (modelCountry.CountryID == null)
                        {
                            objCmd.CommandText = "pr_LOC_Country_Insert";
                        }
                        else
                        {
                            objCmd.CommandText = "pr_LOC_Country_Update";
                            objCmd.Parameters.Add("@CountryID", SqlDbType.Int).Value = modelCountry.CountryID;
                        }

                        // Pass parameters
                        objCmd.Parameters.Add("@CountryName", SqlDbType.VarChar).Value = modelCountry.CountryName;
                        objCmd.Parameters.Add("@CountryCode", SqlDbType.VarChar).Value = modelCountry.CountryCode;

                        objCmd.ExecuteNonQuery(); // Execute the query
                    }
                }

                TempData["CountryInsertMsg"] = "Record Saved Successfully"; // Success message
                return RedirectToAction("Index"); // Redirect to country listing
            }

            return View("CountryAddEdit", modelCountry);
        }
        #endregion
    }
}

