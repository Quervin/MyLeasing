using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLeasing.Common.Models;
using MyLeasing.Web.Data;
using MyLeasing.Web.Data.Entities;
using MyLeasing.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ManagersController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public ManagersController(
            DataContext dataContext,
            IUserHelper userHelper,
            IMailHelper mailHelper)
        {
            _dataContext = dataContext;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        //Métodos para Angular
        [HttpGet]
        [Route("GetManagersWeb")]
        public async Task<IActionResult> GetManagerList()
        {
            try
            {
                var managers = await _dataContext.Managers
                    .Include(m => m.User).ToListAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de las propiedades.",
                    Result = managers
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el listado de los managers." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DetailsManagerWeb/{managerId}")]
        public async Task<IActionResult> DetailsManager(int managerId)
        {
            try
            {
                var manager = await _dataContext.Managers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == managerId);

                if (manager == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información del maneger."
                    });
                }

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información del manager.",
                    Result = manager
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al cargar la información del maneger." + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("CreateWeb")]
        public async Task<IActionResult> Create(AddUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al intertar regristrar el usuario"
                    });
                }

                var user = await _userHelper.GetUserByEmailAsync(request.Email);
                if (user != null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Este correo ya esta registrado."
                    });
                }

                var usernew = await _userHelper.AddUserWeb(request, "Manager");

                if (usernew != null)
                {
                    var manager = new Manager { User = user };

                    _dataContext.Managers.Add(manager);
                    await _dataContext.SaveChangesAsync();

                    var myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    var tokenLink = Url.Action("ConfirmEmail", "Account", new
                    {
                        userid = user.Id,
                        token = myToken
                    }, protocol: HttpContext.Request.Scheme);

                    _mailHelper.SendMail(request.Email, "Correo de confirmación", $"<h1>Email Confirmation</h1>" +
                   $"Para habilitar el usuario, " +
                   $"por favor presionar el siguente link:</br></br><a href = \"{tokenLink}\">Confirmar Correo electrónico</a>");
                }
                else
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Ya existe un usario con este correo electrónico."
                    });
                }

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Se ha registrado el usuario y enviado un correo de confirmación."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intertar regristrar el usuario" + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("EditWeb")]
        public async Task<IActionResult> Edit(EditUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al editar el usuario."
                    });
                }

                var manager = await _dataContext.Managers
                                    .Include(m => m.User)
                                    .FirstOrDefaultAsync(o => o.Id == request.Id);

                manager.User.Document = request.Document;
                manager.User.FirstName = request.FirstName;
                manager.User.LastName = request.LastName;
                manager.User.Address = request.Address;
                manager.User.PhoneNumber = request.Phone;

                await _userHelper.UpdateUserAsync(manager.User);

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "El usuario ha sido actualizado."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al editar el usuario." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DeleteWeb/{managerId}")]
        public async Task<IActionResult> Delete(int managerId)
        {
            try
            {
                var manager = await _dataContext.Managers
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == managerId);

                if (manager == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al eliminar el usuario."
                    });
                }

                _dataContext.Managers.Remove(manager);
                await _dataContext.SaveChangesAsync();
                await _userHelper.DeleteUserAsync(manager.User.Email);

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "El usuario ha sido eliminado correctamente."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al eliminar el usuario." + ex.Message
                });
            }
        }
    }
}
