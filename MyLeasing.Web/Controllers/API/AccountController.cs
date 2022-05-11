using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    public class AccountController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(
            DataContext dataContext,
            IUserHelper userHelper,
            IMailHelper mailHelper)
        {
            _dataContext = dataContext;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] UserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Bad request"
                    });
                }

                var user = await _userHelper.GetUserByEmailAsync(request.Email);
                if (user != null)
                {
                    return BadRequest(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "This email is already registered."
                    });
                }

                user = new User
                {
                    Address = request.Address,
                    Document = request.Document,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.Phone,
                    UserName = request.Email
                };

                var result = await _userHelper.AddUserAsync(user, request.Password);
                if (result != IdentityResult.Success)
                {
                    return BadRequest(result.Errors.FirstOrDefault().Description);
                }

                var userNew = await _userHelper.GetUserByEmailAsync(request.Email);

                if (request.RoleId == 1)
                {
                    await _userHelper.AddUserToRoleAsync(userNew, "Owner");
                    _dataContext.Owners.Add(new Owner { User = userNew });
                }
                else
                {
                    await _userHelper.AddUserToRoleAsync(userNew, "Lessee");
                    _dataContext.Lessees.Add(new Lessee { User = userNew });
                }

                await _dataContext.SaveChangesAsync();

                var myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                var tokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userid = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                _mailHelper.SendMail(request.Email, "Email confirmation", $"<h1>Email Confirmation</h1>" +
                    $"To allow the user, " +
                    $"please click on this link:</br></br><a href = \"{tokenLink}\">Confirm Email</a>");

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "A Confirmation email was sent. Please confirm your account and log into the App."
                });
            }
            catch (Exception ex)
            {

                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Bad request" + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("RecoverPassword")]
        public async Task<IActionResult> RecoverPassword([FromBody] EmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Bad request"
                    });
                }

                var user = await _userHelper.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "This email is not assigned to any user."
                    });
                }

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                var link = Url.Action("ResetPassword", "Account", new { token = myToken }, protocol: HttpContext.Request.Scheme);
                _mailHelper.SendMail(request.Email, "Password Reset", $"<h1>Recover Password</h1>" +
                    $"To reset the password click in this link:</br></br>" +
                    $"<a href = \"{link}\">Reset Password</a>");

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "An email with instructions to change the password was sent."
                });
            }
            catch (Exception ex)
            {

                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Bad request" + ex.Message 
                });
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutUser([FromBody] UserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userEntity = await _userHelper.GetUserByEmailAsync(request.Email);
                if (userEntity == null)
                {
                    return BadRequest(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Bad request"
                    });
                }

                userEntity.FirstName = request.FirstName;
                userEntity.LastName = request.LastName;
                userEntity.Address = request.Address;
                userEntity.PhoneNumber = request.Phone;
                userEntity.Document = request.Document;

                var respose = await _userHelper.UpdateUserAsync(userEntity);
                if (!respose.Succeeded)
                {
                    return BadRequest(respose.Errors.FirstOrDefault().Description);
                }

                var updatedUser = await _userHelper.GetUserByEmailAsync(request.Email);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Bad request" + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("ChangePassword")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Bad request"
                    });
                }

                var user = await _userHelper.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "This email is not assigned to any user."
                    });
                }

                var result = await _userHelper.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    return BadRequest(new Response<object>
                    {
                        IsSuccess = false,
                        Message = result.Errors.FirstOrDefault().Description
                    });
                }

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "The password was changed successfully!"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Bad request" + ex.Message
                });
            }
        }

        //Métodos para Angular

        [HttpPost]
        [Route("RegisterWeb")]
        public async Task<IActionResult> RegisterUser(AddUserRequest request)
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

                var role = "Owner";
                if (request.RoleId == 1)
                {
                    role = "Lessee";
                }

                await _userHelper.AddUserWeb(request, role);


                if (request.RoleId == 1)
                {
                    var lessee = new Lessee
                    {
                        Contracts = new List<Contract>(),
                        User = user
                    };

                    _dataContext.Lessees.Add(lessee);
                }
                else
                {
                    var owner = new Owner
                    {
                        Contracts = new List<Contract>(),
                        Properties = new List<Property>(),
                        User = user
                    };

                    _dataContext.Owners.Add(owner);
                }

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

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Un correo de confirmacion fue enciado. Porfavor confirmar su cuenta para poder Iniciar Sesión."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intertar regristrar el usuario." + ex.Message
                });
            }
           
        }

        [HttpPost]
        [Route("ChangeUserWeb")]
        public async Task<IActionResult> ChangeUser(EditUserRequest request)
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

                var user = await _userHelper.GetUserByEmailAsync(request.Email);
                if (user != null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Usuario no encontrado."
                    });
                }

                user.Document = request.Document;
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Address = request.Address;
                user.PhoneNumber = request.Phone;

                await _userHelper.UpdateUserAsync(user);

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

        [HttpPost]
        [Route("RecoverPasswordWeb")]
        public async Task<IActionResult> RecoverUserPassword(EmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se produjo un error al intentar recuperar la contraseña."
                    });
                }

                var user = await _userHelper.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Este correo electrónico no esta asignado a ningún usuario."
                    });
                }

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                var link = Url.Action("ResetPassword", "Account", new { token = myToken }, protocol: HttpContext.Request.Scheme);
                _mailHelper.SendMail(request.Email, "Restablecer Contraseña", $"<h1>Recuperra Contraseña</h1>" +
                    $"Para restablecer la contraseña por favor presionar el siguiente link:</br></br>" +
                    $"<a href = \"{link}\">Restablecer Contraseña</a>");

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Un correo con las intrucciones para cambiar la contraseña ha sido enviado."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se produjo un error al intentar recuperar la contraseña." + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("ChangePasswordWeb")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangeUserPassword(ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al intentar cambiar la contraseña."
                    });
                }

                var user = await _userHelper.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Este correo electrónico no esta asignado a ningún usuario."
                    });
                }

                var result = await _userHelper.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = result.Errors.FirstOrDefault().Description
                    });
                }

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Se ha cambiado la contraseña correctamente."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar cambiar la contraseña." + ex.Message
                });
            }
        }

    }
}