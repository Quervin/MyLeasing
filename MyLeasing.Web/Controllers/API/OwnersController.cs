using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLeasing.Common.Helpers;
using MyLeasing.Common.Models;
using MyLeasing.Web.Data;
using MyLeasing.Web.Data.Entities;
using MyLeasing.Web.Helpers;
using MyLeasing.Web.Models;

namespace MyLeasing.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OwnersController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IImageHelper _imageHelper;

        public OwnersController(
            DataContext dataContext,
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IImageHelper imageHelper)
        {
            _dataContext = dataContext;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _imageHelper = imageHelper;
        }

        [HttpPost]
        [Route("GetOwnerByEmail")]
        public async Task<IActionResult> GetOwner(EmailRequest emailRequest)
        {
            try
            {
                var user = await _userHelper.GetUserByEmailAsync(emailRequest.Email);
                if (user == null)
                {
                    return BadRequest("User not found.");
                }

                if (await _userHelper.IsUserInRoleAsync(user, "Owner"))
                {
                    return await GetOwnerAsync(emailRequest);
                }
                else
                {
                    return await GetLesseeAsync(emailRequest);
                }
            }
            catch (Exception ex)
            {
                return BadRequest("User not found." + ex.Message);
            }
        }

        private async Task<IActionResult> GetLesseeAsync(EmailRequest emailRequest)
        {
            try
            {
                var lessee = await _dataContext.Lessees
                    .Include(o => o.User)
                    .Include(o => o.Contracts)
                    .ThenInclude(c => c.Owner)
                    .ThenInclude(o => o.User)
                    .FirstOrDefaultAsync(o => o.User.UserName.ToLower().Equals(emailRequest.Email.ToLower()));

                var properties = await _dataContext.Properties
                    .Include(p => p.PropertyType)
                    .Include(p => p.PropertyImages)
                    .Where(p => p.IsAvailable)
                    .ToListAsync();

                var response = new OwnerResponse
                {
                    RoleId = 2,
                    Id = lessee.Id,
                    FirstName = lessee.User.FirstName,
                    LastName = lessee.User.LastName,
                    Address = lessee.User.Address,
                    Document = lessee.User.Document,
                    Email = lessee.User.Email,
                    PhoneNumber = lessee.User.PhoneNumber,
                    Properties = properties?.Select(p => new PropertyResponse
                    {
                        Address = p.Address,
                        HasParkingLot = p.HasParkingLot,
                        Id = p.Id,
                        IsAvailable = p.IsAvailable,
                        Neighborhood = p.Neighborhood,
                        Price = p.Price,
                        PropertyImages = p.PropertyImages?.Select(pi => new PropertyImageResponse
                        {
                            Id = pi.Id,
                            ImageUrl = pi.ImageFullPath
                        }).ToList(),
                        PropertyType = p.PropertyType.Name,
                        Remarks = p.Remarks,
                        Rooms = p.Rooms,
                        SquareMeters = p.SquareMeters,
                        Stratum = p.Stratum
                    }).ToList(),
                    Contracts = lessee.Contracts?.Select(c => new ContractResponse
                    {
                        EndDate = c.EndDate,
                        Id = c.Id,
                        IsActive = c.IsActive,
                        Price = c.Price,
                        Remarks = c.Remarks,
                        StartDate = c.StartDate
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("User not found." + ex.Message);
            }
        }

        private async Task<IActionResult> GetOwnerAsync(EmailRequest emailRequest)
        {
            try
            {
                var owner = await _dataContext.Owners
                    .Include(o => o.User)
                    .Include(o => o.Properties)
                    .ThenInclude(p => p.PropertyType)
                    .Include(o => o.Properties)
                    .ThenInclude(p => p.PropertyImages)
                    .Include(o => o.Contracts)
                    .ThenInclude(c => c.Lessee)
                    .ThenInclude(l => l.User)
                    .FirstOrDefaultAsync(o => o.User.UserName.ToLower().Equals(emailRequest.Email.ToLower()));

                var response = new OwnerResponse
                {
                    RoleId = 1,
                    Id = owner.Id,
                    FirstName = owner.User.FirstName,
                    LastName = owner.User.LastName,
                    Address = owner.User.Address,
                    Document = owner.User.Document,
                    Email = owner.User.Email,
                    PhoneNumber = owner.User.PhoneNumber,
                    Properties = owner.Properties?.Select(p => new PropertyResponse
                    {
                        Address = p.Address,
                        Contracts = p.Contracts?.Select(c => new ContractResponse
                        {
                            EndDate = c.EndDate,
                            Id = c.Id,
                            IsActive = c.IsActive,
                            Lessee = ToLessesResponse(c.Lessee),
                            Price = c.Price,
                            Remarks = c.Remarks,
                            StartDate = c.StartDate
                        }).ToList(),
                        HasParkingLot = p.HasParkingLot,
                        Id = p.Id,
                        IsAvailable = p.IsAvailable,
                        Neighborhood = p.Neighborhood,
                        Price = p.Price,
                        PropertyImages = p.PropertyImages?.Select(pi => new PropertyImageResponse
                        {
                            Id = pi.Id,
                            ImageUrl = pi.ImageFullPath
                        }).ToList(),
                        PropertyType = p.PropertyType.Name,
                        Remarks = p.Remarks,
                        Rooms = p.Rooms,
                        SquareMeters = p.SquareMeters,
                        Stratum = p.Stratum
                    }).ToList(),
                    Contracts = owner.Contracts?.Select(c => new ContractResponse
                    {
                        EndDate = c.EndDate,
                        Id = c.Id,
                        IsActive = c.IsActive,
                        Price = c.Price,
                        Remarks = c.Remarks,
                        StartDate = c.StartDate
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("User not found." + ex.Message);
            }
        }

        private LesseeResponse ToLessesResponse(Lessee lessee)
        {
            return new LesseeResponse
            {
                Address = lessee.User.Address,
                Document = lessee.User.Document,
                Email = lessee.User.Email,
                FirstName = lessee.User.FirstName,
                LastName = lessee.User.LastName,
                PhoneNumber = lessee.User.PhoneNumber
            };
        }

        [HttpGet]
        [Route("GetAvailbleProperties")]
        public async Task<IActionResult> GetAvailbleProperties()
        {
            try
            {
                var properties = await _dataContext.Properties
                    .Include(p => p.PropertyType)
                    .Include(p => p.PropertyImages)
                    .Where(p => p.IsAvailable)
                    .ToListAsync();

                var response = new List<PropertyResponse>(properties.Select(p => new PropertyResponse
                {
                    Address = p.Address,
                    HasParkingLot = p.HasParkingLot,
                    Id = p.Id,
                    IsAvailable = p.IsAvailable,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    Neighborhood = p.Neighborhood,
                    Price = p.Price,
                    PropertyImages = new List<PropertyImageResponse>(p.PropertyImages.Select(pi => new PropertyImageResponse
                    {
                        Id = pi.Id,
                        ImageUrl = pi.ImageFullPath
                    }).ToList()),
                    PropertyType = p.PropertyType.Name,
                    Remarks = p.Remarks,
                    Rooms = p.Rooms,
                    SquareMeters = p.SquareMeters,
                    Stratum = p.Stratum
                }).ToList());

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Property not found." + ex.Message);
            }
        }


        //Métodos para Angular
        [HttpGet]
        [Route("GetOwnersWeb/{index}/{countPages}")]
        public async Task<IActionResult> GetOwners(int index, int countPages)
        {
            try
            {
                var total = await _dataContext.Owners.CountAsync();

                var owners = await _dataContext.Owners
                .Include(o => o.User)
                .Include(o => o.Properties)
                .Include(o => o.Contracts)
                .Skip(index).Take(countPages).ToListAsync();

                var ownersList = owners.Select(x => new OwnerResponseApi()
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
                    Properties = x.Properties != null ? toPropertiesResponseApi(x.Properties) : new List<PropertyResponseApi>(),
                    Contracts = x.Contracts != null ? toContactsResponseApi(x.Contracts) : new List<ContractResponseApi>()
                }).ToList();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de los owners.",
                    Result = ownersList,
                    Total = total
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el listado de los owners." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetOwnerWeb/{ownerId}")]
        public async Task<IActionResult> GetOwner(int ownerId)
        {
            try
            {
                var owner = await _dataContext.Owners
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == ownerId);

                if (owner == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información del owner."
                    });
                }

                var ownerResponse = new OwnerResponseApi()
                {
                    Id = owner.Id,
                    User = new UserResponseApi()
                    {
                        Address = owner.User.Address,
                        Document = owner.User.Document,
                        FirstName = owner.User.FirstName,
                        LastName = owner.User.LastName,
                        Email = owner.User.Email,
                        Phone = owner.User.PhoneNumber
                    }
                };

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información del owner.",
                    Result = ownerResponse
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al cargar la información del owner." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DetailsOwnerWeb/{ownerId}")]
        public async Task<IActionResult> DetailsOwner(int ownerId)
        {
            try
            {
                var owner = await _dataContext.Owners
                .Include(o => o.User)
                .Include(o => o.Properties)
                .ThenInclude(p => p.PropertyImages)
                .Include(o => o.Contracts)
                .ThenInclude(c => c.Lessee)
                .ThenInclude(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == ownerId);

                if (owner == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información del owner."
                    });
                }

                var ownerResponse = new OwnerResponseApi()
                {
                    Id = owner.Id,
                    User = new UserResponseApi()
                    {
                        Address = owner.User.Address,
                        Document = owner.User.Document,
                        FirstName = owner.User.FirstName,
                        LastName = owner.User.LastName,
                        Phone = owner.User.PhoneNumber,
                        Email = owner.User.Email
                    },
                    Properties = owner.Properties != null ? toPropertiesResponseApi(owner.Properties) : new List<PropertyResponseApi>(),
                    Contracts = owner.Contracts != null ? toContactsResponseApi(owner.Contracts) : new List<ContractResponseApi>()
                };

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información del owner.",
                    Result = ownerResponse
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al cargar la información del owner." + ex.Message
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

                var usernew = await _userHelper.AddUserWeb(request, "Owner");

                if (usernew != null)
                {
                    var owner = new Owner
                    {
                        Contracts = new List<Contract>(),
                        Properties = new List<Property>(),
                        User = usernew
                    };

                    _dataContext.Owners.Add(owner);
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

        [HttpGet]
        [Route("DeleteWeb/{ownerId}")]
        public async Task<IActionResult> Delete(int ownerId)
        {
            try
            {
                var owner = await _dataContext.Owners
                    .Include(o => o.User)
                    .Include(o => o.Properties)
                    .FirstOrDefaultAsync(m => m.Id == ownerId);

                if (owner == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al eliminar el usuario."
                    });
                }

                if (owner.Properties.Count != 0)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "El usuario no puede ser eleminado porque este posee propiedades."
                    });
                }

                _dataContext.Owners.Remove(owner);
                await _dataContext.SaveChangesAsync();
                await _userHelper.DeleteUserAsync(owner.User.Email);

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
        [Route("GetPropertyWeb/{propertyId}")]
        public async Task<IActionResult> GetProperty(int propertyId)
        {
            try
            {
                var property = await _dataContext.Properties
                .Include(p => p.Owner)
                .Include(p => p.PropertyType)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

                if (property == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información de la propiedad."
                    });
                }

                var propertyReponse = new PropertyResponseApi()
                {
                    Id = property.Id,
                    Neighborhood = property.Neighborhood,
                    Address = property.Address,
                    Price = property.Price,
                    SquareMeters = property.SquareMeters,
                    Rooms = property.Rooms,
                    Stratum = property.Stratum,
                    HasParkingLot = property.HasParkingLot,
                    IsAvailable = property.IsAvailable,
                    Remarks = property.Remarks,
                    Latitude = property.Latitude,
                    Longitude = property.Longitude,
                    PropertyType = property.PropertyType != null ? new PropertyTypeResponseApi()
                    {
                        Id = property.PropertyType.Id,
                        Name = property.PropertyType.Name
                    } : new PropertyTypeResponseApi(),
                    Owner = property.Owner != null ? new OwnerResponseApi()
                    {
                        Id = property.Owner.Id
                    } : new OwnerResponseApi()
                };

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información de la propiedad.",
                    Result = propertyReponse
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al cargar la información de la propiedad." + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("AddPropertyWeb")]
        public async Task<IActionResult> AddProperty(AddPropertyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se produjo un error al intentar agregar la propiedad."
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

                var propertyType = await _dataContext.PropertyTypes.FindAsync(request.PropertyTypeId);
                if (propertyType == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Tipo de propiedad no valido."
                    });
                }

                var property = new Property
                {
                    Address = request.Address,
                    HasParkingLot = request.HasParkingLot,
                    IsAvailable = request.IsAvailable,
                    Neighborhood = request.Neighborhood,
                    Owner = owner,
                    Price = request.Price,
                    PropertyType = propertyType,
                    Remarks = request.Remarks,
                    Rooms = request.Rooms,
                    SquareMeters = request.SquareMeters,
                    Stratum = request.Stratum,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                };

                _dataContext.Properties.Add(property);
                await _dataContext.SaveChangesAsync();
                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "La propiedad ha sido agregada correctamente."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se produjo un error al intentar agregar la propiedad." + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("UpdatePropertyWeb")]
        public async Task<IActionResult> UpdateProperty(AddPropertyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se produjo un error al intentar actualizar la propiedad."
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

                var propertyType = await _dataContext.PropertyTypes.FindAsync(request.PropertyTypeId);
                if (propertyType == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Tipo de propiedad no valido."
                    });
                }

                var property = new Property
                {
                    Id = request.Id,
                    Address = request.Address,
                    HasParkingLot = request.HasParkingLot,
                    IsAvailable = request.IsAvailable,
                    Neighborhood = request.Neighborhood,
                    Owner = owner,
                    Price = request.Price,
                    PropertyType = propertyType,
                    Remarks = request.Remarks,
                    Rooms = request.Rooms,
                    SquareMeters = request.SquareMeters,
                    Stratum = request.Stratum,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                };

                _dataContext.Properties.Update(property);
                await _dataContext.SaveChangesAsync();
                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "La propiedad ha sido actualizada correctamente."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se produjo un error al intentar actualizar la propiedad." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DetailsPropertyWeb/{propertyId}")]
        public async Task<IActionResult> DetailsProperty(int propertyId)
        {
            try
            {
                var property = await _dataContext.Properties
                .Include(o => o.Owner)
                .ThenInclude(o => o.User)
                .Include(o => o.Contracts)
                .ThenInclude(c => c.Lessee)
                .ThenInclude(l => l.User)
                .Include(o => o.PropertyType)
                .Include(p => p.PropertyImages)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

                if (property == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información de la propiedad."
                    });
                }

                var propertyDetails = new PropertyResponseApi()
                {
                    Id = property.Id,
                    Neighborhood = property.Neighborhood,
                    Address = property.Address,
                    Price = property.Price,
                    SquareMeters = property.SquareMeters,
                    Rooms = property.Rooms,
                    Stratum = property.Stratum,
                    HasParkingLot = property.HasParkingLot,
                    IsAvailable = property.IsAvailable,
                    Remarks = property.Remarks,
                    Latitude = property.Latitude,
                    Longitude = property.Longitude,
                    Owner = property.Owner != null ? new OwnerResponseApi()
                    {
                        Id = property.Owner.Id,
                        User = property.Owner.User != null ? new UserResponseApi()
                        {
                            Address = property.Owner.User.Address,
                            Document = property.Owner.User.Document,
                            FirstName = property.Owner.User.FirstName,
                            LastName = property.Owner.User.LastName,
                            Phone = property.Owner.User.PhoneNumber,
                            Email = property.Owner.User.Email
                        } : new UserResponseApi()
                    } : new OwnerResponseApi(),
                    PropertyType = property.PropertyType != null ? new PropertyTypeResponseApi()
                    {
                        Id = property.PropertyType.Id,
                        Name = property.PropertyType.Name
                    } : new PropertyTypeResponseApi(),
                    PropertyImages = property.PropertyImages != null ? toPropertyImageResponseApi(property.PropertyImages) : new List<PropertyImageResponseApi>(),
                    Contracts = property.Contracts != null ? toContactsResponseApi(property.Contracts) : new List<ContractResponseApi>()
                };

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información de la propiedad.",
                    Result = propertyDetails
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al cargar la información de la propiedad." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DeletePropertyWeb/{propertyId}")]
        public async Task<IActionResult> DeleteProperty(int propertyId)
        {
            try
            {
                var property = await _dataContext.Properties
                    .Include(p => p.Owner)
                    .Include(p => p.PropertyImages)
                    .Include(p => p.Contracts)
                    .FirstOrDefaultAsync(pi => pi.Id == propertyId);

                if (property == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al eliminar la propiedad."
                    });
                }

                if (property.Contracts.Count != 0)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "La propiedad no puede ser eleminada porque este posee contratos."
                    });
                }

                _dataContext.PropertyImages.RemoveRange(property.PropertyImages);
                _dataContext.Properties.Remove(property);
                await _dataContext.SaveChangesAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "La propiedad ha sido eliminada correctamente."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al eliminar la propiedad." + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("AddImageWeb")]
        public async Task<IActionResult> AddImage([FromForm] ImageRequestApi request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al agregar la imagen."
                    });
                }

                var property = await _dataContext.Properties.FindAsync(request.PropertyId);
                if (property == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Propiedad no valida"
                    });
                }

                var path = string.Empty;

                if (request.ImageFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(request.ImageFile);
                }

                var propertyImage = new PropertyImage
                {
                    ImageUrl = path,
                    Property = property
                };

                _dataContext.PropertyImages.Add(propertyImage);
                await _dataContext.SaveChangesAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Imagen agregada correctamente"
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al agregar la imagen." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DeleteImageWeb/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            try
            {
                var propertyImage = await _dataContext.PropertyImages
                    .FirstOrDefaultAsync(pi => pi.Id == imageId);

                if (propertyImage == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al eliminar la imagen."
                    });
                }

                _dataContext.PropertyImages.Remove(propertyImage);
                await _dataContext.SaveChangesAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Se elimino la imagen de la propiedad correctamente.",
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al eliminar la imagen." + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("AddContractWeb")]
        public async Task<IActionResult> AddContract(AddContractRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se produjo un error al intentar agregar el contrato."
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
                    Owner = owner,
                    Price = request.Price,
                    Property = property,
                    Lessee = lessee,
                    Remarks = request.Remarks,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsActive = request.IsActive
                };

                _dataContext.Contracts.Add(contrat);
                await _dataContext.SaveChangesAsync();
                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "El contrato ha sido agregado correctamente."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se produjo un error al intentar agregar el contrato." + ex.Message
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
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    IsActive = contract.IsActive,
                    Remarks = contract.Remarks,
                    Price = contract.Price,
                    Owner = contract.Owner != null ? new OwnerResponseApi()
                    {
                        Id = contract.Id,
                        User = new UserResponseApi()
                        {
                            Address = contract.Owner.User.Address,
                            Document = contract.Owner.User.Document,
                            FirstName = contract.Owner.User.FirstName,
                            LastName = contract.Owner.User.LastName
                        },
                        Properties = contract.Owner.Properties != null ? toPropertiesResponseApi(contract.Owner.Properties) : new List<PropertyResponseApi>(),
                        Contracts = contract.Owner.Contracts != null ? toContactsResponseApi(contract.Owner.Contracts) : new List<ContractResponseApi>()
                    } : new OwnerResponseApi(),
                    Lessee = contract.Lessee != null ? new LesseeResponseApi()
                    {
                        Id = contract.Id,
                        User = new UserResponseApi()
                        {
                            Address = contract.Lessee.User.Address,
                            Document = contract.Lessee.User.Document,
                            FirstName = contract.Lessee.User.FirstName,
                            LastName = contract.Lessee.User.LastName
                        },
                        Contracts = contract.Lessee.Contracts != null ? toContactsResponseApi(contract.Lessee.Contracts) : new List<ContractResponseApi>()
                    } : new LesseeResponseApi(),
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
                        } : new PropertyTypeResponseApi(),
                        Owner = contract.Property.Owner != null ? new OwnerResponseApi()
                        {
                            Id = contract.Property.Owner.Id,
                            User = new UserResponseApi()
                            {
                                Document = contract.Property.Owner.User.Document,
                                Address = contract.Property.Owner.User.Address,
                                FirstName = contract.Property.Owner.User.FirstName,
                                LastName = contract.Property.Owner.User.LastName
                            }
                        } : new OwnerResponseApi(),
                        PropertyImages = contract.Property.PropertyImages != null ? toPropertyImageResponseApi(contract.Property.PropertyImages) : new List<PropertyImageResponseApi>(),
                        Contracts = contract.Property.Contracts != null ? toContactsResponseApi(contract.Property.Contracts) : new List<ContractResponseApi>()
                    } : new PropertyResponseApi()
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
                    .ThenInclude(pt => pt.PropertyType)
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
        [Route("DeleteContracWeb/{contractId}")]
        public async Task<IActionResult> DeleteContrac(int contractId)
        {
            try
            {
                var contract = await _dataContext.Contracts
                    .Include(c => c.Property)
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


        private List<PropertyResponseApi> toPropertiesResponseApi(ICollection<Property> properties)
        {
            return properties.Select(x => new PropertyResponseApi()
            {
                Id = x.Id,
                Neighborhood = x.Neighborhood,
                Address = x.Address,
                Price = x.Price,
                SquareMeters = x.SquareMeters,
                Rooms = x.Rooms,
                Stratum = x.Stratum,
                HasParkingLot = x.HasParkingLot,
                IsAvailable = x.IsAvailable,
                Remarks = x.Remarks,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                PropertyType = x.PropertyType != null ? new PropertyTypeResponseApi()
                {
                    Id = x.PropertyType.Id,
                    Name = x.PropertyType.Name
                } : new PropertyTypeResponseApi(),
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
                PropertyImages = x.PropertyImages != null ? toPropertyImageResponseApi(x.PropertyImages) : new List<PropertyImageResponseApi>(),
                Contracts = x.Contracts != null ? toContactsResponseApi(x.Contracts) : new List<ContractResponseApi>()
            }).ToList();
        }

        private List<PropertyImageResponseApi> toPropertyImageResponseApi(ICollection<PropertyImage> propertyImages)
        {
            return propertyImages.Select(x => new PropertyImageResponseApi()
            {
                Id = x.Id,
                ImageUrl = x.ImageUrl
            }).ToList();
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
