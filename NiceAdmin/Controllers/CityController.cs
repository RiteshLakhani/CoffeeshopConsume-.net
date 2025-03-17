using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using NiceAdmin.Models;
using OfficeOpenXml;
using NiceAdmin.Helper;

namespace NiceAdmin.Controllers
{
    public class CityController : Controller
    {
        private readonly IConfiguration _configuration;

        #region configuration
        public CityController(IConfiguration configuration)
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
            objCmd.CommandText = "PR_LOC_City_SelectAll";

            SqlDataReader objSDR = objCmd.ExecuteReader();
            dt.Load(objSDR);
            conn.Close();
            return View("Index", dt);
        }
        #endregion

        #region Delete
        public IActionResult CityDelete(string CityID)
        {
            // Decrypt the CityID
            int decryptedCityID = Convert.ToInt32(UrlEncryptor.Decrypt(CityID.ToString()));

            string connectionstr = _configuration.GetConnectionString("ConnectionString");

            using (SqlConnection conn = new SqlConnection(connectionstr))
            {
                conn.Open();

                using (SqlCommand sqlCommand = conn.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_LOC_City_Delete";
                    sqlCommand.Parameters.AddWithValue("@CityID", decryptedCityID);
                    sqlCommand.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Add
        public IActionResult CityAddEdit(string? CityID)
        {
            int? decryptedCityID = null;

            // Decrypt only if CityID is not null or empty
            if (!string.IsNullOrEmpty(CityID))
            {
                string decryptedCityIDString = UrlEncryptor.Decrypt(CityID); // Decrypt the encrypted CityID
                decryptedCityID = int.Parse(decryptedCityIDString); // Convert decrypted string to integer
            }

            LoadCountryList();
            if (decryptedCityID.HasValue)
            {
                string connectionstr = _configuration.GetConnectionString("ConnectionString");
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(connectionstr))
                {
                    conn.Open();
                    using (SqlCommand objCmd = conn.CreateCommand())
                    {
                        objCmd.CommandType = CommandType.StoredProcedure;
                        objCmd.CommandText = "PR_LOC_City_SelectByPK";
                        objCmd.Parameters.Add("@CityID", SqlDbType.Int).Value = decryptedCityID;

                        using (SqlDataReader objSDR = objCmd.ExecuteReader())
                        {
                            dt.Load(objSDR);
                        }
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    CityModel model = new CityModel();
                    foreach (DataRow dr in dt.Rows)
                    {
                        model.CityID = Convert.ToInt32(dr["CityID"]);
                        //model.CityID = Convert.ToInt32(UrlEncryptor.Encrypt(decryptedCityID);
                        model.CityName = dr["CityName"].ToString();
                        model.StateID = Convert.ToInt32(dr["StateID"]);
                        model.CountryID = Convert.ToInt32(dr["CountryID"]);
                        model.CityCode = dr["CityCode"].ToString();
                        ViewBag.StateList = GetStateByCountryID(model.CountryID);
                    }
                    GetStatesByCountry(model.CountryID);
                    return View("CityAddEdit", model);
                }
            }

            return View("CityAddEdit");
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
                    sqlCommand.CommandText = "PR_LOC_City_SelectAll";

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
                            var fileName = "Cities.xlsx";
                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                        }
                    }
                }
            }
        }
        #endregion

        #region Save
        // Save action handles both insert and update operations
        [HttpPost]
        public IActionResult Save(CityModel modelCity)
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
                        if (modelCity.CityID == null)
                        {
                            objCmd.CommandText = "PR_LOC_City_Insert";
                        }
                        else
                        {
                            objCmd.CommandText = "PR_LOC_City_Update";
                            objCmd.Parameters.Add("@CityID", SqlDbType.Int).Value = modelCity.CityID;
                        }

                        // Pass parameters
                        objCmd.Parameters.Add("@CityName", SqlDbType.VarChar).Value = modelCity.CityName;
                        objCmd.Parameters.Add("@CityCode", SqlDbType.VarChar).Value = modelCity.CityCode;
                        objCmd.Parameters.Add("@StateID", SqlDbType.Int).Value = modelCity.StateID;
                        objCmd.Parameters.Add("@CountryID", SqlDbType.Int).Value = modelCity.CountryID;

                        objCmd.ExecuteNonQuery(); // Execute the query
                    }
                }

                TempData["CityInsertMsg"] = "Record Saved Successfully"; // Success message
                return RedirectToAction("Index"); // Redirect to city listing
            }

            LoadCountryList(); // Reload dropdowns if validation fails
            return View("CityAddEdit", modelCity);
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

