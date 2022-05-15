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
    public class PropertiesController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public PropertiesController(
            DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostProperty([FromBody] PropertyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var owner = await _dataContext.Owners.FindAsync(request.OwnerId);
                if (owner == null)
                {
                    return BadRequest("Not valid owner.");
                }

                var propertyType = await _dataContext.PropertyTypes.FindAsync(request.PropertyTypeId);
                if (propertyType == null)
                {
                    return BadRequest("Not valid property type.");
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
                    Stratum = request.Stratum
                };

                _dataContext.Properties.Add(property);
                await _dataContext.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest("Not valid owner." + ex.Message);
            }
        }

        //Cuando se hace un segundo post en api s debe de rutear el metodo.
        [HttpPost]
        [Route("AddImageToProperty")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddImageToProperty([FromBody] ImageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var property = await _dataContext.Properties.FindAsync(request.PropertyId);
                if (property == null)
                {
                    return BadRequest("Not valid property.");
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
                return Ok(propertyImage);
            }
            catch (Exception ex)
            {
                return BadRequest("Not valid property." + ex.Message);
            }
        }


        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutProperty([FromRoute] int id, [FromBody] PropertyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != request.Id)
                {
                    return BadRequest();
                }

                var oldProperty = await _dataContext.Properties.FindAsync(request.Id);
                if (oldProperty == null)
                {
                    return BadRequest("Property doesn't exists.");
                }

                var propertyType = await _dataContext.PropertyTypes.FindAsync(request.PropertyTypeId);
                if (propertyType == null)
                {
                    return BadRequest("Not valid property type.");
                }

                oldProperty.Address = request.Address;
                oldProperty.HasParkingLot = request.HasParkingLot;
                oldProperty.IsAvailable = request.IsAvailable;
                oldProperty.Neighborhood = request.Neighborhood;
                oldProperty.Price = request.Price;
                oldProperty.PropertyType = propertyType;
                oldProperty.Remarks = request.Remarks;
                oldProperty.Rooms = request.Rooms;
                oldProperty.SquareMeters = request.SquareMeters;
                oldProperty.Stratum = request.Stratum;

                _dataContext.Properties.Update(oldProperty);
                await _dataContext.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest("Property doesn't exists." + ex.Message);
            }
        }

        [HttpPost]
        [Route("DeleteImageToProperty")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteImageToProperty([FromBody] ImageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var propertyImage = await _dataContext.PropertyImages.FindAsync(request.Id);
                if (propertyImage == null)
                {
                    return BadRequest("Property image doesn't exist.");
                }

                _dataContext.PropertyImages.Remove(propertyImage);
                await _dataContext.SaveChangesAsync();
                return Ok(propertyImage);
            }
            catch (Exception ex)
            {
                return BadRequest("Property image doesn't exist." + ex.Message);
            }
        }

        [HttpGet("GetLastPropertyByOwnerId/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetLastPropertyByOwnerId([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var owner = await _dataContext.Owners
                    .Include(o => o.Properties)
                    .ThenInclude(p => p.PropertyType)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (owner == null)
                {
                    return NotFound();
                }

                var property = owner.Properties.LastOrDefault();
                var response = new PropertyResponse
                {
                    Address = property.Address,
                    HasParkingLot = property.HasParkingLot,
                    Id = property.Id,
                    IsAvailable = property.IsAvailable,
                    Neighborhood = property.Neighborhood,
                    Price = property.Price,
                    PropertyType = property.PropertyType.Name,
                    Remarks = property.Remarks,
                    Rooms = property.Rooms,
                    SquareMeters = property.SquareMeters,
                    Stratum = property.Stratum
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Property doesn't exist." + ex.Message);
            }
        }

        //Métodos para Angular
        [HttpGet]
        [Route("GetListPropertiesWeb/{index}/{countPages}")]
        public async Task<IActionResult> GetListProperties(int index, int countPages)
        {
            try
            {
                var total = await _dataContext.Properties.CountAsync();

                var properties = await _dataContext.Properties
                .Include(p => p.PropertyType)
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
                }).Where(p => p.IsAvailable).Skip(index).Take(countPages).ToListAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de las propiedades.",
                    Result = properties,
                    Total = total
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el listado de las propiedades." + ex.Message
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
                }).FirstOrDefaultAsync(m => m.Id == propertyId);

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Datos de la propiedad.",
                    Result = property
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el la información de la propiedad." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetPropertiesWeb/{index}/{countPages}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetProperties(int index, int countPages)
        {
            try
            {

                var total = await _dataContext.Properties.CountAsync();

                var properties = await _dataContext.Properties
                    .Include(p => p.PropertyType)
                    .Include(p => p.PropertyImages)
                    .Include(p => p.Contracts)
                    .Include(p => p.Owner)
                    .ThenInclude(o => o.User).Select(x => new PropertyResponseApi()
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
                    }).Skip(index).Take(countPages).ToListAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de las propiedades.",
                    Result = properties,
                    Total = total
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el listado de las propiedades." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DetailsPropertyWeb/{propertyId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
                    }).FirstOrDefaultAsync(m => m.Id == propertyId);

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Datos de la propiedad.",
                    Result = property
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el la información de la propiedad." + ex.Message
                });
            }
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

