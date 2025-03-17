using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Models;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Specialized;
using OfficeOpenXml;
using System.Drawing;

namespace NiceAdmin.Controllers
{
    public class BillController : Controller
    {
        private IConfiguration configuration;

        public BillController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        #region BillList
        public IActionResult Bills()
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.CommandText = "PR_Bills_SelectAll";
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(sqlDataReader);
            return View(dataTable);
        }
        #endregion

        #region BillDelete
        public IActionResult BillDelete(int BillID)
        {
            try
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "PR_Bills_DeleteByPK";
                sqlCommand.Parameters.Add("@BillID", SqlDbType.Int).Value = BillID;
                sqlCommand.ExecuteNonQuery();
                return RedirectToAction("Bills");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Console.WriteLine(ex.ToString);
                return RedirectToAction("Bills");
            }
        }
        #endregion

        #region BillSave
        public IActionResult BillSave(BillModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                if (model.BillID == 0)
                {
                    command.CommandText = "PR_Bills_Insert";
                }
                else
                {
                    command.CommandText = "PR_Bills_UpdateByPK";
                    command.Parameters.Add("@BillID", SqlDbType.Int).Value = model.BillID;
                }
                command.Parameters.Add("@BillNumber", SqlDbType.VarChar).Value = model.BillNumber;
                command.Parameters.Add("@BillDate", SqlDbType.Date).Value = model.BillDate;
                command.Parameters.Add("@OrderID", SqlDbType.Int).Value = model.OrderID;
                command.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = model.TotalAmount;
                command.Parameters.Add("@Discount", SqlDbType.Decimal).Value = model.Discount;
                command.Parameters.Add("@NetAmount", SqlDbType.Decimal).Value = model.NetAmount;
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = model.UserID;
                command.ExecuteNonQuery();
                TempData["SuccessMessage"] = model.BillID == 0 ? "Bill added successfully!" : "Bill updated successfully!";
                return RedirectToAction("Bills");
            }

            return View("BillsForm", model);
        }
        #endregion

        #region BillForm/Dropdown
        public IActionResult BillsForm(int? BillID)
        {
            string connectionString = this.configuration.GetConnectionString("ConnectionString");

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

            SqlConnection sqlConnection2 = new SqlConnection(connectionString);
            sqlConnection2.Open();
            SqlCommand command2 = sqlConnection1.CreateCommand();
            command2.CommandType = System.Data.CommandType.StoredProcedure;
            command2.CommandText = "PR_Orders_DropDown";
            SqlDataReader reader2 = command2.ExecuteReader();
            DataTable dataTable2 = new DataTable();
            dataTable2.Load(reader2);
            List<OrderDropDownModel> orderList = new List<OrderDropDownModel>();
            foreach (DataRow data in dataTable2.Rows)
            {
                OrderDropDownModel orderDropDownModel = new OrderDropDownModel();
                orderDropDownModel.OrderID = Convert.ToInt32(data["OrderID"]);
                orderDropDownModel.OrderCode = data["OrderCode"].ToString();
                orderList.Add(orderDropDownModel);
            }

            ViewBag.Orders = orderList;

            BillModel billModel = new BillModel();
            if (BillID.HasValue)
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Bills_SelectByPK";
                command.Parameters.AddWithValue("@BillID", BillID);
                SqlDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(reader);
                foreach (DataRow dataRow in table.Rows)
                {
                    billModel.BillID = Convert.ToInt32(dataRow["OrderID"]);
                    billModel.BillNumber = @dataRow["BillNumber"].ToString();
                    billModel.BillDate = Convert.ToDateTime(@dataRow["BillDate"]);
                    billModel.OrderID = Convert.ToInt32(@dataRow["OrderID"]);
                    billModel.TotalAmount = Convert.ToDecimal(@dataRow["TotalAmount"]);
                    billModel.Discount = Convert.ToDecimal(@dataRow["Discount"]);
                    billModel.NetAmount = Convert.ToDecimal(@dataRow["NetAmount"]);
                    billModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
                }

                ViewData["Title"] = "Edit Bill";
            }
            else
            {
                ViewData["Title"] = "Add Bill";
            }
            return View("BillsForm", billModel);
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
                    sqlCommand.CommandText = "PR_Bills_SelectAll";
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

                            var fileName = "Bills.xlsx";
                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                        }
                    }
                }
            }
        }
        #endregion
    }
}