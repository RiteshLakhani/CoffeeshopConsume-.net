using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Data.SqlClient;
using System.Data;
using NiceAdmin.Models;

namespace NiceAdmin.Controllers
{
    public class StateController : Controller
    {
        private readonly IConfiguration _configuration;

        #region configuration
        public StateController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Index
        public IActionResult Index()
        {
            string connectionstr = this._configuration.GetConnectionString("ConnectionString");
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(connectionstr);
            conn.Open();
            SqlCommand objCmd = conn.CreateCommand();
            objCmd.CommandType = CommandType.StoredProcedure;
            objCmd.CommandText = "PR_LOC_STATE_SELECTALL"; 
            SqlDataReader objSDR = objCmd.ExecuteReader();
            dt.Load(objSDR);
            conn.Close();
            return View("Index", dt);
        }
        #endregion

        #region StateDelete
        public IActionResult StateDelete(int StateID)
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
                        sqlCommand.CommandText = "PR_LOC_STATE_DELETE";
                        sqlCommand.Parameters.Add("@StateID", SqlDbType.Int).Value = StateID;
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

        #region ExportToExcel
        public IActionResult ExportToExcel()
        {
            string connectionString = this._configuration.GetConnectionString("ConnectionString");
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_LOC_STATE_SELECTALL";
                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(sqlDataReader);
                        using (var package = new ExcelPackage())
                        {
                            var worksheet = package.Workbook.Worksheets.Add("States");
                            worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                            var stream = new MemoryStream();
                            package.SaveAs(stream);
                            stream.Position = 0;
                            var fileName = "States.xlsx";
                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                        }
                    }
                }
            }
        }
        #endregion

        #region Add
        // This action displays the State Add/Edit form
        public IActionResult StateAddEdit(int? StateID)
        {
            // Load the dropdown list of countries
            LoadCountryList();

            // Check if an edit operation is requested
            if (StateID.HasValue)
            {
                string connectionstr = _configuration.GetConnectionString("ConnectionString");
                DataTable dt = new DataTable();

                // Fetch state details by ID
                using (SqlConnection conn = new SqlConnection(connectionstr))
                {
                    conn.Open();
                    using (SqlCommand objCmd = conn.CreateCommand())
                    {
                        objCmd.CommandType = CommandType.StoredProcedure;
                        objCmd.CommandText = "PR_LOC_STATE_SELECTBYPK";
                        objCmd.Parameters.Add("@StateID", SqlDbType.Int).Value = StateID;

                        using (SqlDataReader objSDR = objCmd.ExecuteReader())
                        {
                            dt.Load(objSDR); // Load data into DataTable
                        }
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    // Map data to StateModel
                    StateModel model = new StateModel();
                    foreach (DataRow dr in dt.Rows)
                    {
                        model.StateID = Convert.ToInt32(dr["StateID"]);
                        model.StateName = dr["StateName"].ToString();
                        model.CountryID = Convert.ToInt32(dr["CountryID"]);
                        model.StateCode = dr["StateCode"].ToString();
                    }
                    return View("StateAddEdit", model); // Return populated model to view
                }
            }

            return View("StateAddEdit"); // For adding a new state
        }
        #endregion

        #region Save
        // Save action handles both insert and update operations
        [HttpPost]
        public IActionResult SaveState(StateModel modelState)
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
                        if (modelState.StateID == null)
                        {
                            objCmd.CommandText = "PR_LOC_STATE_INSERT";
                        }
                        else
                        {
                            objCmd.CommandText = "PR_LOC_STATE_UPDATE";
                            objCmd.Parameters.Add("@StateID", SqlDbType.Int).Value = modelState.StateID;
                        }

                        // Pass parameters
                        objCmd.Parameters.Add("@StateName", SqlDbType.VarChar).Value = modelState.StateName;
                        objCmd.Parameters.Add("@StateCode", SqlDbType.VarChar).Value = modelState.StateCode;
                        objCmd.Parameters.Add("@CountryID", SqlDbType.Int).Value = modelState.CountryID;

                        objCmd.ExecuteNonQuery(); // Execute the query
                    }
                }

                TempData["StateInsertMsg"] = "Record Saved Successfully"; // Success message
                return RedirectToAction("Index"); // Redirect to state listing
            }

            LoadCountryList(); // Reload dropdowns if validation fails
            return View("StateAddEdit", modelState);
        }
        #endregion

        #region LoadCountryList
        // Load the dropdown list of countries
        private void LoadCountryList()
        {
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                using (SqlCommand objCmd = conn.CreateCommand())
                {
                    objCmd.CommandType = CommandType.StoredProcedure;
                    objCmd.CommandText = "PR_LOC_Country_SelectComboBox";

                    using (SqlDataReader objSDR = objCmd.ExecuteReader())
                    {
                        dt.Load(objSDR); // Load data into DataTable
                    }
                }
            }

            // Map data to list
            List<CountryDropDownModel> countryList = new List<CountryDropDownModel>();
            foreach (DataRow dr in dt.Rows)
            {
                countryList.Add(new CountryDropDownModel
                {
                    CountryID = Convert.ToInt32(dr["CountryID"]),
                    CountryName = dr["CountryName"].ToString()
                });
            }
            ViewBag.CountryList = countryList; // Pass list to view
        }
        #endregion

        #region GetStatesByCountry
        // AJAX handler for loading states dynamically
        [HttpPost]
        public JsonResult GetStatesByCountry(int CountryID)
        {
            List<StateDropDownModel> loc_State = GetStateByCountryID(CountryID); // Fetch states
            return Json(loc_State); // Return JSON response
        }
        #endregion

        #region GetStateByCountryID
        // Helper method to fetch states by country ID
        public List<StateDropDownModel> GetStateByCountryID(int CountryID)
        {
            string connectionstr = _configuration.GetConnectionString("ConnectionString");
            List<StateDropDownModel> loc_State = new List<StateDropDownModel>();

            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();
                using (SqlCommand objCmd = conn.CreateCommand())
                {
                    objCmd.CommandType = CommandType.StoredProcedure;
                    objCmd.CommandText = "PR_LOC_State_SelectComboBoxByCountryID";
                    objCmd.Parameters.AddWithValue("@CountryID", CountryID);

                    using (SqlDataReader objSDR = objCmd.ExecuteReader())
                    {
                        if (objSDR.HasRows)
                        {
                            while (objSDR.Read())
                            {
                                loc_State.Add(new StateDropDownModel
                                {
                                    StateID = Convert.ToInt32(objSDR["StateID"]),
                                    StateName = objSDR["StateName"].ToString()
                                });
                            }
                        }
                    }
                }
            }

            return loc_State;
        }
        #endregion

    }
}
