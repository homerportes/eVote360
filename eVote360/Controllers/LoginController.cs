using eVote360.Application.DTOs.User;
using eVote360.Application.Helpers;
using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.User;
using eVote360.Application.ViewModels.User.Login;
using eVote360.Domain.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Data;

namespace eVote360.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserSession _userSession;

        public LoginController(IUserService userService, IUserSession userSession)
        {
            _userService = userService;
            _userSession = userSession;
        }
        public IActionResult Index()
        {
            if (_userSession.HasUser())
            {
                UserViewModel userSession = _userSession.GetUserSession()!;
                if (userSession != null)
                {
                    return userSession.Role switch
                    {
                        (int)UserRole.Administrator => RedirectToRoute(new { controller = "HomeAdmin", action = "Index" }),
                        (int)UserRole.PoliticalLeader => RedirectToRoute(new { controller = "DirigenteHome", action = "Index" }),
                        _ => RedirectToRoute(new { controller = "Login", action = "Index" }),
                    };
                }
            }
            return View(new LoginViewModel() { Password = "", Username = "" });
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel vm)
        {
            if (_userSession.HasUser())
            {
                UserViewModel userSession = _userSession.GetUserSession()!;
                if (userSession != null)
                {
                    return userSession.Role switch
                    {
                        (int)UserRole.Administrator => RedirectToRoute(new { controller = "HomeAdmin", action = "Index" }),
                        (int)UserRole.PoliticalLeader => RedirectToRoute(new { controller = "DirigenteHome", action = "Index" }),
                        _ => RedirectToRoute(new { controller = "Login", action = "Index" }),
                    };
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Password = "";
                return View(vm);
            }

            UserDTO? userDto = await _userService.LoginAsync(new LoginDTO()
            {
                Password = vm.Password.Trim(),
                Username = vm.Username.Trim()
            });

            if (userDto != null)
            {
                UserViewModel userVm = new()
                {
                    Id = userDto.Id,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    Username = userDto.Username,
                    Role = userDto.Role,
                    PartyId = userDto.PartyId,
                    PartyName = userDto.PartyName,
                    PartyAcronym = userDto.PartyAcronym,
                    IsActive = userDto.IsActive
                };


                HttpContext.Session.Set<UserViewModel>("User", userVm);

                if (userVm.Role == (int)UserRole.Administrator)
                {
                    return RedirectToRoute(new { controller = "HomeAdmin", action = "Index" });
                }

                return RedirectToRoute(new { controller = "DirigenteHome", action = "Index" });
            }
            else
            {
                ModelState.AddModelError("userValidation", " Data access is incorrect");
            }

            vm.Password = "";
            return View(vm);

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("User");
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
