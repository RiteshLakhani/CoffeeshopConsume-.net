using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using System.Reflection.Metadata.Ecma335;

namespace NiceAdmin.Controllers
{
    public class CustomerController : Controller
    {
        private IConfiguration configuration;

        public CustomerController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        #region CustomerList
        public IActionResult Customer()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.CommandText = "PR_Customer_SelectAll";
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(sqlDataReader);

            return View(dataTable);
        }
        #endregion

        #region CustomerDelete
        public IActionResult CustomerDelete(int CustomerID)
        {
            try
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "PR_Customer_DeleteByPK";
                sqlCommand.Parameters.Add("@CustomerID", SqlDbType.Int).Value = CustomerID;
                sqlCommand.ExecuteNonQuery();
                return RedirectToAction("Customer");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Console.WriteLine(ex.ToString);
                return RedirectToAction("Customer");
            }
        }
        #endregion

        #region CustomerSave
        public IActionResult CustomerSave(CustomerModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                if (model.CustomerID == 0)
                {
                    command.CommandText = "PR_Customer_Insert";
                }
                else
                {
                    command.CommandText = "PR_Customer_UpdateByPK";
                    command.Parameters.Add("@CustomerID", SqlDbType.Int).Value = model.CustomerID;
                }
                command.Parameters.Add("@CustomerName", SqlDbType.VarChar).Value = model.CustomerName;
                command.Parameters.Add("@HomeAddress", SqlDbType.VarChar).Value = model.HomeAddress;
                command.Parameters.Add("@Email", SqlDbType.VarChar).Value = model.Email;
                command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = model.MobileNo;
                command.Parameters.Add("@GST_NO", SqlDbType.VarChar).Value = model.GST_NO;
                command.Parameters.Add("@CityName", SqlDbType.VarChar).Value = model.CityName;
                command.Parameters.Add("@PinCode", SqlDbType.VarChar).Value = model.PinCode;
                command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = model.NetAmount;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = model.UserID;
                command.ExecuteNonQuery();

                TempData["SuccessMessage"] = model.CustomerID == 0 ? "Customer added successfully!" : "Customer updated successfully!";
                return RedirectToAction("Customer");
            }
            return View("CustomerForm", model);
        }
        #endregion

        #region CustomerForm/DropDown
        public IActionResult CustomerForm(int? CustomerID)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

            #region UserDropDown
            SqlConnection sqlConnection1 = new SqlConnection(connectionString);
            sqlConnection1.Open();
            SqlCommand command1 = sqlConnection1.CreateCommand();
            command1.CommandType = System.Data.CommandType.StoredProcedure;
            command1.CommandText = "PR_User_DropDown";
            SqlDataReader reader1 = command1.ExecuteReader();
            DataTable dataTable1 = new DataTable();
            dataTable1.Load(reader1);
            List<UserDropDownModel> userList = new List<UserDropDownModel>();
            foreach (DataRow data in dataTable1.Rows)
            {
                UserDropDownModel userDropDownModel = new UserDropDownModel();
                userDropDownModel.UserID = Convert.ToInt32(data["UserID"]);
                userDropDownModel.UserName = data["UserName"].ToString();
                userList.Add(userDropDownModel);
            }

            ViewBag.Users = userList;
            #endregion

            #region CustomerByID

            CustomerModel customerModel = new CustomerModel();
            if (CustomerID.HasValue)
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Customer_SelectByPK";
                command.Parameters.AddWithValue("@CustomerID", CustomerID);
                SqlDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(reader);


                foreach (DataRow dataRow in table.Rows)
                {
                    customerModel.CustomerID = Convert.ToInt32(@dataRow["CustomerID"]);
                    customerModel.CustomerName = @dataRow["CustomerName"].ToString();
                    customerModel.HomeAddress = @dataRow["HomeAddress"].ToString();
                    customerModel.Email = @dataRow["Email"].ToString();
                    customerModel.MobileNo = @dataRow["MobileNo"].ToString();
                    customerModel.GST_NO = @dataRow["GST_NO"].ToString();
                    customerModel.CityName = @dataRow["CityName"].ToString();
                    customerModel.PinCode = @dataRow["PinCode"].ToString();
                    customerModel.NetAmount = Convert.ToDecimal(@dataRow["NetAmount"]);
                    customerModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
                }
                ViewData["Title"] = "Edit Customer";
            }
            else
            {
                ViewData["Title"] = "Add Customer";
            }

            #endregion

            return View("CustomerForm", customerModel);
        }
        #endregion

        #region ExcelCode
        public IActionResult ExportToExcel()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_Customer_SelectAll";
                    using (var sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(sqlDataReader);

                        using (var package = new ExcelPackage())
                        {
                            var worksheet = package.Workbook.Worksheets.Add("Users");
                            worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                            var stream = new MemoryStream();
                            package.SaveAs(stream);
                            stream.Position = 0;

                            var fileName = "Customers.xlsx";
                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
