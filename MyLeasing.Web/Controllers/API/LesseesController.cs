using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLeasing.Common.Models;
using MyLeasing.Web.Data;
using MyLeasing.Web.Data.Entities;
using MyLeasing.Web.Helpers;
using MyLeasing.Web.Models;
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
        [Route("GetLesseesWeb/{index}/{countPages}")]
        public async Task<IActionResult> GetLessees(int index, int countPages)
        {
            try
            {
                var total = await _dataContext.Lessees.CountAsync();

                var lessees = await _dataContext.Lessees
                    .Include(o => o.User)
                    .Include(o => o.Contracts)
                    .Skip(index).Take(countPages).ToListAsync();

                var lesseesList = lessees.Select(x => new LesseeResponseApi()
                {
                    Id = x.Id,
                    User = new UserResponseApi()
                    {
                        Address = x.User.Address,
                        Document = x.User.Document,
                        FirstName = x.User.FirstName,
                        LastName = x.User.LastName,
                        Email = x.User.Email,
                        Phone = x.User.PhoneNumber
                    },
                    Contracts = x.Contracts != null ? toContactsResponseApi(x.Contracts) : new List<ContractResponseApi>()
                }).ToList();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de los lessees.",
                    Result = lesseesList,
                    Total = total
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
        [Route("GetLesseeWeb/{lesseeId}")]
        public async Task<IActionResult> GetLessee(int lesseeId)
        {
            try
            {
                var lessee = await _dataContext.Lessees
               .Include(l => l.User)
               .FirstOrDefaultAsync(m => m.Id == lesseeId);

                if (lessee == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información del lessee."
                    });
                }

                var lesseeResponse = new LesseeResponseApi()
                {
                    Id = lessee.Id,
                    User = new UserResponseApi()
                    {
                        Address = lessee.User.Address,
                        Document = lessee.User.Document,
                        FirstName = lessee.User.FirstName,
                        LastName = lessee.User.LastName,
                        Phone = lessee.User.PhoneNumber,
                        Email = lessee.User.Email
                    }
                };

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información del lessee.",
                    Result = lesseeResponse
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al cargar la información del lessee." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DetailsLesseeWeb/{lesseeId}")]
        public async Task<IActionResult> DetailsLessee(int lesseeId)
        {
            try
            {
                var lessee = await _dataContext.Lessees
               .Include(l => l.User)
               .Include(l => l.Contracts)
               .FirstOrDefaultAsync(m => m.Id == lesseeId);

                if (lessee == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información del lessee."
                    });
                }

                var lesseeResponse = new LesseeResponseApi()
                {
                    Id = lessee.Id,
                    User = new UserResponseApi()
                    {
                        Address = lessee.User.Address,
                        Document = lessee.User.Document,
                        FirstName = lessee.User.FirstName,
                        LastName = lessee.User.LastName,
                        Phone = lessee.User.PhoneNumber,
                        Email = lessee.User.Email
                    },
                    Contracts = lessee.Contracts != null ? toContactsResponseApi(lessee.Contracts) : new List<ContractResponseApi>()
                };

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información del lessee.",
                    Result = lesseeResponse
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al cargar la información del lessee." + ex.Message
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

                var contractResponse = new ContractResponseApi()
                {
                    Id = contract.Id,
                    EndDate = contract.EndDate,
                    IsActive = contract.IsActive,
                    StartDate = contract.StartDate,
                    Remarks = contract.Remarks,
                    Price = contract.Price,
                    Property = contract.Property != null ? new PropertyResponseApi()
                    {
                        Id = contract.Property.Id,
                        Neighborhood = contract.Property.Neighborhood,
                        Address = contract.Property.Address,
                        Price = contract.Property.Price,
                        SquareMeters = contract.Property.SquareMeters,
                        Rooms = contract.Property.Rooms,
                        Stratum = contract.Property.Stratum,
                        HasParkingLot = contract.Property.HasParkingLot,
                        IsAvailable = contract.Property.IsAvailable,
                        Remarks = contract.Property.Remarks,
                        Latitude = contract.Property.Latitude,
                        Longitude = contract.Property.Longitude,
                        PropertyType = contract.Property.PropertyType != null ? new PropertyTypeResponseApi()
                        {
                            Id = contract.Property.PropertyType.Id,
                            Name = contract.Property.PropertyType.Name
                        } : new PropertyTypeResponseApi()
                    } : new PropertyResponseApi(),
                    Owner = contract.Owner != null ? new OwnerResponseApi()
                    {
                        Id = contract.Owner.Id,
                        User = new UserResponseApi()
                        {
                            Document = contract.Owner.User.Document,
                            Address = contract.Owner.User.Address,
                            FirstName = contract.Owner.User.FirstName,
                            LastName = contract.Owner.User.LastName
                        }
                    } : new OwnerResponseApi(),
                    Lessee = contract.Lessee != null ? new LesseeResponseApi()
                    {
                        Id = contract.Lessee.Id,
                        User = new UserResponseApi()
                        {
                            Document = contract.Owner.User.Document,
                            Address = contract.Owner.User.Address,
                            FirstName = contract.Owner.User.FirstName,
                            LastName = contract.Owner.User.LastName
                        }
                    } : new LesseeResponseApi()
                };


                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información del contrato.",
                    Result = contractResponse
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
        [Route("GetContractWeb/{contractId}")]
        public async Task<IActionResult> GetContract(int contractId)
        {
            try
            {
                var contract = await _dataContext.Contracts
                    .Include(p => p.Owner)
                    .Include(p => p.Lessee)
                    .Include(p => p.Property)
                    .ThenInclude(pt=> pt.PropertyType)
                    .FirstOrDefaultAsync(p => p.Id == contractId);

                if (contract == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información del contrato."
                    });
                }

                var contractResponse = new ContractResponseApi()
                {
                    Id = contract.Id,
                    EndDate = contract.EndDate,
                    IsActive = contract.IsActive,
                    StartDate = contract.StartDate,
                    Remarks = contract.Remarks,
                    Price = contract.Price,
                    Property = contract.Property != null ? new PropertyResponseApi()
                    {
                        Id = contract.Property.Id,
                        Neighborhood = contract.Property.Neighborhood,
                        Address = contract.Property.Address,
                        Price = contract.Property.Price,
                        SquareMeters = contract.Property.SquareMeters,
                        Rooms = contract.Property.Rooms,
                        Stratum = contract.Property.Stratum,
                        HasParkingLot = contract.Property.HasParkingLot,
                        IsAvailable = contract.Property.IsAvailable,
                        Remarks = contract.Property.Remarks,
                        Latitude = contract.Property.Latitude,
                        Longitude = contract.Property.Longitude,
                        PropertyType = contract.Property.PropertyType != null ? new PropertyTypeResponseApi()
                        {
                            Id = contract.Property.PropertyType.Id,
                            Name = contract.Property.PropertyType.Name
                        } : new PropertyTypeResponseApi()
                    } : new PropertyResponseApi(),
                    Owner = contract.Owner != null ? new OwnerResponseApi()
                    {
                        Id = contract.Owner.Id,
                        User = contract.Owner.User != null ? new UserResponseApi()
                        {
                            Document = contract.Owner.User.Document,
                            Address = contract.Owner.User.Address,
                            FirstName = contract.Owner.User.FirstName,
                            LastName = contract.Owner.User.LastName
                        } : new UserResponseApi()
                    } : new OwnerResponseApi(),
                    Lessee = contract.Lessee != null ? new LesseeResponseApi()
                    {
                        Id = contract.Lessee.Id,
                        User = contract.Lessee.User != null ? new UserResponseApi()
                        {
                            Document = contract.Lessee.User.Document,
                            Address = contract.Lessee.User.Address,
                            FirstName = contract.Lessee.User.FirstName,
                            LastName = contract.Lessee.User.LastName
                        } : new UserResponseApi()
                    } : new LesseeResponseApi()
                };

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información del contrato.",
                    Result = contractResponse
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
        [Route("GetLesseesWeb")]
        public async Task<IActionResult> GetListLessees()
        {
            try
            {
                var lessee = await _dataContext.Lessees
               .Include(l => l.User)
               .OrderBy(pt => pt.User.FullNameWithDocument)
               .ToListAsync();

                var lesseeResponse = lessee.Select( x=> new LesseeResponseApi()
                {
                    Id = x.Id,
                    User = new UserResponseApi()
                    {
                        Address = x.User.Address,
                        Document = x.User.Document,
                        FirstName = x.User.FirstName,
                        LastName = x.User.LastName,
                        Phone = x.User.PhoneNumber,
                        Email = x.User.Email
                    }
                }).ToList();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Lista de lessees.",
                    Result = lesseeResponse
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al cargar la lista de lessees." + ex.Message
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

                var property = await _dataContext.Properties.FindAsync(request.PropertyId);
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
                    Id = request.Id,
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

        private List<ContractResponseApi> toContactsResponseApi(ICollection<Contract> contracts)
        {
            return contracts.Select(x => new ContractResponseApi()
            {
                Id = x.Id,
                EndDate = x.EndDate,
                IsActive = x.IsActive,
                StartDate = x.StartDate,
                Remarks = x.Remarks,
                Price = x.Price,
                Owner = x.Owner != null ? new OwnerResponseApi()
                {
                    Id = x.Owner.Id,
                    User = new UserResponseApi()
                    {
                        Document = x.Owner.User.Document,
                        Address = x.Owner.User.Address,
                        FirstName = x.Owner.User.FirstName,
                        LastName = x.Owner.User.LastName
                    }
                } : new OwnerResponseApi(),
                Lessee = x.Lessee != null ? new LesseeResponseApi()
                {
                    Id = x.Lessee.Id,
                    User = new UserResponseApi()
                    {
                        Document = x.Lessee.User.Document,
                        Address = x.Lessee.User.Address,
                        FirstName = x.Lessee.User.FirstName,
                        LastName = x.Lessee.User.LastName
                    }
                } : new LesseeResponseApi()
            }).ToList();
        }
    }
}
