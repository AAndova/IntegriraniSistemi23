using EShop.Domain.DomainModels;
using EShop.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EShop.Web.Controllers
{
    public class UserImportController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserImportController> _logger;

        public UserImportController(
            ILogger<UserImportController> logger,
            IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult ImportUsers()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult ImportUsers(ImportUsersViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    List<User> importedUsers = ReadUsersFromExcel(viewModel.UsersFile);
                    _userService.ImportUsers(importedUsers);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during user import.");
                    ModelState.AddModelError("", "An error occurred during user import.");
                }
            }

            return View(viewModel);
        }

        private List<User> ReadUsersFromExcel(Stream usersFile)
        {
            using (var package = new ExcelPackage(usersFile))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    throw new Exception("Invalid Excel file format.");

                var users = new List<User>();
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var email = worksheet.Cells[row, 1].Value?.ToString();
                    var password = worksheet.Cells[row, 2].Value?.ToString();
                    var role = worksheet.Cells[row, 3].Value?.ToString();

                    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(role))
                    {
                        var user = new User
                        {
                            Email = email,
                            Password = password,
                            Role = role
                        };

                        users.Add(user);
                    }
                }

                return users;
            }
        }
    }
}