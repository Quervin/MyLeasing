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
    public class LesseesController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public LesseesController(DataContext dataContext,
            IUserHelper userHelper,
            IMailHelper mailHelper)
        {
            _dataContext = dataContext;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        [HttpGet]
        public IEnumerable<Lessee> GetLessees()
        {
            return _dataContext.Lessees
                .Include(l => l.User)
                .OrderBy(l => l.Id);
        }


        //Métodos para Angular
        [HttpGet]
        [Route("GetLesseesListWeb")]
        public async Task<IActionResult> GetLesseesList()
        {
            try
            {
                var managers = await _dataContext.Lessees
                    .Include(o => o.User)
                    .Include(o => o.Contracts).ToListAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de los lesse.",
                    Result = managers
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el listado de los lessees." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DetailsLesseWeb/{lesseId}")]
        public async Task<IActionResult> DetailsLesse(int lesseId)
        {
            try
            {
                var lessee = await _dataContext.Lessees
               .Include(l => l.User)
               .Include(l => l.Contracts)
               .FirstOrDefaultAsync(m => m.Id == lesseId);

                if (lessee == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información del lesse."
                    });
                }

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información del lesse.",
                    Result = lessee
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al cargar la información del lesse." + ex.Message
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
                        Message = "Se ha producido un error al intertar regristrar el usuario."
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

                var usernew = await _userHelper.AddUserWeb(request, "Lessee");

                if (usernew != null)
                {
                    var lessee = new Lessee
                    {
                        Contracts = new List<Contract>(),
                        User = user,
                    };

                    _dataContext.Lessees.Add(lessee);
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
                    Message = "Se ha producido un error al intertar regristrar el usuario." + ex.Message
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

                var lessee = await _dataContext.Lessees
                        .Include(o => o.User)
                        .FirstOrDefaultAsync(o => o.Id == request.Id);

                lessee.User.Document = request.Document;
                lessee.User.FirstName = request.FirstName;
                lessee.User.LastName = request.LastName;
                lessee.User.Address = request.Address;
                lessee.User.PhoneNumber = request.Phone;

                await _userHelper.UpdateUserAsync(lessee.User);

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
        [Route("DeleteWeb/{lesseId}")]
        public async Task<IActionResult> Delete(int lesseId)
        {
            try
            {
               var lessee = await _dataContext.Lessees
              .Include(o => o.User)
              .FirstOrDefaultAsync(m => m.Id == lesseId);

                if (lessee == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al eliminar el usuario."
                    });
                }

                _dataContext.Lessees.Remove(lessee);
                await _dataContext.SaveChangesAsync();
                await _userHelper.DeleteUserAsync(lessee.User.Email);

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

        [HttpGet]
        [Route("DetailsContractWeb/{contractId}")]
        public async Task<IActionResult> DetailsContract(int contractId)
        {
            try
            {
                var contract = await _dataContext.Contracts
                    .Include(c => c.Owner)
                    .ThenInclude(o => o.User)
                    .Include(c => c.Lessee)
                    .ThenInclude(o => o.User)
                    .Include(c => c.Property)
                    .ThenInclude(p => p.PropertyType)
                    .FirstOrDefaultAsync(pi => pi.Id == contractId);

                if (contract == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información del contrato."
                    });
                }

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información del contrato.",
                    Result = contract
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al cargar la información del contrato." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DeleteContractWeb/{contractId}")]
        public async Task<IActionResult> DeleteContract(int contractId)
        {
            try
            {
                var contract = await _dataContext.Contracts
                    .FirstOrDefaultAsync(c => c.Id == contractId);

                if (contract == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al eliminar el contrato."
                    });
                }

                _dataContext.Contracts.Remove(contract);
                await _dataContext.SaveChangesAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Se elimino el contrato correctamente.",
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al eliminar el contrato." + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("UpdateContractWeb")]
        public async Task<IActionResult> UpdateContract(AddContractRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se produjo un error al intentar actualizar el contrato."
                    });
                }

                var owner = await _dataContext.Owners.FindAsync(request.OwnerId);
                if (owner == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Usuario no valido."
                    });
                }

                var property = await _dataContext.Properties.FindAsync(request.PropertyTypeId);
                if (property == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Propiedad no valida."
                    });
                }

                var lessee = await _dataContext.Lessees.FindAsync(request.LesseeId);
                if (lessee == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Usuario no valido."
                    });
                }

                var contrat = new Contract
                {
                    Owner = owner,
                    Price = request.Price,
                    Property = property,
                    Lessee = lessee,
                    Remarks = request.Remarks,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsActive = request.IsActive,

                };

                _dataContext.Contracts.Update(contrat);
                await _dataContext.SaveChangesAsync();
                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "El contrato ha sido actualizado correctamente."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se produjo un error al intentar actualizar el contrato." + ex.Message
                });
            }
        }
    }
}
