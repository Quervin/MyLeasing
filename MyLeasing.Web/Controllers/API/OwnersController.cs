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
        private readonly IConverterHelper _converterHelper;

        public OwnersController(
            DataContext dataContext,
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IConverterHelper converterHelper)
        {
            _dataContext = dataContext;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
            _mailHelper = mailHelper;
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
                .Include(o => o.Contracts).Select(x => new OwnerResponseApi()
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
                }).Skip(index).Take(countPages).ToListAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de los owners.",
                    Result = owners,
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
                .ThenInclude(l => l.User).Select(x => new OwnerResponseApi()
                {
                    Id = x.Id,
                    User = new UserResponseApi()
                    {
                        Address = x.User.Address,
                        Document = x.User.Document,
                        FirstName = x.User.FirstName,
                        LastName = x.User.LastName
                    },
                    Properties = x.Properties != null ? toPropertiesResponseApi(x.Properties) : new List<PropertyResponseApi>(),
                    Contracts = x.Contracts != null ? toContactsResponseApi(x.Contracts) : new List<ContractResponseApi>()
                }).FirstOrDefaultAsync(m => m.Id == ownerId);

                if (owner == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información del owner."
                    });
                }

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información del owner.",
                    Result = owner
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
                .Include(p => p.PropertyImages).Select(x => new PropertyResponseApi()
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
                    PropertyImages = x.PropertyImages != null ? toPropertyImageResponseApi(x.PropertyImages) : new List<PropertyImageResponseApi>(),
                }).FirstOrDefaultAsync(p => p.Id == propertyId);

                if (property == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al cargar la información de la propiedad."
                    });
                }

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Información de la propiedad.",
                    Result = property
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
        public async Task<IActionResult> AddImage(ImageRequest request)
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

                var imageUrl = string.Empty;
                if (request.ImageArray != null && request.ImageArray.Length > 0)
                {
                    var stream = new MemoryStream(request.ImageArray);
                    var guid = Guid.NewGuid().ToString();
                    var file = $"{guid}.jpg";
                    var folder = "wwwroot\\images\\Properties";
                    var fullPath = $"~/images/Properties/{file}";
                    var response = FilesHelper.UploadPhoto(stream, folder, file);

                    if (response)
                    {
                        imageUrl = fullPath;
                    }
                }

                var propertyImage = new PropertyImage
                {
                    ImageUrl = imageUrl,
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
        [Route("DeleteImageWeb/{propertyId}")]
        public async Task<IActionResult> DeleteImage(int propertyId)
        {
            try
            {
                var propertyImage = await _dataContext.PropertyImages
                    .FirstOrDefaultAsync(pi => pi.Id == propertyId);

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
                    .ThenInclude(p => p.PropertyType).Select(x => new ContractResponseApi()
                    {
                        Id = x.Id,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        IsActive = x.IsActive,
                        Remarks = x.Remarks,
                        Price = x.Price,
                        Owner = x.Owner != null ? new OwnerResponseApi()
                        {
                            Id = x.Id,
                            User = new UserResponseApi()
                            {
                                Address = x.Owner.User.Address,
                                Document = x.Owner.User.Document,
                                FirstName = x.Owner.User.FirstName,
                                LastName = x.Owner.User.LastName
                            },
                            Properties = x.Owner.Properties != null ? toPropertiesResponseApi(x.Owner.Properties) : new List<PropertyResponseApi>(),
                            Contracts = x.Owner.Contracts != null ? toContactsResponseApi(x.Owner.Contracts) : new List<ContractResponseApi>()
                        }  : new OwnerResponseApi(),
                        Lessee = x.Lessee != null ? new LesseeResponseApi()
                        {
                            Id = x.Id,
                            User = new UserResponseApi()
                            {
                                Address = x.Lessee.User.Address,
                                Document = x.Lessee.User.Document,
                                FirstName = x.Lessee.User.FirstName,
                                LastName = x.Lessee.User.LastName
                            },
                            Contracts = x.Lessee.Contracts != null ? toContactsResponseApi(x.Lessee.Contracts) : new List<ContractResponseApi>()
                        } : new LesseeResponseApi(),
                        Property = x.Property != null ? new PropertyResponseApi()
                        {
                            Id = x.Property.Id,
                            Neighborhood = x.Property.Neighborhood,
                            Address = x.Property.Address,
                            Price = x.Property.Price,
                            SquareMeters = x.Property.SquareMeters,
                            Rooms = x.Property.Rooms,
                            Stratum = x.Property.Stratum,
                            HasParkingLot = x.Property.HasParkingLot,
                            IsAvailable = x.Property.IsAvailable,
                            Remarks = x.Property.Remarks,
                            Latitude = x.Property.Latitude,
                            Longitude = x.Property.Longitude,
                            PropertyType = x.Property.PropertyType != null ? new PropertyTypeResponseApi()
                            {
                                Id = x.Property.PropertyType.Id,
                                Name = x.Property.PropertyType.Name
                            } : new PropertyTypeResponseApi(),
                            Owner = x.Property.Owner != null ? new OwnerResponseApi()
                            {
                                Id = x.Property.Owner.Id,
                                User = new UserResponseApi()
                                {
                                    Document = x.Property.Owner.User.Document,
                                    Address = x.Property.Owner.User.Address,
                                    FirstName = x.Property.Owner.User.FirstName,
                                    LastName = x.Property.Owner.User.LastName
                                }
                            } : new OwnerResponseApi(),
                            PropertyImages = x.Property.PropertyImages != null ? toPropertyImageResponseApi(x.Property.PropertyImages) : new List<PropertyImageResponseApi>(),
                            Contracts = x.Property.Contracts != null ? toContactsResponseApi(x.Property.Contracts) : new List<ContractResponseApi>()
                        } : new PropertyResponseApi()
                    }).FirstOrDefaultAsync(pi => pi.Id == contractId);

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
                    IsSuccess = false,
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
                        Document = x.Owner.User.Document,
                        Address = x.Owner.User.Address,
                        FirstName = x.Owner.User.FirstName,
                        LastName = x.Owner.User.LastName
                    }
                } : new LesseeResponseApi()
            }).ToList();
        }

    }
}
