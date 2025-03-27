﻿using assignement_3.BLL.Interfaces;
using assignement_3.DAL.Models;
using assignement_3.PL.dto;
using assignement_3.PL.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace assignement_3.PL.Controllers
{
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleController(RoleManager<IdentityRole> RoleManager,UserManager<AppUser> userManager)
        {
            _roleManager = RoleManager;
         _userManager = userManager;
        }
     
        [HttpGet]
        public async Task<IActionResult> Index(string? SearchInput)
        {
            IEnumerable<RollToReturnDto> rolls;

            if (SearchInput == null)
            {
                rolls = _roleManager.Roles.Select(u => new RollToReturnDto()
                {
                   
                    Id = u.Id,
                    Name= u.Name,
                });
                    
                    }
            else
            {
                rolls = _roleManager.Roles.Select(u => new RollToReturnDto()
                {
                    Id = u.Id,
                    Name = u.Name,
                }).Where(u=>u.Name.ToLower().Contains(SearchInput.ToLower()));
            }

            return View(rolls);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string? id, string viewName = "Details")
        {
            if (id is null) return BadRequest(error: "Invalid Id"); // 400

            var rolls = await _roleManager.FindByIdAsync(id);

            if (rolls is null) return NotFound(new { statusCode = 404, message = $"rolls With Id :{id} is not found" });

            var dto = new RollToReturnDto()
            {
                Id = rolls.Id,
                Name = rolls.Name,
            };

            return View(viewName, dto);
        }

        [HttpGet]
        public Task<IActionResult> Edit(string? id)
        {
            return Details(id, "Edit");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] string id, RollToReturnDto model)
        {
            if (ModelState.IsValid)
            {

                if (id != model.Id) return BadRequest(error: "Invalid Operations !");

                var role = await _roleManager.FindByIdAsync(id);
                if (role is null) return BadRequest(error: "Invalid Operations !");

                var rollsResult = await _roleManager.FindByNameAsync(model.Name);
                if (rollsResult is null)
                {
                    role.Name = model.Name;

                    var result = await _roleManager.UpdateAsync(role);
                    if (result.Succeeded)

                        return RedirectToAction(nameof(Index));
                }

             
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {

           
            
            return await Details(id, "Delete");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute] string? id, RollToReturnDto model)
        {
            if (ModelState.IsValid)
            {

                if (id != model.Id) return BadRequest(error: "Invalid Operations !");

                var role = await _roleManager.FindByIdAsync(id);
                if (role is null) return BadRequest(error: "Invalid Operations !");
            

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)

                    return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Create(RollToReturnDto model)
        {

            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByNameAsync(model.Name);
                if (role is null)
                {
                    role = new IdentityRole()

                    {
                        Name = model.Name
                    };
                    var result = await _roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    }
            }
            return View();
        }


        [HttpGet]
        public async Task< IActionResult >AddOrRemoveUsers(string id)
        {
            ViewData[index: "Id"] = id;
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return NotFound();
            var usersInRole = new List<UsersInRoleViewModel>();
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var userInRole = new UsersInRoleViewModel()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                };

                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userInRole.IsSelected = true;
                }
                else
                {
                    userInRole.IsSelected = false;
                }
                usersInRole.Add(userInRole);
            }
                return View(usersInRole);
        }


        [HttpPost]
        public async Task<IActionResult> AddOrRemoveUsers(string id, List<UsersInRoleViewModel> users)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return NotFound();
            if (ModelState.IsValid) {
                foreach (var user in users)
                {
                    var appUser = await _userManager.FindByIdAsync(user.UserId);
                    if (appUser is not null)
                    {
                        if (user.IsSelected && !await _userManager.IsInRoleAsync(appUser, role.Name))
                        {
                            await _userManager.AddToRoleAsync(appUser, role.Name);
                        }
                        else if(!user.IsSelected && await _userManager.IsInRoleAsync(appUser, role.Name))
                        {
                            await _userManager.RemoveFromRoleAsync(appUser, role.Name);
                        }
                    }

                }
                return RedirectToAction(nameof(Edit), new { id = id });
            }

            return View(users);
        }
    }
}
