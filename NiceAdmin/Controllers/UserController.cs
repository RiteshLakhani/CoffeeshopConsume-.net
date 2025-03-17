using Microsoft.AspNetCore.Mvc;
using NiceAdmin.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol;


namespace NiceAdmin.Controllers
{
    public class UserController : Controller
    {
        private IConfiguration configuration;

        public UserController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        #region UserList
        public IActionResult User()
        {

            string connectionString = this.configuration.GetConnectionString("ConnectionString");
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.CommandText = "PR_User_SelectAll";
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(sqlDataReader);
            return View(dataTable);
        }
        #endregion

        #region UserDelete
        [HttpPost]
        public IActionResult UserDelete(int UserID)
        {
            try
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection sqlConnection = new SqlConnection( connectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "PR_User_DeleteByPK";
                sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
                sqlCommand.ExecuteNonQuery();
                return RedirectToAction("User");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Console.WriteLine(ex.ToString);
                return RedirectToAction("User");
            }
        }
        #endregion

        #region UserSave
        public IActionResult UserSave(UserModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                if (model.UserID == 0)
                {
                    command.CommandText = "PR_User_Insert";
                }
                else
                {
                    command.CommandText = "PR_User_UpdateByPK";
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = model.UserID;
                }
                command.Parameters.Add("@UserName", SqlDbType.VarChar).Value = model.UserName;
                command.Parameters.Add("@Email", SqlDbType.VarChar).Value = model.Email;
                command.Parameters.Add("Password", SqlDbType.VarChar).Value = model.Password;
                command.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = model.MobileNo;
                command.Parameters.Add("@Address", SqlDbType.VarChar).Value = model.Address;
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
                command.ExecuteNonQuery();

                TempData["SuccessMessage"] = model.UserID == 0 ? "User added successfully!" : "User updated successfully!";
                return RedirectToAction("User");
            }

            return View("UserForm", model);
        }
        #endregion

        #region UserForm
        public IActionResult UserForm(int? UserID)
        {
            UserModel userModel = new UserModel();
            if (UserID.HasValue)
            {
                string connectionString = this.configuration.GetConnectionString("ConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_User_SelectByPK";
                command.Parameters.AddWithValue("@UserID", UserID);
                SqlDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(reader);

                foreach (DataRow dataRow in table.Rows)
                {
                    userModel.UserID = Convert.ToInt32(@dataRow["UserID"]);
                    userModel.UserName = @dataRow["UserName"].ToString();
                    userModel.Email = @dataRow["Email"].ToString();
                    userModel.Password = @dataRow["Password"].ToString();
                    userModel.MobileNo = @dataRow["MobileNo"].ToString();
                    userModel.Address = @dataRow["Address"].ToString();
                    userModel.IsActive = Convert.ToBoolean(@dataRow["IsActive"]);
                }

                ViewData["Title"] = "Edit User";
            }
            else
            {
                ViewData["Title"] = "Add User";
            }
            return View("UserForm",userModel);
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
                    sqlCommand.CommandText = "PR_User_SelectAll";
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

                            var fileName = "Users.xlsx";
                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                        }
                    }
                }
            }
        }
        #endregion

        #region Login
        [HttpPost]
        public IActionResult UserLogin(UserLoginModel userLoginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string connectionString = this.configuration.GetConnectionString("ConnectionString");
                    SqlConnection sqlConnection = new SqlConnection(connectionString);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_User_Login";
                    sqlCommand.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userLoginModel.UserName;
                    sqlCommand.Parameters.Add("@Password", SqlDbType.VarChar).Value = userLoginModel.Password;
                    SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(sqlDataReader);
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        HttpContext.Session.SetString("UserID", dr["UserID"].ToString());
                        HttpContext.Session.SetString("UserName", dr["UserName"].ToString());
                    }

                    return RedirectToAction("User", "User");
                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
            }
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        #endregion

        #region Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }
        #endregion

        #region Register
        public IActionResult UserRegister(UserRegisterModel userRegisterModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string connectionString = this.configuration.GetConnectionString("ConnectionString");
                    SqlConnection sqlConnection = new SqlConnection(connectionString);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_User_Register";
                    sqlCommand.Parameters.Add("@UserName", SqlDbType.VarChar).Value = userRegisterModel.UserName;
                    sqlCommand.Parameters.Add("@Password", SqlDbType.VarChar).Value = userRegisterModel.Password;
                    sqlCommand.Parameters.Add("@Email", SqlDbType.VarChar).Value = userRegisterModel.Email;
                    sqlCommand.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = userRegisterModel.MobileNo;
                    sqlCommand.Parameters.Add("@Address", SqlDbType.VarChar).Value = userRegisterModel.Address;
                    sqlCommand.ExecuteNonQuery();
                    return RedirectToAction("Login", "User");
                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
                return RedirectToAction("Register");
            }
            return RedirectToAction("Register");
        }

        public IActionResult Register()
        {
            return View();
        }
        #endregion
    }
}
